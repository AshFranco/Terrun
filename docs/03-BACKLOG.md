# 03 — Backlog del producto

**Dueña:** Diana (Product Owner). **Prioridades:** P0 = imprescindible para la entrega y demo del MVP (`HLD v2.0`) · P1 = complemento competitivo y social del período académico (`HLD v2.0`, condicional a tener cerrado el bloque P0) · P2 = evolución posterior al MVP y fase comercial (`HLD v1.2-post-MVP` / `LLD v1.2`).

Cada historia tiene un ID `CTZ-NN` (los IDs nuevos se agregan al final de la numeración o con sufijo de subdivisión sin renumerar el resto) que se usa en ramas, commits y PR. Las referencias "LLD §x" y "RF-xx / RNF-xx" indican qué leer antes de empezar. Este archivo es el **plan completo del producto**; el estado del sprint en curso vive en `docs/ESTADO.md`.

Regla de recorte (`01-HLD-v1.2-post-MVP.md`, Anexo C, riesgo R-03): si al 11 de diciembre el P0 no está completo, los P1 de menor prioridad pasan a P2 sin discusión. El orden de los P1 lo ajusta Diana con la encuesta a corredores (`CTZ-25`).

---

## S0 — 10 al 13 de noviembre · Diseño y arranque

**Objetivo:** HLD y LLD entregados; repositorio, CI y esqueletos listos para empezar a construir.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-01 | Entregar el HLD y el LLD al profesor | Ashley / Diana | P0 | — | PDFs entregados; correcciones del profesor registradas en `DECISIONES.md` (`D-17`: HLD v2.0 para el período académico y HLD v1.2 conservado post-MVP) |
| CTZ-02 | Crear el repositorio en GitHub (`AshFranco/Terrun`), invitar al equipo, proteger `main` (PR + 1 aprobación + CI) | Ashley | P0 | — | Los 6 integrantes con acceso; push directo a `main` rechazado |
| CTZ-03 | Esqueleto del backend: solución .NET 10 con los 4 proyectos y 2 de pruebas (LLD §1.2), `/health`, Swagger, Docker Compose con PostGIS, CI de build, pruebas y `dotnet format`; `Directory.Build.props` con `Nullable` y `TreatWarningsAsErrors`; `UseSnakeCaseNamingConvention` | Ashley | P0 | CTZ-02 | `docker compose up` + `dotnet test` en verde en máquina limpia; CI corre en cada PR y falla si el formato no cumple |
| CTZ-04 | Cuentas de servicios: Neon (ramas `qa` y `main`), Render (servicio `qa`), Expo (organización del equipo) | Ashley | P0 | — | Credenciales en el gestor del equipo, nunca en el repo |
| CTZ-05 | Enviar la encuesta al grupo de corredores (`docs/producto/encuesta-corredores.md`) | Diana | P0 | — | Formulario publicado en Netlify y compartido; meta superada con 23 respuestas reales |
| CTZ-06 | Cargar el backlog en GitHub Projects (un issue por historia, con etiqueta de sprint y responsable) | Diana | P1 | CTZ-02 | Tablero con columnas Pendiente / En curso / En revisión / Hecha |
| CTZ-07 | Confirmar con Jorge y Rowlis que dominan React (supuesto S-01 del HLD v1.2) | Diana | P0 | — | Resultado registrado en `DECISIONES.md` (`D-15`) |
| CTZ-08 | Esqueleto de la app: Expo SDK con TypeScript, Expo Router, pestañas vacías (Mapa, Feed, Trackear, Clubes, Perfil), TypeScript `strict`, ESLint (sin `any`) + Prettier, Jest, script `api:types`, carpetas `src/i18n/es.json` y `src/theme/` (estándares §5) | Rowlis | P0 | CTZ-02 | `npx expo start` abre la app en Android (e iOS si disponible); lint y pruebas en verde en la CI |
| CTZ-09 | Configuración técnica de `expo-location` + `TaskManager` y de MapLibre RN en un *development build* | Rowlis + Jorge | P0 | CTZ-04, CTZ-08 | Nota en `docs/DECISIONES.md` con librerías, versiones y permisos necesarios (se documenta en paralelo con CTZ-20) |
| CTZ-10 | Sistema de diseño base (colores, tipografía, espaciado, botón, tarjeta, campo de texto) y wireframes de P-01 a P-09 | Jorge | P0 | — | Componentes en `mobile/src/components` y tokens en `mobile/src/theme`; wireframes enlazados en `docs/producto/` |
| CTZ-11 | Plan de pruebas v1 (`docs/qa/01-PLAN-PRUEBAS.md`): alcance (`M1–M15`), niveles, ambientes, criterios de entrada y salida por sprint | Rangelis | P0 | — | Revisado por Ashley y Diana |
| CTZ-12 | Plan de pruebas de campo: inventario de dispositivos reales del equipo (modelo, sistema operativo), 2 rutas medidas (1 km y 5 km), checklist de batería y segundo plano | Jaime | P0 | — | `docs/qa/03-PRUEBAS-CAMPO.md` con las rutas y los dispositivos disponibles |

## S1 — 14 al 20 de noviembre · Cuenta, perfil y prueba de GPS

**Objetivo:** una persona se registra, verifica su correo, inicia sesión y edita su perfil en la app conectada a QA; el GPS en segundo plano está probado en teléfonos Android reales.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-13 | Migración inicial: tablas de M1, M2 y M13 + datos iniciales de `system_setting` y `legal_document` (LLD §2.3, §2.4) | Ashley | P0 | CTZ-03 | La migración se aplica desde cero; preferencias del corredor se crean activas por defecto (`auto-pausa` y avisos activos) |
| CTZ-14 | Registro, verificación de correo, login, refresh rotativo y logout (`RF-01`, `RF-02`; LLD §3.2 M1, §7.1) | Ashley | P0 | CTZ-13 | Reutilizar un refresh rotado revoca la familia; contraseña débil → 400; correo repetido → 409 |
| CTZ-15 | Recuperar y restablecer contraseña por correo (`RF-03`) | Rowlis (revisa Ashley) | P1 | CTZ-14 | `/auth/forgot-password` siempre 202; token de 1 h, de un solo uso; restablecer revoca las sesiones |
| CTZ-16 | `GET /me` y `PATCH /me/profile` (`RF-07`) con preferencias inicializadas activas por defecto | Ashley | P0 | CTZ-14 | Validaciones del LLD; actualiza datos de perfil propio sin depender de la pantalla de ajustes P1 |
| CTZ-17 | `/legal/current`, `/me/consents`, `/settings/public`, `/health` (`RF-12`) | Rowlis (revisa Ashley) | P0 | CTZ-13, CTZ-27 | El registro exige aceptar las versiones vigentes de términos y privacidad |
| CTZ-18 | Despliegue de QA: Render + Neon `qa`, despliegue automático al hacer merge a `main` | Ashley | P0 | CTZ-04, CTZ-03 | URL de QA respondiendo `/health`; Swagger visible |
| CTZ-19 | OpenAPI publicado y tipos de la app generados (`npm run api:types`) | Ashley + Rowlis | P0 | CTZ-16, CTZ-08 | Los tipos compilan en la app; el procedimiento está documentado en el README |
| CTZ-20 | **Prueba de concepto del GPS en segundo plano (Android)**: *development build* Android que trackea 30 min con la pantalla bloqueada y exporta los puntos | Jorge | P0 | CTZ-04, CTZ-08 | Informe en `docs/qa/` con la pérdida de puntos y la batería por dispositivo; veredicto para el hito del 20/11 |
| CTZ-21 | Mapa base MapLibre en la pestaña Mapa, centrado en la ubicación actual (`RF-19`) | Jorge | P0 | CTZ-08 | Mapa fluido sin claves expuestas en el código |
| CTZ-22 | Navegación (grupo de autenticación + pestañas `RF-08`), cliente API, sesión en `SecureStore` con refresh automático | Rowlis | P0 | CTZ-08 | Un 401 renueva el token una vez; un refresh inválido envía al login |
| CTZ-23 | Pantallas P-01 a P-04 (bienvenida `RF-05`, registro `RF-01`, login `RF-02`, verificar correo) | Rowlis | P0 | CTZ-22, CTZ-14 | Flujo completo de alta e inicio de sesión contra QA; errores del catálogo (LLD §3.4) en español |
| CTZ-23b | Pantalla P-05 recuperar y restablecer contraseña (`RF-03`) | Rowlis | P1 | CTZ-23, CTZ-15 | Flujo completo de recuperación de clave por correo contra QA |
| CTZ-24 | Pantalla P-17 consultar y editar perfil propio (`RF-07`) | Rowlis | P0 | CTZ-16 | Los cambios de nombre, `@handle`, foto, ciudad y datos físicos persisten al reabrir la app |
| CTZ-25 | Analizar las 23 respuestas de la encuesta y confirmar el orden de los P1 | Diana | P0 | CTZ-05 | Resumen en `docs/producto/` y decisión en `DECISIONES.md` |
| CTZ-26 | Criterios de aceptación detallados de S2 y S3 | Diana | P0 | — | Historias de S2/S3 cumplen la *Definition of Ready* el viernes 20 |
| CTZ-27 | Textos de términos y política de privacidad v1 (uso de GPS en segundo plano, recorte de privacidad y Ley 172-13, `RF-12`) | Diana | P0 | — | Textos en `docs/producto/legal/` listos para cargar en CTZ-17 |
| CTZ-28 | Casos de prueba de M1/M2 y colección de API (Bruno) contra QA; registro de defectos activo | Rangelis | P0 | CTZ-18 | Colección en `docs/qa/coleccion-api/`; resultados en el informe del sprint |
| CTZ-29 | Prueba de campo del CTZ-20 en los teléfonos Android reales del inventario (CTZ-12, al menos 2–3 equipos) | Jaime | P0 | CTZ-20 | Resultados en `docs/qa/03-PRUEBAS-CAMPO.md` |

## S2 — 21 al 27 de noviembre · Actividades y trackeo deportivo

**Objetivo:** trackear una carrera real, guardarla sin conexión, subirla y ver distancia, ritmo y splits calculados por el servidor.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-30 | Tablas de M3/M4 y `POST /activities` idempotente con validación de tamaño y superposición (`RF-16`) | Ashley | P0 | CTZ-13 | Mismo `clientId` → 200 con la existente; > 30.000 puntos → 413 (MT-09) |
| CTZ-31a | Limpieza de señal GPS y filtro antitrampas en el servidor (`RF-28`, LLD §4.1, §6) | Ashley | P0 | CTZ-30 | MT-01, MT-02, MT-14 y MT-15 en verde; descarta saltos y tramos en vehículo |
| CTZ-31b | Cálculo de métricas oficiales, *splits* (`RF-17`) y ruta simplificada con recorte de privacidad de los primeros y últimos 200 m (`RNF-07`, LLD §4.1) | Ashley | P0 | CTZ-31a | MT-03, MT-04, MT-05 y MT-16 (recorte 200 m) en verde; carrera de 1 h procesada en < 10 s |
| CTZ-32 | `GET`/`PATCH`/`DELETE /activities/{id}`, `/route`, `/splits`, `/me/activities`, `/me/summary` (`RF-18`) | Rowlis (revisa Ashley) | P0 | CTZ-30 | Paginación por cursor; solo el dueño edita o elimina; devuelve métricas oficiales y ruta recortada al integrarse con CTZ-31b |
| CTZ-33 | Tracker completo P-08 (`RF-13`, `RF-14`): máquina de estados, pausa manual y auto-pausa (activa por defecto), métricas en vivo, `SQLite`, recuperación tras cierre forzado (LLD §5.1) | Jorge | P0 | CTZ-20 | Cerrar forzosamente la app en plena carrera no pierde puntos; métricas en vivo y auto-pausa funcional |
| CTZ-34 | Cola de subida con reintentos (`RF-16`) y P-09 resumen post-carrera con sondeo del procesamiento (`RF-18`) | Jorge | P0 | CTZ-33, CTZ-30 | Modo avión → la actividad queda en cola y se sube sola al volver la red |
| CTZ-35 | P-10 historial, P-11 detalle (ruta y splits) y P-12 editar o eliminar (`RF-18`) | Rowlis + Jorge (mapa de P-11) | P0 | CTZ-32 | Construida contra el contrato OpenAPI; muestra datos idénticos a los de la API |
| CTZ-36 | P-15 mi perfil con estadísticas propias (`RF-07`) | Rowlis | P0 | CTZ-32 | Totales iguales a `/me/summary` |
| CTZ-37 | Catálogo inicial: hasta 20 logros y 3 retos iniciales (nombres, reglas, XP; `RF-25`, `RF-26`) | Diana | P1 | — | Tabla en `docs/producto/` lista para el seed |
| CTZ-38 | Pruebas de API de actividades (idempotencia, validaciones, permisos y recorte de 200 m) | Rangelis | P0 | CTZ-31b, CTZ-32 | Casos ejecutados y defectos registrados |
| CTZ-39 | Campo: precisión (`RNF-01`), segundo plano ≥ 1 h (`RNF-02`) y guardado sin conexión (`S-01`) | Jaime | P0 | CTZ-34 | Error de distancia < 3 % en la ruta medida, o defecto registrado |

## S3 — 28 de noviembre al 4 de diciembre · Territorio H3 y calibración antitrampas

**Objetivo:** al terminar una carrera aparecen las zonas en progreso y conquistadas (regla 5 de 7) en el mapa y se validan en calle los umbrales antitrampas.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-40 | Tablas de M5 y motor de territorio: densificación, sub-celdas H3, máscara y conquista permanente 5/7 (`RF-20`, LLD §4.2) | Ashley | P0 | CTZ-31b | MT-06, MT-07, MT-08, MT-09 y MT-18 en verde; cobertura del motor ≥ 80 % (`RNF-11`) |
| CTZ-41 | `GET /zones?bbox=` (GeoJSON con estado), `/zones/{h3}`, `/me/territory`, `/me/conquests` (`RF-19`, `RF-21`) | Ashley | P0 | CTZ-40 | Hasta 2.000 zonas en < 1 s (`RNF-05`); bbox mayor → 422 |
| CTZ-43 | Capa de hexágonos en P-07 (`RF-19`): colores por estado, recarga por área visible, aviso al alejar demasiado | Jorge | P0 | CTZ-41 | Desplazamiento fluido (< 1 s de carga) con hexágonos del sector en gama media |
| CTZ-44 | P-13 detalle de zona (dibujo de las 7 sub-celdas y balance `RF-21`) y zonas nuevas en P-09 | Jorge | P0 | CTZ-41 | La máscara dibujada coincide con la de la API |
| CTZ-46 | P-06 permisos y onboarding (explicación en español del permiso de ubicación `RF-06`) | Rowlis + Jorge | P0 | CTZ-33 | Textos aprobados por Diana; denegar el permiso no rompe la app |
| CTZ-47 | Demo interna con corredores del grupo y calibración de parámetros del motor | Diana | P1 | CTZ-44, CTZ-82 | Ajustes registrados en `DECISIONES.md` |
| CTZ-93 | Conseguir los límites de los 6 barrios iniciales del Distrito Nacional en GeoJSON (`D-04`) | Diana | P1 | — | Archivo en `docs/producto/barrios/` con Piantini, Naco, Evaristo Morales, Serrallés, Paraíso y Bella Vista |
| CTZ-48 | Verificar a mano los casos MT del motor junto con Ashley | Rangelis | P0 | CTZ-40 | Los cálculos a mano coinciden con las pruebas |
| CTZ-49 | Campo: conquistar 3 zonas reales y comparar con el mapa | Jaime | P0 | CTZ-43 | Evidencia con capturas en `docs/qa/` |
| CTZ-82 | Campo: prueba antitrampas en bicicleta y vehículo (`RF-28`) | Jaime | P0 | CTZ-31a, CTZ-34 | Recorridos en vehículo quedan descartados o sin otorgar territorio |

## S4 — 5 al 11 de diciembre · Hito P0 completo (Dominio de 30 días + Producción + Build EAS) y primeros P1

**Objetivo:** P0 completo en producción e instalable en teléfonos reales (incluyendo dominio competitivo de 30 días y robo de zonas), más XP y Google Sign-In.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-50 | Dominio competitivo de 30 días, robo de zonas, `zone_event` y trabajo `NightlyOwnershipJob` (`RF-22`, LLD §4.3) | Ashley | P0 | CTZ-40 | MT-10, MT-11 y MT-12 en verde; sin condiciones de carrera con 2 actividades simultáneas |
| CTZ-51 | XP, niveles y `user_stats` (`RF-24`, LLD §4.4) | Ashley | P1 | CTZ-40 | MT-17 en verde; XP nunca duplicado |
| CTZ-52 | `POST /auth/google` (`RF-04`) | Rowlis (revisa Ashley) | P1 | CTZ-14 | Token de otra audiencia → 401; vincula con una cuenta verificada del mismo correo |
| CTZ-53 | Producción: Render `prod` + Neon `main`, promoción controlada desde QA | Ashley | P0 | CTZ-18 | URL de producción estable; procedimiento en `docs/` |
| CTZ-63 | Perfiles de compilación EAS (`qa` y `prod`, ícono, splash) y generación de instalador móvil para pruebas y demo (`D-02`) | Ashley + Jorge | P0 | CTZ-04, CTZ-09 | El equipo instala el binario compilado directamente en los teléfonos de prueba |
| CTZ-54 | Estados `owned_by_me` / `owned_by_other` en el mapa y eventos de robo en P-13 (`RF-22`) | Jorge | P0 | CTZ-50 | Colores y leyenda aprobados por Diana |
| CTZ-55 | Registro del token push en P-06 (preparación para S5) | Jorge | P1 | CTZ-46 | Token guardado tras conceder el permiso |
| CTZ-56 | Google Sign-In en P-03 (`RF-04`) | Rowlis | P1 | CTZ-52 | Funciona en Android (e iOS si cuenta disponible) |
| CTZ-57 | Nivel y XP en P-15 y en P-09 (`RF-24`) | Rowlis | P1 | CTZ-51 | Barra de progreso al siguiente nivel |
| CTZ-58 | Aceptación del Hito P0 completo por la Product Owner y decisión sobre disponibilidad de cuenta Apple Developer (`D-01`) | Diana | P0 | CTZ-53, CTZ-54, CTZ-63 | Acta de aceptación del P0 en `docs/sprints/S4.md` |
| CTZ-59 | Regresión completa del P0 | Rangelis | P0 | CTZ-53, CTZ-54, CTZ-63 | Informe con defectos por severidad |
| CTZ-60 | Campo: disputa y robo de zona entre 2 usuarios (`RF-22`) | Jaime | P0 | CTZ-54, CTZ-63 | El robo ocurre como en MT-10 |

## S5 — 12 al 18 de diciembre · Muro con privacidad, notificaciones, voz y clubes

**Objetivo:** completar los complementos de comunidad (Muro con privacidad, notificaciones de robo, avisos de voz por km y clubes) y validar iOS si hay cuenta.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-61 | Backend del Muro de actividad (`Feed`, `RF-09`): publicación de carreras, conquistas, dominios y robos mostrando solo rutas con recorte de privacidad de 200 m | Ashley | P1 | CTZ-32, CTZ-50 | Ninguna respuesta del Feed expone los primeros ni últimos 200 m del trazo |
| CTZ-62 | M10: bandeja de notificaciones y trabajo `PushDispatchJob` para alertas de robo de zona (`RF-10`) | Ashley | P1 | CTZ-50 | Tokens inválidos se desactivan; alerta push y en bandeja al sufrir un robo de zona |
| CTZ-69 | M7: backend de clubes básicos (crear, unirse por código, salir y suma de zonas dominadas del club, `RF-29`) | Rowlis (revisa Ashley) | P1 | CTZ-50 | Siempre queda al menos un admin por club; suma de zonas activas correcta |
| CTZ-64 | Notificaciones push en la app: permisos, recepción y navegación a la zona (`RF-10`) | Jorge | P1 | CTZ-62, CTZ-55 | Tocar "te robaron la zona" abre P-13 |
| CTZ-78 | Endpoint `PUT /me/preferences`, pantalla P-30 (preferencias de carrera, voz y alertas, `RF-11`) y avisos de voz en español por kilómetro (`RF-15`) | Jorge + Rowlis | P1 | CTZ-33, CTZ-16 | Permite activar/desactivar auto-pausa, voz y alertas, y anuncia tiempo/ritmo en cada km con pantalla bloqueada |
| CTZ-71 | Pantallas de clubes P-20, P-21 (con mapa del territorio grupal del club) y P-22 (`RF-29`) | Jorge | P1 | CTZ-69 | Crear club, unirse por código y visualizar en el mapa las zonas dominadas por el club |
| CTZ-65 | Pantallas P-14 Muro de actividad (`RF-09`) y P-27 bandeja de notificaciones (`RF-10`) | Rowlis | P1 | CTZ-61, CTZ-62 | Muro con rutas recortadas y bandeja de avisos funcionando contra QA |
| CTZ-68 | Campo: avisos de voz por km y recepción de push real en los teléfonos de prueba | Jaime | P1 | CTZ-63, CTZ-64, CTZ-78 | Evidencia en `docs/qa/` |
| CTZ-20b | Validación de compilación EAS y GPS en segundo plano en iOS (condicionada a cuenta Apple Developer en CTZ-58, `D-01`) | Jorge + Jaime | P1 | CTZ-58, CTZ-63 | Si hay cuenta disponible, prueba de 30 min con pantalla bloqueada en iPhone documentada en `docs/qa/`; si no hay cuenta, la historia se cierra como 'No aplica' sin afectar el hito P0 ni la entrega |

## S6 — 19 al 23 de diciembre · Barrios, logros, retos y rankings (weekly el miércoles 23)

**Objetivo:** coronar al rey o reina de los 6 barrios, activar logros/rachas/retos (conectándolos al Muro y notificaciones) y publicar los rankings individuales y por barrio.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-94 | Barrios en el backend (`RF-23`): semilla de los 6 barrios preasignados a hexágonos H3, cálculo de rey/reina del barrio, endpoints y publicación de coronas en el Muro (CTZ-61) | Ashley | P1 | CTZ-50, CTZ-93, CTZ-61 | MT-19 y MT-20 en verde; cambio de rey/reina aparece en el Muro; zonas fuera de los 6 barrios siguen funcionando |
| CTZ-70 | Logros (evaluación al cerrar carrera), racha semanal (`RF-25`), retos (`RF-26`), trabajos nocturnos e integración de medallas en Muro (CTZ-61) y Notificaciones (CTZ-62) | Ashley | P1 | CTZ-51, CTZ-37, CTZ-61, CTZ-62 | Los logros y retos de CTZ-37 cargados, probados y emitiendo evento en Muro y aviso al desbloquearse |
| CTZ-42 | Backend de rankings individuales y por barrio (`RF-27`): zonas dominadas, zonas conquistadas y distancia | Rowlis (revisa Ashley) | P1 | CTZ-40, CTZ-94 | Devuelve el top y la posición propia del corredor en cada categoría y barrio |
| CTZ-95 | Capa de barrios en P-07 y pantalla P-34 (detalle de barrio con su rey o reina y tabla de posiciones, `RF-23`) | Jorge (mapa) + Rowlis (pantalla) | P1 | CTZ-94 | Nombres y límites de los 6 barrios visibles en el mapa; corona visible |
| CTZ-45 | Pantalla P-23 rankings individuales y por barrio (`RF-27`) | Jorge | P1 | CTZ-42 | Cambio de categoría y de barrio sin recargar la pantalla |
| CTZ-72 | Pantallas P-24 logros y racha (`RF-25`) y P-25/P-26 retos deportivos (`RF-26`) | Rowlis | P1 | CTZ-70 | Inscripción a reto y progreso automático tras cada carrera |

## S7 — 26 al 30 de diciembre · Validación integral de P1 y preparación de pilotos (weekly el miércoles 30)

**Objetivo:** validar todos los módulos P1 integrados sin agregar alcance nuevo antes del congelamiento.

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-73 | Crear el club oficial del grupo de corredores y el primer reto deportivo de prueba | Diana | P1 | CTZ-71, CTZ-72 | Club y reto activos en el entorno para la demostración |
| CTZ-80 | Manual de usuario y guía rápida para corredores de demostración | Diana | P1 | — | Documento en `docs/producto/` listo para el 30/12 |
| CTZ-74 | Pruebas integrales de API y campo de Muro, Clubes, Barrios, Logros, Retos y Rankings | Rangelis + Jaime | P1 | CTZ-65, CTZ-71, CTZ-95, CTZ-72, CTZ-45 | Informe de pruebas del sprint con defectos reportados |

## S8 — 31 de diciembre al 6 de enero · Cierre y congelamiento

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-83 | Cerrar los defectos abiertos de severidad alta y media | Ashley, Jorge, Rowlis | P0 | — | Cero defectos altos abiertos |
| CTZ-84 | Versión candidata (`1.0.0-rc`) desplegada en producción e instalador móvil listo para demostración (`D-01`) | Ashley + Jorge | P0 | CTZ-63, CTZ-83 | Instalador Android verificado (e iOS según disponibilidad de cuenta) |
| CTZ-86 | **Congelamiento de código (6 de enero)** | Ashley | P0 | — | A partir de aquí solo entran correcciones de defectos |

## QA 1 y QA 2 — 7 al 19 de enero

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-87 | Ciclo completo de pruebas (funcionales, API, campo, regresión E2E) | Rangelis + Jaime | P0 | CTZ-86 | Todos los casos ejecutados al 13/1 |
| CTZ-88 | Corrección de defectos finales | Ashley, Jorge, Rowlis | P0 | CTZ-87 | Cero defectos altos al 18/1 |
| CTZ-89 | Informe final de pruebas | Rangelis | P0 | CTZ-87 | `docs/qa/05-INFORME-FINAL.md` |
| CTZ-90 | Guion de la demo, presentación y ensayo general | Diana + Ashley | P0 | CTZ-88 | Ensayo completo el 18/1 |

## P2 — Después de la entrega (Evolución Post-MVP y Fase Comercial · `01-HLD-v1.2-post-MVP.md` y `02-LLD.md`)

| ID | Historia | Responsable | P | Depende de | Criterios de aceptación |
|---|---|---|---|---|---|
| CTZ-66 | Ficha comercial de tiendas (textos, capturas) y política de privacidad en URL pública | Diana | P2 | CTZ-27, CTZ-92 | Material gráfico y textos listos para publicación abierta en Google Play y App Store |
| CTZ-67 | Pruebas de matriz de visibilidad de seguidores, perfiles privados, bloqueos y reportes (LLD §7.2) | Rangelis | P2 | CTZ-79 | Verificación de privacidad entre seguidores y cuentas bloqueadas |
| CTZ-75 | Récords personales por distancia, control de desgaste de zapatillas, zonas de privacidad configurables y exportación GPX | Ashley | P2 | CTZ-32 | Endpoints y cálculo de récords (1K, 5K, 10K, 21K, 42K), catálogo de calzado y archivo `.gpx` |
| CTZ-76 | Inicio de sesión con Apple (`Apple Sign-In`), exportación completa de datos y borrado de cuenta con recálculo territorial | Ashley | P2 | CTZ-14 | Cumplimiento de requisito de App Store y portabilidad de datos |
| CTZ-77 | Cola de moderación de rutas en cuarentena (`in_review`), reportes de usuarios, suspensiones y `audit_log` | Ashley | P2 | CTZ-31a | Endpoints de revisión manual y auditoría administrativa |
| CTZ-79 | Pantallas P-16 perfil de otros, P-18 seguidores, P-19 buscador, kudos y comentarios en P-11, P-28 zapatillas, P-29 récords, P-31 zonas de privacidad/GPX, P-32 cuenta Apple y P-33 reportar | Rowlis + Jorge | P2 | CTZ-75, CTZ-76, CTZ-77 | Interfaz móvil completa para las funciones extendidas del LLD v1.2 |
| CTZ-81 | Pruebas de seguridad sobre moderación, límites de tasa y permisos entre perfiles privados | Rangelis | P2 | CTZ-77 | Informe de pruebas de seguridad sobre endpoints extendidos |
| CTZ-85 | Prueba cerrada de 14 días en Google Play Console con al menos 12 testers | Diana + Jaime | P2 | CTZ-66, CTZ-84 | Requisito de Google Play para cuentas personales antes de pasar a producción abierta |
| CTZ-91 | Publicación abierta en producción en Google Play y App Store | Ashley + Diana | P2 | CTZ-85 | Revisión oficial de tiendas y apertura al público general |
| CTZ-92 | Registro y verificación definitiva de marca comercial (ONAPI, tiendas, dominio y redes) | Diana | P2 | CTZ-25 | Búsqueda formal en ONAPI/USPTO y aseguramiento de dominio y `@handle` oficial |
| CTZ-96 | Integración con relojes inteligentes y salud: *Apple Watch (HealthKit)*, *Wear OS (Health Connect)* y *Garmin* | Jorge + Ashley | P2 | CTZ-30 | Trackeo desde el reloj inteligente y lectura de frecuencia cardíaca |
| CTZ-97 | Importación desde Strava, Garmin y archivos GPX/FIT: conector OAuth y subida manual | Ashley + Rowlis | P2 | CTZ-31b | Conversión de actividades importadas al motor H3 con validación antitrampas |
| CTZ-98 | Fotos geolocalizadas en actividades y muro: adjuntar fotos por carrera en Cloudflare R2 | Rowlis + Ashley | P2 | CTZ-61 | Galería en el detalle de la carrera (P-11) y en el Muro (P-14) |
| CTZ-99 | Temporadas competitivas: reinicio periódico del mapa de dominio con medallas históricas | Ashley + Diana | P2 | CTZ-50, CTZ-70 | Cierre trimestral de temporada, salón de la fama y trofeos permanentes |
| CTZ-100 | Retos colectivos entre clubes: metas grupales y batallas territoriales por barrio | Ashley + Rowlis | P2 | CTZ-69, CTZ-70 | Ranking de clubes por temporada y retos cooperativos |
| CTZ-101 | Planes de entrenamiento guiados: rutinas por objetivo (5K, 10K, 21K) integradas con conquista | Diana + Jorge | P2 | CTZ-33 | Calendario semanal de sesiones con indicaciones por voz |
| CTZ-102 | Suscripción Terrun Pro: mapas de calor personales, analítica avanzada y personalización | Ashley + Diana | P2 | CTZ-58 | Suscripción sin volver *pay-to-win* la disputa de territorio |
| CTZ-103 | Panel web de administración: dashboard visual para moderación de rutas, barrios y retos | Rowlis + Ashley | P2 | CTZ-77 | Interfaz web conectada a los módulos de administración (M12 y M13) |
