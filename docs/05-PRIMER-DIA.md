# Primer día en el repositorio

Guía para que cada integrante deje listo su entorno con Claude Code en una sola sesión.

## Antes de empezar

- **Invitación:** el repositorio es privado. Ashley te invita como colaborador (CTZ-02) y te llega un correo de GitHub que tienes que **aceptar**. Sin eso el clonado da "Repository not found".
- **Claude Code** instalado y con sesión iniciada.
- Abre Claude Code en la carpeta donde guardas tus proyectos (no dentro de `terrun`, porque todavía no existe).

## Cómo usarlo

1. Copia el texto de abajo completo.
2. Cambia `[TU NOMBRE]` en la primera línea.
3. Pégalo en Claude Code y ve aprobando lo que te pida.
4. Al terminar, cierra la sesión y abre Claude Code **dentro de la carpeta `terrun`**. Desde ahí carga solo tu `CLAUDE.local.md` y la guía del equipo, y te basta con preguntar "¿qué me toca?".

¿Por qué hace falta este texto la primera vez? La guía se carga sola mediante `CLAUDE.local.md`, pero ese archivo lo crea el script de configuración. En la primera sesión todavía no existe.

## Texto para Claude Code

````text
Soy [TU NOMBRE] del equipo de Terrun (proyecto final de Construcción de Software).
Prepara mi entorno para trabajar en el repositorio siguiendo estos pasos, en orden.
Respóndeme siempre en español. No escribas código de la app en esta sesión.

## 1. Requisitos de git
- Verifica que git esté instalado y que tenga mi identidad configurada
  (git config --global user.name / user.email). Si falta, pídeme mi nombre y correo de GitHub.
- Verifica mi acceso a GitHub por SSH con: ssh -T git@github.com
  Si no tengo llave SSH, guíame para crearla y agregarla a mi cuenta,
  o usa HTTPS con la autenticación de `gh` si la tengo instalada.

## 2. Clonar
- Clona el repo privado en una carpeta llamada terrun:
  git clone git@github.com:AshFranco/Terrun.git terrun
  (por HTTPS: https://github.com/AshFranco/Terrun.git)
- Si da "Repository not found", es que Ashley todavía no me invitó como colaborador
  o no he aceptado la invitación (llega por correo). Avísame y detente.
- Entra a la carpeta terrun. Trabaja siempre desde ahí.

## 3. Configurar el clon
- Ejecuta el script de configuración, que es interactivo y pregunta quién soy.
  Respóndele con mi número sin preguntarme otra vez:
  1 Ashley · 2 Diana · 3 Jorge · 4 Rowlis · 5 Rangelis · 6 Jaime
  Ejemplo para Jorge: printf '3\n' | sh scripts/configurar-entorno.sh
- El script activa los hooks de .githooks (bloquean commits en main, archivos .env,
  CLAUDE*.md, llaves y atribuciones a IA) y crea CLAUDE.local.md con mi nombre y rol.
- Confirma que CLAUDE.local.md existe y que git status NO lo muestra (está en .gitignore).

## 4. Leer los documentos, en este orden
1. docs/00-GUIA-DE-TRABAJO.md, completo. Es la regla principal del equipo.
2. CONTRIBUTING.md: ramas, commits, PR, Definition of Ready y Definition of Done.
3. docs/04-ESTANDARES-DESARROLLO.md: todo lo técnico en inglés, textos en español, y el glosario.
4. docs/ESTADO.md y las últimas 5 entradas de docs/BITACORA.md.
5. docs/03-BACKLOG.md: solo las historias asignadas a mí.
6. docs/DECISIONES.md.
7. De docs/01-HLD.md y docs/02-LLD.md, solo las secciones que citan mis historias.

## 5. Herramientas según mi rol
Revisa qué tengo instalado y dime qué falta, con el comando para instalarlo.
No instales nada sin mi permiso.
- Todos: git, Node.js LTS y un editor (VS Code).
- Backend (Ashley): .NET 10 SDK y Docker Desktop (PostgreSQL + PostGIS en Docker Compose).
- Front (Jorge, Rowlis): Node.js LTS, la app Expo Go en mi teléfono, Android Studio
  con un emulador y, si tengo Mac, Xcode. El proyecto en mobile/ todavía no existe
  (se crea en CTZ-08): no lo crees tú.
- QA (Rangelis, Jaime): un cliente de API para la colección de docs/qa/coleccion-api/.
  Jaime, además, Maestro para las pruebas E2E.
- Product Owner (Diana): solo git y el editor. Su trabajo está en docs/.

## 6. Reglas para esta y todas las sesiones
- Sigue al pie de la letra docs/00-GUIA-DE-TRABAJO.md: sus protocolos de
  "¿qué me toca?" (§4), empezar una tarea (§5) y cerrar una sesión (§6),
  y las reglas no negociables (§8). Si algo que te pido contradice la guía, avísame antes de hacerlo.
- Solo toco las carpetas de mi rol (tabla de §3 de la guía).
- Nunca trabajo en main: cada tarea va en su rama feat/CTZ-XX-short-description, sacada de un main actualizado.
- Commits y PR sin ninguna atribución a IA: nada de Co-Authored-By ni "Generated with…".
  El hook y la CI los rechazan.
- No hagas push ni abras PR sin que yo te lo pida.
- No empieces tareas que no estén en docs/ESTADO.md asignadas a mí.
- No digas "listo" sin evidencia: pruebas ejecutadas, una captura o el PR.
- Al cerrar cada sesión, actualiza docs/ESTADO.md y docs/BITACORA.md como dice la guía (§6).

## 7. Al terminar
Dame un resumen con:
- Qué quedó configurado y qué me falta instalar.
- Mis tareas del sprint actual, con el formato del protocolo de §4 de la guía:
  sprint, días para la weekly, mis pendientes en orden, lo que otros esperan de mí
  y lo que yo espero de otros.
- Por dónde empezaría y por qué.
Por último, recuérdame cerrar esta sesión y abrir Claude Code dentro de la carpeta terrun.
Así carga CLAUDE.local.md solo y, de ahí en adelante, me basta con preguntar "¿qué me toca?".
````

## Las sesiones siguientes

Abre Claude Code dentro de `terrun` y usa la plantilla de pedido de `docs/00-GUIA-DE-TRABAJO.md` §9. Si este texto cambia (por ejemplo, cuando existan `backend/` y `mobile/` y haya que instalar dependencias), lo actualiza Ashley en un PR.
