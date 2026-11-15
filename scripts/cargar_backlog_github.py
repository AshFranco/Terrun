"""Script de sincronización automática (creación + actualización rápida) entre
docs/03-BACKLOG.md y los Issues de GitHub (AshFranco/Terrun) para CTZ-06.

Garantía de cero pérdida de tiempo:
- Si una historia CTZ-XX no existe en GitHub, la crea con sus etiquetas y criterios.
- Si ya existe y cambiaste en docs/03-BACKLOG.md el sprint, responsable, prioridad,
  título o criterios de aceptación, actualiza automáticamente el Issue en segundos
  (sin tocar los que no cambiaron).

Uso:
  python scripts/cargar_backlog_github.py --dry-run   # Muestra qué crearía o actualizaría
  python scripts/cargar_backlog_github.py             # Sincroniza docs/03-BACKLOG.md -> GitHub
"""
import json
import os
import re
import subprocess
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

REPO_OWNER = "AshFranco"
REPO_NAME = "Terrun"
REPO_ROOT = Path(__file__).resolve().parent.parent
BACKLOG_MD = REPO_ROOT / "docs" / "03-BACKLOG.md"

HECHAS_S0 = {"CTZ-01", "CTZ-02", "CTZ-05", "CTZ-07"}


def parsear_backlog():
    lineas = BACKLOG_MD.read_text(encoding="utf-8").splitlines()
    sprint_actual = None
    historias = []

    for l in lineas:
        if m_sec := re.match(r"^##\s+(S\d+|QA 1 y QA 2|P2)\b", l):
            raw = m_sec.group(1)
            sprint_actual = "QA" if raw == "QA 1 y QA 2" else raw
        elif l.startswith("|") and sprint_actual:
            partes = [c.strip() for c in l.strip().split("|")[1:-1]]
            if len(partes) == 6 and re.match(r"^CTZ-\d+[a-z]?$", partes[0]):
                cid, titulo, resp, prio, dep, criterios = partes
                titulo_limpio = re.sub(r"\*\*(.*?)\*\*", r"\1", titulo)
                titulo_limpio = re.sub(r"\*(.*?)\*", r"\1", titulo_limpio)
                historias.append({
                    "id": cid,
                    "sprint": sprint_actual,
                    "titulo": titulo_limpio,
                    "titulo_md": titulo,
                    "responsable": resp,
                    "prioridad": prio,
                    "depende_de": dep,
                    "criterios": criterios,
                })
    return historias


def obtener_token_git():
    if tok := os.environ.get("GITHUB_TOKEN"):
        return tok.strip()
    proc = subprocess.run(
        ["git", "credential", "fill"],
        input="protocol=https\nhost=github.com\npath=AshFranco/Terrun.git\n\n",
        text=True,
        capture_output=True,
        check=True,
    )
    for line in proc.stdout.splitlines():
        if line.startswith("password="):
            return line.split("=", 1)[1].strip()
    raise RuntimeError("No se encontró token de GitHub en git credential fill ni en GITHUB_TOKEN.")


def gh_api(method, path, token, payload=None):
    url = f"https://api.github.com/repos/{REPO_OWNER}/{REPO_NAME}{path}"
    data = json.dumps(payload).encode("utf-8") if payload is not None else None
    req = urllib.request.Request(
        url,
        data=data,
        method=method,
        headers={
            "Authorization": f"Bearer {token}",
            "Accept": "application/vnd.github+json",
            "Content-Type": "application/json",
            "X-GitHub-Api-Version": "2022-11-28",
            "User-Agent": "Terrun-Backlog-Sync",
        },
    )
    with urllib.request.urlopen(req) as resp:
        return json.loads(resp.read().decode("utf-8"))


def asegurar_etiquetas(token):
    etiquetas = [
        ("historia", "1A2723", "Historia del backlog de Terrun"),
        ("P0", "FF6B4E", "Imprescindible — Núcleo del MVP"),
        ("P1", "2FC2AC", "Importante — Complemento competitivo y social"),
        ("P2", "6E857B", "Posterior al MVP — Evolución y fase comercial"),
    ]
    for s in ["S0", "S1", "S2", "S3", "S4", "S5", "S6", "S7", "S8", "QA", "P2"]:
        etiquetas.append((f"sprint:{s}", "0D1512", f"Etapa o sprint {s}"))
    for r in ["Ashley", "Diana", "Jorge", "Rowlis", "Rangelis", "Jaime"]:
        etiquetas.append((f"resp:{r}", "1E7A6C", f"Responsable: {r}"))

    existentes = {lbl["name"] for lbl in gh_api("GET", "/labels?per_page=100", token)}
    for nombre, color, desc in etiquetas:
        if nombre not in existentes:
            try:
                gh_api("POST", "/labels", token, {"name": nombre, "color": color, "description": desc})
                print(f"  + Etiqueta creada: {nombre}")
            except urllib.error.HTTPError as e:
                if e.code != 422:
                    raise


def obtener_issues_existentes(token):
    mapa = {}
    pagina = 1
    while True:
        items = gh_api("GET", f"/issues?state=all&per_page=100&page={pagina}", token)
        if not items:
            break
        for it in items:
            if "pull_request" in it:
                continue
            if m := re.match(r"^(CTZ-\d+[a-z]?)\b", it["title"]):
                mapa[m.group(1)] = {
                    "number": it["number"],
                    "title": it["title"],
                    "body": (it.get("body") or "").strip(),
                    "labels": sorted(l["name"] for l in it.get("labels", [])),
                    "state": it["state"],
                }
        if len(items) < 100:
            break
        pagina += 1
    return mapa


def etiquetas_de_historia(h):
    lbls = ["historia", h["prioridad"], f"sprint:{h['sprint']}"]
    for r in ["Ashley", "Diana", "Jorge", "Rowlis", "Rangelis", "Jaime"]:
        if r.lower() in h["responsable"].lower():
            lbls.append(f"resp:{r}")
    return sorted(set(lbls))


def cuerpo_de_historia(h):
    criterios_items = [c.strip() for c in h["criterios"].split(";") if c.strip()]
    checklist = "\n".join(f"- [ ] {c}" for c in criterios_items)
    return (
        f"### Ficha de la historia (`{h['id']}`)\n\n"
        f"- **Sprint / Etapa:** `{h['sprint']}`\n"
        f"- **Prioridad:** `{h['prioridad']}`\n"
        f"- **Responsable(s):** {h['responsable']}\n"
        f"- **Depende de:** {h['depende_de']}\n\n"
        f"### Descripción\n\n"
        f"{h['titulo_md']}\n\n"
        f"### Criterios de aceptación\n\n"
        f"{checklist}"
    ).strip()


def main():
    dry_run = "--dry-run" in sys.argv
    historias = parsear_backlog()
    print(f"Total de historias leídas desde docs/03-BACKLOG.md: {len(historias)}")
    if dry_run:
        por_sprint = {}
        for h in historias:
            por_sprint[h["sprint"]] = por_sprint.get(h["sprint"], 0) + 1
        print("Desglose por etapa:", por_sprint)
        return

    token = obtener_token_git()
    print("Autenticado correctamente. Verificando etiquetas en AshFranco/Terrun...")
    asegurar_etiquetas(token)
    existentes = obtener_issues_existentes(token)
    print(f"Issues CTZ ya existentes en el repositorio: {len(existentes)}")

    creadas = 0
    actualizadas = 0
    sin_cambios = 0

    for h in historias:
        cid = h["id"]
        titulo_issue = f"{cid}: {h['titulo']}"
        if len(titulo_issue) > 240:
            titulo_issue = titulo_issue[:237] + "..."
        cuerpo = cuerpo_de_historia(h)
        etiquetas = etiquetas_de_historia(h)

        if cid in existentes:
            actual = existentes[cid]
            num = actual["number"]
            cambio_titulo = actual["title"] != titulo_issue
            cambio_cuerpo = actual["body"] != cuerpo
            cambio_lbls = actual["labels"] != etiquetas
            if cambio_titulo or cambio_cuerpo or cambio_lbls:
                gh_api("PATCH", f"/issues/{num}", token, {
                    "title": titulo_issue,
                    "body": cuerpo,
                    "labels": etiquetas,
                })
                actualizadas += 1
                print(f"  ~ Actualizado Issue #{num}: {cid} ({h['sprint']} · {h['prioridad']})")
                time.sleep(0.4)
            else:
                sin_cambios += 1
            continue

        res = gh_api("POST", "/issues", token, {
            "title": titulo_issue,
            "body": cuerpo,
            "labels": etiquetas,
        })
        num = res["number"]
        creadas += 1
        print(f"  + Creado Issue #{num}: {cid} ({h['sprint']} · {h['prioridad']})")
        if cid in HECHAS_S0:
            gh_api("PATCH", f"/issues/{num}", token, {"state": "closed"})
            print(f"    ✓ Marcado como cerrado ({cid} completado en S0)")
        time.sleep(0.55)

    print(
        f"\n¡Sincronización completada! "
        f"Creados: {creadas} · Actualizados: {actualizadas} · Sin cambios: {sin_cambios}."
    )


if __name__ == "__main__":
    main()
