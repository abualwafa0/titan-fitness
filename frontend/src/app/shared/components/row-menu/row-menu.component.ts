import { Component, HostListener, input, output, signal } from '@angular/core';
import { MatTooltip } from '@angular/material/tooltip';

import { ClickOutsideDirective } from '../../directives/click-outside.directive';
import { IconComponent } from '../icon/icon.component';

export interface RowMenuItem {
  id: string;
  label: string;
  disabled?: boolean;
  /** Shown as a tooltip when the item is disabled. */
  disabledReason?: string;
}

const ITEM_HEIGHT = 40;
const PANEL_PADDING = 8;
const PANEL_GAP = 4;

/**
 * 3-dot menu for a table row. Disabled items stay visible and explain why.
 * <app-row-menu [items]="items" (itemSelected)="onAction($event)" />
 */
@Component({
  selector: 'app-row-menu',
  standalone: true,
  imports: [ClickOutsideDirective, MatTooltip, IconComponent],
  templateUrl: './row-menu.component.html',
  styleUrl: './row-menu.component.css'
})
export class RowMenuComponent {
  readonly items = input<RowMenuItem[]>([]);

  readonly itemSelected = output<string>();
  /** Convenience outputs for the two common actions (item ids 'view' and 'edit'). */
  readonly view = output<void>();
  readonly edit = output<void>();

  readonly isOpen = signal(false);
  readonly panelTop = signal(0);
  readonly panelRight = signal(0);

  toggle(event: MouseEvent): void {
    if (this.isOpen()) {
      this.close();
      return;
    }

    const button = event.currentTarget as HTMLElement;
    const rect = button.getBoundingClientRect();
    const panelHeight = this.items().length * ITEM_HEIGHT + PANEL_PADDING;
    const fitsBelow =
      rect.bottom + PANEL_GAP + panelHeight <= window.innerHeight;

    this.panelTop.set(
      fitsBelow
        ? rect.bottom + PANEL_GAP
        : Math.max(PANEL_GAP, rect.top - PANEL_GAP - panelHeight)
    );
    this.panelRight.set(Math.max(0, window.innerWidth - rect.right));
    this.isOpen.set(true);
  }

  select(item: RowMenuItem): void {
    if (item.disabled) {
      return;
    }

    this.close();
    this.itemSelected.emit(item.id);

    if (item.id === 'view') {
      this.view.emit();
    } else if (item.id === 'edit') {
      this.edit.emit();
    }
  }

  close(): void {
    this.isOpen.set(false);
  }

  @HostListener('window:resize')
  @HostListener('window:scroll')
  onViewportChange(): void {
    if (this.isOpen()) {
      this.close();
    }
  }
}
