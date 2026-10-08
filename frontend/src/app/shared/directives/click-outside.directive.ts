import { Directive, ElementRef, HostListener, output } from '@angular/core';

/**
 * Emits `clickOutside` when the user clicks anywhere outside the host element.
 * Use it to close menus: <div appClickOutside (clickOutside)="close()">
 */
@Directive({
  selector: '[appClickOutside]',
  standalone: true
})
export class ClickOutsideDirective {
  readonly clickOutside = output<MouseEvent>();

  constructor(private readonly host: ElementRef<HTMLElement>) {}

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as Node | null;

    if (target && !this.host.nativeElement.contains(target)) {
      this.clickOutside.emit(event);
    }
  }
}
