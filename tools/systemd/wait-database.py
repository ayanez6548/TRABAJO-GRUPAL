#!/usr/bin/env python3
"""Wait for the configured PostgreSQL listener without logging credentials."""
import os
import socket
import sys
import time

settings = {}
for part in os.environ.get('ConnectionStrings__NorthWind', '').split(';'):
    key, separator, value = part.partition('=')
    if separator and key.strip().lower() in ('host', 'port'):
        settings[key.strip().lower()] = value.strip()
try:
    host = settings['host']
    port = int(settings.get('port', '5432'))
except (KeyError, ValueError):
    sys.exit('Configura Host y Port de PostgreSQL en .env.debian.')
deadline = time.monotonic() + 90
while time.monotonic() < deadline:
    try:
        with socket.create_connection((host, port), timeout=2):
            sys.exit(0)
    except OSError:
        time.sleep(1)
sys.exit('PostgreSQL no respondió durante el inicio del servicio. Revisa Docker y la conexión.')
