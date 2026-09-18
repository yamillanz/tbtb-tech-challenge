## ADDED Requirements

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
