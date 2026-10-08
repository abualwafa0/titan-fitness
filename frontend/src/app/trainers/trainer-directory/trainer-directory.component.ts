import { Component, OnDestroy, OnInit, computed, effect, signal, untracked } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, ParamMap, Params, Router, RouterLink } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Subscription } from 'rxjs';

import { HeaderSearchService } from '../../core/services/header-search.service';
import { RetryService } from '../../core/services/retry.service';
import { ListContextService } from '../../core/services/list-context.service';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { PaginatorComponent } from '../../shared/components/paginator/paginator.component';
import { SortChange, SortDirection } from '../../shared/components/sort-header/sort-header.component';
import { StateMessageComponent } from '../../shared/components/state-message/state-message.component';
import { TrainerFilterDialogComponent } from '../trainer-filter-dialog/trainer-filter-dialog.component';
import { TrainerTableComponent } from '../trainer-table/trainer-table.component';
import {
  TRAINER_SORT_COLUMNS,
  TrainerFilters,
  TrainerListItem,
  TrainerListQuery,
  TrainerSortBy
} from '../trainer.model';
import { TrainerService } from '../trainer.service';

const PAGE_SIZE = 10;

/**
 * Trainer Directory page. Page, sort, search and filters live in the URL query
 * string and in signals; an effect reloads the list when they change.
 */
@Component({
  selector: 'app-trainer-directory',
  standalone: true,
  imports: [
    RouterLink,
    IconComponent,
    PageHeaderComponent,
    PaginatorComponent,
    StateMessageComponent,
    TrainerTableComponent
  ],
  templateUrl: './trainer-directory.component.html',
  styleUrl: './trainer-directory.component.css'
})
export class TrainerDirectoryComponent implements OnInit, OnDestroy {
  private readonly queryParams = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap
  });

  readonly page = computed(() =>
    Math.max(1, Number(this.queryParams().get('page')) || 1)
  );

  readonly sortBy = computed<TrainerSortBy>(() => {
    const value = this.queryParams().get('sortBy') as TrainerSortBy | null;

    return value && TRAINER_SORT_COLUMNS.includes(value) ? value : 'name';
  });

  readonly sortDirection = computed<SortDirection>(() =>
    this.queryParams().get('sortDirection') === 'desc' ? 'desc' : 'asc'
  );

  readonly search = computed(() => this.queryParams().get('search') ?? '');

  readonly filters = computed<TrainerFilters>(() => {
    const params = this.queryParams();
    const isActive = params.get('isActive');

    return {
      branchIds: params
        .getAll('branchIds')
        .map(Number)
        .filter((id) => Number.isFinite(id)),
      specialties: params.getAll('specialties'),
      isActive: isActive === 'true' ? true : isActive === 'false' ? false : null
    };
  });

  readonly activeFilterCount = computed(() => {
    const filters = this.filters();

    return (
      filters.branchIds.length +
      filters.specialties.length +
      (filters.isActive === null ? 0 : 1)
    );
  });

  readonly query = computed<TrainerListQuery>(() => ({
    search: this.search(),
    sortBy: this.sortBy(),
    sortDirection: this.sortDirection(),
    pageNumber: this.page(),
    pageSize: PAGE_SIZE,
    ...this.filters()
  }));

  readonly trainers = signal<TrainerListItem[]>([]);
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
    private readonly trainerService: TrainerService,
    private readonly headerSearch: HeaderSearchService,
    private readonly listContext: ListContextService,
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

    // URL -> header search box (e.g. the sidebar link opened the page without a query)
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

      untracked(() => this.listContext.save('trainers', this.toParams(params)));
    });

    // Reload when anything in the query changes
    effect(() => {
      const query = this.query();

      this.retryService.tick();

      untracked(() => this.load(query));
    });
  }

  ngOnInit(): void {
    this.headerSearch.setPlaceholder('Search trainers...');
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
    const ref = this.dialog.open(TrainerFilterDialogComponent, {
      data: { filters: this.filters() },
      panelClass: 'titan-dialog',
      width: '520px',
      autoFocus: false
    });

    ref.componentInstance.filtersApplied.subscribe((filters) =>
      this.applyFilters(filters)
    );
  }

  private applyFilters(filters: TrainerFilters): void {
    this.update({
      branchIds: filters.branchIds.length > 0 ? filters.branchIds : null,
      specialties: filters.specialties.length > 0 ? filters.specialties : null,
      isActive: filters.isActive,
      page: null
    });
  }

  private load(query: TrainerListQuery): void {
    this.loadSubscription?.unsubscribe();
    this.loading.set(true);
    this.failed.set(false);

    this.loadSubscription = this.trainerService.list(query).subscribe({
      next: (result) => {
        this.trainers.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.trainers.set([]);
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
