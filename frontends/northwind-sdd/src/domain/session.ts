import type { Session } from './types'

export const sessionKey = 'northwind.sdd.session'
export function readSession(raw: string | null, now = Date.now()): Session | null {
  try {
    if (!raw) return null
    const value = JSON.parse(raw)
    if (
      typeof value.accessToken !== 'string' ||
      !value.accessToken ||
      typeof value.expiresAt !== 'string' ||
      !Number.isFinite(Date.parse(value.expiresAt)) ||
      Date.parse(value.expiresAt) <= now
    )
      return null
    const parts = value.accessToken.split('.')
    if (parts.length !== 3) return null
    const payload = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const claims = JSON.parse(
      new TextDecoder().decode(
        Uint8Array.from(atob(payload.padEnd(Math.ceil(payload.length / 4) * 4, '=')), (character) =>
          character.charCodeAt(0),
        ),
      ),
    )
    if (typeof claims.exp !== 'number' || claims.exp * 1000 <= now) return null
    const expiresAt = new Date(
      Math.min(Date.parse(value.expiresAt), claims.exp * 1000),
    ).toISOString()
    return {
      accessToken: value.accessToken,
      expiresAt,
      fullName: typeof claims.FullName === 'string' ? claims.FullName : 'Usuario',
      email: typeof claims.name === 'string' ? claims.name : '',
    }
  } catch {
    return null
  }
}
