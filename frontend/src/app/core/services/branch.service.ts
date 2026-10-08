import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Branch, Studio } from '../models/branch.model';

/** Holds the branches and the "current branch" selected in the header. */
@Injectable({ providedIn: 'root' })
export class BranchService {
  private readonly branchesState = signal<Branch[]>([]);
  private readonly currentBranchIdState = signal<number | null>(null);

  readonly branches = this.branchesState.asReadonly();
  readonly currentBranchId = this.currentBranchIdState.asReadonly();

  readonly currentBranch = computed<Branch | null>(() => {
    const id = this.currentBranchIdState();

    return this.branchesState().find((branch) => branch.branchId === id) ?? null;
  });

  constructor(private readonly http: HttpClient) {}

  /** Loads the branches once and selects the first one as the current branch. */
  load(): void {
    if (this.branchesState().length > 0) {
      return;
    }

    this.http.get<Branch[]>(`${environment.apiUrl}/branches`).subscribe({
      next: (branches) => {
        this.branchesState.set(branches);

        if (this.currentBranchIdState() === null && branches.length > 0) {
          this.currentBranchIdState.set(branches[0].branchId);
        }
      },
      error: () => {
        // The error interceptor already shows the message.
      }
    });
  }

  setCurrentBranch(branchId: number): void {
    this.currentBranchIdState.set(branchId);
  }

  getStudios(branchId: number): Observable<Studio[]> {
    return this.http.get<Studio[]>(
      `${environment.apiUrl}/branches/${branchId}/studios`
    );
  }
}
