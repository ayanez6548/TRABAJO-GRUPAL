import { spawnSync } from 'node:child_process'
import { dirname, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import { initializeEnvironment } from './init-demo-env.mjs'

const root = resolve(dirname(fileURLToPath(import.meta.url)), '..')
const frontend = resolve(root, 'frontends/northwind-sdd')
const [major, minor] = process.versions.node.split('.').map(Number)
if (major < 22 || (major === 22 && minor < 13)) {
  console.error('Usa Node 24 o una versión compatible de 22.13 o posterior.')
  process.exit(1)
}

function run(command, args, cwd = root, shell = false) {
  const result = spawnSync(command, args, { cwd, stdio: 'inherit', shell })
  if (result.error) {
    console.error(`No se pudo ejecutar ${command}. Revisa los requisitos en docs/EQUIPO.md.`)
    process.exit(1)
  }
  if (result.signal || result.status !== 0) process.exit(result.status || 1)
}

run('docker', ['info', '--format', '{{.ServerVersion}}'])
const environment = initializeEnvironment()
console.log(environment.created ? '.env privado creado con claves aleatorias.' : '.env existente conservado.')
run('docker', ['compose', '--profile', 'frontend', 'up', '-d', '--build'])
console.log('API/Swagger: http://localhost:5100/swagger\nBlazor: http://localhost:5200\nReact SDD: http://localhost:5300\nCtrl+C detiene React; los contenedores se administran con docker compose.')
// npm.cmd requires the Windows command processor; all arguments here are fixed literals.
const npm = process.platform === 'win32' ? 'npm.cmd' : 'npm'
run(npm, ['ci'], frontend, process.platform === 'win32')
run(npm, ['run', 'dev'], frontend, process.platform === 'win32')
