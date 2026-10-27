#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
if command -v systemctl >/dev/null && systemctl is-active --quiet northwind-api.service; then
  printf '%s\n' 'La API publicada ya está activa en http://localhost:5100.'
  printf '%s\n' 'Usa sudo systemctl restart northwind-api y journalctl -u northwind-api para administrarla.'
  exit 0
fi
if [[ -z "${ConnectionStrings__NorthWind:-}" && -f .env.debian ]]; then
  while IFS='=' read -r key value; do
    case "$key" in Database__Provider|ConnectionStrings__NorthWind|JwtOptions__SecurityKey|SmtpOptions__*) export "$key=$value" ;; esac
  done < .env.debian
fi
if [[ -z "${ConnectionStrings__NorthWind:-}" ]]; then
  printf '%s\n' 'Set ConnectionStrings__NorthWind to a database connection reachable from Debian.' >&2
  printf '%s\n' 'For the local PostgreSQL container: bash tools/init-demo-env.sh && docker compose up -d db' >&2
  exit 1
fi
export Database__Provider="${Database__Provider:-PostgreSql}"
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="${ASPNETCORE_URLS:-http://localhost:5100}"
export DevelopmentTools__MailtrapUrl=''
dotnet run --project 'src/4. Frameworks And Drivers/UI/NorthWind.Sales.WebApi' --no-launch-profile
