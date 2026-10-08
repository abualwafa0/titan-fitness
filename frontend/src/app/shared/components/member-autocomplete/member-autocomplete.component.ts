import { Component, DestroyRef, effect, input, model, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent
} from '@angular/material/autocomplete';
import {
  Subject,
  catchError,
  debounceTime,
  of,
  switchMap,
  tap
} from 'rxjs';

import { MemberLookup } from '../../../members/member.model';
import { MemberService } from '../../../members/member.service';
import { AvatarComponent } from '../avatar/avatar.component';
import { IconComponent } from '../icon/icon.component';
import { StatusBadgeComponent } from '../status-badge/status-badge.component';

const LOOKUP_LIMIT = 20;
const DEBOUNCE_MS = 250;

/**
 * Member picker: type a name or ID, choose from the list.
 *
 *   <app-member-autocomplete [(member)]="selectedMember" />
 *   <app-member-autocomplete [member]="locked" [disabled]="true" />
 *
 * `member` is null until the user picks someone (typing again clears it).
 */
@Component({
  selector: 'app-member-autocomplete',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    AvatarComponent,
    IconComponent,
    StatusBadgeComponent
  ],
  templateUrl: './member-autocomplete.component.html',
  styleUrl: './member-autocomplete.component.css'
})
export class MemberAutocompleteComponent {
  readonly member = model<MemberLookup | null>(null);
  readonly disabled = input<boolean>(false);

  readonly searchControl = new FormControl<string | MemberLookup>('', {
    nonNullable: true
  });

  readonly results = signal<MemberLookup[]>([]);
  readonly loading = signal<boolean>(false);

  private readonly searchTerms = new Subject<string>();

  constructor(
    private readonly memberService: MemberService,
    destroyRef: DestroyRef
  ) {
    this.searchTerms
      .pipe(
        debounceTime(DEBOUNCE_MS),
        tap(() => this.loading.set(true)),
        switchMap((term) =>
          this.memberService
            .lookup(term, LOOKUP_LIMIT)
            .pipe(catchError(() => of<MemberLookup[]>([])))
        ),
        takeUntilDestroyed(destroyRef)
      )
      .subscribe((members) => {
        this.results.set(members);
        this.loading.set(false);
      });

    // Keep the input in sync when the parent sets / clears `member`.
    effect(() => {
      const selected = this.member();
      const current = this.searchControl.value;

      if (selected) {
        if (current !== selected) {
          this.searchControl.setValue(selected, { emitEvent: false });
        }
      } else if (typeof current !== 'string') {
        this.searchControl.setValue('', { emitEvent: false });
      }
    });

    effect(() => {
      if (this.disabled()) {
        this.searchControl.disable({ emitEvent: false });
      } else {
        this.searchControl.enable({ emitEvent: false });
      }
    });
  }

  readonly displayMember = (value: MemberLookup | string | null): string =>
    typeof value === 'string' ? value : value?.fullName ?? '';

  onFocus(): void {
    const current = this.searchControl.value;

    this.searchTerms.next(typeof current === 'string' ? current : '');
  }

  onInput(event: Event): void {
    const text = (event.target as HTMLInputElement).value;

    if (this.member() !== null) {
      this.member.set(null);
    }

    this.searchTerms.next(text.trim());
  }

  onSelected(event: MatAutocompleteSelectedEvent): void {
    this.member.set(event.option.value as MemberLookup);
  }
}
