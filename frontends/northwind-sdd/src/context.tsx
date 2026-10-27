import { createContext, useCallback, useContext, useEffect, useRef, useState } from 'react'
import type { ReactNode } from 'react'
import { ApiError, cancelled } from './api/client'
import { ValidationFailure } from './domain/rules'
import { readSession, sessionKey } from './domain/session'
import type { Session, Tokens } from './domain/types'

interface Notice {
  title: string
  messages: string[]
  confirm: boolean
  resolve?: (answer: boolean) => void
}
const NoticeContext = createContext({
  show: (_messages: string[], _title?: string) => {},
  confirm: async (_message: string) => false,
})
export function NoticeProvider({ children }: { children: ReactNode }) {
  const [notice, setNotice] = useState<Notice | null>(null)
  const current = useRef<Notice | null>(null)
  const open = useCallback((value: Notice) => {
    current.current?.resolve?.(false)
    current.current = value
    setNotice(value)
  }, [])
  const show = useCallback(
    (messages: string[], title = 'Revisa estos datos') =>
      open({ title, messages: [...new Set(messages)], confirm: false }),
    [open],
  )
  const confirm = useCallback(
    (message: string) =>
      new Promise<boolean>((resolve) =>
        open({ title: 'Quitar producto', messages: [message], confirm: true, resolve }),
      ),
    [open],
  )
  const close = (answer: boolean) => {
    current.current?.resolve?.(answer)
    current.current = null
    setNotice(null)
  }
  return (
    <NoticeContext.Provider value={{ show, confirm }}>
      {children}
      {notice && (
        <Modal title={notice.title} onClose={() => close(false)} small>
          <div className="notice-messages">
            {notice.messages.map((message) => (
              <p key={message}>{message}</p>
            ))}
          </div>
          <div className="dialog-actions">
            {notice.confirm && (
              <button data-initial-focus="true" className="secondary" onClick={() => close(false)}>
                Cancelar
              </button>
            )}
            <button
              data-initial-focus={!notice.confirm ? 'true' : undefined}
              onClick={() => close(true)}
            >
              {notice.confirm ? 'Sí, quitar' : 'Entendido'}
            </button>
          </div>
        </Modal>
      )}
    </NoticeContext.Provider>
  )
}
export const useNotice = () => useContext(NoticeContext)

export function Modal({
  title,
  onClose,
  children,
  small = false,
}: {
  title: string
  onClose: () => void
  children: ReactNode
  small?: boolean
}) {
  const ref = useRef<HTMLDialogElement>(null)
  useEffect(() => {
    const dialog = ref.current!
    const previous = document.activeElement as HTMLElement | null
    dialog.showModal()
    dialog.querySelector<HTMLElement>('[data-initial-focus="true"]')?.focus()
    document.body.classList.add('modal-open')
    return () => {
      dialog.close()
      if (!document.querySelector('dialog[open]')) document.body.classList.remove('modal-open')
      previous?.focus()
    }
  }, [])
  return (
    <dialog
      ref={ref}
      className={small ? 'modal small' : 'modal'}
      aria-label={title}
      onCancel={(event) => {
        event.preventDefault()
        onClose()
      }}
    >
      <header className="dialog-header">
        <h2>{title}</h2>
        <button
          type="button"
          className="icon-button secondary"
          aria-label="Cerrar ventana"
          onClick={onClose}
        >
          ×
        </button>
      </header>
      {children}
    </dialog>
  )
}
const SessionContext = createContext<{
  session: Session | null
  login: (tokens: Tokens) => void
  logout: () => void
}>({ session: null, login: () => {}, logout: () => {} })
export function SessionProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState(() => {
    try {
      return readSession(sessionStorage.getItem(sessionKey))
    } catch {
      return null
    }
  })
  const { show } = useNotice()
  const logout = useCallback(() => {
    sessionStorage.removeItem(sessionKey)
    setSession(null)
  }, [])
  const login = useCallback((tokens: Tokens) => {
    const next = readSession(JSON.stringify(tokens))
    if (!next) throw new ValidationFailure(['El servidor no entregó una sesión válida.'])
    sessionStorage.setItem(sessionKey, JSON.stringify(tokens))
    setSession(next)
  }, [])
  useEffect(() => {
    if (!session) return
    const expire = () => {
      logout()
      show(['Tu sesión venció. Inicia sesión nuevamente.'], 'Sesión finalizada')
    }
    const timer = window.setTimeout(
      expire,
      Math.max(0, new Date(session.expiresAt).getTime() - Date.now()),
    )
    return () => clearTimeout(timer)
  }, [session, logout, show])
  return (
    <SessionContext.Provider value={{ session, login, logout }}>{children}</SessionContext.Provider>
  )
}
export const useSession = () => useContext(SessionContext)
export function useFailure() {
  const { show } = useNotice()
  const { logout, session } = useSession()
  return useCallback(
    (error: unknown) => {
      if (cancelled(error)) return
      if (error instanceof ApiError && error.status === 401 && session) logout()
      show(
        error instanceof ApiError || error instanceof ValidationFailure
          ? error.messages
          : ['No se pudo completar la operación. Inténtalo nuevamente.'],
      )
    },
    [show, logout, session],
  )
}
