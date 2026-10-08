import { HttpParams } from '@angular/common/http';

type ParamItem = string | number | boolean | null | undefined;

export type ParamValue = ParamItem | Array<ParamItem>;

/**
 * Builds HttpParams from a plain object.
 * - null, undefined and '' are skipped
 * - arrays become repeated keys (branchIds=1&branchIds=2)
 */
export function buildParams(source: Record<string, ParamValue>): HttpParams {
  let params = new HttpParams();

  for (const [key, value] of Object.entries(source)) {
    if (Array.isArray(value)) {
      for (const item of value) {
        if (item !== null && item !== undefined && item !== '') {
          params = params.append(key, String(item));
        }
      }

      continue;
    }

    if (value === null || value === undefined || value === '') {
      continue;
    }

    params = params.append(key, String(value));
  }

  return params;
}
