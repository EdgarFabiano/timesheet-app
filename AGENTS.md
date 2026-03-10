# AGENTS.md - Developer Guidelines for Timesheet App

This document provides guidelines for AI agents and developers working on this codebase.

## Project Structure

```
timesheet-app/
├── frontend/                 # Angular 21 SPA
│   ├── src/
│   │   └── app/
│   │       ├── core/        # Services, guards, interceptors
│   │       ├── login/       # Login page
│   │       ├── shell/       # App shell with sidebar
│   │       ├── clients/     # Clients module
│   │       ├── projects/    # Projects module
│   │       ├── employees/   # Employees module
│   │       └── timesheets/  # Timesheets module
│   ├── proxy.conf.json      # API proxy config
│   └── angular.json
│
├── backend/                  # ASP.NET Core Web API
│   ├── TimesheetApp.API/    # Main API project
│   │   ├── Controllers/     # API endpoints
│   │   ├── Services/        # Business logic
│   │   ├── DTOs/            # Data transfer objects
│   │   ├── Models/          # Entity models
│   │   ├── Data/            # DbContext
│   │   └── Migrations/      # EF Core migrations
│   └── TimesheetApp.Tests/  # Unit & integration tests
```

---

## Build, Lint, and Test Commands

### Frontend (Angular)

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Start development server (http://localhost:4200)
npm start
# or: ng serve

# Start with specific port
ng serve --port 4201

# Build for production
npm run build

# Run unit tests
npm test
# or: ng test

# Run tests in watch mode
ng test --watch

# Run a single test file
ng test --include='**/login.component.spec.ts'

# Format code with Prettier
npx prettier --write src/

# Check formatting
npx prettier --check src/
```

### Backend (.NET)

```bash
# Navigate to backend directory
cd backend

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run API (http://localhost:5282)
dotnet run --project TimesheetApp.API

# Run tests
dotnet test

# Run a specific test class
dotnet test --filter "FullyQualifiedName~TimesheetServiceTests"

# Run a specific test method
dotnet test --filter "FullyQualifiedName~TimesheetServiceTests.CreateAsync"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Apply code style fixes
dotnet format
```

### Running Both Services

1. Start backend: `dotnet run --project backend/TimesheetApp.API` (port 5282)
2. Start frontend: `npm start` (port 4200)
3. Frontend proxies `/api/*` requests to backend via `proxy.conf.json`

---

## Code Style Guidelines

### General Principles

- **DRY (Don't Repeat Yourself)**: Extract common logic into shared services/utilities
- **KISS (Keep It Simple)**: Prefer simple solutions over complex ones
- **Single Responsibility**: Each component/service should have a clear purpose
- **Type Safety**: Avoid `any` unless absolutely necessary; use proper types

### Angular (Frontend)

#### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Component | kebab-case | `login.component.ts` |
| Component class | PascalCase | `LoginComponent` |
| Service | kebab-case + .service | `auth.service.ts` |
| Guard | kebab-case + .guard | `auth.guard.ts` |
| Interface | PascalCase | `Client`, `User` |
| Template | same as component | `login.component.html` |
| Styles | same as component | `login.component.css` |

#### Imports

```typescript
// Group imports in this order:
1. Angular core/modules (Component, Injectable, etc.)
2. Angular common modules (CommonModule, RouterModule, etc.)
3. Third-party libraries (Angular Material, RxJS)
4. Custom services/models
5. Relative imports from same feature

// Good:
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { Observable, map } from 'rxjs';
import { AuthService } from '../../core/auth.service';
import { Client } from '../../core/client.model';
```

#### Components

- Use standalone components (Angular 15+)
- Use signals for reactive state management
- Separate template, styles, and logic into different files
- Use `OnPush` change detection for performance

```typescript
@Component({
  selector: 'app-clients',
  standalone: true,
  imports: [CommonModule, MatTableModule, ...],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './clients.component.html',
  styleUrls: ['./clients.component.css']
})
export class ClientsComponent {
  // Use signals for state
  clients = signal<Client[]>([]);
  loading = signal(false);

  // Use dependency injection
  constructor(private clientService: ClientsService) {}
}
```

#### Templates

- Use Angular's new control flow syntax (`@if`, `@for`, `@switch`)
- Avoid complex logic in templates; move to component
- Use semantic HTML and ARIA attributes for accessibility

```html
<!-- Good -->
@if (loading()) {
  <mat-spinner></mat-spinner>
} @else {
  <table mat-table [dataSource]="clients()">
    ...
  </table>
}
```

#### HTTP & Services

- Use HttpClient with typed responses
- Always handle errors with proper error messages
- Use interceptors for auth tokens, logging, etc.

```typescript
getClients(): Observable<Client[]> {
  return this.http.get<Client[]>('/api/clients').pipe(
    catchError(err => {
      console.error('Failed to load clients', err);
      return of([]); // Return empty array on error
    })
  );
}
```

### ASP.NET Core (Backend)

#### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Controller | PascalCase + Controller | `ClientsController` |
| Service | PascalCase + Service | `ClientService` |
| Model | PascalCase | `Client`, `User` |
| DTO | PascalCase + Request/Response | `CreateClientRequest`, `ClientResponse` |
| Record | PascalCase | `CreateClientRequest` |

#### Project Structure

```
Controllers/     # API endpoints, minimal logic
Services/       # Business logic, validation
DTOs/           # Request/Response objects
Models/         # Entity definitions
Data/           # DbContext, migrations
```

#### Error Handling

- Use proper HTTP status codes (200 OK, 201 Created, 400 Bad Request, 401 Unauthorized, 404 Not Found, 500 Internal Server Error)
- Return meaningful error messages
- Use try-catch with proper logging

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
- Implement proper validation

#### Testing

- Unit tests: Test individual services in isolation
- Integration tests: Test API endpoints with real HTTP calls
- Use the existing test helpers (`TestDbContextFactory`, `JwtTokenHelper`)

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
- `GET /api/timesheets?employeeId=&startDate=&endDate=` - Filter by employee + date range
- `GET /api/timesheets?projectId=&startDate=&endDate=` - Filter by project
- `POST /api/timesheets` - Create entry
- `PUT /api/timesheets/{id}` - Update entry

---

## Role-Based Access Control

| Role | Permissions |
|------|-------------|
| Admin | Full CRUD on all entities, view all timesheets, manage users |
| Employee | View projects, log/edit own timesheets |

- Use `AuthService.isAdmin()` to check permissions in Angular
- Backend uses `[Authorize(Roles = "Admin")]` attribute

---

## Additional Notes

- Backend runs on port 5282 (configured in `launchSettings.json`)
- Frontend proxies API calls via `proxy.conf.json`
- Use Angular Material for UI components
- JWT tokens are stored in localStorage (`auth_token`)
- Employee records are linked to User accounts via `EmployeeId`
