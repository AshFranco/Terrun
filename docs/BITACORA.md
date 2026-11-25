# Bitácora del proyecto

Registro histórico de lo que se hace en el proyecto. **Las entradas nuevas van al principio.** Una entrada por sesión de trabajo (no por commit).

Formato:

```text
## AAAA-MM-DD — Persona — CTZ-XX título corto
- Hecho: qué se hizo, en concreto.
- Evidencia: PR #, commit, resultado de pruebas, captura o archivo.
- Decisiones: (si hubo) → registradas en DECISIONES.md como D-NN.
- Siguiente paso: qué queda y quién lo hace.
```

---

## 2026-11-24 — Ashley — CTZ-31a Limpieza de GPS y antitrampas
- Hecho: motor puro `Terrun.Engine/Tracks` (`Geo.HaversineM`, `TrackCleaner` y `AntiCheatJudge`): descarta puntos por precisión, tiempo repetido o hacia atrás y saltos; mide los tramos entre puntos conservados del mismo segmento; marca inválidos los tramos sobre `anticheat.max_speed_kmh`; calcula la distancia válida e inválida y el tiempo en movimiento; y emite las violaciones `min_pace` e `invalid_distance`. Etapa `CleaningStep`: lee los límites de `system_setting` en una consulta, marca `gps_point.is_discarded` y `discard_reason`, y deja la actividad `in_review` (`status_reason = anticheat`) con su `activity_review`. Tabla `activity_review` y parámetros nuevos `anticheat.jump_window_s` y `anticheat.max_invalid_distance_pct`. Cálculos a mano de MT-01, MT-02, MT-03, MT-04 y MT-14 en `docs/qa/04-CASOS/`. LLD v1.16 con el entregable regenerado.
- Evidencia: 174 pruebas en verde (19 nuevas: 14 del motor con `[Trait("Case", "MT-NN")]` y 5 de la etapa contra PostGIS real); `dotnet format` limpio.
- Decisiones: D-36 (la etapa se activa junto con CTZ-31b y Ashley documenta los casos MT, ambas decididas por la usuaria).
- A tener en cuenta (quedan fuera de CTZ-31a):
  - **CTZ-31b:** registrar `CleaningStep` y su etapa de métricas en `ActivitiesModule` en el mismo PR. La prueba `Production_BeforeCtz31b_RegistersNoProcessingStage` va a fallar a propósito: hay que reemplazarla por una que compruebe las dos etapas. Las métricas usan `context.Cleaning` (solo tramos válidos); MT-05 (menos de `anticheat.min_duration_s`: `processed` sin territorio) es de CTZ-31b.
  - Al activarse, el procesador toma todas las actividades `pending` de QA, incluidas las de las pruebas de humo.
  - **MT-04 tal como está escrito** va a revisión por distancia inválida, no por ritmo: con 25 km/h de límite, la regla de ritmo mínimo solo salta sola entre 2:24 y 2:30 min/km. Está documentado en `docs/qa/04-CASOS/MT-04.md` y hay una prueba de ritmo solo.
  - **Rangelis (CTZ-38):** revisar los cálculos de `docs/qa/04-CASOS/`.
  - **Moderación en P2:** las actividades `in_review` solo se resuelven con CTZ-77 (cola de moderación), que es P2; en el MVP quedarían en revisión para siempre. Decisión pendiente de Diana (issue #142). P-09 (CTZ-34, Jorge) tiene que explicar en español que la actividad está en revisión.
  - Falta verificar en QA después del merge (migración de `activity_review` y de los 2 parámetros).
- Siguiente paso: PR #143 a `development` (revisa Jorge); después, CTZ-31b.

## 2026-11-24 — Ashley — CTZ-104 Programador de trabajos
- Hecho: tabla `job_run` (con `last_finished_at`); `JobScheduler` que ejecuta solo los trabajos vencidos según su cron (Cronos, hora AST) y los reserva con un `UPDATE` condicional y 30 min de reserva; `POST /api/v1/internal/jobs/run` con `X-Jobs-Key` (401 sin clave, con una clave incorrecta o si `Jobs__Key` no está configurada; fuera del OpenAPI); workflow `.github/workflows/jobs.yml` cada hora en el minuto 17, que espera hasta 3 min el arranque en frío. Cola en memoria (`ActivityQueue`) que `POST /activities` alimenta, `ActivityQueueWorker` que barre las `pending` al arrancar y procesa cada subida, y `ActivityProcessor` por etapas (SKIP LOCKED, una transacción, 3 intentos y luego `failed`), sin etapas registradas hasta CTZ-31a (D-35). Trabajos registrados: `activity_processor` y `token_cleanup`. LLD v1.15 con el entregable regenerado.
- Corrección de CTZ-30 (ya en QA): las subidas de un mismo corredor se serializan con `pg_advisory_xact_lock`. Al probar aparecieron dos carreras: subidas superpuestas simultáneas podían caer en un *deadlock* de la restricción `EXCLUDE` (500 en vez de 409), y un reintento simultáneo del mismo `clientId` podía recibir 409 en vez de 200. La prueba de concurrencia ahora hace 3 rondas de 8 subidas; 0 fallas en 8 corridas.
- Evidencia: 155 pruebas en verde (20 nuevas: `JobSchedulerTests`, `JobsEndpointTests`, `TokenCleanupJobTests`, `ActivityProcessingTests`), y las clases nuevas repetidas 3 veces sin fallas; `dotnet format` limpio; YAML del workflow validado.
- Decisiones: D-35 (tubería sin etapas decidida por la usuaria).
- A tener en cuenta (quedan fuera de CTZ-104):
  - **Configurado el 24-nov antes del merge:** `Jobs__Key` en Render (`terrun-api-qa`), el secreto `JOBS_KEY` (el mismo valor) y la variable `JOBS_API_URL` en el repositorio. `jobs.yml` corre cada hora apenas llega a `development` (rama por defecto); si alguna vez se cambia la clave, hay que cambiarla en los dos lados.
  - **CTZ-31a:** registrar su etapa en `ActivitiesModule` (`services.AddScoped<IActivityProcessingStep, ...>()`); con eso las actividades pasan a procesarse solas, incluidas todas las `pending` que haya en QA. La etapa usa `context.Db` (misma transacción) y fija `context.Activity.Status` si el veredicto no es `processed`.
  - **Jorge (CTZ-34):** hasta CTZ-31a, las subidas quedan en `pending`; P-09 tiene que tolerar ese estado.
  - Producción (S4): `jobs.yml` solo despierta QA; producción necesita su propia URL y clave.
  - GitHub desactiva los workflows programados tras 60 días sin actividad en el repositorio; después de la entrega hay que reactivarlo a mano si se sigue usando.
  - Falta verificar en QA después del merge (migración de `job_run` y una corrida manual del workflow).
- Siguiente paso: PR #141 a `development` (revisa Jorge); tras el merge, correr `jobs.yml` a mano para verificar.

## 2026-11-24 — Ashley — CTZ-30 Tablas M3/M4 y POST /activities
- Hecho: tablas `activity`, `gps_point`, `split`, `personal_record` y `shoe` con sus enums e índices; restricción `EXCLUDE` contra la superposición (extensión `btree_gist`); parámetro `activity.max_duration_h` = 24 (público). `POST /activities`: idempotente por `clientId` (200 con la existente), 413 si pasa `activity.max_points`, 409 `ACTIVITY_OVERLAP`, validación de tiempos y puntos, zapatilla propia, visibilidad tomada de las preferencias y puntos guardados con `COPY` binario en la misma transacción. Lector `SystemSettingsReader` (Platform) y `FieldValidationException` para errores por campo desde un servicio. Los `400` de valores no convertibles ahora salen por ruta de campo y sin nombres internos. LLD v1.14 con el entregable regenerado.
- Evidencia: 135 pruebas en verde (36 nuevas: `ActivityUploadTests`, `ActivityUploadSizeLimitTests` sobre Kestrel real, y 2 casos más en el OpenAPI), incluidas MT-09, subidas simultáneas con el mismo `clientId` y superpuestas, y 30.000 puntos guardados en pocos segundos; `dotnet format` limpio. Migración aplicada en una rama temporal de Neon copiada de `qa` (`btree_gist`, la restricción, las 5 tablas y 22 settings) y rama borrada.
- Decisiones: D-34 (duración máxima de 24 h y no exigir el correo verificado, decididos por la usuaria).
- A tener en cuenta (quedan fuera de CTZ-30):
  - **Para Jorge (CTZ-34, cola de subida):** `202` y `200` son éxito (el `200` es un reintento de una actividad ya guardada) y ambos traen `id` y `status`. Se reintenta con el mismo `clientId` solo ante error de red, `429` (respetar `Retry-After`) o `5xx`. `400`, `409 ACTIVITY_OVERLAP` y `413` son definitivos: sacarlos de la cola y avisar al corredor, porque reintentarlos nunca va a funcionar. Reglas que el tracker tiene que cumplir: `seq` creciente, `segment` desde 0 que sube en cada pausa, `recordedAt` entre `startedAt` y `endedAt`, máximo 24 h y 30.000 puntos (unos 4 MB; el límite del cuerpo es 8 MB).
  - **CTZ-104:** la subida deja la actividad `pending` pero todavía no la pone en la cola en memoria (`Channel`); eso y el arranque que procesa las `pending` rezagadas son parte de CTZ-104.
  - **CTZ-31a/31b:** los puntos llegan con `is_discarded = false`; los motivos de descarte son `accuracy`, `timestamp` y `jump`. Las métricas, la ruta, `elapsed_time_s` y los resultados quedan nulos hasta que los calcula el procesador.
  - **CTZ-32:** `GET /activities/{id}` todavía no existe; P-09 (CTZ-34) lo necesita para el sondeo.
  - **CTZ-17 (Rowlis):** puede usar `SystemSettingsReader` para `/settings/public`; `activity.max_duration_h` ya es público.
  - El 8 MB lo aplica Kestrel: si un cliente manda más sin `Expect: 100-continue`, ve un corte de conexión en vez del `413`. No le pasa a la app (30.000 puntos son unos 4 MB).
  - Falta verificar en QA después del merge (la migración se aplica al arrancar).
- Siguiente paso: PR #140 a `development` (revisa Jorge); después del merge, verificar en QA.

## 2026-11-24 — Ashley — CTZ-106 Respuestas de error en el OpenAPI
- Hecho: esquema `ApiProblem` (RFC 7807 + `code` como enum del catálogo §3.4 + `traceId` + `errors`) y transformador que declara en cada operación `400`, `429` y `500`; `413` y `415` si recibe cuerpo; `401` si está protegida. Atributo `[ApiError]` con los errores propios de `/auth/*` y `/me`. Regla agregada a los estándares §4, a la DoD y a la plantilla de PR; LLD v1.13 (§3.1) con el entregable regenerado.
- Evidencia: 99 pruebas en verde (13 nuevas en `OpenApiErrorResponsesTests`, incluidas las que comprueban que el 401 sin token y el 415 escriben de verdad el `ApiProblem` declarado); `dotnet format` limpio; tipos generados con `openapi-typescript` 7.13 desde el OpenAPI local compilan con `tsc --strict` y un `code` fuera del catálogo no compila.
- Decisiones: D-33.
- A tener en cuenta (quedan fuera de CTZ-106):
  - **Para Rowlis (CTZ-19 y CTZ-22): regenerar e integrar los tipos de error.**
    1. Después del merge, abrir `https://terrun-api-qa.onrender.com/health` y esperar `Healthy` (Render duerme tras 15 min), confirmar que `/openapi/v1.json` trae `ApiProblem` y correr en `mobile/`: `OPENAPI_URL=https://terrun-api-qa.onrender.com/openapi/v1.json npm run api:types`. Nunca editar `src/api/schema.d.ts` a mano.
    2. Exportar el tipo desde `src/api/index.ts`: `export type ApiProblem = components['schemas']['ApiProblem'];` y `export type ErrorCode = ApiProblem['code'];`.
    3. En el cliente de CTZ-22: toda respuesta no 2xx con `content-type: application/problem+json` se lee como `ApiProblem` y se lanza como un error propio (por ejemplo `ApiError` con `status`, `code`, `traceId` y `errors`). Las pantallas nunca leen el cuerpo crudo.
    4. Traducir por `code`, no por `status` ni por el texto: una clave `errors.<CODE>` en `src/i18n/es.json` por cada valor de `ErrorCode` (si se agrega un código al catálogo y falta su clave, que falle una prueba). `detail` es para desarrolladores y no se muestra.
    5. `VALIDATION_ERROR`: `errors` trae los mensajes por campo en `camelCase` (`handle`, `birthDate`…), con los mismos nombres de los campos del cuerpo; se usa para marcar el campo del formulario, con texto propio en español.
    6. `401 INVALID_TOKEN`: renovar una sola vez con `/auth/refresh` y repetir; si falla, cerrar sesión (criterio de CTZ-22). En `/auth/login` el 401 es `INVALID_CREDENTIALS` y no dispara el refresh.
    7. `429 RATE_LIMITED`: respetar el encabezado `Retry-After`. `500 INTERNAL_ERROR`: mensaje genérico y mostrar el `traceId` para que soporte encuentre el log.
    8. Los errores posibles de cada endpoint están en el OpenAPI (la descripción de cada respuesta lista los códigos); si el backend responde un código no declarado, es un defecto del backend: avisar a Ashley.
  - 405 (método no permitido) no se declara: la app genera sus llamadas desde el contrato y no usa métodos inexistentes.
  - `403 EMAIL_NOT_VERIFIED` aún no lo usa ningún endpoint; se declara con `[ApiError]` cuando alguno lo exija (regla de los estándares §4).
  - Todas las operaciones declaran 400, aunque sean GET sin parámetros (criterio del issue #136).
  - Falta verificar en QA que el OpenAPI desplegado trae `ApiProblem` (Ashley, tras el merge).
- Siguiente paso: PR #139 a `development` (revisa Rowlis); tras el merge, Rowlis genera los tipos en CTZ-19 desde QA siguiendo los pasos de arriba.

## 2026-11-24 — Ashley — CTZ-16 GET /me y PATCH /me/profile
- Hecho: módulo `Profiles/` con `GET /me` (usuario, perfil, preferencias y estadísticas en una consulta) y `PATCH /me/profile` con semántica de *JSON merge patch*; validaciones del perfil (edad mínima 18, peso, estatura, país ISO, longitudes); cambio de `@handle` con 409 `HANDLE_TAKEN` y auditoría `handle_changed`; la foto queda para `/me/photo/upload-url` (P1). LLD v1.12 (§1.2, §3.3, §7.3) y entregable LLD regenerado (docx + pdf).
- Evidencia: 86 pruebas en verde (23 nuevas en `MeTests`, incluido el criterio de CTZ-24: los cambios siguen tras un nuevo login); `dotnet format` sin cambios; OpenAPI revisado (bearer en ambos, `sex` como enum `snake_case`). Las pruebas hallaron un 500 con `"handle": null` (regla sin `Cascade(Stop)`), ya corregido.
- Decisiones: D-32 (merge patch, `@handle` libre con auditoría, foto fuera del PATCH, módulo `Profiles/`).
- Pendientes abiertos como issues (revisión de completitud): #135 CTZ-105 foto de perfil con R2 (CTZ-24 la pide y no tenía historia), #136 CTZ-106 respuestas de error en el OpenAPI (antes de CTZ-19), #137 pregunta a Diana sobre la prioridad de `PUT /me/preferences` y P-30 (LLD P0 vs CTZ-78 P1). Diana agrega CTZ-105 y CTZ-106 al backlog.
- Siguiente paso: PR #138 a `development` (revisa Rowlis); tras el merge, verificar en QA y avisar a Rowlis (CTZ-24) y arrancar CTZ-19 con él.

## 2026-11-21 — Ashley — Revisión y merge de CTZ-25 y CTZ-26
- Hecho: revisé y mergeé los PR de Diana. PR #130 (CTZ-25): verifiqué que cada pregunta sume 28 respuestas, los promedios y el ranking (suma 28,00); decisión renumerada a D-31. PR #133 (CTZ-26): verifiqué las 22 historias contra el LLD tras dos rondas (`densify_step_m` 10 m, `409`/`413` del catálogo, umbral de auto-pausa desde `/settings/public`, `MT-15` solo en CTZ-31b, máscara de 7 bits) y resolví los conflictos con `development` en `BITACORA.md` y `ESTADO.md`.
- Evidencia: PR #130 (`4c54d49`), PR #133 (`f9dd8de`) y este PR (CTZ-26 marcado como mergeado en `docs/ESTADO.md`, cierra la issue #34).
- Decisiones: ninguna nueva (D-31 la registró Diana en CTZ-25).
- Siguiente paso: arrancar S2 (21–27 nov) con los criterios de `docs/producto/criterios-S2-S3.md`.

## 2026-11-19 — Diana — CTZ-26 Criterios de aceptación detallados de S2 y S3
- Hecho: redactadas las 22 historias de los sprints S2 (12 historias) y S3 (10 historias) en `docs/producto/criterios-S2-S3.md` con descripción estructurada (*Como / Quiero / Para*) y criterios de aceptación verificables alineados con `docs/02-LLD.md` (incluyendo la aclaración de `CTZ-33` sobre métricas en pantalla y segundo plano, recorte de `200 m`, reversión de conquista `MT-13` y `CTZ-104`); enlazado en `docs/03-BACKLOG.md` y `docs/producto/README.md`, y actualizado `docs/ESTADO.md` a en revisión.
- Evidencia: PR #133 (`docs/CTZ-26-criterios-S2-S3` → `development`).
- Decisiones: ninguna nueva.
- Siguiente paso: revisión de Ashley para aprobar *Definition of Ready* de S2 y S3 antes de la weekly del viernes 20.

## 2026-11-19 — Diana — Actualización de ambos HLD y entregables
- Hecho: precisados ambos documentos de alto nivel (`docs/01-HLD.md` y `docs/01-HLD-v1.2-post-MVP.md`) y sus entregables (`HLD-Terrun.docx`, `HLD-Terrun.pdf` y `HLD-Terrun-v1.2-post-MVP.docx`) para reflejar el recorte automático de 200 m al inicio y al final de cada ruta frente a terceros (`RF-09`, `RF-20`, `RF-32`, `RNF-07`), la reversión de conquista al eliminar las actividades que la originaron (`§1.4`, `§1.5`, `RF-20`, `RF-49`, `RF-53`) y `Resend` por SMTP autenticado como proveedor definitivo de correo (`D-03`, `§3.2`).
- Evidencia: PR #132 (`docs/hld-correcciones` → `development`).
- Decisiones: ninguna nueva (aplica D-27 y D-29).
- Siguiente paso: revisión y merge de Ashley en PR #132 hacia `development`.

## 2026-11-17 — Diana — CTZ-25 Análisis de encuesta a corredores (n = 28) y orden de P1
- Hecho: tabuladas las 28 respuestas reales de `runner-survey` (meta de 15 superada en +87 %, 16 voluntarios beta sin correos en el repo) en `docs/producto/encuesta-corredores.md`, incluyendo limitaciones de la muestra y síntesis de respuestas abiertas; definido el orden tentativo de prioridad para P1 (Logros → Retos → Rankings → Clubes → Feed social, con avisos de voz como preferencia desactivable) y actualizado `docs/ESTADO.md`.
- Evidencia: PR #130 (`docs/CTZ-25-survey-analysis` → `development`), `docs/producto/encuesta-corredores.md` (`## Análisis (CTZ-25)`), `docs/DECISIONES.md` (`D-31`) y `docs/ESTADO.md`.
- Decisiones: D-31.
- Siguiente paso: revisar resultados tras la beta y completar CTZ-26 (criterios de aceptación de S2 y S3).

## 2026-11-17 — Rowlis — CTZ-08 Esqueleto de la app móvil
- Hecho: esqueleto en `mobile/` con Expo SDK 54 (`expo ~54.0.36`, `react 19.1.0`, `react-native 0.81.5`), TypeScript `strict` (`noUncheckedIndexedAccess`), Expo Router (`typedRoutes`) con las 5 pestañas vacías (`Mapa`, `Feed`, `Trackear`, `Clubes`, `Perfil`), `src/i18n/es.json` con helper `t()` tipado en compilación, tokens iniciales en `src/theme/`, script `npm run api:types` (`openapi-typescript` → `src/api/schema.d.ts`), ESLint 9 (`no-explicit-any: error`, `no-console: error`), Prettier y Jest (`jest-expo` + React Native Testing Library). Vinculado al proyecto EAS de la organización `terrun` (`9ca8c8c5-510a-468e-9dce-d6e360efac90`, esquema `terrun://`, paquete `com.terrun.app`).
- Evidencia: `npm run lint`, `npm run format:check`, `npm run typecheck`, `npm test` (2 suites, 4 pruebas en verde) y `npx expo export --platform android` exitosos en local.
- Decisiones: D-30.
- Siguiente paso: abrir el PR de `feat/CTZ-08-mobile-app-skeleton` contra `development` (cierra issue #15) y continuar con CTZ-09, CTZ-17 y CTZ-22.

## 2026-11-17 — Ashley — Revisión de CTZ-27, semilla legal y recorte de 200 m
- Hecho: revisé y mergeé el PR #128 de Diana tras dos rondas (versión 1.0 con los nombres de la semilla, conservación alineada con LLD §4.5, visibilidad por actividad, cookies, D-28). Migración `AlignLegalEffectiveDateAndAddRouteTrimSetting`: vigencia de los textos 1.0 al 17/11 y parámetro público `privacy.route_trim_m = 200`. Prueba nueva que comprueba que cada URL de `legal_document` corresponde a un archivo del repositorio. LLD v1.11 con el recorte en §2.4, §3, §4.1, §7.2 y el caso MT-21; backlog: CTZ-31b citaba MT-16 (récord de 5 km) en lugar del caso del recorte; README legal con las URLs de `main`, como la semilla.
- Evidencia: 63 pruebas en verde (1 nueva), `dotnet format` sin cambios, build sin advertencias; PR de `fix/CTZ-27-legal-seed-and-route-trim`.
- Decisiones: D-29.
- Siguiente paso: tras el merge, QA aplica la migración al desplegar `development`; Rowlis puede empezar CTZ-17 (lee `privacy.route_trim_m` en `/settings/public`). El recorte se implementa en CTZ-31b/CTZ-32 con MT-21.

## 2026-11-17 — Diana — CTZ-27 Términos y Condiciones y Política de Privacidad v1.0
- Hecho: redactados y publicados los documentos legales v1.0 (vigencia desde 17/11/2026) en `docs/producto/legal/terminos-v1.0.md` y `privacidad-v1.0.md` con lenguaje cercano para el usuario final, cuadro "Lo esencial", cumplimiento de la Ley 172-13 (no uso de cookies, visibilidad por actividad, recorte de 200 m, borrado en 24 h con anonimización de auditoría, correos oficiales `privacidad@terrun-info.com` y `soporte@terrun-info.com`, regiones verificadas y plazo de respuesta de 10 días hábiles); actualizados `legal/README.md` (con `LegalDocumentSeed.cs` y las dos casillas separadas de registro con declaración de 18 años), `CUMPLIMIENTO.md`, `NOTA-FASE-REAL.md` y `ESTADO.md`.
- Evidencia: PR #128 (`docs/CTZ-27-legal-documents-v1` → `development`).
- Decisiones: D-28 (edad mínima 18 años, exclusión temporal de *handbike*, plazo de 10 días hábiles, proveedores y regiones).
- Siguiente paso: tras el merge del PR #128, Ashley actualiza la vigencia en `LegalDocumentSeed.cs` y Rowlis consume `/legal/current` en `CTZ-17` y `CTZ-23`.

## 2026-11-17 — Ashley — Rama `development` antes de `main`
- Hecho: `development` pasa a ser la rama de integración y la rama por defecto; `main` solo recibe PR desde `development`. Los workflows `backend`, `mobile`, `politica` e `integridad` corren en PR y push a las dos ramas; `integridad` rechaza PR a `main` desde cualquier otra rama (y se repite si cambian la base); el hook `pre-commit` bloquea commits directos en `development`; guía, CONTRIBUTING, primer día, README del backend y ESTADO apuntan a `development`.
- Evidencia: PR de `chore/development-branch`.
- Decisiones: D-27.
- Siguiente paso: tras el merge, Ashley crea `development` desde `main`, le aplica la protección, la deja como rama por defecto y cambia en Render la rama de `terrun-api-qa` a `development`. Rowlis abre el PR de CTZ-08 contra `development`.

## 2026-11-16 — Ashley — CI de la app (`mobile.yml`) para CTZ-08
- Hecho: workflow `mobile.yml` (Node LTS con npm: `npm ci`, `lint`, `format:check`, `typecheck` y `test` dentro de `mobile/`). Corre en todos los PR como `backend.yml` y pasa sin revisar nada mientras no exista `mobile/package.json`, así entra a `main` antes que CTZ-08. Lo sube Ashley porque `integridad` solo deja cambiar `.github/` a la dueña del repo (D-20); con eso el PR de CTZ-08 puede cumplir "lint y pruebas en verde en la CI".
- Evidencia: PR de `chore/ci-mobile-workflow`.
- Decisiones: ninguna nueva (aplica D-20).
- Siguiente paso: Rowlis usa npm con `package-lock.json` y esos nombres de scripts en CTZ-08; cuando CTZ-08 esté en `main`, Ashley agrega `mobile` como check obligatorio.

## 2026-11-15 — Ashley — CTZ-14 Registro, verificación, login, refresh y logout
- Hecho: módulo `Identity` con `POST /api/v1/auth/register`, `verify-email`, `resend-verification`, `login`, `refresh` y `logout`. PBKDF2-SHA512 de 210.000 iteraciones, lista de 10.000 contraseñas comunes, JWT de 15 min con `sv` validado en cada solicitud, refresh de 30 días con rotación y revocación de la familia ante una reutilización, código de verificación de 6 dígitos (24 h, 5 intentos), evidencia del consentimiento (`method`, `app_version`), auditoría de cada inicio de sesión y correo por SMTP. Revisión de completitud: login sin pistas de tiempo, carrera de dos registros iguales resuelta por el índice único, límite de tasa por usuario (`sub`), logout que no dispara la alarma de robo, Bearer en OpenAPI y controladores con token por defecto.
- Evidencia: 62 pruebas en verde (26 nuevas de autenticación); mutaciones de los tres criterios verificadas; prueba de humo en `Staging` (registro 201, login, logout 204, refresh posterior 401, log de correo sin el código).
- Decisiones: D-26; LLD v1.10.
- Antes del merge: crear la variable `Jwt__Key` en Render (sin ella QA no arranca). Pendiente: comprar el dominio y configurar SMTP para que QA envíe correos.
- Siguiente paso: CTZ-16 (`GET /me` y `PATCH /me/profile`).

## 2026-11-15 — Ashley — Evaluación de cumplimiento legal
- Hecho: `docs/producto/legal/CUMPLIMIENTO.md` evalúa nueve puntos de cumplimiento contra lo que es Terrun (app gratuita con GPS en segundo plano que se publicará en Google Play): marco aplicable (Ley 172-13, Ley 136-03, eliminación de cuentas y ubicación en segundo plano de Google Play, licencia ODbL), estado de cada punto, responsable y contenido mínimo de la política de privacidad. No aplican, por ahora, la política de cookies (no se usan) ni la de reembolso (no se cobra).
- Evidencia: revisión de `survey/index.html` (sin cookies ni analítica, aviso de privacidad y atribución de OpenStreetMap presentes) y del modelo de datos (`consent` por documento).
- Decisiones pendientes: edad mínima (Diana); `DELETE /me` antes de publicar en Google Play; casilla de consentimiento en la encuesta o cerrarla.
- Siguiente paso: Diana redacta los textos en CTZ-27 con el contenido mínimo; Ashley suma el "cómo" del consentimiento en CTZ-14.

## 2026-11-15 — Ashley — Revisión de completitud de CTZ-03 y CTZ-04
- Hecho: revisión de lo ya mergeado contra el HLD y el LLD completos. En CTZ-03 faltaban las convenciones transversales de la API; quedaron en `src/Terrun.Api/Web/` (prefijo `/api/v1`, enums `snake_case`, errores con `code` y `traceId`, FluentValidation, límite de tasa, 2 MB, cabeceras de seguridad, HSTS y logs JSON). Guía del primer día: Rowlis también necesita .NET y Docker (D-16) y `dotnet tool restore`. Estándares §2.1: las constantes privadas usan `_camelCase`, como exige `.editorconfig`.
- Evidencia: 34 pruebas en verde (13 nuevas de convenciones, una sobre Kestrel real para el tope de 2 MB); mutaciones verificadas (sin prefijo ni convertidor de enums fallan 7 pruebas); API en `Staging` con logs JSON y `traceId` igual en la respuesta y en el log.
- Decisiones: D-24; LLD v1.8. Además, D-23: la revisión de completitud queda obligatoria antes de cada PR (guía §5 paso 7 y §8 regla 10, Definition of Done, plantilla de PR y guía del primer día; PR #120).
- Hallazgo en CTZ-04: con los planes gratis, Render se duerme a los 15 min sin tráfico y Neon da 100 horas de cómputo al mes; los trabajos en segundo plano del LLD §6 no eran viables. Decidido D-25: GitHub Actions como programador, procesamiento al subir y producción en un proyecto de Neon aparte; LLD v1.9 y nueva historia CTZ-104 (S2).
- Siguiente paso: CTZ-14; CTZ-104 entra en el S2 (Diana debe incluirla en la planificación).

## 2026-11-15 — Ashley — CTZ-13 Migración inicial
- Hecho: entidades y enums de M1 (6 tablas), M2 (4), M13 (1) y `audit_log` (M12, adelantada para CTZ-14) en `Terrun.Domain`; configuraciones de EF con los nombres del estándar (`pk_`, `fk_`, `uk_`, `ck_`), `citext`, `geography(point, 4326)`, `CHECK` de peso y radio, y defaults de preferencias activos; migración `AddIdentityProfileAndSettingsTables` con los 20 parámetros del LLD §2.4 y los documentos legales v1.0. La API aplica las migraciones al arrancar. Revisión de integridad contra el LLD completo: se agregaron `security_version` (claim `sv`), índice por `family_id`, interceptor de `created_at`/`updated_at` con `TimeProvider` y `audit_log` inmutable por trigger. 14 pruebas de integración nuevas (22 en total). Las fechas se redondean a microsegundos, la precisión de PostgreSQL (la CI en Linux lo detectó).
- Evidencia: `dotnet test` 22/22; `docker compose up` desde cero migra y responde Healthy (y al reiniciar: "No migrations were applied"); migración aplicada en una rama temporal de Neon copiada de `qa` (12 tablas, `audit_log` rechaza DELETE) y rama borrada. Mutaciones verificadas: un cambio de modelo sin migración y quitar el interceptor de fechas hacen fallar las pruebas.
- Decisiones: D-22; LLD v1.7.
- Siguiente paso: al mergear, Render migra la rama `qa` de Neon. Sigue CTZ-14 (registro y login). Diana debe nombrar los textos de CTZ-27 `terminos-v1.0.md` y `privacidad-v1.0.md`.

## 2026-11-15 — Ashley — Cierre del S0 y planificación del S1
- Hecho: S0 cerrado con 7 de 12 historias (CTZ-01 a CTZ-07); CTZ-08 a CTZ-12 pasan al S1 sin avance en el repo. `ESTADO.md` reescrito para el S1 (23 historias: 18 del backlog y 5 que vienen de S0), con el orden por persona según dependencias. CTZ-18 (despliegue de QA) queda hecha porque se adelantó en CTZ-04. `docs/sprints/S0.md` completado y `docs/sprints/S1.md` creado con el compromiso y los riesgos.
- Evidencia: `docs/ESTADO.md`, `docs/sprints/S0.md`, `docs/sprints/S1.md`; https://terrun-api-qa.onrender.com/health.
- Riesgos: carga de Rowlis (9 historias; CTZ-15 y CTZ-23b, ambas P1, son candidatas a S2 si Diana lo decide) y el hito del 20/11, que depende de que CTZ-08 esté en `main` a más tardar el martes 17.
- Siguiente paso: Rowlis arranca CTZ-08; Ashley, CTZ-13; Diana, CTZ-27.

## 2026-11-15 — Ashley — CTZ-04 Cuentas de servicios (cierre)
- Hecho: Render `terrun-api-qa` (Docker, Virginia, despliegue automático desde `main`) conectado a la rama `qa` de Neon; `/health` = Healthy, `/swagger` 200 y OpenAPI 3.1.1 con `servers` en `https` (gracias a `ASPNETCORE_FORWARDEDHEADERS_ENABLED`). Organización `terrun` en Expo con Diana (Admin) y Jorge, Rowlis, Rangelis y Jaime (Developer) invitados. LLD v1.6 (§8: variables y ambiente de QA).
- Evidencia: https://terrun-api-qa.onrender.com/health; `docs/DECISIONES.md` (D-21).
- Decisiones: D-21.
- Siguiente paso: invitar a Rowlis y Diana a la organización de Neon; pasar `ESTADO.md` al S1 en la planificación.

## 2026-11-15 — Ashley — Reglas de `main` (D-20) y CTZ-04 (Neon)
- Hecho: PR #115 (CTZ-03) mergeado. `main` ya no exige aprobación; quedan obligatorios los checks `autoria`, `integridad` y `backend`, la rama al día, las conversaciones resueltas y el bloqueo de push directo, push forzado y borrado, sin excepciones para administradores. Nuevo workflow `integridad` (`pull_request_target`): solo la dueña del repo cambia `.github/`. Etiquetas `v*` inmutables y alertas de Dependabot activas.
- Hecho: CTZ-04, parte Neon: organización y proyecto `terrun` (Postgres 16, AWS us-east-1), base `terrun`, ramas `main` y `qa` con PostGIS 3.3.10 (es la versión que ofrece Neon para Postgres 16; local usa 3.5). La API local conectada a la rama `qa` respondió `/health` = Healthy. Las credenciales no están en el repo.
- Evidencia: protección de `main` y ruleset de etiquetas consultados por la API de GitHub; `docs/DECISIONES.md` (D-20); LLD v1.5.
- Decisiones: D-20.
- Siguiente paso: Expo (organización e invitaciones) y Render (servicio `qa`) para cerrar CTZ-04; registrar la decisión de credenciales y PostGIS en D-21.

## 2026-11-14 — Ashley — CTZ-03 Esqueleto del backend
- Hecho: solución `backend/Terrun.slnx` (Api, Domain, Infrastructure, Engine + 2 proyectos de pruebas), `TerrunDbContext` con PostGIS y snake_case, `/health` (API + BD), Swagger y OpenAPI 3.1, `Dockerfile` y `docker-compose.yml` con `postgis/postgis:16-3.5`, y CI `backend.yml` (formato, build y pruebas). 8 pruebas en verde: integración con Testcontainers y reglas de dependencias del LLD §1.2. D-19 y LLD v1.4.
- Evidencia: PR #115; `dotnet test` 8/8 y `docker compose up` con `/health` = Healthy, verificados desde cero en local.
- Decisiones: D-19.
- Siguiente paso: aprobación del PR (Rowlis o Diana); luego agregar el check `backend` como obligatorio en la protección de `main` y seguir con CTZ-04 (cuentas de Neon, Render y Expo).

## 2026-11-14 — Ashley — Ajustes tras el HLD v2.0 (PR #6)
- Hecho: LLD v1.3 alineado con el HLD v2.0 ("trackear" en lugar de "grabar", referencia al HLD v2.0 y al v1.2 post-MVP) y entregables `LLD-Terrun.docx`/`.pdf` regenerados; plan de pruebas apuntando al Anexo B del HLD v1.2 post-MVP y a los módulos M1–M13 del LLD; corregida en D-18 la historia que pasa a P0 (CTZ-54, no CTZ-55); encabezado de `ESTADO.md` con el hito del 20/11.
- Evidencia: `docs/02-LLD.md`, `docs/entregables/LLD-Terrun.pdf`, `docs/qa/01-PLAN-PRUEBAS.md`, `docs/DECISIONES.md`, `docs/ESTADO.md`.
- Hecho: CTZ-02 cerrada; los 6 integrantes tienen acceso al repo y `main` quedó protegida (PR obligatorio con 1 aprobación, check `autoria`, sin excepciones para administradores). Un push directo a `main` fue rechazado por GitHub.
- Hecho: tablero del equipo movido a la cuenta AshFranco y ligado al repo (https://github.com/users/AshFranco/projects/4): 106 historias (4 en Hecha, 102 en Pendiente), público, con auto-add de issues nuevos. Enlazado desde el README, `ESTADO.md` y la guía (§2, §5 y §6); la plantilla de PR aclara que `Closes #` lleva el número del issue.
- Siguiente paso: CTZ-03 (esqueleto del backend).

## 2026-11-13 — Diana — Actualización de responsables S0, cierre de encuesta (23 respuestas), HLD v2.0 académico y HLD v1.2 post-MVP
- Hecho: actualizados los responsables de S0 en `docs/03-BACKLOG.md` y `docs/ESTADO.md` (`CTZ-08` y `CTZ-09` → Rowlis; `CTZ-10` → Jorge) y alineada la descripción de roles según `D-16`. Cerrado `CTZ-05` con 23 respuestas recolectadas (superando la meta de 15). Incorporado el **HLD v2.0** como entregable oficial para este período académico / MVP (`docs/01-HLD.md` y `docs/entregables/HLD-Terrun.docx`) aplicando las observaciones del profesor (*trackear*, descarte de P2 y de relojes inteligentes, síntesis en 29 RF —17 P0 y 12 P1— alineados con el backlog y versión concisa de ~10–11 páginas), y conservado el **HLD v1.2** completo con anexos como referencia para la evolución posterior al MVP (`docs/01-HLD-v1.2-post-MVP.md` y `docs/entregables/HLD-Terrun-v1.2-post-MVP.docx`, decisión `D-17`).
- Evidencia: `docs/03-BACKLOG.md`, `docs/ESTADO.md`, `docs/DECISIONES.md` (D-15, D-16 y D-17), `docs/01-HLD.md`, `docs/01-HLD-v1.2-post-MVP.md`, `docs/entregables/HLD-Terrun.docx`, `docs/entregables/HLD-Terrun-v1.2-post-MVP.docx`.
- Decisiones: D-15, D-16, D-17.
- Siguiente paso: cargar el backlog en GitHub Projects (`CTZ-06`) y documentar el análisis detallado de la encuesta en el S1 (`CTZ-25`).

## 2026-11-13 — Ashley — Ajuste de asignaciones del equipo (D-16)
- Hecho: Rowlis toma CTZ-08 y CTZ-09 y Jorge toma CTZ-10; Rowlis se suma al backend en funciones puntuales. Actualizados la guía §3, el LLD (v1.2), `docs/sprints/S0.md`, el README, `CONTRIBUTING.md` (revisores) y `scripts/configurar-entorno.sh`.
- Evidencia: `docs/DECISIONES.md` (D-16).
- Decisiones: D-16.
- Siguiente paso: Diana actualiza los responsables en `docs/03-BACKLOG.md` y `docs/ESTADO.md`; Ashley define qué funciones de backend toma Rowlis.

## 2026-11-13 — Ashley — HLD v1.2 de Diana incorporado al repo
- Hecho: el HLD v1.2 de Diana (estructura oficial de la asignatura, 90 RF por módulos, diseño visual de Terrun) pasa a ser el entregable `docs/entregables/HLD-Terrun.docx` y su PDF, con D-16 aplicado, asteriscos sobrantes quitados y una errata corregida. `docs/01-HLD.md` se regeneró desde el docx como versión de lectura; `scripts/generar_documentos.py` ya no genera el HLD. Referencias a secciones viejas del HLD corregidas en el backlog y el plan de pruebas.
- Evidencia: `docs/entregables/HLD-Terrun.pdf` (46 páginas).
- Siguiente paso: revisar con Diana las observaciones de la revisión (riesgos y hitos que se cayeron, prioridades inconsistentes, cifras de la encuesta).

## 2026-11-11 — Diana — CTZ-07 y CTZ-05 Confirmación del stack React y difusión de la encuesta
- Hecho: confirmado con Jorge y Rowlis que se sienten cómodos trabajando con React + TypeScript y React Native + Expo (queda validado el supuesto S-01 y resuelto el bloqueo de front). Enlace de la encuesta (`https://terrun-encuesta.netlify.app`) compartido con los corredores; ya está circulando y recibiendo respuestas.
- Evidencia: `docs/DECISIONES.md` (D-15), panel de Netlify Forms (`runner-survey`).
- Decisiones: D-15.
- Siguiente paso: alcanzar las 15 respuestas antes del 18/11 para analizarlas en el S1 (CTZ-25) y cargar el backlog en GitHub Projects (CTZ-06) al completarse CTZ-02.

## 2026-11-11 — Ashley — Guía del primer día (onboarding con Claude Code)
- Hecho: `docs/05-PRIMER-DIA.md`, con el texto que cada integrante pega en Claude Code para clonar, correr `configurar-entorno.sh`, leer los documentos en orden, revisar las herramientas de su rol y seguir la guía al pie de la letra. Enlazado desde el README y la guía §2; el README clona en la carpeta `terrun`.
- Evidencia: `docs/05-PRIMER-DIA.md`.
- Siguiente paso: Ashley invita al equipo (CTZ-02) y comparte el documento en el grupo.

## 2026-11-11 — Ashley — Cierre del día: estado del S0
- Hecho hoy: PR #2 (barrios, HLD y LLD v1.1, D-13) y PR #3 (encuesta a corredores, D-14) mergeados a `main`. Encuesta en línea en https://terrun-encuesta.netlify.app con 0 respuestas (la de prueba se borró).
- Estado por tarea:
  - CTZ-01 🔄 Entregar el HLD y el LLD **v1.0** hoy (están en Descargas; la v1.1 con barrios vive en `docs/entregables/`). Anotar aquí las observaciones del profesor.
  - CTZ-02 🔄 Repo creado; falta invitar al equipo como colaboradores y proteger `main` (en repo privado requiere GitHub Pro, gratis con el Student Developer Pack). Activar "Automatically delete head branches".
  - CTZ-03 ⬜ Esqueleto del backend .NET 10. Es la siguiente tarea de Ashley; depende de CTZ-02.
  - CTZ-04 ⬜ Cuentas en Neon, Render y Expo.
  - CTZ-05 🔄 Diana comparte el link por WhatsApp; meta: 15 respuestas antes del 18/11.
  - CTZ-07 ⬜ Diana confirma con Jorge y Rowlis que dominan React (supuesto S-01). Es lo más urgente del S0.
  - CTZ-06 y CTZ-08 a CTZ-12 ⬜ Sin empezar.
- Pendiente técnico: traducir al inglés los scripts `generar_diagramas.py`, `generar_documentos.py` y `configurar-entorno.sh` (D-11).
- Evidencia: `git log origin/main` (merge `469a558`); panel de Netlify → *Forms* → `runner-survey`.
- Siguiente paso: weekly del viernes 13/11. Antes, cerrar CTZ-01 y CTZ-02.

## 2026-11-11 — Ashley — CTZ-05 Página de la encuesta a corredores
- Hecho: página `survey/index.html` con las 12 preguntas, barra de avance, escalas en hexágonos, orden por toques en la pregunta 8 y una demo animada en un teléfono: la carrera 1 por la Churchill toca 10 zonas sin conquistar ninguna y la carrera 2 conquista la zona de la Churchill con Gustavo Mejía Ricart. Respuestas en Netlify Forms; `netlify.toml` y `scripts/build_survey_map.py`.
- Evidencia: vista previa https://claude.ai/artifact/LQkKUP9vBAPeFSR3CWamos; capturas en Chrome sin desborde horizontal; sintaxis del script verificada.
- Decisiones: D-14.
- Publicación: https://terrun-encuesta.netlify.app (Netlify CLI, cuenta personal). Detección de formularios activada y protección de acceso desactivada. Respuesta de prueba recibida con los 16 campos y borrada.
- Siguiente paso: Diana comparte el link por WhatsApp (meta: 15 respuestas antes del 18/11).

## 2026-11-11 — Ashley — Barrios y rey o reina del barrio (D-13)
- Hecho: mapa interactivo con calles reales de Piantini y Naco y la malla H3; simulación: la Churchill completa (2,47 km) toca 10 zonas y no conquista ninguna (máximo 4 de 7); explorar la zona de la Churchill con Gustavo Mejía Ricart la lleva de 3 a 6 sub-celdas. Se adopta el modelo híbrido: HLD y LLD v1.1 (41 tablas, 116 operaciones, MT-19 y MT-20, P-34) e historias CTZ-93 a CTZ-95.
- Evidencia: https://claude.ai/artifact/6V5ai7dURvwS2tfVbFrY1e (privado; compartir desde *Share*), `docs/02-LLD.md` §2.3, §4.3.
- Decisiones: D-13.
- Siguiente paso: Diana busca la fuente de límites de barrios (CTZ-93).

## 2026-11-10 — Ashley — Verificación del nombre Terrun y CTZ-92
- Hecho: push inicial a `AshFranco/Terrun`; verificación de dominios (RDAP), tiendas y marcas; historia CTZ-92 para Diana (decidir la marca comercial antes del 11/12); URL de clonado corregida en el README.
- Evidencia: `docs/producto/nombre-comercial.md`.
- Siguiente paso: Diana ejecuta CTZ-92 en el S4; mientras tanto se trabaja con el nombre Terrun.

## 2026-11-10 — Ashley — Nombre del producto: Terrun (D-12)
- Hecho: nombre aplicado en HLD, LLD, guía, README, backlog, estándares y scripts; carpeta del repo renombrada a `terrun`; HLD y LLD regenerados; commit inicial en `main`.
- Evidencia: `docs/entregables/HLD-Terrun.pdf`, `docs/entregables/LLD-Terrun.pdf`.
- Decisiones: D-12.
- Siguiente paso: verificar disponibilidad del nombre (tiendas, dominio, Instagram) y crear el repo `terrun` en GitHub (CTZ-02).

## 2026-11-10 — Ashley — Código en inglés (D-11)
- Hecho: estándares reescritos con la regla "todo lo técnico en inglés" y un glosario español → inglés; LLD rehecho con las 39 tablas, las 112 operaciones, los parámetros, los códigos de error, los trabajos y la estructura de carpetas en inglés; HLD, backlog, guía, CONTRIBUTING, diagramas y `.editorconfig` alineados; HLD y LLD regenerados.
- Evidencia: `docs/04-ESTANDARES-DESARROLLO.md`, `docs/02-LLD.md`, `docs/entregables/*.pdf`.
- Decisiones: D-11.
- Siguiente paso: el equipo revisa el glosario (§1.3) antes del S1.

## 2026-11-10 — Ashley — Estándares de desarrollo
- Hecho: `docs/04-ESTANDARES-DESARROLLO.md` (idioma, nombres por capa, glosario del dominio, reglas de backend/app/pruebas) y `.editorconfig` con reglas de nombres de C#. Enlazados desde la guía, CONTRIBUTING, la plantilla de PR y los criterios de CTZ-03/CTZ-08. Ejemplo JSON del LLD §3.3 alineado con la regla de unidades.
- Evidencia: `docs/04-ESTANDARES-DESARROLLO.md`, `.editorconfig`, LLD regenerado.
- Decisiones: D-10.
- Siguiente paso: el equipo revisa los estándares antes de empezar el S1.

## 2026-11-10 — Ashley — Organización del repositorio
- Hecho: estructura del repo con guía de trabajo, backlog S0–QA2 (CTZ-01 a CTZ-91), estado del sprint, bitácora, decisiones, plantillas de sprint, QA, issues y PR, hook contra la atribución de IA, CI de política de commits y script `configurar-entorno.sh`.
- Evidencia: `docs/00-GUIA-DE-TRABAJO.md`, `docs/03-BACKLOG.md`, `docs/ESTADO.md`, `CONTRIBUTING.md`, `.githooks/`, `.github/`.
- Siguiente paso: crear el repo en GitHub (CTZ-02) e invitar al equipo; cada integrante ejecuta `./scripts/configurar-entorno.sh`.

## 2026-11-10 — Ashley — CTZ-01 HLD y LLD
- Hecho: definición del proyecto con el equipo (roles, stack, reglas del juego), HLD (19 páginas) y LLD (26 páginas) en Word y PDF con 6 diagramas.
- Evidencia: `docs/01-HLD.md`, `docs/02-LLD.md`, `docs/entregables/*.pdf`; regenerables con `scripts/generar_diagramas.py` y `scripts/generar_documentos.py`.
- Decisiones: D-01 a D-09.
- Siguiente paso: entregar los PDFs al profesor el 11/11 y registrar sus observaciones.
