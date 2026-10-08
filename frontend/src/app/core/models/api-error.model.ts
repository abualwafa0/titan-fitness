/** Field errors returned with a 400 validation response (keys are camelCase). */
export type FieldErrors = Record<string, string[]>;

/** ProblemDetails body returned by the API for every error response. */
export interface ApiProblem {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: FieldErrors;
}
