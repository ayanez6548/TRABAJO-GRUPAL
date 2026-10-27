#!/usr/bin/env bash
set -euo pipefail
. /etc/os-release
if [[ "$ID" != debian || "$VERSION_ID" != 13 ]]; then
  printf '%s\n' 'This setup targets Debian 13 (trixie), the distribution selected for this project.' >&2
  exit 1
fi
sudo apt-get update
sudo apt-get install -y ca-certificates curl gpg openssl python3 fonts-dejavu-core libfontconfig1
curl -fsSL https://packages.microsoft.com/config/debian/13/packages-microsoft-prod.deb -o /tmp/northwind-microsoft-prod.deb
sudo dpkg -i /tmp/northwind-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /tmp/northwind-microsoft.gpg
sudo install -D -m 644 /tmp/northwind-microsoft.gpg /usr/share/keyrings/microsoft.gpg
printf '%s\n' 'deb [arch=amd64,arm64 signed-by=/usr/share/keyrings/microsoft.gpg] https://packages.microsoft.com/repos/code stable main' | sudo tee /etc/apt/sources.list.d/vscode.list > /dev/null
sudo apt-get update
sudo apt-get install -y code
dotnet --list-sdks
printf '%s\n' 'Install Docker Engine using docs/DEBIAN.md if a database server is not already available.'
