$ErrorActionPreference = 'Stop'
$northWindEnvironmentFile = Join-Path (Split-Path $PSScriptRoot -Parent) '.env'
if (Test-Path -LiteralPath $northWindEnvironmentFile) { Write-Output '.env already exists; current credentials were preserved.'; return }
function New-PrivateHex {
    param([int]$ByteCount)
    $buffer = New-Object byte[] $ByteCount
    $generator = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $generator.GetBytes($buffer) }
    finally { $generator.Dispose() }
    return [BitConverter]::ToString($buffer).Replace('-', '')
}
$northWindSqlPassword = 'Nw10!' + (New-PrivateHex -ByteCount 24)
$northWindJwtKey = New-PrivateHex -ByteCount 64
[IO.File]::WriteAllText($northWindEnvironmentFile, "POSTGRES_PASSWORD=$northWindSqlPassword`nJWT_SECURITY_KEY=$northWindJwtKey`nSMTP_MODE=PickupDirectory`nSMTP_USERNAME=`nSMTP_PASSWORD=`n")
Write-Output 'Local .env initialized with random credentials. Do not commit or share this file.'
