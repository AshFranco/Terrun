# Encuesta a corredores (CTZ-05)

Meta: 15 respuestas antes del 18/11. Se comparte por WhatsApp.

## Dónde vive

- **Página:** `survey/index.html`, con una demo animada de cómo se conquista una zona (calles reales de Piantini, `survey/map-data.js`). Es una sola página en HTML, CSS y JavaScript, sin build.
- **Respuestas:** las guarda **Netlify Forms** (formulario `runner-survey`). Nadie tiene que iniciar sesión para responder. Las respuestas se ven y se descargan en CSV desde el panel de Netlify, en *Forms*. El plan gratis guarda hasta 100 respuestas al mes.
- **Vista previa para el equipo:** https://claude.ai/artifact/LQkKUP9vBAPeFSR3CWamos (privado; compartir desde *Share*). Desde ahí las respuestas **no** se guardan.
- **Mapa:** `python3 scripts/build_survey_map.py <export-de-overpass.json>` regenera `survey/map-data.js`.

### Publicación

- **Link público:** https://terrun-encuesta.netlify.app (publicado el 11/11 desde el commit `d941fcf`).
- **Panel:** https://app.netlify.com/projects/terrun-encuesta → *Forms* → `runner-survey` (cuenta personal de Ashley).
- Configuración del sitio: detección de formularios activada y protección de acceso desactivada; si no, los corredores ven un error 401.
- Prueba hecha el 11/11: una respuesta enviada, recibida con todos los campos y borrada.
- El sitio **no** está conectado al repo: los cambios en `survey/` se publican a mano desde la raíz del repo con `netlify deploy --prod --no-build --dir survey --site terrun-encuesta`.

Si se conecta un dominio propio, hay que agregarlo en `isLive` dentro de `survey/index.html`; si no, la página se queda en modo vista previa.

## Campos (para el análisis)

| # | Campo | Valores |
|---|---|---|
| 1 | `runs_per_week` | `1`, `2_3`, `4_5`, `6_plus` |
| 2 | `current_app`, `current_app_other` | `strava`, `nike_run_club`, `garmin_connect`, `adidas_running`, `none`, `other` + texto |
| 3 | `app_likes` | texto (opcional) |
| 4 | `app_missing` | texto (opcional) |
| 5 | `runs_with_phone` | `always`, `sometimes_watch`, `never` |
| 6 | `conquest_interest` | `1` a `5` |
| 7 | `compete_mode` | `solo`, `club`, `both` |
| 8 | `feature_ranking` | las 7 claves en orden, de más a menos importante, separadas por `>`: `metrics`, `territory_map`, `leaderboards`, `monthly_challenges`, `achievements`, `social_feed`, `clubs` |
| 9 | `route_privacy_concern` | `1` a `5` |
| 10 | `voice_cues` | `yes`, `no`, `indifferent` |
| 11 | `premium_price` | `no`, `up_to_3`, `up_to_5`, `more_than_5` |
| 12 | `beta_tester`, `beta_email` | `yes` + correo, `no` |

`form_version` (hoy `v1`) cambia si se modifican las preguntas, para no mezclar versiones en el análisis. El correo de los testers es dato personal: no se copia al repositorio.

## Preguntas

1. ¿Cuántas veces por semana corres? (1 · 2–3 · 4–5 · 6 o más)
2. ¿Qué app usas hoy? (Strava · Nike Run Club · Garmin Connect · Adidas Running · Ninguna · Otra)
3. ¿Qué es lo que **más** valoras de esa app? (abierta)
4. ¿Qué te molesta o le falta? (abierta)
5. ¿Corres con el teléfono? (Siempre · A veces, uso reloj · Nunca)
6. ¿Te motivaría "conquistar" zonas de tu ciudad y defenderlas de otros corredores? (escala 1–5)
7. ¿Preferirías competir solo, con tu club, o ambas cosas?
8. Ordena por importancia: métricas y ritmo · mapa de territorio · rankings · retos mensuales · logros · feed social · clubes
9. ¿Te preocupa que otros vean tus rutas? (escala 1–5)
10. ¿Usarías avisos de voz por km mientras corres? (Sí · No · Me da igual)
11. ¿Pagarías por funciones premium? (No · Hasta US$3/mes · Hasta US$5/mes · Más)
12. ¿Te gustaría ser tester de la app entre diciembre y enero? (Sí, este es mi correo de Google: ___ · No)

## Análisis (CTZ-25)

**Fecha de cierre y análisis:** 17 de noviembre de 2026 · **Respuestas válidas:** **28** (meta inicial: 15; superada en **+87 %**, todas con `form_version = v1`) · **Periodo de recolección:** 11–13 de noviembre de 2026.

### 1. Resultados cuantitativos por pregunta (`n = 28`)

| Pregunta | Distribución de respuestas | Lectura clave |
|---|---|---|
| **1. Frecuencia semanal (`runs_per_week`)** | `1 vez`: **11** (39,3 %) · `2–3 veces`: **9** (32,1 %) · `4–5 veces`: **6** (21,4 %) · `6 o más`: **2** (7,1 %) | **71,4 %** son corredores recreativos o principiantes (1–3 salidas/sem); **28,6 %** son frecuentes (4+ salidas/sem). |
| **2. App actual (`current_app`)** | `Strava`: **12** (42,9 %) · `Ninguna`: **9** (32,1 %) · `Otra` (*Sweatcoin, Fitness, Zepp, Samsung Health*): **4** (14,3 %) · `Garmin Connect`: **3** (10,7 %) | Cerca de la mitad usa Strava y casi un tercio no usa ninguna app todavía. |
| **5. Corre con teléfono (`runs_with_phone`)** | `Siempre`: **14** (50,0 %) · `A veces, uso reloj`: **11** (39,3 %) · `Nunca`: **3** (10,7 %) | **89,3 %** (25/28) corre con el teléfono siempre o a veces: valida el enfoque móvil-primero del MVP; los relojes inteligentes quedan bien ubicados en `P2` / `CTZ-96`. |
| **6. Motivación por conquistar/defender zonas (`conquest_interest`, 1–5)** | `5`: **6** (21,4 %) · `4`: **7** (25,0 %) · `3`: **12** (42,9 %) · `2`: **0** · `1`: **3** (10,7 %) | Promedio **3,46 / 5**. Solo **46,4 %** (13/28) muestra interés alto o muy alto (4–5); 42,9 % es neutral (3) y 10,7 % tiene interés bajo. El interés es positivo, pero no mayoritariamente fuerte. |
| **7. Modo de competencia (`compete_mode`)** | `Solo`: **11** (39,3 %) · `Ambas cosas`: **9** (32,1 %) · `Con mi club`: **8** (28,6 %) | **71,4 %** quiere competir individualmente (`solo` o `ambas`) y **60,7 %** valora competir en club (`club` o `ambas`). |
| **9. Preocupación por privacidad de rutas (`route_privacy_concern`, 1–5)** | `1`: **13** (46,4 %) · `2`: **4** (14,3 %) · `3`: **5** (17,9 %) · `4`: **3** (10,7 %) · `5`: **3** (10,7 %) | Promedio **2,25 / 5**. Aunque 60,7 % reporta preocupación baja (1–2), **21,4 %** (6/28) tiene preocupación alta o máxima (4–5), lo que respalda el recorte automático de 200 m (`RNF-07`). |
| **10. Avisos de voz por km (`voice_cues`)** | `Sí`: **11** (39,3 %) · `Me da igual`: **11** (39,3 %) · `No`: **6** (21,4 %) | **39,3 %** lo quiere activamente y **21,4 %** lo rechaza. Se mantiene como preferencia desactivable de prioridad baja (`CTZ-78`); no hay base para ubicarlo por encima de otros módulos. |
| **11. Disposición a pagar (`premium_price`)** | `No`: **17** (60,7 %) · `Tope de US$3`: **7** (25,0 %) · `Tope de US$5`: **4** (14,3 %) | **39,3 %** pagaría algo por funciones extra, pero con un tope: 25,0 % hasta US$3 y solo 14,3 % hasta US$5. La mayoría espera métricas y rutas básicas gratuitas. |
| **12. Voluntarios para beta (`beta_tester`)** | `Sí`: **16** (57,1 %) · `No`: **12** (42,9 %) | **16 testers disponibles** para diciembre y enero *(sus correos personales se resguardan fuera del repositorio)*. Las 5 respuestas nuevas no sumaron voluntarios. |

### 2. Síntesis de respuestas abiertas (`app_likes` y `app_missing`)

- **Lo que más valoran de su app actual:** medición clara y gratuita de distancia, tiempo y ritmo; precisión del trazo; facilidad, interfaz y rapidez de uso; cantidad de datos registrados; y, en menor medida, comunidad y retroalimentación/motivación.
- **Lo que les molesta o sienten que falta:**
  1. **Muro de pago en funciones básicas** (*paywall* en Strava para ver análisis, rutas o estadísticas, o pedir que la versión gratuita incluya más); lo mencionan al menos 5 personas.
  2. **Guía de entrenamiento y análisis para progresar:** saber cuándo subir o bajar la intensidad, alertas cuando se sale de la zona de ritmo, análisis de qué hacer para mejorar (al menos 4 personas). Es un tema distinto de la gamificación y puede alimentar el backlog de métricas.
  3. **Precisión y rapidez** al reflejar actividades.
  4. **Recompensas/gamificación y descubrimiento de rutas y mapas** (menciones aisladas).
- **Nota de lectura:** 9 de las 28 personas no usan ninguna app; sus comentarios en *"lo que me gusta"* son expectativas, no experiencia real con una herramienta. Dos respuestas nuevas dejaron las preguntas abiertas vacías o con *"N/A"*.

### 3. Ranking de funciones (`feature_ranking`) y orden derivado de los `P1`

Orden promedio asignado por los 28 corredores (**1 = más importante**, **7 = menos importante**):

| Puesto | Función | Posición promedio | Veces elegida #1 | Clasificación en Terrun |
|---|---|---|---|---|
| **1.º** | **Métricas y ritmo (`metrics`)** | **2,54** | 12 | **P0** (núcleo obligatorio, `S2`) |
| **2.º** | **Logros (`achievements`)** | **3,04** | 5 | **P1 — Prioridad 1** |
| **3.º (empate)** | **Mapa de territorio (`territory_map`)** | **3,82** | 2 | **P0** (núcleo obligatorio, `S3–S4`) |
| **3.º (empate)** | **Retos mensuales (`monthly_challenges`)** | **3,82** | 3 | **P1 — Prioridad 2** |
| **5.º** | **Rankings (`leaderboards`)** | **4,25** | 2 | **P1 — Prioridad 3** |
| **6.º** | **Clubes (`clubs`)** | **5,21** | 3 | **P1 — Prioridad 4** |
| **7.º** | **Feed social (`social_feed`)** | **5,32** | 1 | **P1 — Prioridad 5** |

**Cómo leer la tabla:**
- Mapa de territorio y Retos mensuales empatan exactamente en **3,82** (107 puntos de ranking cada uno); su orden relativo no es concluyente.
- Logros queda claramente por delante de ambos (`3,04` frente a `3,82`), y Clubes y Feed quedan claramente al final (`5,21` y `5,32`). Rankings (`4,25`) está en un punto intermedio.
- Clubes empata con Retos en votos como #1 (3 cada uno) pero queda 6.º en promedio: hay un grupo pequeño al que le importa mucho y una mayoría a la que le importa poco.
- El ranking es forzado: quedar en los últimos puestos significa *"menos prioritario"*, no *"no lo quieren"*.

### 4. Limitaciones de la muestra

- **Muestra por conveniencia:** todas las respuestas llegaron desde el mismo enlace de la encuesta, en tres días, y las direcciones IP apuntan a un entorno mayormente local. No es representativa de todos los corredores.
- 9 de 28 no usan app y 11 de 28 corren una sola vez por semana; parte de la muestra puede estar lejos del corredor objetivo de Terrun.
- Con `n = 28`, las diferencias pequeñas entre promedios no son concluyentes; la encuesta sirve para orientar prioridades, no para fijarlas de forma definitiva.

### 5. Conclusión y orden de prioridad de `P1` (`D-29`)

1. Métricas ratifica su lugar como núcleo `P0`. Mapa de territorio queda empatado en 3.º lugar con Retos, por debajo de Logros (`P1`), y el interés por conquistar zonas es positivo pero moderado (promedio `3,46`; `46,4 %` alto). Se confirma como `P0` por ser la propuesta central del producto, no porque los corredores lo pidan por encima de todo; conviene reforzar su propuesta de valor en la beta.
2. Entre los módulos **`P1`**, los datos sugieren que la motivación por metas personales (**Logros `3,04`** y **Retos `3,82`**) pesa más que el componente social (**Clubes `5,21`** y **Feed `5,32`**), con **Rankings (`4,25`)** en el punto intermedio. Las 5 respuestas nuevas no cambiaron el orden de los `P1`.
3. En caso de requerir recorte al cierre de `P0` (regla R-03), se prioriza conservar **Logros/XP, Retos y Rankings** por encima de **Clubes y Feed social**. Los avisos de voz se mantienen como preferencia desactivable de bajo costo, sin competir en prioridad con los módulos del ranking.
4. Se recomienda evaluar en el backlog una función de **guía de ritmo/zonas de entrenamiento**, por ser una necesidad recurrente en las respuestas abiertas.
