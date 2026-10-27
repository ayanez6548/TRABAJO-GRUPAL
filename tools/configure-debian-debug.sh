#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
bash tools/init-demo-env.sh
password="$(sed -n 's/^POSTGRES_PASSWORD=//p' .env)"
key="$(sed -n 's/^JWT_SECURITY_KEY=//p' .env)"
umask 077
printf 'Database__Provider=PostgreSql\nConnectionStrings__NorthWind=Host=localhost;Port=54330;Database=northwind;Username=northwind;Password=%s\nJwtOptions__SecurityKey=%s\nSmtpOptions__Mode=PickupDirectory\n' "$password" "$key" > .env.debian
printf '%s\n' 'Private .env.debian created for VS Code F5. Start the database with docker compose up -d db.'
