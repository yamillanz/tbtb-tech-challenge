## Why

CA-4 del alcance cerrado — el criterio exige que la coordinadora filtre los contactos del mes por gestor y por ciudad y que solo aparezcan los que cumplen **ambos** filtros. La lista del mes ya existe (la trajo CA-3 como soporte de la corrección); este cambio le agrega los filtros combinados del servidor — la consulta combinada que el enunciado exige con su justificación de índice — y la tira de días en la pantalla para navegar el mes por día.

## What Changes

- Filtros en `GET /api/contacts`: parámetros `gestorId` y `city`, combinados (ambos aplicados simultáneamente cuando llegan; cada uno opcional por separado; sin filtros se ve todo el mes). La consulta combina `Contacts` con `Patients` para el filtro por ciudad y con `Gestors` para el filtro de gestor.
- Prueba xUnit con el nombre del criterio: `ConsultarContactos_CuandoSeFiltraPorGestorYCiudad_SoloSeMuestranLosQueCumplenAmbosFiltros`.
- Pantalla "Contactos del mes": selects de gestor y ciudad (las ciudades se derivan de los pacientes ya cargados) combinados, y una **tira de días** encima de la tabla: cada día del mes con contactos se muestra con su conteo y al hacer clic la tabla muestra solo ese día (filtro de presentación, client-side; los datos ya vienen ordenados por fecha).
- Pruebas comportamentales de la pantalla con Testing Library (estándar fijado): filtros combinados, tira de días y acción de corrección coexistiendo.
- Justificación de la consulta y de su índice con volumen real en la bitácora (requisito del enunciado).
- El **calendario de grilla completa** (semanas × días con panel de detalles) queda declarado como evolución propuesta en la bitácora: se acotó por presupuesto de tiempo y la tira de días entrega la navegación por día a una fracción del costo.

## Capabilities

### New Capabilities

(ninguna)

### Modified Capabilities

- `contacts`: se agrega el requerimiento de filtros combinados por gestor y ciudad y el de la tira de días en la pantalla. Los requerimientos existentes no cambian.

## Impact

- `api/`: `ListContactsOfMonthAsync` acepta y aplica los filtros (la unión con `Patients` para la ciudad ya existe por el mapeo de navegación); justificación de índice documentada en la bitácora.
- `web/`: la pantalla "Contactos del mes" gana los selects de filtro y la tira de días; suite de pruebas ampliada.
- Base de datos: sin cambios de esquema; los índices del esquema inicial (`IX_Contacts_ContactDate_GestorId`) ya sostienen el patrón de consulta.
