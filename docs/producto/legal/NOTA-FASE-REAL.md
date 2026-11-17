# Nota Interna: Transición a Fase Real (Post-Académica)

> **Uso interno del equipo — NO incluir en los documentos legales mostrados al usuario final.**  
> **Contexto:** Lista de acciones obligatorias antes de lanzar **Terrun** como plataforma pública real fuera del ámbito académico.

---

## Checklist de transición legal y técnica (`P2`)

1. **Responsable legal identificable:**
   - Sustituir el nombre provisional **"Equipo Terrun"** en los Términos y Condiciones y la Política de Privacidad por una persona física responsable o una entidad jurídica formalmente constituida (con domicilio legal y RNC si aplica).
2. **Titularidad de marca, diseño, código y datos:**
   - Definir por escrito quién es el titular del nombre **Terrun**, el diseño visual, el código fuente (incluyendo definir la licencia en un archivo `LICENSE` en el repositorio `AshFranco/Terrun`, actualmente público y sin licencia) y la base de datos, tomando en cuenta el acuerdo interno del equipo y la política de propiedad intelectual de la universidad (**INTEC**).
3. **Nueva aceptación de Términos y Política:**
   - Publicar una nueva versión de los Términos y la Política de Privacidad, notificar previamente a todos los usuarios existentes y exigir una nueva aceptación expresa (`POST /me/consents`) antes de usar la plataforma pública.
4. **Eliminación de cuenta y Apple Sign-In dentro de la app (`CTZ-76`):**
   - Implementar el botón de eliminación automática de cuenta directamente dentro de la aplicación móvil (requisito obligatorio tanto para **App Store** como para **Google Play**).
   - En iOS, habilitar el inicio de sesión con Apple (*Apple Sign-In*) como alternativa obligatoria cuando exista inicio de sesión con Google.
5. **Revisión legal profesional:**
   - Someter los Términos y Condiciones, la Política de Privacidad y el flujo de consentimiento a revisión por un abogado especialista en protección de datos (**Ley No. 172-13**) y comercio electrónico en la República Dominicana antes de abrir el registro público en tiendas.
