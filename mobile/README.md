# App móvil — React Native + Expo

Responsables: **Jorge** (experiencia de usuario, sistema de diseño, trackeo GPS, almacén local y mapa) y **Rowlis** (estructura base, cuenta, perfil, muro de actividad, clubes, gamificación y rankings). Estructura: `docs/02-LLD.md` §1.3. Reglas de código: `docs/04-ESTANDARES-DESARROLLO.md` §5.

## Requisitos

- Node.js LTS (22+) y `npm`.
- App **Expo Go** en el teléfono (para pantallas iniciales) o **Android Studio** con emulador Android (para *development builds* desde `CTZ-09`).

Todos los comandos se ejecutan **desde esta carpeta** (`mobile/`).

## Instalación y arranque

```bash
npm install
npx expo start
```

- Presiona `a` para abrir en el emulador de Android o escanea el código QR con Expo Go.

## Verificación de calidad (CI `.github/workflows/mobile.yml`)

```bash
npm run lint           # ESLint (TypeScript strict, sin any, sin console.log)
npm run format:check   # Prettier (UTF-8, LF, 2 espacios)
npm run typecheck      # tsc --noEmit
npm test               # Jest + React Native Testing Library
```

Para aplicar el formato automáticamente antes de hacer commit:

```bash
npm run format
```

## Generar tipos desde el OpenAPI del backend (`CTZ-19`)

El contrato OpenAPI 3.1 del backend (`/openapi/v1.json`) es la única fuente de verdad de los tipos de red. **Nunca se edita `src/api/schema.d.ts` a mano** (`docs/00-GUIA-DE-TRABAJO.md` §8 regla 3).

1. **Desde el backend local** (con `docker compose up -d --build` corriendo en `backend/`):
   ```bash
   npm run api:types
   ```
2. **Desde el ambiente de QA** (primero abre `https://terrun-api-qa.onrender.com/health` y espera `Healthy`, ya que el plan gratis de Render duerme tras 15 min sin tráfico):
   ```bash
   # Bash / macOS / Linux
   OPENAPI_URL=https://terrun-api-qa.onrender.com/openapi/v1.json npm run api:types

   # PowerShell (Windows)
   $env:OPENAPI_URL="https://terrun-api-qa.onrender.com/openapi/v1.json"; npm run api:types
   ```
3. Verifica que la app compile con el nuevo contrato:
   ```bash
   npm run typecheck && npm test
   ```

Desde `src/api/index.ts` (`@/api`) se reexportan `paths`, `components`, `operations`, `ApiProblem` (errores RFC 7807) y la unión exhaustiva `ErrorCode`.
