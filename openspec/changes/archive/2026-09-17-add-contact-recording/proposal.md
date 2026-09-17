## Why

CA-2 del alcance cerrado: el gestor registra un contacto asociado a un paciente con su fecha, canal y resultado. Es la escritura base del dominio — sin contactos registrados no existen ni la corrección trazable (CA-3) ni la vista del mes (CA-4). La fecha, el canal y el resultado provienen de supuestos ya documentados en `01-hallazgos.md` (catálogo mínimo de resultados; el filtro por gestor usa el gestor que registró el contacto).

## What Changes

- Script `scripts/001_schema.sql` con el esquema completo del modelo cerrado en el plan (sección 3): `Patients`, `Gestors`, `Contacts` y `ContactAmendments`, con restricciones e índices. La tabla de enmiendas se crea aquí porque el modelo ya está cerrado; su funcionalidad llega con `add-contact-amendment`.
- Entidades EF Core escritas a mano que mapean las tablas existentes (sin migraciones ni scaffolding), y `DbContext` con la conexión por configuración (sin secretos en el código).
- API: `POST /api/contacts` (registra el contacto), `GET /api/patients` y `GET /api/gestors` (alimentan los selects del formulario). DTOs propios en la frontera; errores con ProblemDetails (400) y 404.
- Servicio de dominio con la validación del registro: catálogos de canal (llamada / whatsapp / correo) y de resultado (contestado / no contesta / buzón / número equivocado / reagendado / otro), fecha dentro del mes en curso, paciente y gestor existentes.
- Prueba xUnit con nombre identificador del criterio: `RegistrarContacto_CuandoElGestorCompletaElFormulario_ElContactoQuedaAsociadoAlPaciente`.
- Pantalla Angular "Registrar contacto": formulario con selects de paciente y gestor, validación visible y consumo del API por servicio inyectado.

## Capabilities

### New Capabilities

- `contacts`: registro de contactos de un paciente (fecha, canal, resultado, gestor que lo registró), con catálogos controlados y validación de referencia. Este mismo capability recibirá después los requerimientos de corrección con trazabilidad (CA-3).

### Modified Capabilities

(ninguna — es la primera capacidad del sistema)

## Impact

- `api/`: nuevos `Services/`, `Data/`, `Dtos/`, `Entities/` y controladores (`Contacts`, `Patients`, `Gestors`); paquete EF Core SqlServer agregado al proyecto.
- `scripts/`: nace `001_schema.sql` (se crea la base `TbtbChallenge`).
- `web/`: nuevo formulario y servicios inyectados de consumo del API.
- Base de datos local: contenedor SQL Server ya disponible (`docker-compose.yml` del repo).
