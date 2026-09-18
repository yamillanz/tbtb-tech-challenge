# Tareas: add-pagination

## 1. Contrato y servicio

- [x] 1.1 Sobre paginado (`ContactPageDto`) y `ListContactsOfMonthAsync` con `day`, `page`, `pageSize`: filtro por fecha vigente, `dayCounts` del mes filtrado, orden determinista y validación 400 fuera de rango
- [x] 1.2 Controller: parámetros de query `day`, `page`, `pageSize` cableados al servicio
- [x] 1.3 Prueba xUnit del criterio de paginación (segunda página con total y totalPages) y del criterio de día vigente (contacto movido por enmienda + conteos del mes)

## 2. Pantalla

- [x] 2.1 Models y servicio: `ContactPage`, filtros con `day` y `page`
- [x] 2.2 Componente: controles de paginación, reinicio a página 1 al cambiar mes/filtros/día, página preservada al corregir, tira alimentada por `dayCounts` y fin del filtro de día en el cliente
- [x] 2.3 Prueba de componente del criterio de pantalla (navegación de páginas y página preservada tras corregir)

## 3. Cierre

- [x] 3.1 Verificar el flujo completo en el navegador contra el seed: páginas, navegación por día con conteos y corrección preservando la página
- [x] 3.2 Registrar en la bitácora el avance y las decisiones del change
