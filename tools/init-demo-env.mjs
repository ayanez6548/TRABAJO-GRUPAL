import { randomBytes } from 'node:crypto'
import { existsSync, writeFileSync } from 'node:fs'
import { dirname, join, resolve } from 'node:path'
import { fileURLToPath, pathToFileURL } from 'node:url'

const projectRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..')

export function initializeEnvironment(root = projectRoot) {
  const destination = join(root, '.env')
  if (existsSync(destination)) return { created: false }
  const content = [
    `POSTGRES_PASSWORD=Nw10!${randomBytes(24).toString('hex')}`,
    `JWT_SECURITY_KEY=${randomBytes(64).toString('hex')}`,
    'SMTP_MODE=PickupDirectory',
    'SMTP_USERNAME=',
    'SMTP_PASSWORD=',
    '',
  ].join('\n')
  try {
    writeFileSync(destination, content, { flag: 'wx', mode: 0o600 })
    return { created: true }
  } catch (error) {
    if (error.code === 'EEXIST') return { created: false }
    throw error
  }
}

if (process.argv[1] && pathToFileURL(resolve(process.argv[1])).href === import.meta.url) {
  const result = initializeEnvironment()
  console.log(result.created ? 'Configuración privada creada en .env. No la subas a Git.' : '.env ya existe; sus claves se conservaron.')
}
