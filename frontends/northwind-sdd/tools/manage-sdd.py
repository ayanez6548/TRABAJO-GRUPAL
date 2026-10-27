#!/usr/bin/env python3
"""Manage only this frontend's background process on Debian; no elevated access."""
import argparse
import os
from pathlib import Path
import signal
import socket
import subprocess
import sys
import time
from urllib.request import urlopen

parser = argparse.ArgumentParser(description="Administrar React SDD en Debian")
action = parser.add_mutually_exclusive_group(required=True)
action.add_argument("--start", action="store_true")
action.add_argument("--status", action="store_true")
action.add_argument("--stop", action="store_true")
parser.add_argument("--port", type=int, default=5300)
parser.add_argument("--api-url", default="http://127.0.0.1:5100")
args = parser.parse_args()
if os.name != "posix":
    parser.exit(1, "Ejecuta esta herramienta dentro de Debian.\n")
root = Path(__file__).resolve().parents[1]
runtime = root / "App_Data"
pid_file = runtime / f"react-{args.port}.pid"
script = root / "tools/serve-sdd.py"


def managed_pid():
    try:
        pid = int(pid_file.read_text().strip())
        process = Path("/proc") / str(pid)
        command = (process / "cmdline").read_bytes().decode().split("\0")
        if pid > 1 and process.stat().st_uid == os.getuid() and (process / "cwd").resolve() == root and str(script) in command and os.getsid(pid) == pid:
            return pid
    except (OSError, ValueError):
        pass
    return None


def status(pid):
    print(f"React SDD activo (PID {pid}). URL: http://127.0.0.1:{args.port}")
    try:
        with urlopen(f"http://127.0.0.1:{args.port}/api/health", timeout=5) as response:
            print("API: " + response.read().decode())
    except OSError:
        print("React inició, pero la API todavía no responde. Revisa northwind-api.")


pid = managed_pid()
if args.stop:
    if pid:
        os.kill(pid, signal.SIGTERM)
        print("Se solicitó detener únicamente React SDD.")
    else:
        print("No hay un proceso React SDD administrado activo.")
    parser.exit()
if pid:
    status(pid)
    parser.exit()
if args.status:
    parser.exit(1, "React SDD no está iniciado mediante esta herramienta.\n")
if not (root / "dist/index.html").is_file():
    parser.exit(1, "Falta dist/index.html. Compila React o usa el paquete que incluye dist.\n")
try:
    with socket.create_connection(("127.0.0.1", args.port), timeout=1):
        parser.exit(1, "El puerto está ocupado por otra ejecución. No se modificó ese proceso.\n")
except OSError:
    pass
runtime.mkdir(exist_ok=True)
with (runtime / f"react-{args.port}.log").open("ab") as output:
    process = subprocess.Popen([sys.executable, "-u", str(script), "--port", str(args.port), "--api-url", args.api_url], cwd=root,
                               stdin=subprocess.DEVNULL, stdout=output, stderr=subprocess.STDOUT, start_new_session=True)
pid_file.write_text(str(process.pid))
deadline = time.monotonic() + 10
while time.monotonic() < deadline and process.poll() is None:
    try:
        with urlopen(f"http://127.0.0.1:{args.port}/", timeout=1) as response:
            if response.status == 200:
                status(process.pid)
                print("Puedes cerrar SSH. Después de reiniciar Debian, vuelve a ejecutar --start.")
                parser.exit()
    except OSError:
        pass
    time.sleep(0.2)
if process.poll() is None:
    process.terminate()
parser.exit(1, f"React no pudo iniciar. Revisa {runtime / f'react-{args.port}.log'}\n")
