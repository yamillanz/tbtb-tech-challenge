import { HttpErrorResponse } from '@angular/common/http';
import { render, screen, waitFor, within } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { of, throwError } from 'rxjs';
import { ContactList } from './contact-list';
import { ContactsService } from '../services/contacts.service';
import { Contact, ContactFilters, ContactListItem } from '../models/contacts.models';

const contacts: ContactListItem[] = [
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
    contactDate: '2026-09-02',
    channel: 'whatsapp',
    result: 'contestado'
  },
  {
    id: 99,
    patientName: 'Valentina Restrepo',
    gestorName: 'Andrés Peña',
    contactDate: '2026-09-05',
    channel: 'llamada',
    result: 'no contesta'
  }
];

const currentContact: ContactListItem = { ...contacts[0], result: 'contestado' };

type ListCalls = { gestorId?: number; city?: string }[];

function createFakeService(overrides: Partial<ContactsService> = {}, listCalls?: ListCalls): ContactsService {
  return {
    listContacts: (filters: ContactFilters) => {
      listCalls?.push({ ...filters });
      return of(contacts);
    },
    listGestors: () => of([{ id: 19, name: 'Andrés Peña' }]),
    listPatients: () => of([{ id: 1, name: 'Ana Lucía Ortega', documentNumber: 'CC-1', city: 'Bogotá' }]),
    createAmendment: () => of({ ...currentContact, id: 95, patientName: 'Ana Lucía Ortega', gestorName: 'Laura Gómez' }),
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

  it('FiltrosDeLaPantalla_CuandoSeAplicanGestorYCiudad_ConsultanElServidorCombinados', async () => {
    const listCalls: ListCalls = [];
    const createFake = createFakeService(
      {
    listContacts: (filters: ContactFilters) => {
          listCalls.push({ ...filters });
          return of(listCalls.length === 1 ? contacts : [contacts[1]]);
        }
      }
    );

    await renderContactListWith(createFake);

    const user = userEvent.setup();
    await user.selectOptions(screen.getByLabelText('Gestor'), screen.getByRole('option', { name: 'Andrés Peña' }));
    await user.selectOptions(screen.getByLabelText('Ciudad'), screen.getByRole('option', { name: 'Bogotá' }));

    expect(listCalls.length).toBeGreaterThanOrEqual(3);
    expect(listCalls[listCalls.length - 1]).toEqual({ gestorId: 19, city: 'Bogotá' });
  });

  it('NavegacionPorDia_CuandoSeHaceClicEnUnDiaDeLaTira_LaTablaMuestraSoloEseDia', async () => {
    await renderContactListWith(createFakeService());

    const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: '02 · 2' }));

    expect(screen.getByRole('cell', { name: 'Ana Lucía Ortega' })).toBeTruthy();
    expect(screen.getByRole('cell', { name: 'Diego Alejandro Suárez' })).toBeTruthy();
    expect(screen.queryByRole('cell', { name: 'Valentina Restrepo' })).toBeNull();

    await user.click(screen.getByRole('button', { name: 'Mes completo' }));
    expect(await screen.findByRole('cell', { name: 'Valentina Restrepo' })).toBeTruthy();
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
    await user.selectOptions(screen.getByLabelText('Gestor que corrige'), within(screen.getByLabelText('Gestor que corrige')).getByRole('option', { name: 'Andrés Peña' }));
    await user.click(screen.getByRole('button', { name: 'Guardar corrección' }));

    expect(await screen.findByText(detalle)).toBeTruthy();
    const modal = screen.getByRole('dialog');
    expect(within(modal).getByLabelText('Motivo de la corrección')).toBeTruthy();
  });

  it('CorregirContacto_CuandoSeCompletaDesdeLaPantalla_LaListaRefrescaConElValorVigente', async () => {
    const correctedContact: Contact = {
      id: 95,
      patientId: 23,
      patientName: 'Ana Lucía Ortega',
      gestorId: 19,
      gestorName: 'Laura Gómez',
      contactDate: '2026-09-02',
      channel: 'llamada',
      result: 'contestado'
    };

    let rows = contacts;
    const createAmendment = jasmine.createSpy('createAmendment').and.callFake(() => {
      rows = rows.map((contact) => (contact.id === 95 ? { ...contact, result: "contestado" } : contact));
      return of(correctedContact);
    });

    await renderContactListWith(createFakeService({ listContacts: () => of(rows), createAmendment }));

    const user = userEvent.setup();
    await user.click(await screen.findAllByRole('button', { name: 'Corregir' }).then((botones) => botones[0]));
    await user.selectOptions(screen.getByLabelText('Gestor que corrige'), within(screen.getByLabelText('Gestor que corrige')).getByRole('option', { name: 'Andrés Peña' }));
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
