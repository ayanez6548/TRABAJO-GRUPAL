#!/usr/bin/env python3
"""Create source-only full and backend delivery archives, excluding local secrets."""
from pathlib import Path
import json
import os
import re
import zipfile

root = Path(__file__).resolve().parents[1]
excluded = {'bin', 'obj', 'publish', 'artifacts', '.vs', '.git', 'App_Data', 'TestResults', '.nuget-offline', '__pycache__', 'node_modules', 'dist', 'coverage', '.vite'}
files = [p for p in root.rglob('*') if p.is_file()
         and not any(part in excluded for part in p.relative_to(root).parts)
         and not (p.name.startswith('.env') and p.name != '.env.example')
         and p.suffix.lower() not in {'.bak', '.mdf', '.ldf', '.log', '.suo', '.user', '.zip', '.pyc'}]
backend = json.loads((root / 'NorthWind.Backend.slnf').read_text())['solution']['projects']
backend_dirs = {Path(p.replace('\\', '/')).parent.as_posix() for p in backend}
private = []
secret_path = Path(os.environ.get('APPDATA', '')) / 'Microsoft/UserSecrets/NorthWind-Practicas-8-WebApi/secrets.json'
if secret_path.is_file():
    private += [v for k, v in json.loads(secret_path.read_text(encoding='utf-8-sig')).items()
                if isinstance(v, str) and len(v) > 6 and any(s in k.lower() for s in ('password', 'username', 'securitykey'))]
for env_name in ('.env', '.env.debian'):
    p = root / env_name
    if p.is_file():
        private += [line.split('=', 1)[1] for line in p.read_text().splitlines()
                    if '=' in line and any(s in line.split('=', 1)[0].lower() for s in ('password', 'username', 'security'))
                    and len(line.split('=', 1)[1]) > 6]
for p in files:
    if p.suffix.lower() in {'.cs', '.razor', '.json', '.md', '.props', '.csproj', '.http', '.ps1', '.sh', '.py', '.yaml', '.ts', '.tsx', '.js', '.mjs', '.html', '.css', '.txt'}:
        content = p.read_text(encoding='utf-8-sig')
        if any(value in content for value in private):
            raise ValueError(f'Private configuration detected in {p.relative_to(root)}')

solution = (root / 'NorthWind.sln').read_text(encoding='utf-8-sig')
removed = []
def filter_project(match):
    block = match.group(0)
    header = block.splitlines()[0]
    fields = re.findall(r'"([^"]*)"', header)
    if len(fields) >= 4 and fields[2].endswith('.csproj') and fields[2].replace('\\', '/') not in backend:
        removed.append(fields[3])
        return ''
    return block
backend_solution = re.sub(r'^Project\(.*?^EndProject\s*\n', filter_project, solution, flags=re.M | re.S)
backend_solution = '\n'.join(line for line in backend_solution.splitlines()
                             if not any(guid in line for guid in removed)) + '\n'

for backend_only in (False, True):
    destination = root.parent / ('NorthWind-Backend-NET10-Debian.zip' if backend_only else 'NorthWind-NET10.zip')
    with zipfile.ZipFile(destination, 'w', zipfile.ZIP_DEFLATED) as archive:
        for p in files:
            relative = p.relative_to(root).as_posix()
            if backend_only:
                if relative.startswith('frontends/'):
                    continue
                if relative.startswith('src/') and not any(relative.startswith(d + '/') for d in backend_dirs):
                    continue
                if relative.startswith('tests/') or relative in {'NorthWind.slnLaunch', 'tools/start-frontend.sh'}:
                    continue
            content = p.read_bytes()
            if backend_only and relative == 'NorthWind.sln':
                content = backend_solution.encode('utf-8')
            elif backend_only and relative == 'Dockerfile.debian':
                content = re.sub(r'FROM sdk AS frontend.*?(?=FROM sdk AS backend)', '', content.decode(), flags=re.S).encode()
            elif backend_only and relative == 'compose.yaml':
                content = re.sub(r'  frontend:.*?(?=^volumes:)', '', content.decode(), flags=re.M | re.S).encode()
            elif backend_only and relative == 'README.md':
                content = b'# NorthWind backend - .NET 10 / Debian 13\n\nThis delivery contains only the 23 backend projects. The complete solution and individual Blazor frontend are in NorthWind-NET10.zip.\n\nFollow docs/DEBIAN.md for backend execution with the SDK and VS Code or Docker. Frontend instructions in that guide apply to the full delivery.\n\nRun: bash tools/init-demo-env.sh, then docker compose up -d --build api. Open http://localhost:5100/swagger.\n'
            archive.writestr('NorthWind/' + relative, content)
    with zipfile.ZipFile(destination) as archive:
        assert archive.testzip() is None
        projects = [n for n in archive.namelist() if n.endswith('.csproj')]
        assert len(projects) == (23 if backend_only else 32), len(projects)
        assert not any('/.env' in n and not n.endswith('/.env.example') for n in archive.namelist())
    print(f'{destination.name}: {len(projects)} projects, source only, private configuration excluded.')
