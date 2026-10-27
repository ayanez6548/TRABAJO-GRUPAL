#!/usr/bin/env python3
"""Start, inspect or stop this project's background demonstration sessions."""
from pathlib import Path
import argparse
import os
import signal
import socket
import subprocess
import time
from urllib.request import urlopen

parser = argparse.ArgumentParser(description='Inicia, consulta o detiene la demostración en Debian.')
action = parser.add_mutually_exclusive_group()
action.add_argument('--stop', action='store_true', help='Detiene las ejecuciones administradas de este proyecto.')
action.add_argument('--start', choices=('backend', 'frontend'), help='Inicia la API o Blazor y comprueba su respuesta HTTP.')
args = parser.parse_args()
if os.name != 'posix':
    parser.exit(message='Ejecuta esta herramienta en Debian.\n')
root = Path(__file__).resolve().parents[1]
try:
    published_backend = subprocess.run(['systemctl', 'is-active', '--quiet', 'northwind-api.service'],
                                       capture_output=True).returncode == 0
except FileNotFoundError:
    published_backend = False

if args.start == 'backend' and published_backend:
    try:
        with urlopen('http://localhost:5100/health', timeout=5) as response:
            if response.status != 200: raise OSError('API not ready')
    except OSError:
        parser.exit(1, 'El servicio está iniciado pero la API aún no responde. Revisa sudo journalctl -u northwind-api.\n')
    print('API publicada: HTTP 200 en http://localhost:5100/health; servicio northwind-api activo.')
    print('Inicio automático con Debian. Usa sudo systemctl restart northwind-api para reiniciarla.')
    parser.exit()

if args.start:
    label, port, endpoint = ('API', 5100, '/health') if args.start == 'backend' else ('Blazor', 5200, '/')
    script = f'tools/start-{args.start}.sh'
    if not (root / script).is_file():
        parser.exit(1, 'Este paquete no contiene el proyecto solicitado. Usa la carpeta completa para Blazor.\n')
    try:
        with socket.create_connection(('localhost', port), timeout=2):
            parser.exit(1, f'El puerto {port} ya está ocupado. Comprueba la aplicación antes de iniciar otra copia.\n')
    except OSError:
        pass
    folder = root / 'artifacts' / f'Demo-{time.time_ns()}'
    folder.mkdir(parents=True)
    log = folder / f'{args.start}.log'
    with log.open('wb') as output:
        process = subprocess.Popen(['bash', script], cwd=root, stdin=subprocess.DEVNULL,
                                   stdout=output, stderr=subprocess.STDOUT, start_new_session=True)
    (folder / f'{args.start}.pid').write_text(str(process.pid))
    deadline = time.monotonic() + 180
    while time.monotonic() < deadline:
        if process.poll() is not None:
            parser.exit(1, f'{label} no pudo iniciar. Revisa el archivo:\n{log}\n')
        try:
            with urlopen(f'http://localhost:{port}{endpoint}', timeout=3) as response:
                if response.status == 200:
                    print(f'{label}: respuesta HTTP 200 en http://localhost:{port}{endpoint}')
                    print(f'Registro de ejecución: {log}')
                    print('Permanece ejecutándose al cerrar SSH; vuelve a iniciarlo después de reiniciar Debian.')
                    parser.exit()
        except OSError:
            pass
        time.sleep(1)
    # A timeout must not leave an unverified process occupying the demonstration port.
    if process.poll() is None:
        os.killpg(process.pid, signal.SIGTERM)
    parser.exit(1, f'{label} no respondió en 180 segundos. Revisa el archivo:\n{log}\n')

if published_backend:
    print('API publicada: servicio northwind-api activo; inicio automático con Debian.')
    if args.stop:
        print('Para detener el backend publicado usa sudo systemctl stop northwind-api.')
seen = set()
files = list((root / 'artifacts').glob('SpanishUpdate-*/*.pid'))
files += list((root / 'artifacts').glob('Demo-*/*.pid'))
for file in sorted(files):
    try:
        pid = int(file.read_text().strip())
        process = Path('/proc') / str(pid)
        if pid <= 1 or pid in seen or process.stat().st_uid != os.getuid():
            continue
        if os.getpgid(pid) != pid or os.getsid(pid) != pid or (process / 'cwd').resolve() != root:
            continue
        arguments = (process / 'cmdline').read_bytes().decode('utf-8', 'replace').split('\0')
        script = {'backend': 'tools/start-backend.sh', 'frontend': 'tools/start-frontend.sh'}.get(file.stem)
        if not script or len(arguments) < 2 or Path(arguments[0]).name != 'bash' or arguments[1] != script:
            continue
        seen.add(pid)
        label = 'API' if file.stem == 'backend' else 'Blazor'
        if args.stop:
            os.killpg(pid, signal.SIGTERM)
            print(f'{label}: se solicitó la detención de la demostración.')
        else:
            print(f'{label}: en ejecución (PID {pid}).')
    except (OSError, ValueError):
        continue
if not seen and not published_backend:
    print('No hay ejecuciones administradas activas en este proyecto; las iniciadas manualmente se detienen con Ctrl+C.')
