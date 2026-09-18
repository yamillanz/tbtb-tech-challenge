import { HttpErrorResponse } from '@angular/common/http';
import { fireEvent, render, screen, within } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { of, throwError } from 'rxjs';
import { ContactForm } from './contact-form';
import { ContactsService } from '../services/contacts.service';
import { Contact, CreateContactRequest, GestorOption, PatientOption } from '../models/contacts.models';

const pacientes: PatientOption[] = [
  { id: 1, name: 'Carlos Andrés Mendoza', documentNumber: 'CC-2', city: 'Medellín' },
  { id: 2, name: 'María Fernanda Rojas', documentNumber: 'CC-1', city: 'Bogotá' }
];

const gestores: GestorOption[] = [
  { id: 2, name: 'Andrés Peña' },
  { id: 1, name: 'Laura Gómez' }
];

const contactoCreado: Contact = {
  id: 9,
  patientId: 2,
  patientName: 'María Fernanda Rojas',
  gestorId: 1,
  gestorName: 'Laura Gómez',
  contactDate: '2026-09-18',
  channel: 'whatsapp',
  result: 'contestado',
  notes: 'Contacto de demostración'
};

function hoy(): string {
  return new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString().slice(0, 10);
}

function createFakeService(overrides: Partial<ContactsService> = {}): ContactsService {
  return {
    listPatients: () => of(pacientes),
    listGestors: () => of(gestores),
    createContact: () => of(contactoCreado),
    ...overrides
  } as unknown as ContactsService;
}

async function renderFormWith(serviceDouble: ContactsService): Promise<void> {
  await render(ContactForm, {
    providers: [{ provide: ContactsService, useValue: serviceDouble }]
  });
}

describe('ContactForm', () => {
  it('RenderizadoDelFormulario_CuandoSeAbre_MuestraLosPacientesYGestoresDelCatalogo', async () => {
    await renderFormWith(createFakeService());

    expect(await screen.findByRole('option', { name: /María Fernanda Rojas/ })).toBeTruthy();
    expect(screen.getByRole('option', { name: /Carlos Andrés Mendoza/ })).toBeTruthy();
    expect(screen.getByRole('option', { name: /Laura Gómez/ })).toBeTruthy();
    expect(screen.getByRole('option', { name: /Andrés Peña/ })).toBeTruthy();

    const opcionesDePaciente = within(screen.getByLabelText('Paciente')).getAllByRole('option');
    expect(opcionesDePaciente[1].textContent).toContain('Carlos');

    const opcionesDeGestor = within(screen.getByLabelText('Gestor')).getAllByRole('option');
    expect(opcionesDeGestor[1].textContent).toContain('Andrés');
  });

  it('EnvioDelFormulario_CuandoFaltaUnCampoObligatorio_MuestraElMensajeJuntoAlCampo', async () => {
    const createContact = jasmine.createSpy('createContact').and.throwError('no debe invocarse');
    await renderFormWith(createFakeService({ createContact }));

    const user = userEvent.setup();
    await user.click(screen.getByRole('button', { name: 'Registrar contacto' }));

    expect(screen.getByText('Selecciona el paciente.')).toBeTruthy();
    expect(screen.getByText('Selecciona el gestor.')).toBeTruthy();
    expect(screen.getByText('Selecciona el canal.')).toBeTruthy();
    expect(screen.getByText('Selecciona el resultado.')).toBeTruthy();
    expect(createContact.calls.count()).toBe(0);
  });

  it('EnvioDelFormulario_CuandoSeCompletaTodo_InvocaAlServicioYMuestraExito', async () => {
    const createContact = jasmine.createSpy('createContact').and.returnValue(of(contactoCreado));
    await renderFormWith(createFakeService({ createContact }));

    const user = userEvent.setup();
    await user.selectOptions(screen.getByLabelText('Paciente'), screen.getByRole('option', { name: /María Fernanda Rojas/ }));
    await user.selectOptions(screen.getByLabelText('Gestor'), screen.getByRole('option', { name: /Laura Gómez/ }));
    await user.selectOptions(screen.getByLabelText('Canal'), screen.getByRole('option', { name: 'whatsapp' }));
    await user.selectOptions(screen.getByLabelText('Resultado'), screen.getByRole('option', { name: 'contestado' }));
    await user.type(screen.getByLabelText('Notas (opcional)'), 'Contacto de demostración');
    await user.click(screen.getByRole('button', { name: 'Registrar contacto' }));

    const payloadEsperado: CreateContactRequest = {
      patientId: 2,
      gestorId: 1,
      contactDate: hoy(),
      channel: 'whatsapp',
      result: 'contestado',
      notes: 'Contacto de demostración'
    };
    expect(createContact).toHaveBeenCalledWith(payloadEsperado);

    expect((await screen.findByRole('status')).textContent).toContain('El contacto quedó registrado.');
    expect((screen.getByLabelText('Notas (opcional)') as HTMLTextAreaElement).value).toBe('');
  });

  it('EnvioDelFormulario_CuandoElServidorResponde400_MuestraElErrorJuntoAlCampo', async () => {
    const detalle = 'La fecha del contacto debe estar dentro del mes en curso (2026-09-01 a 2026-09-30).';
    const createContact = jasmine.createSpy('createContact').and.callFake(() =>
      throwError(() => new HttpErrorResponse({
        status: 400,
        error: { title: 'Solicitud inválida', status: 400, detail: detalle, errors: { contactDate: detalle } }
      }))
    );
    await renderFormWith(createFakeService({ createContact }));

    const user = userEvent.setup();
    await user.selectOptions(screen.getByLabelText('Paciente'), screen.getByRole('option', { name: /María Fernanda Rojas/ }));
    await user.selectOptions(screen.getByLabelText('Gestor'), screen.getByRole('option', { name: /Laura Gómez/ }));
    await user.selectOptions(screen.getByLabelText('Canal'), screen.getByRole('option', { name: 'whatsapp' }));
    await user.selectOptions(screen.getByLabelText('Resultado'), screen.getByRole('option', { name: 'contestado' }));
    fireEvent.input(screen.getByLabelText('Fecha del contacto'), { target: { value: '2026-08-15' } });
    await user.click(screen.getByRole('button', { name: 'Registrar contacto' }));

    expect(await screen.findByText(detalle)).toBeTruthy();
    const seleccionDePaciente = screen.getByLabelText('Paciente') as HTMLSelectElement;
    expect(seleccionDePaciente.selectedIndex).toBeGreaterThan(0);
  });

  it('EnvioDelFormulario_CuandoElServidorResponde404_MuestraLaAlertaGeneral', async () => {
    const createContact = jasmine.createSpy('createContact').and.callFake(() =>
      throwError(() => new HttpErrorResponse({
        status: 404,
        error: { title: 'Recurso no encontrado', status: 404, detail: 'El paciente 999 no existe.' }
      }))
    );
    await renderFormWith(createFakeService({ createContact }));

    const user = userEvent.setup();
    await user.selectOptions(screen.getByLabelText('Paciente'), screen.getByRole('option', { name: /María Fernanda Rojas/ }));
    await user.selectOptions(screen.getByLabelText('Gestor'), screen.getByRole('option', { name: /Laura Gómez/ }));
    await user.selectOptions(screen.getByLabelText('Canal'), screen.getByRole('option', { name: 'whatsapp' }));
    await user.selectOptions(screen.getByLabelText('Resultado'), screen.getByRole('option', { name: 'contestado' }));
    await user.click(screen.getByRole('button', { name: 'Registrar contacto' }));

    expect(await screen.findByRole('alert')).toBeTruthy();
    expect(screen.getByRole('alert').textContent).toContain('El paciente 999 no existe.');
    expect(screen.queryByText('No se pudo registrar el contacto.')).toBeNull();
  });
});
