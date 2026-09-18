## Context

El formulario (`ContactForm`) está implementado con reactive forms, señales y `OnPush`, y consume `ContactsService` inyectado. Las pruebas existentes en `web/` son las del scaffold (`app.spec.ts`, con una prueba trivial de creación). El candidato fijó el criterio: pruebas de componentes comportamentales — renderizar, interactuar y observar — y descartar las de creación triviales.

## Goals / Non-Goals

**Goals:**
- Pruebas que verifican el comportamiento visible del formulario de punta a punta en el DOM.
- Que `npm test` (Karma, ChromeHeadless) quede en verde.

**Non-Goals:**
- Pruebas de la API (ya cubiertas por la xUnit contra SQL Server, cambio archivado).
- Modificar el comportamiento del componente o del servicio (solo se verifica lo existente).
- Pruebas E2E con navegador (ya realizadas manualmente con Playwright durante el apply del cambio anterior; aquí se cubre la unidad de interfaz).

## Decisions

1. **`@testing-library/angular` en lugar de TestBed nativo.** El estilo consultable por rol/label y `userEvent` reproduce el uso real (renderizar → interactuar → observar) con menos código que `fixture.debugElement`/`By.css`; los queries `getBy*` fallan solos cuando no encuentran, lo que hace las aserciones explícitas sin depender de matchers adicionales (Karma usa Jasmine, no jest-dom). Alternativa descartada: `TestBed` + `DebugElement` — behavioral posible pero más verboso y con selectores frágiles.

2. **`ContactsService` se reemplaza por un doble de prueba en los providers del render.** Las pruebas verifican el componente, no el transporte: `listPatients`/`listGestors` devuelven listas fijas y `createContact` un observable configurado por caso (éxito, 400 con `errors`, 404). Se usan `throwError(() => new HttpErrorResponse(...))` para reproducir el error de `HttpClient`. Alternativa descartada: `HttpClientTestingModule` — prueba el wiring HTTP, que para estas conductas es ruido.

3. **Nombres de pruebas en español** con el mismo criterio del repositorio: identifican la conducta verificada con el formato Accion_Cuando_Entonces, coherente con la prueba del criterio en la capa de servicio.

4. **La prueba trivial `should create the app` se retira de `app.spec.ts`** y se conserva la de render del título. El criterio del candidato: una prueba que solo verifica que el componente se creó no aporta señal.

## Risks / Trade-offs

- [Karma requiere un binario de Chrome] → `google-chrome-stable` está instalado; el builder usa ChromeHeadless por defecto.
- [Dependencia de pruebas extra en `web/`] → es de desarrollo, no afecta el bundle de producción; se justifica por el criterio de pruebas comportamentales.

## Migration Plan

1. Instalar `@testing-library/angular@20` (devDependencies).
2. Escribir `contact-form.spec.ts` con las pruebas del spec.
3. Ejecutar `ng test` y dejar en verde.
Rollback: retirar la dependencia y el archivo de pruebas (sin impacto en la aplicación).
