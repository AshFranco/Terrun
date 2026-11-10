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
