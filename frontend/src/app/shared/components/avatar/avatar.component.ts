import { Component, computed, input, signal } from '@angular/core';

import { environment } from '../../../../environments/environment';

/**
 * Round picture of a person. Shows the photo when there is one, otherwise the
 * initials of the name.
 *
 *   <app-avatar [name]="member.fullName" [photo]="member.photo" [size]="34" />
 */
@Component({
  selector: 'app-avatar',
  standalone: true,
  templateUrl: './avatar.component.html',
  styleUrl: './avatar.component.css'
})
export class AvatarComponent {
  readonly name = input<string>('');
  /** Path returned by the API (e.g. "uploads/members/a.jpg") or a full URL. */
  readonly photo = input<string | null>(null);
  readonly size = input<number>(34);

  private readonly failedUrl = signal<string | null>(null);

  readonly initials = computed(() => {
    const parts = this.name().trim().split(/\s+/).filter(Boolean);

    if (parts.length === 0) {
      return '?';
    }

    const first = parts[0].charAt(0);
    const last = parts.length > 1 ? parts[parts.length - 1].charAt(0) : '';

    return (first + last).toUpperCase();
  });

  readonly photoUrl = computed<string | null>(() => {
    const photo = this.photo();

    if (!photo) {
      return null;
    }

    if (/^https?:\/\//i.test(photo)) {
      return photo;
    }

    return `${environment.filesUrl}/${photo.replace(/^\/+/, '')}`;
  });

  /** The photo URL to show (null when missing or when it failed to load). */
  readonly photoSrc = computed<string | null>(() => {
    const url = this.photoUrl();

    return url && url !== this.failedUrl() ? url : null;
  });

  onImageError(): void {
    this.failedUrl.set(this.photoUrl());
  }
}
