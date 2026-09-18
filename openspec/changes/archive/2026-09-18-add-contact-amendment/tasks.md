## 1. Dominio

- [x] 1.1 Extraer los catálogos de canal y resultado a la clase compartida `Catalogs` y hacer que `ContactService` la consuma (refactor sin cambio de comportamiento)
- [x] 1.2 Crear `ContactAmendmentService` con la creación de la enmienda (validación de motivo, al menos un campo, catálogos y fecha del mes; el contacto debe existir; solo `Add` sobre la tabla de enmiendas) y el historial por contacto
- [x] 1.3 Agregar a `ContactService` la consulta del mes con resolución del valor vigente (enmiendas aplicadas en orden de creación, campo por campo)

## 2. API

- [x] 2.1 DTOs de enmienda y de lista: `CreateAmendmentRequest`, `AmendmentDto`, `ContactListItemDto`
- [x] 2.2 `POST /api/contacts/{id}/amendments` (200 con el contacto vigente), `GET /api/contacts/{id}/amendments` y `GET /api/contacts?month=` en los controladores
- [x] 2.3 Verificar manualmente con peticiones HTTP locales los escenarios del spec (200, 400 motivo, 400 sin campos, 400 catálogo, 404, lista con valor vigente)

## 3. Prueba del criterio

- [x] 3.1 Escribir `CorregirContacto_CuandoSeCorrigeUnContacto_ElOriginalNoCambiaYElReporteMuestraElValorVigente` contra SQL Server: original intacto, enmienda con motivo/autor/anterior/nuevo y valor vigente en la consulta
- [x] 3.2 Completar con la prueba de motivo vacío (400) y ejecutar `dotnet test` en verde

## 4. Pantalla

- [x] 4.1 Ampliar `ContactsService` con `listContacts`, `createAmendment` y modelos nuevos
- [x] 4.2 Crear la pantalla "Contactos del mes" (ruta `/contactos`, lista sin filtros) con acción "Corregir" por fila y modal de enmienda (motivo obligatorio, campos corregibles, errores junto al campo)
- [x] 4.3 Escribir `contact-list.spec.ts` con el doble de `ContactsService` (estándar Testing Library + userEvent): lista con valores vigentes, corrección sin motivo con error junto al campo conservando lo cargado y corrección exitosa con la lista refrescada
- [x] 4.4 Ejecutar la suite de interfaz completa (`npm test`, ChromeHeadless) en verde
- [x] 4.5 Ajustar rutas de la aplicación (hogar → `/contactos`) y verificar el flujo completo en el navegador: corregir el contacto con error del seed y ver la lista refrescada con el valor vigente

## 5. Verificación de la demo de trazabilidad

- [x] 5.1 Verificar por consulta directa tras la corrección del seed: el contacto original intacto, la enmienda registrada y la lista devolviendo el valor vigente
