import { SessionStatus } from '../shared/utils/class-state.util';

export type ScheduleView = 'day' | 'week';

/** One session of GET /class-sessions. */
export interface ClassSessionItem {
  sessionId: number;
  className: string;
  branchId: number;
  branchName: string;
  studioId: number | null;
  studioName: string | null;
  trainerId: number | null;
  trainerName: string | null;
  sessionDate: string;
  startTime: string;
  durationInMinutes: number;
  capacityLimit: number;
  bookedPlaces: number;
  waitlistCount: number;
  status: SessionStatus;
}

/** GET /class-sessions?date&toDate&branchId. */
export interface ClassSchedule {
  date: string;
  toDate: string | null;
  branchId: number | null;
  totalBookedPlaces: number;
  totalCapacity: number;
  capacityFilledPercentage: number;
  sessions: ClassSessionItem[];
}

/** GET /class-sessions/{id}/booking-context. */
export interface BookingContext {
  sessionId: number;
  className: string;
  branchId: number;
  branchName: string;
  studioId: number | null;
  studioName: string | null;
  trainerId: number | null;
  trainerName: string | null;
  sessionDate: string;
  startTime: string;
  durationInMinutes: number;
  capacityLimit: number;
  bookedPlaces: number;
  remainingPlaces: number;
  waitlistCount: number;
  status: SessionStatus;
  description: string | null;
}

/** Body of POST /class-sessions and PUT /class-sessions/{id}. */
export interface ClassSessionRequest {
  className: string;
  branchId: number;
  studioId: number | null;
  trainerId: number | null;
  sessionDate: string;
  /** "HH:mm:ss" */
  startTime: string;
  durationInMinutes: number;
  capacityLimit: number;
  description: string | null;
}

/** Response of POST /class-sessions. */
export interface ClassSessionSaved {
  sessionId: number;
  className: string;
  branchId: number;
  studioId: number | null;
  trainerId: number | null;
  sessionDate: string;
  startTime: string;
  durationInMinutes: number;
  capacityLimit: number;
}

/** Response of POST /class-sessions/{id}/bookings. */
export interface BookingResponse {
  bookingId: number;
  sessionId: number;
  memberId: number;
  bookedOn: string;
  status: 'Booked' | 'Waitlisted' | string;
  waitlistPosition: number | null;
}

export type SessionActionId = 'view' | 'edit' | 'book' | 'cancel';

export interface SessionAction {
  action: SessionActionId;
  session: ClassSessionItem;
}
