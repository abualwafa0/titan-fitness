import { SortDirection } from '../shared/components/sort-header/sort-header.component';

export type MemberSortBy = 'name' | 'number' | 'status' | 'branch' | 'lastVisit';

export const MEMBER_SORT_COLUMNS: readonly MemberSortBy[] = [
  'name',
  'number',
  'status',
  'branch',
  'lastVisit'
];

/** Statuses the member filter can use (membership status of the member). */
export const MEMBER_FILTER_STATUSES: readonly string[] = ['Active', 'Frozen', 'Expired'];

export type MembershipStatus = 'Pending' | 'Active' | 'Frozen' | 'Expired' | 'Cancelled';

export type MembershipAccessScope = 'HomeBranchOnly' | 'AllBranches';

export type FreezeReason = 'ExtendedTravel' | 'Medical' | 'Injury' | 'Financial' | 'Other';

/** One row of GET /members. */
export interface MemberListItem {
  memberId: number;
  fullName: string;
  membershipNumber: string;
  status: MembershipStatus | null;
  homeBranchId: number;
  branchName: string;
  lastVisit: string | null;
  membershipId: number | null;
  freezesRemaining: number | null;
}

/** One option of GET /members/lookup (autocomplete, check-in dialog). */
export interface MemberLookup {
  memberId: number;
  fullName: string;
  membershipNumber: string;
  photo: string | null;
  status: string | null;
}

export interface CurrentMembership {
  membershipId: number;
  planId: number;
  planName: string;
  pricePaid: number;
  startDate: string;
  endDate: string;
  status: MembershipStatus;
  freezesUsed: number;
  maximumNumberOfFreezes: number;
  freezeDaysUsed: number;
  maximumFreezeDays: number;
  guestPassesUsed: number;
  guestPassQuota: number;
}

export interface MembershipSummary {
  membershipId: number;
  planId: number;
  planName: string;
  purchaseDate: string;
  startDate: string;
  endDate: string;
  status: MembershipStatus;
  pricePaid: number;
  durationInMonths: number;
  maximumFreezeDays: number;
  maximumNumberOfFreezes: number;
  guestPassQuota: number;
  accessScope: MembershipAccessScope;
}

export interface MemberActivity {
  activityType: 'Check-In' | 'Class Attendance' | string;
  title: string;
  details: string | null;
  occurredOn: string;
}

/** GET /members/{id}. */
export interface MemberProfile {
  memberId: number;
  membershipNumber: string;
  fullName: string;
  email: string | null;
  phone: string | null;
  address: string | null;
  joinedDate: string;
  photo: string | null;
  homeBranchId: number;
  homeBranchName: string;
  status: MembershipStatus | null;
  currentMembership: CurrentMembership | null;
  memberships: MembershipSummary[];
  recentActivities: MemberActivity[];
}

/** What the member form dialog needs to edit a member (a MemberProfile fits). */
export interface MemberFormSource {
  memberId: number;
  fullName: string;
  membershipNumber: string;
  homeBranchId: number;
  status: string | null;
}

/** Result the member form dialog closes with. */
export interface MemberSaveResult {
  memberId: number;
}

/** Response of POST /members. */
export interface CreateMemberResponse {
  memberId: number;
  membershipNumber: string;
}

/** GET /memberships/{id}/freeze-context. */
export interface FreezeContext {
  membershipId: number;
  memberId: number;
  memberName: string;
  membershipNumber: string;
  planId: number;
  planName: string;
  status: MembershipStatus;
  startDate: string;
  endDate: string;
  maximumFreezeDays: number;
  maximumNumberOfFreezes: number;
  freezeDaysUsed: number;
  numberOfFreezesUsed: number;
  remainingFreezeDays: number;
  remainingNumberOfFreezes: number;
  allowedDurationsInMonths: number[];
}

/** Body of POST /memberships/{id}/freeze. */
export interface FreezeRequest {
  startDate: string;
  durationInMonths: number;
  reason: FreezeReason;
  additionalNotes?: string;
}

/** Response of POST /memberships/{id}/freeze. */
export interface FreezeResponse {
  freezeId: number;
  membershipId: number;
  startDate: string;
  endDate: string;
  durationInMonths: number;
  newMembershipEndDate: string;
}

/** Filters chosen in the Filter Members dialog. */
export interface MemberFilters {
  branchIds: number[];
  statuses: string[];
}

export interface MemberListQuery extends MemberFilters {
  search: string;
  sortBy: MemberSortBy;
  sortDirection: SortDirection;
  pageNumber: number;
  pageSize: number;
}

/** "TF-1001" -> "#TF-1001". */
export function formatMemberNumber(membershipNumber: string | null | undefined): string {
  const value = (membershipNumber ?? '').trim();

  if (!value) {
    return '';
  }

  return value.startsWith('#') ? value : `#${value}`;
}
