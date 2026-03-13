import { Component, OnInit, signal, computed, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpParams } from '@angular/common/http';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
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
import { Timesheet, WeekDay, BulkSaveRequest } from './timesheet.model';
import { Employee } from '../core/employee.model';
import { Project } from '../core/project.model';
import { AuthService } from '../core/auth.service';

interface TimesheetRow {
  project: Project;
  days: { [key: string]: { hours: number; originalHours: number; notes: string | null; originalNotes: string | null } };
}

interface EditingNotes {
  rowIndex: number;
  dateStr: string;
  projectId: string;
}

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

  employees = signal<Employee[]>([]);
  projects = signal<Project[]>([]);
  loading = signal(false);
  isAdmin = signal(false);
  
  selectedEmployeeId = signal<string | null>(null);
  selectedProjectId = signal<string | null>(null);
  weekStart = signal<Date>(this.getWeekStart(new Date()));
  weekEnd = signal<Date>(this.getWeekEnd(new Date()));

  weekDays = signal<WeekDay[]>([]);
  timesheetRows = signal<TimesheetRow[]>([]);
  
  notesDialogOpen = signal(false);
  editingNotes = signal<EditingNotes | null>(null);
  notesForm = this.fb.group({
    notes: ['']
  });
  
  form!: FormGroup;
  
  currentUser = this.authService.getUser();
  private currentEmployeeId = signal<string | null>(null);

  displayedColumns = computed(() => {
    const cols = ['project'];
    this.weekDays().forEach(day => cols.push(day.dateStr));
    cols.push('total');
    return cols;
  });

  weeklyTotal = computed(() => {
    let total = 0;
    this.filteredTimesheetRows().forEach(row => {
      this.weekDays().forEach(day => {
        total += row.days[day.dateStr]?.hours || 0;
      });
    });
    return total;
  });

  filteredTimesheetRows = computed(() => {
    const projectId = this.selectedProjectId();
    if (!projectId) {
      return this.timesheetRows();
    }
    return this.timesheetRows().filter(row => row.project.id === projectId);
  });

  hasChanges = computed(() => {
    return this.timesheetRows().some(row => 
      Object.values(row.days).some(day => 
        day.hours !== day.originalHours || 
        day.notes !== day.originalNotes
      )
    );
  });

  ngOnInit(): void {
    this.isAdmin.set(this.authService.isAdmin());
    this.initForm();
    this.loadData();
  }

  private initForm(): void {
    this.form = this.fb.group({});
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

  private generateWeekDays(start: Date): WeekDay[] {
    const days: WeekDay[] = [];
    const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    for (let i = 0; i < 7; i++) {
      const d = new Date(start);
      d.setDate(d.getDate() + i);
      days.push({
        date: d,
        dateStr: this.formatDateForApi(d),
        dayName: dayNames[i],
        dayNumber: d.getDate()
      });
    }
    return days;
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
      next: (projects) => {
        this.projects.set(projects.filter(p => p.isActive));
      },
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
        this.weekDays.set(this.generateWeekDays(this.weekStart()));
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
    if (!employeeId) {
      this.loading.set(false);
      return;
    }

    const startDate = this.formatDateForApi(this.weekStart());
    const endDate = this.formatDateForApi(this.weekEnd());

    const params = new HttpParams()
      .set('employeeId', employeeId)
      .set('startDate', startDate)
      .set('endDate', endDate);

    this.timesheetsService.getWithFilters(params).subscribe({
      next: (data) => {
        this.buildTimesheetRows(data);
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load timesheets', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  private buildTimesheetRows(timesheets: Timesheet[]): void {
    const rows: TimesheetRow[] = [];
    const dayStrs = this.weekDays().map(d => d.dateStr);

    this.projects().forEach(project => {
      const row: TimesheetRow = {
        project: project,
        days: {}
      };
      
      dayStrs.forEach(dateStr => {
        const ts = timesheets.find(t => t.projectId === project.id && t.date === dateStr);
        row.days[dateStr] = {
          hours: ts?.hoursWorked || 0,
          originalHours: ts?.hoursWorked || 0,
          notes: ts?.notes || null,
          originalNotes: ts?.notes || null
        };
      });
      
      rows.push(row);
    });

    this.timesheetRows.set(rows);
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
    this.weekDays.set(this.generateWeekDays(this.weekStart()));
    this.loadTimesheets();
  }

  goToCurrentWeek(): void {
    this.weekStart.set(this.getWeekStart(new Date()));
    this.weekEnd.set(this.getWeekEnd(new Date()));
    this.weekDays.set(this.generateWeekDays(this.weekStart()));
    this.loadTimesheets();
  }

  onFilterChange(): void {
    this.loadTimesheets();
  }

  getRowTotal(row: TimesheetRow): number {
    return Object.values(row.days).reduce((sum, d) => sum + d.hours, 0);
  }

  isToday(dateStr: string): boolean {
    const today = this.formatDateForApi(new Date());
    return dateStr === today;
  }

  updateHours(rowIndex: number, dateStr: string, value: number): void {
    const rows = [...this.timesheetRows()];
    if (!rows[rowIndex].days[dateStr]) {
      rows[rowIndex].days[dateStr] = { hours: 0, originalHours: 0, notes: null, originalNotes: null };
    }
    rows[rowIndex].days[dateStr].hours = value;
    this.timesheetRows.set(rows);
  }

  openNotesDialog(rowIndex: number, dateStr: string): void {
    const row = this.timesheetRows()[rowIndex];
    this.editingNotes.set({
      rowIndex,
      dateStr,
      projectId: row.project.id
    });
    this.notesForm.patchValue({
      notes: row.days[dateStr]?.notes || ''
    });
    this.notesDialogOpen.set(true);
  }

  closeNotesDialog(): void {
    this.notesDialogOpen.set(false);
    this.editingNotes.set(null);
    this.notesForm.reset();
  }

  saveNotes(): void {
    const editing = this.editingNotes();
    if (!editing) return;

    const rows = [...this.timesheetRows()];
    const newNotes = this.notesForm.value.notes?.trim() || null;

    if (!rows[editing.rowIndex].days[editing.dateStr]) {
      rows[editing.rowIndex].days[editing.dateStr] = { hours: 0, originalHours: 0, notes: null, originalNotes: null };
    }
    rows[editing.rowIndex].days[editing.dateStr].notes = newNotes;
    this.timesheetRows.set(rows);

    this.closeNotesDialog();
  }

  hasNotes(rowIndex: number, dateStr: string): boolean {
    const row = this.timesheetRows()[rowIndex];
    return !!(row?.days[dateStr]?.notes);
  }

  getDayTotal(dateStr: string): number {
    return this.timesheetRows().reduce((sum, row) => {
      return sum + (row.days[dateStr]?.hours || 0);
    }, 0);
  }

  save(): void {
    const employeeId = this.selectedEmployeeId();
    if (!employeeId) {
      this.snackBar.open('Employee not found', 'Close', { duration: 3000 });
      return;
    }

    const entries: { projectId: string; date: string; hoursWorked: number; notes: string | null }[] = [];

    this.timesheetRows().forEach(row => {
      Object.entries(row.days).forEach(([dateStr, data]) => {
        const hoursChanged = data.hours !== data.originalHours;
        const notesChanged = data.notes !== data.originalNotes;
        
        if (data.hours > 0 && (hoursChanged || notesChanged)) {
          entries.push({
            projectId: row.project.id,
            date: dateStr,
            hoursWorked: data.hours,
            notes: data.notes
          });
        }
      });
    });

    if (entries.length === 0) {
      this.snackBar.open('No changes to save', 'Close', { duration: 3000 });
      return;
    }

    const request: BulkSaveRequest = {
      employeeId: employeeId,
      entries: entries
    };

    this.timesheetsService.bulkSave(request).subscribe({
      next: (response) => {
        if (response.errors.length > 0) {
          this.snackBar.open(`Saved with errors: ${response.errors.join(', ')}`, 'Close', { duration: 5000 });
        } else {
          this.snackBar.open('Timesheets saved successfully', 'Close', { duration: 3000 });
        }
        this.loadTimesheets();
      },
      error: () => {
        this.snackBar.open('Failed to save timesheets', 'Close', { duration: 3000 });
      }
    });
  }

  get weekRangeDisplay(): string {
    const options: Intl.DateTimeFormatOptions = { month: 'short', day: 'numeric' };
    return `${this.weekStart().toLocaleDateString('en-US', options)} - ${this.weekEnd().toLocaleDateString('en-US', options)}`;
  }
}
