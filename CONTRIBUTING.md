# Guía de contribución

## Antes del primer commit

```bash
./scripts/configurar-entorno.sh
```

El script activa los hooks del repositorio (`core.hooksPath .githooks`), verifica que tu `user.name` y tu `user.email` estén configurados y crea tu `CLAUDE.local.md` (personal, ignorado por git) para que tu asistente de código cargue la guía del equipo y sepa quién eres.

## Autoría

Los commits los firman las personas. Ninguna herramienta de IA aparece como autora ni coautora: nada de trailers `Co-authored-by:` de asistentes ni firmas "Generated with…". Lo verifican el hook `commit-msg` y la CI.

Usar asistentes para escribir código está bien; atribuirles autoría en el historial, no. El historial dice quién es responsable del cambio.

## Ramas

`main` está protegida: solo se entra por PR con 1 aprobación y CI en verde.

```text
feat/CTZ-XX-short-description     funcionalidad nueva
fix/CTZ-XX-short-description      corrección de defecto
docs/CTZ-XX-short-description     solo documentación
chore/CTZ-XX-short-description    configuración, dependencias, CI
```

Siempre desde `main` actualizado: `git switch main && git pull && git switch -c feat/CTZ-XX-...`. El nombre de la rama va en inglés.

## Commits

Formato Conventional Commits, **en inglés** (modo imperativo, minúsculas, sin punto final):

```text
feat(territory): conquer zone when 5 of 7 cells are visited
fix(gps): keep the activity when the OS kills the app
docs(lld): add challenge standings endpoint
```

Ámbitos: `api`, `db`, `engine`, `territory`, `auth`, `social`, `app`, `gps`, `map`, `ui`, `qa`, `docs`, `ci`.

## Pull requests

- Uno por historia (`CTZ-XX`). Si pasa de ~400 líneas, se parte: un PR de 2.000 líneas no se revisa, se aprueba a ciegas.
- Título en inglés con el formato de los commits; descripción en español usando la plantilla: qué resuelve, cómo probarlo, checklist de la Definition of Done.
- **Revisores:** el backend de Ashley lo revisa Jorge o Rowlis (como mínimo, lo que toca el contrato); el backend de Rowlis lo revisa Ashley; el front lo revisa el otro dev front; todo cambio del contrato de API lo aprueba Ashley.
- El autor hace el merge (squash) después de la aprobación.

## Definition of Ready

Una historia entra al sprint solo si:

- [ ] Tiene criterios de aceptación escritos y verificables.
- [ ] Sus dependencias están terminadas o tienen mock acordado.
- [ ] Indica las secciones del LLD que aplican.
- [ ] Tiene wireframe o referencia visual si toca UI.
- [ ] Tiene responsable y cabe en el sprint.

## Definition of Done

Una historia se cierra solo si:

- [ ] Está en `main` mediante un PR aprobado.
- [ ] Cumple `docs/04-ESTANDARES-DESARROLLO.md` (idioma, nombres, glosario, formato).
- [ ] Los criterios de aceptación los verificó alguien distinto del autor (QA o PO).
- [ ] Tiene pruebas automatizadas (unitarias o de integración) y pasan.
- [ ] Si toca el motor: hay casos calculados a mano (LLD §9).
- [ ] Ningún parámetro del juego está fijo en el código.
- [ ] Endpoints documentados en Swagger y tipos de la app regenerados.
- [ ] Probado en Android **y** en iOS si toca la app.
- [ ] Sin `console.log`, `TODO`, credenciales ni coordenadas reales de usuarios en el código.
- [ ] `docs/ESTADO.md` y `docs/BITACORA.md` actualizados.
- [ ] CI en verde.

## Estándares no negociables

Ver `docs/00-GUIA-DE-TRABAJO.md` §8 y `docs/04-ESTANDARES-DESARROLLO.md`.
