import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { buildParams } from '../core/utils/http-params.util';
import { DashboardData } from './dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  constructor(private readonly http: HttpClient) {}

  get(branchId?: number | null): Observable<DashboardData> {
    const params = buildParams({ branchId });

    return this.http.get<DashboardData>(`${environment.apiUrl}/dashboard`, { params });
  }
}
