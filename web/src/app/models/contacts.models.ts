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
