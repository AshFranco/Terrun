# Cumplimiento legal de Terrun

**Responsable del documento:** Diana (PO), con Ashley (SM). **Revisión:** 15/11/2026.

> Esta evaluación la preparó el equipo con fuentes públicas; **no es asesoría legal**. Antes de publicar en las tiendas, los textos legales deben revisarlos el profesor o una persona con formación jurídica.

Terrun es una app móvil **gratuita** que trata datos personales sensibles por su naturaleza (**ubicación GPS en segundo plano**, rutas, datos físicos para las calorías) y que se publicará en **Google Play** (CTZ-85). Por eso, además de la lista básica de cumplimiento, aplican la Ley 172-13 y las políticas de la tienda.

## Marco aplicable

| Norma o política | Por qué aplica | Qué exige en la práctica |
|---|---|---|
| **Ley 172-13** de Protección de Datos Personales (RD) | Guardamos correo, ubicación, rutas y datos físicos de personas en RD | Consentimiento informado y previo, finalidad declarada, calidad y seguridad de los datos, y derechos de acceso, rectificación, cancelación y oposición (*habeas data*) |
| **Ley 136-03**, Código para la protección de niños, niñas y adolescentes | Una app de ubicación puede atraer a menores | Definir una **edad mínima** o un consentimiento de padres o tutores; el tratamiento de datos de menores se rige por esta ley |
| **Política de eliminación de cuentas de Google Play** | La app permite crear cuentas | Eliminar la cuenta **desde la app y desde un enlace web**, borrar los datos asociados (desactivar no cuenta) y declararlo en el formulario de *Data safety* |
| **Política de permisos de Google Play (ubicación en segundo plano)** | Trackeamos con la pantalla bloqueada (RNF-02) | Aviso destacado dentro de la app antes de pedir el permiso (distinto de la política de privacidad), formulario de declaración en Play Console y un **video** que muestre el aviso, el permiso del sistema y los dos caminos (aceptar y rechazar). Responsables: Jorge (aviso en el trackeo, CTZ-20 y CTZ-33) y quien prepare la publicación (CTZ-85) |
| **Licencia ODbL de OpenStreetMap** y licencias de las teselas del mapa | El mapa usa datos de OpenStreetMap (y MapTiler, D-03) | Atribución visible en el mapa (`© OpenStreetMap contributors` y la del proveedor de teselas) |

## Los 9 puntos

| # | Punto | ¿Aplica? | Estado hoy | Qué falta y quién lo hace |
|---|---|---|---|---|
| 1 | **Términos y condiciones** | Sí | Pendiente: CTZ-27 (S1) | **Diana:** `docs/producto/legal/terminos-v1.0.md` (la base ya apunta a ese nombre). Debe incluir la edad mínima (punto 9) y las reglas de juego limpio (antitrampas) |
| 2 | **Política de privacidad** | Sí | Pendiente: CTZ-27 (S1) | **Diana:** `privacidad-v1.0.md`, con el contenido mínimo de la sección siguiente |
| 3 | **Política de cookies** | **No, por ahora** | La app nativa y la API no usan cookies; la encuesta no usa cookies ni analítica | Basta una línea en la política de privacidad: "Terrun no usa cookies". Se revisa si algún sitio web agrega analítica o publicidad |
| 4 | **Dos casillas separadas en cada formulario** | Sí | El modelo ya guarda cada documento por separado (`consent` por `legal_document`) | **Rowlis (CTZ-23):** en P-02, una casilla para los términos y otra para la privacidad, **desmarcadas** por defecto y con enlace a cada texto. **Ashley (CTZ-14):** el registro rechaza la cuenta si falta cualquiera de las dos versiones vigentes. **Encuesta:** agregar una casilla de consentimiento junto al correo de tester, o cerrar el formulario si ya no se usa |
| 5 | **Registro de cuándo y cómo aceptó cada persona** | Sí | La tabla `consent` guarda la fecha, la IP y la versión exacta (CTZ-13) | **Ashley (CTZ-14):** agregar **cómo** se aceptó (`method`: registro o ajustes) y la **versión de la app**. **Rowlis (CTZ-17):** `/me/consents` para aceptar versiones nuevas |
| 6 | **Política de reembolso** | **No** | Terrun no cobra nada en el período académico; el premium es posterior al MVP | Una línea en los términos: "El servicio es gratuito". Si llega el premium, aplican las políticas de reembolso de Google Play y App Store y la Ley 358-05 de protección al consumidor |
| 7 | **Pedir solo los datos necesarios** | Sí | El registro pide correo, contraseña, nombre y `@handle`; los datos físicos son opcionales | **Ashley:** fecha de nacimiento, sexo, peso y altura siguen siendo **opcionales** y se explica para qué sirven (calorías). La IP se guarda solo como evidencia del consentimiento y de seguridad (auditoría). Nunca se pide la dirección: el recorte de privacidad usa zonas que define el corredor |
| 8 | **Derechos de cada imagen** | Sí | La encuesta usa Google Fonts (licencia libre) y atribuye a OpenStreetMap | **Jorge (CTZ-10 y CTZ-21):** inventario de íconos, ilustraciones, logo y fuentes con su licencia en `docs/producto/legal/activos.md`; atribución de OpenStreetMap y del proveedor de teselas visible en el mapa de la app |
| 9 | **Leyes locales** | Sí | Ver el marco aplicable | **Diana:** decidir la edad mínima (propuesta: 18 años, o 16 con consentimiento de los padres o tutores) y registrarla en `DECISIONES.md`. **Ashley:** la eliminación de cuenta (`DELETE /me`, hoy P1) debe estar lista antes de publicar en Google Play, y hace falta una página web pública para pedirla |

## Contenido mínimo de la política de privacidad (Ley 172-13 y Google Play)

1. **Quién es responsable** de los datos (el equipo de Terrun, proyecto de la asignatura) y un correo de contacto.
2. **Qué datos** se recogen: correo, nombre y `@handle`, contraseña (solo su hash), ubicación GPS durante el trackeo **incluso con la pantalla bloqueada**, rutas y métricas, datos físicos opcionales, IP y dispositivo (seguridad), y fotos de perfil.
3. **Para qué** se usa cada uno: cuenta, cálculo de distancia y territorio, calorías, seguridad y antitrampas. **No** se venden ni se usan para publicidad.
4. **Qué ven otros:** el territorio (`@handle` y puntos) es público por la mecánica del juego; las actividades respetan su visibilidad; las rutas se recortan en las zonas de privacidad; **nunca** se muestra la ubicación en vivo (RNF-07).
5. **Con quién se comparte:** proveedores que alojan los datos (Render, Neon, Cloudflare R2, el proveedor de correo) y Expo para las notificaciones push.
6. **Cuánto tiempo** se guardan y cómo se borran: al eliminar la cuenta, los datos se borran en 24 h y el registro de auditoría se anonimiza (LLD §4.5).
7. **Derechos** de acceso, rectificación, cancelación y oposición: cómo ejercerlos (desde la app y por correo) y en qué plazo se responde.
8. **Edad mínima** (punto 9).
9. **Cookies:** no se usan.
10. **Cambios** a la política: se publica una versión nueva y se pide aceptarla de nuevo (`/me/consents`).

## Dónde se publican los textos

Google Play exige una URL pública para la política de privacidad y otra para pedir la eliminación de la cuenta. Con el dominio que se va a comprar para el correo (decisión del 15/11 en CTZ-14), los textos y la página de eliminación pueden publicarse en ese dominio con Netlify, que el equipo ya usa para la encuesta.
