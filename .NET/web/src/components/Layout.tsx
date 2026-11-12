import { NavLink, Outlet } from 'react-router-dom'

export function Layout() {
  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand">
          <strong>GESTCOM</strong>
          <span>Pedidos e estoque · .NET 8</span>
        </div>
        <nav className="nav">
          <NavLink to="/" end>
            Menu
          </NavLink>
          <NavLink to="/produtos">Produtos</NavLink>
          <NavLink to="/pedidos">Novo pedido</NavLink>
          <NavLink to="/reposicao">Reposição</NavLink>
        </nav>
        <div className="topbar-meta">
          Migração do Visual FoxPro 9
          <br />
          Quality Digital · demonstração
        </div>
      </header>
      <main className="page">
        <Outlet />
      </main>
    </div>
  )
}
