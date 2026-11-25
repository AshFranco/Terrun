<!-- Título del PR en inglés (formato Conventional Commits); descripción en español -->
## Qué resuelve

<!-- Historia del backlog: CTZ-XX. En "Closes #" va el número del ISSUE de esa historia (búscalo en el tablero), no el número CTZ -->
Closes #

## Cómo probarlo

1.
2.

## Revisión de completitud

<!-- Cumplir los criterios de aceptación no basta. Revisa la historia contra el LLD completo (§1–§10: datos, API, algoritmos, seguridad §7, configuración §8) y contra las historias que dependen de esta en docs/03-BACKLOG.md -->
- Secciones del LLD e historias dependientes revisadas:
- Lo que se agregó por la revisión:
- Lo que se dejó fuera y por qué:

## Definition of Done

- [ ] Cumple los estándares de desarrollo (`docs/04-ESTANDARES-DESARROLLO.md`)
- [ ] Criterios de aceptación verificados por alguien distinto del autor
- [ ] Revisión de completitud hecha (sección de arriba llena)
- [ ] Pruebas automatizadas agregadas y en verde
- [ ] Si toca el motor: casos calculados a mano (LLD §9)
- [ ] Ningún parámetro del juego fijo en el código
- [ ] Endpoints documentados en Swagger, con sus errores propios declarados con `[ApiError]` (D-33), y tipos de la app regenerados
- [ ] Probado en Android e iOS (si toca la app)
- [ ] Sin `console.log`, `TODO` ni credenciales
- [ ] `docs/ESTADO.md` y `docs/BITACORA.md` actualizados
- [ ] Si cambia el contrato de API: `docs/02-LLD.md` actualizado y aprobado por Ashley
