#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
frontend_root="$project_root/frontends/northwind-sdd"
if [[ ! -f "$frontend_root/dist/index.html" ]]; then
  printf '%s\n' 'Falta compilar React. Dentro de frontends/northwind-sdd ejecuta: npm ci && npm run build'
  exit 1
fi
exec python3 "$frontend_root/tools/serve-sdd.py" --api-url "${NORTHWIND_API_URL:-http://127.0.0.1:5100}" --port "${NORTHWIND_SDD_PORT:-5300}"
