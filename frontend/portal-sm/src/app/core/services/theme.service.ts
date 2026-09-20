import { Injectable, signal, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type Theme = 'dark' | 'light';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'nexas_theme_preference';
  private platformId = inject(PLATFORM_ID);
  
  // Usamos um Signal para estado reativo moderno (Angular 16+)
  public currentTheme = signal<Theme>('light');

  constructor() {
    this.initTheme();
  }

  private initTheme(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    // 1. Verificar localStorage
    const savedTheme = localStorage.getItem(this.THEME_KEY) as Theme;
    if (savedTheme === 'dark' || savedTheme === 'light') {
      this.setTheme(savedTheme);
      return;
    }

    // 2. Se não houver nada salvo, o Light é o padrão
    this.setTheme('light');
  }

  public toggleTheme(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const nextTheme = this.currentTheme() === 'dark' ? 'light' : 'dark';
    this.setTheme(nextTheme);
  }

  private setTheme(theme: Theme): void {
    this.currentTheme.set(theme);
    
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.THEME_KEY, theme);
      
      // Atualiza a classe no body
      if (theme === 'light') {
        document.body.classList.add('theme-light');
        document.body.classList.remove('theme-dark');
      } else {
        document.body.classList.add('theme-dark');
        document.body.classList.remove('theme-light');
      }
    }
  }
}
