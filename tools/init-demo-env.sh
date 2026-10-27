#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
if [[ -f .env ]]; then printf '%s\n' '.env already exists; current credentials were preserved.'; exit 0; fi
umask 077
sql_password="Nw10!$(openssl rand -hex 24)"
jwt_key="$(openssl rand -hex 64)"
printf 'POSTGRES_PASSWORD=%s\nJWT_SECURITY_KEY=%s\nSMTP_MODE=PickupDirectory\nSMTP_USERNAME=\nSMTP_PASSWORD=\n' "$sql_password" "$jwt_key" > .env
printf '%s\n' 'Local .env initialized. Keep this file private.'
