import { SortDirection } from '../shared/components/sort-header/sort-header.component';

export type PlanAccessScope = 'HomeBranchOnly' | 'AllBranches';

export type PlanSortBy = 'name' | 'price' | 'duration' | 'guestPasses' | 'access' | 'status';

export const PLAN_SORT_COLUMNS: readonly PlanSortBy[] = [
  'name',
  'price',
  'duration',
  'guestPasses',
  'access',
  'status'
];

/** One row of GET /plans. */
export interface PlanListItem {
  planId: number;
  planName: string;
  price: number;
  durationInMonths: number;
  maximumFreezeDays: number;
  maximumNumberOfFreezes: number;
  guestPassQuota: number;
  accessScope: PlanAccessScope;
  isPublished: boolean;
}

/** GET /plans/{id}. */
export interface PlanDetails extends PlanListItem {
  soldMembershipsCount: number;
}

/** Body of POST /plans and PUT /plans/{id} (branchIds are never sent). */
export interface PlanRequest {
  planName: string;
  price: number;
  durationInMonths: number;
  maximumFreezeDays: number;
  maximumNumberOfFreezes: number;
  guestPassQuota: number;
  accessScope: PlanAccessScope;
  isPublished: boolean;
}

/** Response of POST /plans. */
export interface CreatePlanResponse {
  planId: number;
}

/** GET /plans/filter-options. */
export interface PlanFilterOptions {
  durations: number[];
  minPrice: number;
  maxPrice: number;
}

/** Filters chosen in the Filter Plans dialog (null / empty = not filtered). */
export interface PlanFilters {
  durations: number[];
  accessScope: PlanAccessScope | null;
  minPrice: number | null;
  maxPrice: number | null;
  isPublished: boolean | null;
}

export interface PlanListQuery extends PlanFilters {
  search: string;
  sortBy: PlanSortBy;
  sortDirection: SortDirection;
  pageNumber: number;
  pageSize: number;
}

export function accessLabel(scope: PlanAccessScope | string | null | undefined): string {
  return scope === 'AllBranches' ? 'All branches' : 'Home branch only';
}
