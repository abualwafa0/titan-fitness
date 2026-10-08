import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { PagedResult } from '../core/models/paged-result.model';
import { buildParams } from '../core/utils/http-params.util';
import {
  CreateTrainerResponse,
  TrainerDetails,
  TrainerListItem,
  TrainerListQuery,
  TrainerLookup,
  TrainerRequest
} from './trainer.model';

/** All HTTP calls of the trainers feature. */
@Injectable({ providedIn: 'root' })
export class TrainerService {
  private readonly baseUrl = `${environment.apiUrl}/trainers`;

  constructor(private readonly http: HttpClient) {}

  list(query: TrainerListQuery): Observable<PagedResult<TrainerListItem>> {
    const params = buildParams({
      search: query.search,
      branchIds: query.branchIds,
      specialties: query.specialties,
      isActive: query.isActive,
      sortBy: query.sortBy,
      sortDirection: query.sortDirection,
      pageNumber: query.pageNumber,
      pageSize: query.pageSize
    });

    return this.http.get<PagedResult<TrainerListItem>>(this.baseUrl, { params });
  }

  getById(trainerId: number): Observable<TrainerDetails> {
    return this.http.get<TrainerDetails>(`${this.baseUrl}/${trainerId}`);
  }

  create(request: TrainerRequest): Observable<CreateTrainerResponse> {
    return this.http.post<CreateTrainerResponse>(this.baseUrl, request);
  }

  update(trainerId: number, request: TrainerRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${trainerId}`, request);
  }

  /** Active trainers of one branch (used by the class dialog). */
  lookup(branchId: number, search?: string): Observable<TrainerLookup[]> {
    const params = buildParams({ branchId, search });

    return this.http.get<TrainerLookup[]>(`${this.baseUrl}/lookup`, { params });
  }

  getSpecialties(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/specialties`);
  }
}
