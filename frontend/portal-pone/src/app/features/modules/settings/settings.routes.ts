import { Routes } from '@angular/router';
import { SettingsHomeComponent } from './pages/settings-home/settings-home.component';

export const routes: Routes = [
  {
    path: '',
    component: SettingsHomeComponent,
    title: 'Configurações'
  }
];
