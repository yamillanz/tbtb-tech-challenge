import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Contact, CreateContactRequest, GestorOption, PatientOption } from '../models/contacts.models';

@Injectable({ providedIn: 'root' })
export class ContactsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  createContact(request: CreateContactRequest): Observable<Contact> {
    return this.http.post<Contact>(`${this.baseUrl}/contacts`, request);
  }

  listPatients(): Observable<PatientOption[]> {
    return this.http.get<PatientOption[]>(`${this.baseUrl}/patients`);
  }

  listGestors(): Observable<GestorOption[]> {
    return this.http.get<GestorOption[]>(`${this.baseUrl}/gestors`);
  }
}
