import { Component, Signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { IconComponent } from '../../../shared/components/icon/icon.component';
import { Branch } from '../../models/branch.model';
import { BranchService } from '../../services/branch.service';
import { HeaderSearchService } from '../../services/header-search.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, IconComponent],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  readonly branches: Signal<Branch[]>;
  readonly currentBranchId: Signal<number | null>;
  readonly draft: Signal<string>;
  readonly placeholder: Signal<string>;

  constructor(
    private readonly branchService: BranchService,
    private readonly searchService: HeaderSearchService
  ) {
    this.branches = this.branchService.branches;
    this.currentBranchId = this.branchService.currentBranchId;
    this.draft = this.searchService.draft;
    this.placeholder = this.searchService.placeholder;
  }

  onBranchChange(event: Event): void {
    const value = Number((event.target as HTMLSelectElement).value);

    if (!Number.isNaN(value)) {
      this.branchService.setCurrentBranch(value);
    }
  }

  onSearchInput(event: Event): void {
    this.searchService.onInput((event.target as HTMLInputElement).value);
  }
}
