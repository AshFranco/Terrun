---
titulo: Terrun
subtitulo: Documento de Diseño de Alto Nivel (HLD)
asignatura: Construcción de Software
version: 1.2
fecha: 13 de noviembre de 2026
---

> El entregable con el diseño visual de Terrun es `docs/entregables/HLD-Terrun.docx` (y su PDF): se edita en Word. Este archivo es la versión de lectura para el equipo y para Claude; cualquier cambio se hace en los dos.

# Control del documento

| VERSIÓN | FECHA | DESCRIPCIÓN DEL CAMBIO | AUTORES / RESPONSABLES |
|---|---|---|---|
| 1.0 | 10/11/2026 | Versión inicial del documento para aprobación del proyecto ("Conquista tu Zona"). | Ashley (Scrum Master) / Diana (Product Owner) |
| 1.1 | 11/11/2026 | Adopción del nombre comercial Terrun (D-12) e incorporación de barrios reales con título de rey o reina del barrio sobre la malla de zonas (D-13). | Ashley (Scrum Master) |
| 1.2 | 11/11/2026 | Reestructuración integral según el índice oficial de la asignatura (Introducción, Descripción general, Requisitos divididos por módulos, Características del sistema y Anexos), redacción en lenguaje natural accesible para todo el equipo y aplicación de la guía de diseño visual de Terrun; ajuste de asignaciones del equipo (D-16). | Diana (Product Owner) / Ashley (Scrum Master) |

## Equipo del proyecto

| INTEGRANTE | ROL EN SCRUM | RESPONSABILIDAD PRINCIPAL EN EL EQUIPO |
|---|---|---|
| **Ashley** | Scrum Master | Coordinación ágil del equipo, arquitectura de servidor (Backend), base de datos, motor de territorio, seguridad y publicación en la nube; asigna y revisa las funciones de servidor que desarrolla Rowlis. |
| **Diana** | Product Owner | Visión del producto, gestión y priorización de tareas (Backlog), criterios de aceptación, validación con corredores reales y documentación. |
| **Jorge** | Equipo de desarrollo | Desarrollo móvil (núcleo técnico): sistema de diseño base, grabación del recorrido por GPS, guardado sin internet, mapa interactivo y dibujo de zonas. |
| **Rowlis** | Equipo de desarrollo | Desarrollo móvil (experiencia de usuario): estructura base de la app, registro e inicio de sesión, perfil, red social, clubes y recompensas; apoyo al servidor (Backend) en funciones puntuales. |
| **Rangelis** | Equipo de desarrollo | Aseguramiento de calidad (QA): plan de pruebas, casos de prueba por módulo, pruebas del servidor y seguimiento de errores. |
| **Jaime** | Equipo de desarrollo | Aseguramiento de calidad (QA): pruebas de campo corriendo en la calle con teléfonos reales, pruebas de regresión y validación por entrega. |

# 1. Introducción

## 1.1 Propósito

El propósito de **Terrun** (combinación de terra, tierra, y run, correr) es resolver uno de los mayores obstáculos del ejercicio físico urbano: **la pérdida de motivación y la monotonía**. Aunque las aplicaciones deportivas tradicionales (como Strava, Nike Run Club o Adidas Running) son excelentes midiendo kilómetros y tiempos, funcionan como simples cuadernos de bitácora pensados para atletas que buscan batir récords de velocidad; para la mayoría de las personas, correr siempre por las mismas cuadras sin un estímulo inmediato termina provocando el abandono en las primeras semanas.

Bajo el lema **"Conquista tu zona · Cada carrera suma territorio"**, nuestra solución tiene como propósito **transformar la ciudad real en un tablero de juego interactivo** donde cada salida a correr, trotar o caminar sirve para explorar, pintar y disputar zonas reales del mapa urbano. De esta manera, unimos en una sola aplicación móvil la precisión técnica de un reloj deportivo con la emoción diaria de un juego de estrategia territorial y comunidad.

A nivel documental, el propósito de este **Diseño de Alto Nivel (HLD)** es describir de forma clara y en lenguaje natural cómo funciona esta solución, a qué público sirve, cuáles son sus requisitos por módulo y qué arquitectura la sostiene, dejando las especificaciones de código, tablas y fórmulas matemáticas para el documento complementario de **Diseño de Bajo Nivel (LLD)**.

## 1.2 Público

La aplicación **Terrun** está diseñada para atender a cuatro perfiles principales de usuarios dentro del ecosistema de corredores urbanos (inicialmente validado con corredores de Santo Domingo, pero escalable a cualquier ciudad):

**1.  Corredores recreativos y personas que inician el hábito:** usuarios que salen a caminar, trotar o correr de forma casual y que necesitan una meta visual, divertida y alcanzable en cada salida (completar las calles de una zona cercana para sumarla a su colección personal) en lugar de competir únicamente por quién corre más rápido.

**2.  Corredores frecuentes y competitivos:** deportistas constantes que entrenan varias veces por semana y encuentran en Terrun un nuevo desafío estratégico: **defender el dominio** de las zonas por donde entrenan, arrebatar hexágonos a otros corredores en la ventana de los últimos 30 días, escalar en los rankings semanales y coronarse como **rey o reina de su barrio** (por ejemplo, en sectores como Piantini, Naco, Evaristo Morales o Bella Vista).

**3.  Clubes de running y grupos de entrenamiento:** comunidades deportivas, grupos de amigos o equipos universitarios y corporativos que salen a correr juntos y buscan un objetivo colectivo: unir los recorridos de todos sus integrantes para expandir el territorio dominado por su club y superar retos grupales.

**4.  Exploradores urbanos:** personas motivadas por el coleccionismo y el descubrimiento que, gracias al mapa de zonas, se animan a variar sus rutas habituales para conocer nuevas calles, parques y sectores de su ciudad.

> **Lectores de este documento:** Además de describir el público de la aplicación, este documento está redactado para que todo el equipo del proyecto —Product Owner, Scrum Master, desarrolladores móviles y de servidor, aseguramiento de calidad (QA) y el cuerpo docente de Construcción de Software— comparta una misma visión del producto sin barreras técnicas.

## 1.3 Uso previsto

En la vida cotidiana del corredor, **Terrun** está pensada para utilizarse en cuatro momentos conectados entre sí:

**1.  Antes de salir a correr (consulta y estrategia en el mapa):** el corredor abre la aplicación y explora el mapa de su sector. De un vistazo identifica qué zonas hexagonales están libres, en cuáles ya tiene progreso acumulado (por ejemplo, 3 de 7 sub-celdas visitadas) y cuáles están dominadas por otros corredores o clubes, decidiendo así por qué calles le conviene trazar su ruta del día.

**2.  Durante la carrera (registro libre, con pantalla apagada y sin depender de internet):** con solo dos toques en la pestaña Grabar, el usuario inicia su actividad y guarda el teléfono en el bolsillo o brazalete con la pantalla bloqueada. La aplicación registra el recorrido por GPS segundo a segundo en la memoria interna del teléfono (funcionando incluso si se queda sin datos móviles), pausa el cronómetro automáticamente al detenerse en un semáforo (auto-pausa) y emite avisos de voz al completar cada kilómetro.

**3.  Al terminar el recorrido (validación, conquista y recompensas):** al presionar Finalizar, la aplicación envía la ruta al servidor. En pocos segundos, el servidor limpia la señal GPS, comprueba que el trayecto se haya hecho a pie y no en un vehículo, calcula las métricas deportivas oficiales (distancia, ritmo, tiempos por kilómetro o splits, calorías y récords personales) y actualiza el mapa: ilumina las **zonas recién conquistadas**, notifica si se logró **robar el dominio** de una zona o el reinado de un barrio y suma los puntos de experiencia (**XP**) ganados.

**4.  Entre entrenamientos (comunidad, defensa del territorio y seguimiento):** cuando no está corriendo, el usuario utiliza la aplicación para revisar el muro social (feed) de sus amigos, darles ánimos (kudos) y comentar sus carreras, consultar el ranking de su club, controlar el desgaste en kilómetros de sus zapatillas y responder a las notificaciones emergentes (push) cuando otro corredor le arrebata una zona y debe salir a recuperarla.

(En cuanto a la gestión de ingeniería del equipo, este documento se utiliza como la referencia principal para desglosar el Backlog por módulos, guiar la implementación de la arquitectura aprobada y validar los criterios de aceptación en cada entrega semanal).

## 1.4 Lo que hacemos

Para convertir esa experiencia en realidad, en **Terrun** construimos una plataforma completa compuesta por una aplicación móvil para **Android e iOS** y un servidor central con motor geoespacial propio. En concreto, esto es lo que hace nuestro sistema:

### 1. Transformamos el mapa urbano en una malla hexagonal de dos niveles

Dividimos la superficie de la ciudad en **zonas hexagonales** de unos 174 metros de lado (aproximadamente 0,10 km², el equivalente a un par de manzanas). Para evitar que alguien conquiste toda una zona simplemente cruzándola rápido por una avenida principal, dividimos cada zona internamente en **7 sub-celdas pequeñas** (una central y seis a su alrededor).

### 2. Combinamos dos modos de juego: exploración personal y competencia activa

- **Conquista permanente (5 de 7 sub-celdas):** cuando un corredor logra pisar al menos **5 de las 7 sub-celdas** de una zona —ya sea en una sola carrera exploratoria o sumando varias salidas en días distintos—, **conquista esa zona para siempre** en su mapa personal.
- **Dominio competitivo (ventana móvil de 30 días):** cada sub-celda distinta pisada en una actividad válida otorga **1 punto de zona**. Quien sume más puntos dentro de una zona durante los **últimos 30 días** (con un mínimo de 3 puntos) se convierte en su **dueño actual**. Si deja de correr por ahí o viene otro corredor y suma más puntos que él, **le roban la zona** y recibe un aviso inmediato en su teléfono.
- **Barrios reales y rey o reina del barrio:** agrupamos los hexágonos dentro de los **barrios oficiales** de la ciudad (como Piantini, Naco, Serrallés, Paraíso o Bella Vista). La persona que domine simultáneamente más zonas dentro de un barrio (mínimo 3 zonas) recibe la corona de **rey o reina del barrio**, visible en el mapa y en su perfil.

### 3. Entregamos medición deportiva precisa, juego limpio y privacidad

- **Medición deportiva completa:** calculamos distancia en kilómetros, tiempo en movimiento, ritmo promedio (**min/km**), ganancia de elevación, calorías estimadas, tabla de parciales por kilómetro (splits), detección automática de récords personales (1 km, 5 km, 10 km, 21 km, 42 km), exportación en archivo **GPX** y control de vida útil de zapatillas.
- **Verificación antitrampas en el servidor:** el teléfono nunca decide por sí solo quién gana una zona. Todas las coordenadas pasan por nuestro motor en el servidor, que descarta rebotes de señal GPS, anula tramos realizados a velocidad de vehículo (> 25 km/h) y envía a cuarentena (en revisión) carreras con ritmos sobrehumanos.
- **Privacidad del hogar por diseño:** permitimos configurar **zonas de privacidad** alrededor de la casa o el trabajo del corredor para que el servidor recorte y oculte el inicio y el final de sus rutas ante otros usuarios, y nunca exponemos la ubicación de nadie en tiempo real.

### 4. Fomentamos la vida en comunidad y la progresión constante

Integramos seguimiento entre amigos (con soporte para cuentas públicas y privadas), muro de actividades con kudos y comentarios, **clubes de corredores** con mapa territorial compartido, sistema de niveles por **XP**, rachas semanales, medallas automáticas, retos mensuales y tablas de posiciones (rankings) globales, por ciudad, por barrio y por club.

### 5. Cumplimiento de los objetivos de la asignatura

| REQUISITO DE CONSTRUCCIÓN DE SOFTWARE | CÓMO LO MATERIALIZA TERRUN |
|---|---|
| Crear un sistema o aplicación con un propósito claro, trabajando en equipo | Aplicación móvil para motivar la actividad física mediante un juego territorial sobre el mapa real; construida por un equipo de 6 integrantes con roles Scrum definidos. |
| Capturar datos reales y almacenarlos en una base de datos | Captura de recorridos GPS en la calle, métricas deportivas, estado de zonas H3 e interacciones sociales en una base de datos **PostgreSQL 16 con PostGIS** (41 tablas). |
| Publicar mediante un servicio web o tienda de aplicaciones | Servidor API publicado en internet (**Render**) y aplicación móvil distribuida en **Google Play** (prueba interna/cerrada) y **App Store** (TestFlight). |
| Contar con diseño de alto nivel (HLD), seguimiento semanal y Scrum Master | Este documento HLD junto al LLD detallado; ciclos semanales (sprints) de viernes a viernes con reuniones Weekly; **Ashley** como Scrum Master y **Diana** como Product Owner. |

## 1.5 Acrónimos y definiciones

A lo largo de este documento se mencionan conceptos propios de las reglas de **Terrun**, términos del mundo del running y siglas técnicas. Esta sección define cada uno de ellos en lenguaje claro para que el lector comprenda cualquier apartado del documento sin ambigüedades:

### A. Conceptos del juego territorial y del mundo del running

| TÉRMINO EN EL DOCUMENTO | QUÉ SIGNIFICA Y POR QUÉ ES IMPORTANTE PARA ENTENDER EL PROYECTO |
|---|---|
| **Actividad** | Una salida a correr, trotar o caminar grabada de principio a fin con el GPS del teléfono. Es la acción base que genera tanto las estadísticas deportivas como el avance en el mapa. |
| **Zona** | Cada uno de los hexágonos grandes en los que Terrun divide el mapa de la ciudad (celda H3 de resolución 9, de ~174 m de lado y ~0,105 km², similar a un par de cuadras). Es la pieza del mapa que se conquista y se disputa. |
| **Sub-celda** | Cada una de las **7 partes hexagonales pequeñas** (celdas H3 de resolución 10: 1 en el centro y 6 alrededor) que componen el interior de una Zona. Sirven para comprobar si el corredor realmente recorrió la zona por dentro. |
| **Conquista (Regla 5 de 7)** | Logro personal y **permanente**: ocurre cuando un corredor ha pisado al menos **5 de las 7 sub-celdas** de una zona, ya sea en una sola carrera o acumulando sub-celdas entre varias carreras distintas. Una zona conquistada nunca se pierde. |
| **Punto de zona** | Unidad con la que se mide quién domina una zona: cada sub-celda distinta pisada dentro de una zona durante una actividad válida suma **1 punto** (por tanto, una carrera puede aportar como máximo 7 puntos a una misma zona). |
| **Dominio (Ventana de 30 días)** | Propiedad competitiva y **temporal** de una zona: el **dueño actual** es el corredor que acumula más puntos de zona contando únicamente los **últimos 30 días** (con un mínimo de 3 puntos; si hay empate en puntos, conserva la zona quien ya era dueño). |
| **Robo de zona** | Evento competitivo que sucede cuando otro corredor supera los puntos de los últimos 30 días del dueño actual y le quita el dominio de esa zona, disparando un aviso al teléfono del dueño anterior. |
| **Barrio** | Sector real y conocido de la ciudad (como Piantini, Naco, Bella Vista o Serrallés) cuyos límites agrupan un conjunto de zonas hexagonales. Los barrios no se conquistan directamente, sino a través de las zonas que contienen. |
| **Rey o reina del barrio** | Título dinámico que recibe la persona que **domina más zonas** dentro de un mismo barrio en ese momento (siempre que domine al menos 3 zonas del barrio). |
| Ritmo (Pace) | Medida estándar de velocidad en el running: indica cuántos minutos y segundos tarda el corredor en recorrer **1 kilómetro** (por ejemplo, **5:30 min/km**). Mientras menor es el número, más rápido va el corredor. |
| **Split (Parcial por km)** | El corte o resumen individual de cada kilómetro completado dentro de una carrera (cuánto tiempo y qué ritmo hiciste en el km 1, en el km 2, en el km 3, etc.). |
| **Tiempo en movimiento vs. Tiempo total** | El tiempo total (elapsed time) cuenta todo el reloj desde que inicias hasta que terminas; el tiempo en movimiento (moving time) descuenta los minutos que estuviste detenido en pausas o semáforos, y es el que se usa para calcular tu ritmo real. |
| **Auto-pausa** | Función inteligente del grabador que pausa sola el reloj cuando detecta que el corredor se detuvo (por ejemplo, esperando cruzar una avenida) y vuelve a correr el tiempo en cuanto reanuda la marcha. |
| **Zona de privacidad** | Círculo de seguridad (por ejemplo, de 200 metros de radio) que el usuario configura alrededor de su casa o trabajo. El sistema recorta automáticamente el dibujo de la ruta dentro de ese círculo para que ningún desconocido vea dónde vive. |
| **Actividad en revisión (Cuarentena)** | Estado preventivo en el que el servidor coloca una carrera si detecta velocidades o ritmos sospechosos de haberse hecho en vehículo; mientras está en revisión no otorga zonas ni puntos hasta que un moderador la verifique. |
| **Kudos** | Aplauso o "me gusta" deportivo que los corredores dan a las actividades de sus amigos en el muro social para felicitarlos. |
| Racha semanal (Streak) | Contador de constancia que indica cuántas semanas seguidas lleva el corredor saliendo a realizar al menos una actividad válida sin fallar. |
| **XP (Puntos de experiencia)** | Puntos que gana el corredor por cada kilómetro recorrido, zona conquistada, zona robada, logro o reto completado, y que determinan su **nivel** dentro de la aplicación. |

### B. Acrónimos y términos técnicos o de proyecto mencionados en el documento

| SIGLA O TÉRMINO | SIGNIFICADO COMPLETO | QUÉ DEBES ENTENDER CUANDO APARECE EN ESTE DOCUMENTO |
|---|---|---|
| **HLD / LLD** | High-Level Design / Low-Level Design | **HLD** es este documento de Diseño de Alto Nivel (explica el qué, el porqué y la vista general); **LLD** es el documento de Diseño de Bajo Nivel (especifica el cómo técnico: las 41 tablas, las 116 operaciones de servidor y las fórmulas exactas). |
| **RF / RNF** | Requisito Funcional / Requisito No Funcional | Un **RF** describe una función que el usuario o el sistema puede realizar (ej. "grabar una carrera"); un **RNF** describe una condición de calidad obligatoria (ej. "gastar menos de 10 % de batería por hora"). |
| **P0 / P1 / P2** | Prioridades de construcción 0, 1 y 2 | **P0 (Imprescindible):** el núcleo que debe estar funcionando sin falta para la entrega académica. **P1 (Nivel comercial):** funciones sociales y competitivas que completan el producto. **P2 (Futuro):** ideas diseñadas para después de finalizar la materia. |
| H3 (Uber H3) | Hierarchical Spatial Index de Uber | Librería matemática gratuita creada por Uber que recubre el planeta con hexágonos de distintos tamaños. En Terrun usamos el **nivel 9** para las Zonas grandes y el **nivel 10** para sus 7 Sub-celdas internas. |
| **GPS / GPX** | Global Positioning System / GPS Exchange Format | **GPS** es el sensor del teléfono que obtiene la latitud, longitud y altitud del corredor en la calle; **GPX** es el formato universal de archivo (**.gpx**) que permite descargar una ruta para abrirla en otras apps o relojes deportivos. |
| **GeoJSON** | Geographic JSON | Formato estándar en el que nuestro servidor le envía al teléfono los bordes de los hexágonos y de los barrios junto con sus colores y estados para dibujarlos sobre el mapa. |
| **Offline-first y SQLite** | "Primero sin internet" / Base de datos local | Significa que la aplicación no necesita estar conectada a internet mientras corres: guarda cada punto GPS dentro del propio teléfono en una pequeña base de datos llamada **SQLite** y lo sube cuando regresa la señal. |
| **Servicio en primer plano** | Foreground Service (Android / iOS) | Permiso especial que muestra un aviso fijo ("Grabando carrera en curso") en la pantalla bloqueada del teléfono para evitar que el sistema operativo cierre la aplicación o apague el GPS para ahorrar batería. |
| **Idempotencia y `clientId` (UUID)** | Envío único garantizado | Al empezar a correr, el teléfono le asigna a la carrera un código único irrepetible (**UUID** o **clientId**). Si al subirla el internet parpadea y el teléfono la envía dos veces, el servidor reconoce ese código y evita que la carrera se guarde duplicada. |
| **Monolito Modular** | Arquitectura del servidor en .NET 10 | Forma en que construimos nuestro servidor: funciona como un solo programa central fácil de publicar y mantener, pero por dentro está dividido en **13 módulos ordenados** que no se mezclan entre sí. |
| **API REST y OpenAPI (Swagger)** | Interfaz de comunicación y catálogo de rutas | La **API REST** es el conjunto de puertas en internet por donde la aplicación móvil habla con el servidor; **OpenAPI (Swagger)** es el catálogo interactivo que documenta todas esas puertas y genera los tipos de datos automáticamente. |
| **PostGIS** | Extensión espacial de PostgreSQL | Complemento geográfico de nuestra base de datos que permite guardar rutas y polígonos en metros reales y responder en milisegundos preguntas como "¿qué zonas hexagonales caben en la pantalla actual del usuario?". |
| **JWT y OIDC** | JSON Web Token / OpenID Connect | **JWT** es el pase digital cifrado que mantiene la sesión abierta en el teléfono sin pedir la contraseña a cada minuto; **OIDC** es el protocolo seguro que permite iniciar sesión con un toque usando Google o Apple. |
| **CI/CD, QA y EAS** | Automatización, Calidad y Compilación móvil | **CI/CD:** robots en GitHub que prueban y publican el código automáticamente al aprobar un cambio. **QA:** equipo y ambiente de pruebas donde se valida todo antes de pasar a producción. **EAS:** servicio en la nube de Expo que genera los instaladores de Android e iPhone. |
| **Ley 172-13** | Ley de Protección de Datos Personales (RD) | Legislación de la República Dominicana que regula el manejo responsable de datos personales; guía nuestras reglas de consentimiento informado, privacidad por defecto y derecho a eliminar la cuenta. |

# 2. Descripción general

## 2.1 Necesidades del usuario

En Terrun participan cuatro tipos de personas (usuarios), cada una con necesidades muy concretas:

| TIPO DE USUARIO | QUIÉN ES | QUÉ NECESITA DEL SISTEMA |
|---|---|---|
| **Corredor** | Persona registrada que usa la aplicación para caminar, trotar o correr. | • Que la aplicación sea muy fácil de usar (empezar a grabar en dos toques).
• Que mida bien su distancia y su ritmo aunque guarde el teléfono en el bolsillo con la pantalla apagada o pase por un lugar sin internet.
• Ver cómo sus carreras pintan y conquistan zonas reales de su ciudad y de su barrio.
• Proteger su seguridad personal ocultando dónde empieza y termina su carrera (su casa).
• Compartir logros con amigos y su club sin que nadie haga trampa en vehículo. |
| **Visitante** | Persona que acaba de descargar la aplicación y aún no tiene cuenta. | • Entender de un vistazo de qué trata el juego en la pantalla de bienvenida.
• Crear una cuenta rápido (con su correo o con un botón de Google/Apple) sabiendo cómo se cuidarán sus datos de ubicación. |
| **Moderador** | Miembro de confianza encargado de mantener el juego limpio. | • Ver una lista clara de las carreras que el sistema detectó como sospechosas (por ejemplo, velocidades de automóvil) y de los reportes enviados por otros usuarios, para aprobarlas o anularlas. |
| **Administrador** | Equipo responsable del producto. | • Ajustar las reglas del juego (cuántas sub-celdas hacen falta para conquistar, cuántos puntos de experiencia da cada acción o qué velocidad se considera trampa) sin tener que reprogramar ni volver a publicar la aplicación. |

### Validación con corredores reales

Para no adivinar qué es lo que más les entusiasma a los corredores de Santo Domingo, el equipo lanzó una encuesta interactiva (con una demostración animada de cómo se conquistan las zonas en avenidas reales como la Winston Churchill y Piantini). Las respuestas de estos corredores reales nos permiten ordenar qué funciones de prioridad comercial (**P1**) construimos primero y quiénes participarán como primeros probadores de la aplicación.

## 2.2 Dependencias y Asunciones

Para que el proyecto funcione y se entregue a tiempo, dependemos de ciertos servicios externos y partimos de acuerdos (asunciones) validados por el equipo:

### Dependencias externas del sistema

**1.  Mapas base (OpenStreetMap / MapTiler):** la aplicación necesita descargar los planos de calles sobre los cuales se dibujan nuestras zonas hexagonales.

**2.  Inicio de sesión con Google y Apple:** para que el usuario pueda entrar con un toque sin inventar otra contraseña.

**3.  Servicio de notificaciones (Expo Push, Google FCM y Apple APNs):** para avisar al teléfono cuando alguien le roba una zona, le da un kudo o gana el título de su barrio.

**4.  Servicio de correo electrónico (Brevo / Resend):** para enviar el código de verificación al registrarse y el enlace de recuperación si olvida su contraseña.

**5.  Infraestructura en la nube (Neon, Render y Cloudflare R2):** donde viven nuestra base de datos, nuestro servidor y las fotos de perfil o de clubes.

**6.  Tiendas de aplicaciones (Google Play y App Store):** sujetas a sus propios tiempos de revisión y políticas de privacidad sobre el uso del GPS.

### Asunciones (Supuestos) y pendientes del proyecto

| CÓDIGO | TIPO | DESCRIPCIÓN EN LENGUAJE SENCILLO | ESTADO / RESPONSABLE |
|---|---|---|---|
| **S-01** | Asunción | Los desarrolladores de la aplicación móvil (Jorge y Rowlis) se sienten cómodos trabajando con **React y TypeScript**, por lo que usar React Native con Expo es el camino más rápido y seguro. | **Confirmado (11/11/2026):** validado con ambos desarrolladores (Decisión D-15). |
| **S-02** | Asunción | El equipo contará con las cuentas de desarrollador de Google Play y Apple Developer antes del quinto sprint para subir las versiones de prueba a las tiendas. | Planificado — **Diana** (Sprint 4). |
| **S-03** | Asunción | Los corredores cuentan con teléfonos con **Android 10 o superior** o **iPhone con iOS 16 o superior** y conceden el permiso de ubicación mientras usan la aplicación. | Cubierto en el diseño de permisos de la pantalla de inicio. |
| **P-01** | Pendiente | Recolectar al menos 15 respuestas de la encuesta a corredores antes del 18 de noviembre para ajustar el orden de las funciones complementarias (P1). | En curso — **Diana** (la encuesta ya está circulando). |
| **P-02** | Pendiente | Confirmar con el profesor el formato exacto y la duración de la presentación final del 20 de enero. | En seguimiento — **Ashley**. |
| **P-03** | Pendiente | Ajustar en la calle, con tres corredores de prueba, si los puntos de experiencia iniciales y los límites antitrampas se sienten justos. | Planificado — **Diana y Jaime** (Sprint 3). |

# 3. Requisitos y características del sistema

## 3.1 Requisitos funcionales (Divididos por módulos)

> **Nota sobre esta sección:** Para facilitar la lectura, la trazabilidad y el volcado al Backlog, los requisitos funcionales se organizan en dos bloques: primero los **módulos básicos de la aplicación** (comunes a la gestión de cuenta, acceso, navegación, ajustes y soporte legal) y a continuación los **módulos específicos que caracterizan a Terrun** (grabación deportiva GPS, equipamiento, conquista de territorio H3, barrios, comunidad, clubes, gamificación y antitrampas). Cada tabla identifica el **código del requisito (`RF-XX`)**, su **nombre en infinitivo**, su **descripción**, la **prioridad acordada por el equipo (`P0`, `P1` o `P2`)** y el **código del requisito del cual depende**.

**Leyenda de prioridades del proyecto:**

- **P0 (Imprescindible — MVP académico):** funciones obligatorias para el flujo principal y la demostración del curso.
- **P1 (Nivel comercial):** funciones que completan la experiencia competitiva, social y de publicación en tiendas dentro del diseño del proyecto.
- **P2 (Futuro — Versión 2):** capacidades previstas para la etapa comercial posterior a la entrega de la asignatura.

### Parte I — Módulos básicos de la aplicación

### Módulo 1 — Registro y autenticación

Agrupa la creación de la cuenta del corredor, la verificación de su identidad por correo, el inicio y cierre de sesión, la recuperación y el restablecimiento de contraseña, y el acceso mediante proveedores sociales.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-01** | **Registrar cuenta con correo** | Permitir al visitante crear una cuenta ingresando su correo electrónico y una contraseña segura (mínimo 10 caracteres), generando el envío de un correo de verificación. | **P0** | **RF-35** |
| **RF-02** | **Verificar correo electrónico** | Confirmar la titularidad del correo mediante el código o enlace recibido (vigencia de 24 horas) y permitir solicitar un reenvío si el mensaje no llegó. | **P0** | **RF-01** |
| **RF-03** | **Iniciar sesión con correo** | Autenticar al corredor con su correo verificado y su contraseña, manteniendo su sesión activa de forma segura en el teléfono (SecureStore) con renovación automática (refresh token rotativo). | **P0** | **RF-02** |
| **RF-04** | **Cerrar sesión activa** | Finalizar la sesión del usuario en el teléfono y revocar inmediatamente el pase de renovación (refresh token) en el servidor. | **P0** | **RF-03** |
| **RF-05** | **Solicitar recuperación de contraseña** | Enviar al correo electrónico registrado un enlace o código temporal (vigencia de 1 hora y un solo uso) cuando el usuario olvida su contraseña y no puede iniciar sesión. | **P0** | **RF-01** |
| **RF-06** | **Restablecer contraseña con token** | Definir una nueva contraseña validando el código o enlace temporal recibido por correo y revocar automáticamente todas las sesiones abiertas del usuario. | **P0** | **RF-05** |
| **RF-07** | **Iniciar sesión con Google** | Permitir registrarse o entrar con un toque usando una cuenta verificada de Google (OIDC), vinculándola automáticamente si el correo ya existe en el sistema. | **P1** | **RF-35** |
| **RF-08** | **Iniciar sesión con Apple** | Permitir registrarse o entrar en dispositivos iOS usando una cuenta de Apple (Sign in with Apple), validando su firma oficial en el servidor. | **P1** | **RF-35** |

### Módulo 2 — Onboarding y permisos iniciales

Agrupa la pantalla de bienvenida para quienes abren la aplicación por primera vez y las solicitudes individuales de permisos del teléfono.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-09** | **Mostrar pantalla de bienvenida** | Presentar al visitante la introducción visual a la mecánica de Terrun y las opciones para registrarse o iniciar sesión. | **P0** | — |
| **RF-10** | **Solicitar permiso de ubicación** | Explicar en español para qué necesita la aplicación el GPS (incluso con pantalla bloqueada) antes de lanzar el aviso del sistema operativo, permitiendo usar el resto de la app si se deniega. | **P0** | **RF-03** |
| **RF-11** | **Solicitar permiso de notificaciones** | Pedir autorización al usuario para enviarle avisos emergentes al teléfono sobre robos de zona e interacciones sociales, sin bloquear el uso de la app si lo rechaza. | **P1** | **RF-03** |

### Módulo 3 — Gestión de perfil y cuenta

Agrupa la consulta del perfil propio y de terceros, la edición de datos personales y foto, el cambio de contraseña con sesión activa, la descarga de datos y la baja de la cuenta.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-12** | **Consultar perfil propio** | Mostrar al corredor sus datos personales, nombre de usuario **@handle**, foto, nivel actual, récords y resumen de estadísticas propias. | **P0** | **RF-03** |
| **RF-13** | **Consultar perfil de otro corredor** | Mostrar el perfil público de otro usuario y sus estadísticas; si su cuenta es privada, ocultar el detalle a quienes no sean seguidores aceptados. | **P0 / P1** | **RF-03** |
| **RF-14** | **Editar datos personales del perfil** | Permitir modificar el nombre visible, el identificador único **@handle**, la biografía, la ciudad, el país y los datos físicos (fecha de nacimiento, sexo, peso en kg y estatura en cm para calorías). | **P0** | **RF-12** |
| **RF-15** | **Actualizar fotografía de perfil** | Permitir subir o reemplazar la foto de perfil almacenándola en el servicio de archivos en la nube mediante una URL segura firmada. | **P1** | **RF-14** |
| **RF-16** | **Cambiar contraseña desde la cuenta** | Permitir al corredor con sesión iniciada modificar su contraseña actual ingresando primero su clave vigente e invalidando las sesiones anteriores. | **P1** | **RF-03** |
| **RF-17** | **Exportar datos personales** | Permitir descargar desde los ajustes de cuenta un archivo comprimido (**ZIP**) con toda la información del usuario en formato **JSON** y sus rutas en formato **GPX**. | **P1** | **RF-03** |
| **RF-18** | **Eliminar cuenta de usuario** | Permitir dar de baja definitiva la cuenta previa confirmación, revocando sus sesiones, eliminando sus datos personales y liberando las zonas que dominaba en el mapa. | **P1** | **RF-03** |

### Módulo 4 — Navegación, pantalla principal y búsqueda

Agrupa la barra de navegación principal de la aplicación, el muro de actividad social y los buscadores de usuarios y clubes.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-19** | **Navegar por pestañas principales** | Organizar la aplicación en una barra inferior de 5 pestañas (Mapa, Feed, Grabar, Clubes y Perfil), abriendo el Mapa como pantalla principal (Home) para iniciar carrera en máximo dos toques. | **P0** | **RF-03** |
| **RF-20** | **Consultar muro de actividad (Feed)** | Mostrar en la pestaña Feed la lista cronológica paginada de las actividades recientes propias y de los corredores seguidos. | **P1** | **RF-19** |
| **RF-21** | **Buscar corredores** | Permitir encontrar a otros usuarios registrados escribiendo su nombre visible o su identificador **@handle**. | **P1** | **RF-19** |
| **RF-22** | **Buscar clubes de corredores** | Permitir encontrar clubes deportivos filtrando por su nombre o por la ciudad a la que pertenecen. | **P1** | **RF-19** |

### Módulo 5 — Notificaciones

Agrupa el registro del teléfono para alertas push, el envío de avisos al dispositivo y la consulta y lectura de la bandeja interna.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-23** | **Registrar dispositivo para avisos Push** | Guardar el identificador del teléfono (Expo Push Token) al conceder permisos y desvincularlo cuando el usuario cierra sesión. | **P1** | **RF-11** |
| **RF-24** | **Enviar notificaciones Push al teléfono** | Emitir avisos emergentes al teléfono ante robos de zona, cambios de rey del barrio, kudos, comentarios, seguidores, logros o retos, abriendo la pantalla asociada al tocarlos. | **P1** | **RF-23** |
| **RF-25** | **Consultar bandeja de notificaciones** | Mostrar dentro de la aplicación el listado paginado de notificaciones recibidas y el contador de avisos pendientes por leer. | **P1** | **RF-03** |
| **RF-26** | **Marcar notificaciones como leídas** | Permitir cambiar el estado de una notificación individual o de toda la bandeja a leída, actualizando el contador de pendientes. | **P1** | **RF-25** |

### Módulo 6 — Configuración, ajustes y privacidad

Agrupa cada preferencia de unidades, ayudas durante la carrera, privacidad de cuenta, recorte de rutas en el hogar y bloqueo de usuarios.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-27** | **Configurar unidades de medida** | Permitir elegir entre el sistema métrico (**km**, **min/km**, **kg**) y el sistema imperial (**mi**, **min/mi**, **lb**) para mostrar todas las pantallas. | **P0** | **RF-03** |
| **RF-28** | **Configurar auto-pausa y avisos de voz** | Permitir activar o desactivar la pausa automática en paradas y los avisos de voz durante la carrera, definiendo cada cuántos kilómetros se anuncian. | **P0** | **RF-03** |
| **RF-29** | **Configurar privacidad del perfil** | Permitir definir si el perfil es público o privado (exigiendo aprobar manualmente a cada nuevo seguidor). | **P0** | **RF-12** |
| **RF-30** | **Configurar visibilidad por defecto de rutas** | Permitir elegir quién puede ver las actividades nuevas por defecto: Público, Solo seguidores o Privado (solo yo). | **P0** | **RF-03** |
| **RF-31** | **Configurar preferencias de avisos** | Permitir encender o silenciar individualmente cada tipo de notificación (robo de zona, kudos, comentarios, nuevos seguidores y retos). | **P0** | **RF-03** |
| **RF-32** | **Gestionar zonas de privacidad del hogar** | Permitir crear y eliminar hasta 5 áreas circulares (de 100 m a 1.000 m de radio) alrededor de casa o trabajo para ocultar el inicio y fin de las rutas frente a terceros. | **P1** | **RF-03** |
| **RF-33** | **Bloquear y desbloquear usuarios** | Permitir bloquear a un usuario (eliminando el seguimiento e impidiendo visibilidad mutua), consultar la lista de bloqueados y desbloquearlo cuando se desee. | **P1** | **RF-03** |

### Módulo 7 — Soporte legal y reporte de problemas

Agrupa la consulta de documentos legales vigentes, el registro del consentimiento del usuario y el envío de reportes a moderación.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-34** | **Consultar términos y política de privacidad** | Mostrar al usuario la versión vigente de los términos de uso y de la política de privacidad redactada conforme a la Ley 172-13. | **P0** | — |
| **RF-35** | **Registrar aceptación de documentos legales** | Guardar constancia verificable de la versión, fecha y dirección IP con que el usuario aceptó los términos y la política de privacidad vigentes. | **P0** | **RF-34** |
| **RF-36** | **Reportar problemas, abusos o trampas** | Permitir enviar un reporte sobre un usuario, actividad sospechosa, comentario o club indicando el motivo (trampa, acoso, spam, contenido inapropiado u otro) y su detalle. | **P1** | **RF-03** |

### Parte II — Módulos específicos que caracterizan a Terrun

> **Priorización respaldada por la encuesta a corredores (23 respuestas reales):** El orden de estos módulos y el peso de sus requisitos **P1** incorpora los resultados de la encuesta (**runner-survey**), donde el **87 %** corre con su teléfono y el **91 %** mostró interés en la mecánica territorial. El orden de importancia votado por los corredores fue: **1.º Métricas y ritmo (124 pts)**, **2.º Logros y recompensas (110 pts)**, **3.º Mapa de territorio (101 pts)**, **4.º Retos mensuales (95 pts)**, **5.º Rankings (81 pts)**, **6.º Clubes (69 pts)** y **7.º Feed social (64 pts)**, destacando además el reclamo de contar con métricas de ritmo/distancia y rutas sin muros de pago y con retroalimentación de ritmo durante la carrera.

### Módulo 8 — Grabación de actividades y métricas deportivas

Es el corazón deportivo de Terrun (votado como **prioridad #1** en la encuesta): captura el recorrido real por GPS en la calle, opera sin conexión a internet, ofrece retroalimentación de ritmo por kilómetro y calcula gratuitamente todas las estadísticas de desempeño y récords del corredor.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-37** | **Grabar recorrido con GPS en primer y segundo plano** | Registrar las coordenadas de una carrera, trote o caminata segundo a segundo tanto con la aplicación abierta como con la pantalla bloqueada en el bolsillo, manteniendo un aviso visible de grabación activa. | **P0** | **RF-10** |
| **RF-38** | **Pausar y reanudar grabación de actividad** | Permitir pausar y reanudar manualmente el cronómetro o pausarlo de forma automática (auto-pausa) cuando la velocidad baja al detenerse en un semáforo, abriendo un nuevo segmento de ruta al retomar la marcha. | **P0** | **RF-28**, **RF-37** |
| **RF-39** | **Emitir avisos de voz por kilómetro** | Anunciar por altavoz o auriculares la distancia, el tiempo acumulado y el ritmo del último kilómetro completado para guiar la intensidad del corredor sin mirar la pantalla. | **P1** | **RF-28**, **RF-37** |
| **RF-40** | **Almacenar recorrido localmente sin internet** | Guardar cada punto GPS en tiempo real dentro de la base de datos interna del teléfono (**SQLite**), permitiendo recuperar la actividad sin perder puntos si el sistema operativo cierra la aplicación. | **P0** | **RF-37** |
| **RF-41** | **Sincronizar actividad con el servidor sin duplicados** | Mantener una cola de subida con reintentos automáticos cuando vuelva la conexión a internet, enviando la carrera con un código único (**clientId**) para que nunca se duplique. | **P0** | **RF-40** |
| **RF-42** | **Calcular métricas oficiales de la actividad** | Calcular en el servidor, a partir de los puntos GPS válidos, la distancia oficial, el tiempo total, el tiempo en movimiento, el ritmo promedio (**min/km**), la velocidad máxima, la ganancia/pérdida de elevación y las calorías quemadas. | **P0** | **RF-41** |
| **RF-43** | **Generar parciales por kilómetro (Splits)** | Desglosar automáticamente el tiempo empleado, el ritmo promedio y el desnivel de cada kilómetro individual de la actividad (incluyendo el último tramo parcial). | **P0** | **RF-42** |
| **RF-44** | **Mostrar resumen post-carrera** | Presentar al finalizar la actividad una pantalla de resumen con la ruta trazada, las métricas calculadas, las zonas conquistadas en esa salida y la experiencia (**XP**) obtenida. | **P0** | **RF-42** |
| **RF-45** | **Consultar historial de actividades propias** | Mostrar el listado paginado de las actividades del corredor con filtros por fecha y tipo (carrera, trote o caminata). | **P0** | **RF-42** |
| **RF-46** | **Consultar detalle y ruta de una actividad** | Mostrar la vista detallada de una carrera con su trazo simplificado sobre el mapa (recortando las zonas de privacidad si la consulta un tercero) y su tabla de splits. | **P0** | **RF-43**, **RF-45** |
| **RF-47** | **Consultar resúmenes de progreso por periodo** | Mostrar los totales acumulados de distancia, tiempo y actividades por semana, mes o año, comparándolos con el periodo anterior para evaluar la progresión del corredor. | **P0** | **RF-45** |
| **RF-48** | **Editar información de una actividad** | Permitir al dueño de la actividad modificar su título, descripción, tipo de ejercicio, nivel de visibilidad o zapatilla utilizada. | **P0** | **RF-46** |
| **RF-49** | **Eliminar una actividad registrada** | Permitir al dueño borrar una actividad propia, revirtiendo automáticamente sus visitas de sub-celdas, su experiencia (XP) y recalculando el dominio de las zonas afectadas. | **P0** | **RF-46** |
| **RF-50** | **Detectar récords personales automáticamente** | Identificar tras cada carrera si el corredor logró su menor tiempo histórico en **1 km, 5 km, 10 km, media maratón (21,1 km) o maratón (42,2 km)**, o su carrera de mayor distancia, y mostrarlos en su vitrina de récords. | **P1** | **RF-42** |

### Módulo 9 — Territorio, conquista hexagonal H3 y barrios

Es el motor que distingue a Terrun de las bitácoras tradicionales: convierte las rutas corridas sobre la ciudad en exploración de sub-celdas, conquistas permanentes, disputas de dominio de 30 días y reinados por barrio.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-51** | **Visualizar mapa de zonas hexagonales por estado** | Mostrar sobre el mapa interactivo las zonas H3 (resolución 9) del área visible coloreadas según su estado frente al corredor (libre, en progreso, conquistada por mí, dominada por mí o dominada por otro). | **P0** | **RF-19** |
| **RF-52** | **Registrar progreso de sub-celdas visitadas** | Convertir el trazo válido de cada actividad en sub-celdas H3 (resolución 10, de 0 a 6) visitadas y actualizar la máscara de progreso (de 7 partes) del corredor en cada zona tocada. | **P0** | **RF-42** |
| **RF-53** | **Otorgar conquista permanente de zona (Regla 5 de 7)** | Conceder al corredor la conquista definitiva de una zona cuando acumule al menos **5 de sus 7 sub-celdas** visitadas entre una o varias carreras válidas. | **P0** | **RF-52** |
| **RF-54** | **Calcular dominio competitivo de zona (30 días)** | Asignar como dueño actual de una zona al corredor que sume más puntos de sub-celdas visitadas en los **últimos 30 días** (mínimo 3 puntos; en caso de empate conserva la zona el dueño vigente). | **P1** | **RF-52** |
| **RF-55** | **Recalcular vencimiento nocturno de dominio** | Ejecutar cada noche un proceso automático que descuente los puntos con más de 30 días de antigüedad, liberando o cambiando de dueño las zonas que no fueron defendidas. | **P1** | **RF-54** |
| **RF-56** | **Notificar robo o pérdida de dominio de zona** | Registrar el evento territorial (robo, defensa, conquista de dominio o pérdida por inactividad) y enviar una alerta inmediata al dueño anterior cuando otro corredor le arrebata su zona. | **P1** | **RF-24**, **RF-54** |
| **RF-57** | **Consultar detalle y sub-celdas de una zona** | Mostrar al tocar un hexágono quién es su dueño actual, el dibujo de sus 7 sub-celdas con el progreso propio y los 5 corredores con más puntos en los últimos 30 días. | **P0** | **RF-51**, **RF-52** |
| **RF-58** | **Consultar historial de eventos de una zona** | Mostrar la cronología de conquistas de dominio, defensas y robos ocurridos en una zona específica. | **P1** | **RF-54**, **RF-57** |
| **RF-59** | **Consultar resumen territorial del corredor** | Mostrar al usuario el conteo y listado de sus zonas conquistadas, las zonas que domina actualmente (con su margen de puntos y fecha de vencimiento) y las que están en riesgo de perderse. | **P0 / P1** | **RF-53**, **RF-54** |
| **RF-60** | **Visualizar límites de barrios en el mapa** | Agrupar las zonas dentro de los polígonos de los barrios reales de la ciudad (Piantini, Naco, Bella Vista, etc.) y dibujar sus bordes y nombres sobre el mapa. | **P1** | **RF-51** |
| **RF-61** | **Coronar rey o reina del barrio** | Otorgar automáticamente el título de rey o reina del barrio al corredor que domine más zonas dentro de él (mínimo 3 zonas dominadas), notificando cuando alguien gana o pierde la corona. | **P1** | **RF-54**, **RF-60** |
| **RF-62** | **Consultar detalle y clasificación de un barrio** | Mostrar la ficha de un barrio con su rey o reina actual, el top 10 de corredores por zonas dominadas en ese sector y las zonas que el propio usuario posee allí. | **P1** | **RF-61** |

### Módulo 10 — Gamificación: experiencia (XP), logros, rachas y retos

Mantiene la motivación a mediano y largo plazo mediante un sistema de recompensas (votado en **2.º lugar** por los corredores en el apartado de Logros y **4.º lugar** en Retos mensuales).

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-63** | **Otorgar puntos de experiencia (XP)** | Acreditar transacciones de XP al corredor por cada kilómetro válido recorrido, zona conquistada, dominio obtenido, robo de zona, logro desbloqueado o reto completado, sin duplicar recompensas. | **P1** | **RF-42**, **RF-53** |
| **RF-64** | **Calcular nivel de progresión del corredor** | Determinar el nivel actual del usuario a partir de su XP total acumulado según la curva configurada y mostrar la barra de progreso hacia el siguiente nivel. | **P1** | **RF-63** |
| **RF-65** | **Contabilizar racha semanal de constancia** | Calcular al cierre de cada semana cuántas semanas consecutivas lleva el corredor completando al menos una actividad válida y registrar su racha histórica más larga. | **P1** | **RF-42** |
| **RF-66** | **Desbloquear logros automáticamente** | Evaluar tras cada actividad procesada si el corredor alcanzó las condiciones de alguna medalla activa del catálogo (por distancia, conquistas, robos, racha o nivel) y otorgarla. | **P1** | **RF-63** |
| **RF-67** | **Consultar catálogo y avance de logros** | Mostrar la galería de medallas obtenidas y pendientes con su porcentaje de avance, tanto en el perfil propio como al visitar el perfil de otro corredor. | **P1** | **RF-66** |
| **RF-68** | **Inscribirse y avanzar en retos deportivos** | Permitir al corredor unirse o abandonar retos globales o de club con una meta numérica y fecha límite, sumando automáticamente el avance de cada actividad realizada dentro del plazo. | **P1** | **RF-42** |
| **RF-69** | **Consultar clasificación de un reto** | Mostrar el detalle de cada reto activo o finalizado junto con la tabla de posiciones y progreso de todos los corredores inscritos. | **P1** | **RF-68** |

### Módulo 11 — Rankings y tablas de posiciones

Permite a los corredores medir su desempeño individual o local frente a la comunidad (votado en **5.º lugar** de la encuesta).

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-70** | **Consultar rankings en vivo por métrica y alcance** | Mostrar las tablas de posiciones semanales y mensuales ordenadas por zonas conquistadas, zonas dominadas, distancia o XP, filtrando por alcance global, ciudad, club o amigos e incluyendo siempre la fila del propio usuario. | **P0 / P1** | **RF-42**, **RF-53** |
| **RF-71** | **Consultar rankings históricos cerrados** | Guardar al cierre de cada semana y de cada mes una instantánea de las tablas de posiciones para permitir consultar los ganadores de periodos anteriores. | **P1** | **RF-70** |

### Módulo 12 — Clubes de corredores y territorio grupal

Atiende al **56,5 % de los encuestados** que indicaron preferir competir en club o combinando modo individual y grupal.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-72** | **Crear y editar un club de corredores** | Permitir fundar un club definiendo su nombre, ciudad, descripción, foto y privacidad (abierto o con aprobación requerida), quedando su creador como administrador. | **P1** | **RF-03** |
| **RF-73** | **Unirse y abandonar un club** | Permitir a un corredor integrarse directamente a un club abierto, enviar solicitud a uno privado o abandonar un club al que pertenece (conservando siempre al menos un administrador activo). | **P1** | **RF-72** |
| **RF-74** | **Administrar miembros y roles del club** | Permitir a los administradores de un club aprobar solicitudes de ingreso, promover miembros a administradores o expulsar integrantes. | **P1** | **RF-73** |
| **RF-75** | **Visualizar territorio dominado por el club** | Mostrar sobre el mapa todas las zonas dominadas por el conjunto de miembros activos del club y el resumen territorial colectivo de la agrupación. | **P1** | **RF-54**, **RF-73** |

### Módulo 13 — Interacción social entre corredores

Complementa la experiencia comunitaria con seguimiento entre amigos, aplausos deportivos (kudos) y comentarios en las carreras.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-76** | **Seguir y dejar de seguir a un corredor** | Permitir seguir de inmediato a corredores con cuenta pública, enviar solicitud de seguimiento a cuentas privadas y dejar de seguir en cualquier momento. | **P1** | **RF-13** |
| **RF-77** | **Aprobar o rechazar solicitudes de seguimiento** | Permitir al dueño de un perfil privado aceptar o rechazar las solicitudes pendientes de quienes desean ver sus actividades. | **P1** | **RF-29**, **RF-76** |
| **RF-78** | **Consultar listas de seguidores y seguidos** | Mostrar el listado paginado de las personas que siguen a un corredor y de aquellas a las que él sigue. | **P1** | **RF-76** |
| **RF-79** | **Dar y quitar kudos en una actividad** | Permitir otorgar o retirar un aplauso deportivo (kudos) en cualquier actividad visible y consultar la lista de corredores que lo dieron. | **P1** | **RF-20**, **RF-46** |
| **RF-80** | **Publicar y eliminar comentarios en una actividad** | Permitir escribir comentarios de apoyo en actividades visibles y eliminar comentarios propios (o cualquier comentario escrito dentro de una actividad propia). | **P1** | **RF-20**, **RF-46** |

### Módulo 14 — Equipamiento deportivo y exportación de rutas

Permite controlar la vida útil del calzado y compartir o analizar los recorridos fuera de la aplicación.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-81** | **Registrar y editar zapatillas** | Permitir dar de alta pares de zapatillas indicando marca, modelo, alias, kilometraje previo y límite de retiro (700 km por defecto), así como marcar un par como predeterminado. | **P1** | **RF-03** |
| **RF-82** | **Acumular kilometraje y retirar zapatillas** | Sumar automáticamente la distancia de cada actividad válida a la zapatilla asociada, alertar cuando alcance su límite de kilómetros y permitir marcarla como retirada. | **P1** | **RF-42**, **RF-81** |
| **RF-83** | **Exportar recorrido de actividad en GPX** | Generar y descargar el archivo estándar **.gpx** de una actividad propia con su secuencia de puntos y tiempos para usarlo en otros dispositivos o visores de rutas. | **P1** | **RF-46** |

### Módulo 15 — Integridad del juego (Antitrampas), moderación y configuración

Asegura que la competencia territorial sea justa (evitando trampas en vehículo) y que todos los parámetros del motor sean ajustables sin modificar el código.

| CÓDIGO | NOMBRE DEL RF | DESCRIPCIÓN | PRIORIDAD | DEPENDE DE |
|---|---|---|---|---|
| **RF-84** | **Descartar puntos GPS inexactos y saltos irreales** | Filtrar automáticamente en el servidor los puntos con precisión peor a 30 metros o saltos de más de 200 metros en menos de 5 segundos antes de calcular la distancia. | **P0** | **RF-41** |
| **RF-85** | **Excluir tramos realizados en vehículo** | Detectar los segmentos cuya velocidad supere el límite configurado (25 km/h) para que no sumen distancia ni otorguen progreso o dominio de zonas. | **P0** | **RF-84** |
| **RF-86** | **Poner en cuarentena actividades sospechosas** | Marcar en estado en revisión las actividades con ritmo promedio sobrehumano (< 2:30 min/km) o con más del 20 % de su distancia en tramos inválidos, excluyéndolas del mapa hasta que las revise un moderador. | **P1** | **RF-85** |
| **RF-87** | **Resolver actividades en revisión** | Permitir a un moderador inspeccionar las actividades en cuarentena y aprobarlas (aplicando su territorio) o rechazarlas definitivamente. | **P1** | **RF-86** |
| **RF-88** | **Resolver reportes de la comunidad y sancionar** | Permitir a un moderador atender los reportes abiertos, ocultar contenido infractor y suspender o reactivar cuentas de usuarios que incumplan las normas. | **P1** | **RF-36** |
| **RF-89** | **Registrar auditoría inalterable de seguridad** | Guardar automáticamente en una bitácora de solo inserción (**audit_log**) cada cambio de rol, suspensión, eliminación de cuenta, resolución de reporte o ajuste de parámetros. | **P1** | **RF-03** |
| **RF-90** | **Administrar parámetros dinámicos del motor** | Permitir consultar y actualizar en la tabla **system_setting** las reglas del juego (umbral 5 de 7, ventana de 30 días, mínimo de zonas por barrio, velocidades antitrampas y tabla de XP) sin valores fijos en el código. | **P0** | — |

## 3.2 Requisitos de interfaz externa

Esta sección explica cómo se comunica Terrun con el mundo exterior: las personas que tocan la pantalla, el teléfono físico y los servicios en internet.

![Diagrama de contexto de Terrun](diagramas/01-contexto.png)

### 1. Interfaces de usuario (Pantallas de la aplicación móvil)

- La aplicación cuenta con un diseño de **34 pantallas** organizadas en una barra inferior de 5 pestañas principales: **Mapa, Muro (Feed), Grabar, Clubes y Perfil**.
- **Idioma y claridad:** todos los textos, botones y mensajes de error que ve el corredor están en **español claro**, almacenados en un archivo de traducciones listo para agregar inglés en el futuro.
- **Accesibilidad:** los botones principales (especialmente iniciar, pausar y terminar carrera) tienen un tamaño mínimo táctil de 44×44 puntos y alto contraste visual (paleta verde esmeralda **#0B8A79** sobre fondos claros **#EDF1EE** o modo oscuro **#0D1512**) para poder leerse fácilmente bajo el sol mientras se corre.

### 2. Interfaces de hardware (Sensores del teléfono)

- **Receptor GPS y ubicación del sistema operativo:** la aplicación se comunica con el chip GPS de Android e iOS. Mientras se graba una carrera, mantiene activo un servicio en primer plano (mostrando un aviso fijo de "Grabando carrera" en la barra del teléfono) para que el sistema operativo no apague el GPS al bloquear la pantalla.
- **Almacenamiento interno y sonido:** utiliza la memoria interna del teléfono (mediante una base de datos local SQLite) para guardar los puntos del recorrido segundo a segundo, y el altavoz/auriculares para dar avisos de voz al completar cada kilómetro.

### 3. Interfaces de software (Servicios externos)

| SERVICIO EXTERNO | PARA QUÉ SE CONECTA CON TERRUN | CÓMO SE COMUNICAN |
|---|---|---|
| **Google y Apple (Identidad)** | Inicio de sesión social con un toque. | La aplicación obtiene un pase seguro (ID Token OIDC) y nuestro servidor verifica su firma oficial con Google o Apple. |
| **MapTiler / OpenStreetMap** | Mostrar el mapa de calles, avenidas y parques de la ciudad. | El componente **MapLibre** de la aplicación descarga fragmentos del mapa en formato vectorial ligero. |
| **Expo Push Service (FCM / APNs)** | Enviar notificaciones emergentes a teléfonos Android e iPhone. | Nuestro servidor envía los avisos en lote por HTTPS al servicio de Expo, que los entrega a Google y Apple. |
| **Brevo / Resend (Correo)** | Enviar correos de confirmación de cuenta y cambio de contraseña. | El servidor se conecta mediante una solicitud web segura (API HTTPS) usando una llave privada guardada fuera del código. |
| **Cloudflare R2 (Archivos)** | Guardar las fotos de perfil de los usuarios y los escudos de los clubes. | El servidor genera un permiso temporal (URL firmada) para subir y leer imágenes sin saturar la base de datos. |

### 4. Interfaces de comunicación y formatos de datos

- **Protocolo seguro:** toda la comunicación entre el teléfono y el servidor viaja cifrada por internet mediante **HTTPS (TLS)** en formato **JSON**.
- **Formatos geográficos:** las zonas y barrios que el servidor envía al mapa viajan en el formato estándar **GeoJSON**, y las carreras exportadas por el usuario se generan en formato **GPX 1.1**.
- **Unidades universales:** internamente, el teléfono y el servidor siempre hablan en metros, segundos y hora universal (UTC) para evitar errores de cálculo; la aplicación los convierte a kilómetros y minutos por kilómetro justo al mostrarlos en pantalla.

## 3.3 Características del sistema (Arquitectura a utilizar)

> **Nota arquitectónica:** En este proyecto, las "características del sistema" definen la **arquitectura tecnológica** elegida para construir Terrun, cómo se dividen sus componentes y cómo fluye la información desde que el corredor da un paso en la calle hasta que conquista una zona.

### 1. Estilo arquitectónico: Cliente-Servidor con Monolito Modular

El sistema está dividido en **tres grandes bloques** que trabajan en equipo:

**1.**  Aplicación móvil en el teléfono (React Native + Expo con TypeScript):

- Es la cara visible para el corredor.
- Está diseñada bajo el principio "primero sin internet" (offline-first) durante la carrera: mientras la persona corre, la aplicación no depende de tener señal; guarda cada coordenada en una base de datos local dentro del teléfono (SQLite) y, cuando termina la carrera y hay conexión, sube el paquete completo al servidor.

**2.**  Servidor central (API REST en .NET 10 — Monolito Modular):

- En lugar de dividir el servidor en docenas de microservicios pequeños (lo cual sería lento y complejo de mantener para un equipo universitario), utilizamos un **Monolito Modular**: es un solo programa fácil de desplegar, pero por dentro está ordenado en compartimentos separados por tema (Identidad, Perfil, Actividades, Territorio, Social, Clubes, Gamificación, Rankings, Notificaciones y Moderación).
- Dentro del servidor vive una pieza especial llamada **Motor de Territorio (`Terrun.Engine`)**: es una librería de cálculo puro que contiene toda la inteligencia matemática de los hexágonos H3, las reglas de conquista (5 de 7) y el detector de trampas, aislada para poder probarla miles de veces por segundo con total exactitud.

**3.**  Base de datos espacial (PostgreSQL 16 + PostGIS):

- Es la única fuente de la verdad donde se guardan las 41 tablas del sistema, incluyendo las rutas geográficas, las zonas hexagonales, los barrios y el puntaje de cada corredor.

![Arquitectura de componentes de Terrun](diagramas/02-componentes.png)

### 2. Los 6 principios de nuestra arquitectura

**1.  El servidor tiene la última palabra:** el teléfono muestra datos en vivo para guiar al corredor, pero la distancia oficial, la detección de trampas y quién conquista o roba una zona lo calcula **siempre el servidor** a partir de los puntos GPS originales. Así nadie puede alterar la aplicación para regalarse zonas.

**2.**  Envíos a prueba de fallos de red (Idempotencia): cada carrera recibe un código único universal (UUID) en el teléfono en el instante en que empieza. Si el internet falla al subirla y el teléfono la reenvía tres veces, el servidor reconoce el código y la procesa una sola vez.

**3.**  Respuesta rápida en dos pasos (Procesamiento asíncrono): cuando el corredor termina de correr y sube su ruta, el servidor la recibe en menos de un segundo, la guarda como "pendiente" y libera al teléfono. Inmediatamente, un trabajador interno del servidor limpia los puntos GPS, calcula los hexágonos H3 y actualiza los rankings en pocos segundos.

**4.  Reglas del juego configurables:** ningún número del juego (como el requisito de 5 sub-celdas, los 30 días de dominio o el límite de 25 km/h) está clavado en el código; todo se lee de la base de datos.

**5.  Privacidad desde el diseño:** las rutas nuevas solo son visibles para seguidores por defecto, y nunca se muestra dónde está corriendo una persona en tiempo real.

**6.**  Contrato único entre servidor y aplicación (OpenAPI): el servidor genera automáticamente un catálogo de todas sus operaciones (Swagger/OpenAPI) y la aplicación móvil importa ese catálogo para que ambos hablen exactamente el mismo idioma sin errores de escritura.

### 3. Resumen de tecnologías elegidas y por qué

| PARTE DEL SISTEMA | TECNOLOGÍA ELEGIDA | OTRAS OPCIONES EVALUADAS | POR QUÉ ELEGIMOS ESTA PARA TERRUN |
|---|---|---|---|
| **Aplicación móvil** | **React Native + Expo (TypeScript)** | Flutter; página web móvil (PWA) | Permite crear la versión de Android y de iPhone con un mismo código; tiene soporte real de GPS con pantalla bloqueada (algo que una web móvil no logra) y aprovecha el dominio de React que ya tienen Jorge y Rowlis. |
| **Mapa interactivo** | **MapLibre + OpenStreetMap / MapTiler** | Google Maps | Es gratuito, de código abierto y permite dibujar cientos de hexágonos de colores con fluidez sin cobrar por cada vez que un corredor abre el mapa. |
| **Grabación GPS y datos locales** | **expo-location + TaskManager + expo-sqlite** | Guardado simple en memoria | Mantiene viva la grabación con un aviso en el teléfono y guarda miles de puntos por carrera de forma segura aunque se apague el celular. |
| **Servidor (Backend)** | **.NET 10 Web API + EF Core 10** | Node.js (NestJS), Python (FastAPI) | Muy rápido procesando cálculos matemáticos y geográficos; Ashley ya tiene experiencia comprobada desplegándolo con seguridad y pruebas automáticas. |
| **Sistema de zonas** | **Uber H3 (`pocketken.H3` y `h3-js`)** | Cuadrícula de cuadrados propia | Los hexágonos tienen la misma distancia hacia todos sus vecinos y cada zona grande (resolución 9) se divide exactamente en 7 sub-celdas hijas (resolución 10). |
| **Base de datos** | **PostgreSQL 16 + PostGIS (en Neon)** | MongoDB, SQL Server | Es el estándar mundial para datos geográficos; permite buscar qué zonas hay en la pantalla del mapa en fracciones de segundo. |
| **Publicación y nube** | **Render + Cloudflare R2 + GitHub Actions + Expo EAS** | Servidores manuales | Publica automáticamente los cambios aprobados en el ambiente de pruebas (QA) y compila los instaladores para Google Play y App Store en la nube. |

### 4. Cómo fluye una carrera paso a paso

![Flujo completo al grabar una carrera y conquistar zonas](diagramas/03-secuencia-actividad.png)

**1.  En la calle:** el corredor presiona Iniciar; el teléfono activa el GPS y guarda cada punto del recorrido en su memoria interna (SQLite).

**2.  Al terminar:** el corredor presiona Finalizar; la aplicación empaqueta la ruta con su código único y la envía al servidor. El servidor confirma su recepción de inmediato.

**3.  Limpieza y antitrampas:** el procesador del servidor revisa la ruta, descarta puntos donde el GPS rebotó contra edificios y verifica que no se haya ido a velocidad de vehículo.

**4.  Cálculo de territorio (H3):** el motor convierte la ruta limpia en sub-celdas visitadas, marca cuántas de las 7 partes de cada zona se pisaron, otorga las **conquistas nuevas** (si llegó a 5 de 7) y recalcula quién es el **dueño de los últimos 30 días** y el **rey o reina del barrio**.

**5.  Recompensas y avisos:** se suman los puntos de experiencia (XP), se revisa si se cumplió algún logro o reto, y si alguien perdió su zona, se le envía una notificación.

**6.  Resumen en pantalla:** el teléfono muestra al corredor su mapa final con las zonas recién conquistadas iluminadas.

### 5. Ambientes de trabajo y publicación (Despliegue)

![Esquema de ambientes y despliegue](diagramas/04-despliegue.png)

Contamos con tres ambientes separados para que el trabajo diario nunca rompa la versión que ven los usuarios o el profesor:

- **Ambiente Local (en la computadora de cada desarrollador):** para programar y probar cambios nuevos sin afectar a nadie.
- **Ambiente de Pruebas (QA):** se actualiza solo cada vez que se aprueba e integra un cambio en GitHub. Es donde Rangelis y Jaime prueban la aplicación conectada a una base de datos de prueba en **Neon** y un servidor de prueba en **Render**.
- **Ambiente de Producción:** es la versión estable para la demostración final y para los corredores reales, distribuida a través de **Google Play** y **App Store (TestFlight)**.

## 3.4 Requisitos no funcionales

Los requisitos no funcionales definen **qué tan bien** debe comportarse el sistema en cuanto a precisión, rapidez, cuidado de la batería, privacidad y calidad:

| CÓDIGO | ASPECTO DE CALIDAD | QUÉ DEBE CUMPLIR TERRUN EN LA PRÁCTICA |
|---|---|---|
| **RNF-01** | **Precisión deportiva** | En una carrera al aire libre, la distancia medida por la aplicación no debe variar más de un **3 %** frente a la distancia real de una ruta medida. |
| **RNF-02** | **Grabación con pantalla apagada** | La aplicación debe seguir grabando el recorrido sin cortarse durante al menos **2 horas seguidas** con el teléfono bloqueado en el bolsillo. |
| **RNF-03** | **Cuidado de la batería** | Salir a correr durante una hora completa no debe gastar más del **10 % de la batería** en un teléfono de gama media en buen estado. |
| **RNF-04** | **Funcionamiento sin internet** | El corredor puede iniciar, pausar y terminar una carrera sin tener datos móviles ni Wi-Fi; si la aplicación se cierra por accidente, al reabrirla recupera la carrera sin perder los puntos ya grabados. |
| **RNF-05** | **Rapidez del servidor** | Las pantallas habituales deben responder en menos de **medio segundo** (500 ms en el 95 % de los casos), y procesar una carrera completa de 1 hora debe tardar menos de **10 segundos**. |
| **RNF-06** | **Fluidez del mapa** | Al mover el mapa por la ciudad, las zonas hexagonales visibles deben cargarse en menos de **1 segundo** (soportando hasta 2.000 hexágonos en vista) y moverse con suavidad. |
| **RNF-07** | **Seguridad de las cuentas** | Toda la información viaja cifrada por HTTPS, las contraseñas se guardan protegidas con cifrado irreversible y las sesiones usan pases de corta duración renovables. |
| **RNF-08** | **Privacidad y Ley 172-13** | Cumple con la **Ley 172-13 de Protección de Datos Personales de la República Dominicana** y las normas de Apple y Google: nunca muestra la ubicación en vivo de nadie, aplica zonas de privacidad alrededor del hogar y pide permiso claro antes de usar el GPS. |
| **RNF-09** | **Disponibilidad** | El servidor de producción debe estar disponible y en línea al menos el **99 % del tiempo** durante el periodo de pruebas y evaluación. |
| **RNF-10** | **Teléfonos compatibles** | Funciona tanto en teléfonos **Android (versión 10 en adelante)** como en **iPhone (iOS 16 en adelante)**. |
| **RNF-11** | **Facilidad de uso** | Cualquier corredor debe poder iniciar una carrera en un máximo de **2 toques** desde que abre la aplicación, con todos los textos en español claro. |
| **RNF-12** | **Calidad verificada** | Al menos el **80 %** de las reglas del motor de territorio deben estar cubiertas por pruebas automáticas que se ejecutan antes de aceptar cualquier cambio de código. |
| **RNF-13** | **Capacidad de crecimiento** | La arquitectura está diseñada para soportar hasta **10.000 corredores activos** sin necesidad de rediseñar el sistema. |
| **RNF-14** | **Reglas ajustables** | Ningún valor numérico del juego está escrito de forma rígida en el código; todos pueden ajustarse desde la tabla de configuración del servidor. |

# 4. Anexos

## Anexo A — Detalle de las reglas del juego, antitrampas y experiencia (XP)

![Cómo se divide una zona en 7 sub-celdas y cómo se conquista](diagramas/05-mecanica-h3.png)

### A.1 Resumen visual de la conquista y el dominio

| CONCEPTO | REGLA ACORDADA POR EL EQUIPO | QUÉ PASA SI DEJAS DE CORRER AHÍ |
|---|---|---|
| **Conquista personal de una zona** | Pisar al menos **5 de las 7 sub-celdas** que forman el hexágono, sumando todas tus carreras. | **Nunca se pierde:** queda guardada para siempre en tu mapa personal de exploración. |
| **Dominio competitivo de una zona** | Ser quien más sub-celdas haya recorrido dentro de esa zona en los **últimos 30 días** (mínimo 3 puntos). En caso de empate, conserva la zona el dueño actual. | **Se puede perder:** si otro corredor suma más puntos que tú te la roba; y si pasan 30 días sin que la defiendas, tus puntos vencen. |
| **Rey o reina del barrio** | Ser la persona que domina actualmente más zonas dentro de un mismo barrio (mínimo 3 zonas dominadas). | **Cambia dinámicamente:** si otro corredor te supera en cantidad de zonas dominadas dentro del barrio, hereda la corona y ambos reciben una notificación. |

### A.2 Reglas iniciales contra trampas

Para asegurar que nadie conquiste la ciudad subiéndose a un carro o una motocicleta, el servidor aplica automáticamente estos filtros a cada actividad:

| QUÉ REVISA EL SERVIDOR | LÍMITE INICIAL CONFIGURADO | QUÉ HACE EL SISTEMA CUANDO OCURRE |
|---|---|---|
| **Señal de GPS inexacta** | Margen de error mayor a **30 metros** | Descarta ese punto suelto para que la línea no salte sobre edificios. |
| **Tramo en vehículo** | Velocidad superior a **25 km/h** en un tramo | Ese tramo no suma distancia ni sirve para conquistar o dominar zonas. |
| **Ritmo sobrehumano** | Promedio más rápido que **2:30 min/km** sostenido | La carrera entera pasa a estado en revisión (cuarentena) y no afecta el mapa hasta que la revise un moderador. |
| **Paseo demasiado corto** | Duración menor a **3 minutos** | Se guarda en el historial personal, pero no otorga puntos de territorio. |
| **Salto irreal ("teletransporte")** | Saltar más de **200 metros en menos de 5 segundos** | Se ignora el salto por tratarse de un fallo del GPS del teléfono. |
| **Dos carreras al mismo tiempo** | Horas de inicio y fin que se pisan entre sí | El servidor rechaza la segunda carrera para evitar trampas con dos teléfonos. |

### A.3 Tabla inicial de puntos de experiencia (XP)

| ACCIÓN LOGRADA POR EL CORREDOR | PUNTOS DE EXPERIENCIA (XP) QUE RECIBE |
|---|---|
| Por cada **kilómetro válido** recorrido | **+10 XP** |
| Por **conquistar una zona** por primera vez (5 de 7 sub-celdas) | **+50 XP** |
| Por **convertirse en dueño** de una zona libre | **+30 XP** |
| Por **robarle el dominio** de una zona a otro corredor | **+40 XP** |
| Por desbloquear una **medalla / logro** o completar un **reto** | Según la dificultad del logro o reto (entre **+50 y +500 XP**) |

## Anexo B — Estrategia de calidad, pruebas y seguridad

### B.1 Cómo probamos que todo funcione antes de entregar

| TIPO DE PRUEBA | QUÉ VERIFICAMOS EXACTAMENTE | CON QUÉ HERRAMIENTA | RESPONSABLE |
|---|---|---|---|
| **Pruebas del motor (Unitarias)** | Que las fórmulas de distancia, conquista 5/7, empates de dominio, barrios y antitrampas den el mismo resultado que nuestros cálculos hechos a mano. | xUnit (.NET) | Ashley (con verificación manual de Rangelis) |
| **Pruebas de servidor y base de datos** | Que el registro, el inicio de sesión, la subida de carreras y los permisos de privacidad funcionen contra una base de datos PostgreSQL + PostGIS real. | xUnit + Testcontainers | Ashley y Rowlis |
| **Pruebas de rutas de la API** | Que cada operación del servidor responda bien tanto cuando los datos son correctos como cuando alguien envía datos inválidos. | Bruno / Postman | Rangelis |
| **Pruebas de la aplicación móvil** | Que el grabador de carreras, la cola de envíos sin internet y los formatos de pantalla no fallen. | Jest + Testing Library | Jorge y Rowlis |
| **Pruebas de campo en la calle** | Salir a correr de verdad con teléfonos Android y iPhone en rutas medidas de 1 km y 5 km, probando pantalla apagada, gasto de batería y modo avión. | Teléfonos reales en calle | Jaime (junto al grupo de corredores) |
| **Pruebas de regresión y aceptación** | Recorrer los flujos completos antes de cada viernes y verificar que cada tarea cumpla todo lo pedido por la Product Owner. | Maestro (E2E) y revisión funcional | Rangelis, Jaime y Diana |

### B.2 Cuándo damos una tarea por terminada (Definition of Done)

En nuestro equipo, ninguna tarea se marca como "terminada" hasta que cumple esta lista:

**1.**  Su código fue revisado y aprobado por otro compañero antes de integrarse a la rama principal (**main**).

**2.**  Las pruebas automáticas pasan en verde.

**3.**  Si toca la aplicación móvil, se probó tanto en Android como en iOS.

**4.**  No tiene valores del juego fijos en el código ni contraseñas expuestas.

**5.**  Otra persona distinta de quien la programó (QA o la Product Owner) verificó que cumple todos sus criterios de aceptación.

**6.**  Quedó registrada en el archivo de estado (**ESTADO.md**) y en la bitácora del equipo (**BITACORA.md**).

## Anexo C — Plan de entregas (Sprints), responsabilidades y riesgos

### C.1 Calendario de Sprints hasta la entrega final

Trabajamos en ciclos semanales de viernes a viernes. El desarrollo de funciones nuevas se **congela el miércoles 6 de enero de 2027** para dedicar dos semanas exclusivas a pruebas exhaustivas y pulido antes de la presentación final del **miércoles 20 de enero de 2027**.

| SPRINT | FECHAS | META PRINCIPAL DE LA SEMANA | REUNIÓN DE REVISIÓN (WEEKLY) |
|---|---|---|---|
| **S0** | 10 al 13 de noviembre | Entrega de documentos HLD y LLD; repositorio configurado; esqueleto del proyecto y encuesta a corredores en marcha. | Viernes 13 de noviembre |
| **S1** | 14 al 20 de noviembre | Registro, inicio de sesión, edición de perfil, servidor de pruebas (QA) en línea y **prueba real de GPS con pantalla bloqueada** en Android e iPhone. | Viernes 20 de noviembre |
| **S2** | 21 al 27 de noviembre | Grabador de carreras completo, guardado sin internet, subida al servidor, cálculo de ritmo/kilómetros e historial de actividades. | Viernes 27 de noviembre |
| **S3** | 28 nov al 4 de diciembre | **Motor de territorio en acción:** sub-celdas H3, conquista de zonas (5 de 7), mapa coloreado por zonas y primer ranking. | Viernes 4 de diciembre |
| **S4** | 5 al 11 de diciembre | **Hito P0 completo en producción:** dominio competitivo de 30 días, robo de zonas, puntos XP/niveles e inicio con Google. | Viernes 11 de diciembre |
| **S5** | 12 al 18 de diciembre | Red social (seguir amigos, muro, kudos, comentarios), notificaciones emergentes al teléfono y primera versión en tiendas de prueba. | Viernes 18 de diciembre |
| **S6** | 19 al 23 de diciembre | Clubes de corredores, medallas/logros, rachas, retos y **barrios con rey o reina del barrio**. | Miércoles 23 de diciembre (por Navidad) |
| **S7** | 26 al 30 de diciembre | Récords personales, zapatillas, zonas de privacidad, inicio con Apple, exportar GPX y herramientas de moderación. | Miércoles 30 de diciembre (por Año Nuevo) |
| **S8** | 31 dic al 6 de enero | Cierre de detalles pendientes, **congelamiento de código (6 de enero)** y apertura de prueba cerrada con 12+ corredores en Google Play. | — |
| **QA 1 y 2** | 7 al 19 de enero | Dos semanas completas de pruebas de campo, pruebas de regresión, corrección de defectos y ensayo general de la demostración. | Viernes 8 y 15 de enero |
| **Entrega** | **Miércoles 20 de enero** | **Presentación y demostración final del proyecto.** | — |

### C.2 Matriz de responsabilidades del equipo (RACI)

(R = Quien lo ejecuta · A = Quien responde finalmente · C = A quien se consulta · I = A quien se informa)

| ACTIVIDAD O DECISIÓN CLAVE | RESPONSABLE DE EJECUTARLO (R) | QUIEN APRUEBA / RESPONDE (A) | CONSULTADO (C) | INFORMADO (I) |
|---|---|---|---|---|
| Arquitectura y contrato del servidor | Ashley | Ashley | Jorge, Rowlis | Todo el equipo |
| Orden de prioridades del backlog | Diana | Diana | Ashley | Todo el equipo |
| Reglas y parámetros del juego | Ashley | Diana | Equipo y corredores | Todo el equipo |
| Diseño visual de pantallas móviles | Jorge, Rowlis | Diana | Ashley | Todo el equipo |
| Validación de calidad de cada sprint | Rangelis, Jaime | Diana | Ashley | Todo el equipo |
| Publicación en tiendas (Play Store / App Store) | Ashley, Jorge | Diana | Jaime | Todo el equipo |

### C.3 Principales riesgos y cómo los prevenimos

| RIESGO IDENTIFICADO | QUÉ TAN PROBABLE ES | IMPACTO | NUESTRO PLAN PARA EVITARLO O RESOLVERLO |
|---|---|---|---|
| **R-01:** Que algunos teléfonos (como Xiaomi o Samsung) apaguen el GPS al bloquear la pantalla para ahorrar batería. | Media | Alto | Hacemos una prueba real de 30 minutos en el **Sprint 1** antes de construir el resto; usamos un servicio en primer plano con notificación visible y damos una guía en pantalla al usuario para excluir la app del ahorro agresivo de batería. |
| **R-02:** Que haya demasiadas tareas de servidor concentradas en una sola persona (Ashley). | Alta | Alto | Rowlis apoya el servidor con funciones puntuales asignadas por Ashley (D-16), y construimos primero todo lo imprescindible (**P0** hasta el 11 de diciembre); si para esa fecha algo se retrasa, las funciones secundarias (**P1**) de menor peso en la encuesta se mueven a **P2** sin afectar la entrega principal. |
| **R-03:** Que las fiestas de Navidad y Año Nuevo reduzcan las horas disponibles del equipo. | Alta | Medio | Los sprints 6 y 7 (finales de diciembre) ya están planificados con menos carga y con reuniones adelantadas al miércoles. |
| **R-04:** Que alguien intente ganar zonas subiéndose a un carro o bicicleta. | Media | Medio | El servidor filtra tramos de más de 25 km/h y pone en revisión ritmos imposibles antes de tocar el mapa de zonas. |
| **R-05:** Que Google Play pida 14 días de prueba con 12 probadores antes de liberar la aplicación al público. | Alta | Medio | Desde diciembre reunimos la lista de 12+ corredores con correo de Google y abrimos la prueba cerrada el 6 de enero, coincidiendo con nuestras dos semanas de QA. |
