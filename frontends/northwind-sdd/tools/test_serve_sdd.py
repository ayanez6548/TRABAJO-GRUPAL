import importlib.util
import json
import tempfile
import threading
import unittest
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.error import HTTPError
from urllib.request import Request, urlopen

spec = importlib.util.spec_from_file_location("serve_sdd", Path(__file__).with_name("serve-sdd.py"))
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class Backend(BaseHTTPRequestHandler):
    def do_GET(self):
        self.send_response(404 if self.path == "/orders/9" else 200)
        self.send_header("Content-Type", "application/pdf" if self.path.endswith(".pdf") else "application/json")
        self.end_headers()
        self.wfile.write(b"%PDF-test" if self.path.endswith(".pdf") else json.dumps({"path": self.path, "auth": self.headers.get("Authorization"), "origin": self.headers.get("Origin")}).encode())

    def do_POST(self):
        self.send_response(200)
        self.end_headers()
        self.wfile.write(self.rfile.read(int(self.headers["Content-Length"])))

    def log_message(self, *args):
        pass


class ServerTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.temp = tempfile.TemporaryDirectory()
        cls.root = Path(cls.temp.name)
        (cls.root / "index.html").write_text("<html lang='es'>NorthWind</html>")
        (cls.root / "worker.mjs").write_text("export const worker = true")
        cls.backend = ThreadingHTTPServer(("127.0.0.1", 0), Backend)
        cls.server = module.create_server("127.0.0.1", 0, cls.root, f"http://127.0.0.1:{cls.backend.server_port}")
        cls.url = f"http://127.0.0.1:{cls.server.server_port}"
        for server in (cls.backend, cls.server):
            threading.Thread(target=server.serve_forever, daemon=True).start()

    @classmethod
    def tearDownClass(cls):
        for server in (cls.server, cls.backend):
            server.shutdown()
            server.server_close()
        cls.temp.cleanup()

    def test_spa_direct_route(self):
        with urlopen(self.url + "/invoices") as response:
            self.assertIn(b"NorthWind", response.read())
            self.assertEqual(response.headers["Cache-Control"], "no-store")

    def test_proxy_preserves_bearer_query_and_status(self):
        response = json.load(urlopen(Request(self.url + "/api/orders?page=2", headers={"Authorization": "Bearer fixture", "Origin": self.url})))
        self.assertEqual(response, {"path": "/orders?page=2", "auth": "Bearer fixture", "origin": None})
        with self.assertRaises(HTTPError) as error:
            urlopen(self.url + "/api/orders/9")
        self.assertEqual(error.exception.code, 404)

    def test_proxy_post_and_pdf(self):
        payload = b'{"customerId":"ALFKI"}'
        self.assertEqual(urlopen(Request(self.url + "/api/CreateOrder", data=payload)).read(), payload)
        with urlopen(self.url + "/api/orders/1/invoice.pdf") as response:
            self.assertEqual(response.headers["Content-Type"], "application/pdf")
            self.assertEqual(response.read(), b"%PDF-test")

    def test_rejects_traversal_missing_assets_and_secrets(self):
        for path in ("/%2e%2e/secret", "/.env", "/assets/missing.js", "/%5csecret", "/C:/Windows"):
            with self.subTest(path=path), self.assertRaises(HTTPError) as error:
                urlopen(self.url + path)
            self.assertEqual(error.exception.code, 404)

    def test_rejects_arbitrary_target(self):
        for target in ("file:///etc/passwd", "http://user:secret@localhost", "http://localhost/api"):
            with self.assertRaises(ValueError):
                module.create_server("127.0.0.1", 0, self.root, target)

    def test_worker_module_mime_type(self):
        with urlopen(self.url + "/worker.mjs") as response:
            self.assertEqual(response.headers["Content-Type"], "text/javascript")


if __name__ == "__main__":
    unittest.main()
