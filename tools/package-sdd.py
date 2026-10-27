#!/usr/bin/env python3
"""Package the third frontend, its specs, source and compiled files for Debian."""
from pathlib import Path
import zipfile

root = Path(__file__).resolve().parents[1]
frontend = root / "frontends/northwind-sdd"
destination = root.parent / "NorthWind-React-SDD.zip"
excluded = {"node_modules", "coverage", "__pycache__", ".git", ".vite", "App_Data"}
if not (frontend / "dist/index.html").is_file():
    raise SystemExit("Build the frontend first: npm run build")
with zipfile.ZipFile(destination, "w", zipfile.ZIP_DEFLATED) as archive:
    for file in frontend.rglob("*"):
        if not file.is_file():
            continue
        relative = file.relative_to(frontend)
        if any(part in excluded for part in relative.parts):
            continue
        if file.name.startswith(".env") and file.name != ".env.example":
            continue
        if file.suffix in (".log", ".pyc"):
            continue
        archive.write(file, "northwind-sdd/" + relative.as_posix())
with zipfile.ZipFile(destination) as archive:
    assert archive.testzip() is None
    names = archive.namelist()
    assert "northwind-sdd/dist/index.html" in names
    assert "northwind-sdd/specs/001-sales-frontend/spec.md" in names
    assert not any("/node_modules/" in name or name.endswith("/.env") for name in names)
print(f"{destination.name}: {len(names)} archivos, {destination.stat().st_size} bytes; incluye dist y fuentes SDD.")
