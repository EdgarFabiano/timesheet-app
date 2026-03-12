import { Component, OnInit, signal, computed, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTableDataSource } from '@angular/material/table';
import { TimesheetsService } from './timesheets.service';
import { EmployeesService } from '../core/employees.service';
import { ProjectsService } from '../core/projects.service';
import { Timesheet, CreateTimesheetRequest, UpdateTimesheetRequest } from './timesheet.model';
import { Employee } from '../core/employee.model';
import { Project } from '../core/project.model';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-timesheets',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './timesheets.component.html',
  styleUrls: ['./timesheets.component.css']
})
export class TimesheetsComponent implements OnInit {
  private timesheetsService = inject(TimesheetsService);
  private employeesService = inject(EmployeesService);
  private projectsService = inject(ProjectsService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  timesheets = signal<Timesheet[]>([]);
  employees = signal<Employee[]>([]);
  projects = signal<Project[]>([]);
  loading = signal(false);
  isAdmin = signal(false);
  
  selectedEmployeeId = signal<string | null>(null);
  selectedProjectId = signal<string | null>(null);
  weekStart = signal<Date>(this.getWeekStart(new Date()));
  weekEnd = signal<Date>(this.getWeekEnd(new Date()));

  displayedColumns = computed(() => {
    const cols = ['date', 'projectName', 'hoursWorked', 'notes', 'actions'];
    if (this.isAdmin()) {
      cols.splice(1, 0, 'employeeName');
    }
    return cols;
  });
  dataSource = new MatTableDataSource<Timesheet>();
  
  dialogOpen = signal(false);
  editingTimesheet = signal<Timesheet | null>(null);
  timesheetForm!: FormGroup;

  currentUser = this.authService.getUser();
  private currentEmployeeId = signal<string | null>(null);

  ngOnInit(): void {
    this.isAdmin.set(this.authService.isAdmin());
    this.initForm();
    this.loadData();
  }

  private initForm(): void {
    this.timesheetForm = this.fb.group({
      projectId: ['', Validators.required],
      date: [new Date(), Validators.required],
      hoursWorked: ['', [Validators.required, Validators.min(0.5), Validators.max(24)]],
      notes: ['']
    });
  }

  private getWeekStart(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day;
    d.setDate(diff);
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

  private formatDateForApi(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  loadData(): void {
    this.loading.set(true);
    this.projectsService.getAll().subscribe({
      next: (projects) => this.projects.set(projects.filter(p => p.isActive)),
      error: () => {}
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
        }
        this.loadTimesheets();
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  loadTimesheets(): void {
    this.loading.set(true);
    
    const employeeId = this.selectedEmployeeId();
    const projectId = this.selectedProjectId();
    const startDate = this.formatDateForApi(this.weekStart());
    const endDate = this.formatDateForApi(this.weekEnd());

    if (employeeId) {
      this.timesheetsService.getByEmployeeAndDateRange(employeeId, startDate, endDate).subscribe({
        next: (data) => {
          this.timesheets.set(data);
          this.dataSource.data = data;
          this.loading.set(false);
        },
        error: () => {
          this.snackBar.open('Failed to load timesheets', 'Close', { duration: 3000 });
          this.loading.set(false);
        }
      });
    } else if (projectId) {
      this.timesheetsService.getByProject(projectId, startDate, endDate).subscribe({
        next: (data) => {
          this.timesheets.set(data);
          this.dataSource.data = data;
          this.loading.set(false);
        },
        error: () => {
          this.snackBar.open('Failed to load timesheets', 'Close', { duration: 3000 });
          this.loading.set(false);
        }
      });
    } else {
      this.loading.set(false);
    }
  }

  onWeekChange(direction: 'prev' | 'next'): void {
    const current = this.weekStart();
    const newDate = new Date(current);
    if (direction === 'prev') {
      newDate.setDate(newDate.getDate() - 7);
    } else {
      newDate.setDate(newDate.getDate() + 7);
    }
    this.weekStart.set(this.getWeekStart(newDate));
    this.weekEnd.set(this.getWeekEnd(newDate));
    this.loadTimesheets();
  }

  goToCurrentWeek(): void {
    this.weekStart.set(this.getWeekStart(new Date()));
    this.weekEnd.set(this.getWeekEnd(new Date()));
    this.loadTimesheets();
  }

  onFilterChange(): void {
    this.loadTimesheets();
  }

  canEditOrDelete(timesheet: Timesheet): boolean {
    if (this.isAdmin()) return true;
    return timesheet.employeeId === this.currentEmployeeId();
  }

  openAddDialog(): void {
    this.editingTimesheet.set(null);
    this.timesheetForm.reset({ date: new Date(), notes: '' });
    this.dialogOpen.set(true);
    this.cdr.detectChanges();
  }

  openEditDialog(timesheet: Timesheet): void {
    this.editingTimesheet.set(timesheet);
    this.timesheetForm.patchValue({
      projectId: timesheet.projectId,
      date: new Date(timesheet.date),
      hoursWorked: timesheet.hoursWorked,
      notes: timesheet.notes || ''
    });
    this.dialogOpen.set(true);
    this.cdr.detectChanges();
  }

  closeDialog(): void {
    this.dialogOpen.set(false);
    this.editingTimesheet.set(null);
  }

  saveTimesheet(): void {
    if (this.timesheetForm.invalid) return;

    const formValue = this.timesheetForm.value;
    const editing = this.editingTimesheet();
    const employeeId = this.isAdmin() 
      ? (this.selectedEmployeeId() || this.currentEmployeeId())
      : this.currentEmployeeId();

    if (!employeeId) {
      this.snackBar.open('Employee not found', 'Close', { duration: 3000 });
      return;
    }

    if (editing) {
      const request: UpdateTimesheetRequest = {
        hoursWorked: formValue.hoursWorked,
        notes: formValue.notes || null
      };
      this.timesheetsService.update(editing.id, request).subscribe({
        next: () => {
          this.snackBar.open('Timesheet updated successfully', 'Close', { duration: 3000 });
          this.loadTimesheets();
          this.closeDialog();
        },
        error: () => this.snackBar.open('Failed to update timesheet', 'Close', { duration: 3000 })
      });
    } else {
      const request: CreateTimesheetRequest = {
        employeeId: employeeId,
        projectId: formValue.projectId,
        date: this.formatDateForApi(new Date(formValue.date)),
        hoursWorked: formValue.hoursWorked,
        notes: formValue.notes || null
      };
      this.timesheetsService.create(request).subscribe({
        next: () => {
          this.snackBar.open('Timesheet created successfully', 'Close', { duration: 3000 });
          this.loadTimesheets();
          this.closeDialog();
        },
        error: () => this.snackBar.open('Failed to create timesheet', 'Close', { duration: 3000 })
      });
    }
  }

  deleteTimesheet(timesheet: Timesheet): void {
    if (!confirm('Are you sure you want to delete this timesheet entry?')) return;
    
    this.timesheetsService.delete(timesheet.id).subscribe({
      next: () => {
        this.snackBar.open('Timesheet deleted successfully', 'Close', { duration: 3000 });
        this.loadTimesheets();
      },
      error: () => this.snackBar.open('Failed to delete timesheet', 'Close', { duration: 3000 })
    });
  }

  get weekRangeDisplay(): string {
    const options: Intl.DateTimeFormatOptions = { month: 'short', day: 'numeric' };
    return `${this.weekStart().toLocaleDateString('en-US', options)} - ${this.weekEnd().toLocaleDateString('en-US', options)}`;
  }

  get totalHours(): number {
    return this.timesheets().reduce((sum, t) => sum + t.hoursWorked, 0);
  }
}
