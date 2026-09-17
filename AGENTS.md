# AGENTS.md — Cómo se trabaja este repositorio

Este repositorio se construye **spec-first**: los documentos mandan y el código los sigue. Ese orden es verificable en el historial de commits.

## Orden de trabajo

1. **`01-hallazgos.md`** — lectura crítica de la especificación: vacíos, contradicciones, riesgos y los supuestos con los que se avanza.
2. **`02-plan.md`** — alcance cerrado, exclusiones justificadas, modelo de datos, contrato de interfaz, secuencia de trabajo y riesgos.
3. **`03-bitacora.md`** — matriz de trazabilidad (criterio → implementación → prueba) y registro cronológico de decisiones.
4. **Recién después, el código.**

Los commits de `01-hallazgos.md` y `02-plan.md` son anteriores al primer commit de código.

## Reglas de trabajo

- Una prueba automatizada por cada criterio de aceptación en alcance; el nombre de la prueba identifica el criterio que verifica.
- La lógica de negocio no vive en el controlador ni en el componente de interfaz.
- Sin secretos en el código: cadenas de conexión y credenciales por configuración o variables de entorno (con archivo de ejemplo).
- Mensajes de commit que explican qué cambió y por qué, en incrementos legibles.
- Los datos de prueba se cargan con un script versionado.

## Uso de asistentes de IA

Se usan asistentes de IA durante el trabajo y quedan declarados en `03-bitacora.md`, junto con las decisiones aceptadas, rechazadas o corregidas y su motivo. Cualquier línea de este repositorio debe poder ser explicada por su autor.

## Metodología: OpenSpec

Las funcionalidades se trabajan como **cambios de OpenSpec** (desarrollo guiado por especificaciones), con el flujo **propose → apply → archive**:

- `openspec/specs/` — cómo funciona el sistema hoy (fuente de verdad).
- `openspec/changes/<cambio>/` — `proposal.md` (por qué y qué), `design.md` (cómo), `tasks.md` (checklist) y `specs/` (deltas de especificación).
- `openspec/changes/archive/` — cambios terminados con sus specs ya fusionadas.

Ninguna funcionalidad se implementa sin su propuesta cerrada. El detalle de los cambios previstos está en `02-plan.md` (sección 8) y su avance se refleja en `03-bitacora.md`.
