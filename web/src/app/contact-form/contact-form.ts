import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ContactsService } from '../services/contacts.service';
import { GestorOption, PatientOption, ProblemDetailsResponse } from '../models/contacts.models';

@Component({
  selector: 'app-contact-form',
  imports: [ReactiveFormsModule],
  templateUrl: './contact-form.html',
  styleUrl: './contact-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly nfb = inject(NonNullableFormBuilder);
  private readonly contactsService = inject(ContactsService);

  readonly patients = signal<PatientOption[]>([]);
  readonly gestors = signal<GestorOption[]>([]);
  readonly submitting = signal(false);
  readonly submitted = signal(false);
  readonly generalError = signal<string | null>(null);

  readonly channels = ['llamada', 'whatsapp', 'correo'];
  readonly results = ['contestado', 'no contesta', 'buzón', 'número equivocado', 'reagendado', 'otro'];

  readonly form = this.fb.group({
    patientId: this.fb.control<number | null>(null, Validators.required),
    gestorId: this.fb.control<number | null>(null, Validators.required),
    contactDate: this.nfb.control(today(), Validators.required),
    channel: this.nfb.control('', Validators.required),
    result: this.nfb.control('', Validators.required),
    notes: this.nfb.control('')
  });

  ngOnInit(): void {
    this.contactsService.listPatients().subscribe({
      next: (patients) => this.patients.set(patients)
    });

    this.contactsService.listGestors().subscribe({
      next: (gestors) => this.gestors.set(gestors)
    });
  }

  submit(): void {
    this.submitted.set(false);
    this.generalError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);

    this.contactsService
      .createContact({
        patientId: value.patientId ?? 0,
        gestorId: value.gestorId ?? 0,
        contactDate: value.contactDate,
        channel: value.channel,
        result: value.result,
        notes: value.notes ? value.notes : undefined
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.submitted.set(true);
          this.form.reset({ patientId: null, gestorId: null, contactDate: today(), channel: '', result: '', notes: '' });
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
    if (errors) {
      for (const [field, message] of Object.entries(errors)) {
        const control = this.form.get(field);
        control?.setErrors({ server: message });
        control?.markAsTouched();
      }
      return;
    }

    this.generalError.set(body?.detail ?? 'No se pudo registrar el contacto.');
  }

  private clearServerErrors(): void {
    for (const control of Object.values(this.form.controls)) {
      if (control.errors?.['server']) {
        control.setErrors(null);
      }
    }
  }
}

function today(): string {
  return new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString().slice(0, 10);
}
