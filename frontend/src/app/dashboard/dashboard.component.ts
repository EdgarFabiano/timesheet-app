import { Component, OnInit, signal, computed, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { HttpParams } from '@angular/common/http';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableDataSource } from '@angular/material/table';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { TimesheetsService } from '../timesheets/timesheets.service';
import { EmployeesService } from '../core/employees.service';
import { ProjectsService } from '../core/projects.service';
import { Timesheet } from '../timesheets/timesheet.model';
import { Employee } from '../core/employee.model';
import { Project } from '../core/project.model';
import { AuthService } from '../core/auth.service';

interface DailyTotal {
  date: string;
  dayName: string;
  totalHours: number;
}

interface ProjectTotal {
  projectId: string;
  projectName: string;
  totalHours: number;
  percentage: number;
}

interface EmployeeTotal {
  employeeId: string;
  employeeName: string;
  totalHours: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatInputModule,
    MatTabsModule,
    BaseChartDirective
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  private timesheetsService = inject(TimesheetsService);
  private employeesService = inject(EmployeesService);
  private projectsService = inject(ProjectsService);
  private authService = inject(AuthService);

  employees = signal<Employee[]>([]);
  projects = signal<Project[]>([]);
  timesheets = signal<Timesheet[]>([]);
  loading = signal(false);
  isAdmin = signal(false);

  startDate = signal<Date>(this.getWeekStart(new Date()));
  endDate = signal<Date>(this.getWeekEnd(new Date()));
  selectedEmployeeId = signal<string | null>(null);
  selectedProjectId = signal<string | null>(null);

  currentUser = this.authService.getUser();
  private currentEmployeeId = signal<string | null>(null);

  displayedColumns = computed(() => {
    const cols = ['date', 'project', 'hours', 'notes'];
    if (this.isAdmin()) {
      cols.splice(1, 0, 'employee');
    }
    return cols;
  });

  dataSource = computed(() => {
    const data = this.filteredTimesheets();
    return new MatTableDataSource<Timesheet>(data);
  });

  filteredTimesheets = computed(() => {
    let data = this.timesheets();
    const empId = this.selectedEmployeeId();
    const projId = this.selectedProjectId();

    if (empId) {
      data = data.filter(t => t.employeeId === empId);
    }
    if (projId) {
      data = data.filter(t => t.projectId === projId);
    }
    return data.sort((a, b) => b.date.localeCompare(a.date));
  });

  weeklyTotal = computed(() => {
    return this.filteredTimesheets().reduce((sum, t) => sum + t.hoursWorked, 0);
  });

  dailyTotals = computed<DailyTotal[]>(() => {
    const totals: { [key: string]: number } = {};
    const dayNames: { [key: string]: string } = {};

    this.filteredTimesheets().forEach(t => {
      totals[t.date] = (totals[t.date] || 0) + t.hoursWorked;
      const date = new Date(t.date + 'T00:00:00');
      dayNames[t.date] = this.getDayName(date.getDay());
    });

    return Object.entries(totals)
      .map(([date, hours]) => ({
        date,
        dayName: dayNames[date] || date,
        totalHours: hours
      }))
      .sort((a, b) => a.date.localeCompare(b.date));
  });

  projectTotals = computed<ProjectTotal[]>(() => {
    const totals: { [key: string]: number } = {};
    const names: { [key: string]: string } = {};
    const allHours = this.filteredTimesheets().reduce((sum, t) => sum + t.hoursWorked, 0);

    this.filteredTimesheets().forEach(t => {
      totals[t.projectId] = (totals[t.projectId] || 0) + t.hoursWorked;
      names[t.projectId] = t.projectName;
    });

    return Object.entries(totals)
      .map(([projectId, hours]) => ({
        projectId,
        projectName: names[projectId],
        totalHours: hours,
        percentage: allHours > 0 ? Math.round((hours / allHours) * 100) : 0
      }))
      .sort((a, b) => b.totalHours - a.totalHours);
  });

  employeeTotals = computed<EmployeeTotal[]>(() => {
    const totals: { [key: string]: number } = {};
    const names: { [key: string]: string } = {};

    this.filteredTimesheets().forEach(t => {
      totals[t.employeeId] = (totals[t.employeeId] || 0) + t.hoursWorked;
      names[t.employeeId] = t.employeeName;
    });

    return Object.entries(totals)
      .map(([employeeId, hours]) => ({
        employeeId,
        employeeName: names[employeeId],
        totalHours: hours
      }))
      .sort((a, b) => b.totalHours - a.totalHours);
  });

  barChartData = computed<ChartData<'bar'>>(() => {
    const daily = this.dailyTotals();
    return {
      labels: daily.map(d => d.dayName),
      datasets: [{
        data: daily.map(d => d.totalHours),
        label: 'Hours',
        backgroundColor: '#1976d2',
        borderRadius: 4
      }]
    };
  });

  barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      title: { display: true, text: 'Daily Hours' }
    },
    scales: {
      y: { beginAtZero: true, title: { display: true, text: 'Hours' } }
    }
  };

  pieChartData = computed<ChartData<'pie'>>(() => {
    const projects = this.projectTotals();
    return {
      labels: projects.map(p => p.projectName),
      datasets: [{
        data: projects.map(p => p.totalHours),
        backgroundColor: [
          '#1976d2', '#388e3c', '#f57c00', '#7b1fa2', '#d32f2f',
          '#00796b', '#5d4037', '#455a64', '#c2185b', '#512da8'
        ]
      }]
    };
  });

  pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'right' },
      title: { display: true, text: 'Project Distribution' }
    }
  };

  employeeChartData = computed<ChartData<'bar'>>(() => {
    const employees = this.employeeTotals();
    return {
      labels: employees.map(e => e.employeeName),
      datasets: [{
        data: employees.map(e => e.totalHours),
        label: 'Hours',
        backgroundColor: '#388e3c',
        borderRadius: 4
      }]
    };
  });

  employeeChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: 'y',
    plugins: {
      legend: { display: false },
      title: { display: true, text: 'Hours by Employee' }
    },
    scales: {
      x: { beginAtZero: true, title: { display: true, text: 'Hours' } }
    }
  };

  ngOnInit(): void {
    this.isAdmin.set(this.authService.isAdmin());
    
    if (!this.isAdmin()) {
      this.startDate.set(this.getWeekStart(new Date()));
      this.endDate.set(this.getWeekEnd(new Date()));
    } else {
      const oneYearAgo = new Date();
      oneYearAgo.setFullYear(oneYearAgo.getFullYear() - 1);
      this.startDate.set(oneYearAgo);
      this.endDate.set(new Date());
    }
    
    this.loadData();
  }

  private getWeekStart(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    d.setDate(d.getDate() - day);
    d.setHours(0, 0, 0, 0);
    return d;
  }

  private getWeekEnd(date: Date): Date {
    const start = this.getWeekStart(date);
    const end = new Date(start);
    end.setDate(end.getDate() + 6);
    end.setHours(23, 59, 59, 999);
    return end;
  }

  private getDayName(day: number): string {
    return ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'][day];
  }

  loadData(): void {
    this.loading.set(true);

    this.projectsService.getAll().subscribe({
      next: (projects) => this.projects.set(projects.filter(p => p.isActive))
    });

    this.employeesService.getAll().subscribe({
      next: (employees) => {
        this.employees.set(employees.filter(e => e.isActive));

        if (!this.isAdmin() && this.currentUser) {
          const currentEmp = employees.find(e => e.email === this.currentUser?.email);
          if (currentEmp) {
            this.currentEmployeeId.set(currentEmp.id);
            this.selectedEmployeeId.set(currentEmp.id);
          }
        } else {
          this.selectedEmployeeId.set(null);
        }
        this.loadTimesheets();
      }
    });
  }

  loadTimesheets(): void {
    this.loading.set(true);

    const params = new HttpParams()
      .set('startDate', this.formatDate(this.startDate()))
      .set('endDate', this.formatDate(this.endDate()));

    if (this.selectedEmployeeId()) {
      params.set('employeeId', this.selectedEmployeeId()!);
    }

    if (this.selectedProjectId()) {
      params.set('projectId', this.selectedProjectId()!);
    }

    this.timesheetsService.getWithFilters(params).subscribe({
      next: (data) => {
        this.timesheets.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onFilterChange(): void {
    this.loadTimesheets();
  }

  onStartDateChange(event: any): void {
    if (event.value) {
      this.startDate.set(event.value);
      this.loadTimesheets();
    }
  }

  onEndDateChange(event: any): void {
    if (event.value) {
      this.endDate.set(event.value);
      this.loadTimesheets();
    }
  }

  setDateRange(range: 'week' | 'month' | 'year'): void {
    const today = new Date();
    let start: Date;

    switch (range) {
      case 'week':
        start = this.getWeekStart(today);
        break;
      case 'month':
        start = new Date(today.getFullYear(), today.getMonth(), 1);
        break;
      case 'year':
        start = new Date(today.getFullYear(), 0, 1);
        break;
    }

    this.startDate.set(start);
    this.endDate.set(today);
    this.loadTimesheets();
  }

  exportToCsv(): void {
    const data = this.filteredTimesheets();
    if (data.length === 0) return;

    const headers = ['Date', 'Employee', 'Project', 'Hours', 'Notes'];
    const rows = data.map(t => [
      t.date,
      t.employeeName,
      t.projectName,
      t.hoursWorked.toString(),
      t.notes || ''
    ]);

    const csvContent = [headers, ...rows]
      .map(row => row.map(cell => `"${cell.replace(/"/g, '""')}"`).join(','))
      .join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `timesheet-report-${this.startDate().toISOString().split('T')[0]}-${this.endDate().toISOString().split('T')[0]}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  get weekRangeDisplay(): string {
    const options: Intl.DateTimeFormatOptions = { month: 'short', day: 'numeric', year: 'numeric' };
    return `${this.startDate().toLocaleDateString('en-US', options)} - ${this.endDate().toLocaleDateString('en-US', options)}`;
  }

  private formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
