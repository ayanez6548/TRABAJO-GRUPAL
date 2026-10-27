#!/usr/bin/env python3
"""Serve the compiled SPA and proxy /api to one explicitly configured backend."""
import argparse
import http.client
import json
import mimetypes
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import unquote, urlsplit


class FrontendHandler(BaseHTTPRequestHandler):
    server_version = "NorthWindFrontend/1.0"

    def log_message(self, format, *args):
        # Do not log query strings, credentials, headers, or request bodies.
        print(f"{self.command} {self.path.split('?')[0]} — {args[1] if len(args) > 1 else ''}", flush=True)

    def send_content(self, status, body, content_type, cache="no-store"):
        self.send_response(status)
        self.send_header("Content-Type", content_type)
        self.send_header("Content-Length", str(len(body)))
        self.send_header("Cache-Control", cache)
        self.send_header("X-Content-Type-Options", "nosniff")
        self.send_header("Referrer-Policy", "same-origin")
        self.end_headers()
        self.wfile.write(body)

    def problem(self, status, message):
        self.send_content(status, json.dumps({"status": status, "detail": message}, ensure_ascii=False).encode(), "application/problem+json; charset=utf-8")

    def proxy(self):
        target = self.server.api_target
        connection_type = http.client.HTTPSConnection if target.scheme == "https" else http.client.HTTPConnection
        connection = connection_type(target.hostname, target.port, timeout=60)
        try:
            length = int(self.headers.get("Content-Length", "0"))
            if length < 0 or length > 1024 * 1024 or self.headers.get("Transfer-Encoding"):
                self.problem(413, "La solicitud supera el tamaño permitido.")
                return
            body = self.rfile.read(length) if length else None
            headers = {key: self.headers[key] for key in ("Authorization", "Accept", "Content-Type") if key in self.headers}
            path = self.path[4:]
            if not path.startswith("/") or path.startswith("//") or "\\" in unquote(path):
                self.problem(400, "La dirección solicitada no es válida.")
                return
            # A fixed target prevents this from becoming an arbitrary URL proxy.
            connection.request(self.command, path, body, headers)
            response = connection.getresponse()
            self.send_content(response.status, response.read(), response.getheader("Content-Type", "application/octet-stream"))
        except (OSError, http.client.HTTPException, ValueError):
            self.problem(502, "No se pudo conectar con la API. Comprueba que el backend esté iniciado.")
        finally:
            connection.close()

    def do_GET(self):
        path = urlsplit(self.path).path
        if path.startswith("/api/"):
            self.proxy()
            return
        decoded = unquote(path)
        root = self.server.static_root
        relative = decoded.lstrip("/")
        if "\\" in decoded or any(part.startswith(".") or ":" in part for part in Path(relative).parts):
            self.problem(404, "No se encontró la página.")
            return
        file = (root / relative).resolve()
        if not file.is_relative_to(root):
            self.problem(404, "No se encontró la página.")
            return
        if not file.is_file():
            if Path(relative).suffix or relative.startswith("assets/"):
                self.problem(404, "No se encontró el archivo.")
                return
            file = root / "index.html"
        try:
            body = file.read_bytes()
        except OSError:
            self.problem(404, "Compila el frontend con npm run build antes de iniciar.")
            return
        content_type = mimetypes.guess_type(str(file))[0] or "application/octet-stream"
        if file.suffix in (".js", ".mjs"):
            content_type = "text/javascript"
        self.send_content(200, body, content_type, "public, max-age=31536000, immutable" if file.parent.name == "assets" else "no-store")

    def do_POST(self):
        if urlsplit(self.path).path.startswith("/api/"):
            self.proxy()
        else:
            self.problem(405, "Esta dirección no acepta esa operación.")


def create_server(host, port, root, api_url):
    target = urlsplit(api_url)
    if target.scheme not in ("http", "https") or not target.hostname or target.username or target.password or target.path not in ("", "/") or target.query or target.fragment:
        raise ValueError("API URL must be an HTTP(S) origin without credentials or path")
    server = ThreadingHTTPServer((host, port), FrontendHandler)
    server.static_root = Path(root).resolve()
    server.api_target = target
    return server


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="NorthWind React: archivos compilados y proxy de API")
    parser.add_argument("--host", default="127.0.0.1")
    parser.add_argument("--port", type=int, default=5300)
    parser.add_argument("--api-url", default="http://127.0.0.1:5100")
    parser.add_argument("--dist", type=Path, default=Path(__file__).resolve().parents[1] / "dist")
    args = parser.parse_args()
    server = create_server(args.host, args.port, args.dist, args.api_url)
    print(f"NorthWind React: http://{args.host}:{args.port} — API: {args.api_url}", flush=True)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        server.server_close()
