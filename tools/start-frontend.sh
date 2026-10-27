#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="${ASPNETCORE_URLS:-http://localhost:5200}"
dotnet run --project 'src/4. Frameworks And Drivers/UIs/NorthWind.Sales.BlazorClient' --no-launch-profile
