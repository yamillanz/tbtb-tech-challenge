# Delta de especificación: contacts

## MODIFIED Requirements

### Requirement: Errores de validación en formato ProblemDetails
Todo error de validación o de referencia SHALL responder en formato ProblemDetails con el campo y el mensaje del error, de forma que la interfaz pueda mostrarlo junto al campo correspondiente. El sobre SHALL incluir el diccionario `errors` con el campo incumplido y su mensaje en español.

#### Scenario: Validación con detalle de campo
- **WHEN** el registro de contacto incumple una regla de validación
- **THEN** la respuesta 400 es un ProblemDetails cuyo detalle identifica el campo incumplido y el mensaje en español

#### Scenario: El sobre expone el campo incumplido en errors
- **WHEN** el registro de contacto incumple una regla de validación
- **THEN** la respuesta es `application/problem+json` con `title`, `status`, `detail` y `errors` con el campo incumplido y su mensaje
