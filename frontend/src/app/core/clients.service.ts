import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { Client, CreateClientRequest, UpdateClientRequest } from './client.model';

@Injectable({ providedIn: 'root' })
export class ClientsService {
  private readonly apiUrl = '/api/clients';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Client[]> {
    return this.http.get<Client[]>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }

  getById(id: string): Observable<Client | null> {
    return this.http.get<Client>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleErrorById)
    );
  }

  create(request: CreateClientRequest): Observable<Client> {
    return this.http.post<Client>(this.apiUrl, request);
  }

  update(id: string, request: UpdateClientRequest): Observable<Client> {
    return this.http.put<Client>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private handleError(error: HttpErrorResponse): Observable<Client[]> {
    console.error('Clients API error:', error);
    return of([]);
  }

  private handleErrorById(error: HttpErrorResponse): Observable<Client | null> {
    console.error('Clients API error:', error);
    return of(null);
  }
}
