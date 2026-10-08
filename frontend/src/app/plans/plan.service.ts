import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { PagedResult } from '../core/models/paged-result.model';
import { buildParams } from '../core/utils/http-params.util';
import {
  CreatePlanResponse,
  PlanDetails,
  PlanFilterOptions,
  PlanListItem,
  PlanListQuery,
  PlanRequest
} from './plan.model';

/** All HTTP calls of the plans feature. */
@Injectable({ providedIn: 'root' })
export class PlanService {
  private readonly baseUrl = `${environment.apiUrl}/plans`;

  constructor(private readonly http: HttpClient) {}

  list(query: PlanListQuery): Observable<PagedResult<PlanListItem>> {
    const params = buildParams({
      search: query.search,
      durations: query.durations,
      accessScope: query.accessScope,
      minPrice: query.minPrice,
      maxPrice: query.maxPrice,
      isPublished: query.isPublished,
      sortBy: query.sortBy,
      sortDirection: query.sortDirection,
      pageNumber: query.pageNumber,
      pageSize: query.pageSize
    });

    return this.http.get<PagedResult<PlanListItem>>(this.baseUrl, { params });
  }

  getById(planId: number): Observable<PlanDetails> {
    return this.http.get<PlanDetails>(`${this.baseUrl}/${planId}`);
  }

  create(request: PlanRequest): Observable<CreatePlanResponse> {
    return this.http.post<CreatePlanResponse>(this.baseUrl, request);
  }

  update(planId: number, request: PlanRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${planId}`, request);
  }

  getFilterOptions(): Observable<PlanFilterOptions> {
    return this.http.get<PlanFilterOptions>(`${this.baseUrl}/filter-options`);
  }
}
