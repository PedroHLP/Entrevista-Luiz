import { useEffect, useState } from 'react'
import { api, num } from '../api'
import { NotaLegado } from '../components/NotaLegado'
import type { Produto, SugestaoReposicao } from '../types'

export function Reposicao() {
  const [lista, setLista] = useState<SugestaoReposicao[]>([])
  const [produtos, setProdutos] = useState<Record<string, Produto>>({})
  const [erro, setErro] = useState<string | null>(null)
  const [carregando, setCarregando] = useState(true)

  async function carregar() {
    setCarregando(true)
    setErro(null)
    try {
      const [sugestoes, catalogo] = await Promise.all([api.reposicao(), api.produtos()])
      setLista(sugestoes)
      setProdutos(Object.fromEntries(catalogo.map((p) => [p.codigo, p])))
    } catch (e) {
      setErro(e instanceof Error ? e.message : 'Falha ao carregar a reposição.')
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    void carregar()
  }, [])

  const total = lista.reduce((acc, s) => acc + s.quantidadeSugerida, 0)

  return (
    <>
      <h1>Sugestão de reposição</h1>
      <p className="lead">Base do pedido de compra, gerada quando o estoque cai abaixo do mínimo.</p>
      <NotaLegado
        programas="rel_reposicao.prg, trg_baixa_estoque.prg"
        texto="A fórmula é a mesma do trigger: quantidade sugerida = estoque mínimo × 2 − estoque atual. Usa o saldo já depois da quebra operacional. Produto com mínimo zero nunca entra na lista."
      />

      {erro && <div className="alert erro">{erro}</div>}

      <div className="card">
        <div className="toolbar">
          <strong>Sugestões pendentes</strong>
          <button type="button" className="btn secondary" onClick={() => void carregar()}>
            Atualizar
          </button>
        </div>
        {carregando ? (
          <p className="empty">Carregando…</p>
        ) : lista.length === 0 ? (
          <p className="empty">Nenhuma sugestão de reposição pendente. Feche um pedido que deixe o estoque abaixo do mínimo para gerar uma linha.</p>
        ) : (
          <>
            <table>
              <thead>
                <tr>
                  <th>Código</th>
                  <th>Descrição</th>
                  <th className="num">Atual</th>
                  <th className="num">Mínimo</th>
                  <th className="num">Sugerido</th>
                  <th>Emissão</th>
                </tr>
              </thead>
              <tbody>
                {lista.map((s) => (
                  <tr key={s.codigoProduto}>
                    <td className="codigo">{s.codigoProduto}</td>
                    <td>{produtos[s.codigoProduto]?.descricao ?? '—'}</td>
                    <td className="num">{num(s.quantidadeAtual)}</td>
                    <td className="num">{num(s.quantidadeMinima)}</td>
                    <td className="num">{num(s.quantidadeSugerida)}</td>
                    <td>{new Date(s.data).toLocaleDateString('pt-BR')}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <p className="muted" style={{ marginTop: '0.8rem' }}>
              Itens sugeridos (soma qtd): <strong>{num(total)}</strong>
            </p>
          </>
        )}
      </div>
    </>
  )
}
