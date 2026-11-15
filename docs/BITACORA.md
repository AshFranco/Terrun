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

## 2026-11-15 — Ashley — Revisión de completitud de CTZ-03 y CTZ-04
- Hecho: revisión de lo ya mergeado contra el HLD y el LLD completos. En CTZ-03 faltaban las convenciones transversales de la API; quedaron en `src/Terrun.Api/Web/` (prefijo `/api/v1`, enums `snake_case`, errores con `code` y `traceId`, FluentValidation, límite de tasa, 2 MB, cabeceras de seguridad, HSTS y logs JSON). Guía del primer día: Rowlis también necesita .NET y Docker (D-16) y `dotnet tool restore`. Estándares §2.1: las constantes privadas usan `_camelCase`, como exige `.editorconfig`.
- Evidencia: 34 pruebas en verde (13 nuevas de convenciones, una sobre Kestrel real para el tope de 2 MB); mutaciones verificadas (sin prefijo ni convertidor de enums fallan 7 pruebas); API en `Staging` con logs JSON y `traceId` igual en la respuesta y en el log.
- Decisiones: D-24; LLD v1.8.
- Riesgo abierto (CTZ-04): con los planes gratis, Render se duerme a los 15 min sin tráfico y Neon da 100 horas de cómputo al mes; los trabajos en segundo plano del LLD §6 no son viables tal como están diseñados. Pendiente de decisión.
- Siguiente paso: decidir la estrategia de los trabajos en segundo plano y de disponibilidad (RNF-08) antes del S2.

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
