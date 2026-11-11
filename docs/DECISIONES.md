# Registro de decisiones

Toda decisión que cambie el alcance, las reglas del juego, el stack o el proceso se registra aquí. Las decisiones nuevas van **al final**, con el siguiente número.

| ID | Fecha | Decisión | Motivo | Decidió |
|---|---|---|---|---|
| D-01 | 10/11/2026 | La app se construye con React Native + Expo (TypeScript) | Una sola base para iOS y Android, GPS en segundo plano real, publicación en tiendas con EAS, aprovecha la experiencia React del equipo front. Confirmado en CTZ-07 (D-15) | Ashley |
| D-02 | 10/11/2026 | Backend en .NET 10 + EF Core, con PostgreSQL + PostGIS | Experiencia previa (FuelTrack): JWT, CI, Docker y despliegue ya probados | Ashley |
| D-03 | 10/11/2026 | Zona = hexágono H3 de resolución 9; 7 sub-celdas de resolución 10 | Área casi uniforme y jerarquía exacta de 7 hijas | Equipo |
| D-04 | 10/11/2026 | Conquista personal: al menos 5 de 7 sub-celdas, acumulado entre carreras (parámetro) | Premia explorar la zona a fondo, no solo atravesarla | Ashley (aprobado) |
| D-05 | 10/11/2026 | Dominio: dueño = más sub-celdas recorridas en los últimos 30 días; un empate conserva al dueño | Mantiene el juego vivo: el territorio no defendido se pierde | Ashley (aprobado) |
| D-06 | 10/11/2026 | Diana es la Product Owner; Ashley, la Scrum Master | Scrum necesita un dueño del backlog distinto de la SM | Equipo |
| D-07 | 10/11/2026 | Diseño completo de nivel de mercado (39 tablas, 112 operaciones), construido por prioridad P0/P1/P2 | La app apunta a comercializarse; lo que no alcance antes del 6/1 queda diseñado | Ashley |
| D-08 | 10/11/2026 | Congelamiento de código el miércoles 6 de enero; QA del 7 al 19 de enero; entrega el miércoles 20 | El equipo reserva dos semanas completas para QA | Equipo |
| D-09 | 10/11/2026 | La guía para asistentes de código vive en `docs/00-GUIA-DE-TRABAJO.md` y se carga desde un `CLAUDE.local.md` personal que no se sube al repo | El repositorio no debe contener archivos con nombre de herramientas de IA | Ashley |
| D-10 | 10/11/2026 | Estándares de desarrollo obligatorios (`docs/04-ESTANDARES-DESARROLLO.md`): dominio en español y técnica en inglés, sin tildes ni ñ en identificadores, glosario único, convenciones por capa y formato automático | Código coherente entre 3 desarrolladores y asistentes de código; evita sinónimos y mezclas de idioma | Ashley |
| D-11 | 10/11/2026 | **Todo el código en inglés estándar** (identificadores, BD, API, JSON, códigos de error, comentarios, logs, commits y ramas). En español quedan la documentación de `docs/`, las descripciones de PR e issues y los textos de la app (vía `src/i18n/es.json`). Reemplaza la parte de idioma de D-10 | Pedido del equipo: más estandarizado; facilita la comercialización y el trabajo con librerías y asistentes | Ashley |
| D-12 | 10/11/2026 | El producto se llama **Terrun** (*terra* + *run*); "Conquista tu zona" queda como lema. Repo, solución .NET (`Terrun.*`) y deep link (`terrun://`) usan el nombre nuevo | Nombre corto, internacional y pronunciable en español e inglés. Pendiente verificar disponibilidad en tiendas, dominio y redes | Equipo |
| D-13 | 11/11/2026 | **Barrios sobre la malla de zonas:** el hexágono sigue siendo lo que se conquista; cada zona pertenece a un barrio real y quien domina más zonas de un barrio es su rey o reina. Límites de barrios cargados como datos (ONE, OSM o manual) | Identidad local ("rey de Naco") sin perder la justicia de la malla H3. OpenStreetMap solo tiene el punto con el nombre de los barrios, no sus límites (verificado el 11/11) | Ashley |
| D-14 | 11/11/2026 | La encuesta a corredores es una página propia (`survey/`) publicada en **Netlify** y sus respuestas se guardan en **Netlify Forms**, en vez de Google Forms | Muestra la mecánica de conquista mientras se responde; no pide iniciar sesión y exporta a CSV. Gratis hasta 100 respuestas al mes | Ashley |
| D-15 | 11/11/2026 | Confirmado el supuesto S-01: Jorge y Rowlis se sienten cómodos con React + TypeScript y usarán React Native + Expo | Validado directamente con ambos desarrolladores front (CTZ-07); queda en firme D-01 sin necesidad de reevaluar Flutter | Diana, Jorge, Rowlis |
