# 03 — Bitácora

## Matriz de trazabilidad

| Criterio | Implementación (archivos / commits) | Prueba que lo verifica | Estado |
|---|---|---|---|
| CA-2 — Registro de contactos | _pendiente: se completa al implementar_ | `RegistrarContacto_CuandoElGestorCompletaElFormulario_ElContactoQuedaAsociadoAlPaciente` | Parcial |
| CA-3 — Corrección con trazabilidad | _pendiente: se completa al implementar_ | `CorregirContacto_CuandoSeCorrigeUnContacto_ElOriginalNoCambiaYElReporteMuestraElValorVigente` | Parcial |
| CA-4 — Vista del mes con filtros | _pendiente: se completa al implementar_ | `ConsultarContactos_CuandoSeFiltraPorGestorYCiudad_SoloSeMuestranLosQueCumplenAmbosFiltros` | Parcial |

Los estados válidos son: cubierto, parcial o fuera de alcance. Un criterio declarado en alcance sin prueba que lo respalde cuenta como no cubierto. La columna se actualiza al cerrar cada tarea.

## Registro de decisiones y uso de IA

Lista cronológica. Cada entrada: qué se decidió, por qué, y si la propuesta salió del asistente o de mí.

| Fecha/hora | Decisión | Motivo | Origen |
|---|---|---|---|
| 17/09 | Alcance cerrado en CA-2 + CA-3 + CA-4 | Forman una historia vertical completa (registrar → corregir → consultar) y concentran el criterio de entorno regulado en CA-3. Se descartó CA-1 por no aportar señal técnica nueva. | Propia, contrastada con el asistente |
| 17/09 | CA-5 y CA-6 excluidos y declarados | Sus reglas no están definidas en el extracto (no son implementables sin inventar negocio). Se declaran como exclusión justificada, no como oferta condicionada. | Propia |
| 17/09 | Consultas enviadas al coordinador del proceso (3 bloqueantes + recortes declarados) | El enunciado invita a preguntar y los huecos son intencionales; se escalan solo los bloqueantes, cada uno con su supuesto, para no detener el trabajo si no hay respuesta. | Propia |
| 17/09 | El coordinador respondió: no resuelve las consultas por correo (está previsto así), confirma los supuestos como camino y remite las exclusiones al plan | Refuerza que lo evaluado es la solidez del supuesto, su impacto declarado y la coherencia entre modelo, contrato y pruebas; se sigue con los supuestos documentados. | Externa (respuesta del coordinador del proceso) |
| 17/09 | Corrección modelada como enmienda inmutable (no actualización in-place) | En un entorno regulado un registro no se sobrescribe sin rastro; el contacto original queda intacto y cada corrección agrega motivo, autor y fecha. | Propia |
| 17/09 | SQL Server y API en contenedores Docker; acceso a datos con EF Core | El entorno en contenedor hace el README reproducible en una máquina limpia; EF Core es el ORM estándar de ASP.NET Core. | Propia, con sugerencia del asistente |
| 17/09 | Esquema con scripts `.sql` numerados escritos a mano (`scripts/001_schema.sql`, `002_seed.sql`); EF Core solo mapea las tablas existentes | El enunciado pide el esquema versionado en `scripts/` y exige poder sostener cada línea en la defensa: un esquema propio se explica completo; el código autogenerado de las migraciones EF, no. | Asistente (aceptada tras comparar con migraciones EF) |
| 17/09 | Contrato completado con `GET /api/patients` y `GET /api/gestors` | El formulario de registro y el filtro por gestor necesitan datos de referencia que el contrato no devolvía; el filtro de ciudad se deriva de los pacientes cargados para no multiplicar endpoints. | Asistente (detectó el hueco en la revisión del plan, previa al commit) |
| 17/09 | "El reporte" de CA-3 interpretado como el registro de contactos del mes en curso (la vista del mes), no como el reporte de adherencia | CA-6 está fuera de alcance; sin esta interpretación, CA-3 quedaría sin dónde verificarse en la entrega. | Propia, a partir de un hueco señalado por el asistente |
| 17/09 | Supuesto de "paciente activo" y unicidad compuesta del documento incorporados al modelo (`Status` + `StatusChangedAt`, único sobre tipo + número) | El modelo tenía un campo de estado sin supuesto que lo respaldara y un unique solo sobre el número de documento, que rechazaría pacientes legítimos con documentos de distinto tipo. | Asistente (detectado en la revisión del plan) |
| 17/09 | Trabajo organizado como cambios de OpenSpec (propose → apply → archive) | Es la metodología con la que ya trabajo: cada criterio de aceptación se convierte en un cambio con propuesta, tareas y deltas de especificación, y al cerrarse queda archivado con las specs actualizadas. Aporta trazabilidad de extremo a extremo, que es exactamente lo que este ejercicio pide demostrar. | Propia |
| 17/09 | Rechazada una propuesta del asistente: crear `openspec/project.md` antes del primer cambio de OpenSpec | El candidato cuestionó la fuente; la verificación contra la documentación oficial mostró que `project.md` es del esquema legacy y que la versión actual usa `config.yaml` con sección `context:` (opcional). Se corrigió antes de crear el archivo. | Asistente (propuesta corregida a partir del cuestionamiento del candidato) |
| 17/09 | La ejecución de OpenSpec se lleva comando a comando, manualmente (propose → apply → archive, un cambio por vez) | Control directo del flujo por el candidato: cada propuesta se revisa antes de avanzar y cada artefacto queda defendible. | Propia |
| 17/09 | Regla de trabajo fijada: el asistente hace commit y push solo cuando el candidato lo indica, nunca por iniciativa propia | El historial es parte de la entrega y su contenido y momento son decisiones del autor; la regla queda también registrada en `AGENTS.md`. | Propia |
| 17/09 | _pendiente: registrar una segunda propuesta del asistente rechazada o corregida, con su motivo (la primera ya está registrada arriba)_ | | |

**Herramientas de IA utilizadas:** **OpenCode** como agente de código, con los modelos **GLM 5.3**, **GLM 5.3 Flash** y **DeepSeek V4.1 Flash**. Hasta aquí se usaron en: análisis crítico del extracto y revisión de los entregables, redacción y corrección de los documentos (`01`, `02`, `03`), y decisiones de arquitectura del plan (acceso a datos, contrato de la API). Se usa **OpenSpec** como metodología de trabajo: los cambios, tareas y deltas de especificación se registran en `openspec/` a medida que se resuelven. _Las partes restantes (propuestas de cambios OpenSpec, código, pruebas, README) se declaran al cierre, con la distinción por herramienta._
