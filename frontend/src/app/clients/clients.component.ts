import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ClientsService } from '../core/clients.service';
import { Client, CreateClientRequest, UpdateClientRequest } from '../core/client.model';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-clients',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    MatSlideToggleModule
  ],
  templateUrl: './clients.component.html',
  styleUrls: ['./clients.component.css']
})
export class ClientsComponent implements OnInit {
  clients = signal<Client[]>([]);
  loading = signal(false);
  isAdmin = signal(false);
  
  displayedColumns: string[] = ['name', 'contactEmail', 'isActive', 'createdAt', 'actions'];
  
  dialogOpen = signal(false);
  editingClient = signal<Client | null>(null);
  clientForm!: FormGroup;

  constructor(
    private clientsService: ClientsService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.isAdmin.set(this.authService.isAdmin());
    this.initForm();
    this.loadClients();
  }

  private initForm(): void {
    this.clientForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      contactEmail: ['', [Validators.required, Validators.email, Validators.maxLength(320)]],
      isActive: [true]
    });
  }

  loadClients(): void {
    this.loading.set(true);
    this.clientsService.getAll().subscribe({
      next: (data) => {
        this.clients.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load clients', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  openAddDialog(): void {
    this.editingClient.set(null);
    this.clientForm.reset({ isActive: true });
    this.dialogOpen.set(true);
  }

  openEditDialog(client: Client): void {
    this.editingClient.set(client);
    this.clientForm.patchValue({
      name: client.name,
      contactEmail: client.contactEmail,
      isActive: client.isActive
    });
    this.dialogOpen.set(true);
  }

  closeDialog(): void {
    this.dialogOpen.set(false);
    this.editingClient.set(null);
  }

  saveClient(): void {
    if (this.clientForm.invalid) return;

    const formValue = this.clientForm.value;
    const editing = this.editingClient();

    if (editing) {
      const request: UpdateClientRequest = {
        name: formValue.name,
        contactEmail: formValue.contactEmail,
        isActive: formValue.isActive
      };
      this.clientsService.update(editing.id, request).subscribe({
        next: () => {
          this.snackBar.open('Client updated successfully', 'Close', { duration: 3000 });
          this.loadClients();
          this.closeDialog();
        },
        error: () => this.snackBar.open('Failed to update client', 'Close', { duration: 3000 })
      });
    } else {
      const request: CreateClientRequest = {
        name: formValue.name,
        contactEmail: formValue.contactEmail,
        isActive: formValue.isActive
      };
      this.clientsService.create(request).subscribe({
        next: () => {
          this.snackBar.open('Client created successfully', 'Close', { duration: 3000 });
          this.loadClients();
          this.closeDialog();
        },
        error: () => this.snackBar.open('Failed to create client', 'Close', { duration: 3000 })
      });
    }
  }

  deleteClient(client: Client): void {
    if (!confirm(`Are you sure you want to deactivate "${client.name}"?`)) return;
    
    this.clientsService.delete(client.id).subscribe({
      next: () => {
        this.snackBar.open('Client deactivated successfully', 'Close', { duration: 3000 });
        this.loadClients();
      },
      error: () => this.snackBar.open('Failed to deactivate client', 'Close', { duration: 3000 })
    });
  }
}
