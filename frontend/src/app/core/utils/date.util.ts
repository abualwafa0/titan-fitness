/**
 * Date helpers. All values are LOCAL (no time zone conversion):
 * - dates are 'yyyy-MM-dd' strings
 * - times are 'HH:mm' or 'HH:mm:ss' strings
 * - date-times sent to the API are 'yyyy-MM-ddTHH:mm:ss' WITHOUT a 'Z'
 */

const MONTHS = [
  'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
  'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'
];

const DAY_IN_MS = 86_400_000;

function pad(value: number): string {
  return value.toString().padStart(2, '0');
}

function daysInMonth(year: number, monthIndex: number): number {
  return new Date(year, monthIndex + 1, 0).getDate();
}

function splitDate(iso: string): [number, number, number] {
  const [year, month, day] = iso.substring(0, 10).split('-').map(Number);

  return [year, month, day];
}

/** Local Date -> 'yyyy-MM-dd'. */
export function toIsoDate(date: Date): string {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

/** Today as 'yyyy-MM-dd'. */
export function todayIso(): string {
  return toIsoDate(new Date());
}

/** 'yyyy-MM-dd' -> local Date at 00:00. */
export function parseIsoDate(iso: string): Date {
  const [year, month, day] = splitDate(iso);

  return new Date(year, month - 1, day);
}

/** Adds (or subtracts) days to a 'yyyy-MM-dd' date. */
export function addDays(iso: string, days: number): string {
  const date = parseIsoDate(iso);
  date.setDate(date.getDate() + days);

  return toIsoDate(date);
}

/**
 * Adds months to a 'yyyy-MM-dd' date using the same rule as .NET
 * DateOnly.AddMonths (the day is clamped to the last day of the month).
 */
export function addMonths(iso: string, months: number): string {
  const [year, month, day] = splitDate(iso);
  const target = new Date(year, month - 1 + months, 1);
  const lastDay = daysInMonth(target.getFullYear(), target.getMonth());

  return toIsoDate(
    new Date(target.getFullYear(), target.getMonth(), Math.min(day, lastDay))
  );
}

/** Number of days from one date to another (negative when `to` is earlier). */
export function diffDays(fromIso: string, toIso: string): number {
  const from = parseIsoDate(fromIso);
  const to = parseIsoDate(toIso);

  const fromUtc = Date.UTC(from.getFullYear(), from.getMonth(), from.getDate());
  const toUtc = Date.UTC(to.getFullYear(), to.getMonth(), to.getDate());

  return Math.round((toUtc - fromUtc) / DAY_IN_MS);
}

/** Copy of the date with seconds cleared and minutes rounded DOWN to 5. */
export function roundDownTo5(date: Date): Date {
  const rounded = new Date(date);

  rounded.setSeconds(0, 0);
  rounded.setMinutes(rounded.getMinutes() - (rounded.getMinutes() % 5));

  return rounded;
}

/** Local Date -> 'HH:mm' (value of an <input type="time">). */
export function toTimeInput(date: Date): string {
  return `${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

/** 'HH:mm' or 'HH:mm:ss' -> 'HH:mm:ss'. */
export function normalizeTime(time: string): string {
  const [hours, minutes, seconds] = time.split(':').map(Number);

  return `${pad(hours || 0)}:${pad(minutes || 0)}:${pad(seconds || 0)}`;
}

/** Joins a date and a time into 'yyyy-MM-ddTHH:mm:ss' (no 'Z'). */
export function combineLocal(dateIso: string, time: string): string {
  return `${dateIso.substring(0, 10)}T${normalizeTime(time)}`;
}

/** Local Date of a session / check-in given its date and time. */
export function toLocalDate(dateIso: string, time: string): Date {
  const [year, month, day] = splitDate(dateIso);
  const [hours, minutes, seconds] = normalizeTime(time).split(':').map(Number);

  return new Date(year, month - 1, day, hours, minutes, seconds);
}

/** Parses an API date-time ('yyyy-MM-ddTHH:mm:ss', no 'Z') as local time. */
export function parseLocalDateTime(value: string): Date {
  return value.length <= 10 ? parseIsoDate(value) : new Date(value);
}

/** True when the given date + time is later than now. */
export function isFutureDateTime(dateIso: string, time: string): boolean {
  return toLocalDate(dateIso, time).getTime() > Date.now();
}

/** 'HH:mm[:ss]', an API date-time string or a Date -> '08:30 AM'. */
export function formatTime12(value: string | Date): string {
  let hours: number;
  let minutes: number;

  if (value instanceof Date) {
    hours = value.getHours();
    minutes = value.getMinutes();
  } else if (value.includes('T')) {
    const date = new Date(value);

    hours = date.getHours();
    minutes = date.getMinutes();
  } else {
    [hours, minutes] = value.split(':').map(Number);
  }

  const suffix = hours >= 12 ? 'PM' : 'AM';
  const hours12 = hours % 12 === 0 ? 12 : hours % 12;

  return `${pad(hours12)}:${pad(minutes)} ${suffix}`;
}

/** Minutes since midnight for 'HH:mm[:ss]'. */
export function minutesOfDay(time: string): number {
  const [hours, minutes] = time.split(':').map(Number);

  return hours * 60 + minutes;
}

/** Adds minutes to a time of day and returns 'HH:mm:ss' (wraps at 24h). */
export function addMinutesToTime(time: string, minutes: number): string {
  const total = (minutesOfDay(time) + minutes + 24 * 60) % (24 * 60);

  return `${pad(Math.floor(total / 60))}:${pad(total % 60)}:00`;
}

/** 'yyyy-MM-dd' -> 'Oct 12, 2023'. */
export function formatDateMedium(iso: string): string {
  const [year, month, day] = splitDate(iso);

  return `${MONTHS[month - 1]} ${pad(day)}, ${year}`;
}

/** 'Today' / 'Tomorrow' / 'Yesterday', otherwise 'Oct 12, 2023'. */
export function relativeDayLabel(iso: string): string {
  const today = todayIso();

  if (iso === today) {
    return 'Today';
  }

  if (iso === addDays(today, 1)) {
    return 'Tomorrow';
  }

  if (iso === addDays(today, -1)) {
    return 'Yesterday';
  }

  return formatDateMedium(iso);
}
