# AGENTS.md - Developer Guidelines for Timesheet App

This document provides guidelines for AI agents and developers working on this codebase.

## Project Structure

```
timesheet-app/
├── frontend/                 # Angular 21 SPA (Vitest, SCSS)
│   ├── src/app/
│   │   ├── core/            # Services, guards, interceptors
│   │   ├── login/           # Login page
│   │   ├── shell/           # App shell with sidebar
│   │   ├── clients/          # Clients module
│   │   ├── projects/         # Projects module
│   │   ├── employees/        # Employees module
│   │   └── timesheets/       # Timesheets module
│   ├── proxy.conf.json       # API proxy config
│   ├── angular.json
│   └── tsconfig.json         # Strict TypeScript config
│
├── backend/                  # ASP.NET Core 10 Web API
│   ├── TimesheetApp.slnx     # Solution file
│   ├── TimesheetApp.API/     # Main API project
│   │   ├── Controllers/      # API endpoints
│   │   ├── Services/         # Business logic
│   │   ├── DTOs/             # Request/Response objects
│   │   ├── Models/           # Entity models
│   │   ├── Data/             # DbContext
│   │   └── Migrations/        # EF Core migrations
│   └── TimesheetApp.Tests/   # xUnit tests
```

---

## Build, Lint, and Test Commands

### Frontend (Angular 21 + Vitest)

```bash
cd frontend

# Install dependencies
npm install

# Development server (http://localhost:4200)
npm start

# Build for production
npm run build

# Run unit tests
npm test

# Watch mode
npm test -- --watch

# Single test file
npm test -- --include='**/login.component.spec.ts'

# Run tests with coverage
npm test -- --coverage

# Format code with Prettier
npx prettier --write src/

# Check formatting
npx prettier --check src/
```

### Backend (.NET 10)

```bash
cd backend

# Build the solution
dotnet build TimesheetApp.slnx

# Run API (http://localhost:5282)
dotnet run --project TimesheetApp.API

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~TimesheetServiceTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~TimesheetServiceTests.CreateAsync"

# Apply code style fixes
dotnet format
```

### Running Both Services

1. Start backend: `dotnet run --project backend/TimesheetApp.API`
2. Start frontend: `npm start` in frontend directory
3. Frontend proxies `/api/*` to backend via `proxy.conf.json`

---

## Code Style Guidelines

### General Principles

- **DRY**: Extract common logic into shared services/utilities
- **KISS**: Prefer simple solutions over complex ones
- **Single Responsibility**: Each component/service has a clear purpose
- **Type Safety**: Avoid `any`; use strict TypeScript (`strict: true` enabled)

---

### Angular (Frontend)

#### TypeScript Configuration
Project uses strict mode with these settings (tsconfig.json):
- `strict: true`
- `noImplicitOverride: true`
- `noPropertyAccessFromIndexSignature: true`
- `noImplicitReturns: true`
- `strictTemplates: true`

#### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Component file | kebab-case | `login.component.ts` |
| Component class | PascalCase | `LoginComponent` |
| Service | kebab-case + .service | `auth.service.ts` |
| Guard | kebab-case + .guard | `auth.guard.ts` |
| Interface | PascalCase | `Client`, `User` |
| Template | same as component | `login.component.html` |
| Styles | SCSS, same as component | `login.component.scss` |

#### Import Order

```typescript
// 1. Angular core (Component, signal, etc.)
import { Component, ChangeDetectionStrategy, signal } from '@angular/core';
// 2. Angular common (CommonModule, RouterModule, etc.)
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
// 3. Third-party (Angular Material, RxJS operators)
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { Observable, map, catchError, of } from 'rxjs';
// 4. Custom services/models
import { AuthService } from '../../core/auth.service';
import { Client } from '../../core/client.model';
```

#### Components

- Use standalone components (Angular 15+)
- Use **signals** for reactive state management
- Use **OnPush** change detection for performance
- Separate template, styles, and logic into different files

```typescript
@Component({
  selector: 'app-clients',
  standalone: true,
  imports: [CommonModule, MatTableModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './clients.component.html',
  styleUrls: ['./clients.component.scss']
})
export class ClientsComponent {
  clients = signal<Client[]>([]);
  loading = signal(false);

  constructor(private clientService: ClientsService) {}
}
```

#### Templates

- Use Angular's new control flow syntax (`@if`, `@for`, `@switch`)
- Avoid complex logic in templates; use computed signals
- Use semantic HTML with ARIA attributes

```html
@if (loading()) {
  <mat-spinner></mat-spinner>
} @else {
  <table mat-table [dataSource]="clients()">
    ...
  </table>
}

@for (client of clients(); track client.id) {
  <tr mat-row></tr>
}
```

#### HTTP & Services

- Use HttpClient with typed responses
- Always handle errors with catchError and return safe defaults

```typescript
getClients(): Observable<Client[]> {
  return this.http.get<Client[]>('/api/clients').pipe(
    catchError(err => {
      console.error('Failed to load clients', err);
      return of([]);
    })
  );
}
```

---

### ASP.NET Core (Backend)

#### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Controller | PascalCase + Controller | `ClientsController` |
| Service | PascalCase + Service | `ClientService` |
| Model | PascalCase | `Client`, `User` |
| DTO | PascalCase + Request/Response | `CreateClientRequest` |
| Record | PascalCase | `CreateClientRequest` |

#### Project Structure

```
Controllers/   # API endpoints, minimal logic
Services/      # Business logic, validation
DTOs/          # Request/Response records
Models/        # Entity definitions
Data/          # DbContext, migrations
```

#### Error Handling

- Use proper HTTP status codes (200, 201, 400, 401, 404, 500)
- Return meaningful error messages
- Use try-catch with logging

```csharp
[HttpPost]
public async Task<ActionResult<ClientResponse>> Create([FromBody] CreateClientRequest request)
{
    try
    {
        var created = await _clientService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create client");
        return StatusCode(500, "An error occurred while creating the client");
    }
}
```

#### Database Access

- Use Entity Framework Core with async methods
- Use DTOs for API requests/responses, not entity models directly
- Implement validation with data annotations or FluentValidation

#### Testing (xUnit + Moq + Testcontainers)

- Unit tests: Test services in isolation with Moq
- Integration tests: Test API with Testcontainers.PostgreSql
- Use `TestDbContextFactory`, `JwtTokenHelper` test helpers

---

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Register

### Clients (Admin only for CRUD)
- `GET /api/clients` - List all
- `GET /api/clients/{id}` - Get by ID
- `POST /api/clients` - Create
- `PUT /api/clients/{id}` - Update
- `DELETE /api/clients/{id}` - Deactivate

### Projects
- `GET /api/projects` - List all
- `GET /api/projects/{id}` - Get by ID
- `POST /api/projects` - Create (Admin)
- `PUT /api/projects/{id}` - Update (Admin)
- `DELETE /api/projects/{id}` - Delete (Admin)

### Employees
- `GET /api/employees` - List all
- `GET /api/employees/{id}` - Get by ID
- `POST /api/employees` - Create (Admin)
- `PUT /api/employees/{id}` - Update (Admin)
- `DELETE /api/employees/{id}` - Delete (Admin)

### Timesheets
- `GET /api/timesheets?employeeId=&startDate=&endDate=` - Filter by employee
- `GET /api/timesheets?projectId=&startDate=&endDate=` - Filter by project
- `POST /api/timesheets` - Create entry
- `PUT /api/timesheets/{id}` - Update entry

---

## Role-Based Access Control

| Role | Permissions |
|------|-------------|
| Admin | Full CRUD on all entities, view all timesheets, manage users |
| Employee | View projects, log/edit own timesheets |

- Angular: Use `AuthService.isAdmin()` to check permissions
- Backend: Use `[Authorize(Roles = "Admin")]` attribute

---

## Technical Notes

- Backend: .NET 10, PostgreSQL via EF Core, JWT auth, port 5282
- Frontend: Angular 21, Vitest for tests, SCSS styles, Angular Material
- JWT tokens stored in localStorage (`auth_token`)
- Employee records linked to User accounts via `EmployeeId`
- Frontend proxies `/api/*` to backend via `proxy.conf.json`
