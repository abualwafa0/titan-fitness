import { SortDirection } from '../shared/components/sort-header/sort-header.component';

/** Columns the trainers list can be sorted by (query value sent to the API). */
export type TrainerSortBy = 'name' | 'number' | 'specialty' | 'branch' | 'status';

export const TRAINER_SORT_COLUMNS: readonly TrainerSortBy[] = [
  'name',
  'number',
  'specialty',
  'branch',
  'status'
];

/** One row of GET /trainers. */
export interface TrainerListItem {
  trainerId: number;
  trainerNumber: string;
  trainerName: string;
  specialty: string | null;
  branchId: number;
  branchName: string;
  isActive: boolean;
}

/** GET /trainers/{id}. */
export interface TrainerDetails {
  trainerId: number;
  trainerNumber: string;
  trainerName: string;
  specialty: string | null;
  email: string | null;
  phone: string | null;
  branchId: number;
  branchName: string;
  isActive: boolean;
}

/** One option of GET /trainers/lookup (active trainers of a branch). */
export interface TrainerLookup {
  trainerId: number;
  trainerNumber: string;
  trainerName: string;
  specialty: string | null;
}

/** Body of POST /trainers and PUT /trainers/{id}. */
export interface TrainerRequest {
  trainerName: string;
  specialty: string | null;
  email: string;
  phone: string | null;
  branchId: number;
  isActive: boolean;
}

/** Response of POST /trainers. */
export interface CreateTrainerResponse {
  trainerId: number;
  trainerNumber: string;
}

/** Filters chosen in the Filter Trainers dialog. isActive null = no status filter. */
export interface TrainerFilters {
  branchIds: number[];
  specialties: string[];
  isActive: boolean | null;
}

export interface TrainerListQuery extends TrainerFilters {
  search: string;
  sortBy: TrainerSortBy;
  sortDirection: SortDirection;
  pageNumber: number;
  pageSize: number;
}

/** "TR-1001" -> "#TR-1001". */
export function formatTrainerNumber(trainerNumber: string | null | undefined): string {
  const value = (trainerNumber ?? '').trim();

  if (!value) {
    return '';
  }

  return value.startsWith('#') ? value : `#${value}`;
}
