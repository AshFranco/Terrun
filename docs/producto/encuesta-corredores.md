# Encuesta a corredores (CTZ-05)

Meta: 15 respuestas antes del 18/11. Se comparte por WhatsApp.

## Dónde vive

- **Página:** `survey/index.html`, con una demo animada de cómo se conquista una zona (calles reales de Piantini, `survey/map-data.js`). Es una sola página en HTML, CSS y JavaScript, sin build.
- **Respuestas:** las guarda **Netlify Forms** (formulario `runner-survey`). Nadie tiene que iniciar sesión para responder. Las respuestas se ven y se descargan en CSV desde el panel de Netlify, en *Forms*. El plan gratis guarda hasta 100 respuestas al mes.
- **Vista previa para el equipo:** https://claude.ai/artifact/LQkKUP9vBAPeFSR3CWamos (privado; compartir desde *Share*). Desde ahí las respuestas **no** se guardan.
- **Mapa:** `python3 scripts/build_survey_map.py <export-de-overpass.json>` regenera `survey/map-data.js`.

### Publicar (una sola vez)

1. En https://app.netlify.com: *Add new site* → *Import an existing project* → GitHub → `AshFranco/Terrun`. Netlify lee `netlify.toml` (publica la carpeta `survey/`, sin comando de build).
2. *Site configuration* → *Change site name*: `terrun-encuesta` (queda `https://terrun-encuesta.netlify.app`).
3. *Forms* → *Enable form detection* y luego *Deploys* → *Trigger deploy*. Sin este paso Netlify no registra el formulario.
4. Probar: responder una vez desde el teléfono, ver la respuesta en *Forms* → `runner-survey` y borrarla.
5. *Forms* → *Form notifications*: agregar un aviso por correo para cada respuesta nueva (opcional).

Cada merge a `main` vuelve a publicar la página. Si se conecta un dominio propio, hay que agregarlo en `isLive` dentro de `survey/index.html`; si no, la página se queda en modo vista previa.

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

Completar al cerrar la encuesta: número de respuestas, resultados por pregunta y el orden de los P1 que se deriva de ellos (registrar la decisión en `docs/DECISIONES.md`).
