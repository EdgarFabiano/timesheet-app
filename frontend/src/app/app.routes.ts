import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { ShellComponent } from './shell/shell.component';
import { authGuard } from './core/auth.guard';
import { adminGuard } from './core/admin.guard';
import { employeeGuard } from './core/employee.guard';
import { ClientsComponent } from './clients/clients.component';
import { ProjectsComponent } from './projects/projects.component';
import { EmployeesComponent } from './employees/employees.component';
import { TimesheetsComponent } from './timesheets/timesheets.component';
import { DashboardComponent } from './dashboard/dashboard.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: 'clients', component: ClientsComponent, canActivate: [adminGuard] },
      { path: 'projects', component: ProjectsComponent, canActivate: [adminGuard] },
      { path: 'employees', component: EmployeesComponent, canActivate: [adminGuard] },
      { path: 'timesheets', component: TimesheetsComponent, canActivate: [employeeGuard] },
      { path: '', component: DashboardComponent }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
