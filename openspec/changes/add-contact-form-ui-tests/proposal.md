## Why

El formulario de registro de contactos quedó implementado y verificado manualmente, pero no tiene pruebas automatizadas de su comportamiento. El enunciado declara que las pruebas de interfaz son opcionales y suman; además, las conductas observables de la pantalla (catálogo cargado, validación de obligatorios, confirmación de éxito y errores del servidor junto al campo) son parte del comportamiento del sistema que el spec de `contacts` no declara todavía.

## What Changes

- Se agregan al spec de `contacts` los requerimientos observables de la pantalla de registro: catálogo en los selects, validación de campos obligatorios, confirmación de éxito y presentación de errores del servidor (junto al campo y como alerta general).
- Se agregan pruebas comportamentales del componente con `@testing-library/angular`: se renderiza el componente, se interactúa con el DOM como un usuario (`userEvent`) y se observa el resultado; se descartan las pruebas triviales de creación ("should create") porque no aportan señal.
- Instalación de `@testing-library/angular` (v20, en lockstep con Angular 20) como dependencia de desarrollo de `web/`.

## Capabilities

### New Capabilities

(ninguna)

### Modified Capabilities

- `contacts`: se agregan requerimientos de comportamiento de la pantalla de registro (los requerimientos existentes del API no cambian).

## Impact

- `web/`: nueva suite de pruebas del componente (`contact-form.spec.ts`) y ajuste menor de `app.spec.ts` (se retira la prueba trivial de creación, que no aporta señal); `package.json`/`package-lock.json` con la dependencia de pruebas.
- Sin cambios en código de aplicación: las pruebas verifican el comportamiento ya implementado.
