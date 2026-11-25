# Estado actual del proyecto

> **Sprint en curso: S1 — Cuenta, perfil y prueba de GPS** · del sábado 14 al viernes 20 de noviembre de 2026
> **Weekly:** viernes 20 de noviembre · **Hito:** GPS en segundo plano funcionando con la pantalla bloqueada en Android, viernes 20 de noviembre
> **Última actualización:** 20/11/2026 — Rowlis

Tablero: [Terrun — Backlog y Sprints](https://github.com/users/AshFranco/projects/4) · columnas Pendiente / En curso / En revisión / Hecha.

Leyenda: ⬜ pendiente · 🔄 en curso · 👀 en revisión (PR #n) · ✅ hecha · ⛔ bloqueada (motivo)

Detalle y criterios de aceptación de cada historia: `docs/03-BACKLOG.md`. Compromiso y riesgos del sprint: `docs/sprints/S1.md`. QA: https://terrun-api-qa.onrender.com (`/health`, `/swagger`).

## Tareas por persona

Cada lista está en el orden en que conviene hacerla: primero lo que bloquea a otros.

### Ashley — Scrum Master + Backend
- ✅ **CTZ-18** Despliegue de QA: Render + Neon `qa`, despliegue automático al hacer merge a `development` (D-27). *Adelantado en CTZ-04 (D-21): `/health` = Healthy y Swagger visible.*
- 👀 **CTZ-13** Migración inicial: tablas de M1, M2 y M13 + datos iniciales de `system_setting` y `legal_document`. *Bloquea CTZ-14 y CTZ-17.*
- 👀 **CTZ-14** Registro, verificación de correo, login, refresh rotativo y logout. *Depende de CTZ-13. Bloquea CTZ-15, CTZ-16 y CTZ-23.* *En revisión (PR #124).*
- 👀 **CTZ-16** `GET /me` y `PATCH /me/profile`. *Depende de CTZ-14. Bloquea CTZ-19 y CTZ-24.* *En revisión (PR #138); contrato en LLD §3.3 (v1.12, D-32). Pendientes: #135, #136, #137.*
- 👀 **CTZ-106** Respuestas de error en el OpenAPI (`ApiProblem` + `[ApiError]`, D-33). *En revisión (PR #139). Va antes de CTZ-19 para que los tipos traigan la forma del error (issue #136).*
- ⬜ **CTZ-19** OpenAPI publicado y tipos de la app (`npm run api:types`), con Rowlis. *Depende de CTZ-16, CTZ-106 y CTZ-08.*
- 🔄 **CTZ-32** (S2) `GET`/`PATCH`/`DELETE /activities/{id}`, `/route`, `/splits`, `/me/activities` y `/me/summary` (D-38). *Desbloquea CTZ-34, CTZ-35 y CTZ-36. Issue #41.*
- 👀 **CTZ-31b** (S2) Métricas oficiales, splits, ruta simplificada, calorías, MT-05 y recorte de terceros en el motor (D-37). *En revisión (PR #144). Activa en producción el procesamiento. Issue #40.*
- 👀 **CTZ-31a** (S2) Limpieza de GPS y antitrampas: motor puro, etapa `CleaningStep`, `activity_review` y casos MT en `docs/qa/04-CASOS/` (D-36). *En revisión (PR #143). La etapa se activa con CTZ-31b. Issue #39.*
- 👀 **CTZ-104** (S2) Programador de trabajos: `job_run`, `POST /internal/jobs/run`, cola en memoria, barrido al arrancar y `jobs.yml` cada hora (D-35). *En revisión (PR #141). Issue #122.*
- 👀 **CTZ-30** (S2) Tablas M3/M4 y `POST /activities` idempotente, con límite de puntos y sin superposición (D-34). *En revisión (PR #140). Bloquea CTZ-31a, CTZ-32 y CTZ-34 (Jorge). Issue #38.*
- ⬜ **CTZ-105** Foto de perfil: bucket R2 y `POST /me/photo/upload-url` (issue #135). *Sprint por decidir (Diana). La necesita CTZ-24.*
- Revisar los PR de backend de Rowlis: CTZ-15 y CTZ-17.

### Diana — Product Owner
- ✅ **CTZ-27** Términos y política de privacidad v1.0 en `docs/producto/legal/`. *Mergeado (PR #128), revisado por Ashley; vigencia del 17/11 cargada en la semilla (D-29).*
- ✅ **CTZ-25** Analizar las 28 respuestas de la encuesta y confirmar el orden de los P1. *Mergeado (PR #130), revisado por Ashley; orden de los P1 en D-31.*
- ✅ **CTZ-26** Criterios de aceptación detallados de S2 y S3 (*Definition of Ready* el viernes 20) en `docs/producto/criterios-S2-S3.md`. *Mergeado (PR #133), revisado por Ashley; S2 y S3 cumplen la Definition of Ready.*

### Jorge — Front, experiencia de usuario y núcleo técnico
- ⬜ **CTZ-10** Sistema de diseño base + wireframes de P-01 a P-09. *Viene de S0. Lo usan las pantallas de Rowlis (CTZ-23 y CTZ-24).*
- ⬜ **CTZ-09** Configuración de `expo-location` + `TaskManager` y MapLibre RN en un *development build*, con Rowlis. *Viene de S0. Depende de CTZ-08.*
- ⬜ **CTZ-20** Prueba de concepto del GPS en segundo plano (Android): 30 min con la pantalla bloqueada. *Depende de CTZ-08. Es el hito del 20/11 y bloquea CTZ-29.*
- ⬜ **CTZ-21** Mapa base MapLibre en la pestaña Mapa. *Depende de CTZ-08.*

### Rowlis — Front y apoyo puntual en Backend
- 👀 **CTZ-08** Esqueleto de la app Expo + TypeScript + Expo Router + lint + Jest. *Viene de S0. En revisión (PR #131). El ID del proyecto de Expo está en el issue #15.*
- ⬜ **CTZ-09** Configuración de GPS y MapLibre, con Jorge. *Viene de S0. Depende de CTZ-08.*
- ⬜ **CTZ-22** Navegación, cliente API y sesión en `SecureStore` con refresh automático. *Depende de CTZ-08. Bloquea CTZ-23.*
- ⬜ **CTZ-17** `/legal/current`, `/me/consents` y `/settings/public` (revisa Ashley). *Desbloqueada: CTZ-13 y CTZ-27 hechas.*
- ⬜ **CTZ-23** Pantallas P-01 a P-04 contra QA. *Depende de CTZ-22 y CTZ-14.*
- ⬜ **CTZ-24** Pantalla P-17 perfil propio. *Depende de CTZ-16.*
- ⬜ **CTZ-19** Tipos de la app desde el OpenAPI, con Ashley.
- ⬜ **CTZ-15** (P1) Recuperar y restablecer contraseña (revisa Ashley). *Depende de CTZ-14.*
- ⬜ **CTZ-23b** (P1) Pantalla P-05 recuperar contraseña. *Depende de CTZ-23 y CTZ-15.*

### Rangelis — QA
- ⬜ **CTZ-11** Plan de pruebas v1 en `docs/qa/01-PLAN-PRUEBAS.md`. *Viene de S0.*
- ⬜ **CTZ-28** Casos de M1/M2 y colección de API (Bruno) contra QA; registro de defectos activo. *QA ya está en línea (CTZ-18).*

### Jaime — QA
- ⬜ **CTZ-12** Plan de pruebas de campo: dispositivos y rutas medidas en `docs/qa/03-PRUEBAS-CAMPO.md`. *Viene de S0. Lo necesita CTZ-29.*
- ⬜ **CTZ-29** Prueba de campo del CTZ-20 en 2–3 teléfonos Android reales. *Depende de CTZ-20 y CTZ-12.*

## Bloqueos abiertos

| Desde | Bloqueo | Afecta a | Responsable de resolverlo |
|---|---|---|---|
| 15/11 | El esqueleto de la app (CTZ-08) no existe todavía | Jorge (CTZ-09, CTZ-20, CTZ-21), Rowlis (CTZ-22 a CTZ-24) y Jaime (CTZ-29) | Rowlis, antes del martes 17 |

## Avance del sprint

Las historias compartidas (CTZ-09 y CTZ-19) cuentan para las dos personas y una sola vez en el total del equipo.

| Persona | Hechas | Total |
|---|---|---|
| Ashley | 1 | 5 |
| Diana | 0 | 3 |
| Jorge | 0 | 4 |
| Rowlis | 0 | 9 |
| Rangelis | 0 | 2 |
| Jaime | 0 | 2 |
| **Equipo** | **1** | **23** |

## Próximo sprint (S2 — 21 al 27 de noviembre)

Actividades y trackeo deportivo (ver `docs/03-BACKLOG.md`, sección S2).
