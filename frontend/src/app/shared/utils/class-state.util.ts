/** Status stored for a class session (as returned by the API). */
export type SessionStatus = 'Open' | 'InProgress' | 'Completed' | 'Cancelled';

/** State shown on the badge of a class session. */
export type ClassState =
  | 'Upcoming'
  | 'InProgress'
  | 'Full'
  | 'Completed'
  | 'Cancelled';

/**
 * Precedence: Cancelled > Completed > Full > InProgress > Upcoming.
 * Full means every place is taken (booked >= capacity).
 */
export function getClassState(
  status: SessionStatus,
  bookedPlaces: number,
  capacity: number
): ClassState {
  if (status === 'Cancelled') {
    return 'Cancelled';
  }

  if (status === 'Completed') {
    return 'Completed';
  }

  if (capacity > 0 && bookedPlaces >= capacity) {
    return 'Full';
  }

  if (status === 'InProgress') {
    return 'InProgress';
  }

  return 'Upcoming';
}

/** Percentage of the capacity that is booked (0 when capacity is 0). */
export function fillPercent(bookedPlaces: number, capacity: number): number {
  return capacity > 0 ? (bookedPlaces / capacity) * 100 : 0;
}

/** True when the class is 90% full or more (red dot / red bar). */
export function isAlmostFull(bookedPlaces: number, capacity: number): boolean {
  return fillPercent(bookedPlaces, capacity) >= 90;
}
