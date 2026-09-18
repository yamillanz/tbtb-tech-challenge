# seed-data Specification

## Purpose
Datos de demostración reproducibles: un script idempotente carga gestores, pacientes y contactos del mes en curso con fechas calculadas al ejecutarse, de modo que la funcionalidad pueda demostrarse sin inventar datos ni importar el mes en que se corra el repositorio.
## Requirements
### Requirement: Datos de demostración reproducibles
El repositorio SHALL incluir un script idempotente de datos de prueba que cargue gestores, pacientes y contactos del mes en curso con fechas calculadas al ejecutarse, de modo que la funcionalidad pueda demostrarse sin inventar datos ni importar el mes en que se corra.

#### Scenario: Re-ejecución del script
- **WHEN** el script de datos de prueba se ejecuta más de una vez contra la misma base
- **THEN** no se insertan filas duplicadas y los conteos por tabla se mantienen

#### Scenario: Fechas de contacto válidas para la demostración
- **WHEN** el script carga los contactos de demostración
- **THEN** todas las fechas de contacto quedan dentro del mes en curso y ninguna en el futuro, calculadas al ejecutarse y sin literales de fecha

