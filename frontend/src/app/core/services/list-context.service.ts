import { Injectable } from '@angular/core';
import { Params } from '@angular/router';

export type ListKey = 'members' | 'trainers' | 'plans';

/**
 * Remembers the query params (page, sort, search, filters) of the last list
 * the user was on, so "Back to list" returns to the same place.
 */
@Injectable({ providedIn: 'root' })
export class ListContextService {
  private readonly store = new Map<ListKey, Params>();

  save(key: ListKey, params: Params): void {
    this.store.set(key, { ...params });
  }

  get(key: ListKey): Params {
    return { ...(this.store.get(key) ?? {}) };
  }
}
