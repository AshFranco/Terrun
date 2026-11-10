# Guía de trabajo del equipo — Terrun

Este archivo define **cómo trabaja el equipo** y es la primera lectura obligatoria para cualquier persona o asistente de código que trabaje en el repositorio. Si algo de aquí contradice otro documento, manda este archivo; si contradice el HLD/LLD en temas técnicos, mandan el HLD/LLD y se corrige esta guía.

## 1. El proyecto en 5 líneas

- App móvil para corredores: el mapa está dividido en hexágonos H3; al correr se **conquistan** zonas (5 de 7 sub-celdas, acumulado) y se **dominan** (más sub-celdas en los últimos 30 días).
- Stack: app **React Native + Expo (TypeScript)** en `mobile/`; API **.NET 10 + EF Core** en `backend/`; **PostgreSQL + PostGIS**.
- Asignatura: Construcción de Software. Sprints semanales de **viernes a viernes**, con weekly cada viernes.
- **Congelamiento de código: miércoles 6 de enero de 2027.** Entrega final: **miércoles 20 de enero de 2027**.
- Diseño completo en `docs/01-HLD.md` y `docs/02-LLD.md` (39 tablas, 112 operaciones de API, 33 pantallas).

## 2. Mapa de documentos

| Documento | Para qué sirve | Quién lo actualiza |
|---|---|---|
| `docs/00-GUIA-DE-TRABAJO.md` | Reglas y protocolos del equipo (este archivo) | Ashley (SM) |
| `docs/01-HLD.md` | Diseño de alto nivel: alcance, arquitectura, plan | Ashley / Diana |
| `docs/02-LLD.md` | Diseño de bajo nivel: tablas, API, algoritmos, pantallas | Ashley (contrato de API) |
| `docs/03-BACKLOG.md` | Todas las historias del proyecto, por sprint y persona | Diana (PO) |
| `docs/04-ESTANDARES-DESARROLLO.md` | Reglas de código: idioma, nombres por capa, glosario, pruebas, formato | Ashley |
| `docs/ESTADO.md` | **Lo que toca ahora**: sprint en curso, tareas por persona, bloqueos | Cada persona marca lo suyo; Ashley lo rehace en cada planificación |
| `docs/BITACORA.md` | Histórico: qué se hizo, quién, cuándo y con qué evidencia | Cada persona al cerrar una sesión de trabajo |
| `docs/DECISIONES.md` | Decisiones tomadas (técnicas o de producto) con fecha y motivo | Quien toma la decisión |
| `docs/sprints/` | Un archivo por sprint: compromiso, resultado y resumen de la weekly | Ashley |
| `docs/qa/` | Plan de pruebas, casos, registro de defectos, pruebas de campo | Rangelis y Jaime |
| `CONTRIBUTING.md` | Ramas, commits, PR, Definition of Ready y Definition of Done | Ashley |

## 3. Equipo y fronteras de trabajo

| Persona | Rol | Carpeta que toca | No toca |
|---|---|---|---|
| **Ashley** | Scrum Master + Backend | `backend/`, `.github/`, `docs/02-LLD.md` (contrato) | `mobile/` salvo coordinación explícita |
| **Diana** | Product Owner | `docs/03-BACKLOG.md`, `docs/DECISIONES.md`, `docs/producto/` | Código |
| **Jorge** | Front — núcleo técnico | `mobile/src/recording`, `mobile/src/storage`, `mobile/src/map`, pantallas P-06 a P-09, P-11 (mapa/splits), P-13, P-31 | `backend/`; pantallas de Rowlis |
| **Rowlis** | Front — experiencia | `mobile/app/(auth)`, `mobile/src/components`, `mobile/src/theme`, `mobile/src/i18n`, `mobile/src/api`, `mobile/src/state`, resto de pantallas | `backend/`; módulos de Jorge |
| **Rangelis** | QA — API y casos | `docs/qa/`, colección de pruebas de API | Código de producción |
| **Jaime** | QA — campo y regresión | `docs/qa/`, pruebas E2E (Maestro) | Código de producción |

Archivos compartidos (`README.md`, `.gitignore`, `mobile/package.json`, `mobile/app/_layout.tsx`): se cambian en un PR propio y se avisa en el grupo.

## 4. Protocolo: "¿qué nos toca trabajar?" / "¿qué me toca?"

Cuando alguien pregunte qué toca hacer, el asistente **debe**:

1. **Identificar a la persona.** Leer `CLAUDE.local.md` (dice quién usa esta máquina). Si no lo dice, preguntar el nombre antes de responder.
2. **Leer `docs/ESTADO.md`** completo y las últimas 5 entradas de `docs/BITACORA.md`.
3. **Responder con este formato**, sin inventar tareas que no estén en `ESTADO.md` o `03-BACKLOG.md`:
   - Sprint en curso, fechas y **días que faltan para la weekly**.
   - **Tus tareas pendientes**, en el orden en que conviene hacerlas (primero lo que bloquea a otros), cada una con su ID, sus criterios de aceptación y su dependencia.
   - **Lo que otros esperan de ti**: tareas de otras personas que dependen de las tuyas.
   - **Lo que tú esperas de otros**: tareas tuyas bloqueadas, y por quién.
   - Bloqueos abiertos que te afectan.
   - Una propuesta concreta: "Empezaría por CTZ-XX porque…".
4. **Si quien pregunta es Ashley (Scrum Master)**, añadir la vista de equipo: avance por persona (hechas/total), riesgos para la weekly y tareas sin dueño o atrasadas.
5. **Si quien pregunta es Diana (Product Owner)**, añadir: historias listas para aceptar, decisiones de producto pendientes y el estado de la validación con corredores.
6. Si `ESTADO.md` está desactualizado (el sprint ya terminó por fecha), decirlo y proponer a Ashley ejecutar el protocolo de weekly (§7).

## 5. Protocolo: empezar una tarea

1. Confirmar que la tarea está en `ESTADO.md` y asignada a quien la va a hacer. Si no está, **no se empieza**: se avisa a Diana o a Ashley.
2. Actualizar `main` y crear la rama: `git switch main && git pull && git switch -c feat/CTZ-XX-short-description`.
3. Leer las secciones del LLD que correspondan (las cita cada historia del backlog) y `docs/04-ESTANDARES-DESARROLLO.md`.
4. Marcar la tarea en `ESTADO.md` como `🔄 en curso`.
5. Trabajar **solo dentro de tu carpeta** (§3). Si hace falta un endpoint que no existe o un cambio de contrato: primero se actualiza `docs/02-LLD.md` §3 en un PR propio, se avisa a Ashley y, mientras tanto, el front trabaja con un mock.
6. No adelantarse: si algo pertenece a un sprint futuro, no se construye ahora.

## 6. Protocolo: cerrar una sesión de trabajo

Al terminar cada sesión (aunque la tarea no esté completa), el asistente **debe**:

1. Actualizar el estado de la tarea en `docs/ESTADO.md`: `⬜ pendiente`, `🔄 en curso`, `👀 en revisión (PR #n)`, `✅ hecha`, `⛔ bloqueada (motivo)`.
2. Agregar una entrada **al principio** de `docs/BITACORA.md` con el formato de ese archivo: fecha, persona, tarea, qué se hizo, evidencia (PR, commit, captura, resultado de pruebas) y el siguiente paso.
3. Si se tomó una decisión, registrarla en `docs/DECISIONES.md`.
4. Si cambió el contrato de API o el modelo de datos, verificar que `docs/02-LLD.md` quedó actualizado.
5. Hacer commit de esos documentos en la misma rama de la tarea.

Una tarea solo pasa a `✅ hecha` cuando cumple la Definition of Done de `CONTRIBUTING.md` (PR mergeado y criterios verificados por alguien distinto del autor).

## 7. Protocolo: weekly y planificación (lo ejecuta Ashley)

Antes de la weekly del viernes, cuando Ashley pida "prepara la weekly":

1. Leer `ESTADO.md` y las entradas de `BITACORA.md` del sprint.
2. Crear o completar `docs/sprints/SN.md` (plantilla en `docs/sprints/README.md`) con: compromiso, hecho, no hecho y por qué, impedimentos, métricas (historias hechas/comprometidas, defectos abiertos) y demo.
3. Redactar el **resumen para el profesor** (máximo 10 líneas, sin detalles internos de infraestructura): avances, impedimentos y plan del próximo sprint.

Después de la weekly, cuando Ashley pida "planifica el siguiente sprint":

1. Mover lo no terminado al siguiente sprint (o a P2, si lo decide Diana).
2. Reescribir `ESTADO.md` con las historias del nuevo sprint, tomadas de `03-BACKLOG.md`.
3. Registrar en `BITACORA.md` el cierre del sprint y la apertura del siguiente.

## 8. Reglas no negociables

1. **Autoría humana.** Los commits y PR los firman las personas. Prohibido `Co-authored-by` de herramientas de IA y firmas tipo "Generated with…". Lo bloquean el hook `commit-msg` y la CI.
2. **Nadie hace push directo a `main`.** Todo entra por PR con 1 aprobación y la CI en verde.
3. **El contrato de API vive en `docs/02-LLD.md` §3** y los tipos de la app se generan desde el OpenAPI. No se escriben a mano.
4. **Ningún parámetro del juego en el código** (umbral 5, 30 días, 25 km/h, XP…): todo sale de `system_setting`.
5. **El servidor siempre recalcula.** La app muestra; el backend decide la distancia oficial, la conquista y el dominio.
6. **Toda actividad lleva un `clientId` (UUID) generado en el teléfono.** Reenviar no duplica.
7. **Estándares de desarrollo** (`docs/04-ESTANDARES-DESARROLLO.md`): **todo el código en inglés** (identificadores, BD, API, comentarios, logs y commits) y documentación en español; nombres del glosario. Antes de escribir código, el asistente lee ese archivo.
8. **Probar antes de hacer commit.** No se hace commit de código que no compila o cuyas pruebas fallan.
9. **Sin secretos en el repo.** Las variables van en `.env` (ignorado) y en Render/EAS.
10. **No decir "listo" sin evidencia:** pruebas ejecutadas, captura o PR.

## 9. Plantilla de pedido para el asistente

```text
Soy [NOMBRE], [ROL]. Vamos a trabajar en [CTZ-XX: título] (ver docs/ESTADO.md).
Antes de escribir código, lee docs/00-GUIA-DE-TRABAJO.md, docs/04-ESTANDARES-DESARROLLO.md,
la historia en docs/03-BACKLOG.md y estas secciones del LLD: [§x.y].
Reglas: rama feat/CTZ-XX-...; solo toca [mi carpeta]; si necesitas cambiar el contrato
de API, detente y avísame; no implementes nada de sprints futuros.
Al terminar: corre las pruebas, haz commit en mi rama (sin push, sin merge),
actualiza docs/ESTADO.md y docs/BITACORA.md, y dime cómo probarlo.
```
