## 1. Script de datos

- [ ] 1.1 Escribir `scripts/002_seed.sql`: guarda de idempotencia, 6 gestores, 16 pacientes en seis ciudades (Bogotá, Medellín, Cali, Lima, Quito y Guayaquil) con inicio de tratamiento retroactivo relativo, ~30 contactos del mes en curso con días calculados por módulo del día actual (nunca futuros) y un contacto con error evidente para la demo de corrección
- [ ] 1.2 Ejecutar el script contra el contenedor y verificar los conteos por tabla

## 2. Verificación de la demo

- [ ] 2.1 Verificar con consulta directa que todas las fechas de contacto caen dentro del mes en curso y ninguna en el futuro
- [ ] 2.2 Re-ejecutar el script y verificar la idempotencia: los conteos no cambian
