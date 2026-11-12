import { type FormEvent, useEffect, useMemo, useState } from 'react'
import { api, brl, destaqueNcm, num, rotuloTipo } from '../api'
import { NotaLegado } from '../components/NotaLegado'
import type { Cliente, ItemPedidoEntrada, Pedido, Produto } from '../types'

type Linha = ItemPedidoEntrada & { id: number }

export function InclusaoPedido() {
  const [clientes, setClientes] = useState<Cliente[]>([])
  const [produtos, setProdutos] = useState<Produto[]>([])
  const [codigoCliente, setCodigoCliente] = useState('')
  const [linhas, setLinhas] = useState<Linha[]>([])
  const [codProd, setCodProd] = useState('')
  const [qtd, setQtd] = useState('1')
  const [erro, setErro] = useState<string | null>(null)
  const [pedido, setPedido] = useState<Pedido | null>(null)
  const [historico, setHistorico] = useState<Pedido[]>([])
  const [enviando, setEnviando] = useState(false)

  const cliente = clientes.find((c) => c.codigo === codigoCliente)
  const mapaProdutos = useMemo(
    () => Object.fromEntries(produtos.map((p) => [p.codigo, p])),
    [produtos],
  )

  useEffect(() => {
    let ativo = true
    Promise.all([api.clientes(), api.produtos(), api.pedidos()])
      .then(([c, p, h]) => {
        if (!ativo) return
        setClientes(c)
        setProdutos(p)
        setHistorico(h)
        if (c[0]) setCodigoCliente(c[0].codigo)
      })
      .catch((e: unknown) => setErro(e instanceof Error ? e.message : 'Falha ao carregar o catálogo.'))
    return () => {
      ativo = false
    }
  }, [])

  function incluirItem(ev: FormEvent) {
    ev.preventDefault()
    setErro(null)
    setPedido(null)
    const produto = mapaProdutos[codProd]
    const quantidade = Number(qtd.replace(',', '.'))
    if (!produto) {
      setErro('Selecione um produto.')
      return
    }
    if (!produto.ativo) {
      setErro('Produto inexistente ou inativo.')
      return
    }
    if (!(quantidade > 0)) {
      setErro('Quantidade inválida.')
      return
    }
    if (quantidade > produto.estoque) {
      setErro(`Estoque insuficiente. Disponível: ${num(produto.estoque)}.`)
      return
    }
    setLinhas((atual) => [...atual, { id: Date.now(), codigoProduto: produto.codigo, quantidade }])
    setQtd('1')
  }

  function remover(id: number) {
    setLinhas((atual) => atual.filter((l) => l.id !== id))
    setPedido(null)
  }

  async function fecharPedido() {
    setErro(null)
    if (!codigoCliente) {
      setErro('Selecione o cliente.')
      return
    }
    if (linhas.length === 0) {
      setErro('Pedido cancelado (nenhum item).')
      return
    }
    setEnviando(true)
    try {
      const resultado = await api.registrarPedido(
        codigoCliente,
        linhas.map((l) => ({ codigoProduto: l.codigoProduto, quantidade: l.quantidade })),
      )
      setPedido(resultado)
      setLinhas([])
      const [p, h] = await Promise.all([api.produtos(), api.pedidos()])
      setProdutos(p)
      setHistorico(h)
    } catch (e) {
      setErro(e instanceof Error ? e.message : 'Não foi possível fechar o pedido.')
    } finally {
      setEnviando(false)
    }
  }

  return (
    <>
      <h1>Inclusão de pedido de venda</h1>
      <p className="lead">
        O cálculo (desconto, ICMS, CFOP, quebra e reposição) é feito na API, com as mesmas rotinas do legado.
      </p>
      <NotaLegado
        programas="ped_inclui.prg, funcoes.prg, spc_calc_icms.prg, spc_desc_funcionario.prg, trg_baixa_estoque.prg"
        texto="A validação de estoque acontece antes de gravar. A quebra de 1 unidade no NCM 2202 só entra na baixa, por isso vender o estoque inteiro de refrigerante pode deixar o saldo em −1. O desconto de funcionário não recalcula o ICMS."
      />

      {erro && <div className="alert erro">{erro}</div>}

      <div className="grid-2">
        <div className="card">
          <label>
            Cliente
            <select value={codigoCliente} onChange={(e) => setCodigoCliente(e.target.value)}>
              {clientes.map((c) => (
                <option key={c.codigo} value={c.codigo}>
                  {c.codigo} — {c.nome} ({rotuloTipo(c.tipo)} / {c.uf})
                </option>
              ))}
            </select>
          </label>
          {cliente && (
            <p className="muted" style={{ marginTop: '0.5rem' }}>
              Tipo <strong>{rotuloTipo(cliente.tipo)}</strong>
              {cliente.tipo === 'V' && ' · faixa 5% a partir de 6 un., 10% a partir de 12'}
              {cliente.tipo === 'F' && ' · 5% no cabeçalho se o subtotal for ≥ R$ 50'}
              {' · '}
              UF {cliente.uf} (CFOP {cliente.uf === 'SP' ? '5102' : '6102'})
            </p>
          )}

          <form className="itens-add" style={{ marginTop: '1rem' }} onSubmit={incluirItem}>
            <label>
              Produto
              <select value={codProd} onChange={(e) => setCodProd(e.target.value)}>
                <option value="">Selecione…</option>
                {produtos.map((p) => (
                  <option key={p.codigo} value={p.codigo}>
                    {p.codigo} — {p.descricao} ({brl(p.preco)} · est. {num(p.estoque)})
                  </option>
                ))}
              </select>
            </label>
            <label>
              Qtd
              <input value={qtd} onChange={(e) => setQtd(e.target.value)} />
            </label>
            <button className="btn secondary" type="submit">
              Incluir item
            </button>
          </form>

          {codProd && mapaProdutos[codProd] && destaqueNcm(mapaProdutos[codProd].ncm) && (
            <p className="muted" style={{ marginTop: '0.4rem' }}>
              <span className="badge warn">{destaqueNcm(mapaProdutos[codProd].ncm)}</span>
            </p>
          )}

          {linhas.length === 0 ? (
            <p className="empty">Nenhum item lançado. É o equivalente a encerrar o pedido em branco no FoxPro.</p>
          ) : (
            <table style={{ marginTop: '0.8rem' }}>
              <thead>
                <tr>
                  <th>Produto</th>
                  <th className="num">Qtd</th>
                  <th className="num">Preço</th>
                  <th className="num">Estoque</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {linhas.map((l) => {
                  const p = mapaProdutos[l.codigoProduto]
                  return (
                    <tr key={l.id}>
                      <td>
                        <span className="codigo">{l.codigoProduto}</span>
                        <br />
                        {p?.descricao}
                        {p && destaqueNcm(p.ncm) && <span className="badge warn">{destaqueNcm(p.ncm)}</span>}
                      </td>
                      <td className="num">{num(l.quantidade)}</td>
                      <td className="num">{p ? brl(p.preco) : '—'}</td>
                      <td className="num">{p ? num(p.estoque) : '—'}</td>
                      <td>
                        <button type="button" className="btn ghost" onClick={() => remover(l.id)}>
                          Remover
                        </button>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          )}

          <div className="actions" style={{ marginTop: '1rem' }}>
            <button className="btn" type="button" disabled={enviando} onClick={() => void fecharPedido()}>
              {enviando ? 'Fechando…' : 'Fechar pedido e baixar estoque'}
            </button>
          </div>
        </div>

        <div className="card">
          {!pedido ? (
            <UltimosPedidos pedidos={historico} clientes={clientes} onAbrir={setPedido} />
          ) : (
            <>
              <ResultadoPedido pedido={pedido} produtos={mapaProdutos} />
              <div className="actions" style={{ marginTop: '1rem' }}>
                <button type="button" className="btn ghost" onClick={() => setPedido(null)}>
                  Ver últimos pedidos
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </>
  )
}

function UltimosPedidos({
  pedidos,
  clientes,
  onAbrir,
}: {
  pedidos: Pedido[]
  clientes: Cliente[]
  onAbrir: (pedido: Pedido) => void
}) {
  if (pedidos.length === 0) {
    return (
      <p className="empty">
        Depois de fechar, os totais oficiais da API aparecem aqui: desconto (truncado), ICMS, CFOP, quebra e sugestão
        de reposição.
      </p>
    )
  }

  const nomes = Object.fromEntries(clientes.map((c) => [c.codigo, c.nome]))
  const recentes = [...pedidos].sort((a, b) => b.numero - a.numero)
  const faturado = pedidos.reduce((acc, p) => acc + p.valorTotal, 0)

  return (
    <>
      <div className="toolbar">
        <strong>Últimos pedidos</strong>
        <span className="muted">
          {pedidos.length} pedidos · {brl(faturado)}
        </span>
      </div>
      <table>
        <thead>
          <tr>
            <th>Pedido</th>
            <th>Cliente</th>
            <th className="num">Total</th>
          </tr>
        </thead>
        <tbody>
          {recentes.map((p) => (
            <tr key={p.numero} className="clickable" onClick={() => onAbrir(p)}>
              <td className="codigo">{String(p.numero).padStart(8, '0')}</td>
              <td>
                {nomes[p.codigoCliente] ?? p.codigoCliente}
                <br />
                <span className="muted">
                  {p.itens.length} {p.itens.length === 1 ? 'item' : 'itens'} · CFOP {p.cfop}
                </span>
              </td>
              <td className="num">{brl(p.valorTotal)}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <p className="muted" style={{ marginTop: '0.8rem' }}>
        Clique num pedido para ver os totais calculados pela API. Feche um novo pedido ao lado para ver também a baixa
        de estoque e a quebra.
      </p>
    </>
  )
}

function ResultadoPedido({
  pedido,
  produtos,
}: {
  pedido: Pedido
  produtos: Record<string, Produto>
}) {
  const teveQuebra = pedido.baixas?.some((b) => b.aplicouQuebra)

  return (
    <>
      <div className="toolbar">
        <strong>Pedido {String(pedido.numero).padStart(8, '0')}</strong>
        <span className="badge ok">Status {pedido.status}</span>
      </div>
      <p className="muted">
        CFOP <strong>{pedido.cfop}</strong> · {new Date(pedido.data).toLocaleDateString('pt-BR')}
      </p>

      <table>
        <thead>
          <tr>
            <th>Item</th>
            <th className="num">% desc.</th>
            <th className="num">Desconto</th>
            <th className="num">Alíq.</th>
            <th className="num">ICMS</th>
            <th className="num">Líquido</th>
          </tr>
        </thead>
        <tbody>
          {pedido.itens.map((i) => (
            <tr key={i.item}>
              <td>
                <span className="codigo">{i.codigoProduto}</span> · {num(i.quantidade)} un.
                <br />
                <span className="muted">{produtos[i.codigoProduto]?.descricao ?? ''}</span>
              </td>
              <td className="num">{num(i.percentualDesconto)}</td>
              <td className="num">{brl(i.valorDesconto)}</td>
              <td className="num">{num(i.aliquotaIcms)}%</td>
              <td className="num">{brl(i.valorIcms)}</td>
              <td className="num">{brl(i.valorItem)}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="totais" style={{ marginTop: '0.9rem' }}>
        <div className="linha">
          <span>Valor bruto</span>
          <span>{brl(pedido.valorBruto)}</span>
        </div>
        <div className="linha">
          <span>
            Desconto
            {pedido.valorDescontoFuncionario > 0 && ' (inclui funcionário)'}
          </span>
          <span>{brl(pedido.valorDesconto)}</span>
        </div>
        <div className="linha">
          <span>ICMS</span>
          <span>{brl(pedido.valorIcms)}</span>
        </div>
        <div className="linha forte">
          <span>Total</span>
          <span>{brl(pedido.valorTotal)}</span>
        </div>
      </div>

      {pedido.baixas && pedido.baixas.length > 0 && (
        <div className={teveQuebra ? 'resultado-quebra' : ''} style={{ marginTop: '1rem' }}>
          <h3>Baixa de estoque {teveQuebra && <span className="badge warn">Quebra aplicada</span>}</h3>
          <table>
            <thead>
              <tr>
                <th>Produto</th>
                <th className="num">Vendido</th>
                <th className="num">Baixado</th>
                <th className="num">Antes</th>
                <th className="num">Depois</th>
              </tr>
            </thead>
            <tbody>
              {pedido.baixas.map((b) => (
                <tr key={b.codigoProduto}>
                  <td className="codigo">
                    {b.codigoProduto}
                    {b.aplicouQuebra && <span className="badge warn">+1 quebra</span>}
                  </td>
                  <td className="num">{num(b.quantidadeVendida)}</td>
                  <td className="num">{num(b.quantidadeBaixada)}</td>
                  <td className="num">{num(b.estoqueAnterior)}</td>
                  <td className="num">{num(b.estoqueNovo)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {pedido.sugestoes && pedido.sugestoes.length > 0 && (
        <p className="muted" style={{ marginTop: '0.8rem' }}>
          Sugestão de reposição:{' '}
          {pedido.sugestoes.map((s) => (
            <span key={s.codigoProduto}>
              produto {s.codigoProduto} · sugeridas <strong>{num(s.quantidadeSugerida)}</strong> un.{' '}
            </span>
          ))}
          Veja o relatório completo em Reposição.
        </p>
      )}
    </>
  )
}
