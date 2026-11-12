import { Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { Menu } from './pages/Menu'
import { InclusaoPedido } from './pages/Pedido'
import { Produtos } from './pages/Produtos'
import { Reposicao } from './pages/Reposicao'

export function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Menu />} />
        <Route path="produtos" element={<Produtos />} />
        <Route path="pedidos" element={<InclusaoPedido />} />
        <Route path="reposicao" element={<Reposicao />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  )
}
