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
import { PlanFilterDialogComponent } from '../plan-filter-dialog/plan-filter-dialog.component';
import { PlanTableComponent } from '../plan-table/plan-table.component';
import {
  PLAN_SORT_COLUMNS,
  PlanAccessScope,
  PlanFilters,
  PlanListItem,
  PlanListQuery,
  PlanSortBy
} from '../plan.model';
import { PlanService } from '../plan.service';

const PAGE_SIZE = 10;

function toNumberOrNull(value: string | null): number | null {
  if (value === null || value.trim() === '') {
    return null;
  }

  const parsed = Number(value);

  return Number.isFinite(parsed) ? parsed : null;
}

/**
 * Plan Catalogue page. Page, sort, search and filters live in the URL query
 * string and in signals; an effect reloads the list when they change.
 */
@Component({
  selector: 'app-plan-catalogue',
  standalone: true,
  imports: [
    RouterLink,
    IconComponent,
    PageHeaderComponent,
    PaginatorComponent,
    StateMessageComponent,
    PlanTableComponent
  ],
  templateUrl: './plan-catalogue.component.html',
  styleUrl: './plan-catalogue.component.css'
})
export class PlanCatalogueComponent implements OnInit, OnDestroy {
  private readonly queryParams = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap
  });

  readonly page = computed(() =>
    Math.max(1, Number(this.queryParams().get('page')) || 1)
  );

  readonly sortBy = computed<PlanSortBy>(() => {
    const value = this.queryParams().get('sortBy') as PlanSortBy | null;

    return value && PLAN_SORT_COLUMNS.includes(value) ? value : 'name';
  });

  readonly sortDirection = computed<SortDirection>(() =>
    this.queryParams().get('sortDirection') === 'desc' ? 'desc' : 'asc'
  );

  readonly search = computed(() => this.queryParams().get('search') ?? '');

  readonly filters = computed<PlanFilters>(() => {
    const params = this.queryParams();
    const scope = params.get('accessScope');
    const published = params.get('isPublished');

    return {
      durations: params
        .getAll('durations')
        .map(Number)
        .filter((months) => Number.isFinite(months)),
      accessScope:
        scope === 'AllBranches' || scope === 'HomeBranchOnly'
          ? (scope as PlanAccessScope)
          : null,
      minPrice: toNumberOrNull(params.get('minPrice')),
      maxPrice: toNumberOrNull(params.get('maxPrice')),
      isPublished: published === 'true' ? true : published === 'false' ? false : null
    };
  });

  readonly activeFilterCount = computed(() => {
    const filters = this.filters();

    return (
      filters.durations.length +
      (filters.accessScope === null ? 0 : 1) +
      (filters.minPrice === null && filters.maxPrice === null ? 0 : 1) +
      (filters.isPublished === null ? 0 : 1)
    );
  });

  readonly query = computed<PlanListQuery>(() => ({
    search: this.search(),
    sortBy: this.sortBy(),
    sortDirection: this.sortDirection(),
    pageNumber: this.page(),
    pageSize: PAGE_SIZE,
    ...this.filters()
  }));

  readonly plans = signal<PlanListItem[]>([]);
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
    private readonly planService: PlanService,
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

      untracked(() => this.listContext.save('plans', this.toParams(params)));
    });

    // Reload when anything in the query changes
    effect(() => {
      const query = this.query();

      this.retryService.tick();

      untracked(() => this.load(query));
    });
  }

  ngOnInit(): void {
    this.headerSearch.setPlaceholder('Search plans...');
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
    const ref = this.dialog.open(PlanFilterDialogComponent, {
      data: { filters: this.filters() },
      panelClass: 'titan-dialog',
      width: '540px',
      autoFocus: false
    });

    ref.componentInstance.filtersApplied.subscribe((filters) =>
      this.applyFilters(filters)
    );
  }

  private applyFilters(filters: PlanFilters): void {
    this.update({
      durations: filters.durations.length > 0 ? filters.durations : null,
      accessScope: filters.accessScope,
      minPrice: filters.minPrice,
      maxPrice: filters.maxPrice,
      isPublished: filters.isPublished,
      page: null
    });
  }

  private load(query: PlanListQuery): void {
    this.loadSubscription?.unsubscribe();
    this.loading.set(true);
    this.failed.set(false);

    this.loadSubscription = this.planService.list(query).subscribe({
      next: (result) => {
        this.plans.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.plans.set([]);
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
