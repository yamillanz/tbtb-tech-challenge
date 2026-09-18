# Propuesta: add-error-path-tests

## Por qué

La auditoría de esta jornada detecta desbalance de cobertura: las pruebas concentran los caminos felices y dejan sin prueba criterios de error que **ya están especificados**. La regla del repositorio (una prueba por criterio en alcance) obliga a cerrar la brecha; no cambia comportamiento, así que este cambio no lleva delta de especificación — cubre los criterios que viven en `openspec/specs/contacts` desde CA-2/CA-3 y el `day` malformado de add-pagination.

Estado real de la auditoría:

| Criterio specado | Prueba hoy | Estado |
|---|---|---|
| Motivo obligatorio de la enmienda (400) | API + UI | ✓ |
| Campos obligatorios del formulario (cliente) | UI | ✓ |
| 400 junto al campo en registro | UI | ✓ |
| 404 → alerta general en registro | UI | ✓ |
| Paginación fuera de rango (400) | API (servicio) | ✓ |
| Catálogos del registro fuera de catálogo (400) | — | ✗ sin prueba |
| Fecha fuera del mes en curso (400) | — | ✗ sin prueba |
| Referencias inexistentes paciente/gestor (404) | — | ✗ sin prueba |
| Enmienda sin campos a corregir (400) | — | ✗ sin prueba (el mensaje apareció en el E2E, nadie lo automatiza) |
| Enmienda con campo corregible fuera de catálogo (400) | — | ✗ sin prueba |
| Contacto corregido inexistente (404) | — | ✗ sin prueba |
| `day` malformado en la consulta (400) | — | ✗ sin prueba |
| Sobre HTTP ProblemDetails con `errors.{campo}` | solo verificación manual con curl | ✗ sin prueba automatizada |

## Qué cambia

1. **Pruebas de servicio** para los rechazos de negocio: catálogos y fecha del registro, referencias inexistentes (paciente, gestor, contacto corregido), enmienda sin campos y con campo corregible fuera de catálogo, `day` malformado. Ninguna persiste datos (las reglas abortan antes de `SaveChanges`), así que no requieren limpieza.
2. **Prueba HTTP con `WebApplicationFactory`** que ejercita el `ApiExceptionHandler` real: un 400 con canal fuera de catálogo debe responder `application/problem+json` con `title`, `status`, `detail` y `errors.channel`. Es el criterio con más complejidad: el sobre solo existe en el pipeline HTTP, no a nivel servicio.
3. **Prueba de interfaz** de la pantalla de corrección ante 404 del servidor → alerta general (mismo patrón que el registro ya tiene).

## Criterios de aceptación (en alcance)

- **CA-E1** El registro rechaza con 400 los catálogos fuera de catálogo y la fecha fuera del mes en curso.
- **CA-E2** Las referencias inexistentes (paciente, gestor, contacto corregido) responden 404 sin registrar nada.
- **CA-E3** La enmienda sin campos a corregir o con campo corregible fuera de catálogo responde 400 sin registrarse.
- **CA-E4** El `day` malformado en la consulta del mes responde 400.
- **CA-E5** El sobre HTTP de error es ProblemDetails que identifica el campo incumplido en `errors`.
- **CA-E6** La pantalla de corrección muestra la alerta general ante 404 del servidor.

## Fuera de alcance

- Nuevas reglas de validación o cambios de mensajes existentes (solo pruebas).
- Cobertura de las páginas del API pública del seed (fuera del alcance del PRD).
