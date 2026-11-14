---
titulo: terrun
lema: CONQUISTA TU ZONA · CADA CARRERA SUMA TERRITORIO
subtitulo: Documento de Diseño de Alto Nivel (HLD)
asignatura: Construcción de Software
version: 2.0
fecha: 13 de noviembre de 2026
---

# Control de cambios y equipo

> **Nota sobre versiones en el repositorio (`D-17`):** Esta **versión 2.0** (`docs/01-HLD.md` y `docs/entregables/HLD-Terrun.docx`) es el entregable oficial utilizado para **este período académico (alcance del MVP)** tras las observaciones del profesor. La **versión 1.2** extendida (con anexos completos y funcionalidades posteriores al MVP) se conserva en [`docs/01-HLD-v1.2-post-MVP.md`](01-HLD-v1.2-post-MVP.md) y `docs/entregables/HLD-Terrun-v1.2-post-MVP.docx`.

| Versión | Fecha | Descripción del cambio | Autores |
|---|---|---|---|
| **1.2** | 11/11/2026 | Reestructuración según el índice oficial de la asignatura, definición de roles del equipo y priorización del alcance. | Diana (Product Owner) / Ashley (Scrum Master) |
| **2.0** | 13/11/2026 | Versión académica concisa tras retroalimentación del profesor: uso del término *trackear*, acotación exclusiva al MVP y síntesis de funciones básicas y diferenciadoras. | Diana (Product Owner) / Ashley (Scrum Master) |

## Equipo del proyecto

| Integrante | Rol en Scrum | Responsabilidad principal en el proyecto |
|---|---|---|
| **Ashley** | Scrum Master | Coordinación ágil, arquitectura de servidor (Backend), base de datos espacial, motor de territorio, seguridad y despliegue. |
| **Diana** | Product Owner | Visión del producto, priorización del Backlog, criterios de aceptación, validación con corredores y documentación. |
| **Jorge** | Equipo de desarrollo | Desarrollo móvil (experiencia de usuario y núcleo técnico): diseño de pantallas, trackeo GPS, guardado sin internet y mapa interactivo. |
| **Rowlis** | Equipo de desarrollo | Desarrollo móvil y apoyo al servidor: estructura base de la app, cuenta, perfil, muro de actividad, clubes, gamificación y rankings. |
| **Rangelis** | Equipo de desarrollo | Aseguramiento de calidad (QA): plan de pruebas, casos de prueba por módulo, validación del servidor y seguimiento de defectos. |
| **Jaime** | Equipo de desarrollo | Aseguramiento de calidad (QA): pruebas de campo corriendo en calle con teléfonos reales, regresión y validación por entrega. |

# 1. Introducción

## 1.1 Propósito

El propósito de **Terrun** (*terra*, tierra, y *run*, correr) es resolver uno de los principales obstáculos del ejercicio urbano: **la falta de motivación y la monotonía**. Las aplicaciones deportivas tradicionales (como Strava o Nike Run Club) funcionan como bitácoras pasivas pensadas para quien busca récords de velocidad; para la mayoría de las personas, correr siempre por las mismas calles sin un estímulo inmediato provoca el abandono en las primeras semanas.

Bajo el lema **"Conquista tu zona · Cada carrera suma territorio"**, nuestra solución **transforma la ciudad real en un tablero de juego interactivo** donde cada salida a correr, trotar o caminar permite trackear el recorrido desde el teléfono, explorar, pintar y disputar zonas reales del mapa urbano, combinando la medición deportiva móvil con un juego de estrategia territorial y comunidad.

## 1.2 Público

La aplicación **Terrun** está dirigida a cuatro perfiles de usuarios urbanos:

1. **Corredores recreativos y principiantes:** personas que salen a caminar, trotar o correr y buscan una meta visual y alcanzable en cada salida (completar las calles de una zona cercana) en lugar de competir solo por velocidad.
2. **Corredores frecuentes y competitivos:** deportistas constantes que buscan **defender el dominio** de sus zonas en los últimos 30 días, subir en el ranking y coronarse como **rey o reina de su barrio**.
3. **Clubes de running y grupos de entrenamiento:** comunidades deportivas o grupos de amigos que unen los recorridos de sus integrantes para expandir el territorio dominado por su club.
4. **Exploradores urbanos:** usuarios motivados por descubrir y coleccionar nuevas calles, parques y sectores de su ciudad.

## 1.3 Uso previsto

En la rutina del corredor, **Terrun** se utiliza únicamente con el teléfono móvil en cuatro momentos:

1. **Antes de salir (estrategia en el mapa):** el corredor consulta el mapa de su sector para identificar qué zonas están libres, cuáles tiene en progreso y cuáles dominan otros corredores.
2. **Durante la carrera (trackeo en segundo plano y sin internet):** inicia el trackeo en dos toques y bloquea la pantalla; la app registra la ruta por GPS en la memoria local, muestra métricas en vivo, aplica *auto-pausa* en paradas y emite avisos de voz por kilómetro.
3. **Al terminar (validación y conquista):** el servidor filtra la señal GPS, descarta tramos en vehículo, calcula las métricas deportivas oficiales e ilumina las **zonas conquistadas**, los **robos de dominio**, el liderazgo por barrio y la experiencia (**XP**) ganada.
4. **Entre entrenamientos (comunidad y seguimiento):** el usuario revisa el muro de actividad (*Feed*), el territorio de su club, los rankings, sus medallas y sus notificaciones.

## 1.4 Lo que hacemos

Construimos una plataforma compuesta por una aplicación móvil para **Android e iOS** y un servidor central con motor geoespacial propio:

### 1. Malla hexagonal de dos niveles sobre la ciudad
Dividimos el mapa urbano en **zonas hexagonales** de unos 174 m de lado (0,10 km²). Para evitar que alguien conquiste una zona solo cruzándola por una avenida, cada zona se compone internamente de **7 sub-celdas pequeñas** (1 central y 6 alrededor).

![Cómo se divide una zona en 7 sub-celdas y cómo se conquista](diagramas/05-mecanica-h3.png)

### 2. Exploración personal y competencia territorial
- **Conquista permanente (5 de 7 sub-celdas):** al pisar al menos **5 de las 7 sub-celdas** de una zona (en una o varias salidas), el corredor la conquista para siempre en su historial personal.
- **Dominio competitivo (30 días) y robo de zonas:** quien pise más sub-celdas dentro de una zona en los **últimos 30 días** es su **dueño actual**; si otro corredor supera esa marca reciente, **le roba la zona**.
- **Barrios del polígono central:** agrupamos los hexágonos dentro de **6 barrios de Santo Domingo** (*Piantini, Naco, Evaristo Morales, Serrallés, Paraíso y Bella Vista*); quien domine más zonas activas dentro de un barrio recibe la corona de **rey o reina del barrio**.

### 3. Medición deportiva móvil, juego limpio y comunidad
Durante la carrera, el teléfono calcula en vivo el tiempo, la distancia y el ritmo; al finalizar, el servidor valida la ruta contra trampas en vehículo y calcula las métricas oficiales (*splits*, elevación y calorías), integrando clubes, muro de actividad con recorte de privacidad, niveles por **XP**, rachas, logros, retos y *rankings*.

## 1.5 Acrónimos y definiciones

| Término o sigla | Definición en el proyecto Terrun |
|---|---|
| **MVP y P0 / P1** | **MVP:** Producto Mínimo Viable para la entrega académica. **P0:** requisito imprescindible del núcleo. **P1:** complemento competitivo y social sujeto al cierre de P0. |
| **Actividad y Trackeo** | Salida a correr, trotar o caminar cuyo recorrido se registra (*trackea*) con el **GPS** (Sistema de Posicionamiento Global) del teléfono. |
| **Zona y Sub-celda (H3)** | **Zona:** hexágono grande del mapa (Uber H3 nivel 9, unos 174 m de lado). **Sub-celda:** cada una de las **7 partes pequeñas** (H3 nivel 10) que forman el interior de una Zona. |
| **Conquista (Regla 5 de 7)** | Logro personal **permanente** al pisar al menos **5 de las 7 sub-celdas** de una zona entre una o varias carreras. |
| **Dominio (30 días) y Robo** | Propiedad **activa** de una zona: pertenece a quien sume más sub-celdas visitadas en los **últimos 30 días**. Si otro corredor supera esa marca, ocurre un **robo de zona**. |
| **Barrio y Rey/Reina** | Cada uno de los 6 sectores del alcance inicial en Santo Domingo preasignados a los hexágonos del mapa; quien domine más zonas en él lidera el barrio. |
| **Ritmo (Pace) y Split** | **Ritmo:** tiempo empleado por kilómetro (`min/km`) en movimiento. **Split:** parcial individual de cada kilómetro completado. |
| **XP y Racha semanal** | **XP:** puntos de experiencia ganados por correr y conquistar zonas. **Racha:** semanas consecutivas completando al menos una actividad válida. |
| **HLD / LLD / RF / RNF** | **HLD:** diseño de alto nivel. **LLD:** diseño técnico detallado. **RF:** requisito funcional. **RNF:** requisito no funcional o de calidad. |
| **Scrum y QA** | **Scrum:** marco ágil de trabajo por *sprints*. **QA:** aseguramiento de calidad mediante pruebas automatizadas y de campo. |
| **API / HTTPS / JWT** | **API:** interfaz de comunicación entre la app y el servidor sobre conexión cifrada (**HTTPS**) autenticada con tokens seguros (**JWT**). |
| **Offline-first, GeoJSON y PostGIS** | **Offline-first:** guardado local en `SQLite` para trackear sin internet. **GeoJSON:** formato para dibujar hexágonos en el mapa. **PostGIS:** extensión espacial de PostgreSQL. |
| **Ley 172-13** | Ley dominicana de protección de datos personales que rige el consentimiento informado y la privacidad de las rutas del corredor. |

# 2. Descripción general

## 2.1 Necesidades del usuario

| Tipo de usuario | Quién es | Qué necesita del sistema |
|---|---|---|
| **Corredor** | Usuario registrado que sale a caminar, trotar o correr con su teléfono. | • Iniciar el trackeo de su recorrido en dos toques.<br>• Medir su carrera con pantalla bloqueada y sin depender de internet.<br>• Conquistar y disputar zonas de su ciudad y su barrio sin trampas en vehículo.<br>• Compartir territorio con su club y seguir su avance en rankings, retos y medallas. |
| **Visitante** | Persona que abre la app por primera vez sin cuenta. | • Comprender rápidamente la dinámica de Terrun y crear su cuenta conociendo cómo se protege su privacidad. |
| **Administrador** | Equipo técnico del producto. | • Ajustar en la base de datos las reglas del juego (umbrales de conquista, vigencia de 30 días, velocidades y puntos XP) sin republicar la app. |

## 2.2 Dependencias y Asunciones

El funcionamiento y la entrega del sistema dependen de las siguientes condiciones técnicas y operativas:

| Código | Tipo | Descripción de la dependencia o asunción | Mitigación y alcance en el proyecto |
|---|---|---|---|
| **D-01** | Dependencia | **Plataformas móviles y cuenta de publicación:** operación con el GPS interno en **Android 10+ (prioridad de entrega)** e **iOS 16+ (sujeto a disponibilidad de cuenta de pago en Apple Developer para TestFlight)**. | Demostración oficial garantizada en dispositivos Android reales y soporte multiplataforma en código. |
| **D-02** | Dependencia | **Instalador de desarrollo para GPS en segundo plano:** el sistema operativo móvil exige un binario compilado (*development build*) para mantener activo el GPS con la pantalla bloqueada. | Generación de compilaciones de desarrollo desde los primeros sprints en lugar de usar visores limitados. |
| **D-03** | Dependencia | **Servicios externos en capa gratuita:** disponibilidad y cuotas sin costo de *MapTiler / OpenStreetMap* (mapas), *Render* (servidor), *Neon* (base de datos), *Brevo / Resend* (correos) y *Cloudflare R2* (imágenes). | Caché de zonas en el teléfono y consumo acotado al volumen de usuarios de validación académica. |
| **D-04** | Dependencia | **Cartografía semilla de barrios:** delimitación de los 6 barrios iniciales de Santo Domingo (*Piantini, Naco, Evaristo Morales, Serrallés, Paraíso y Bella Vista*). | Cada hexágono H3 lleva preasignado su barrio en la carga inicial de base de datos, evitando cálculos geométricos pesados por carrera. |
| **S-01** | Asunción | **Conectividad móvil intermitente en calle:** el corredor puede perder señal de datos móviles durante el recorrido o al pasar entre edificios. | Arquitectura *offline-first* que guarda cada punto GPS en `SQLite` local y sincroniza al recuperar conexión. |

# 3. Requisitos y características del sistema

## 3.1 Requisitos funcionales

**Leyenda de prioridades del alcance del MVP (congelación de código: 6 de enero):**
- **P0 (Imprescindible — Núcleo del MVP):** los **17 requisitos obligatorios** que cubren el flujo completo de extremo a extremo (acceso, trackeo GPS sin internet, validación antitrampas, conquista 5 de 7 y disputa de zonas de 30 días).
- **P1 (Importante — Complemento competitivo y social):** los **12 requisitos deseables** (acceso con Google, barrios, XP, logros, retos, clubes, rankings, muro y avisos) condicionales a tener cerrado el bloque P0.

### Parte I — Funciones básicas de la aplicación

| Código | Módulo | Función y descripción resumida | Prioridad | Depende de |
|---|---|---|---|---|
| **RF-01** | **Registro y acceso** | **Registrar y verificar cuenta con correo:** crear cuenta con correo y contraseña segura y confirmar la titularidad del corredor mediante correo de verificación. | P0 | `RF-12` |
| **RF-02** | **Registro y acceso** | **Iniciar y cerrar sesión con correo:** autenticar al corredor con su correo y contraseña manteniendo su sesión segura mediante tokens `JWT` y revocándola al salir. | P0 | `RF-01` |
| **RF-03** | **Registro y acceso** | **Recuperar y cambiar contraseña:** restablecer la contraseña olvidada mediante enlace temporal enviado por correo o actualizarla desde los ajustes de cuenta. | P1 | `RF-01` |
| **RF-04** | **Registro y acceso** | **Iniciar sesión con cuenta de Google:** permitir el acceso rápido mediante identidad verificada de Google como alternativa al inicio con contraseña. | P1 | `RF-02` |
| **RF-05** | **Onboarding** | **Mostrar pantalla de bienvenida:** presentar al visitante la propuesta visual de Terrun y el acceso directo al registro o inicio de sesión. | P0 | — |
| **RF-06** | **Onboarding** | **Solicitar permiso de ubicación GPS:** explicar en español por qué se requiere el GPS en primer y segundo plano antes de mostrar el diálogo de permisos del teléfono. | P0 | — |
| **RF-07** | **Perfil y cuenta** | **Gestionar perfil propio:** consultar y editar nombre visible, `@handle`, foto de perfil, ciudad, datos físicos (para estimar calorías) y resumen de estadísticas propias. | P0 | `RF-02` |
| **RF-08** | **Navegación** | **Navegar por pestañas y pantalla de inicio:** organizar la app en pestañas principales con resumen rápido del corredor y botón para iniciar el trackeo en máximo dos toques. | P0 | `RF-02` |
| **RF-09** | **Comunidad** | **Consultar muro de actividad (Feed) con privacidad:** mostrar carreras, conquistas, dominios, coronas de barrio y logros recientes ocultando los metros iniciales y finales cerca del hogar. | P1 | `RF-17`, `RF-20`, `RF-22`, `RF-23`, `RF-25` |
| **RF-10** | **Notificaciones** | **Enviar notificaciones push y bandeja de avisos:** alertar al teléfono cuando otro corredor roba una zona o al desbloquear logros, listando los avisos dentro de la aplicación. | P1 | `RF-02`, `RF-22`, `RF-25` |
| **RF-11** | **Configuración** | **Configurar preferencias de carrera y avisos:** activar o desactivar la *auto-pausa* en paradas, los anuncios de voz por kilómetro y las alertas de territorio. | P1 | `RF-02` |
| **RF-12** | **Soporte legal** | **Aceptar términos y política de privacidad (Ley 172-13):** mostrar los documentos legales en español y registrar la fecha y versión aceptadas al crear la cuenta. | P0 | — |

### Parte II — Funciones específicas que caracterizan a Terrun

| Código | Módulo | Función y descripción resumida | Prioridad | Depende de |
|---|---|---|---|---|
| **RF-13** | **Trackeo deportivo** | **Trackear recorrido con GPS en primer y segundo plano:** capturar la ruta segundo a segundo y mostrar tiempo, distancia y ritmo en vivo incluso con la pantalla bloqueada. | P0 | `RF-06` |
| **RF-14** | **Trackeo deportivo** | **Pausar y reanudar carrera (manual y auto-pausa):** detener y reanudar el cronómetro en movimiento de forma manual o automática cuando el corredor se detiene en un semáforo. | P0 | `RF-13` |
| **RF-15** | **Trackeo deportivo** | **Emitir avisos de voz por kilómetro:** anunciar en español por audio el tiempo acumulado y el ritmo al completar cada kilómetro durante el trackeo. | P1 | `RF-11`, `RF-13` |
| **RF-16** | **Trackeo deportivo** | **Guardar ruta sin internet y sincronizar sin duplicados:** almacenar cada punto GPS en `SQLite` local durante la carrera y enviar el recorrido al servidor con identificador único. | P0 | `RF-13` |
| **RF-17** | **Trackeo deportivo** | **Calcular métricas deportivas oficiales y parciales (Splits):** calcular en el servidor la distancia oficial, tiempo en movimiento, ritmo (`min/km`), elevación, calorías y parciales por kilómetro. | P0 | `RF-16`, `RF-28` |
| **RF-18** | **Trackeo deportivo** | **Mostrar resumen post-carrera e historial de actividades:** presentar al finalizar el mapa del trazo, las métricas oficiales y las zonas ganadas, y listar el historial de carreras propias. | P0 | `RF-17` |
| **RF-19** | **Territorio H3** | **Visualizar mapa interactivo de zonas hexagonales:** dibujar sobre el mapa urbano las zonas H3 coloreadas según su estado (*libre, en progreso, conquistada, dominada por mí o por otro*). | P0 | `RF-08` |
| **RF-20** | **Territorio H3** | **Otorgar conquista permanente de zona (Regla 5 de 7):** proyectar el trazo validado sobre las 7 sub-celdas H3 internas y conceder la conquista definitiva al acumular al menos **5 de 7**. | P0 | `RF-17`, `RF-28` |
| **RF-21** | **Territorio H3** | **Consultar ficha de sub-celdas y balance territorial:** mostrar al tocar un hexágono el avance de sus 7 sub-celdas, su dueño actual y el total de zonas del corredor. | P0 | `RF-19`, `RF-20` |
| **RF-22** | **Territorio H3** | **Calcular dominio competitivo (30 días) y robo de zonas:** asignar el dominio activo de cada zona a quien sume más sub-celdas en los últimos 30 días, recalcular vencimientos y registrar robos. | P0 | `RF-20` |
| **RF-23** | **Barrios** | **Coronar rey o reina del barrio:** contabilizar las zonas activas dominadas dentro de cada uno de los 6 barrios del alcance inicial y coronar a quien lidere cada sector. | P1 | `RF-22` |
| **RF-24** | **Gamificación** | **Otorgar experiencia (XP) y subir de nivel:** acreditar puntos de experiencia por kilómetros válidos, conquistas y dominios de zona, actualizando el nivel del corredor. | P1 | `RF-17`, `RF-20` |
| **RF-25** | **Gamificación** | **Contabilizar racha semanal y desbloquear logros:** actualizar las semanas consecutivas entrenando y evaluar automáticamente el catálogo de medallas al cerrar cada carrera. | P1 | `RF-17`, `RF-24` |
| **RF-26** | **Gamificación** | **Participar en retos deportivos:** inscribirse en retos temporales de distancia o conquista de zonas y sumar automáticamente el aporte de cada actividad válida. | P1 | `RF-17` |
| **RF-27** | **Rankings** | **Consultar rankings individuales y por barrio:** mostrar las tablas de clasificación por zonas dominadas, zonas conquistadas, barrios y kilómetros acumulados. | P1 | `RF-17`, `RF-22`, `RF-23` |
| **RF-28** | **Antitrampas** | **Filtrar saltos de GPS y descartar tramos en vehículo:** depurar en el servidor los rebotes de señal y rechazar segmentos con velocidades o aceleraciones incompatibles con ir a pie. | P0 | `RF-16` |
| **RF-29** | **Clubes** | **Gestionar clubes y sumar territorio grupal:** crear un club, unirse mediante código de invitación y contabilizar el total de zonas dominadas por sus integrantes. | P1 | `RF-02`, `RF-22` |

---

## 3.2 Requisitos de interfaz externa

![Diagrama de contexto de Terrun](diagramas/01-contexto.png)

### 1. Interfaces de usuario, hardware y comunicaciones
- **Interfaz móvil y sensores:** pantallas íntegras en español con alto contraste para lectura al aire libre, uso del receptor GPS mediante servicio en primer plano (para trackear con pantalla bloqueada), almacenamiento local `SQLite` y salida de audio para avisos por kilómetro.
- **Protocolo de comunicación:** intercambio de datos cifrado sobre **HTTPS (TLS)** en formato **JSON** y representación de hexágonos y límites de barrios en estándar **GeoJSON**.

### 2. Interfaces de software (Servicios externos)

| Servicio externo | Propósito en Terrun | Mecanismo de integración |
|---|---|---|
| **MapTiler / OpenStreetMap** | Proveer la cartografía vectorial de calles y parques de Santo Domingo. | Descarga de teselas vectoriales desde **MapLibre** por HTTPS. |
| **Brevo / Resend y Google OAuth** | Envío de correos transaccionales y autenticación opcional con cuenta de Google. | Consumo de API REST por HTTPS y validación de tokens de identidad en el servidor. |
| **Cloudflare R2 y Expo Push / FCM** | Almacenamiento de avatares en la nube y entrega de notificaciones *push* al teléfono. | Subida mediante enlaces firmados temporales y envío de alertas desde el servidor. |

---

## 3.3 Características del sistema

### 1. Resumen de características principales
1. **Trackeo deportivo autónomo** (*offline-first*): registro continuo de carreras a pie con el GPS del teléfono en primer y segundo plano, métricas en vivo y resguardo local en `SQLite` ante cortes de internet.
2. **Motor territorial H3 de doble capa:** conversión de rutas reales en progreso acumulativo sobre sub-celdas (nivel 10) para otorgar conquistas permanentes (**regla 5 de 7**) y disputar el dominio activo de zonas (nivel 9) en ventanas móviles de **30 días**.
3. **Validación antitrampas en el servidor:** el teléfono captura el trazo en bruto, pero únicamente el servidor filtra saltos de GPS, descarta recorridos en vehículo y adjudica territorio y métricas oficiales.
4. **Barrios, clubes y comunidad:** liderazgo por barrios de Santo Domingo, agrupación en clubes para sumar territorio colectivo y muro de actividad con recorte de privacidad cerca del hogar.
5. **Progresión y motivación continua:** sistema de experiencia (**XP**) y niveles, rachas semanales, desbloqueo automático de medallas, retos deportivos y tablas de *rankings*.

### 2. Arquitectura general y tecnologías

El sistema emplea una arquitectura **Cliente-Servidor basada en un Monolito Modular**: una aplicación móvil en **React Native + Expo**, una API central en **.NET 10** que concentra los módulos de negocio y el motor matemático geoespacial, y una base de datos **PostgreSQL 16 + PostGIS**.

![Arquitectura de componentes de Terrun](diagramas/02-componentes.png)

| Componente | Tecnología elegida | Alternativas | Justificación técnica y del equipo |
|---|---|---|---|
| **Aplicación móvil** | **React Native + Expo (TypeScript)** | Flutter; PWA | Código compartido para Android e iOS, soporte nativo de GPS en segundo plano y dominio previo en el equipo. |
| **Mapa y almacenamiento local** | **MapLibre + SQLite** | Google Maps | Renderizado fluido de cientos de hexágonos sin costo de licencia y persistencia local sin internet. |
| **Servidor (Backend)** | **.NET 10 Web API** | NestJS, FastAPI | Alto desempeño en cálculo geoespacial y experiencia previa comprobada en producción dentro del equipo. |
| **Malla territorial y datos** | **Uber H3 + PostgreSQL 16 / PostGIS** | Cuadrícula propia | Relación exacta de 1 zona a 7 sub-celdas con barrios preasignados en semilla y consultas espaciales rápidas. |
| **Infraestructura y entrega** | **Render + Neon + GitHub Actions + EAS** | Servidor manual | Pruebas automatizadas en cada cambio, despliegue continuo del servidor y generación de instaladores móviles. |

### 3. Flujo de procesamiento y ambientes de trabajo
- **Ciclo de una carrera:** el teléfono guarda los puntos GPS en `SQLite` mientras el usuario corre y muestra el ritmo en vivo; al finalizar con internet, envía el lote identificado con un código único para evitar duplicados. El servidor limpia la ruta, descarta tramos en vehículo, actualiza las sub-celdas y zonas H3, evalúa en un solo paso los robos de dominio, barrios, XP y logros, y devuelve el resumen oficial.
- **Ambientes:** el desarrollo se realiza en entorno **Local** (con Docker), se valida continuamente en el ambiente de **Pruebas (QA)** desplegado en Render y Neon, y se publica en el ambiente de **Producción** para la demostración final.

---

## 3.4 Requisitos no funcionales

| Código | Aspecto de calidad | Criterio medible que debe cumplir el sistema | Prioridad |
|---|---|---|---|
| **RNF-01** | **Precisión deportiva** | Variación máxima del **3 %** en la distancia calculada frente a una ruta urbana de referencia. | P0 |
| **RNF-02** | **Trackeo en segundo plano** | Captura continua durante **al menos 1 hora** con la pantalla bloqueada sin cierre del servicio GPS. | P0 |
| **RNF-03** | **Consumo de batería** | Gasto moderado (meta de hasta **10 % de batería por hora** de trackeo activo) en un teléfono de gama media. | P1 |
| **RNF-04** | **Tiempos de respuesta** | Consultas del mapa y perfil en menos de **500 ms** (p95) y validación completa de una carrera en menos de **10 s**. | P0 |
| **RNF-05** | **Fluidez del mapa** | Carga de hexágonos visibles del sector en menos de **1 segundo** al desplazar el mapa. | P0 |
| **RNF-06** | **Seguridad** | Tráfico cifrado por HTTPS, contraseñas con hash irreversible y sesiones con tokens `JWT` de corta duración. | P0 |
| **RNF-07** | **Privacidad (Ley 172-13)** | Consentimiento informado, prohibición de exponer ubicación en vivo y recorte de ruta cerca del domicilio en el muro. | P0 |
| **RNF-08** | **Disponibilidad** | Servidor disponible y operativo durante las sesiones de pruebas de campo y la demostración final. | P1 |
| **RNF-09** | **Compatibilidad móvil** | Operación en **Android 10+ (prioridad de entrega)** e **iOS 16+ (sujeto a disponibilidad de cuenta Apple Developer)**. | P0 |
| **RNF-10** | **Usabilidad** | Inicio del trackeo en máximo **2 toques** desde la pantalla principal, con interfaz íntegra en español. | P0 |
| **RNF-11** | **Cobertura de pruebas** | Al menos el **80 %** de las reglas del motor de territorio y antitrampas verificadas con pruebas automáticas. | P0 |
| **RNF-12** | **Escalabilidad** | Diseño modular e índices espaciales que no impidan escalar en el futuro. | P1 |
