"""Genera los diagramas PNG del HLD/LLD en docs/diagramas/.

Uso:  uv run --native-tls --with matplotlib python scripts/generar_diagramas.py
"""
import math
from pathlib import Path

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch, RegularPolygon

OUT = Path(__file__).resolve().parent.parent / "docs" / "diagramas"
OUT.mkdir(parents=True, exist_ok=True)

AZUL = "#1F4E79"
AZUL_CLARO = "#DCE9F5"
VERDE = "#2E7D32"
VERDE_CLARO = "#E3F1E4"
NARANJA = "#E65100"
NARANJA_CLARO = "#FDEBDD"
GRIS = "#555555"
GRIS_CLARO = "#EFEFEF"
plt.rcParams["font.family"] = "DejaVu Sans"


def caja(ax, x, y, w, h, titulo, sub="", fc=AZUL_CLARO, ec=AZUL, fs=10, subfs=8):
    ax.add_patch(FancyBboxPatch((x, y), w, h, boxstyle="round,pad=0.02,rounding_size=0.08",
                                fc=fc, ec=ec, lw=1.6))
    if sub:
        ax.text(x + w / 2, y + h * 0.68, titulo, ha="center", va="center", fontsize=fs,
                weight="bold", color=ec)
        ax.text(x + w / 2, y + h * 0.32, sub, ha="center", va="center", fontsize=subfs,
                color="#222222", linespacing=1.3)
    else:
        ax.text(x + w / 2, y + h / 2, titulo, ha="center", va="center", fontsize=fs,
                weight="bold", color=ec)


def flecha(ax, x1, y1, x2, y2, texto="", color=GRIS, off=(0, 0.12), fs=7.5, doble=False):
    ax.annotate("", xy=(x2, y2), xytext=(x1, y1),
                arrowprops=dict(arrowstyle="<|-|>" if doble else "-|>", color=color, lw=1.3))
    if texto:
        ax.text((x1 + x2) / 2 + off[0], (y1 + y2) / 2 + off[1], texto, ha="center", va="center",
                fontsize=fs, color=color, bbox=dict(fc="white", ec="none", pad=1))


def lienzo(w, h, titulo):
    fig, ax = plt.subplots(figsize=(w, h), dpi=200)
    ax.set_xlim(0, w)
    ax.set_ylim(0, h)
    ax.axis("off")
    ax.text(w / 2, h - 0.3, titulo, ha="center", va="center", fontsize=13, weight="bold",
            color=AZUL)
    return fig, ax


def guardar(fig, nombre):
    fig.savefig(OUT / nombre, bbox_inches="tight", facecolor="white")
    plt.close(fig)
    print("ok", nombre)


# 1. Contexto -----------------------------------------------------------------
def contexto():
    fig, ax = lienzo(12, 7, "Diagrama de contexto — Terrun")
    caja(ax, 0.3, 2.9, 2.2, 1.2, "Corredor", "Usuario final\n(Android / iOS)", GRIS_CLARO, GRIS)
    caja(ax, 0.3, 0.8, 2.2, 1.2, "Moderador /\nAdministrador", "Revisa reportes y\nactividades sospechosas", GRIS_CLARO, GRIS, fs=9)
    caja(ax, 4.2, 2.6, 3.4, 1.8, "Terrun",
         "App móvil + API REST + BD\nGrabación GPS, territorio H3,\nsocial, gamificación", AZUL_CLARO, AZUL, fs=12)
    externos = [
        (9.0, 5.3, "Google / Apple", "Inicio de sesión (OIDC)"),
        (9.0, 3.95, "Expo Push", "→ FCM (Android) / APNs (iOS)"),
        (9.0, 2.6, "Mapas OSM / MapTiler", "Teselas del mapa base"),
        (9.0, 1.25, "Correo (Brevo/Resend)", "Verificación y recuperación"),
    ]
    for x, y, t, s in externos:
        caja(ax, x, y, 2.7, 1.0, t, s, NARANJA_CLARO, NARANJA, fs=9.5)
    caja(ax, 4.6, 0.4, 2.6, 1.0, "App Store / Google Play", "Distribución de la app", NARANJA_CLARO, NARANJA, fs=9)
    flecha(ax, 2.5, 3.5, 4.2, 3.5, "corre, conquista,\nsigue a otros", off=(0, 0.35))
    flecha(ax, 2.5, 1.4, 4.2, 2.8, "modera", off=(-0.2, 0.15))
    flecha(ax, 7.6, 4.2, 9.0, 5.8, "valida ID token", off=(-0.35, 0.2))
    flecha(ax, 7.6, 3.8, 9.0, 4.45, "envía push", off=(0, 0.15))
    flecha(ax, 7.6, 3.2, 9.0, 3.1, "pide teselas", off=(0, 0.15))
    flecha(ax, 7.6, 2.8, 9.0, 1.75, "envía correos", off=(-0.2, -0.05))
    flecha(ax, 5.9, 1.4, 5.9, 2.6, "instala", off=(0.35, 0))
    guardar(fig, "01-contexto.png")


# 2. Contenedores / componentes ---------------------------------------------
def componentes():
    fig, ax = lienzo(13, 8.2, "Arquitectura de componentes")
    # App
    ax.add_patch(FancyBboxPatch((0.3, 0.5), 4.0, 6.9, boxstyle="round,pad=0.02", fc="#F7FAFD", ec=AZUL, lw=1.2, ls="--"))
    ax.text(2.3, 7.15, "App móvil — React Native + Expo (TS)", ha="center", fontsize=9, weight="bold", color=AZUL)
    app = [
        ("Pantallas (Expo Router)", "Mapa, Grabar, Feed, Perfil,\nClubes, Rankings, Ajustes"),
        ("Grabador GPS", "expo-location + TaskManager\nservicio en primer plano"),
        ("Almacén local", "expo-sqlite: actividades\npendientes de subir"),
        ("Capa de datos", "TanStack Query + cliente API\nZustand (estado de sesión)"),
        ("Mapa", "MapLibre RN + capa de\nhexágonos (GeoJSON)"),
    ]
    for i, (t, s) in enumerate(app):
        caja(ax, 0.6, 6.0 - i * 1.25, 3.4, 1.0, t, s, AZUL_CLARO, AZUL, fs=9, subfs=7.5)
    # API
    ax.add_patch(FancyBboxPatch((5.2, 0.5), 4.6, 6.9, boxstyle="round,pad=0.02", fc="#F6FBF6", ec=VERDE, lw=1.2, ls="--"))
    ax.text(7.5, 7.15, "API REST — .NET 10 (monolito modular)", ha="center", fontsize=9, weight="bold", color=VERDE)
    mods = ["Identidad", "Perfil", "Actividades", "Territorio", "Social", "Clubes",
            "Gamificación", "Rankings", "Notificaciones", "Moderación"]
    for i, m in enumerate(mods):
        col, fila = i % 2, i // 2
        caja(ax, 5.45 + col * 2.2, 6.15 - fila * 0.72, 2.0, 0.58, m, "", VERDE_CLARO, VERDE, fs=8.5)
    caja(ax, 5.45, 1.55, 4.2, 0.85, "Motor de Territorio (librería pura)",
         "H3, conquista 5/7, dominio 30 días, antitrampas", "#FFFFFF", VERDE, fs=8.5, subfs=7.5)
    caja(ax, 5.45, 0.65, 4.2, 0.75, "Trabajos en segundo plano",
         "procesar actividad · dominio nocturno · push · rankings", "#FFFFFF", VERDE, fs=8.5, subfs=7.2)
    # BD
    caja(ax, 10.6, 3.6, 2.2, 1.8, "PostgreSQL 16\n+ PostGIS", "Neon (gestionado)\n41 tablas", NARANJA_CLARO, NARANJA, fs=9.5)
    caja(ax, 10.6, 1.4, 2.2, 1.3, "Almacén de\nobjetos (R2)", "fotos de perfil\ny de club", NARANJA_CLARO, NARANJA, fs=9)
    flecha(ax, 4.3, 3.9, 5.2, 3.9, "HTTPS\nJSON + JWT", off=(0, 0.4), doble=True)
    flecha(ax, 9.8, 4.5, 10.6, 4.5, "EF Core\nNpgsql", off=(0, 0.4), doble=True)
    flecha(ax, 9.8, 2.0, 10.6, 2.0, "S3 API", off=(0, 0.25))
    guardar(fig, "02-componentes.png")


# 3. Secuencia: registrar actividad -----------------------------------------
def secuencia():
    fig, ax = lienzo(13, 9, "Secuencia — grabar una carrera y conquistar zonas")
    actores = ["Corredor", "App móvil", "API", "Procesador\n(2º plano)", "Motor de\nTerritorio", "PostgreSQL"]
    xs = [1.0, 3.2, 5.5, 7.8, 10.0, 12.0]
    for x, a in zip(xs, actores):
        caja(ax, x - 0.8, 7.6, 1.6, 0.75, a, "", AZUL_CLARO, AZUL, fs=8.5)
        ax.plot([x, x], [0.3, 7.6], color="#BBBBBB", lw=1, ls="--")
    pasos = [
        (0, 1, "Iniciar carrera"),
        (1, 1, "Guarda puntos GPS en SQLite (cada 1–3 s)"),
        (0, 1, "Finalizar"),
        (1, 2, "POST /activities {clientId, points}"),
        (2, 5, "INSERT activity (status = pending)"),
        (2, 1, "202 Accepted (idempotente por clientId)"),
        (3, 5, "Toma la actividad pending (SKIP LOCKED)"),
        (3, 4, "Filtrar precisión/velocidad, métricas, splits"),
        (4, 4, "Densificar ruta → sub-celdas H3 res 10"),
        (4, 3, "Visitas, progreso 5/7, conquistas, puntos 30 d"),
        (3, 5, "Guarda visitas, conquistas, dominio, XP, logros"),
        (3, 5, "notification zone_stolen (outbox)"),
        (1, 2, "GET /activities/{id} (sondeo)"),
        (2, 1, "procesada: km, ritmo, zonas nuevas"),
        (1, 0, "Resumen + mapa con zonas conquistadas"),
    ]
    y = 7.15
    for i, (a, b, t) in enumerate(pasos, 1):
        if a == b:
            ax.annotate("", xy=(xs[a] + 0.05, y - 0.22), xytext=(xs[a] + 0.05, y),
                        arrowprops=dict(arrowstyle="-|>", color=VERDE, lw=1.2,
                                        connectionstyle="arc3,rad=-1.2"))
            ax.text(xs[a] + 0.45, y - 0.1, f"{i}. {t}", fontsize=7.5, va="center", color="#222222",
                    bbox=dict(fc="white", ec="none", pad=0.5))
            y -= 0.15
        else:
            flecha(ax, xs[a], y, xs[b], y, "", color=AZUL if a < b else GRIS)
            ax.text((xs[a] + xs[b]) / 2, y + 0.13, f"{i}. {t}", fontsize=7.5, ha="center",
                    color="#222222", bbox=dict(fc="white", ec="none", pad=0.5))
        y -= 0.45
    guardar(fig, "03-secuencia-actividad.png")


# 4. Despliegue ---------------------------------------------------------------
def despliegue():
    fig, ax = lienzo(12, 6.5, "Diagrama de despliegue")
    caja(ax, 0.3, 3.2, 2.6, 1.6, "Teléfono", "App RN (build EAS)\nSQLite local", GRIS_CLARO, GRIS)
    caja(ax, 0.3, 0.8, 2.6, 1.6, "GitHub", "Repo + Actions (CI)\nimagen Docker", GRIS_CLARO, GRIS)
    caja(ax, 4.0, 3.0, 3.6, 2.0, "Render — Web Service", "Contenedor Docker API .NET 10\nTrabajos en 2º plano (HostedService)\nambientes: qa y prod", VERDE_CLARO, VERDE)
    caja(ax, 8.6, 3.6, 3.0, 1.6, "Neon PostgreSQL", "PostGIS · ramas qa/prod\nbackups automáticos", NARANJA_CLARO, NARANJA)
    caja(ax, 8.6, 1.4, 3.0, 1.4, "Cloudflare R2", "fotos (URL firmada)", NARANJA_CLARO, NARANJA)
    caja(ax, 4.0, 0.6, 3.6, 1.6, "Expo EAS", "Build Android/iOS\nSubmit → Play / App Store", AZUL_CLARO, AZUL)
    flecha(ax, 2.9, 4.0, 4.0, 4.0, "HTTPS", off=(0, 0.18), doble=True)
    flecha(ax, 7.6, 4.3, 8.6, 4.3, "TLS 5432", off=(0, 0.18))
    flecha(ax, 7.6, 3.4, 8.6, 2.2, "S3", off=(0.1, 0.1))
    flecha(ax, 2.9, 1.8, 4.0, 3.2, "deploy\nautomático", off=(-0.25, 0.25))
    flecha(ax, 2.9, 1.2, 4.0, 1.2, "", off=(0, 0.15))
    flecha(ax, 4.0, 1.6, 2.9, 3.4, "instala", off=(0.35, -0.25))
    guardar(fig, "04-despliegue.png")


# 5. Mecánica H3 --------------------------------------------------------------
def mecanica():
    fig, ax = plt.subplots(figsize=(11, 5.2), dpi=200)
    ax.set_aspect("equal")
    ax.axis("off")
    ax.set_xlim(-0.5, 21)
    ax.set_ylim(-4.6, 5)
    ax.text(10.2, 4.5, "Mecánica de conquista: zona H3 res 9 = 7 sub-celdas res 10",
            ha="center", fontsize=13, weight="bold", color=AZUL)
    r = 1.0
    centros = [(0, 0)] + [(math.sqrt(3) * r * math.cos(math.radians(30 + 60 * k)),
                           math.sqrt(3) * r * math.sin(math.radians(30 + 60 * k))) for k in range(6)]

    def zona(cx, visitadas, titulo, estado, color_estado):
        ax.add_patch(RegularPolygon((cx, 0), 6, radius=2.75 * r, orientation=0,
                                    fc="none", ec=AZUL, lw=2))
        for i, (x, y) in enumerate(centros):
            fc = VERDE if i in visitadas else "#F3F3F3"
            ax.add_patch(RegularPolygon((cx + x, y), 6, radius=r * 0.97, orientation=math.radians(30),
                                        fc=fc, ec="#888888", lw=1, alpha=0.85))
        ax.text(cx, -3.4, titulo, ha="center", fontsize=10, weight="bold")
        ax.text(cx, -4.15, estado, ha="center", fontsize=9.5, color=color_estado, weight="bold")

    zona(3, {0, 1, 4}, "Carrera 1: 3 de 7", "En progreso (3/7)", NARANJA)
    zona(10.2, {0, 1, 2, 4}, "Carrera 2 suma 1 sub-celda", "En progreso (4/7)", NARANJA)
    zona(17.4, {0, 1, 2, 4, 5, 6}, "Carrera 3 suma 2", "¡CONQUISTADA! (6/7 ≥ 5)", VERDE)
    ax.text(10.2, 3.5, "Las sub-celdas visitadas se acumulan entre carreras. Umbral configurable (system_setting).",
            ha="center", fontsize=9, color=GRIS)
    guardar(fig, "05-mecanica-h3.png")


# 6. ER núcleo -----------------------------------------------------------------
def er():
    fig, ax = lienzo(15.8, 10, "Modelo entidad-relación — núcleo (detalle completo en el diccionario de datos)")
    T = {
        "app_user": (5.6, 7.6, ["id PK", "email UK", "role", "status"], AZUL),
        "profile": (2.6, 8.6, ["user_id PK/FK", "display_name", "city"], AZUL),
        "user_preference": (2.6, 7.0, ["user_id PK/FK", "units", "default_visibility"], AZUL),
        "user_stats": (8.8, 8.6, ["user_id PK/FK", "total_xp, level", "current_streak_weeks"], AZUL),
        "activity": (5.6, 4.8, ["id PK", "user_id FK", "client_id UK", "status, distance_m", "route geography"], VERDE),
        "gps_point": (2.4, 4.8, ["activity_id FK", "seq, lat, lon", "accuracy_m, recorded_at"], VERDE),
        "split": (2.4, 3.2, ["activity_id FK", "number, duration_s"], VERDE),
        "personal_record": (2.4, 1.6, ["user_id FK", "type", "activity_id FK"], VERDE),
        "shoe": (8.8, 6.6, ["id PK", "user_id FK", "total_distance_m"], VERDE),
        "zone": (12.4, 5.0, ["h3 PK", "polygon", "city"], NARANJA),
        "cell_visit": (8.8, 3.4, ["user_id, activity_id", "cell_h3, zone_h3", "visit_date"], NARANJA),
        "zone_progress": (14.3, 2.6, ["user_id, zone_h3 PK", "cell_mask (7 bits)", "last_visit_at"], NARANJA),
        "conquest": (8.8, 1.4, ["id PK", "user_id, zone_h3", "activity_id"], NARANJA),
        "zone_ownership": (12.0, 1.0, ["zone_h3 PK", "owner_id FK", "owner_points, owned_since"], NARANJA),
        "xp_transaction": (12.4, 7.4, ["id PK", "user_id FK", "reason, amount"], AZUL),
    }
    claros = {AZUL: AZUL_CLARO, VERDE: VERDE_CLARO, NARANJA: NARANJA_CLARO}
    w = 2.3
    pos = {}
    for nombre, (x, y, cols, c) in T.items():
        h = 0.38 + 0.27 * len(cols)
        ax.add_patch(FancyBboxPatch((x - w / 2, y - h / 2), w, h, boxstyle="round,pad=0.01", fc=claros[c], ec=c, lw=1.4))
        ax.text(x, y + h / 2 - 0.2, nombre, ha="center", fontsize=8.5, weight="bold", color=c)
        for i, col in enumerate(cols):
            ax.text(x - w / 2 + 0.1, y + h / 2 - 0.47 - i * 0.27, col, fontsize=7, color="#222222")
        pos[nombre] = (x, y)
    rel = [("app_user", "profile", "1:1"), ("app_user", "user_preference", "1:1"),
           ("app_user", "user_stats", "1:1"), ("app_user", "activity", "1:N"),
           ("app_user", "shoe", "1:N"), ("activity", "gps_point", "1:N"),
           ("activity", "split", "1:N"), ("activity", "personal_record", "1:N"),
           ("activity", "cell_visit", "1:N"), ("zone", "cell_visit", "1:N"),
           ("zone", "zone_progress", "1:N"), ("activity", "conquest", "1:N"),
           ("zone", "zone_ownership", "1:1"), ("app_user", "xp_transaction", "1:N"),
           ("shoe", "activity", "1:N")]
    for a, b, card in rel:
        (x1, y1), (x2, y2) = pos[a], pos[b]
        ax.plot([x1, x2], [y1, y2], color="#9A9A9A", lw=0.9, zorder=0)
        ax.text((x1 + x2) / 2, (y1 + y2) / 2, card, fontsize=6.5, color=GRIS,
                bbox=dict(fc="white", ec="none", pad=0.3))
    ax.text(0.3, 0.3, "Azul: identidad y gamificación · Verde: actividades · Naranja: territorio",
            fontsize=8, color=GRIS)
    guardar(fig, "06-er-nucleo.png")


if __name__ == "__main__":
    contexto()
    componentes()
    secuencia()
    despliegue()
    mecanica()
    er()
