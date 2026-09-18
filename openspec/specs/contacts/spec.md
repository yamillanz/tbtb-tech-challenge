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

### Requirement: La vista filtra los contactos por gestor y ciudad combinados
La consulta de contactos del mes SHALL aceptar los filtros `gestorId` y `city` como opcionales y combinables: cuando llegan ambos, SHALL devolver solo los contactos que cumplen ambos; cada filtro SHALL también funcionar individualmente; sin filtros SHALL devolver todo el mes.

#### Scenario: Ambos filtros aplicados
- **WHEN** la coordinadora consulta el mes con un gestor y una ciudad
- **THEN** solo aparecen los contactos registrados por ese gestor a pacientes de esa ciudad

#### Scenario: Sin filtros
- **WHEN** la coordinadora consulta el mes sin filtros
- **THEN** aparecen todos los contactos del mes con sus valores vigentes

### Requirement: La pantalla ofrece los filtros y la tira de días
La pantalla de contactos del mes SHALL ofrecer los filtros por gestor y ciudad (combinados, con ciudades derivadas de los pacientes cargados) y una tira de días con conteo por día del mes, que al hacer clic muestra en la tabla únicamente los contactos de ese día.

#### Scenario: Filtros combinados desde la pantalla
- **WHEN** la coordinadora selecciona gestor y ciudad en la pantalla
- **THEN** la tabla muestra solo los contactos que cumplen ambos filtros

#### Scenario: Navegación por día con la tira
- **WHEN** la coordinadora hace clic en un día de la tira
- **THEN** la tabla muestra solo los contactos de ese día y puede volver al mes completo

### Requirement: La corrección y los filtros coexisten
La acción de corrección SHALL seguir disponible sobre los contactos filtrados y la tira de días SHALL reflejarse con el resultado del filtro del servidor activo.

#### Scenario: Corrección sobre la lista filtrada
- **WHEN** la coordinadora filtra por gestor y ciudad y corrige un contacto del resultado
- **THEN** el valor vigente se actualiza en la lista filtrada al refrescarla

### Requirement: La consulta del mes se entrega paginada
La consulta de contactos del mes SHALL responder con un sobre paginado (items, total, page, pageSize, totalPages) en lugar de un arreglo plano, SHALL aceptar `page` y `pageSize` opcionales con valores por defecto (1 y 20) y SHALL responder 400 en ProblemDetails cuando estén fuera de rango. El orden de los items SHALL ser determinista (fecha vigente, luego identificador).

#### Scenario: Segunda página
- **WHEN** la coordinadora consulta el mes con `page=2` y `pageSize=20` sobre un mes filtrado con 34 contactos
- **THEN** la respuesta contiene los items 21–34 del orden determinista, con `total=34` y `totalPages=2`

#### Scenario: Paginación fuera de rango
- **WHEN** la consulta llega con `page=0` o `pageSize` fuera de 1–100
- **THEN** la API responde 400 con ProblemDetails indicando el parámetro inválido

### Requirement: El filtro por día se evalúa en el servidor con la fecha vigente
La consulta SHALL aceptar `day` opcional en formato YYYY-MM-DD y SHALL filtrar por la fecha vigente del contacto (resuelta tras enmiendas), no por la fecha original. La respuesta SHALL incluir `dayCounts` con los conteos por día del mes filtrado por gestor y ciudad, independiente del `day` solicitado, y SHALL responder 400 cuando `day` no tenga el formato esperado.

#### Scenario: Contacto movido de día por una enmienda
- **WHEN** una enmienda movió un contacto del día 02 al día 05 y la consulta llega con `day=05`
- **THEN** el contacto aparece en la respuesta del día 05 y la fecha original permanece intacta en la base

#### Scenario: Conteos de la tira con un día seleccionado
- **WHEN** la consulta llega con `day=03`
- **THEN** `dayCounts` continúa listando todos los días del mes filtrado con sus conteos

### Requirement: La pantalla muestra la paginación y preserva el contexto al corregir
La pantalla SHALL mostrar los controles de paginación (anterior, siguiente, página actual sobre total y el total de contactos), SHALL reiniciar a la página 1 cuando cambia el mes o los filtros o se selecciona otro día, y SHALL preservar la página actual al corregir con éxito un contacto. La tira de días SHALL alimentarse de los conteos que devuelve el servidor.

#### Scenario: Navegación de páginas desde la pantalla
- **WHEN** la coordinadora avanza a la página siguiente
- **THEN** el servicio se invoca con la página siguiente y la tabla muestra esa porción

#### Scenario: Página preservada tras corregir
- **WHEN** la coordinadora corrige un contacto desde la página 2
- **THEN** la lista se recarga en la página 2 mostrando el valor vigente

