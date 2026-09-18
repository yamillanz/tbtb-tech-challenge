## ADDED Requirements

### Requirement: Corrección de contacto como enmienda trazable
El sistema SHALL permitir corregir un contacto registrado mediante una enmienda append-only que registre el motivo, el gestor que corrige, la fecha y los valores anterior y nuevo de cada campo corregido. El contacto original SHALL permanecer inmutable: la corrección SHALL NOT actualizarlo ni eliminarlo.

#### Scenario: Corrección exitosa
- **WHEN** el gestor corrige uno o más campos de un contacto con un motivo
- **THEN** la enmienda queda registrada con motivo, autor, fecha y los valores anterior y nuevo, el contacto original conserva sus valores intactos y la API responde 200 con el contacto con los valores vigentes

#### Scenario: Contacto inexistente
- **WHEN** el gestor intenta corregir un contacto que no existe
- **THEN** la API responde 404 con ProblemDetails y no se registra enmienda alguna

### Requirement: Reglas de la enmienda
La corrección SHALL requerir un motivo no vacío, SHALL requerir al menos un campo a corregir y SHALL validar los campos corregibles contra los mismos catálogos y reglas del registro (canal, resultado, fecha dentro del mes en curso). La enmienda SHALL NOT reasignar paciente ni gestor.

#### Scenario: Enmienda sin motivo
- **WHEN** el gestor envía una corrección sin motivo o con un motivo vacío
- **THEN** la API responde 400 con ProblemDetails y la enmienda no se registra

#### Scenario: Enmienda sin campos a corregir
- **WHEN** el gestor envía una corrección que no modifica ningún campo
- **THEN** la API responde 400 indicando que debe corregir al menos un campo

#### Scenario: Campo corregible fuera del catálogo
- **WHEN** la corrección fija un canal o un resultado fuera del catálogo
- **THEN** la API responde 400 con el detalle del campo inválido y la enmienda no se registra

### Requirement: La consulta refleja el valor vigente
La consulta de contactos del mes SHALL resolver el valor vigente de cada campo aplicando las enmiendas registradas en orden de creación, de modo que el reporte refleje la información corregida sin alterar los valores originales.

#### Scenario: Vista con valor vigente
- **WHEN** un contacto del mes tiene una enmienda sobre un campo y la consulta del mes se ejecuta
- **THEN** el resultado muestra el valor corregido en ese campo y los valores originales permanecen intactos en la base

### Requirement: Historial de enmiendas consultable
El sistema SHALL exponer el historial de enmiendas de un contacto (motivo, autor, fecha, valores anterior y nuevo por campo), ordenado por fecha de creación.

#### Scenario: Auditoría disponible
- **WHEN** se consulta el historial de enmiendas de un contacto corregido
- **THEN** la API responde con las enmiendas registradas con su motivo, autor y valores anterior y nuevo

### Requirement: La pantalla muestra los contactos del mes con los valores vigentes
La pantalla de contactos del mes SHALL listar los contactos del mes en curso (paciente, gestor, fecha, canal y resultado) con los valores vigentes resueltos, obtenidos desde el servicio inyectado, y SHALL ofrecer la acción de corrección por contacto.

#### Scenario: Lista al abrir la pantalla
- **WHEN** la pantalla de contactos del mes se abre
- **THEN** muestra los contactos del mes con sus valores vigentes

### Requirement: La pantalla de corrección presenta errores junto al campo y actualiza la lista
La pantalla de corrección SHALL requerir el motivo y SHALL presentar los errores del servidor junto al campo correspondiente conservando lo cargado; al corregir con éxito SHALL refrescar la lista para mostrar el valor vigente.

#### Scenario: Corrección sin motivo desde la pantalla
- **WHEN** el gestor envía el modal de corrección sin motivo
- **THEN** el mensaje del servidor aparece junto al campo de motivo y los datos cargados se conservan

#### Scenario: Corrección exitosa desde la pantalla
- **WHEN** el gestor corrige un contacto desde el modal con motivo
- **THEN** la lista se refresca y muestra el valor vigente del contacto corregido
