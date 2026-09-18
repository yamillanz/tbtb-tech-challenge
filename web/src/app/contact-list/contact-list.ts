import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ContactsService } from '../services/contacts.service';
import { ContactListItem, CreateAmendmentRequest, GestorOption, ProblemDetailsResponse } from '../models/contacts.models';

export interface DayEntry {
  date: string;
  count: number;
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

  readonly contacts = signal<ContactListItem[]>([]);
  readonly gestors = signal<GestorOption[]>([]);
  readonly cities = signal<string[]>([]);
  readonly selected = signal<ContactListItem | null>(null);
  readonly submitting = signal(false);
  readonly corrected = signal(false);
  readonly generalError = signal<string | null>(null);

  readonly filterGestorId = signal<number | null>(null);
  readonly filterCity = signal<string>('');
  readonly selectedDay = signal<string | null>(null);

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

  readonly days = signal<DayEntry[]>([]);

  readonly visibleContacts = signal<ContactListItem[]>([]);

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
    this.loadContacts();
  }

  filterByCity(city: string): void {
    this.filterCity.set(city);
    this.loadContacts();
  }

  selectDay(day: string | null): void {
    this.selectedDay.set(day);
    this.refreshVisibleContacts();
  }

  private loadContacts(): void {
    const gestorId = this.filterGestorId() ?? undefined;
    const city = this.filterCity() || undefined;

    this.contactsService.listContacts({ gestorId, city }).subscribe({
      next: (contacts) => {
        this.contacts.set(contacts);
        this.rebuildDayStrip(contacts);
        this.refreshVisibleContacts();
      }
    });
  }

  private rebuildDayStrip(contacts: ContactListItem[]): void {
    const conteos = new Map<string, number>();
    for (const contact of contacts) {
      conteos.set(contact.contactDate, (conteos.get(contact.contactDate) ?? 0) + 1);
    }

    this.days.set(
      [...conteos.entries()]
        .map(([date, count]) => ({ date, count }))
        .sort((a, b) => a.date.localeCompare(b.date))
    );

    if (this.selectedDay() && !conteos.has(this.selectedDay()!)) {
      this.selectedDay.set(null);
    }
    this.refreshVisibleContacts();
  }

  private refreshVisibleContacts(): void {
    const day = this.selectedDay();
    const list = day ? this.contacts().filter((c) => c.contactDate === day) : this.contacts();
    this.visibleContacts.set(list);
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

    const value = this.form.getRawValue();
    if (value.correctingGestorId === null) {
      this.form.markAllAsTouched();
      return;
    }

    const request: CreateAmendmentRequest = {
      gestorId: value.correctingGestorId,
      reason: value.reason
    };

    if (value.channel !== contact.channel) {
      request.channel = value.channel;
    }
    if (value.result !== contact.result) {
      request.result = value.result;
    }
    if (value.contactDate !== contact.contactDate) {
      request.contactDate = value.contactDate;
    }
    if (value.notes !== (contact.notes ?? '')) {
      request.notes = value.notes;
    }

    this.submitting.set(true);

    this.contactsService.createAmendment(contact.id, request).subscribe({
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
