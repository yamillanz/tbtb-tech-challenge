# 02 — Plan y cierre de alcance

## 1. Alcance cerrado

Esta entrega cubre **tres criterios de aceptación**, elegidos por formar una misma historia vertical (registrar → corregir con trazabilidad → consultar):

- **CA-2 — Registro de contactos.** El gestor registra un contacto asociado a un paciente con fecha, canal y resultado.
- **CA-3 — Corrección con trazabilidad.** El gestor corrige un contacto registrado con error; la corrección queda auditada (motivo, autor, fecha) y el reporte refleja la información vigente. "El reporte" se interpreta como el registro de contactos del mes en curso —donde el gestor registra los contactos día a día—, que en esta entrega es la vista de contactos del mes (sección 4): con CA-6 fuera de alcance, es la única salida donde la corrección puede observarse.
- **CA-4 — Vista del mes con filtros.** La coordinadora ve los contactos del mes en curso y filtra por gestor y por ciudad, con ambos filtros combinados.

El alcance se mantiene deliberadamente acotado: una funcionalidad que atraviesa las tres capas (base de datos, API y pantalla) completa y defendible vale más que varias a medias.

## 2. Fuera de alcance

| Excluido | Justificación |
|---|---|
| CA-1 (registro de pacientes) | La entidad `Patient` existe porque CA-2 y CA-4 dependen de ella, y se puebla con el script de datos de prueba. Se excluye la funcionalidad de registro: no aporta señal técnica nueva frente a CA-3, que es donde vive el criterio de entorno regulado. |
| CA-5 (ilocalizable) | La regla no está definida en el extracto (ver hallazgo correspondiente): no se puede implementar sin inventar qué cuenta como "no contesta" ni qué implica el estado. |
| CA-6 (reporte de adherencia) | Depende del calendario de seguimiento, pendiente de confirmación con el área médica (ver hallazgo correspondiente). |
| Autorregistro (Alcance #2) | Contradice la obligatoriedad del teléfono y abre riesgos de consentimiento y duplicación (ver hallazgos correspondientes). |
| Autenticación y permisos | El extracto no define usuarios ni roles. Se modela el gestor como entidad simple y se documenta la ausencia de autenticación. |
| Reporte congelado al cierre de mes | Requiere definir quién cierra y cuándo; se documenta como evolución y no como parte de esta entrega. |
| Extensión móvil offline | Se diseña en la sección 7 de este plan; no se implementa. |

## 3. Modelo de datos

Base de datos: **SQL Server**. El esquema se crea con scripts `.sql` versionados y numerados, escritos a mano en `scripts/`: `001_schema.sql` (tablas, restricciones e índices) y `002_seed.sql` (datos de prueba). EF Core queda solo para el acceso a datos: clases de entidad que mapean las tablas ya creadas, también escritas a mano. Se descartó crear el esquema con migraciones de EF Core porque el enunciado ubica el esquema versionado en `scripts/` y porque su código autogenerado es difícil de sostener línea por línea en la defensa; un esquema propio se explica completo. Las fechas del seed se calculan al ejecutarse, relativas al mes en curso (`GETDATE()`, `DATEFROMPARTS`), nunca literales: cargadas fijas, la vista del mes aparecería vacía al ejecutar el repositorio en otro mes.

**Patient** — paciente del programa.
`Id` (int, PK) · `Name` (nvarchar(200), obligatorio) · `DocumentType` (nvarchar(20), obligatorio) · `DocumentNumber` (nvarchar(50), obligatorio; único junto al tipo) · `Phone` (nvarchar(30), obligatorio) · `Email` (nvarchar(200)) · `City` (nvarchar(100), obligatorio) · `TreatmentStartDate` (date, obligatorio) · `Status` (nvarchar(20): activo/inactivo) · `StatusChangedAt` (datetime2) · `CreatedAt` (datetime2)

**Gestor** — persona que contacta pacientes y registra contactos.
`Id` (int, PK) · `Name` (nvarchar(200), obligatorio) · `Email` (nvarchar(200))

**Contact** — contacto realizado. **Registro inmutable**: no se actualiza ni se borra.
`Id` (int, PK) · `PatientId` (FK → Patient, obligatorio) · `GestorId` (FK → Gestor, obligatorio) · `ContactDate` (date, obligatorio) · `Channel` (nvarchar(20): llamada / whatsapp / correo) · `Result` (nvarchar(30): catálogo definido en el supuesto) · `Notes` (nvarchar(500)) · `CreatedAt` (datetime2)

**ContactAmendment** — enmienda de un contacto. **Solo inserción (append-only).**
`Id` (int, PK) · `ContactId` (FK → Contact, obligatorio) · `Reason` (nvarchar(300), obligatorio) · `AmendedByGestorId` (FK → Gestor) · `OldChannel` / `NewChannel`, `OldResult` / `NewResult`, `OldContactDate` / `NewContactDate`, `OldNotes` / `NewNotes` (nullable, solo los campos corregidos) · `AmendedAt` (datetime2)

**Nota sobre la corrección de un registro ya guardado:** el contacto original nunca se modifica. La corrección inserta una fila en `ContactAmendment` con el valor anterior, el nuevo, el motivo y el autor. La vista del contacto y el reporte resuelven el valor vigente aplicando la última enmienda por campo. La enmienda corrige canal, resultado, fecha y notas; no reasigna paciente ni gestor: reasignar un contacto es otra operación, con distinto impacto en la trazabilidad y en el filtro por gestor de la vista del mes. Esta decisión responde al hallazgo de trazabilidad: en un entorno regulado, el historial es parte del dato.

**Índices:** `IX_Contacts_ContactDate_GestorId` (vista mensual filtrada por gestor) y `IX_Contacts_PatientId_ContactDate` (historial por paciente). Con el volumen inicial (~400 pacientes) no son críticos, pero el patrón de consulta mensual por gestor es el que crecerá con el programa.

## 4. Contrato de interfaz

**API (ASP.NET Core Web API, .NET 8).** DTOs propios en la frontera; nunca se exponen entidades de EF.

| Método | Ruta | Entrada | Salida | Errores |
|---|---|---|---|---|
| POST | `/api/contacts` | `CreateContactRequest` { patientId, gestorId, contactDate, channel, result, notes } | `ContactDto` (201) | 400 validación (ProblemDetails), 404 paciente o gestor inexistente |
| POST | `/api/contacts/{id}/amendments` | `CreateAmendmentRequest` { gestorId, reason, channel?, result?, contactDate?, notes? } | `ContactDto` actualizado (200) | 400 (motivo obligatorio, sin campos a corregir), 404 contacto inexistente |
| GET | `/api/contacts?month=YYYY-MM&gestorId=&city=` | parámetros de consulta; sin `month` se asume el mes en curso | `ContactListItemDto[]` | 400 (mes inválido o fuera de rango) |
| GET | `/api/contacts/{id}/amendments` | — | `AmendmentDto[]` | 404 |
| GET | `/api/patients` | — | `PatientOptionDto[]` { id, name, documentNumber, city } | — |
| GET | `/api/gestors` | — | `GestorOptionDto[]` { id, name } | — |

Los endpoints de pacientes y gestores alimentan los selects del formulario de registro y el filtro por gestor de la vista del mes. El filtro de ciudad no tiene endpoint propio: se llena con las ciudades distintas de los pacientes ya cargados.

**Pantallas (Angular 17+):**
- **Contactos del mes** — tabla con filtros por gestor y ciudad (combinados; las ciudades se derivan de los pacientes cargados), acción "Corregir" por fila.
- **Registrar contacto** — formulario con validación visible (paciente, gestor, fecha, canal, resultado).
- **Corregir contacto** — modal con motivo obligatorio y los campos corregibles.

**Caso de error de punta a punta:** intentar corregir sin motivo → la API responde 400 con el detalle del campo y el formulario lo muestra junto al campo, sin perder lo cargado.

## 5. Secuencia de trabajo

| # | Tarea | Estimado |
|---|---|---|
| 1 | Repositorio, documentos y commit inicial (hallazgos + plan, antes de cualquier código) | 30 min |
| 2 | Entorno: SQL Server en Docker, .NET 8 SDK, proyecto API y proyecto Angular | 45 min |
| 3 | Modelo de datos: `001_schema.sql` (esquema), `002_seed.sql` (datos) y entidades EF Core de acceso | 60 min |
| 4 | Capa de servicio: registro de contacto (CA-2) y enmienda (CA-3) con validaciones | 60 min |
| 5 | API: controladores, DTOs y manejo de error | 45 min |
| 6 | Pruebas xUnit: una por criterio (CA-2, CA-3, CA-4) | 45 min |
| 7 | Pantalla Angular: tabla, filtros, formulario y modal de corrección | 90 min |
| 8 | README, bitácora, matriz de trazabilidad y limpieza final | 45 min |

Si el tiempo aprieta, el recorte previsto es bajar a CA-2 + CA-3 (se documenta el recorte en este plan y en la bitácora). La secuencia suma alrededor de siete horas de trabajo efectivo: por encima de la referencia de cuatro a seis horas del enunciado, principalmente por la curva de aprendizaje del stack, y dentro del reloj de la prueba con margen.

Las tareas 3 a 7 se ejecutan dentro de los cambios de OpenSpec (ver sección 8): cada cambio atraviesa en vertical el modelo, el servicio, la API, las pruebas y la pantalla de su criterio, y avanza con su propio `proposal.md`, `tasks.md` y delta de specs antes de escribir el código de ese cambio. Las tareas 1 y 2 (documentos, entorno y scaffolding) preceden a todos los cambios.

## 6. Riesgos

| Riesgo | Mitigación |
|---|---|
| Curva de aprendizaje de .NET y SQL Server | Código simple y explícito, sin patrones que no se puedan explicar; apoyo en la documentación oficial y en el asistente, revisando cada fragmento. |
| El entorno de base de datos no levanta a tiempo | SQL Server en contenedor con script de arranque reproducible; el README documenta el paso a paso probado en limpio. |
| El seed deja la demo vacía si el repositorio se ejecuta en otro mes | Fechas calculadas al ejecutarse, relativas al mes en curso; la prueba del seed verifica que existan contactos en el mes actual. |
| Sobrealcance | Tope explícito de tres criterios; cualquier idea adicional se anota en la bitácora y queda fuera. |
| Que la corrección quede como una actualización encubierta | Prueba específica que verifica que el contacto original no cambia y que la enmienda conserva el valor anterior. |
| Tiempo insuficiente por compromisos paralelos | Orden de tareas por peso de evaluación; recorte previsto y declarado (ver sección 5). |

## 7. Extensión móvil (media página, sin código)

Un gestor en campo, sin señal, necesita registrar contactos igual. El diseño mínimo: la app guarda en el dispositivo una copia de los pacientes y gestores que va a usar, y encola localmente cada contacto registrado con un identificador generado en el teléfono (UUID) y su fecha real de captura. Al recuperar conexión, sincroniza la cola contra el servidor.

Para las correcciones, el caso delicado: el teléfono puede haber corregido un contacto que en el servidor también cambió. La regla propuesta es que **el servidor es la fuente de verdad**: al sincronizar, cada contacto se envía con la versión que el teléfono conoce; si el servidor tiene una versión más nueva, rechaza el envío y devuelve el estado actual para que el gestor resuelva el conflicto en pantalla (no se pisa nada en silencio). Como los contactos son registros inmutables y las correcciones son enmiendas con motivo, el conflicto es raro y siempre queda auditado: ninguna operación offline puede sobrescribir historial sin dejar rastro.

Qué se guarda en el dispositivo: pacientes y gestores de referencia, y la cola de operaciones pendientes. Cuándo se sincroniza: al recuperar conectividad y al abrir la app. Qué no se guarda: datos de pacientes que el gestor no necesita para su ruta.

## 8. Metodología de trabajo y cambios (OpenSpec)

El trabajo se organiza con **OpenSpec** (desarrollo guiado por especificaciones): cada unidad de trabajo es un *cambio* con su propuesta, su diseño técnico, sus tareas y sus deltas de especificación. El flujo es **propose → apply → archive**: nada se implementa sin propuesta cerrada, y al terminar el cambio se archiva y las specs quedan actualizadas como fuente de verdad.

- `openspec/specs/` — cómo funciona el sistema hoy (fuente de verdad de los requerimientos y sus escenarios).
- `openspec/changes/<cambio>/` — `proposal.md` (por qué y qué), `design.md` (cómo), `tasks.md` (checklist de implementación) y `specs/` (deltas de especificación).
- `openspec/changes/archive/` — cambios terminados, con sus deltas ya fusionados en las specs.

Los documentos del ejercicio y OpenSpec se complementan: `01-hallazgos.md` y `02-plan.md` son la capa de gobierno de la entrega (lectura crítica, alcance y exclusiones); OpenSpec es la capa de ejecución, donde cada criterio de aceptación se convierte en un cambio con tareas verificables. La matriz de trazabilidad de `03-bitacora.md` une las dos capas: criterio → cambio → archivos/commits → prueba.

**Cambios previstos:**

| Cambio (OpenSpec) | Objetivo | Cubre | Artefactos | Estado |
|---|---|---|---|---|
| `add-contact-recording` | Registrar contactos (fecha, canal, resultado) asociados a un paciente | CA-2 | proposal.md · design.md · tasks.md · specs/contacts/spec.md | Archivado |
| `add-contact-amendment` | Corregir un contacto con trazabilidad: enmienda inmutable con motivo y autor; el reporte refleja el valor vigente | CA-3 | proposal.md · design.md · tasks.md · specs/contacts/spec.md (delta) | Archivado |
| `add-monthly-contacts-view` | Vista de contactos del mes en curso con filtros combinados por gestor y ciudad, tira de días y paginación (cierre del alcance + change `add-pagination`) | CA-4 | proposal.md · design.md · tasks.md · specs/contacts/spec.md (delta) | Archivado |
| `add-test-data-seed` | Datos de prueba reproducibles (pacientes, gestores y contactos con fechas calculadas al ejecutarse, relativas al mes en curso) | Soporte | tasks.md (sin delta de specs: cambio de infraestructura) | Archivado |

**Cambios surgidos durante la implementación** (propuestos, aplicados y archivados con el mismo flujo; quedan declarados aquí para mantener el plan coherente con la entrega):

| Cambio (OpenSpec) | Objetivo | Cubre | Estado |
|---|---|---|---|
| `add-contact-form-ui-tests` | Pruebas comportamentales del formulario con Testing Library (renderizar, interactuar y observar), en lugar de pruebas triviales de creación | CA-2 (interfaz) | Archivado |
| `add-pagination` | Sobre paginado en el límite de la API, filtro por día en el servidor con fecha vigente y `dayCounts` del mes filtrado | CA-4 (extensión) | Archivado |
| `add-error-path-tests` | Cierre de la brecha happy-path/error: 9 pruebas de error y refinación del sobre ProblemDetails con `errors.{campo}` | CA-2/CA-3/CA-4 (verificación) | Archivado |

Cada cambio se valida (`openspec validate`) antes de implementarse y se archiva al completarse, de modo que el historial del repositorio muestre la secuencia completa: propuesta cerrada → tareas ejecutadas → specs actualizadas.
