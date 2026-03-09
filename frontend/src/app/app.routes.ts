import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { ShellComponent } from './shell/shell.component';
import { authGuard } from './core/auth.guard';
import { ClientsComponent } from './clients/clients.component';
import { ProjectsComponent } from './projects/projects.component';
import { EmployeesComponent } from './employees/employees.component';
import { TimesheetsComponent } from './timesheets/timesheets.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: 'clients', component: ClientsComponent },
      { path: 'projects', component: ProjectsComponent },
      { path: 'employees', component: EmployeesComponent },
      { path: 'timesheets', component: TimesheetsComponent },
      { path: '', redirectTo: 'clients', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
