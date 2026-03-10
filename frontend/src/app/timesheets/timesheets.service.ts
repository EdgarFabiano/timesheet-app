import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { Timesheet, CreateTimesheetRequest, UpdateTimesheetRequest } from '../timesheets/timesheet.model';

@Injectable({ providedIn: 'root' })
export class TimesheetsService {
  private readonly apiUrl = '/api/timesheets';

  constructor(private http: HttpClient) {}

  getByEmployeeAndDateRange(employeeId: string, startDate: string, endDate: string): Observable<Timesheet[]> {
    const params = new HttpParams()
      .set('employeeId', employeeId)
      .set('startDate', startDate)
      .set('endDate', endDate);
    
    return this.http.get<Timesheet[]>(this.apiUrl, { params }).pipe(
      catchError(this.handleError)
    );
  }

  getByProject(projectId: string, startDate?: string, endDate?: string): Observable<Timesheet[]> {
    let params = new HttpParams().set('projectId', projectId);
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    
    return this.http.get<Timesheet[]>(this.apiUrl, { params }).pipe(
      catchError(this.handleError)
    );
  }

  getById(id: string): Observable<Timesheet | null> {
    return this.http.get<Timesheet>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleErrorById)
    );
  }

  create(request: CreateTimesheetRequest): Observable<Timesheet> {
    return this.http.post<Timesheet>(this.apiUrl, request);
  }

  update(id: string, request: UpdateTimesheetRequest): Observable<Timesheet> {
    return this.http.put<Timesheet>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private handleError(error: HttpErrorResponse): Observable<Timesheet[]> {
    console.error('Timesheets API error:', error);
    return of([]);
  }

  private handleErrorById(error: HttpErrorResponse): Observable<Timesheet | null> {
    console.error('Timesheets API error:', error);
    return of(null);
  }
}
