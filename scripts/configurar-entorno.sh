#!/bin/sh
# Configura tu clon del repositorio. Ejecutar una vez por clon:  ./scripts/configurar-entorno.sh
set -e
cd "$(git rev-parse --show-toplevel)"

git config core.hooksPath .githooks
echo "✔ Hooks activados (.githooks)"

if [ -z "$(git config user.name)" ] || [ -z "$(git config user.email)" ]; then
    echo "✖ Falta tu identidad de git. Ejecuta:"
    echo "    git config user.name  \"Tu Nombre\""
    echo "    git config user.email \"tu.correo@ejemplo.com\""
    exit 1
fi
echo "✔ Identidad git: $(git config user.name) <$(git config user.email)>"

if [ -f CLAUDE.local.md ]; then
    echo "✔ CLAUDE.local.md ya existe (no se modifica)"
    exit 0
fi

echo ""
echo "¿Quién eres?"
echo "  1) Ashley   — Scrum Master + Backend"
echo "  2) Diana    — Product Owner"
echo "  3) Jorge    — Front: núcleo técnico (GPS, mapa, offline)"
echo "  4) Rowlis   — Front: experiencia (cuenta, social, clubes, gamificación) + backend puntual"
echo "  5) Rangelis — QA: plan, casos y API"
echo "  6) Jaime    — QA: campo y regresión"
printf "Número: "
read -r OPCION
case "$OPCION" in
    1) PERSONA="Ashley"; ROL="Scrum Master + Backend" ;;
    2) PERSONA="Diana"; ROL="Product Owner" ;;
    3) PERSONA="Jorge"; ROL="Front: núcleo técnico" ;;
    4) PERSONA="Rowlis"; ROL="Front: experiencia + backend puntual" ;;
    5) PERSONA="Rangelis"; ROL="QA: plan, casos y API" ;;
    6) PERSONA="Jaime"; ROL="QA: campo y regresión" ;;
    *) echo "Opción no válida"; exit 1 ;;
esac

cat > CLAUDE.local.md <<EOT
# Contexto personal (no se sube al repositorio)

La persona que trabaja en esta máquina es **$PERSONA** ($ROL).
Responde siempre en español.

Antes de cualquier tarea, sigue la guía del equipo:

@docs/00-GUIA-DE-TRABAJO.md
EOT
echo "✔ CLAUDE.local.md creado para $PERSONA (ignorado por git)"
