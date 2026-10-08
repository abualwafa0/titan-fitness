import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/** Placeholder page. Sign-in is not implemented yet. */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {}
