import { useEffect, useState } from 'react'
import { BrowserRouter, Link, NavLink, Navigate, Outlet, Route, Routes } from 'react-router-dom'
import { NoticeProvider, SessionProvider, useSession } from './context'
import { Access } from './Access'
import { OrderPage } from './OrderPage'
import { InvoicesPage } from './InvoicesPage'

function Clock() {
  const [now, setNow] = useState(new Date())
  useEffect(() => {
    const timer = setInterval(() => setNow(new Date()), 1000)
    return () => clearInterval(timer)
  }, [])
  return (
    <time className="clock" dateTime={now.toISOString()}>
      {now.toLocaleDateString('es-EC', { day: 'numeric', month: 'short' })} ·{' '}
      {now.toLocaleTimeString('es-EC')}
    </time>
  )
}
function Layout() {
  const { session, logout } = useSession()
  return (
    <>
      <header className="site-header">
        <div className="header-inner">
          <Link className="brand" to="/">
            <span className="brand-mark">N</span>NorthWind
            <span className="brand-subtitle">Ventas</span>
          </Link>
          <nav aria-label="Navegación principal">
            <NavLink to="/">Inicio</NavLink>
            {session && (
              <>
                <NavLink to="/orders/new">Nueva venta</NavLink>
                <NavLink to="/invoices">Facturas</NavLink>
              </>
            )}
          </nav>
          <div className="account-nav">
            {session ? (
              <>
                <div className="user-name">
                  <strong>{session.fullName}</strong>
                  <Clock />
                </div>
                <button className="secondary" onClick={logout}>
                  Salir
                </button>
              </>
            ) : (
              <>
                <Link to="/register">Crear cuenta</Link>
                <Link className="button" to="/login">
                  Iniciar sesión
                </Link>
              </>
            )}
          </div>
        </div>
      </header>
      <main className="container">
        <Outlet />
      </main>
      <footer className="site-footer">NorthWind · Ventas y comprobantes</footer>
    </>
  )
}
function Protected() {
  return useSession().session ? <Outlet /> : <Navigate to="/login" replace />
}
function Home() {
  const { session } = useSession()
  return (
    <section className="home">
      <div>
        <p className="eyebrow">NorthWind / Ventas</p>
        <h1>
          Todo listo para
          <br />
          la siguiente venta.
        </h1>
        <p className="intro">
          Selecciona un cliente, prepara su pedido y guarda la factura. Tus ventas quedan a mano
          para consultarlas cuando las necesites.
        </p>
        <Link className="button" to={session ? '/orders/new' : '/login'}>
          {session ? 'Preparar una venta' : 'Entrar a mi cuenta'} <span aria-hidden="true">→</span>
        </Link>
      </div>
      <aside className="home-note">
        <span className="note-number">01</span>
        <h2>Antes de guardar</h2>
        <p>
          Revisa los productos, las cantidades y la dirección de entrega. El total se calcula
          mientras preparas la orden.
        </p>
        <hr />
        <span className="note-number">02</span>
        <h2>Después de vender</h2>
        <p>
          Encuentra tus comprobantes en Facturas y descarga el PDF para compartirlo o imprimirlo.
        </p>
      </aside>
    </section>
  )
}
export function App() {
  return (
    <BrowserRouter>
      <NoticeProvider>
        <SessionProvider>
          <Routes>
            <Route element={<Layout />}>
              <Route index element={<Home />} />
              <Route path="login" element={<Access />} />
              <Route path="register" element={<Access register />} />
              <Route element={<Protected />}>
                <Route path="orders/new" element={<OrderPage />} />
                <Route path="invoices" element={<InvoicesPage />} />
              </Route>
              <Route path="*" element={<Navigate to="/" replace />} />
            </Route>
          </Routes>
        </SessionProvider>
      </NoticeProvider>
    </BrowserRouter>
  )
}
