import { Injectable, signal, effect } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly THEME_KEY = 'dark_mode';
  
  isDarkMode = signal(this.loadTheme());

  constructor() {
    effect(() => {
      const isDark = this.isDarkMode();
      document.documentElement.style.colorScheme = isDark ? 'dark' : 'light';
      localStorage.setItem(this.THEME_KEY, JSON.stringify(isDark));
    });
  }

  toggleTheme() {
    this.isDarkMode.update(v => !v);
  }

  private loadTheme(): boolean {
    const stored = localStorage.getItem(this.THEME_KEY);
    return stored ? JSON.parse(stored) : false;
  }
}
