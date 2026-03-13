import { Component, ViewChild, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatSidenavModule, MatSidenav } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../core/auth.service';
import { ThemeService } from '../core/theme.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatSlideToggleModule,
    MatDividerModule
  ],
  templateUrl: './shell.component.html',
  styleUrls: ['./shell.component.css']
})
export class ShellComponent {
  @ViewChild('sidenav') sidenav!: MatSidenav;

  isAdmin = signal(false);
  currentUser = signal<{ email: string; role: string } | null>(null);
  isDarkMode = signal(false);

  constructor(private auth: AuthService, private theme: ThemeService) {
    const user = this.auth.getUser();
    this.currentUser.set(user);
    this.isAdmin.set(this.auth.isAdmin());
    this.isDarkMode.set(this.theme.isDarkMode());
  }

  toggleTheme() {
    this.theme.toggleTheme();
    this.isDarkMode.set(this.theme.isDarkMode());
  }

  logout() {
    this.auth.logout();
  }
}
