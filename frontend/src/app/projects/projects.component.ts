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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { ProjectsService } from '../core/projects.service';
import { ClientsService } from '../core/clients.service';
import { Project, CreateProjectRequest, UpdateProjectRequest } from '../core/project.model';
import { Client } from '../core/client.model';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-projects',
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
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.css']
})
export class ProjectsComponent implements OnInit {
  projects = signal<Project[]>([]);
  clients = signal<Client[]>([]);
  loading = signal(false);
  isAdmin = signal(false);
  
  displayedColumns: string[] = ['name', 'clientName', 'startDate', 'endDate', 'isActive', 'actions'];
  
  dialogOpen = signal(false);
  editingProject = signal<Project | null>(null);
  projectForm!: FormGroup;

  constructor(
    private projectsService: ProjectsService,
    private clientsService: ClientsService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.isAdmin.set(this.authService.isAdmin());
    this.initForm();
    this.loadData();
  }

  private initForm(): void {
    this.projectForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: [''],
      startDate: [new Date(), Validators.required],
      endDate: [null],
      isActive: [true],
      clientId: ['', Validators.required]
    });
  }

  loadData(): void {
    this.loading.set(true);
    this.projectsService.getAll().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load projects', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });

    this.clientsService.getAll().subscribe({
      next: (clients) => this.clients.set(clients.filter(c => c.isActive)),
      error: () => {}
    });
  }

  openAddDialog(): void {
    this.editingProject.set(null);
    this.projectForm.reset({ 
      startDate: new Date(), 
      isActive: true,
      description: ''
    });
    this.dialogOpen.set(true);
  }

  openEditDialog(project: Project): void {
    this.editingProject.set(project);
    this.projectForm.patchValue({
      name: project.name,
      description: project.description || '',
      startDate: new Date(project.startDate),
      endDate: project.endDate ? new Date(project.endDate) : null,
      isActive: project.isActive,
      clientId: project.clientId
    });
    this.dialogOpen.set(true);
  }

  closeDialog(): void {
    this.dialogOpen.set(false);
    this.editingProject.set(null);
  }

  saveProject(): void {
    if (this.projectForm.invalid) return;

    const formValue = this.projectForm.value;
    const editing = this.editingProject();

    if (editing) {
      const request: UpdateProjectRequest = {
        name: formValue.name,
        description: formValue.description || null,
        startDate: formValue.startDate,
        endDate: formValue.endDate,
        isActive: formValue.isActive,
        clientId: formValue.clientId
      };
      this.projectsService.update(editing.id, request).subscribe({
        next: () => {
          this.snackBar.open('Project updated successfully', 'Close', { duration: 3000 });
          this.loadData();
          this.closeDialog();
        },
        error: () => this.snackBar.open('Failed to update project', 'Close', { duration: 3000 })
      });
    } else {
      const request: CreateProjectRequest = {
        name: formValue.name,
        description: formValue.description || null,
        startDate: formValue.startDate,
        endDate: formValue.endDate,
        isActive: formValue.isActive,
        clientId: formValue.clientId
      };
      this.projectsService.create(request).subscribe({
        next: () => {
          this.snackBar.open('Project created successfully', 'Close', { duration: 3000 });
          this.loadData();
          this.closeDialog();
        },
        error: () => this.snackBar.open('Failed to create project', 'Close', { duration: 3000 })
      });
    }
  }

  deleteProject(project: Project): void {
    if (!confirm(`Are you sure you want to delete "${project.name}"?`)) return;
    
    this.projectsService.delete(project.id).subscribe({
      next: () => {
        this.snackBar.open('Project deleted successfully', 'Close', { duration: 3000 });
        this.loadData();
      },
      error: () => this.snackBar.open('Failed to delete project', 'Close', { duration: 3000 })
    });
  }
}
