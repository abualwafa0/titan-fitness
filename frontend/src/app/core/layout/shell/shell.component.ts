import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { BranchService } from '../../services/branch.service';
import { HeaderComponent } from '../header/header.component';
import { SidebarComponent } from '../sidebar/sidebar.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, HeaderComponent],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css'
})
export class ShellComponent implements OnInit {
  constructor(private readonly branchService: BranchService) {}

  ngOnInit(): void {
    this.branchService.load();
  }
}
