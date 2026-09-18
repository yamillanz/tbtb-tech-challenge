## ADDED Requirements

### Requirement: La pantalla de registro muestra el catálogo de referencia
La pantalla de registro de contactos SHALL mostrar los pacientes (nombre, documento y ciudad) y los gestores disponibles en los selects del formulario, obtenidos desde el servicio inyectado.

#### Scenario: Catálogo visible al abrir la pantalla
- **WHEN** la pantalla de registro se abre
- **THEN** los selects de paciente y gestor muestran las opciones disponibles del catálogo, ordenadas por nombre

### Requirement: La pantalla valida los campos obligatorios antes de enviar
La pantalla SHALL mostrar el mensaje de campo obligatorio junto a cada campo vacío al intentar enviar el formulario, y SHALL NOT invocar al servicio de contactos en ese caso.

#### Scenario: Envío con campos vacíos
- **WHEN** se intenta enviar el formulario sin seleccionar paciente, gestor, canal ni resultado
- **THEN** aparecen los mensajes de obligatorio junto a cada campo y el servicio de contactos no es invocado

### Requirement: La pantalla confirma el registro y reinicia el formulario
La pantalla SHALL invocar al servicio con el payload mapeado del formulario al enviar, SHALL mostrar una confirmación de éxito cuando el registro queda creado y SHALL reiniciar el formulario a su estado inicial.

#### Scenario: Envío exitoso
- **WHEN** el formulario se completa con valores válidos y se envía
- **THEN** el servicio recibe el payload mapeado (paciente, gestor, fecha, canal, resultado y notas), se muestra la confirmación de éxito y el formulario vuelve a su estado inicial

### Requirement: La pantalla presenta los errores del servidor junto al campo
La pantalla SHALL presentar el mensaje del servidor junto al campo incumplido cuando la API responde 400 con el detalle por campo, SHALL conservar los datos cargados y SHALL presentar una alerta general con el detalle cuando la API responde 404.

#### Scenario: Error 400 con detalle por campo
- **WHEN** la API responde 400 con el detalle del campo incumplido
- **THEN** el mensaje del servidor aparece junto al campo correspondiente y los valores cargados se conservan

#### Scenario: Error 404 de referencia inexistente
- **WHEN** la API responde 404 por una referencia inexistente
- **THEN** la pantalla muestra una alerta general con el detalle de la respuesta
