import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { buildParams } from '../core/utils/http-params.util';
import {
  BookingContext,
  BookingResponse,
  ClassSchedule,
  ClassSessionRequest,
  ClassSessionSaved
} from './class.model';

/** All HTTP calls of the classes feature. */
@Injectable({ providedIn: 'root' })
export class ClassService {
  private readonly baseUrl = `${environment.apiUrl}/class-sessions`;

  constructor(private readonly http: HttpClient) {}

  getSchedule(date: string, toDate?: string | null, branchId?: number | null): Observable<ClassSchedule> {
    const params = buildParams({ date, toDate, branchId });

    return this.http.get<ClassSchedule>(this.baseUrl, { params });
  }

  getBookingContext(sessionId: number): Observable<BookingContext> {
    return this.http.get<BookingContext>(`${this.baseUrl}/${sessionId}/booking-context`);
  }

  schedule(request: ClassSessionRequest): Observable<ClassSessionSaved> {
    return this.http.post<ClassSessionSaved>(this.baseUrl, request);
  }

  update(sessionId: number, request: ClassSessionRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${sessionId}`, request);
  }

  cancel(sessionId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${sessionId}/cancel`, {});
  }

  book(sessionId: number, memberId: number, notesForTrainer?: string): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${this.baseUrl}/${sessionId}/bookings`, {
      memberId,
      ...(notesForTrainer ? { notesForTrainer } : {})
    });
  }
}
