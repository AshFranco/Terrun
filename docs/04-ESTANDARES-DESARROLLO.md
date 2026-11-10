# 04 — Estándares de desarrollo

Reglas de código para todo el repositorio. Son de **cumplimiento obligatorio**: un PR que no las respete se devuelve. Lo que se puede automatizar lo verifican el formateador, el linter y la CI; el resto se revisa en el PR.

Si una regla no tiene sentido en un caso concreto, se discute en el PR y, si se acepta la excepción, se registra en `docs/DECISIONES.md`.

## 1. Idioma

### 1.1 Regla general

> **Todo lo técnico se escribe en inglés. Lo que leen las personas del equipo o los usuarios, en español.**

| Qué | Idioma | Ejemplo |
|---|---|---|
| Identificadores (clases, métodos, variables, archivos, carpetas) | Inglés | `ActivityService`, `calculateOwnershipAsync()`, `distanceM` |
| Base de datos (tablas, columnas, enums, índices) | Inglés | `activity`, `zone_ownership`, `created_at` |
| API (rutas, JSON, valores de enums, parámetros) | Inglés | `GET /zones?bbox=`, `"status": "owned_by_me"` |
| Códigos de error de la API | Inglés, `UPPER_SNAKE_CASE` | `ACTIVITY_OVERLAP` |
| Comentarios en el código | Inglés | `// Discarded: accuracy above threshold (LLD §4.1)` |
| Mensajes de log | Inglés | `"Activity {ActivityId} processed in {Ms} ms"` |
| Commits, nombres de ramas y títulos de PR | Inglés | `feat(territory): conquer zone at 5 of 7 cells` |
| Descripción de PR, issues, revisiones | Español | "Qué resuelve", "Cómo probarlo" |
| Documentación en `docs/` (HLD, LLD, backlog, bitácora…) | Español | Este archivo |
| Textos que ve el usuario | Español, en el archivo de traducciones (§5) | `t('zone.conquered')` → "Zona conquistada" |

### 1.2 Inglés estándar

- Inglés de Estados Unidos: `color`, `center`, `meters`, `behavior`.
- Palabras completas y no abreviaturas inventadas: `distance`, no `dist`; `activity`, no `act`. Abreviaturas aceptadas: `id`, `url`, `dto`, `api`, `db`, `gps`, `xp`, `bbox`, `h3`, `min`, `max`, `avg`.
- Términos del mundo del running tal como los usan Strava, Garmin y similares: `activity`, `pace`, `split`, `elevation gain`, `moving time`, `elapsed time`, `kudos`, `feed`, `leaderboard`, `streak`, `personal record`.

### 1.3 Glosario del dominio (nombres oficiales)

Un concepto tiene **un solo nombre** en todas las capas. No se inventan sinónimos (`Run`, `Workout`, `Hex`, `Territory` como entidad…). Si aparece un concepto nuevo, se agrega aquí **antes** de usarlo.

| Concepto (español, en docs) | Nombre en el código | Tabla | JSON |
|---|---|---|---|
| Usuario / corredor | `User` | `app_user` | `user` |
| Actividad (carrera, trote, caminata) | `Activity` (`ActivityType`: `run`, `jog`, `walk`) | `activity` | `activity` |
| Punto GPS | `GpsPoint` | `gps_point` | `points` |
| Split por km | `Split` | `split` | `splits` |
| Récord personal | `PersonalRecord` | `personal_record` | `personalRecords` |
| Zapatilla | `Shoe` | `shoe` | `shoe` |
| Zona (H3 res 9) | `Zone` | `zone` | `zone`, `h3` |
| Sub-celda (H3 res 10) | `Cell` | `cell_visit.cell_h3` | `cell` |
| Visita a una sub-celda | `CellVisit` | `cell_visit` | — |
| Progreso en una zona (máscara 7 bits) | `ZoneProgress` | `zone_progress` | `myProgress` |
| Conquista | `Conquest` | `conquest` | `conquest` |
| Dominio de una zona | `ZoneOwnership` | `zone_ownership` | `ownership` |
| Dueño de una zona | `Owner` | `owner_id` | `owner` |
| Robo de una zona | `Steal` (evento `stolen`) | `zone_event` | `stolen` |
| Puntos de zona | `ZonePoints` | — (se calcula) | `points` |
| Zona de privacidad | `PrivacyZone` | `privacy_zone` | `privacyZones` |
| Seguimiento | `Follow` | `follow` | `follow` |
| Club | `Club` | `club` | `club` |
| Logro | `Achievement` | `achievement` | `achievement` |
| Reto | `Challenge` | `challenge` | `challenge` |
| Racha | `Streak` | `user_stats.current_streak_weeks` | `streak` |
| Experiencia y nivel | `Xp`, `Level` | `xp_transaction`, `user_stats` | `xp`, `level` |
| Ranking | `Leaderboard` | `leaderboard_snapshot` | `leaderboard` |
| Parámetro del sistema | `SystemSetting` | `system_setting` | `settings` |
| Motor de territorio | `TerritoryEngine` (proyecto `Engine`) | — | — |

## 2. Nombres por capa

### 2.1 C# (.NET 10)

| Elemento | Convención | Ejemplo |
|---|---|---|
| Clases, records, enums, propiedades, métodos | `PascalCase` | `class Activity`, `DistanceM`, `CalculateSplits()` |
| Interfaces | `I` + `PascalCase` | `ITerritoryService` |
| Parámetros y variables locales | `camelCase` | `activityId`, `validPoints` |
| Campos privados | `_camelCase` | `_clock`, `_db` |
| Constantes | `PascalCase` | `const int CellsPerZone = 7;` (solo constantes matemáticas o de H3, nunca reglas del juego) |
| Métodos asíncronos | Sufijo `Async` | `ProcessActivityAsync()` |
| Booleanos | Prefijo `Is`, `Has`, `Can` | `IsPublic`, `HasPhoto`, `IsConquered` |
| DTO de entrada | `<Verb><Entity>Request` | `CreateActivityRequest`, `UpdateProfileRequest` |
| DTO de salida | `<Entity>Response` / `<Entity>DetailResponse` | `ZoneResponse`, `ActivityDetailResponse` |
| Servicios, controladores, validadores | `<Entity>Service`, `<Entities>Controller`, `<Request>Validator` | `ClubService`, `ClubsController`, `CreateClubRequestValidator` |
| Trabajos en segundo plano | `<Name>Job` | `NightlyOwnershipJob` |
| Migraciones de EF | `PascalCase` descriptivo | `AddTerritoryTables` |
| Namespaces | Siguen las carpetas | `Terrun.Api.Modules.Territory` |

### 2.2 TypeScript / React Native

| Elemento | Convención | Ejemplo |
|---|---|---|
| Variables y funciones | `camelCase` | `distanceM`, `formatPace()` |
| Componentes | `PascalCase`, archivo con el mismo nombre | `ActivityCard.tsx` |
| Hooks | `use` + `PascalCase` | `useRecording`, `useVisibleZones` |
| Tipos e interfaces | `PascalCase`, sin prefijo `I` | `type RecordingState = ...` |
| Constantes de módulo | `UPPER_SNAKE_CASE` | `GPS_INTERVAL_MS` (solo valores técnicos; las reglas del juego vienen de `/settings/public`) |
| Booleanos | Prefijo `is`, `has`, `can` | `isRecording`, `hasConnection` |
| Manejadores de eventos | `handle` + acción (props: `on` + acción) | `handleStartPress`, `onZoneSelect` |
| Archivos que no son componentes | `camelCase.ts` | `uploadQueue.ts`, `formatPace.ts` |
| Rutas de Expo Router | `kebab-case` | `app/settings/privacy-zones.tsx` |
| Pruebas | Junto al archivo: `<file>.test.ts(x)` | `formatPace.test.ts` |

### 2.3 Base de datos (PostgreSQL)

| Elemento | Convención | Ejemplo |
|---|---|---|
| Tablas | `snake_case`, **singular** | `activity`, `club_member` |
| Columnas | `snake_case` | `distance_m`, `created_at` |
| Claves foráneas | `<table>_id` | `user_id`, `zone_h3` (excepción: claves H3) |
| Fechas y horas | Sufijo `_at` (`timestamptz`, UTC); solo fecha: sufijo `_date` | `created_at`, `visit_date` |
| Unidades | Sufijo con la unidad | `_m`, `_s`, `_s_per_km`, `_kmh`, `_mps`, `_kg`, `_cm` |
| Booleanos | Prefijo `is_` / `has_` | `is_active`, `is_private` |
| Restricciones e índices | `pk_`, `fk_`, `uk_`, `ck_`, `ix_` + tabla + columnas | `uk_app_user_email`, `ix_activity_user_id_started_at` |
| Tipos enum | `snake_case` | `activity_status` con valores `pending`, `processed` |
| Palabras reservadas | Se evitan (`user`, `order`, `date`) | `app_user`, `sort_order`, `visit_date` |

EF Core se configura con `UseSnakeCaseNamingConvention()`: `DistanceM` en C# se convierte en `distance_m`.

### 2.4 API REST

| Elemento | Convención | Ejemplo |
|---|---|---|
| Rutas | Sustantivos en plural, `kebab-case` | `/privacy-zones`, `/activities/{id}/splits` |
| Recursos del usuario actual | Prefijo `/me` | `/me/conquests` |
| Acciones que no son CRUD | Sub-recurso con verbo | `/auth/reset-password`, `/admin/reports/{id}/resolve` |
| Parámetros de consulta y JSON | `camelCase` | `?bbox=...&limit=20`, `"clientId"` |
| Valores de enums en JSON | `snake_case` | `"status": "owned_by_me"` |
| Fechas | ISO 8601 en UTC | `"2026-11-21T10:02:11Z"` |
| Unidades | SI con la unidad en el nombre | `"distanceM": 5230.4`, `"durationS": 1800` |

## 3. Reglas de código (backend y app)

1. **Ningún número del juego en el código.** Umbrales, ventanas, XP y límites antitrampas se leen de `system_setting` (backend) o de `/settings/public` (app). Los literales solo se permiten para constantes matemáticas o técnicas con nombre (`MetersPerKilometer = 1000`).
2. **Unidades del SI por dentro y formato solo en la pantalla.** Se guardan y transmiten metros, segundos y UTC. La conversión a km, min/km o la hora local se hace solo en la capa de presentación (`mobile/src/utils/format*.ts`).
3. **Funciones cortas y con un solo propósito.** Como guía, menos de 40 líneas por función y menos de 300 por archivo.
4. **Sin código muerto ni comentado.** Para eso está git.
5. **Comentarios para explicar el *porqué*, no el *qué*.** Las reglas de dominio no obvias llevan referencia: `// LLD §4.3: a tie keeps the current owner`.
6. **Sin `TODO` ni `FIXME` en `main`.** Lo pendiente se convierte en issue.
7. **Errores explícitos.** Nada de `catch` vacíos. El backend lanza excepciones de dominio que el middleware convierte en `ProblemDetails` con su `code` (LLD §3.4); la app traduce cada `code` a un mensaje en español.
8. **Nada de datos personales en los logs:** ni coordenadas, ni correos, ni tokens. Se registran IDs.
9. **Fechas siempre con zona.** `DateTimeOffset` en C#, cadenas ISO con `Z` en TypeScript. Nunca `DateTime.Now`: se usa `IClock`.
10. **Dependencias nuevas con justificación.** Agregar un paquete de NuGet o npm exige una línea en `docs/DECISIONES.md` (qué es, por qué y qué alternativa se descartó).

## 4. Backend (.NET)

- `Nullable` activado y `TreatWarningsAsErrors` en todos los proyectos (`Directory.Build.props`).
- Formato con `dotnet format`, que respeta `.editorconfig`; la CI lo verifica.
- **Controladores delgados:** reciben, validan (FluentValidation) y delegan en un servicio. Ninguna consulta a la BD dentro de un controlador.
- **Lecturas con `AsNoTracking()`** y proyección a DTO con `Select`. Nunca se devuelven entidades de EF por la API.
- **Toda lista se pagina** (cursor, máximo 50).
- **Sin consultas N+1.**
- **Transacciones** explícitas en el procesamiento de actividades (LLD §4.1); fuera de eso, un `SaveChangesAsync` por solicitud.
- **`CancellationToken`** en todo método asíncrono que llegue a la BD o a HTTP.
- **El proyecto `Engine` es puro:** sin EF, sin HTTP, sin `DateTime.Now`. Recibe datos y devuelve resultados, por eso se prueba sin base de datos.

## 5. App (React Native + Expo)

- TypeScript en modo `strict`; prohibido `any` (se usa `unknown` y se valida).
- ESLint + Prettier, verificados por la CI. Sin advertencias.
- **Las pantallas no llaman a `fetch`.** Usan hooks de `src/api` construidos con TanStack Query y los tipos generados desde el OpenAPI.
- **Sin colores, tamaños ni fuentes sueltos en las pantallas.** Todo sale de los tokens del tema (`src/theme/`).
- **Ningún texto visible escrito en el componente.** Todos los textos van en `src/i18n/es.json` y se usan con `t('key')`. Las claves, en inglés y con puntos: `activity.summary.title`. Esto deja la app lista para traducirla al inglés cuando se comercialice.
- **Estado global mínimo** (Zustand): sesión y preferencias. El resto es estado del servidor (TanStack Query) o local del componente.
- **Accesibilidad:** áreas táctiles de al menos 44×44 pt, `accessibilityLabel` en botones con solo ícono, contraste AA.
- **Listas** con `FlatList` o `FlashList`; nunca `map` dentro de un `ScrollView` con datos sin límite.

## 6. Pruebas

| Regla | Detalle |
|---|---|
| Nombre de las pruebas (C#) | `Method_Scenario_ExpectedResult`: `Conquer_FiveOfSevenCells_CreatesConquest` |
| Nombre de las pruebas (TS) | `describe('formatPace')` + `it('returns 5:30 for 330 seconds')` |
| Estructura | Arrange / Act / Assert, separados por una línea en blanco |
| Casos del motor | Cada caso MT del LLD §9 tiene su prueba con el ID en un atributo: `[Trait("Case", "MT-07")]` |
| Datos | Fábricas de datos de prueba; nunca coordenadas de la casa de nadie |
| Independencia | Ninguna prueba depende del orden ni de otra prueba |
| Integración | Contra PostgreSQL + PostGIS real (Testcontainers), nunca contra una BD en memoria |

## 7. Formato automático

| Herramienta | Alcance | Dónde |
|---|---|---|
| `.editorconfig` | Todo el repo: UTF-8, LF, indentación, espacios finales, nombres de C# | Raíz |
| `dotnet format` | Backend | CI y antes del commit |
| Prettier + ESLint | App | CI y antes del commit |

Configurar el editor para que formatee al guardar. Un PR con cambios solo de formato en archivos ajenos se rechaza: el formato se aplica a lo que uno toca.
