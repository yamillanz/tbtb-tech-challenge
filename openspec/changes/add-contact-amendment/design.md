## Context

CA-2, las pruebas de interfaz y el seed están completos y archivados; el spec de `contacts` es la fuente de verdad con 9 requerimientos. La tabla `ContactAmendments` existe desde el esquema inicial (el modelo se cerró en el plan §3) con columnas por campo (Old/New) motivo, autor y fecha. El seed deja un contacto marcado como `buzón` con nota de error evidente, insumo de la demostración. El supuesto clave, documentado en hallazgos y respondido por el coordinador: corrección = enmienda versionada, no sobrescritura.

## Goals / Non-Goals

**Goals:**
- Corrección append-only con trazabilidad completa (quién, cuándo, por qué, anterior→nuevo por campo).
- Valor vigente resuelto en la consulta del mes, de modo que el reporte refleje la corrección.
- Historial de enmiendas consultable por API.
- Pantalla con el estándar de error visible ya establecido en el formulario.

**Non-Goals:**
- Reasignar paciente o gestor (la enmienda no lo permite: es otra operación con otro impacto de trazabilidad).
- Filtros por gestor y ciudad en la lista (llegan con CA-4).
- Congelar snapshots de reporte (hallazgo declarado fuera de alcance).
- Restricciones a nivel de base de datos contra UPDATE (ver decisión 2).

## Decisions

1. **Una fila de enmienda por corrección, con los pares anterior/nuevo por campo** (`OldChannel/NewChannel`, etc., nullables): una sola fila describe la corrección completa con su motivo y autor. Alternativa descartada: una fila por campo — más granular pero más ruido en la auditoría y en el historial que el gestor debe leer.

2. **La inmutabilidad se hace cumplir por diseño del servicio, no por la base de datos.** El servicio de enmiendas solo llama a `Add()` sobre `ContactAmendments` y no existe código que actualice `Contacts`; el método de corrección es la única puerta. Alternativa descartada para este alcance: trigger que bloquee UPDATE en la base — defensa en profundidad válida pero innecesaria para este entregable; queda anotada como evolución si el PO la pide (coherente con "cambiarlo después exige decir por qué").

3. **Resolución del valor vigente en el servicio, con algoritmo explícito:** cargar los contactos del mes y sus enmiendas, ordenarlas por `Id` (cresciente: orden de creación) y aplicarlas campo por campo (canal, resultado, fecha, notas). Simple y defendible línea por línea. Alternativa descartada: funciones de ventana en SQL — más difícil de sostener en la defensa para un volumen (~30 contactos, pocas enmiendas) donde la diferencia de rendimiento es despreciable; la consulta combinada con JOIN de la vista mensual sigue siendo real y sus índices ya existen.

4. **Catálogos compartidos en una clase estática (`Catalogs`)** consumida por la creación y la enmienda: una sola lista de canales y resultados; los `CHECK` de la base siguen siendo la segunda barrera. Evita que la enmienda valide contra listas paralelas.

5. **La enmienda solo acepta canal, resultado, fecha y notas** (el request no tiene campos de reasignación): corregir paciente o gestor sería otra operación con otro impacto de trazabilidad y de filtro, tal como lo declara el plan (§3).

6. **Pantalla "Contactos del mes" como nueva ruta `/contactos` y hogar de la aplicación** (`/` redirige a la lista): la corrección necesita encontrar el contacto, y esta lista sin filtros es el soporte mínimo; CA-4 añade los filtros combinados sobre la misma vista. El modal reutiliza el estándar de errores del servidor junto al campo que ya quedó verificado en el formulario.

7. **Las pruebas comportamentales de la pantalla van incluidas en este cambio, no en uno aparte.** El patrón establecido en `add-contact-form-ui-tests` se reutiliza: `@testing-library/angular` con `userEvent`, doble de `ContactsService` en los providers del render y nombres de prueba en español con formato Accion_Cuando_Entonces. El plan de pruebas cubre tres conductas: la lista muestra los contactos del mes con sus valores vigentes; el envío del modal sin motivo presenta el error del servidor junto al campo conservando lo cargado; la corrección exitosa invoca al servicio con el payload y refresca la lista con el valor vigente. Se planifican en esta propuesta para que el spec, las pruebas y la pantalla nazcan atados — corregir una pantalla sin su plan de pruebas desde la propuesta, como ocurrió con el formulario, obligó a un change aparte posterior.

## Risks / Trade-offs

- [La resolución en memoria no escala con miles de enmiendas por contacto] → Con el volumen del programa (≈400 pacientes, contacto por paciente/mes) es despreciable; el patrón de consulta y los índices quedan listos para el cambio de vista con filtros.
- [El historial crece sin límite y el modal no lo muestra todavía] → El endpoint `GET /api/contacts/{id}/amendments` expone la auditoría para la demostración; una vista de historial en la interfaz queda como evolución, no como parte del criterio.
- [Dos servicios escriben en la misma base] → Roles claros: `ContactService` crea y consulta; `ContactAmendmentService` enmienda y audita; ambos comparten catálogos y el mismo contexto.

## Migration Plan

1. Ejecutar la suite xUnit contra la base con el seed (la prueba crea su arreglo y limpia).
2. Demostración: corregir el contacto con error del seed (resultado `buzón` → `contestado`, con motivo) desde la pantalla; verificar por consulta directa que el original intacto, la enmienda registrada y la lista mostrando el valor vigente.
Rollback: sin cambios de esquema; revertir el código revierte la funcionalidad (las enmiendas ya insertadas son parte del historial).

## Open Questions

(ninguna — el supuesto de corrección está validado por el coordinador del proceso y documentado en hallazgos)
