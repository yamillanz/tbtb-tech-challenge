# Design: add-pagination

## Contexto

`ListContactsOfMonthAsync` resuelve los valores vigentes **en memoria** después de cargar el mes (la resolución append-only de enmiendas no se traduce limpiamente a SQL sin duplicar la lógica con funciones de ventana por campo). Esa decisión ya validada define el marco de la paginación.

## Decisiones

### D1 — Paginación en el límite de la API, sobre la ventana mensual ya acotada

El sobre paginado (`items`/`total`/`page`/`pageSize`/`totalPages`) lo arma el servicio **después** de resolver vigentes: carga el mes filtrado (por gestor/ciudad), resuelve enmiendas, calcula conteos por día, filtra por día y recorta la página. El mes es la unidad natural de consulta del producto (400–1.200 filas) y la resolución vigente vive en memoria por diseño.

Alternativa descartada: paginación y filtro de día en SQL con la resolución de enmiendas expresada en el motor (una subconsulta de "última enmienda por campo" o funciones de ventana). Se rechaza porque duplica la lógica de vigentes, encarece la prueba unitaria y el volumen mensual no lo exige. Si el histórico creciera a cientos de miles de contactos por mes, la evolución sería una tabla materializada de valores vigentes y paginación SQL sobre ella — se deja anotada, no se construye.

La garantía que importa se mantiene: el servidor responde a lo sumo `pageSize` ítems por llamada, con `total` y `totalPages` del conjunto filtrado.

### D2 — El filtro por día se evalúa sobre la fecha vigente

`ContactDate` es corregible por enmienda, así que filtrar por la fecha original sería incorrecto: un contacto movido de día aparecería en el día viejo. El servicio filtra por la fecha **resuelta** (post-enmiendas). Formato `yyyy-MM-dd`; formato inválido → 400. Un día válido sin contactos devuelve lista vacía (es un filtro, no una regla de validez).

### D3 — `dayCounts` siempre refleja el mes filtrado, independiente del día

Los conteos de la tira se calculan sobre el mes filtrado por gestor/ciudad (ignorando `day`), con fechas vigentes. Así, con un día seleccionado, la tira sigue mostrando todos los días con conteo y se puede navegar entre ellos sin recargar la lógica del cliente. Un solo request alimenta tabla y tira (sin endpoint secundario).

### D4 — Orden determinista: fecha vigente, luego Id

`OrderBy(ContactDateVigente).ThenBy(Id)` — ya era el orden del listado; para paginar es obligatorio que sea determinista entre llamadas. Documentado porque con paginación cualquier orden no determinista produce filas duplicadas u omitidas entre páginas.

### D5 — Página preservada al corregir, reiniciada al cambiar contexto

- Cambio de mes, gestor o ciudad, o selección de otro día → página 1 (el contexto cambió).
- Corrección exitosa → recarga con la página actual (el usuario está mirando esa fila; no se lo expulsa).
- Validación de entrada: `page ≥ 1`, `1 ≤ pageSize ≤ 100`; fuera de rango → 400 ProblemDetails (estilo existente de `ValidationException`).

## Contrato

```
GET /api/contacts?month=YYYY-MM&gestorId=&city=&day=YYYY-MM-DD&page=&pageSize=
200 →
{
  "items": [ ContactListItemDto... ],
  "total": int,
  "page": int,
  "pageSize": int,
  "totalPages": int,
  "dayCounts": { "YYYY-MM-DD": int, ... }
}
```

`month`, `gestorId`, `city`, `day`, `page`, `pageSize` todos opcionales con defaults (`page=1`, `pageSize=20`). `dayCounts` con claves string `yyyy-MM-dd`.

## Non-goals

Selector de pageSize, deep-linking, scroll infinito, calendario de grilla (evolución declarada en bitácora), tabla materializada de vigentes (anotada como evolución de escala).
