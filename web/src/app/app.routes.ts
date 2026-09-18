import { Routes } from '@angular/router';
import { ContactForm } from './contact-form/contact-form';
import { ContactList } from './contact-list/contact-list';

export const routes: Routes = [
  { path: '', redirectTo: 'contactos', pathMatch: 'full' },
  { path: 'contactos', component: ContactList },
  { path: 'contactos/nuevo', component: ContactForm }
];
