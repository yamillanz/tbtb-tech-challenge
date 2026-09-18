# Tareas: add-error-path-tests

## 1. Infraestructura HTTP

- [x] 1.1 Paquete `Microsoft.AspNetCore.Mvc.Testing` y `public partial class Program`; prueba HTTP del criterio ProblemDetails (400 con `errors.channel` vía pipeline real)

## 2. Rechazos de negocio a nivel servicio

- [x] 2.1 Registro: catálogos fuera de catálogo, fecha fuera del mes y referencias inexistentes (paciente/gestor) sin persistir
- [x] 2.2 Enmienda: sin campos a corregir, campo corregible fuera de catálogo y contacto inexistente sin registrarse
- [x] 2.3 Consulta del mes: `day` malformado se rechaza

## 3. Pantalla

- [x] 3.1 Prueba de componente: corrección ante 404 del servidor muestra la alerta general

## 4. Cierre

- [x] 4.1 Matriz de trazabilidad: columna "Prueba" actualizada con los tests de error de cada criterio
- [x] 4.2 Bitácora: hallazgo de la auditoría y estado del change
