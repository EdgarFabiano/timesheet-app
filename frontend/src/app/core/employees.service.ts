import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { Employee, CreateEmployeeRequest, UpdateEmployeeRequest } from './employee.model';

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

  private handleError(error: HttpErrorResponse): Observable<Employee[]> {
    console.error('Employees API error:', error);
    return of([]);
  }

  private handleErrorById(error: HttpErrorResponse): Observable<Employee | null> {
    console.error('Employees API error:', error);
    return of(null);
  }
}
