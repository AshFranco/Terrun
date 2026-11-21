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

Con la API corriendo en local (`http://localhost:5080/openapi/v1.json`) o apuntando a QA:

```bash
# Desde el backend local (por defecto)
npm run api:types

# Desde el ambiente de QA
OPENAPI_URL=https://terrun-api-qa.onrender.com/openapi/v1.json npm run api:types
```

El comando regenera `src/api/schema.d.ts`. Ningún tipo del contrato de la API se escribe a mano (`docs/00-GUIA-DE-TRABAJO.md` §8 regla 3).
