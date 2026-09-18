## 1. API — filtros combinados

- [ ] 1.1 Ampliar `ListContactsOfMonthAsync` con los filtros opcionales `gestorId` y `city` combinados (AND cuando llegan ambos, individuales si llega uno, sin filtros → mes completo)
- [ ] 1.2 Escribir `ConsultarContactos_CuandoSeFiltraPorGestorYCiudad_SoloSeMuestranLosQueCumplenAmbosFiltros` contra SQL Server (arreglo con dos gestores y pacientes de dos ciudades; ambos filtros → solo la intersección; sin filtros → todo) y ejecutar `dotnet test` en verde
- [ ] 1.3 Verificar manualmente los escenarios del spec por HTTP (ambos filtros, filtro solo, sin filtros) y documentar en la bitácora la justificación de la consulta combinada y del índice con volumen real

## 2. Pantalla

- [ ] 2.1 Crear la tira de días (agrupación client-side de los contactos por día con conteo) y los selects de filtro por gestor y ciudad con ciudades derivadas de los pacientes; aplicar ambos filtros del servidor y el día de la tira a la tabla
- [ ] 2.2 Escribir `contact-list.spec.ts` ampliada con Testing Library: filtros combinados, navegación por día de la tira y coexistencia con la corrección
- [ ] 2.3 Ejecutar la suite de interfaz completa (`npm test`, ChromeHeadless) en verde

## 3. Cierre

- [ ] 3.1 Verificar el flujo completo en el navegador contra el seed: filtros combinados, tira de días y corrección sobre la lista filtrada
- [ ] 3.2 Registrar en la bitácora la evolución propuesta del calendario de grilla completa
