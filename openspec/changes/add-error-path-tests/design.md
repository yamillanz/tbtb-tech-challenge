# Design: add-error-path-tests

## Contexto

Las reglas de negocio viven en `ContactRules` y los servicios lanzan `ValidationException` (400) o `KeyNotFoundException` (404); el `ApiExceptionHandler` (`IExceptionHandler`) las traduce al sobre ProblemDetails con el diccionario `errors`. El decide el nivel de prueba por criterio.

## Decisiones

### D1 — Nivel de prueba: servicio para los rechazos de negocio, HTTP solo para el sobre

Los 400/404 de negocio se prueban a nivel servicio como el resto de la suite: la excepción con su `Field` y mensaje en español es el contrato que la UI consume. El handler es una traducción 1:1 y probar cada regla por HTTP duplicaría la suite contra el mismo código. La excepción es el criterio del **sobre ProblemDetails**: ese contrato solo existe en el pipeline HTTP, así que se prueba con `WebApplicationFactory` contra el arranque real de la app.

### D2 — `WebApplicationFactory<Program>` sin cambios de comportamiento

Se agrega `Microsoft.AspNetCore.Mvc.Testing` al proyecto de pruebas y la convención `public partial class Program` al final de `Program.cs` (requerida por la factoría, sin efecto funcional). El test arranca la aplicación con la misma `ConnectionStrings__TbtbDatabase` y hace `POST /api/contacts` con canal inválido: la regla de catálogo aborta **antes** de tocar la base, así que la prueba no necesita datos ni limpieza. Un solo test HTTP basta: todos los 400 comparten el mismo handler.

### D3 — Sin delta de especificación (warning de archive esperado)

No cambia comportamiento: los criterios viven ya en `openspec/specs/contacts` y en el requerimiento de add-pagination. El archive de este cambio no fusiona nada y `openspec` emite una advertencia no bloqueante por falta de delta; se acepta y se documenta aquí. El valor del cambio es cerrar la columna "Prueba" de la matriz, no modificar specs.

### D4 — Las pruebas de error no dejan rastro

Los rechazos ocurren antes de persistir (las validaciones preceden a `SaveChanges`; los 404 solo leen). Las pruebas de error no necesitan `finally` de limpieza — con una excepción: la prueba de enmienda fuera de catálogo crea el contacto base (persistido), así que esa sí limpia.

## Detalle de pruebas

| Test | Criterio | Nivel |
|---|---|---|
| `RegistrarContacto_CuandoElCanalOElResultadoEstanFueraDelCatalogo_LaSolicitudNoSePersiste` | Catálogos controlados | servicio |
| `RegistrarContacto_CuandoLaFechaEstaFueraDelMesEnCurso_LaSolicitudNoSePersiste` | Fecha fuera del mes | servicio |
| `RegistrarContacto_CuandoLaReferenciaNoExiste_LaSolicitudNoSePersiste` | Referencias válidas (paciente/gestor 404) | servicio |
| `CorregirContacto_CuandoLaEnmiendaNoModificaNingunCampo_LaEnmiendaNoSeRegistra` | Reglas de la enmienda | servicio |
| `CorregirContacto_CuandoElCampoCorregibleEstaFueraDelCatalogo_LaEnmiendaNoSeRegistra` | Catálogo en enmienda | servicio (con cleanup) |
| `CorregirContacto_CuandoElContactoCorregidoNoExiste_LaEnmiendaNoSeRegistra` | Contacto inexistente 404 | servicio |
| `ConsultarContactos_CuandoElDiaNoTieneFormatoValido_LaConsultaSeRechaza` | day malformado | servicio |
| `ErroresDeValidacion_CuandoElCanalEstaFueraDelCatalogo_LaRespuestaEsProblemDetailsConElCampoIncumplido` | Sobre ProblemDetails | HTTP (factory) |
| `CorreccionDesdeLaPantalla_CuandoElServidorResponde404_MuestraLaAlertaGeneral` | Alerta general en corrección | Testing Library |
