# Estado actual del proyecto

> **Sprint en curso: S0 — Diseño y arranque** · del martes 10 al viernes 13 de noviembre de 2026
> **Weekly:** viernes 13 de noviembre · **Próximo hito:** entrega del HLD y el LLD, miércoles 11 de noviembre
> **Última actualización:** 10/11/2026 — Ashley

Leyenda: ⬜ pendiente · 🔄 en curso · 👀 en revisión (PR #n) · ✅ hecha · ⛔ bloqueada (motivo)

Detalle y criterios de aceptación de cada historia: `docs/03-BACKLOG.md`.

## Tareas por persona

### Ashley — Scrum Master + Backend
- 🔄 **CTZ-01** Entregar HLD y LLD (mié 11/11). Documentos generados en `docs/entregables/`; falta entregarlos.
- ⬜ **CTZ-02** Crear el repo en GitHub, invitar al equipo y proteger `main`. *Bloquea a CTZ-03, CTZ-06 y CTZ-08.*
- ⬜ **CTZ-03** Esqueleto del backend + Docker Compose con PostGIS + CI. *Depende de CTZ-02.*
- ⬜ **CTZ-04** Cuentas en Neon, Render y Expo.

### Diana — Product Owner
- 🔄 **CTZ-05** Enviar la encuesta a los corredores (`docs/producto/encuesta-corredores.md`). *Publicada en https://terrun-encuesta.netlify.app; falta que Diana comparta el link (meta: 15 respuestas antes del 18/11).*
- ⬜ **CTZ-07** Confirmar con Jorge y Rowlis que dominan React (supuesto S-01). *Urgente: condiciona el stack de la app.*
- ⬜ **CTZ-06** Cargar el backlog en GitHub Projects. *Depende de CTZ-02.*

### Jorge — Front, núcleo técnico
- ⬜ **CTZ-08** Esqueleto de la app Expo + TypeScript + Expo Router + lint + Jest. *Depende de CTZ-02.*
- ⬜ **CTZ-09** Investigar expo-location + TaskManager y MapLibre RN en un development build. *Depende de CTZ-08.*

### Rowlis — Front, experiencia
- ⬜ **CTZ-10** Sistema de diseño base + wireframes de P-01 a P-09.

### Rangelis — QA
- ⬜ **CTZ-11** Plan de pruebas v1 en `docs/qa/01-PLAN-PRUEBAS.md`.

### Jaime — QA
- ⬜ **CTZ-12** Plan de pruebas de campo: dispositivos y rutas medidas en `docs/qa/03-PRUEBAS-CAMPO.md`.

## Bloqueos abiertos

| Desde | Bloqueo | Afecta a | Responsable de resolverlo |
|---|---|---|---|
| 10/11 | El repo aún no existe en GitHub | CTZ-03, CTZ-06, CTZ-08 | Ashley (CTZ-02) |
| 10/11 | Falta confirmar que Jorge y Rowlis dominan React | Toda la app | Diana (CTZ-07) |

## Avance del sprint

| Persona | Hechas | Total |
|---|---|---|
| Ashley | 0 | 4 |
| Diana | 0 | 3 |
| Jorge | 0 | 2 |
| Rowlis | 0 | 1 |
| Rangelis | 0 | 1 |
| Jaime | 0 | 1 |
| **Equipo** | **0** | **12** |

## Próximo sprint (S1 — 14 al 20 de noviembre)

Cuenta, perfil y prueba de GPS en segundo plano: CTZ-13 a CTZ-29. Hito del 20/11: GPS funcionando con la pantalla bloqueada en Android e iOS.
