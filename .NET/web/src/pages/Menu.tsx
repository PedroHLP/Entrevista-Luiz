import { Link } from 'react-router-dom'
import { NotaLegado } from '../components/NotaLegado'

export function Menu() {
  return (
    <>
      <h1>Menu principal</h1>
      <p className="lead">
        As mesmas opções do Gestcom legado, agora em uma tela web que fala com a API .NET.
      </p>
      <NotaLegado
        programas="menu.prg"
        texto="No FoxPro este era o menu de texto (opções 1, 2, 3 e 9). Aqui cada item abre a tela correspondente, com as regras de negócio iguais às do legado."
      />
      <div className="grid-3">
        <Link className="tile" to="/produtos">
          <div className="num">1</div>
          <h2>Cadastro de produtos</h2>
          <p>Código, descrição, NCM, preço, estoque e estoque mínimo.</p>
        </Link>
        <Link className="tile" to="/pedidos">
          <div className="num">2</div>
          <h2>Inclusão de pedido de venda</h2>
          <p>Cliente, itens, desconto, ICMS/CFOP, baixa de estoque e quebra operacional.</p>
        </Link>
        <Link className="tile" to="/reposicao">
          <div className="num">3</div>
          <h2>Sugestão de reposição</h2>
          <p>Relatório-base do pedido de compra, quando o estoque cai abaixo do mínimo.</p>
        </Link>
      </div>
    </>
  )
}
