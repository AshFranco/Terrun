# Terrun

**Cada carrera suma territorio.** App móvil para corredores: el mapa real está dividido en hexágonos; al correr conquistas zonas y, si las recorres más que nadie en los últimos 30 días, las dominas.

Proyecto final de la asignatura **Construcción de Software** · Entrega: 20 de enero de 2027.

| Integrante | Rol |
|---|---|
| Ashley | Scrum Master + Backend |
| Diana | Product Owner |
| Jorge | Front — núcleo técnico |
| Rowlis | Front — experiencia + backend puntual |
| Rangelis | QA |
| Jaime | QA |

## Empezar

Con Claude Code: sigue [`docs/05-PRIMER-DIA.md`](docs/05-PRIMER-DIA.md), que clona, configura y revisa tu entorno en una sesión.

A mano:

```bash
git clone git@github.com:AshFranco/Terrun.git terrun
cd terrun
./scripts/configurar-entorno.sh
```

Después lee, en este orden:

1. [`docs/00-GUIA-DE-TRABAJO.md`](docs/00-GUIA-DE-TRABAJO.md) — cómo trabaja el equipo.
2. [`docs/ESTADO.md`](docs/ESTADO.md) — qué toca ahora y a quién.
3. [`CONTRIBUTING.md`](CONTRIBUTING.md) — ramas, commits, PR, Definition of Ready y Definition of Done.

## Documentación

| Documento | Contenido |
|---|---|
| [`docs/01-HLD.md`](docs/01-HLD.md) | Diseño de alto nivel (PDF en `docs/entregables/`) |
| [`docs/02-LLD.md`](docs/02-LLD.md) | Diseño de bajo nivel: tablas, API, algoritmos, pantallas |
| [`docs/03-BACKLOG.md`](docs/03-BACKLOG.md) | Historias CTZ-01 a CTZ-91 por sprint y responsable |
| [`docs/04-ESTANDARES-DESARROLLO.md`](docs/04-ESTANDARES-DESARROLLO.md) | Reglas de código: idioma, nombres, glosario, pruebas |
| [`docs/05-PRIMER-DIA.md`](docs/05-PRIMER-DIA.md) | Texto para que Claude Code prepare tu entorno el primer día |
| [`docs/BITACORA.md`](docs/BITACORA.md) | Histórico de trabajo |
| [`docs/DECISIONES.md`](docs/DECISIONES.md) | Decisiones del proyecto |
| [`docs/sprints/`](docs/sprints/) | Compromiso y resultado de cada sprint |
| [`docs/qa/`](docs/qa/) | Plan de pruebas, defectos, pruebas de campo |

## Stack

App React Native + Expo (TypeScript) · API .NET 10 + EF Core · PostgreSQL + PostGIS · H3 · MapLibre · Render · Neon · Expo EAS.

## Regenerar el HLD y el LLD

```bash
uv run --with matplotlib python scripts/generar_diagramas.py
uv run --with python-docx python scripts/generar_documentos.py
soffice --headless --convert-to pdf --outdir docs/entregables docs/entregables/*.docx
```
