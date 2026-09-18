## Context

La lista del mes ya existe (GET /api/contacts, valores vigentes, orden por fecha) — la trajo CA-3 como soporte mínimo de la corrección, sin filtros. Los índices del esquema inicial (`IX_Contacts_ContactDate_GestorId`, `IX_Contacts_PatientId_ContactDate`) fueron diseñados en el plan (§3) para este patrón de consulta. La pantalla tiene la tabla y el modal de corrección; le faltan los filtros del criterio y la navegación por día.

## Goals / Non-Goals

**Goals:**
- Filtros combinados por gestor y ciudad en la API, con la semántica del criterio (ambos → intersección).
- Consulta combinada real (Contacts + Patients + Gestors) con su justificación de índice en la bitácora.
- Navegación por día en la pantalla sin cambios de API.

**Non-Goals:**
- Calendario de grilla completa (semanas × días con panel de detalles): evolución propuesta y declarada en la bitácora; la tira de días cubre la navegación por día en esta entrega.
- **Paginación:** queda identificada como el **siguiente change propuesto** — la UX de la lista sin paginación decae rápido con el volumen del mes (ya incómoda con ~30 contactos del seed). El change de la vista del mes se entrega con la lista completa porque el criterio es de filtros; la paginación merece su propio change con su spec (tamaño de página, conteo, y su efecto en la tira de días).
- Filtro por canal o resultado (no está en el criterio).

## Decisions

1. **Filtros combinados del lado del servidor con semántica AND:** la API recibe `gestorId` y `city` opcionales; cuando llegan ambos, solo pasan los contactos que cumplen los dos. Cada filtro también funciona solo (decisión de diseño razonable: el criterio fija el caso de ambos, no prohíbe los individuales; la pantalla los ofrece combinados). Sin filtros → todo el mes. Alternativa descartada: filtrar en la pantalla — el criterio es del sistema y la consulta es la que el enunciado pide justificar.

2. **La consulta combina tablas de verdad:** el filtro por ciudad exige `Contacts JOIN Patients` (la ciudad vive en el paciente) y el filtro por gestor usa `Contacts.GestorId`. EF lo traduce en una sola consulta SQL con JOINs. Justificación completa con índice y volumen va en la bitácora (requisito explícito del enunciado, R8).

3. **La tira de días es presentación, no API:** los contactos ya llegan ordenados por fecha; la pantalla los agrupa por día (chips con conteo) y el clic filtra la tabla localmente. Alternativa descartada: un parámetro `day` en la API — un caso de uso de presentación no justifica un parámetro de contrato, y el ancho de banda ya entrega el mes completo.

4. **Las ciudades del filtro se derivan de los pacientes cargados** (decisión ya tomada al completar el contrato): el select de ciudad se llena con las ciudades distintas de los pacientes, sin endpoint propio.

5. **El calendario de grilla completa queda como evolución propuesta** (bitácora): el criterio no lo pide, la tira de días entrega la navegación por día, y la grilla (semanas, celdas vacías, panel de detalles) costaría 1.5-2 h adicionales que el reloj de la entrega prioriza para README, matriz y limpieza. Si el reloj lo permite tras cerrar el entregable, se materializa como change aparte.

6. **Pruebas de interfaz incluidas desde los artefactos** (patrón corregido en CA-3): Testing Library con doble de `ContactsService` cubre filtros combinados, tira de días y coexistencia con la corrección.

## Risks / Trade-offs

- [La tira de días filtra localmente mientras los filtros del servidor filtran en la API] → Coexisten: la tira es un filtro de presentación sobre el resultado ya filtrado por el servidor; sin interacción entre capas es imposible que se contradigan.
- [El filtro por ciudad depende del texto de la ciudad] → Se filtra por igualdad exacta del valor del catálogo de pacientes (los seed usan valores consistentes); una normalización de ciudades queda como evolución ya declarada en hallazgos.
- [Volumen: el mes completo en una respuesta] → ~400 contactos/mes a escala real, JSON liviano sin paginación justificable todavía; documentado como evolución si el programa crece.

## Migration Plan

1. Ejecutar la suite xUnit contra la base con el seed (la prueba crea su arreglo y limpia).
2. Demostración en pantalla: filtrar por un gestor y una ciudad con datos del seed y ver solo la intersección; tira de días para navegar por fecha.
Rollback: revertir el código (sin cambios de esquema ni de datos).

## Open Questions

(ninguna — la semántica de los filtros está en el criterio y el supuesto del gestor registrado ya está validado)
