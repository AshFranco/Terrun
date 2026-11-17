# Documentos Legales de Terrun (`CTZ-27` · `RF-12` · `RNF-07`)

**Responsable:** Diana Ferreras (Product Owner)  
**Versión vigente:** `1.7`  
**Fecha de entrada en vigor:** `2026-11-17T00:00:00Z` (`17 de noviembre de 2026`)  
**Marco legal:** Ley No. 172-13 de la República Dominicana (Protección de Datos de Carácter Personal).

---

## 1. Archivos en esta carpeta

| Archivo | Tipo (`legal_document_type`) | Versión (`version`) | Fecha de vigencia (`effective_from`) | Propósito |
|---|---|---|---|---|
| [`terminos-y-condiciones-v1.md`](terminos-y-condiciones-v1.md) | `terms` | `1.7` | `2026-11-17T00:00:00Z` | Reglas de uso de la app en fase de pruebas, desplazamiento no motorizado inclusivo, juego limpio antitrampas (`RF-28`), seguridad vial y límite de responsabilidad. |
| [`politica-de-privacidad-v1.md`](politica-de-privacidad-v1.md) | `privacy` | `1.7` | `2026-11-17T00:00:00Z` | Tratamiento de datos personales bajo la **Ley No. 172-13**, datos técnicos y de perfil, diferencia mapa/muro, recorte de 200 m (`RNF-07`), transferencia internacional (`Neon`, `Render`, `Cloudflare R2`, `Resend`, `Google Workspace`), conservación y derechos. |
| [`NOTA-FASE-REAL.md`](NOTA-FASE-REAL.md) | Interno | — | — | Checklist de acciones legales y técnicas obligatorias cuando Terrun pase de proyecto académico en pruebas a plataforma pública real. |

---

## 2. Datos iniciales para la semilla de base de datos (`CTZ-13` y `CTZ-17`)

En la migración inicial (`CTZ-13`, tabla `legal_document` del módulo `M1`) y en los endpoints `/legal/current` y `/me/consents` (`CTZ-17`), se deben cargar estos dos registros vigentes (**los usuarios nuevos aceptan la versión `1.7`**):

```json
[
  {
    "type": "terms",
    "version": "1.7",
    "url": "docs/producto/legal/terminos-y-condiciones-v1.md",
    "effectiveFrom": "2026-11-17T00:00:00Z"
  },
  {
    "type": "privacy",
    "version": "1.7",
    "url": "docs/producto/legal/politica-de-privacidad-v1.md",
    "effectiveFrom": "2026-11-17T00:00:00Z"
  }
]
```

*(Nota: cuando se publique la URL comercial en `CTZ-66`, el campo `url` apuntará al dominio web público sin cambiar la versión salvo que cambie el contenido).*

---

## 3. Textos cortos de interfaz para la aplicación móvil (`src/i18n/es.json`)

### 3.1 Casilla de consentimiento en Registro (`P-02`, `RF-01`, `RF-12`)
> *"He leído y acepto los **Términos y Condiciones (v1.7)** y la **Política de Privacidad (v1.7)** de Terrun, incluyendo el tratamiento de mis datos de actividad deportiva y ubicación GPS conforme a la Ley No. 172-13."*

### 3.2 Aviso previo al permiso de ubicación GPS (`P-06`, `RF-06`)
> *"Para trackear tus carreras y conquistar zonas hexagonales aunque lleves el teléfono en el bolsillo con la pantalla bloqueada, Terrun necesita acceso a tu ubicación GPS en primer y segundo plano **únicamente mientras tengas una actividad en curso**. Tu ubicación nunca se muestra en vivo a otros usuarios y tus rutas en el muro recortan automáticamente los primeros y últimos 200 metros para proteger tu privacidad."*
