## Why

CA-3 del alcance cerrado — el diferenciador de entorno regulado: el extracto pide corregir un contacto "para que el reporte salga bien" (sobrescritura implícita), pero en un entorno farmacéutico un registro no se puede sobrescribir sin dejar rastro. El hallazgo y su supuesto ya documentados (enmienda versionada con motivo, autor y fecha; contacto original inmutable; el reporte muestra el valor vigente) fueron validados por el coordinador del proceso como camino. Este cambio materializa ese supuesto de punta a punta.

## What Changes

- Servicio de enmiendas: corregir un contacto crea una fila en `ContactAmendments` (append-only) con motivo, autor y valores anterior/nuevo por campo corregido; el contacto original nunca se actualiza ni se elimina.
- Resolución del valor vigente: la consulta de contactos del mes aplica la última enmienda por campo sobre los valores originales — así "el reporte refleja la información corregida".
- API: `POST /api/contacts/{id}/amendments` (200 con el contacto vigente; 400 motivo vacío o sin campos a corregir o catálogo inválido; 404 contacto inexistente), `GET /api/contacts/{id}/amendments` (historial de enmiendas) y `GET /api/contacts?month=…` (lista del mes con valores vigentes; sin `month` se asume el mes en curso). La lista sin filtros es el soporte mínimo de la pantalla de corrección; los filtros combinados por gestor y ciudad llegan con `add-monthly-contacts-view` (CA-4).
- Pantalla "Contactos del mes" (lista sin filtros por ahora) con acción "Corregir" por fila y modal de enmienda con motivo obligatorio y errores visibles junto al campo.
- Pruebas comportamentales de la pantalla nueva con Testing Library, según el estándar ya fijado (renderizar, interactuar y observar): lista con valores vigentes, corrección sin motivo con error junto al campo y corrección exitosa con refresco de lista.
- Catálogos de canal y resultado extraídos a un punto compartido para que la creación y la enmienda validen contra la misma lista.
- Prueba xUnit con el nombre del criterio: `CorregirContacto_CuandoSeCorrigeUnContacto_ElOriginalNoCambiaYElReporteMuestraElValorVigente`.

## Capabilities

### New Capabilities

(ninguna)

### Modified Capabilities

- `contacts`: se agregan los requerimientos de corrección con trazabilidad (enmienda, sus reglas, la resolución del valor vigente, el historial y la pantalla de corrección). Los requerimientos existentes no cambian.

## Impact

- `api/`: nuevo `ContactAmendmentService`, métodos de consulta con valores vigentes en `ContactService`, nuevos controladores/DTOs y clase compartida de catálogos (refactor menor de `ContactService`).
- `web/`: nueva pantalla "Contactos del mes" con modal de corrección, servicios de consumo y suite de pruebas comportamentales (`contact-list.spec.ts`).
- Base de datos: sin cambios de esquema — la tabla `ContactAmendments` ya existe desde el esquema inicial; el script de datos ya incluye un contacto con error evidente para la demostración.
