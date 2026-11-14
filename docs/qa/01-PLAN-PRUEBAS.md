# 01 — Plan de pruebas

**Responsable:** Rangelis (CTZ-11) · **Estado:** por redactar en S0 · **Base:** HLD Anexo B y LLD §9

Completar cada sección:

1. **Objetivo y alcance.** Qué se prueba en cada módulo (M1–M15) y qué queda fuera.
2. **Niveles de prueba.** Unitarias, integración, contrato de API, componentes de la app, campo, regresión, aceptación. Indicar herramienta y responsable (HLD Anexo B.1).
3. **Ambientes.** Local, QA (Render + Neon `qa`), producción. Datos de prueba y usuarios de prueba.
4. **Criterios de entrada por sprint.** Por ejemplo: historia en `👀 en revisión`, desplegada en QA, con criterios de aceptación escritos.
5. **Criterios de salida por sprint.** Por ejemplo: casos ejecutados, cero defectos de severidad alta abiertos en las historias del sprint.
6. **Severidades de defectos.** Alta (bloquea un flujo P0 o pierde datos), media (funciona con un rodeo), baja (cosmético).
7. **Casos obligatorios del motor.** MT-01 a MT-18 (LLD §9): quién los verifica a mano.
8. **Pruebas no funcionales.** RNF-01 precisión, RNF-02 segundo plano, RNF-03 batería, RNF-04 sin conexión, RNF-05 rendimiento.
9. **Calendario.** Qué se prueba en cada sprint (según `docs/03-BACKLOG.md`) y el ciclo completo de QA del 7 al 19 de enero.
10. **Entregables.** Informe por sprint y `05-INFORME-FINAL.md`.
