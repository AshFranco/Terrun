# Backend — API .NET 10

Responsable: **Ashley** (Rowlis en funciones puntuales). Estructura, reglas de dependencia y patrones: `docs/02-LLD.md` §1.2. Reglas de código: `docs/04-ESTANDARES-DESARROLLO.md`.

## Requisitos

- .NET SDK 10.0.400 o superior (`global.json`).
- Docker Desktop (PostGIS local y pruebas de integración con Testcontainers).

Todos los comandos se corren **desde esta carpeta** (`backend/`): ahí está el `global.json` que activa el SDK y el motor de pruebas correctos.

## Levantar todo con Docker

```bash
docker compose up -d --build
```

| Servicio | URL |
|---|---|
| Salud (API + BD) | http://localhost:5080/health |
| Swagger | http://localhost:5080/swagger |
| OpenAPI 3.1 | http://localhost:5080/openapi/v1.json |
| PostgreSQL + PostGIS | `localhost:5432`, base `terrun`, usuario `terrun`, clave `terrun_local` |

La imagen oficial de PostGIS solo existe para amd64; en Mac con Apple Silicon corre emulada (el primer arranque tarda un poco más). Para apagar: `docker compose down` (agrega `-v` para borrar los datos).

## Correr la API sin Docker

Con la base del compose arriba (`docker compose up -d db`):

```bash
dotnet run --project src/Terrun.Api
```

En `Development` la cadena de conexión sale de `appsettings.Development.json`. En QA y producción se pasa con la variable `ConnectionStrings__Db` (LLD §8); nunca se escribe en el repo.

**QA:** https://terrun-api-qa.onrender.com (`/health`, `/swagger`). Se despliega solo con cada merge a `main` y usa la rama `qa` de Neon (D-21). El plan gratis de Render se duerme tras unos minutos sin tráfico: la primera llamada puede tardar hasta un minuto.

## Migraciones de la base

La API aplica sola las migraciones pendientes al arrancar (`Database:MigrateOnStartup`, activado en `appsettings.json`): con `docker compose up`, en QA y en producción la base queda al día sin pasos manuales (D-22). Al arrancar contra una base vacía, EF registra una vez `Failed executing DbCommand ... __EFMigrationsHistory` porque consulta el historial antes de crearlo; es esperado y enseguida aplica la migración.

Para cambiar el modelo (entidades en `src/Terrun.Domain`, configuraciones en `src/Terrun.Infrastructure/Persistence/Configurations`):

```bash
dotnet tool restore                      # instala dotnet-ef en la versión del repo (dotnet-tools.json)
dotnet ef migrations add AddTerritoryTables --project src/Terrun.Infrastructure --startup-project src/Terrun.Infrastructure --output-dir Persistence/Migrations
dotnet ef migrations script --project src/Terrun.Infrastructure --startup-project src/Terrun.Infrastructure   # revisar el SQL antes del commit
```

El nombre de la migración va en `PascalCase` descriptivo (estándares §2.1). Si el modelo cambia sin migración, la prueba `Model_ComparedWithTheMigrations_HasNoPendingChanges` falla y la API no arranca.

## Pruebas y formato

```bash
dotnet test --solution Terrun.slnx       # unitarias, de arquitectura y de integración (PostGIS real)
dotnet format Terrun.slnx                 # aplica el formato del .editorconfig
```

La CI (`.github/workflows/backend.yml`) corre en cada PR: `dotnet format --verify-no-changes`, build en Release y pruebas. Un PR con formato incorrecto no se puede mergear.

## Estructura

| Proyecto | Contenido |
|---|---|
| `src/Terrun.Api` | Host: `Program.cs`, ProblemDetails, Swagger, módulos (`Modules/`) y trabajos en segundo plano (`Jobs/`, desde S4) |
| `src/Terrun.Domain` | Entidades, enums y excepciones de dominio |
| `src/Terrun.Infrastructure` | `TerrunDbContext` (snake_case, PostGIS), configuraciones de EF, semilla de `system_setting` y `legal_document`, y migraciones (`Persistence/`) |
| `src/Terrun.Engine` | Motor puro (geo, métricas, antitrampas, territorio, progresión): sin EF, sin HTTP |
| `tests/Terrun.Engine.Tests` | Pruebas unitarias del motor y regla de dependencias del Engine |
| `tests/Terrun.Api.Tests` | Pruebas de integración con Testcontainers y reglas de dependencias entre capas |

Las versiones de NuGet se definen solo en `Directory.Packages.props`; cada paquete nuevo necesita una línea en `docs/DECISIONES.md`.
