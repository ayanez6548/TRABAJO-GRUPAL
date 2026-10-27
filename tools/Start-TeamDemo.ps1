$ErrorActionPreference = 'Stop'
& node (Join-Path $PSScriptRoot 'start-team-demo.mjs')
if ($LASTEXITCODE -ne 0) { throw 'No se pudo completar el arranque. Revisa docs/EQUIPO.md.' }
