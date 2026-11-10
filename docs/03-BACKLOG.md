# 03 — Backlog del producto

**Dueña:** Diana (Product Owner). **Prioridades:** P0 = imprescindible para la demo · P1 = nivel de mercado · P2 = posterior a la entrega.

Cada historia tiene un ID `CTZ-NN` que se usa en ramas, commits y PR. Las referencias "LLD §x" indican qué leer antes de empezar. Este archivo es el **plan**; el estado del sprint en curso vive en `docs/ESTADO.md`.

Regla de recorte (HLD §11.1): si al 11 de diciembre el P0 no está completo, los P1 de menor prioridad pasan a P2 sin discusión. El orden de los P1 lo ajusta Diana con la encuesta a corredores (CTZ-25).

---

## S0 — 10 al 13 de noviembre · Diseño y arranque

**Objetivo:** HLD y LLD entregados; repositorio, CI y esqueletos listos para empezar a construir el lunes.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-01 | Entregar el HLD y el LLD al profesor | Ashley | P0 | — | PDFs entregados el 11/11; correcciones del profesor registradas en `DECISIONES.md` |
| CTZ-02 | Crear el repositorio en GitHub (cuenta AshFranco), invitar al equipo, proteger `main` (PR + 1 aprobación + CI) | Ashley | P0 | — | Los 6 integrantes con acceso; push directo a `main` rechazado |
| CTZ-03 | Esqueleto del backend: solución .NET 10 con los 4 proyectos y 2 de pruebas (LLD §1.2), `/health`, Swagger, Docker Compose con PostGIS, CI de build, pruebas y `dotnet format`; `Directory.Build.props` con `Nullable` y `TreatWarningsAsErrors`; `UseSnakeCaseNamingConvention` | Ashley | P0 | CTZ-02 | `docker compose up` + `dotnet test` en verde en máquina limpia; CI corre en cada PR y falla si el formato no cumple |
| CTZ-04 | Cuentas de servicios: Neon (ramas `qa` y `main`), Render (servicio qa), Expo (organización del equipo) | Ashley | P0 | — | Credenciales en el gestor del equipo, nunca en el repo |
| CTZ-05 | Enviar la encuesta al grupo de corredores (`docs/producto/encuesta-corredores.md`) | Diana | P0 | — | Formulario publicado y compartido; meta: 15 respuestas antes del 18/11 |
| CTZ-06 | Cargar el backlog en GitHub Projects (un issue por historia, con etiqueta de sprint y responsable) | Diana | P1 | CTZ-02 | Tablero con columnas Pendiente / En curso / En revisión / Hecha |
| CTZ-07 | Confirmar con Jorge y Rowlis que dominan React (supuesto S-01 del HLD) | Diana | P0 | — | Resultado en `DECISIONES.md` |
| CTZ-08 | Esqueleto de la app: Expo SDK con TypeScript, Expo Router, pestañas vacías (Mapa, Feed, Grabar, Clubes, Perfil), TypeScript `strict`, ESLint (sin `any`) + Prettier, Jest, script `api:types`, carpetas `src/i18n/es.json` y `src/theme/` (estándares §5) | Jorge | P0 | CTZ-02 | `npx expo start` abre la app en Android e iOS; lint y pruebas en verde en la CI |
| CTZ-09 | Investigación previa del GPS: configuración de expo-location + TaskManager y de MapLibre RN en un *development build* | Jorge | P0 | CTZ-08 | Nota en `docs/DECISIONES.md` con librerías, versiones y permisos necesarios |
| CTZ-10 | Sistema de diseño base (colores, tipografía, espaciado, botón, tarjeta, campo de texto) y wireframes de P-01 a P-09 | Rowlis | P0 | — | Componentes en `mobile/src/components` y tokens en `mobile/src/theme`; wireframes enlazados en `docs/producto/` |
| CTZ-11 | Plan de pruebas v1 (`docs/qa/01-PLAN-PRUEBAS.md`): alcance, niveles, ambientes, criterios de entrada y salida por sprint | Rangelis | P0 | — | Revisado por Ashley y Diana |
| CTZ-12 | Plan de pruebas de campo: inventario de dispositivos (modelo, sistema operativo), 2 rutas medidas (1 km y 5 km), checklist de batería y segundo plano | Jaime | P0 | — | `docs/qa/03-PRUEBAS-CAMPO.md` con las rutas y los dispositivos |

## S1 — 14 al 20 de noviembre · Cuenta, perfil y prueba de GPS

**Objetivo:** una persona se registra, verifica su correo, inicia sesión y edita su perfil en la app conectada a QA; el GPS en segundo plano está probado en dispositivos reales.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-13 | Migración inicial: tablas de M1, M2 y M13 + datos iniciales de `system_setting` y `legal_document` (LLD §2.3, §2.4) | Ashley | P0 | CTZ-03 | La migración se aplica desde cero; prueba de integración de restricciones (UK de email y handle) |
| CTZ-14 | Registro, verificación de correo, login, refresh rotativo y logout (LLD §3.2 M1, §7.1) | Ashley | P0 | CTZ-13 | Reutilizar un refresh rotado revoca la familia; contraseña débil → 400; correo repetido → 409 |
| CTZ-15 | Recuperar y restablecer contraseña por correo | Ashley | P0 | CTZ-14 | `/auth/forgot-password` siempre 202; token de 1 h, de un solo uso; restablecer revoca las sesiones |
| CTZ-16 | `GET /me`, `PATCH /me/profile`, `PUT /me/preferences`, `GET /users/{id}` | Ashley | P0 | CTZ-14 | Validaciones del LLD; un perfil privado oculta los datos a quien no lo sigue |
| CTZ-17 | `/legal/current`, `/me/consents`, `/settings/public`, `/health` | Ashley | P0 | CTZ-13 | El registro exige aceptar las versiones vigentes |
| CTZ-18 | Despliegue de QA: Render + Neon `qa`, despliegue automático al hacer merge a `main` | Ashley | P0 | CTZ-04, CTZ-03 | URL de QA respondiendo `/health`; Swagger visible |
| CTZ-19 | OpenAPI publicado y tipos de la app generados (`npm run api:types`) | Ashley + Rowlis | P0 | CTZ-16, CTZ-08 | Los tipos compilan en la app; el procedimiento está documentado en el README |
| CTZ-20 | **Prueba de concepto del GPS en segundo plano**: *development build* Android e iOS que graba 30 min con la pantalla bloqueada y exporta los puntos | Jorge | P0 | CTZ-09 | Informe en `docs/qa/` con la pérdida de puntos y la batería por dispositivo; veredicto para el hito del 20/11 |
| CTZ-21 | Mapa base MapLibre en la pestaña Mapa, centrado en la ubicación actual | Jorge | P0 | CTZ-08 | Mapa fluido en ambos sistemas; sin claves en el código |
| CTZ-22 | Navegación (grupo de autenticación + pestañas), cliente API, sesión en SecureStore con refresh automático | Rowlis | P0 | CTZ-08 | Un 401 renueva el token una vez; un refresh inválido envía al login |
| CTZ-23 | Pantallas P-01 a P-05 (bienvenida, registro, login, verificar correo, recuperar) | Rowlis | P0 | CTZ-22, CTZ-14 | Flujo completo contra QA; errores del catálogo (LLD §3.4) traducidos a mensajes claros |
| CTZ-24 | P-17 editar perfil y P-30 preferencias | Rowlis | P0 | CTZ-16 | Los cambios persisten y se reflejan al reabrir la app |
| CTZ-25 | Analizar la encuesta y reordenar los P1 | Diana | P0 | CTZ-05 | Resumen en `docs/producto/` y decisión en `DECISIONES.md` |
| CTZ-26 | Criterios de aceptación detallados de S2 y S3 | Diana | P0 | — | Historias de S2/S3 cumplen la Definition of Ready el viernes 20 |
| CTZ-27 | Borrador de términos y política de privacidad v1 (ubicación, Ley 172-13, eliminación de cuenta) | Diana | P1 | — | Textos en `docs/producto/legal/` revisados por Ashley |
| CTZ-28 | Casos de prueba de M1/M2 y colección de API (Bruno) contra QA; registro de defectos activo | Rangelis | P0 | CTZ-18 | Colección en `docs/qa/coleccion-api/`; resultados en el informe del sprint |
| CTZ-29 | Prueba de campo del CTZ-20 en al menos 3 dispositivos (1 iPhone, 2 Android de marcas distintas) | Jaime | P0 | CTZ-20 | Resultados en `docs/qa/03-PRUEBAS-CAMPO.md` |

## S2 — 21 al 27 de noviembre · Actividades

**Objetivo:** grabar una carrera real, guardarla sin conexión, subirla y ver distancia, ritmo y splits calculados por el servidor.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-30 | Tablas de M3/M4 y `POST /activities` idempotente con validación de tamaño y superposición | Ashley | P0 | CTZ-13 | Mismo `clientId` → 200 con la existente; > 30.000 puntos → 413 (MT-09) |
| CTZ-31 | Procesador: limpieza, antitrampas básicas, métricas, splits, ruta simplificada (LLD §4.1, §6) | Ashley | P0 | CTZ-30 | MT-01 a MT-05, MT-14 y MT-15 en verde; actividad de 1 h procesada en < 10 s |
| CTZ-32 | `GET`/`PATCH`/`DELETE /activities/{id}`, `/route`, `/splits`, `/me/activities`, `/me/summary` | Ashley | P0 | CTZ-31 | Paginación por cursor; solo el dueño edita o elimina |
| CTZ-33 | Grabador completo P-08: máquina de estados, auto-pausa, segmentos, SQLite, recuperación tras cierre forzado (LLD §5.1) | Jorge | P0 | CTZ-20 | Matar la app en plena carrera no pierde puntos; métricas en vivo |
| CTZ-34 | Cola de subida con reintentos y P-09 resumen con sondeo del procesamiento | Jorge | P0 | CTZ-33, CTZ-30 | Modo avión → la actividad queda en cola y se sube sola al volver la red |
| CTZ-35 | P-10 historial, P-11 detalle (ruta y splits sin social), P-12 editar o eliminar | Rowlis + Jorge (mapa de P-11) | P0 | CTZ-32 | Datos idénticos a los de la API |
| CTZ-36 | P-15 mi perfil con estadísticas básicas | Rowlis | P0 | CTZ-32 | Totales iguales a `/me` |
| CTZ-37 | Contenido inicial: 20 logros y 3 retos (nombres, reglas, XP) | Diana | P1 | — | Tabla en `docs/producto/` lista para el seed |
| CTZ-38 | Pruebas de API de actividades (idempotencia, validaciones, permisos) | Rangelis | P0 | CTZ-32 | Casos ejecutados y defectos registrados |
| CTZ-39 | Campo: precisión (RNF-01), segundo plano (RNF-02), sin conexión (RNF-04) | Jaime | P0 | CTZ-34 | Error de distancia < 3 % en la ruta medida, o defecto registrado |

## S3 — 28 de noviembre al 4 de diciembre · Territorio

**Objetivo:** al terminar una carrera aparecen las zonas en progreso y conquistadas en el mapa.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-40 | Tablas de M5 y motor de territorio: densificación, sub-celdas H3, máscara, conquista 5/7 (LLD §4.2) | Ashley | P0 | CTZ-31 | MT-06, MT-07, MT-08, MT-09 y MT-18 en verde; cobertura del motor ≥ 80 % |
| CTZ-41 | `GET /zones?bbox=` (GeoJSON con estado), `/zones/{h3}`, `/me/territory`, `/me/conquests` | Ashley | P0 | CTZ-40 | Hasta 2.000 zonas en < 1 s; bbox mayor → 422 |
| CTZ-42 | Rankings en vivo: zonas conquistadas y distancia; global y por ciudad | Ashley | P0 | CTZ-40 | Mi posición incluida aunque no esté en el top |
| CTZ-43 | Capa de hexágonos en P-07: colores por estado, recarga por área visible, aviso al alejar demasiado | Jorge | P0 | CTZ-41 | 60 fps en el desplazamiento con 500 hexágonos en gama media |
| CTZ-44 | P-13 detalle de zona (dibujo de las 7 sub-celdas) y zonas nuevas en P-09 | Jorge | P0 | CTZ-41 | La máscara dibujada coincide con la de la API |
| CTZ-45 | P-23 rankings | Rowlis | P0 | CTZ-42 | Cambio de métrica y de alcance sin recargar la pantalla |
| CTZ-46 | P-06 permisos y onboarding (explicación del permiso de ubicación) | Rowlis + Jorge | P0 | CTZ-33 | Textos aprobados por Diana; denegar el permiso no rompe la app |
| CTZ-47 | Demo interna con 3 corredores del grupo; ajustar parámetros | Diana | P1 | CTZ-44 | Cambios en `DECISIONES.md` |
| CTZ-48 | Verificar a mano los casos MT del motor junto con Ashley | Rangelis | P0 | CTZ-40 | Los cálculos a mano coinciden con las pruebas |
| CTZ-49 | Campo: conquistar 3 zonas reales y comparar con el mapa | Jaime | P0 | CTZ-43 | Evidencia con capturas en `docs/qa/` |

## S4 — 5 al 11 de diciembre · Hito P0 completo

**Objetivo:** P0 completo en producción: dominio competitivo, XP y Google Sign-In.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-50 | Dominio de 30 días, `zone_event` y trabajo `NightlyOwnershipJob` (LLD §4.3) | Ashley | P1 | CTZ-40 | MT-10, MT-11 y MT-12 en verde; sin condiciones de carrera con 2 actividades simultáneas |
| CTZ-51 | XP, niveles y `user_stats` (LLD §4.4) | Ashley | P1 | CTZ-40 | MT-17 en verde; XP nunca duplicado |
| CTZ-52 | `POST /auth/google` | Ashley | P1 | CTZ-14 | Token de otra audiencia → 401; vincula con una cuenta verificada del mismo correo |
| CTZ-53 | Producción: Render prod + Neon `main`, promoción manual desde QA | Ashley | P0 | CTZ-18 | URL de producción estable; procedimiento en `docs/` |
| CTZ-54 | Estados `owned_by_me` / `owned_by_other` en el mapa y eventos en P-13 | Jorge | P1 | CTZ-50 | Colores y leyenda aprobados por Diana |
| CTZ-55 | Registro del token push en P-06 (preparación para S5) | Jorge | P1 | CTZ-46 | Token guardado tras conceder el permiso |
| CTZ-56 | Google Sign-In en P-03 | Rowlis | P1 | CTZ-52 | Funciona en Android e iOS |
| CTZ-57 | Nivel y XP en P-15 y en P-09 | Rowlis | P1 | CTZ-51 | Barra de progreso al siguiente nivel |
| CTZ-58 | Aceptación del P0 completo y apertura de las cuentas de Google Play y Apple Developer | Diana | P0 | CTZ-53 | Acta de aceptación en `docs/sprints/S4.md` |
| CTZ-59 | Regresión completa del P0 | Rangelis | P0 | CTZ-53 | Informe con defectos por severidad |
| CTZ-60 | Campo: robo de zona entre 2 usuarios | Jaime | P1 | CTZ-54 | El robo y la notificación ocurren como en MT-10 |

## S5 — 12 al 18 de diciembre · Social, notificaciones y tiendas

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-61 | M6: seguir y solicitudes, feed, kudos, comentarios, bloqueo, reportes y reglas de visibilidad (LLD §7.2) | Ashley | P1 | CTZ-32 | Pruebas de integración de toda la matriz de visibilidad |
| CTZ-62 | M10: notificaciones y trabajo `PushDispatchJob` | Ashley | P1 | CTZ-50 | Tokens inválidos se desactivan; reintentos limitados |
| CTZ-63 | Builds EAS (perfiles qa/prod, ícono, splash), prueba interna en Google Play y TestFlight | Ashley + Jorge | P1 | CTZ-58 | El equipo instala la app desde la tienda |
| CTZ-64 | Push en la app: permisos, recepción y navegación por enlace interno | Jorge | P1 | CTZ-62 | Tocar "te robaron la zona" abre P-13 |
| CTZ-65 | P-14 feed, P-16 perfil de otro, P-18 seguidores, P-19 buscar, kudos y comentarios en P-11, P-27 notificaciones | Rowlis | P1 | CTZ-61 | Funciona con perfiles públicos y privados |
| CTZ-66 | Ficha de tienda (textos y capturas) y política de privacidad publicada en una URL | Diana | P1 | CTZ-27 | URL pública lista para las tiendas |
| CTZ-67 | Pruebas de social y visibilidad (matriz LLD §7.2) | Rangelis | P1 | CTZ-61 | Ninguna fuga de datos privados |
| CTZ-68 | Campo: instalación desde la tienda y push real en los 3 dispositivos | Jaime | P1 | CTZ-63 | Evidencia en `docs/qa/` |

## S6 — 19 al 23 de diciembre · Clubes y gamificación (weekly el miércoles 23)

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-69 | M7: clubes, miembros y territorio del club | Ashley | P1 | CTZ-50 | Siempre queda al menos un admin por club |
| CTZ-70 | Logros (evaluación de reglas), rachas, retos y trabajos `ChallengeCloseJob` y `LeaderboardSnapshotJob` | Ashley | P1 | CTZ-51 | Los 20 logros de CTZ-37 cargados y probados |
| CTZ-71 | Mapa del territorio del club en P-21 | Jorge | P1 | CTZ-69 | Mismo estilo que P-07 |
| CTZ-72 | P-20/P-21/P-22 clubes, P-24 logros y nivel, P-25/P-26 retos | Rowlis | P1 | CTZ-69, CTZ-70 | Flujo de unirse con aprobación |
| CTZ-73 | Crear el club del grupo de corredores y el primer reto de enero | Diana | P1 | CTZ-72 | Club con al menos 10 miembros |
| CTZ-74 | Pruebas de clubes, logros y retos | Rangelis + Jaime | P1 | CTZ-72 | Informe del sprint |

## S7 — 26 al 30 de diciembre · Cuenta completa y moderación (weekly el miércoles 30)

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-75 | Récords, zapatillas, zonas de privacidad (recorte de la ruta) y exportación GPX | Ashley | P1 | CTZ-32 | MT-16 en verde; un GPX válido se abre en otra app |
| CTZ-76 | Apple Sign-In, cambiar contraseña, exportar datos y eliminar cuenta (LLD §4.5) | Ashley | P1 | CTZ-14 | Tras eliminar, sus zonas se recalculan |
| CTZ-77 | Moderación: revisiones, reportes, suspensión, auditoría y administración de parámetros, logros y retos | Ashley | P1 | CTZ-61 | Toda acción queda en `audit_log` |
| CTZ-78 | P-31 zonas de privacidad, exportar GPX y avisos de voz | Jorge | P1 | CTZ-75 | Avisos de voz cada km con la pantalla bloqueada |
| CTZ-79 | P-28 zapatillas, P-29 récords, P-32 cuenta (Apple, exportar, eliminar), P-33 reportar | Rowlis | P1 | CTZ-75, CTZ-76 | Apple Sign-In probado en iPhone |
| CTZ-80 | Manual de usuario y lista de al menos 12 testers con correo de Google | Diana | P1 | — | Lista lista para el 31/12 |
| CTZ-81 | Pruebas de seguridad básicas (IDOR, autenticación, límites de tasa) | Rangelis | P1 | CTZ-77 | Informe con hallazgos |
| CTZ-82 | Campo: antitrampas (bicicleta y carro) | Jaime | P1 | CTZ-31 | Actividades marcadas `in_review` o sin territorio |

## S8 — 31 de diciembre al 6 de enero · Cierre y congelamiento

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-83 | Cerrar los defectos abiertos de severidad alta y media | Ashley, Jorge, Rowlis | P0 | — | Cero defectos altos abiertos |
| CTZ-84 | Versión candidata (1.0.0-rc) en QA y en las tiendas | Ashley + Jorge | P0 | CTZ-83 | Instalable desde ambas tiendas |
| CTZ-85 | Abrir la prueba cerrada de Google Play con los 12+ testers (14 días) | Diana + Jaime | P0 | CTZ-80, CTZ-84 | Prueba abierta el 6/1 |
| CTZ-86 | **Congelamiento de código (6 de enero)** | Ashley | P0 | — | A partir de aquí solo entran correcciones de defectos |

## QA 1 y QA 2 — 7 al 19 de enero

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-87 | Ciclo completo de pruebas (funcionales, API, campo, regresión E2E) | Rangelis + Jaime | P0 | CTZ-86 | Todos los casos ejecutados al 13/1 |
| CTZ-88 | Corrección de defectos | Ashley, Jorge, Rowlis | P0 | CTZ-87 | Cero defectos altos al 18/1 |
| CTZ-89 | Informe final de pruebas | Rangelis | P0 | CTZ-87 | `docs/qa/05-INFORME-FINAL.md` |
| CTZ-90 | Guion de la demo, presentación y ensayo | Diana + Ashley | P0 | CTZ-88 | Ensayo completo el 18/1 |
| CTZ-91 | Publicación en producción en Google Play y App Store (si los plazos de revisión lo permiten) | Ashley + Diana | P1 | CTZ-85 | App visible en la tienda o, si no, en prueba cerrada |

## P2 — Después de la entrega (fase comercial)

Importación desde Strava, Garmin y GPX/FIT · HealthKit y Health Connect · fotos en actividades · temporadas · planes de entrenamiento · suscripción premium · panel web de administración.
