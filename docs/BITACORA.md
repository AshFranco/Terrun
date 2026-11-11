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

## 2026-11-11 — Ashley — CTZ-05 Página de la encuesta a corredores
- Hecho: página `survey/index.html` con las 12 preguntas, barra de avance, escalas en hexágonos, orden por toques en la pregunta 8 y una demo animada en un teléfono: la carrera 1 por la Churchill toca 10 zonas sin conquistar ninguna y la carrera 2 conquista la zona de la Churchill con Gustavo Mejía Ricart. Respuestas en Netlify Forms; `netlify.toml` y `scripts/build_survey_map.py`.
- Evidencia: vista previa https://claude.ai/artifact/LQkKUP9vBAPeFSR3CWamos; capturas en Chrome sin desborde horizontal; sintaxis del script verificada.
- Decisiones: D-14.
- Siguiente paso: Ashley conecta el repo en Netlify y activa la detección de formularios; Diana comparte el link por WhatsApp (meta: 15 respuestas antes del 18/11).

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
