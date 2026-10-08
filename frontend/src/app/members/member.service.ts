import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { PagedResult } from '../core/models/paged-result.model';
import { buildParams } from '../core/utils/http-params.util';
import {
  CreateMemberResponse,
  FreezeContext,
  FreezeRequest,
  FreezeResponse,
  MemberListItem,
  MemberListQuery,
  MemberLookup,
  MemberProfile
} from './member.model';

/** All HTTP calls of the members feature (and the membership freeze). */
@Injectable({ providedIn: 'root' })
export class MemberService {
  private readonly membersUrl = `${environment.apiUrl}/members`;
  private readonly membershipsUrl = `${environment.apiUrl}/memberships`;

  constructor(private readonly http: HttpClient) {}

  list(query: MemberListQuery): Observable<PagedResult<MemberListItem>> {
    const params = buildParams({
      search: query.search,
      branchIds: query.branchIds,
      statuses: query.statuses,
      sortBy: query.sortBy,
      sortDirection: query.sortDirection,
      pageNumber: query.pageNumber,
      pageSize: query.pageSize
    });

    return this.http.get<PagedResult<MemberListItem>>(this.membersUrl, { params });
  }

  getProfile(memberId: number): Observable<MemberProfile> {
    return this.http.get<MemberProfile>(`${this.membersUrl}/${memberId}`);
  }

  lookup(search: string, limit = 20): Observable<MemberLookup[]> {
    const params = buildParams({ search, limit });

    return this.http.get<MemberLookup[]>(`${this.membersUrl}/lookup`, { params });
  }

  /** Multipart request: the API accepts form data (a photo can be added later). */
  create(fullName: string, homeBranchId: number): Observable<CreateMemberResponse> {
    return this.http.post<CreateMemberResponse>(
      this.membersUrl,
      this.toFormData(fullName, homeBranchId)
    );
  }

  update(memberId: number, fullName: string, homeBranchId: number): Observable<void> {
    return this.http.put<void>(
      `${this.membersUrl}/${memberId}`,
      this.toFormData(fullName, homeBranchId)
    );
  }

  getFreezeContext(membershipId: number): Observable<FreezeContext> {
    return this.http.get<FreezeContext>(
      `${this.membershipsUrl}/${membershipId}/freeze-context`
    );
  }

  freeze(membershipId: number, request: FreezeRequest): Observable<FreezeResponse> {
    return this.http.post<FreezeResponse>(
      `${this.membershipsUrl}/${membershipId}/freeze`,
      request
    );
  }

  private toFormData(fullName: string, homeBranchId: number): FormData {
    const body = new FormData();

    body.append('fullName', fullName);
    body.append('homeBranchId', String(homeBranchId));

    return body;
  }
}
