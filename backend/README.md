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
| `src/Terrun.Infrastructure` | `TerrunDbContext` (snake_case, PostGIS), configuraciones de EF y migraciones (desde CTZ-13) |
| `src/Terrun.Engine` | Motor puro (geo, métricas, antitrampas, territorio, progresión): sin EF, sin HTTP |
| `tests/Terrun.Engine.Tests` | Pruebas unitarias del motor y regla de dependencias del Engine |
| `tests/Terrun.Api.Tests` | Pruebas de integración con Testcontainers y reglas de dependencias entre capas |

Las versiones de NuGet se definen solo en `Directory.Packages.props`; cada paquete nuevo necesita una línea en `docs/DECISIONES.md`.
