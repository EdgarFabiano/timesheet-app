import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { Employee, CreateEmployeeRequest, UpdateEmployeeRequest } from './employee.model';

interface AssignmentResponse {
  id: string;
  employeeId: string;
  employeeName: string;
  projectId: string;
  projectName: string;
  assignedAt: Date;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class EmployeesService {
  private readonly apiUrl = '/api/employees';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Employee[]> {
    return this.http.get<Employee[]>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }

  getById(id: string): Observable<Employee | null> {
    return this.http.get<Employee>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleErrorById)
    );
  }

  create(request: CreateEmployeeRequest): Observable<Employee> {
    return this.http.post<Employee>(this.apiUrl, request);
  }

  update(id: string, request: UpdateEmployeeRequest): Observable<Employee> {
    return this.http.put<Employee>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getAssignments(employeeId: string): Observable<AssignmentResponse[]> {
    return this.http.get<AssignmentResponse[]>(`/api/assignments?employeeId=${employeeId}`).pipe(
      catchError(this.handleErrorAssignments)
    );
  }

  assignProject(employeeId: string, projectId: string): Observable<AssignmentResponse> {
    return this.http.post<AssignmentResponse>('/api/assignments', {
      employeeId,
      projectId,
      isActive: true
    });
  }

  removeAssignment(assignmentId: string): Observable<void> {
    return this.http.delete<void>(`/api/assignments/${assignmentId}`);
  }

  private handleError(error: HttpErrorResponse): Observable<Employee[]> {
    console.error('Employees API error:', error);
    return of([]);
  }

  private handleErrorById(error: HttpErrorResponse): Observable<Employee | null> {
    console.error('Employees API error:', error);
    return of(null);
  }

  private handleErrorAssignments(error: HttpErrorResponse): Observable<AssignmentResponse[]> {
    console.error('Assignments API error:', error);
    return of([]);
  }
}
