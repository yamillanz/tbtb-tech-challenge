export interface CreateContactRequest {
  patientId: number;
  gestorId: number;
  contactDate: string;
  channel: string;
  result: string;
  notes?: string;
}

export interface Contact {
  id: number;
  patientId: number;
  patientName: string;
  gestorId: number;
  gestorName: string;
  contactDate: string;
  channel: string;
  result: string;
  notes?: string;
}

export interface PatientOption {
  id: number;
  name: string;
  documentNumber: string;
  city: string;
}

export interface GestorOption {
  id: number;
  name: string;
}

export interface ProblemDetailsResponse {
  title?: string;
  detail?: string;
  errors?: Record<string, string>;
}

export interface ContactListItem {
  id: number;
  patientName: string;
  gestorName: string;
  contactDate: string;
  channel: string;
  result: string;
  notes?: string;
}

export interface CreateAmendmentRequest {
  gestorId: number;
  reason: string;
  channel?: string;
  result?: string;
  contactDate?: string;
  notes?: string;
}

export interface Amendment {
  id: number;
  contactId: number;
  reason: string;
  amendedBy: string;
  amendedAt: string;
  oldChannel?: string;
  newChannel?: string;
  oldResult?: string;
  newResult?: string;
  oldContactDate?: string;
  newContactDate?: string;
  oldNotes?: string;
  newNotes?: string;
}
