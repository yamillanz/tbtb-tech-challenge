# Propuesta: add-pagination

## Por qué

El listado del mes entrega todos los contactos del mes en una sola respuesta. Con el volumen real del programa (~400 pacientes → 400–1.200 contactos/mes) la tabla se degrada: la interfaz renderiza cientos de filas y la coordinación pierde el contexto. La paginación quedó identificada como siguiente change en el design de `add-monthly-contacts-view`.

Además, el filtro por día hoy se evalúa en el cliente sobre la página cargada: con volumen, un día puede tener más contactos que los que caben en una vista, y el conteo de la tira debe seguir reflejando el mes completo aunque se esté mirando una porción.

## Qué cambia

1. **La consulta del mes se entrega paginada**: la API responde con un sobre (`items`, `total`, `page`, `pageSize`, `totalPages`) en lugar de un arreglo plano. `page` y `pageSize` llegan por query string (por defecto 1 y 20).
2. **El filtro por día se evalúa en el servidor** y sobre la **fecha vigente** (una enmienda puede mover un contacto de día; el filtro debe seguir al valor corregido). La respuesta incluye `dayCounts`: los conteos por día del mes filtrado por gestor/ciudad, independientes del día seleccionado, para que la tira siga mostrando todos los días.
3. **La pantalla muestra los controles de paginación** (anterior/siguiente, "Página X de Y" y el total), reinicia a la página 1 cuando cambian mes o filtros y **preserva la página al corregir**.

## Criterios de aceptación (en alcance)

- **CA-P1** La lista del mes se entrega paginada con el total del mes filtrado; página y tamaño inválidos responden 400 en ProblemDetails.
- **CA-P2** El filtro por día se resuelve en el servidor con la fecha vigente y la respuesta incluye los conteos por día del mes filtrado.
- **CA-P3** La pantalla ofrece los controles, reinicia la página al cambiar mes o filtros y la preserva al corregir.

## Fuera de alcance

- Selector de tamaño de página (se fija en 20).
- Estado de la página en la URL (deep-linking).
- Scroll infinito y el calendario de grilla completa (evolución declarada en bitácora).
- Paginación en SQL con resolución de enmiendas en el motor (ver design, decisión D1).

## Riesgos

- Cambio de forma de la respuesta (arreglo → sobre): API y UI se despliegan juntas en este repositorio, riesgo contenido.
- La corrección refresca la lista: si se reiniciara a la página 1, el usuario pierde su contexto; se maneja preservando la página (D4 en design).
