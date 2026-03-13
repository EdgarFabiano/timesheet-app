import { Component, OnInit, signal } from '@angular/core';
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
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatListModule } from '@angular/material/list';
import { EmployeesService } from '../core/employees.service';
import { ProjectsService } from '../core/projects.service';
import { Project } from '../core/project.model';
import { Employee, CreateEmployeeRequest, UpdateEmployeeRequest } from '../core/employee.model';
import { AuthService } from '../core/auth.service';

interface Assignment {
  id: string;
  projectId: string;
  projectName: string;
  isActive: boolean;
}

@Component({
  selector: 'app-employees',
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
    MatSlideToggleModule,
    MatListModule
  ],
  templateUrl: './employees.component.html',
  styleUrls: ['./employees.component.css']
})
export class EmployeesComponent implements OnInit {
  employees = signal<Employee[]>([]);
  loading = signal(false);
  isAdmin = signal(false);
  
  displayedColumns: string[] = ['fullName', 'email', 'department', 'isActive', 'assignmentsCount', 'createdAt', 'actions'];
  
  dialogOpen = signal(false);
  editingEmployee = signal<Employee | null>(null);
  employeeForm!: FormGroup;

  projects = signal<Project[]>([]);
  selectedProjectId = signal<string | null>(null);
  assignments = signal<Assignment[]>([]);
  assignmentsLoading = signal(false);

  constructor(
    private employeesService: EmployeesService,
    private projectsService: ProjectsService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.isAdmin.set(this.authService.isAdmin());
    this.initForm();
    this.loadEmployees();
  }

  private initForm(): void {
    this.employeeForm = this.fb.group({
      azureAdObjectId: ['', [Validators.required, Validators.maxLength(256)]],
      fullName: ['', [Validators.required, Validators.maxLength(200)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(320)]],
      department: ['', [Validators.required, Validators.maxLength(100)]],
      isActive: [true]
    });
  }

  loadEmployees(): void {
    this.loading.set(true);
    this.employeesService.getAll().subscribe({
      next: (data) => {
        this.employees.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load employees', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  openAddDialog(): void {
    this.editingEmployee.set(null);
    this.employeeForm.reset({ isActive: true });
    this.dialogOpen.set(true);
  }

  openEditDialog(employee: Employee): void {
    this.editingEmployee.set(employee);
    this.employeeForm.patchValue({
      azureAdObjectId: employee.azureAdObjectId,
      fullName: employee.fullName,
      email: employee.email,
      department: employee.department,
      isActive: employee.isActive
    });
    this.loadProjects();
    this.loadAssignments(employee.id);
    this.dialogOpen.set(true);
  }

  private loadProjects(): void {
    this.projectsService.getAll().subscribe({
      next: (data) => this.projects.set(data.filter(p => p.isActive))
    });
  }

  private loadAssignments(employeeId: string): void {
    this.assignmentsLoading.set(true);
    this.employeesService.getAssignments(employeeId).subscribe({
      next: (data) => {
        this.assignments.set(data);
        this.assignmentsLoading.set(false);
      },
      error: () => {
        this.assignmentsLoading.set(false);
        this.snackBar.open('Failed to load assignments', 'Close', { duration: 3000 });
      }
    });
  }

  closeDialog(): void {
    this.dialogOpen.set(false);
    this.editingEmployee.set(null);
    this.loadEmployees();
  }

  saveEmployee(): void {
    if (this.employeeForm.invalid) return;

    const formValue = this.employeeForm.value;
    const editing = this.editingEmployee();

    if (editing) {
      const request: UpdateEmployeeRequest = {
        azureAdObjectId: formValue.azureAdObjectId,
        fullName: formValue.fullName,
        email: formValue.email,
        department: formValue.department,
        isActive: formValue.isActive
      };
      this.employeesService.update(editing.id, request).subscribe({
        next: () => {
          this.snackBar.open('Employee updated successfully', 'Close', { duration: 3000 });
          this.loadEmployees();
          this.closeDialog();
        },
        error: () => this.snackBar.open('Failed to update employee', 'Close', { duration: 3000 })
      });
    } else {
      const request: CreateEmployeeRequest = {
        azureAdObjectId: formValue.azureAdObjectId,
        fullName: formValue.fullName,
        email: formValue.email,
        department: formValue.department,
        isActive: formValue.isActive
      };
      this.employeesService.create(request).subscribe({
        next: () => {
          this.snackBar.open('Employee created successfully', 'Close', { duration: 3000 });
          this.loadEmployees();
          this.closeDialog();
        },
        error: () => this.snackBar.open('Failed to create employee', 'Close', { duration: 3000 })
      });
    }
  }

  deleteEmployee(employee: Employee): void {
    if (!confirm(`Are you sure you want to delete "${employee.fullName}"?`)) return;
    
    this.employeesService.delete(employee.id).subscribe({
      next: () => {
        this.snackBar.open('Employee deleted successfully', 'Close', { duration: 3000 });
        this.loadEmployees();
      },
      error: () => this.snackBar.open('Failed to delete employee', 'Close', { duration: 3000 })
    });
  }

  assignProject(): void {
    const projectId = this.selectedProjectId();
    const employee = this.editingEmployee();
    if (!projectId || !employee) return;

    this.employeesService.assignProject(employee.id, projectId).subscribe({
      next: () => {
        this.snackBar.open('Project assigned successfully', 'Close', { duration: 3000 });
        this.loadAssignments(employee.id);
        this.loadEmployees();
        this.selectedProjectId.set(null);
      },
      error: (err) => {
        if (err.status === 409) {
          this.snackBar.open('Employee is already assigned to this project', 'Close', { duration: 3000 });
        } else {
          this.snackBar.open('Failed to assign project', 'Close', { duration: 3000 });
        }
      }
    });
  }

  removeAssignment(assignment: Assignment): void {
    if (!confirm(`Remove assignment to "${assignment.projectName}"?`)) return;

    this.employeesService.removeAssignment(assignment.id).subscribe({
      next: () => {
        this.snackBar.open('Assignment removed successfully', 'Close', { duration: 3000 });
        const employee = this.editingEmployee();
        if (employee) {
          this.loadAssignments(employee.id);
          this.loadEmployees();
        }
      },
      error: () => this.snackBar.open('Failed to remove assignment', 'Close', { duration: 3000 })
    });
  }
}
