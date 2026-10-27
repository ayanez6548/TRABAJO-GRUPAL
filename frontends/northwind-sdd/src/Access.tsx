import { useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from './api/client'
import { useFailure, useNotice, useSession } from './context'
import { Field } from './components'
import { accountIssues, credentialsIssues, issueMessages } from './domain/rules'
import type { Account, Tokens } from './domain/types'

export function Access({ register = false }: { register?: boolean }) {
  const [account, setAccount] = useState<Account>({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
  })
  const [busy, setBusy] = useState(false)
  const pending = useRef(false)
  const { show } = useNotice()
  const { login } = useSession()
  const failure = useFailure()
  const navigate = useNavigate()
  const update = (field: keyof Account) => (value: string) =>
    setAccount((previous) => ({ ...previous, [field]: value }))
  async function submit() {
    if (pending.current) return
    const issues = register
      ? accountIssues(account)
      : credentialsIssues(account.email, account.password)
    if (issues.length) {
      show(issueMessages(issues))
      return
    }
    pending.current = true
    setBusy(true)
    try {
      if (register) {
        const { confirmPassword: _, ...body } = account
        await api.post('/user/Register', body)
        setAccount((previous) => ({ ...previous, password: '', confirmPassword: '' }))
        navigate('/login')
        show(['Tu cuenta está lista. Ingresa con tu correo y contraseña.'], 'Cuenta creada')
      } else {
        login(
          await api.post<Tokens>('/user/Login', {
            email: account.email,
            password: account.password,
          }),
        )
        navigate('/orders/new')
      }
    } catch (error) {
      failure(error)
    } finally {
      pending.current = false
      setBusy(false)
    }
  }
  return (
    <section className="access panel">
      <p className="eyebrow">Tu cuenta</p>
      <h1>{register ? 'Crear cuenta' : 'Iniciar sesión'}</h1>
      <p className="muted">
        {register
          ? 'Registra tus datos para empezar a trabajar.'
          : 'Ingresa para preparar ventas y consultar tus facturas.'}
      </p>
      <form
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          void submit()
        }}
      >
        <fieldset disabled={busy}>
          {register && (
            <>
              <Field
                label="Nombres"
                autoComplete="given-name"
                value={account.firstName}
                onChange={update('firstName')}
                rule="letters"
                limit={30}
                hint="Solo letras y espacios. Máximo 30 caracteres."
              />
              <Field
                label="Apellidos"
                autoComplete="family-name"
                value={account.lastName}
                onChange={update('lastName')}
                rule="letters"
                limit={30}
                hint="Solo letras y espacios. Máximo 30 caracteres."
              />
            </>
          )}
          <Field
            label="Correo electrónico"
            inputMode="email"
            autoComplete="email"
            value={account.email}
            onChange={update('email')}
            rule="email"
            limit={30}
            hint="Máximo 30 caracteres. Símbolos: . _ - + y una sola @."
          />
          <Field
            label="Contraseña"
            type="password"
            autoComplete={register ? 'new-password' : 'current-password'}
            value={account.password}
            onChange={update('password')}
            hint={
              register
                ? 'Al menos 6 caracteres: mayúscula, minúscula, número y símbolo. Sin límite de longitud.'
                : undefined
            }
          />
          {register && (
            <Field
              label="Confirmar contraseña"
              type="password"
              autoComplete="new-password"
              value={account.confirmPassword}
              onChange={update('confirmPassword')}
            />
          )}
          <button className="full" type="submit">
            {busy ? 'Un momento…' : register ? 'Crear cuenta' : 'Ingresar'}
          </button>
        </fieldset>
      </form>
      <p className="access-link">
        {register ? '¿Ya tienes cuenta?' : '¿Primera vez aquí?'}{' '}
        <Link to={register ? '/login' : '/register'}>
          {register ? 'Inicia sesión' : 'Crea tu cuenta'}
        </Link>
      </p>
    </section>
  )
}
