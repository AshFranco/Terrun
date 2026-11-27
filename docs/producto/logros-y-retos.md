# Catálogo inicial de logros y retos (`CTZ-37`)

**Responsable:** Diana (Product Owner) · **Fecha:** 27/11/2026  
**Referencias:** `RF-25`, `RF-26`, `CTZ-70`, `docs/02-LLD.md` (§2.3 tablas `achievement` y `challenge`, §2.4 `xp.*`, §4.4 Progresión).

Este catálogo define los **20 logros iniciales** y los **3 retos globales iniciales** listos para cargarse en la semilla del backend (`CTZ-70`). Todas las reglas utilizan exclusivamente las métricas soportadas por el motor de progresión (`LLD §4.4`) y respetan las restricciones de esquema (`code <= 40`, `name <= 60`, `description <= 200`, `title <= 80`, `challenge.description <= 500`).

---

## 1. Principios de diseño (Gamificación responsable e inclusión)

- **Motivación *White Hat* (Octalysis):** prioridad al logro personal (*Development & Accomplishment*), sentido territorial (*Epic Meaning*), pertenencia (*Ownership*) y exploración (*Curiosity*), sin mecánicas punitivas ni FOMO artificial.
- **Accesible desde el día 1 (*First Major Win-State*):** `Primera huella` (1 actividad) y `Bandera plantada` (1 zona conquistada) brindan victorias inmediatas para sostener la activación; **17 de los 20 logros** son alcanzables durante un piloto de 1 a 2 meses y los 3 de *Endgame* (`Atlas viviente`, `Espíritu 21K`, `Centenario urbano`) ofrecen recorrido a largo plazo.
- **Lenguaje inclusivo y sin sesgo de velocidad:** ninguno de los logros ni retos exige ritmo rápido ni excluye a personas que caminan, trotan, corren o utilizan prótesis deportivas o silla de ruedas manual (`terminos-v1.0.md §6.1`). Las rachas son **semanales** (no diarias) para respetar descansos, salud y disponibilidad.
- **Nota sobre `category = social`:** el enum `achievement_category` incluye `social`, pero ninguna de las 9 métricas actuales de `LLD §4.4` evalúa seguidores, clubes o kudos (etapa P2). Los 20 logros iniciales cubren `consistency`, `territory`, `distance` y `special`; los logros sociales se añadirán cuando el motor soporte esas métricas.

---

## 2. Catálogo inicial de 20 logros (`achievement`)

Todos los registros se insertan con `is_active = true`. Los valores de `icon` son claves semánticas de referencia (compatibles con Ionicons / MaterialCommunityIcons para que el equipo móvil los mapee en `P-24`).

| `sort_order` | `code` | `name` | `category` | `description` | `icon` | `rule` (`jsonb`) | `xp` | `is_active` |
|---:|---|---|---|---|---|---|---:|---|
| 1 | `first_footprint` | Primera huella | `consistency` | Completaste tu primera actividad en Terrun. Toda ruta empieza aquí. | `footsteps-outline` | `{"metric":"total_activities","threshold":1}` | 50 | `true` |
| 2 | `steady_pulse` | Pulso constante | `consistency` | Cinco salidas registradas: ya estás construyendo tu propio ritmo. | `pulse-outline` | `{"metric":"total_activities","threshold":5}` | 100 | `true` |
| 3 | `unstoppable_habit` | Hábito en marcha | `consistency` | Quince actividades completadas. Moverte ya es parte de tu semana. | `repeat-outline` | `{"metric":"total_activities","threshold":15}` | 200 | `true` |
| 4 | `two_week_spark` | Doble semana activa | `consistency` | Dos semanas seguidas saliendo al menos una vez. Tu constancia se nota. | `flame-outline` | `{"metric":"streak_weeks","threshold":2}` | 100 | `true` |
| 5 | `golden_month` | Mes de oro | `consistency` | Cuatro semanas consecutivas en movimiento, a tu ritmo y sin rendirte. | `calendar-outline` | `{"metric":"streak_weeks","threshold":4}` | 250 | `true` |
| 6 | `first_territory` | Bandera plantada | `territory` | Conquistaste tu primera zona (5 de 7 sub-celdas). Ya dejaste marca en el mapa. | `flag-outline` | `{"metric":"zones_conquered","threshold":1}` | 50 | `true` |
| 7 | `neighborhood_trail` | Rutas del barrio | `territory` | Cinco zonas conquistadas: empiezas a conocer cada esquina de tu sector. | `map-outline` | `{"metric":"zones_conquered","threshold":5}` | 150 | `true` |
| 8 | `urban_compass` | Brújula urbana | `territory` | Quince zonas en tu historial. Ninguna calle te detiene. | `compass-outline` | `{"metric":"zones_conquered","threshold":15}` | 300 | `true` |
| 9 | `city_atlas` | Atlas viviente | `territory` | Treinta zonas conquistadas: convertiste la ciudad en tu propio tablero. | `globe-outline` | `{"metric":"zones_conquered","threshold":30}` | 500 | `true` |
| 10 | `home_turf` | Cancha propia | `territory` | Tomaste el dominio activo de tu primera zona en los últimos 30 días. | `shield-outline` | `{"metric":"zones_owned","threshold":1}` | 75 | `true` |
| 11 | `sector_bastion` | Bastión del sector | `territory` | Dominas cinco zonas a la vez. Tu presencia manda en el mapa. | `shield-checkmark-outline` | `{"metric":"zones_owned","threshold":5}` | 200 | `true` |
| 12 | `territory_shift` | Cambio de mando | `territory` | Superaste la marca reciente de otra persona y tomaste el dominio de su zona. | `swap-horizontal-outline` | `{"metric":"total_steals","threshold":1}` | 75 | `true` |
| 13 | `bold_challenger` | Espíritu retador | `territory` | Cinco dominios disputados y ganados con esfuerzo propio en la calle. | `trophy-outline` | `{"metric":"total_steals","threshold":5}` | 200 | `true` |
| 14 | `five_k_milestone` | Meta 5K | `distance` | Recorriste 5 km en una sola salida, a tu propio paso. | `ribbon-outline` | `{"metric":"activity_distance_m","threshold":5000}` | 100 | `true` |
| 15 | `ten_k_milestone` | Horizonte 10K | `distance` | Alcanzaste los 10 km en una misma actividad. Gran resistencia. | `medal-outline` | `{"metric":"activity_distance_m","threshold":10000}` | 200 | `true` |
| 16 | `half_marathon_feat` | Espíritu 21K | `distance` | Completaste la distancia de medio maratón (21,1 km) en una sola jornada. | `star-outline` | `{"metric":"activity_distance_m","threshold":21097}` | 400 | `true` |
| 17 | `distance_25k` | Veinticinco en ruta | `distance` | Sumaste 25 km acumulados explorando tus calles. | `trail-sign-outline` | `{"metric":"total_distance_m","threshold":25000}` | 150 | `true` |
| 18 | `distance_100k` | Centenario urbano | `distance` | ¡100 km acumulados en Terrun! Cada kilómetro cuenta en tu historia. | `diamond-outline` | `{"metric":"total_distance_m","threshold":100000}` | 350 | `true` |
| 19 | `dawn_patrol` | Al alba | `special` | Saliste antes de las 7:00 a. m. mientras la ciudad apenas despertaba. | `sunny-outline` | `{"metric":"started_before_hour","threshold":7}` | 100 | `true` |
| 20 | `rising_legend` | Ascenso nivel 5 | `special` | Alcanzaste el nivel 5 sumando constancia, distancia y territorio. | `trending-up-outline` | `{"metric":"level","threshold":5}` | 200 | `true` |

---

## 3. Catálogo inicial de 3 retos globales (`challenge`)

Todos son retos globales (`club_id = null`, `is_active = true`). La columna `duration_days` sirve de referencia para fijar `starts_at` y `ends_at` (`ends_at > starts_at`) al momento de ejecutar el *seed* en QA o producción.

| # | `title` | `description` | `metric` | `goal` | `duration_days` | `xp` | `club_id` | `is_active` |
|---:|---|---|---|---:|---:|---:|---|---|
| 1 | Desafío de Arranque: 4 salidas a tu ritmo | Registra 4 actividades válidas caminando, trotando o corriendo en 2 semanas. Sin importar la velocidad, lo que cuenta es salir. | `activities` | `4.00` | 14 | 200 | `null` | `true` |
| 2 | Exploración de Barrio: 5 zonas nuevas | Descubre calles nuevas y conquista 5 zonas completas (5 de 7 sub-celdas) durante el mes. | `zones_conquered` | `5.00` | 30 | 300 | `null` | `true` |
| 3 | Travesía Urbana: 25 km acumulados | Acumula 25 km en un mes sumando todas tus salidas. Ideal para construir fondo y constancia. | `distance_m` | `25000.00` | 30 | 350 | `null` | `true` |

---

## 4. Criterios de accesibilidad e inclusión para la implementación UI (`P-24`, `P-25`)

> **Alcance de esta revisión:** estos criterios corresponden a la fase de diseño del catálogo (`CTZ-37`). No sustituyen la verificación manual ni las pruebas con lectores de pantalla (TalkBack / VoiceOver) sobre el *build* real en QA.

- **No depender solo del color:** el estado de cada logro (*bloqueado*, *en progreso*, *desbloqueado*) y el avance de cada reto deben comunicarse mediante texto e ícono/etiqueta, no únicamente por cambio de color o saturación.
- **Movimiento reducido:** si el sistema operativo tiene activa la preferencia de reducir movimiento, las celebraciones al desbloquear un logro o completar un reto deben mostrarse sin animaciones bruscas ni destellos.
- **Avisos legibles sin perderse:** las notificaciones de logro desbloqueado deben quedar consultables en la bandeja (`P-27`) y en `P-24`, evitando depender solo de un *toast* efímero que desaparezca antes de que el usuario o un lector de pantalla termine de leerlo.
- **Valor básico intacto:** ninguna medalla ni reto condiciona el acceso a las funciones básicas de trackeo, métricas o conquista de zonas.
