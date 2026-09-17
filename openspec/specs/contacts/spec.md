# contacts Specification

## Purpose
TBD - created by archiving change add-contact-recording. Update Purpose after archive.
## Requirements
### Requirement: Registro de contacto asociado a un paciente
El sistema SHALL permitir al gestor registrar un contacto asociado a un paciente indicando fecha, canal y resultado del contacto, y el registro SHALL conservar el gestor que lo registró. El contacto registrado SHALL quedar disponible para la vista del mes y para su corrección con trazabilidad.

#### Scenario: Registro exitoso
- **WHEN** el gestor registra un contacto para un paciente registrado con fecha dentro del mes en curso, canal del catálogo y resultado del catálogo
- **THEN** el contacto queda asociado al paciente con su fecha, canal y resultado, y la API responde 201 con el contacto creado

#### Scenario: Fecha fuera del mes en curso
- **WHEN** el gestor registra un contacto con una fecha fuera del mes en curso
- **THEN** la API responde 400 con ProblemDetails indicando la fecha inválida y el contacto no se persiste

### Requirement: Catálogos controlados de canal y resultado
El sistema SHALL aceptar solo los canales del catálogo (llamada, whatsapp, correo) y solo los resultados del catálogo (contestado, no contesta, buzón, número equivocado, reagendado, otro). Los catálogos SHALL estar controlados también por restricción en la base de datos.

#### Scenario: Canal fuera del catálogo
- **WHEN** el gestor registra un contacto con un canal que no está en el catálogo
- **THEN** la API responde 400 con ProblemDetails indicando el canal inválido y el contacto no se persiste

#### Scenario: Resultado fuera del catálogo
- **WHEN** el gestor registra un contacto con un resultado que no está en el catálogo
- **THEN** la API responde 400 con ProblemDetails indicando el resultado inválido y el contacto no se persiste

### Requirement: Referencias válidas de paciente y gestor
El contacto SHALL referenciar un paciente existente y un gestor existente en el momento del registro.

#### Scenario: Paciente inexistente
- **WHEN** el gestor registra un contacto con un identificador de paciente que no existe
- **THEN** la API responde 404 con ProblemDetails y el contacto no se persiste

#### Scenario: Gestor inexistente
- **WHEN** el gestor registra un contacto con un identificador de gestor que no existe
- **THEN** la API responde 404 con ProblemDetails y el contacto no se persiste

### Requirement: Errores de validación en formato ProblemDetails
Todo error de validación o de referencia SHALL responder en formato ProblemDetails con el campo y el mensaje del error, de forma que la interfaz pueda mostrarlo junto al campo correspondiente.

#### Scenario: Validación con detalle de campo
- **WHEN** el registro de contacto incumple una regla de validación
- **THEN** la respuesta 400 es un ProblemDetails cuyo detalle identifica el campo incumplido y el mensaje en español

### Requirement: Listas de referencia para el formulario
El sistema SHALL exponer la lista de pacientes (id, nombre, documento y ciudad) y la lista de gestores (id y nombre), ordenadas por nombre, para alimentar los selects del formulario de registro.

#### Scenario: Listas disponibles para el formulario
- **WHEN** el formulario de registro de contactos se abre
- **THEN** obtiene los pacientes y los gestores disponibles desde la API con servicio inyectado

