import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Contact, ContactFilters, ContactPage, CreateAmendmentRequest, CreateContactRequest, GestorOption, PatientOption } from '../models/contacts.models';

@Injectable({ providedIn: 'root' })
export class ContactsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  createContact(request: CreateContactRequest): Observable<Contact> {
    return this.http.post<Contact>(`${this.baseUrl}/contacts`, request);
  }

  listContacts(filters: ContactFilters = {}): Observable<ContactPage> {
    let params = new HttpParams();
    if (filters.gestorId !== undefined) {
      params = params.set('gestorId', filters.gestorId);
    }
    if (filters.city !== undefined) {
      params = params.set('city', filters.city);
    }
    if (filters.day !== undefined) {
      params = params.set('day', filters.day);
    }
    if (filters.page !== undefined) {
      params = params.set('page', filters.page);
    }
    return this.http.get<ContactPage>(`${this.baseUrl}/contacts`, { params });
  }

  createAmendment(contactId: number, request: CreateAmendmentRequest): Observable<Contact> {
    return this.http.post<Contact>(`${this.baseUrl}/contacts/${contactId}/amendments`, request);
  }

  listPatients(): Observable<PatientOption[]> {
    return this.http.get<PatientOption[]>(`${this.baseUrl}/patients`);
  }

  listGestors(): Observable<GestorOption[]> {
    return this.http.get<GestorOption[]>(`${this.baseUrl}/gestors`);
  }
}
