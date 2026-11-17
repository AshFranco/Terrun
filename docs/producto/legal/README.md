# Documentos Legales de Terrun (`CTZ-27` · `RF-12` · `RNF-07`)

**Responsable:** Diana Ferreras (Product Owner)  
**Versión vigente:** `1.0`  
**Fecha de entrada en vigor:** `2026-11-17T00:00:00Z` (`17 de noviembre de 2026`)  
**Marco legal:** Ley No. 172-13 de la República Dominicana (Protección de Datos de Carácter Personal).

---

## 1. Archivos en esta carpeta

| Archivo | Tipo (`legal_document_type`) | Versión (`version`) | Fecha de vigencia (`effective_from`) | Propósito |
|---|---|---|---|---|
| [`terminos-v1.0.md`](terminos-v1.0.md) | `terms` | `1.0` | `2026-11-17T00:00:00Z` | Reglas de uso de la app en fase de pruebas, desplazamiento no motorizado inclusivo, juego limpio antitrampas (`RF-28`), seguridad vial y límite de responsabilidad. |
| [`privacidad-v1.0.md`](privacidad-v1.0.md) | `privacy` | `1.0` | `2026-11-17T00:00:00Z` | Tratamiento de datos personales bajo la **Ley No. 172-13**, datos técnicos y de perfil, diferencia mapa/muro, recorte de 200 m (`RNF-07`), transferencia internacional (`Neon`, `Render`, `Cloudflare R2`, `Resend`, `Google Workspace`), conservación y derechos. |
| [`NOTA-FASE-REAL.md`](NOTA-FASE-REAL.md) | Interno | — | — | Checklist de acciones legales y técnicas obligatorias cuando Terrun pase de proyecto académico en pruebas a plataforma pública real. |

---

## 2. Semilla de base de datos (`LegalDocumentSeed.cs`)

La semilla de documentos legales vive en `backend/src/Terrun.Infrastructure/Persistence/Seed/LegalDocumentSeed.cs`. Estos son los dos registros vigentes (`1.0`, vigencia desde `2026-11-17T00:00:00Z`):

```json
[
  {
    "type": "terms",
    "version": "1.0",
    "url": "https://github.com/AshFranco/Terrun/blob/development/docs/producto/legal/terminos-v1.0.md",
    "effectiveFrom": "2026-11-17T00:00:00Z"
  },
  {
    "type": "privacy",
    "version": "1.0",
    "url": "https://github.com/AshFranco/Terrun/blob/development/docs/producto/legal/privacidad-v1.0.md",
    "effectiveFrom": "2026-11-17T00:00:00Z"
  }
]
```

*(Nota: cuando se publique la URL comercial en `CTZ-66`, el campo `url` apuntará al dominio web público sin cambiar la versión salvo que cambie el contenido).*

---

## 3. Textos cortos de interfaz para la aplicación móvil (`src/i18n/es.json`)

### 3.1 Casillas de consentimiento separadas en Registro (`P-02`, `RF-01`, `RF-12`, `D-26`)
Deben mostrarse **por separado y desmarcadas por defecto**:
- **Casilla 1 (Términos):** *"He leído y acepto los **Términos y Condiciones (v1.0)** de Terrun y declaro que tengo al menos 18 años."*
- **Casilla 2 (Privacidad):** *"He leído y acepto la **Política de Privacidad (v1.0)** de Terrun, incluyendo el tratamiento de mis datos de actividad deportiva y ubicación GPS conforme a la Ley No. 172-13."*

### 3.2 Aviso previo al permiso de ubicación GPS (`P-06`, `RF-06`)
> *"Para trackear tus carreras y conquistar zonas hexagonales aunque lleves el teléfono en el bolsillo con la pantalla bloqueada, Terrun necesita acceso a tu ubicación GPS en primer y segundo plano **únicamente mientras tengas una actividad en curso**. Tu ubicación nunca se muestra en vivo a otros usuarios y tus rutas en el muro recortan automáticamente los primeros y últimos 200 metros para proteger tu privacidad."*
