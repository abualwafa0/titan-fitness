import { Injectable, computed, signal } from '@angular/core';

export type UserRole = 'manager' | 'front-desk';

/**
 * Minimal session holder. There is no real sign-in yet: the token is empty and
 * the role is fixed to 'manager' so every screen is available.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenState = signal<string>('');
  private readonly roleState = signal<UserRole>('manager');

  readonly token = this.tokenState.asReadonly();
  readonly role = this.roleState.asReadonly();
  readonly isManager = computed(() => this.roleState() === 'manager');

  clearSession(): void {
    this.tokenState.set('');
  }
}
