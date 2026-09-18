## 1. Dependencia de pruebas

- [ ] 1.1 Instalar `@testing-library/angular@20` como dependencia de desarrollo de `web/`

## 2. Pruebas comportamentales del formulario

- [ ] 2.1 Escribir `contact-form.spec.ts` con el doble de `ContactsService` en los providers del render (listas fijas y observables configurables por caso)
- [ ] 2.2 Prueba de catálogo: al abrir la pantalla, los selects muestran pacientes y gestores ordenados
- [ ] 2.3 Prueba de obligatorios: envío vacío muestra los mensajes junto a cada campo y no invoca al servicio
- [ ] 2.4 Prueba de envío exitoso: el servicio recibe el payload mapeado, se muestra la confirmación y el formulario se reinicia
- [ ] 2.5 Prueba de error 400: el mensaje del servidor aparece junto al campo incumplido y los datos cargados se conservan
- [ ] 2.6 Prueba de error 404: la alerta general muestra el detalle de la respuesta

## 3. Cierre

- [ ] 3.1 Retirar la prueba trivial `should create the app` de `app.spec.ts` y conservar la de render del título
- [ ] 3.2 Ejecutar `npm test` (Karma, ChromeHeadless) y dejar todas las pruebas en verde
