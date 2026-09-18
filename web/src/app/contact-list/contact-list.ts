import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ContactsService } from '../services/contacts.service';
import { ContactListItem, ContactPage, CreateAmendmentRequest, GestorOption, ProblemDetailsResponse } from '../models/contacts.models';

export interface DayEntry {
  date: string;
  count: number;
}

export interface CorrectionFormValue {
  correctingGestorId: number | null;
  reason: string;
  channel: string;
  result: string;
  contactDate: string;
  notes: string;
}

@Component({
  selector: 'app-contact-list',
  imports: [ReactiveFormsModule],
  templateUrl: './contact-list.html',
  styleUrl: './contact-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactList {
  private readonly nfb = inject(NonNullableFormBuilder);
  private readonly contactsService = inject(ContactsService);

  readonly pageData = signal<ContactPage | null>(null);
  readonly gestors = signal<GestorOption[]>([]);
  readonly cities = signal<string[]>([]);
  readonly selected = signal<ContactListItem | null>(null);
  readonly submitting = signal(false);
  readonly corrected = signal(false);
  readonly generalError = signal<string | null>(null);

  readonly filterGestorId = signal<number | null>(null);
  readonly filterCity = signal<string>('');
  readonly selectedDay = signal<string | null>(null);
  readonly currentPage = signal(1);

  readonly channels = ['llamada', 'whatsapp', 'correo'];
  readonly results = ['contestado', 'no contesta', 'buzón', 'número equivocado', 'reagendado', 'otro'];

  readonly form = this.nfb.group({
    correctingGestorId: this.nfb.control<number | null>(null, Validators.required),
    reason: this.nfb.control(''),
    channel: this.nfb.control(''),
    result: this.nfb.control(''),
    contactDate: this.nfb.control(''),
    notes: this.nfb.control('')
  });

  readonly dayEntries = computed<DayEntry[]>(() =>
    Object.entries(this.pageData()?.dayCounts ?? {})
      .map(([date, count]) => ({ date, count }))
      .sort((a, b) => a.date.localeCompare(b.date))
  );

  readonly pageItems = computed<ContactListItem[]>(() => this.pageData()?.items ?? []);

  constructor() {
    this.contactsService.listGestors().subscribe({
      next: (gestors) => this.gestors.set(gestors)
    });

    this.contactsService.listPatients().subscribe({
      next: (patients) => this.cities.set([...new Set(patients.map((p) => p.city))].sort())
    });

    this.loadContacts();
  }

  filterByGestor(value: string | null): void {
    this.filterGestorId.set(value === null || value === '' ? null : Number(value));
    this.currentPage.set(1);
    this.loadContacts();
  }

  filterByCity(city: string): void {
    this.filterCity.set(city);
    this.currentPage.set(1);
    this.loadContacts();
  }

  selectDay(day: string | null): void {
    this.selectedDay.set(day);
    this.currentPage.set(1);
    this.loadContacts();
  }

  goToPage(target: number): void {
    const page = this.pageData();
    if (!page || target < 1 || target > page.totalPages || target === page.page) {
      return;
    }
    this.currentPage.set(target);
    this.loadContacts();
  }

  private loadContacts(): void {
    const gestorId = this.filterGestorId() ?? undefined;
    const city = this.filterCity() || undefined;
    const day = this.selectedDay() ?? undefined;

    this.contactsService.listContacts({ gestorId, city, day, page: this.currentPage() }).subscribe({
      next: (pageData) => {
        this.pageData.set(pageData);
        const currentDay = this.selectedDay();
        if (currentDay && !(currentDay in pageData.dayCounts)) {
          this.selectDay(null);
        }
      }
    });
  }

  openCorrection(contact: ContactListItem): void {
    this.generalError.set(null);
    this.form.reset({
      correctingGestorId: null,
      reason: '',
      channel: contact.channel,
      result: contact.result,
      contactDate: contact.contactDate,
      notes: contact.notes ?? ''
    });
    this.selected.set(contact);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancel(): void {
    this.selected.set(null);
    this.generalError.set(null);
  }

  save(): void {
    this.corrected.set(false);
    this.generalError.set(null);

    const contact = this.selected();
    if (!contact) {
      return;
    }

    const formValue: CorrectionFormValue = this.form.getRawValue();
    if (formValue.correctingGestorId === null) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);

    this.contactsService.createAmendment(contact.id, this.buildAmendmentRequest(contact, formValue.correctingGestorId, formValue)).subscribe({
      next: () => {
        this.submitting.set(false);
        this.corrected.set(true);
        this.cancel();
        this.loadContacts();
      },
      error: (response: HttpErrorResponse) => {
        this.submitting.set(false);
        this.applyServerErrors(response.error);
      }
    });
  }

  private buildAmendmentRequest(
    contact: ContactListItem,
    correctingGestorId: number,
    formValue: CorrectionFormValue
  ): CreateAmendmentRequest {
    const request: CreateAmendmentRequest = {
      gestorId: correctingGestorId,
      reason: formValue.reason
    };

    if (formValue.channel !== contact.channel) {
      request.channel = formValue.channel;
    }

    if (formValue.result !== contact.result) {
      request.result = formValue.result;
    }

    if (formValue.contactDate !== contact.contactDate) {
      request.contactDate = formValue.contactDate;
    }

    if (formValue.notes !== (contact.notes ?? '')) {
      request.notes = formValue.notes;
    }

    return request;
  }

  private applyServerErrors(body: ProblemDetailsResponse | null): void {
    this.clearServerErrors();

    const errors = body?.errors;
    if (!errors) {
      this.generalError.set(body?.detail ?? 'No se pudo registrar la corrección.');
      return;
    }

    let fieldMatched = false;
    for (const [field, message] of Object.entries(errors)) {
      const control = this.form.get(field);
      if (control) {
        control.setErrors({ server: message });
        control.markAsTouched();
        fieldMatched = true;
      }
    }

    if (fieldMatched) {
      return;
    }

    this.generalError.set(body?.detail ?? 'No se pudo registrar la corrección.');
  }

  private clearServerErrors(): void {
    for (const control of Object.values(this.form.controls)) {
      if (control.errors?.['server']) {
        control.setErrors(null);
      }
    }
  }
}
