## Why

La funcionalidad implementada necesita datos con los que demostrarse: el enunciado exige "un script que cargue datos suficientes para que podamos ver la funcionalidad andando sin inventarlos nosotros" (requisito mínimo 5 de la Parte III). Además, la vista y el registro de contactos operan sobre "el mes en curso": si el seed llevara fechas literales de un mes fijo, la demo aparecería vacía cuando el repositorio se corra en otro mes.

## What Changes

- Script `scripts/002_seed.sql`: carga gestores, pacientes de las tres ciudades/países del programa (Colombia, Perú y Ecuador) y contactos del mes en curso que mezclan gestores, canales y resultados, con un contacto con error evidente listo para la demostración de la corrección con trazabilidad (CA-3).
- Fechas del seed calculadas al ejecutarse: los días de contacto caen dentro del mes en curso y nunca en el futuro, sin literales.
- Script idempotente: si ya existen datos, no inserta nada (se puede re-ejecutar sin efecto).

## Capabilities

### New Capabilities

- `seed-data`: la garantía del repositorio entregable de proveer datos de demostración reproducibles (script idempotente, fechas relativas al mes en curso), que el enunciado exige como requisito mínimo de la Parte III.

### Modified Capabilities

(ninguna)

## Impact

- `scripts/002_seed.sql` (nuevo archivo).
- La base local queda con datos suficientes para: registrar contactos (CA-2) sobre pacientes y gestores reales, y —al implementarse CA-3 y CA-4— corregir el contacto con error y filtrar la vista por gestor y ciudad.
- La forma de ejecutarlo ya está documentada en `scripts/README.md`.
