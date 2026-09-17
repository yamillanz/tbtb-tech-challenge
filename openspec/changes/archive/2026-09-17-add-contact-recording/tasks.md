## 1. Esquema de base de datos

- [x] 1.1 Escribir `scripts/001_schema.sql`: crear base `TbtbChallenge`, tablas `Patients`, `Gestors`, `Contacts`, `ContactAmendments` con claves, `CHECK` de catálogos, únicos (tipo+número de documento) e índices del plan (sección 3), con guardas `IF NOT EXISTS`
- [x] 1.2 Ejecutar el script contra el contenedor local y verificar tablas y restricciones

## 2. Acceso a datos

- [x] 2.1 Agregar paquete `Microsoft.EntityFrameworkCore.SqlServer` al proyecto de la API
- [x] 2.2 Crear entidades escritas a mano (`Patient`, `Gestor`, `Contact`, `ContactAmendment`) mapeando las tablas existentes
- [x] 2.3 Crear `TbtbChallengeDbContext` con `OnModelCreating` mínimo (tablas, claves, relaciones) y registro con cadena de conexión por configuración (variable de entorno o user-secrets, sin secretos en el código)

## 3. Dominio y frontera

- [x] 3.1 Crear DTOs en la frontera: `CreateContactRequest`, `ContactDto`, `PatientOptionDto`, `GestorOptionDto`
- [x] 3.2 Crear `ContactService` con la validación de dominio (catálogos de canal y resultado, fecha dentro del mes en curso, existencia de paciente y gestor) y la persistencia del contacto
- [x] 3.3 Crear `IExceptionHandler` que mapee `ValidationException` → 400 ProblemDetails y `KeyNotFoundException` → 404, con mensajes en español

## 4. API

- [x] 4.1 `POST /api/contacts` en `ContactsController`: entrada `CreateContactRequest`, salida `ContactDto` (201), delega en el servicio
- [x] 4.2 `GET /api/patients` y `GET /api/gestors` devolviendo las listas ordenadas por nombre
- [x] 4.3 Verificar manualmente con peticiones HTTP locales los escenarios del spec (201, 400 y 404)

## 5. Prueba del criterio

- [x] 5.1 Crear proyecto xUnit `api/tests/TbtbChallenge.Api.Tests` con referencia a la API y a la solución `api/TbtbChallenge.sln`
- [x] 5.2 Escribir `RegistrarContacto_CuandoElGestorCompletaElFormulario_ElContactoQuedaAsociadoAlPaciente`: crea paciente y gestor de arregle, registra el contacto y verifica la asociación con fecha, canal y resultado contra SQL Server, con limpieza posterior
- [x] 5.3 Ejecutar `dotnet test` y dejar la prueba en verde

## 6. Pantalla Angular

- [x] 6.1 Crear `ContactsService` inyectado (HttpClient) con `registrarContacto`, `listarPacientes` y `listarGestores`
- [x] 6.2 Crear componente standalone tipado del formulario "Registrar contacto" (reactive forms, selects de paciente y gestor, canal y resultado desde catálogos, fecha) con `ChangeDetectionStrategy.OnPush` y ruta `/contactos/nuevo`
- [x] 6.3 Mostrar el error de la API junto al campo correspondiente sin perder lo cargado (caso de error visible de punta a punta)
- [x] 6.4 Verificar el flujo completo contra el contenedor local: registrar un contacto real y verlo persistido
