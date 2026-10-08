/** Body of POST /check-ins. */
export interface CheckInRequest {
  memberId: number;
  branchId: number;
  /** Local date-time without Z ("2026-10-05T08:30:00"). */
  checkInDateTime?: string;
  notes?: string;
}

/** Response of POST /check-ins (always HTTP 200, also when the check-in is refused). */
export interface CheckInResponse {
  checkInId: number;
  memberId: number;
  branchId: number;
  checkInDateTime: string;
  result: 'Admitted' | 'Refused';
  refusalReason: string | null;
}
