# Criterios de aceptación detallados — Sprints S2 y S3 (`CTZ-26`)

**Responsable:** Diana (Product Owner) · **Fecha:** 19/11/2026  
**Objetivo:** Dejar todas las historias de **S2** (21–27 nov) y **S3** (28 nov–4 dic) con descripción estructurada (*Como / Quiero / Para*) y criterios de aceptación verificables (*Definition of Ready*), alineadas con `docs/03-BACKLOG.md` y `docs/02-LLD.md`.

---

## Resumen de historias

| Sprint | Historias incluidas | Total |
|---|---|---|
| **S2 — Actividades y trackeo deportivo** (21–27 nov) | `CTZ-30`, `CTZ-31a`, `CTZ-104`, `CTZ-31b`, `CTZ-32`, `CTZ-33`, `CTZ-34`, `CTZ-35`, `CTZ-36`, `CTZ-37`, `CTZ-38`, `CTZ-39` | 12 |
| **S3 — Territorio H3 y calibración antitrampas** (28 nov–4 dic) | `CTZ-40`, `CTZ-41`, `CTZ-43`, `CTZ-44`, `CTZ-46`, `CTZ-47`, `CTZ-93`, `CTZ-48`, `CTZ-49`, `CTZ-82` | 10 |

---

# Sprint S2 — Actividades y trackeo deportivo (21 al 27 de noviembre)

> **Meta del sprint:** trackear una carrera real, guardarla sin conexión, subirla y ver distancia, ritmo y *splits* calculados por el servidor.

---

### `CTZ-30` — Tablas de M3/M4 y `POST /activities` idempotente con validación de tamaño y superposición
**Responsable:** Ashley · **Prioridad:** P0 · **Depende de:** `CTZ-13` · **Referencias:** `RF-16`, `LLD §2.3, §3, §4.1`

- **Descripción:**
  - **Como:** desarrollador backend del sistema.
  - **Quiero:** implementar las tablas de actividades, segmentos y puntos (M3/M4) junto con el endpoint idempotente `POST /activities`.
  - **Para:** recibir de forma segura las carreras grabadas desde el teléfono sin duplicados ni registros inválidos o superpuestos.
- **Criterios de aceptación:**
  - [ ] Migración aplicada con las tablas del módulo M3/M4 e índices necesarios para consultar actividades por usuario y fecha.
  - [ ] `POST /activities` autenticado devuelve `202 Accepted` con el `id` y estado `pending` al recibir una actividad nueva válida, encolándola para procesamiento.
  - [ ] Enviar la misma actividad con el mismo `clientId` para el mismo usuario responde `200 OK` con la actividad existente sin duplicar puntos ni efectos (`MT-09`).
  - [ ] Una actividad que supere los `30.000` puntos GPS responde `413 PAYLOAD_TOO_LARGE` con el catálogo de errores en español.
  - [ ] Si el rango de fechas (`startedAt`–`endedAt`) se superpone con otra actividad válida del mismo usuario, el servidor rechaza la petición con `409 ACTIVITY_OVERLAP`.
  - [ ] Pruebas automatizadas en verde cubriendo idempotencia (`MT-09`), límite de tamaño, superposición y permisos.

---

### `CTZ-31a` — Limpieza de señal GPS y filtro antitrampas en el servidor
**Responsable:** Ashley · **Prioridad:** P0 · **Depende de:** `CTZ-30` · **Referencias:** `RF-28`, `LLD §4.1, §6, §8`

- **Descripción:**
  - **Como:** sistema de validación deportiva de Terrun.
  - **Quiero:** limpiar los puntos GPS recibidos y detectar tramos con velocidades o saltos incompatibles con desplazarse a pie.
  - **Para:** garantizar que solo el esfuerzo físico real sume distancia y avance en el juego territorial.
- **Criterios de aceptación:**
  - [ ] Puntos con `dt <= 0` se descartan (`timestamp`) y saltos mayores a `anticheat.max_jump_m` (`200 m`) en menos de `5 s` se descartan (`jump`).
  - [ ] Puntos con precisión GPS peor al umbral configurado (`> 30 m`) se descartan y no suman distancia (`MT-02`).
  - [ ] Tramos entre puntos consecutivos del mismo segmento cuya velocidad supere `anticheat.max_speed_kmh` (`25 km/h`) se marcan como inválidos y no suman distancia ni territorio (`MT-03`).
  - [ ] Pausas entre segmentos no suman distancia entre el fin de un segmento y el inicio del siguiente (`MT-14`).
  - [ ] Actividades con ritmo promedio inferior a `anticheat.min_pace_s_per_km` (`< 2:30 min/km`) o con más del `20 %` de la distancia en tramos inválidos quedan en estado `in_review` y crean registro en `activity_review` (`MT-04`).
  - [ ] Todos los casos matemáticos `MT-01`, `MT-02`, `MT-03`, `MT-04` y `MT-14` pasan en verde.

---

### `CTZ-104` — Programador de trabajos en segundo plano
**Responsable:** Ashley · **Prioridad:** P0 · **Depende de:** `CTZ-13`, `CTZ-18` · **Referencias:** `D-25`, `LLD §6`

- **Descripción:**
  - **Como:** administrador técnico de la plataforma.
  - **Quiero:** contar con la tabla `job_run`, el endpoint interno `POST /internal/jobs/run` protegido con `X-Jobs-Key`, una cola en memoria para procesar actividades al subirse y el workflow horario `jobs.yml`.
  - **Para:** ejecutar tareas programadas y recuperar actividades pendientes de forma idempotente aunque la API entre en reposo en la capa gratuita.
- **Criterios de aceptación:**
  - [ ] Tabla `job_run` creada para registrar último inicio, último éxito y último error de cada trabajo.
  - [ ] Llamar a `POST /internal/jobs/run` sin la cabecera `X-Jobs-Key` o con una clave incorrecta responde `401 Unauthorized`.
  - [ ] `POST /activities` deja el `id` en la cola en memoria (`Channel`) para procesarse de inmediato, y al arrancar la API se procesan las actividades `pending` rezagadas (`FOR UPDATE SKIP LOCKED`).
  - [ ] El endpoint ejecuta únicamente los trabajos vencidos según `job_run` y su expresión cron; si una corrida se salta, la siguiente la recupera sin duplicar efectos.
  - [ ] El workflow `.github/workflows/jobs.yml` corre cada hora (minuto 17) con espera suficiente para el arranque en frío de Render (~3 min) y deja traza en `job_run`.

---

### `CTZ-31b` — Cálculo de métricas oficiales, *splits* y ruta simplificada con recorte de 200 m
**Responsable:** Ashley · **Prioridad:** P0 · **Depende de:** `CTZ-31a` · **Referencias:** `RF-17`, `RNF-04`, `RNF-07`, `D-29`, `LLD §4.1, §7.2`

- **Descripción:**
  - **Como:** corredor que termina una actividad.
  - **Quiero:** que el servidor calcule mis métricas oficiales, parciales por kilómetro (*splits*) y ruta simplificada ocultando los primeros y últimos `200 m` a terceros.
  - **Para:** consultar datos deportivos precisos y compartir mis recorridos sin exponer mi punto exacto de salida o llegada.
- **Criterios de aceptación:**
  - [ ] Se calculan sobre puntos válidos: distancia (`Haversine`), tiempo en movimiento (`velocidad >= 0,5 m/s`), ritmo promedio, elevación (media móvil de 5 puntos + histéresis de `3 m`), calorías (tabla MET por velocidad y peso) y *splits* cada `1.000 m` (`MT-15`).
  - [ ] Actividades con duración menor a `anticheat.min_duration_s` quedan `processed` pero sin otorgar territorio (`MT-05`).
  - [ ] La ruta simplificada (`Douglas-Peucker`, tolerancia `5 m`) se guarda completa y el recorte de `privacy.route_trim_m` (`200 m`) al inicio y al final se aplica al leer por terceros (`MT-21`).
  - [ ] Si la distancia total de la ruta es menor o igual a `2 × privacy.route_trim_m` (`<= 400 m`), no se devuelve trazo a terceros (`MT-21`); el dueño siempre ve su ruta completa.
  - [ ] Una actividad equivalente a 1 hora de grabación se procesa en menos de `10 s` (`RNF-04`) y la transacción atómica marca `failed` tras 3 intentos fallidos.

---

### `CTZ-32` — Endpoints de consulta, edición y eliminación de actividades (`/activities/*` y `/me/*`)
**Responsable:** Ashley · **Prioridad:** P0 · **Depende de:** `CTZ-30` · **Referencias:** `RF-18`, `LLD §3, §4.5, §7.2`

- **Descripción:**
  - **Como:** corredor autenticado.
  - **Quiero:** consultar el detalle, ruta, *splits*, historial paginado y resumen de mis actividades, así como editarlas o eliminarlas.
  - **Para:** gestionar mi bitácora deportiva con control total sobre mi información y privacidad.
- **Criterios de aceptación:**
  - [ ] `GET /activities/{id}`, `GET /activities/{id}/route` (GeoJSON) y `GET /activities/{id}/splits` devuelven métricas oficiales y respetan visibilidad y recorte de `200 m` para terceros al integrarse con `CTZ-31b`.
  - [ ] `GET /me/activities` soporta paginación por cursor y filtros por tipo de actividad y rango de fechas.
  - [ ] `GET /me/summary?period=week|month|year` devuelve totales del periodo y comparación contra el periodo anterior.
  - [ ] `PATCH /activities/{id}` permite editar título, descripción, tipo, visibilidad y zapatilla únicamente al dueño (`403 Forbidden` para otros usuarios).
  - [ ] `DELETE /activities/{id}` solo permite borrar al dueño, elimina visitas y recalcula el estado territorial y XP correspondiente (`MT-13` cuando se integre `CTZ-40`).

---

### `CTZ-33` — Tracker completo `P-08` con pausa, métricas en vivo, `SQLite` y segundo plano
**Responsable:** Jorge · **Prioridad:** P0 · **Depende de:** `CTZ-20` · **Referencias:** `RF-13`, `RF-14`, `RNF-02`, `RNF-10`, `LLD §5.1`

- **Descripción:**
  - **Como:** corredor.
  - **Quiero:** iniciar, pausar y reanudar mi carrera viendo mis métricas en vivo en la pantalla, mientras el GPS sigue grabando sin cortes cuando bloqueo el teléfono (con notificación persistente de progreso en Android).
  - **Para:** entrenar con el teléfono guardado en el bolsillo o brazalete sin perder puntos ni métricas.
- **Criterios de aceptación:**
  - [ ] Desde la pantalla principal se puede iniciar el trackeo en máximo `2 toques` (`RNF-10`), aplicando la máquina de estados `idle → recording ↔ paused → finished` (`LLD §5.1`).
  - [ ] En pantalla abierta (`P-08`) se muestran en vivo tiempo en movimiento, distancia y ritmo; en Android con pantalla bloqueada se mantiene visible la notificación persistente con el avance (ej. distancia y tiempo).
  - [ ] La *auto-pausa* (activa por defecto) toma el umbral `activity.auto_pause_speed_mps` desde `GET /settings/public` (sin valores fijos en código), pausa durante `10 s` por debajo del umbral y reanuda al superar `1 m/s`, abriendo un nuevo segmento en cada pausa (manual o automática).
  - [ ] Cada punto GPS (`BestForNavigation`, `2 s` o `5 m`) se persiste inmediatamente en `SQLite` local.
  - [ ] Si el sistema operativo cierra la app forzosamente en plena carrera, al reabrirla se detecta la sesión pendiente en `SQLite` y se ofrece reanudar o finalizar sin perder los puntos guardados.

---

### `CTZ-34` — Cola de subida sin conexión y pantalla `P-09` resumen post-carrera
**Responsable:** Jorge · **Prioridad:** P0 · **Depende de:** `CTZ-33`, `CTZ-30` · **Referencias:** `RF-16`, `RF-18`, `S-01`, `LLD §5.1`

- **Descripción:**
  - **Como:** corredor que termina un entrenamiento (con o sin cobertura móvil).
  - **Quiero:** que mi carrera quede en cola local si no hay internet, se suba automáticamente al recuperar conexión y me muestre el resumen oficial apenas termine de procesarse.
  - **Para:** no perder nunca una salida registrada y ver de inmediato mis resultados reales.
- **Criterios de aceptación:**
  - [ ] Al finalizar en modo avión o sin internet, la actividad pasa a estado `queued` en `SQLite` y se sube automáticamente al recuperar conexión con el mismo `clientId`.
  - [ ] Ante errores de red o servidor se reintenta con espera progresiva (*exponential backoff*: `5 s`, `30 s`, `2 min`, `10 min`) sin duplicar la actividad en el servidor.
  - [ ] Al recibir `202 Accepted`, `P-09` realiza sondeo (*polling*) a `GET /activities/{id}` hasta pasar de `pending` a `processed` (o `in_review`) y muestra métricas oficiales, mapa del trazo y parciales.
  - [ ] Si la actividad queda `in_review`, `P-09` informa claramente en español el motivo y estado sin mostrar mensajes técnicos crudos.

---

### `CTZ-35` — Pantallas `P-10` historial, `P-11` detalle y `P-12` editar o eliminar actividad
**Responsable:** Rowlis + Jorge (mapa de `P-11`) · **Prioridad:** P0 · **Depende de:** `CTZ-32` · **Referencias:** `RF-18`, `LLD §5.2`

- **Descripción:**
  - **Como:** corredor.
  - **Quiero:** revisar mi historial de carreras, entrar al detalle con su ruta y tabla de *splits*, y editar o eliminar cualquiera de mis actividades.
  - **Para:** llevar control de mis entrenamientos y corregir o borrar registros cuando lo necesite.
- **Criterios de aceptación:**
  - [ ] Construidas contra el contrato OpenAPI tipado (`src/api/schema.d.ts`) y mostrando datos idénticos a los de la API.
  - [ ] `P-10` lista las actividades propias con paginación continua y filtros por tipo y fecha.
  - [ ] `P-11` muestra el mapa del recorrido completo del dueño (Jorge) y la tabla de *splits* por kilómetro junto con las métricas oficiales.
  - [ ] `P-12` permite editar título, descripción, tipo o visibilidad (`PATCH`) y eliminar la actividad (`DELETE`) previa confirmación clara en español, actualizando el historial al volver.

---

### `CTZ-36` — Pantalla `P-15` mi perfil con estadísticas propias
**Responsable:** Rowlis · **Prioridad:** P0 · **Depende de:** `CTZ-32` · **Referencias:** `RF-07`, `LLD §5.2`

- **Descripción:**
  - **Como:** corredor.
  - **Quiero:** ver en mi perfil mis datos, nivel y estadísticas acumuladas por periodo.
  - **Para:** seguir mi evolución deportiva desde un solo lugar.
- **Criterios de aceptación:**
  - [ ] `P-15` consume `GET /me` y `GET /me/summary?period=...` y muestra totales exactamente iguales a los devueltos por la API.
  - [ ] Permite alternar el resumen entre semana, mes y año mostrando la comparación frente al periodo anterior.
  - [ ] Incluye acceso directo al historial (`P-10`), edición de perfil (`P-17`) y preferencias (`P-30`), con estados vacíos y de carga en español.

---

### `CTZ-37` — Catálogo inicial de logros y retos (`docs/producto/`)
**Responsable:** Diana · **Prioridad:** P1 · **Depende de:** — · **Referencias:** `RF-25`, `RF-26`, `LLD §2.3, §4.4`

- **Descripción:**
  - **Como:** Product Owner de Terrun.
  - **Quiero:** definir hasta `20` logros iniciales y `3` retos iniciales con sus nombres en español, reglas compatibles con el motor y puntos `XP`.
  - **Para:** dejar listo el contenido que alimentará el *seed* de gamificación y motivar la constancia de los corredores.
- **Criterios de aceptación:**
  - [ ] Documento creado en `docs/producto/` con tabla estructurada lista para trasladar al *seed* (`code`, `name`, `description`, `category`, `rule` en JSON, `xp`).
  - [ ] Cada logro usa únicamente métricas soportadas por el motor (`LLD §4.4`: `total_distance_m`, `activity_distance_m`, `total_activities`, `zones_conquered`, `zones_owned`, `total_steals`, `streak_weeks`, `started_before_hour`, `level`).
  - [ ] Los `3` retos iniciales incluyen título, descripción, tipo, meta, ventana temporal y `XP` de recompensa, con tono motivador y sin recompensar velocidad pura.

---

### `CTZ-38` — Pruebas de API de actividades y privacidad
**Responsable:** Rangelis · **Prioridad:** P0 · **Depende de:** `CTZ-31b`, `CTZ-32` · **Referencias:** `LLD §3, §8`

- **Descripción:**
  - **Como:** integrante de QA.
  - **Quiero:** ejecutar casos de prueba funcionales y de API sobre el módulo de actividades en QA.
  - **Para:** verificar que la idempotencia, validaciones, permisos y recorte de `200 m` funcionan correctamente antes de cerrar el sprint.
- **Criterios de aceptación:**
  - [ ] Colección de API (Bruno) actualizada con casos para `POST /activities`, `GET /activities/{id}`, `/route`, `/splits`, `PATCH`, `DELETE` y `/me/summary`.
  - [ ] Verificado en QA: reenvío con mismo `clientId` (`200`), `> 30.000` puntos (`413`), superposición (`409`), edición/borrado bloqueados a no-dueños (`403`).
  - [ ] Verificado el caso `MT-21`: un tercero no ve los primeros ni los últimos `200 m` de la ruta (y en rutas `<= 400 m` no ve trazo), mientras el dueño la ve completa.
  - [ ] Resultados documentados en `docs/qa/` y cualquier fallo registrado en `docs/qa/02-REGISTRO-DEFECTOS.md`.

---

### `CTZ-39` — Pruebas de campo: precisión, segundo plano y guardado sin conexión
**Responsable:** Jaime · **Prioridad:** P0 · **Depende de:** `CTZ-34` · **Referencias:** `RNF-01`, `RNF-02`, `S-01`

- **Descripción:**
  - **Como:** integrante de QA de campo.
  - **Quiero:** probar la app corriendo en calle con teléfonos Android reales en una ruta medida de referencia, durante al menos 1 hora con pantalla bloqueada y en modo avión.
  - **Para:** confirmar que el producto cumple en condiciones reales de uso antes de pasar al motor territorial.
- **Criterios de aceptación:**
  - [ ] Recorrido completado en ruta urbana de referencia verificando error de distancia menor al `3 %` (`RNF-01`).
  - [ ] Grabación continua durante al menos `1 hora` con la pantalla bloqueada sin cortes del sistema operativo (`RNF-02`).
  - [ ] Prueba en modo avión / sin datos demostrando que la carrera se guarda en local y se sincroniza sin duplicados al recuperar red (`S-01`).
  - [ ] Evidencias y métricas registradas en `docs/qa/03-PRUEBAS-CAMPO.md` (o defectos reportados si algún criterio falla).

---

# Sprint S3 — Territorio H3 y calibración antitrampas (28 de noviembre al 4 de diciembre)

> **Meta del sprint:** al terminar una carrera aparecen las zonas en progreso y conquistadas (regla 5 de 7) en el mapa y se validan en calle los umbrales antitrampas.

---

### `CTZ-40` — Tablas de M5 y motor de territorio H3 (regla 5 de 7 y conquista permanente)
**Responsable:** Ashley · **Prioridad:** P0 · **Depende de:** `CTZ-31b` · **Referencias:** `RF-20`, `RNF-11`, `LLD §2.3, §4.2, §4.5`

- **Descripción:**
  - **Como:** desarrollador backend del motor territorial.
  - **Quiero:** implementar las tablas de M5 y el algoritmo que densifica trazos válidos sobre sub-celdas H3 (nivel 10), actualiza máscaras y otorga conquistas permanentes al completar `5 de 7`.
  - **Para:** transformar cada carrera válida en avance y conquista real sobre la malla hexagonal de la ciudad.
- **Criterios de aceptación:**
  - [ ] Migración aplicada con tablas de M5 (`zone`, `cell_visit`, `zone_progress`, `conquest`, etc.) e índices espaciales/consulta definidos en `LLD §2.3`.
  - [ ] El motor densifica cada tramo válido cada `territory.densify_step_m` (`10 m`), obtiene la sub-celda nivel 10 y asigna su zona padre únicamente mediante `cellToParent` (`MT-18`), actualizando `cell_mask`.
  - [ ] Al alcanzar `>= 5` de `7` sub-celdas (`territory.conquest_threshold`) entre una o varias actividades válidas, registra `conquest` (única por usuario y zona) y suma `xp.conquest` (`+50 XP`) sin duplicar (`MT-06`, `MT-07`, `MT-08`).
  - [ ] Al eliminar una actividad (`DELETE /activities/{id}`), se recalculan `cell_mask` y conquistas: si una zona baja de 5 de 7, se revierte la conquista y se ajusta el XP (`MT-13`).
  - [ ] Pruebas unitarias en verde cubriendo `MT-06`, `MT-07`, `MT-08`, `MT-09`, `MT-13` y `MT-18`, con cobertura del motor `>= 80 %` (`RNF-11`).

---

### `CTZ-41` — Endpoints territoriales (`/zones`, `/zones/{h3}`, `/me/territory`, `/me/conquests`)
**Responsable:** Ashley · **Prioridad:** P0 · **Depende de:** `CTZ-40` · **Referencias:** `RF-19`, `RF-21`, `RNF-04`, `RNF-05`, `LLD §3`

- **Descripción:**
  - **Como:** cliente móvil de Terrun.
  - **Quiero:** consultar las zonas H3 visibles dentro de un `bbox` en GeoJSON, el detalle de cada hexágono y el resumen y listado de conquistas propias.
  - **Para:** pintar el mapa territorial según el estado de cada zona y mostrar el progreso detallado al usuario.
- **Criterios de aceptación:**
  - [ ] `GET /zones?bbox=minLon,minLat,maxLon,maxLat` devuelve GeoJSON con el estado por usuario (`free`, `in_progress`, `conquered`, `owned_by_me`, `owned_by_other`) en `< 1 s` para hasta `2.000` zonas (`RNF-05`).
  - [ ] Si el `bbox` solicitado excede `map.max_zones_per_bbox` (`2.000` zonas), el endpoint responde `422` con código `BBOX_TOO_LARGE`.
  - [ ] `GET /zones/{h3}` devuelve el dueño actual (si existe), puntos, máscara de progreso propio (7 bits, uno por sub-celda) y top 5 de los últimos 30 días.
  - [ ] `GET /me/territory` y `GET /me/conquests` devuelven totales y listado paginado de zonas conquistadas con la actividad que las originó, en `< 500 ms` p95 (`RNF-04`).

---

### `CTZ-43` — Capa interactiva de hexágonos H3 en pantalla `P-07`
**Responsable:** Jorge · **Prioridad:** P0 · **Depende de:** `CTZ-41` · **Referencias:** `RF-19`, `RNF-05`, `LLD §5.2`

- **Descripción:**
  - **Como:** corredor que explora su sector en el mapa.
  - **Quiero:** ver sobre `P-07` los hexágonos H3 coloreados según su estado territorial y que se recarguen fluidamente al mover o acercar el mapa.
  - **Para:** planificar por qué calles correr para completar zonas en progreso o conquistar zonas nuevas.
- **Criterios de aceptación:**
  - [ ] `P-07` consulta `GET /zones?bbox=...` al mover o hacer zoom (con *debounce*) y dibuja los hexágonos con colores diferenciados por estado (`free`, `in_progress`, `conquered`, `owned_by_me`, `owned_by_other`).
  - [ ] Si la API devuelve `422 BBOX_TOO_LARGE`, la app muestra un aviso claro en español invitando a acercar el mapa sin mostrar errores ni bloquear la vista.
  - [ ] Desplazamiento fluido (`< 1 s` de carga por área visible) probado en un dispositivo Android de gama media.
  - [ ] Al tocar cualquier hexágono en `P-07`, se abre su detalle (`P-13`).

---

### `CTZ-44` — Pantalla `P-13` detalle de zona (7 sub-celdas) y zonas nuevas en `P-09`
**Responsable:** Jorge · **Prioridad:** P0 · **Depende de:** `CTZ-41` · **Referencias:** `RF-21`, `LLD §5.2`

- **Descripción:**
  - **Como:** corredor.
  - **Quiero:** tocar una zona en el mapa para ver el dibujo de sus 7 sub-celdas con mi avance, y ver en `P-09` qué zonas conquisté al terminar una carrera.
  - **Para:** saber exactamente qué partes del hexágono me faltan por pisar y celebrar mi progreso al finalizar.
- **Criterios de aceptación:**
  - [ ] `P-13` consume `GET /zones/{h3}` y dibuja las 7 sub-celdas (1 central y 6 alrededor) reflejando exactamente la máscara `cell_mask` devuelta por la API.
  - [ ] Muestra el conteo (`X de 7 sub-celdas`), estado de conquista/dominio y el top 5 de corredores en los últimos 30 días.
  - [ ] `P-09` (resumen post-carrera) muestra las zonas nuevas conquistadas y el avance de sub-celdas generado por esa actividad.

---

### `CTZ-46` — Pantalla `P-06` onboarding y solicitud de permisos de ubicación
**Responsable:** Rowlis + Jorge · **Prioridad:** P0 · **Depende de:** `CTZ-33` · **Referencias:** `RF-06`, `LLD §5.2`

- **Descripción:**
  - **Como:** corredor que entra por primera vez a la aplicación.
  - **Quiero:** leer una explicación clara en español sobre por qué Terrun necesita permiso de ubicación (incluso con pantalla bloqueada) antes de que aparezca el diálogo del sistema.
  - **Para:** decidir con confianza conceder el permiso y entender que solo se usa al grabar una carrera.
- **Criterios de aceptación:**
  - [ ] `P-06` muestra en español el aviso destacado previo exigido antes de lanzar el diálogo nativo de permiso de ubicación (con textos aprobados por Diana y alineados a Google Play).
  - [ ] Si el usuario acepta, continúa al flujo normal de la app con permiso concedido.
  - [ ] Si el usuario rechaza el permiso, la aplicación **no se rompe** ni queda bloqueada: informa qué funciones requieren GPS y permite continuar navegando o reintentar desde ajustes.

---

### `CTZ-47` — Demo interna con corredores del grupo y calibración de parámetros del motor
**Responsable:** Diana · **Prioridad:** P1 · **Depende de:** `CTZ-44`, `CTZ-82` · **Referencias:** `RF-90`, `LLD §2.4`

- **Descripción:**
  - **Como:** Product Owner.
  - **Quiero:** coordinar una sesión interna de prueba en calle con el equipo y calibrar si los parámetros iniciales del motor (`system_setting`) se sienten justos y motivadores.
  - **Para:** ajustar velocidades, saltos o recompensas antes de abrir las pruebas externas y la demo final.
- **Criterios de aceptación:**
  - [ ] Sesión ejecutada con carreras reales revisando experiencia de conquista 5/7, respuesta del mapa y comportamiento del filtro antitrampas.
  - [ ] Hallazgos y cualquier ajuste en parámetros de `system_setting` (`anticheat.*`, `territory.*`, `xp.*`) registrados formalmente en `docs/DECISIONES.md`.

---

### `CTZ-93` — Delimitación GeoJSON de los 6 barrios iniciales del Distrito Nacional
**Responsable:** Diana · **Prioridad:** P1 · **Depende de:** — · **Referencias:** `D-04`, `HLD §1.4, §2.2`

- **Descripción:**
  - **Como:** Product Owner.
  - **Quiero:** recopilar y validar los polígonos en formato GeoJSON de los 6 barrios iniciales (*Piantini, Naco, Evaristo Morales, Serrallés, Paraíso y Bella Vista*).
  - **Para:** que el backend pueda preasignar cada hexágono H3 a su barrio en la semilla antes de construir el módulo de reinados en S4.
- **Criterios de aceptación:**
  - [ ] Archivo(s) GeoJSON guardados en `docs/producto/barrios/` cubriendo los 6 barrios sin huecos ni solapamientos graves entre sectores contiguos.
  - [ ] Polígonos verificados visualmente sobre el mapa de Santo Domingo (coordenadas `lon, lat` válidas en WGS84) y listos para el script/semilla del backend.

---

### `CTZ-48` — Verificación manual de los casos matemáticos (`MT`) del motor territorial
**Responsable:** Rangelis · **Prioridad:** P0 · **Depende de:** `CTZ-40` · **Referencias:** `LLD §8`

- **Descripción:**
  - **Como:** integrante de QA.
  - **Quiero:** calcular a mano los resultados esperados de los casos matemáticos del motor territorial junto con Ashley y compararlos con la ejecución automatizada.
  - **Para:** asegurar que la lógica geoespacial y de puntuación no tiene errores silenciosos.
- **Criterios de aceptación:**
  - [ ] Casos `MT-06`, `MT-07`, `MT-08`, `MT-09`, `MT-13` y `MT-18` verificados paso a paso contra los resultados reales del motor (`xUnit`) y endpoints en QA.
  - [ ] Evidencia de coincidencia registrada en `docs/qa/` (o defectos reportados si existe alguna discrepancia).

---

### `CTZ-49` — Prueba de campo: conquistar 3 zonas reales y comparar con el mapa
**Responsable:** Jaime · **Prioridad:** P0 · **Depende de:** `CTZ-43` · **Referencias:** `RF-19`, `RF-20`

- **Descripción:**
  - **Como:** integrante de QA de campo.
  - **Quiero:** salir a correr o caminar para conquistar al menos `3` zonas reales en calle y comparar las sub-celdas pisadas con lo que dibujan `P-07` y `P-13`.
  - **Para:** validar que la malla hexagonal en el mapa coincide con el recorrido físico real.
- **Criterios de aceptación:**
  - [ ] Completadas al menos `3` conquistas reales (`>= 5 de 7` sub-celdas) en recorridos de campo.
  - [ ] Verificado que las sub-celdas iluminadas en `P-13` y el estado del hexágono en `P-07` coinciden con las calles recorridas.
  - [ ] Evidencias con capturas de pantalla y notas guardadas en `docs/qa/03-PRUEBAS-CAMPO.md`.

---

### `CTZ-82` — Prueba de campo antitrampas en bicicleta y vehículo
**Responsable:** Jaime · **Prioridad:** P0 · **Depende de:** `CTZ-31a`, `CTZ-34` · **Referencias:** `RF-28`, `LLD §4.1`

- **Descripción:**
  - **Como:** integrante de QA de campo.
  - **Quiero:** registrar recorridos reales desplazándome en bicicleta (> 25 km/h) y en automóvil para observar cómo los procesa el servidor.
  - **Para:** comprobar que el filtro antitrampas impide conquistar zonas haciendo trampa en vehículos.
- **Criterios de aceptación:**
  - [ ] Grabados y subidos al menos un recorrido en bicicleta rápida / vehículo superando `25 km/h` y otro mixto (tramo a pie + tramo en vehículo `> 20 %` de la distancia).
  - [ ] Verificado en QA que los tramos `> 25 km/h` se marcan inválidos y no otorgan distancia ni conquistas/territorio, y que la actividad con `> 20 %` inválido pasa a `in_review`.
  - [ ] Resultados y capturas documentados en `docs/qa/03-PRUEBAS-CAMPO.md`.
