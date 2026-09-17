# scripts/

Migraciones y datos de prueba de SQL Server, versionadas y numeradas.

| Script | Contenido | Llega con el cambio |
|---|---|---|
| `001_schema.sql` | Base de datos y tablas completas del modelo del plan (sección 3), con restricciones e índices | `add-contact-recording` |
| `002_seed.sql` | Datos de prueba con fechas relativas al mes en curso | `add-test-data-seed` |

## Ejecución

Con el contenedor levantado (`docker compose up -d` desde la raíz, tras copiar `.env.example` a `.env`):

```bash
cat scripts/001_schema.sql | docker exec -i tbtb-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD"
cat scripts/002_seed.sql | docker exec -i tbtb-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d TbtbChallenge
```

Las fechas del seed se calculan al ejecutarse (`GETDATE()`), nunca fijas: si quedaran hardcodeadas a un mes concreto, la vista de contactos del mes aparecería vacía al correr el repositorio en otro mes.
