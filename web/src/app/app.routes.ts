import { Routes } from '@angular/router';
import { ContactForm } from './contact-form/contact-form';

export const routes: Routes = [
  { path: '', redirectTo: 'contactos/nuevo', pathMatch: 'full' },
  { path: 'contactos/nuevo', component: ContactForm }
];
