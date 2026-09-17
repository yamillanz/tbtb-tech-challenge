## Context

Primer cambio del repositorio: existe el scaffolding (`api/` proyecto .NET 8 con controladores y Swagger en Development; `web/` Angular 20; SQL Server 2022 en contenedor vía `docker-compose.yml`). No hay esquema, entidades ni endpoints de dominio. El modelo de datos está cerrado en el plan (sección 3) y el contrato en su sección 4; los supuestos que este diseño materializa están documentados en `01-hallazgos.md`.

## Goals / Non-Goals

**Goals:**
- Esquema persistente reproducible en una máquina limpia (script SQL versionado) y entidades de acceso a datos.
- Registro de contacto de punta a punta: validación de dominio en el servicio, DTOs en la frontera, prueba atada a CA-2 y formulario Angular con error visible.
- Endpoints de referencia (pacientes y gestores) que alimentan el formulario.

**Non-Goals:**
- Corrección de contactos y enmiendas (cambio `add-contact-amendment`): la tabla `ContactAmendments` se crea pero sin funcionalidad.
- Vista del mes con filtros (cambio `add-monthly-contacts-view`).
- Datos de prueba (cambio `add-test-data-seed`), autenticación, autorregistro, paginación.

## Decisions

1. **Esquema por script `.sql` escrito a mano (`scripts/001_schema.sql`) en lugar de migraciones de EF Core.** El enunciado ubica el esquema versionado en `scripts/` y exige poder sostener cada línea en la defensa: un esquema propio se explica completo; el código autogenerado de las migraciones no. El script crea la base `TbtbChallenge` y las cuatro tablas del modelo cerrado, con guardas `IF NOT EXISTS` para poder re-ejecutarse. Alternativa descartada: migraciones EF (código autogenerado difícil de defender, residiría fuera de `scripts/`).

2. **Acceso a datos database-first: entidades escritas a mano mapeando las tablas existentes.** EF Core queda como ORM de lectura/escritura; el esquema no lo genera él. Un solo `TbtbChallengeDbContext` con `OnModelCreating` mínimo (tablas, claves y relaciones; los `CHECK` de catálogos viven en el script SQL). Alternativa descartada: `dotnet ef dbcontext scaffold` (código generado, difícil de sostener línea por línea).

3. **Un solo proyecto con capas por carpetas y namespaces** (`Controllers/`, `Services/`, `Data/`, `Dtos/`, `Entities/`) en lugar de solución multi-proyecto. Cumple "capas separadas entre controlador, servicio y acceso a datos" con una estructura que el candidato explica entera; el flujo controlador → servicio → datos queda explícito sin indirections.

4. **Catálogos de canal y resultado como `nvarchar` con restricciones `CHECK` en el esquema + validación en el servicio con las mismas listas.** Alternativa descartada: tablas de catálogo — complejidad de joins y CRUD innecesaria para listas fijas y cortas (supuesto documentado en hallazgos). El doble control (SQL + servicio) es defensa en profundidad: el servicio valida antes de llegar a la base y devuelve errores legibles.

5. **Errores de dominio como excepciones propias mapeadas a ProblemDetails por un único `IExceptionHandler`:** `ValidationException` → 400, `KeyNotFoundException` → 404. Alternativas descartadas: el controlador validando (mueve lógica a la frontera) o una librería de validación externa (dependencia extra sin señal). Este mismo mecanismo lo reutiliza el cambio de enmiendas.

6. **Pruebas xUnit contra el SQL Server local** (el contenedor ya corriendo): la prueba crea su paciente y gestor de arregle, ejecuta el servicio y verifica que el contacto queda asociado con fecha, canal y resultado. Alternativa descartada: proveedor InMemory de EF — no ejercita semántica real de FK, unicidad ni `CHECK`, y la demo final corre igual contra el contenedor. La prueba se limpia tras ejecutarse.

7. **`ContactDate` como `date` (sin hora).** El "mes" de la vista y del reporte es una decisión de calendario, no de instante; el supuesto de zona horaria (UTC-5 común a los tres países) se materializa al validar que la fecha caiga dentro del mes en curso. Un `datetime` trasladaría el problema de zonas al filtrado.

8. **Rutas de pantalla:** `/` para la vista del mes (cambio futuro) y `/contactos/nuevo` para el formulario de registro. La pantalla consume `ContactsService` (HTTP inyectado), nunca `HttpClient` directo desde el componente.

## Risks / Trade-offs

- [Primera experiencia con EF Core y SQL Server] → Mapeo explícito y código simple; el volumen del cambio es bajo (4 entidades, un contexto) y hay prueba que verifica la persistencia real.
- [Catálogos fijos en `CHECK` + servicio] → Agregar un canal o resultado futuro exige editar script y constante; para listas de 3 y 6 valores el costo es menor que el beneficio de simplicidad.
- [Pruebas contra base real requieren el contenedor arriba] → El README ya exige levantar SQL Server para la demo; la prueba documenta su prerrequisito y se limpia sola.
- [Fechas de contacto futuras dentro del mes] → El extracto no las prohíbe y el hallazgo solo explicita lo retroactivo; se permite cualquier día del mes en curso y se registra como decisión, no como regla del negocio.

## Migration Plan

1. Levantar el contenedor (`docker compose up -d`) con `.env` desde `.env.example`.
2. Ejecutar `001_schema.sql` (crea `TbtbChallenge` y las tablas; re-ejecutable).
3. La aplicación en Development usa la cadena por configuración local (variable de entorno o user-secrets); en una máquina limpia el README lo documenta.
Rollback (solo entorno local): `DROP DATABASE TbtbChallenge` y volver a ejecutar los scripts.

## Open Questions

(ninguna pendiente de bloqueo — los supuestos operativos están declarados en `01-hallazgos.md`)
