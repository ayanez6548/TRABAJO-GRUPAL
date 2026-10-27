param([string]$ApiUrl = 'http://localhost:5100', [switch]$RequireLinux)
$ErrorActionPreference = 'Stop'
$python = Get-Command python3, python -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $python) { throw 'Install Python 3 to run the portable HTTP verification.' }
$arguments = @((Join-Path $PSScriptRoot 'smoke-test.py'), '--api', $ApiUrl)
if ($RequireLinux) { $arguments += '--require-linux' }
& $python.Source @arguments
exit $LASTEXITCODE
