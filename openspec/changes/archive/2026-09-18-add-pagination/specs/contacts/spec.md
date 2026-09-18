# Delta de especificación: contacts

## ADDED Requirements

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
