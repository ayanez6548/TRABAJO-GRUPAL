import assert from 'node:assert/strict'
import { mkdirSync, mkdtempSync, readFileSync, writeFileSync } from 'node:fs'
import { dirname, join, resolve } from 'node:path'
import test from 'node:test'
import { fileURLToPath } from 'node:url'
import { initializeEnvironment } from './init-demo-env.mjs'

const artifacts = resolve(dirname(fileURLToPath(import.meta.url)), '../artifacts/TeamHandoff')
mkdirSync(artifacts, { recursive: true })

test('configuración nueva usa claves aleatorias y modo correo local', () => {
  const root = mkdtempSync(join(artifacts, 'fresh-'))
  assert.equal(initializeEnvironment(root).created, true)
  const content = readFileSync(join(root, '.env'), 'utf8')
  assert.match(content, /^POSTGRES_PASSWORD=Nw10![a-f0-9]{48}$/m)
  assert.match(content, /^JWT_SECURITY_KEY=[a-f0-9]{128}$/m)
  assert.match(content, /^SMTP_MODE=PickupDirectory$/m)
})

test('reiniciar la preparación nunca reemplaza claves ni preferencias existentes', () => {
  const root = mkdtempSync(join(artifacts, 'existing-'))
  const existing = 'POSTGRES_PASSWORD=fixture-not-a-real-secret\nSMTP_MODE=CustomFixture\n'
  writeFileSync(join(root, '.env'), existing)
  assert.equal(initializeEnvironment(root).created, false)
  assert.equal(readFileSync(join(root, '.env'), 'utf8'), existing)
})

test('dos copias independientes no comparten las claves generadas', () => {
  const first = mkdtempSync(join(artifacts, 'one-'))
  const second = mkdtempSync(join(artifacts, 'two-'))
  initializeEnvironment(first)
  initializeEnvironment(second)
  assert.notEqual(readFileSync(join(first, '.env'), 'utf8'), readFileSync(join(second, '.env'), 'utf8'))
})
