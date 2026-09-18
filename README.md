# Prueba Técnica — Desarrollador (TBTB Global)

Implementación del ejercicio: una funcionalidad vertical completa (registro de contactos, corrección con trazabilidad y consulta mensual) sobre SQL Server + .NET 8 (ASP.NET Core Web API) + Angular 20.

## Qué entrega la solución

- **Registrar contactos**: fecha, canal y resultado de catálogos controlados (también por restricción en la base), con el gestor que registra y errores en ProblemDetails con el campo incumplido.
- **Corregir con trazabilidad**: enmiendas append-only con motivo y autor; el contacto original nunca se actualiza y la consulta resuelve los valores vigentes aplicando las enmiendas en orden de creación.
- **Consultar el mes**: lista del mes con filtros combinados (gestor y ciudad), tira de días con conteos del mes filtrado, filtro por día evaluado en el servidor sobre la **fecha vigente**, y paginación con sobre `items/total/page/pageSize/totalPages/dayCounts`.

Cobertura de verificación: **14 pruebas xUnit contra SQL Server** (incluida una prueba HTTP con `WebApplicationFactory` sobre el sobre ProblemDetails real) y **13 pruebas comportamentales de interfaz con Testing Library**. La matriz de trazabilidad completa (criterio → implementación → prueba) está en `03-bitacora.md`.

## Estructura

- `01-hallazgos.md` — lectura crítica de la especificación: vacíos, contradicciones, riesgos y supuestos.
- `02-plan.md` — alcance cerrado, exclusiones justificadas, modelo de datos, contrato de interfaz, secuencia y riesgos.
- `03-bitacora.md` — matriz de trazabilidad y registro cronológico de decisiones (incluye el uso de asistentes de IA).
- `openspec/` — especificaciones viva (`openspec/specs/`) y cambios archivados (`openspec/changes/archive/`).
- `scripts/` — esquema y datos de prueba de SQL Server, versionados y numerados (ver `scripts/README.md`).
- `api/` — servicio .NET 8.
- `web/` — cliente Angular 20.
- `docker-compose.yml` + `.env.example` — SQL Server de desarrollo.

## Requisitos previos

- Docker (para el SQL Server de desarrollo).
- .NET SDK 8.
- Node.js 20+ con npm.

## Levantar el proyecto desde cero

### 1. SQL Server

```bash
cp .env.example .env        # define una contraseña propia para MSSQL_SA_PASSWORD
docker compose up -d
```

### 2. Esquema y datos de prueba

Con el contenedor levantado (los scripts son idempotentes y pueden re-ejecutarse):

```bash
docker exec -i tbtb-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" < scripts/001_schema.sql
docker exec -i tbtb-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d TbtbChallenge < scripts/002_seed.sql
```

Se usa redirección `<` en lugar de `cat | docker`: si tu terminal tiene `cat` reemplazado por `bat` u otra herramienta con colores y números de línea, los códigos ANSI llegarían al SQL y el script fallaría al parsearse.

### 3. API (.NET 8, puerto 5267)

La cadena de conexión no vive en el repositorio: `appsettings.json` trae el marcador `REPLACE_ME` y el valor real llega por variable de entorno con el mismo nombre de clave.

```bash
export ConnectionStrings__TbtbDatabase='Server=localhost,1433;Database=TbtbChallenge;User Id=sa;Password=<TU_PASSWORD>;TrustServerCertificate=True;'
dotnet run --project api/src
```

Verificación: `curl http://127.0.0.1:5267/api/health` responde 200. Usa `127.0.0.1` explícito: en algunos entornos `localhost` resuelve a IPv6 y la conexión falla.

### 4. Interfaz (Angular 20, puerto 4200)

```bash
cd web
npm ci
npm start
```

La interfaz queda en `http://localhost:4200/contactos` y el proxy de desarrollo (`web/proxy.conf.json`) apunta a `http://127.0.0.1:5267`.

## Pruebas y verificación

```bash
# API — las pruebas corren contra SQL Server (requiere la variable de entorno de arriba)
dotnet test api/TbtbChallenge.sln

# Formato y reglas de Roslyn
dotnet format api --verify-no-changes

# Interfaz — pruebas comportamentales (Karma + Testing Library) y lint
cd web && npm test && npm run lint
```

## Disciplina del repositorio

- **Especificación primero**: las funcionalidades se trabajan como cambios de OpenSpec (`propose → apply → archive`); `openspec/specs/contacts` es la fuente de verdad del contrato, con 21 requerimientos incluidos los de paginación y el sobre ProblemDetails.
- **Una prueba por criterio de aceptación**: el nombre de cada prueba es el espejo del criterio que verifica.
- **Idioma del código**: todo identificador en inglés; el español queda en los datos, la interfaz, los nombres de las pruebas (espejo de los criterios) y el término de dominio `Gestor`. Antes de commitear código, el barrido de idioma documentado en `AGENTS.md` corre sobre los archivos tocados.
- **Sin secretos**: cadenas de conexión y credenciales por configuración o variables de entorno, con `.env.example` de referencia.
- **Historia trazable**: los commits explican qué cambió y por qué; los cambios archivados de OpenSpec documentan la decisión, el design y las tareas de cada funcionalidad.

## Asistentes de IA

El trabajo se realizó con asistentes de IA declarados en `03-bitacora.md`, con las decisiones aceptadas, rechazadas y corregidas de cada jornada y su motivo. Toda línea del repositorio puede ser explicada por su autor.
