import { Component, OnDestroy, OnInit, computed, effect, signal, untracked } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, ParamMap, Params, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Subscription } from 'rxjs';

import { CheckInService } from '../../check-ins/check-in.service';
import { HeaderSearchService } from '../../core/services/header-search.service';
import { RetryService } from '../../core/services/retry.service';
import { ListContextService } from '../../core/services/list-context.service';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { PaginatorComponent } from '../../shared/components/paginator/paginator.component';
import { SortChange, SortDirection } from '../../shared/components/sort-header/sort-header.component';
import { StateMessageComponent } from '../../shared/components/state-message/state-message.component';
import { MemberFilterDialogComponent } from '../member-filter-dialog/member-filter-dialog.component';
import {
  MemberFormDialogComponent,
  MemberFormDialogData
} from '../member-form-dialog/member-form-dialog.component';
import { MemberTableComponent } from '../member-table/member-table.component';
import {
  MEMBER_SORT_COLUMNS,
  MemberFilters,
  MemberListItem,
  MemberListQuery,
  MemberSaveResult,
  MemberSortBy
} from '../member.model';
import { MemberService } from '../member.service';

const PAGE_SIZE = 10;

/**
 * Member Directory page. Page, sort, search and filters live in the URL query
 * string and in signals; an effect reloads the list when they change (or when a
 * check-in happens).
 */
@Component({
  selector: 'app-member-directory',
  standalone: true,
  imports: [
    IconComponent,
    PageHeaderComponent,
    PaginatorComponent,
    StateMessageComponent,
    MemberTableComponent
  ],
  templateUrl: './member-directory.component.html',
  styleUrl: './member-directory.component.css'
})
export class MemberDirectoryComponent implements OnInit, OnDestroy {
  private readonly queryParams = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap
  });

  readonly page = computed(() =>
    Math.max(1, Number(this.queryParams().get('page')) || 1)
  );

  readonly sortBy = computed<MemberSortBy>(() => {
    const value = this.queryParams().get('sortBy') as MemberSortBy | null;

    return value && MEMBER_SORT_COLUMNS.includes(value) ? value : 'name';
  });

  readonly sortDirection = computed<SortDirection>(() =>
    this.queryParams().get('sortDirection') === 'desc' ? 'desc' : 'asc'
  );

  readonly search = computed(() => this.queryParams().get('search') ?? '');

  readonly filters = computed<MemberFilters>(() => {
    const params = this.queryParams();

    return {
      branchIds: params
        .getAll('branchIds')
        .map(Number)
        .filter((id) => Number.isFinite(id)),
      statuses: params.getAll('statuses')
    };
  });

  readonly activeFilterCount = computed(() => {
    const filters = this.filters();

    return filters.branchIds.length + filters.statuses.length;
  });

  readonly query = computed<MemberListQuery>(() => ({
    search: this.search(),
    sortBy: this.sortBy(),
    sortDirection: this.sortDirection(),
    pageNumber: this.page(),
    pageSize: PAGE_SIZE,
    ...this.filters()
  }));

  readonly members = signal<MemberListItem[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);
  readonly failed = signal(false);

  readonly hasCriteria = computed(
    () => this.search() !== '' || this.activeFilterCount() > 0
  );

  private loadSubscription: Subscription | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly dialog: MatDialog,
    private readonly memberService: MemberService,
    private readonly headerSearch: HeaderSearchService,
    private readonly listContext: ListContextService,
    private readonly checkInService: CheckInService,
    private readonly retryService: RetryService
  ) {
    // Start with the search text that is already in the URL (before the effects below run).
    this.headerSearch.setTerm(this.route.snapshot.queryParamMap.get('search') ?? '');

    // Header search box -> URL
    effect(() => {
      const term = this.headerSearch.term();

      untracked(() => {
        if (term !== this.search()) {
          this.update({ search: term || null, page: null });
        }
      });
    });

    // URL -> header search box
    effect(() => {
      const search = this.search();

      untracked(() => {
        if (search !== this.headerSearch.term()) {
          this.headerSearch.setTerm(search);
        }
      });
    });

    // Remember the list state for "Back to list"
    effect(() => {
      const params = this.queryParams();

      untracked(() => this.listContext.save('members', this.toParams(params)));
    });

    // Reload when anything in the query changes, or after a check-in
    effect(() => {
      const query = this.query();

      this.checkInService.checkInVersion();
      this.retryService.tick();

      untracked(() => this.load(query));
    });
  }

  ngOnInit(): void {
    this.headerSearch.setPlaceholder('Search members...');
  }

  ngOnDestroy(): void {
    this.loadSubscription?.unsubscribe();
    this.headerSearch.reset();
  }

  reload(): void {
    this.load(this.query());
  }

  onSort(change: SortChange): void {
    this.update({
      sortBy: change.column,
      sortDirection: change.direction,
      page: null
    });
  }

  onPageChange(page: number): void {
    this.update({ page: page === 1 ? null : page });
  }

  openFilter(): void {
    const ref = this.dialog.open(MemberFilterDialogComponent, {
      data: { filters: this.filters() },
      panelClass: 'titan-dialog',
      width: '480px',
      autoFocus: false
    });

    ref.componentInstance.filtersApplied.subscribe((filters) =>
      this.applyFilters(filters)
    );
  }

  openAddMember(): void {
    this.dialog
      .open<MemberFormDialogComponent, MemberFormDialogData, MemberSaveResult>(
        MemberFormDialogComponent,
        {
          data: { mode: 'add' },
          panelClass: 'titan-dialog',
          width: '480px'
        }
      )
      .afterClosed()
      .subscribe((saved) => {
        if (saved) {
          this.reload();
        }
      });
  }

  private applyFilters(filters: MemberFilters): void {
    this.update({
      branchIds: filters.branchIds.length > 0 ? filters.branchIds : null,
      statuses: filters.statuses.length > 0 ? filters.statuses : null,
      page: null
    });
  }

  private load(query: MemberListQuery): void {
    this.loadSubscription?.unsubscribe();
    this.loading.set(true);
    this.failed.set(false);

    this.loadSubscription = this.memberService.list(query).subscribe({
      next: (result) => {
        this.members.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.members.set([]);
        this.totalCount.set(0);
        this.failed.set(true);
        this.loading.set(false);
      }
    });
  }

  private update(queryParams: Params): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'merge'
    });
  }

  private toParams(map: ParamMap): Params {
    const params: Params = {};

    for (const key of map.keys) {
      const values = map.getAll(key);

      params[key] = values.length > 1 ? values : values[0];
    }

    return params;
  }
}
