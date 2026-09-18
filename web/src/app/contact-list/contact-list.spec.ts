import { HttpErrorResponse } from '@angular/common/http';
import { render, screen, waitFor, within } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { of, throwError } from 'rxjs';
import { ContactList } from './contact-list';
import { ContactsService } from '../services/contacts.service';
import { Contact, ContactListItem } from '../models/contacts.models';

const contactos: ContactListItem[] = [
  {
    id: 95,
    patientName: 'Ana Lucía Ortega',
    gestorName: 'Laura Gómez',
    contactDate: '2026-09-02',
    channel: 'llamada',
    result: 'buzón',
    notes: 'Se marcó por error; el paciente respondió'
  },
  {
    id: 96,
    patientName: 'Diego Alejandro Suárez',
    gestorName: 'Andrés Peña',
    contactDate: '2026-09-03',
    channel: 'whatsapp',
    result: 'contestado'
  }
];

const contactoVigente: ContactListItem = { ...contactos[0], result: 'contestado' };

function createFakeService(overrides: Partial<ContactsService> = {}): ContactsService {
  return {
    listContacts: () => of(contactos),
    createAmendment: () => of({ ...contactoVigente, id: 95, patientName: 'Ana Lucía Ortega', gestorName: 'Laura Gómez' }),
    listGestors: () => of([{ id: 19, name: 'Andrés Peña' }]),
    listPatients: () => of([]),
    createContact: () => throwError(() => new Error('no debe invocarse')),
    ...overrides
  } as unknown as ContactsService;
}

async function renderContactListWith(serviceDouble: ContactsService): Promise<void> {
  await render(ContactList, {
    providers: [{ provide: ContactsService, useValue: serviceDouble }]
  });
}

describe('ContactList', () => {
  it('RenderizadoDeLaLista_CuandoSeAbre_MuestraLosContactosDelMesConLosValoresVigentes', async () => {
    await renderContactListWith(createFakeService());

    expect(await screen.findByRole('cell', { name: 'Ana Lucía Ortega' })).toBeTruthy();
    expect(screen.getByRole('cell', { name: 'buzón' })).toBeTruthy();
    expect(screen.getByRole('cell', { name: 'Diego Alejandro Suárez' })).toBeTruthy();
  });

  it('CorreccionDesdeLaPantalla_CuandoFaltaElMotivo_MuestraElErrorJuntoAlCampo', async () => {
    const detalle = 'El motivo de la corrección es obligatorio.';
    const createAmendment = jasmine.createSpy('createAmendment').and.callFake(() =>
      throwError(() => new HttpErrorResponse({
        status: 400,
        error: { title: 'Solicitud inválida', status: 400, detail: detalle, errors: { reason: detalle } }
      }))
    );
    await renderContactListWith(createFakeService({ createAmendment }));

    const user = userEvent.setup();
    await user.click(await screen.findAllByRole('button', { name: 'Corregir' }).then((botones) => botones[0]));
    await user.selectOptions(screen.getByLabelText('Gestor que corrige'), screen.getByRole('option', { name: 'Andrés Peña' }));
    await user.click(screen.getByRole('button', { name: 'Guardar corrección' }));

    expect(await screen.findByText(detalle)).toBeTruthy();
    const modal = screen.getByRole('dialog');
    expect(within(modal).getByLabelText('Motivo de la corrección')).toBeTruthy();
  });

  it('CorregirContacto_CuandoSeCompletaDesdeLaPantalla_LaListaRefrescaConElValorVigente', async () => {
    const contactoCorregido: Contact = {
      id: 95,
      patientId: 23,
      patientName: 'Ana Lucía Ortega',
      gestorId: 19,
      gestorName: 'Laura Gómez',
      contactDate: '2026-09-02',
      channel: 'llamada',
      result: 'contestado'
    };

    let filas = contactos;
    const createAmendment = jasmine.createSpy('createAmendment').and.callFake(() => {
      filas = filas.map((contacto) => (contacto.id === 95 ? { ...contacto, result: 'contestado' } : contacto));
      return of(contactoCorregido);
    });

    await renderContactListWith(createFakeService({ listContacts: () => of(filas), createAmendment }));

    const user = userEvent.setup();
    await user.click(await screen.findAllByRole('button', { name: 'Corregir' }).then((botones) => botones[0]));
    await user.selectOptions(screen.getByLabelText('Gestor que corrige'), screen.getByRole('option', { name: 'Andrés Peña' }));
    await user.selectOptions(screen.getByLabelText('Resultado'), screen.getByRole('option', { name: 'contestado' }));
    await user.type(screen.getByLabelText('Motivo de la corrección'), 'Se marcó sin responder por error');
    await user.click(screen.getByRole('button', { name: 'Guardar corrección' }));

    expect(createAmendment).toHaveBeenCalledWith(95, {
      gestorId: 19,
      reason: 'Se marcó sin responder por error',
      result: 'contestado'
    });

    expect(await screen.findByRole('status')).toBeTruthy();
    expect(screen.getByRole('status').textContent).toContain('La corrección quedó registrada.');
    expect(screen.queryByRole('dialog')).toBeNull();

    await waitFor(() => {
      const fila = screen.getByRole('row', { name: /Ana Lucía Ortega/ });
      expect(fila.textContent).toContain('contestado');
    });
  });
});
