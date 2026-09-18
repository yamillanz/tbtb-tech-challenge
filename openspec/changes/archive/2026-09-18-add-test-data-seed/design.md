## Context

El esquema ya existe (`scripts/001_schema.sql` con guardas `IF NOT EXISTS`) y la base `TbtbChallenge` está creada en el contenedor. La API y la pantalla están implementadas para CA-2; CA-3 y CA-4 llegan después con sus cambios, y el seed debe dejar el terreno listo para las tres demostraciones. Los evaluadores ejecutarán el repositorio en cualquier mes.

## Goals / Non-Goals

**Goals:**
- Datos suficientes y realistas para las tres demostraciones del alcance sin inventar nada a mano.
- Idempotencia: re-ejecutar el script no duplica datos.
- Fechas siempre dentro del mes en curso y nunca en el futuro.

**Non-Goals:**
- Cambios en el esquema (la tabla de enmiendas ya existe; sin datos en ella).
- Proveedores de datos programáticos o fábricas de datos en el código de la aplicación.
- Cobertura de todos los meses: el seed solo llena el mes en curso (es lo que la vista consulta).

## Decisions

1. **Guarda de idempotencia al inicio del script:** si `Patients` ya tiene filas, el script termina sin insertar (`IF EXISTS ... RETURN`). Alternativa descartada: borrar y volver a insertar — destruye datos de la sesión de demostración en cada re-ejecución y sería sorprendente para quien esté probando.

2. **Fechas de contacto relativas al mes en curso con módulo del día actual:** `contactDate = @inicioMes + (offset % DAY(GETDATE()))`, donde `@inicioMes = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)`. El módulo garantiza que el día caiga entre el 1 y el día de hoy: nunca fechas futuras y siempre dentro del mes, aun si el script corre el día 1 del mes. Alternativa descartada: offset fijo hasta el día 27 — en febrero un día 29/30 sería inválido, y con `DATEADD(DAY, -n, GETDATE())` los contactos cruzarían al mes anterior y la vista mensual los mostraría vacíos.

3. **Fechas de inicio de tratamiento retroactivas relativas** (`DATEADD(MONTH, -1..-6, GETDATE())`): los pacientes parecen iniciar un tratamiento crónico reciente, coherente con el contexto del programa, sin literales.

4. **Distribución para que los filtros de CA-4 se vean:** 6 gestores, 16 pacientes repartidos en seis ciudades (Bogotá, Medellín, Cali, Lima, Quito y Guayaquil — los tres países del programa) y ~30 contactos del mes que mezclan gestores, canales y resultados. Con el mismo patrón de consulta mensual por gestor, el índice `IX_Contacts_ContactDate_GestorId` tiene trabajo real desde el primer día.

5. **Un contacto con error evidente:** resultado `buzón` con nota "Se marcó por error; el paciente respondió" — es el insumo para la demostración de CA-3 (corrección con trazabilidad) cuando llegue su cambio.

6. **Los datos van en español** (nombres, ciudades, notas): son datos para una audiencia hispanohablante; los identificadores técnicos del script (tablas, columnas, variables) van en inglés, como el resto del código.

## Risks / Trade-offs

- [Densidad de contactos baja si el script corre el día 1 del mes] → El módulo colapsa los contactos en un solo día en lugar de romper el mes; la vista sigue mostrando datos y a partir del día 2 la distribución se abre.
- [El guard de idempotencia no actualiza datos si el seed cambió entre versiones] → Para esta entrega es aceptable: el seed es estable; una evolución futura puede versionar los cambios con otro script numerado.

## Migration Plan

1. Ejecutar `002_seed.sql` contra el contenedor (instrucciones en `scripts/README.md`).
2. Re-ejecutar para verificar que la guarda evita duplicados.
3. La demo de CA-2 ya puede registrar contactos sobre estos pacientes y gestores.

## Open Questions

(ninguna — el volumen y la distribución son decisiones de demostración, documentadas arriba)
