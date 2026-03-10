import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { Project, CreateProjectRequest, UpdateProjectRequest } from './project.model';

@Injectable({ providedIn: 'root' })
export class ProjectsService {
  private readonly apiUrl = '/api/projects';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Project[]> {
    return this.http.get<Project[]>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }

  getById(id: string): Observable<Project | null> {
    return this.http.get<Project>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleErrorById)
    );
  }

  create(request: CreateProjectRequest): Observable<Project> {
    return this.http.post<Project>(this.apiUrl, request);
  }

  update(id: string, request: UpdateProjectRequest): Observable<Project> {
    return this.http.put<Project>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private handleError(error: HttpErrorResponse): Observable<Project[]> {
    console.error('Projects API error:', error);
    return of([]);
  }

  private handleErrorById(error: HttpErrorResponse): Observable<Project | null> {
    console.error('Projects API error:', error);
    return of(null);
  }
}
