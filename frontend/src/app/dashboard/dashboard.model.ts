import { SessionStatus } from '../shared/utils/class-state.util';

/** A class session as the dashboard returns it (note: durationMinutes). */
export interface DashboardSession {
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
  durationMinutes: number;
  bookedPlaces: number;
  capacityLimit: number;
  waitlistCount: number;
  status: SessionStatus;
}

/** GET /dashboard. */
export interface DashboardData {
  date: string;
  branchId: number | null;
  checkInsToday: number;
  checkInsSameWeekdayLastWeek: number;
  activeMembers: number;
  membersCurrentlyInside: number;
  upcomingSessions: DashboardSession[];
  currentRunningSession: DashboardSession | null;
  bookingsToday: number;
  capacityFilledPercentage: number;
}
