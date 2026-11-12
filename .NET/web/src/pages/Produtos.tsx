import { type FormEvent, useEffect, useState } from 'react'
import { api, brl, destaqueNcm, num } from '../api'
import { NotaLegado } from '../components/NotaLegado'
import type { Produto } from '../types'

const vazio: Produto = {
  codigo: '',
  descricao: '',
  ncm: '',
  preco: 0,
  estoque: 0,
  estoqueMinimo: 0,
  ativo: true,
}

export function Produtos() {
  const [lista, setLista] = useState<Produto[]>([])
  const [form, setForm] = useState<Produto>(vazio)
  const [editando, setEditando] = useState(false)
  const [erro, setErro] = useState<string | null>(null)
  const [ok, setOk] = useState<string | null>(null)
  const [carregando, setCarregando] = useState(true)

  async function carregar() {
    setCarregando(true)
    setErro(null)
    try {
      setLista(await api.produtos())
    } catch (e) {
      setErro(e instanceof Error ? e.message : 'Falha ao carregar produtos.')
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    void carregar()
  }, [])

  function selecionar(p: Produto) {
    setForm({ ...p })
    setEditando(true)
    setOk(null)
    setErro(null)
  }

  function novo() {
    setForm(vazio)
    setEditando(false)
    setOk(null)
    setErro(null)
  }

  async function salvar(ev: FormEvent) {
    ev.preventDefault()
    setErro(null)
    setOk(null)
    if (!form.codigo.trim() || !form.descricao.trim() || form.ncm.trim().length < 8) {
      setErro('Informe código, descrição e um NCM com 8 dígitos.')
      return
    }
    try {
      const salvo = await api.salvarProduto({
        ...form,
        codigo: form.codigo.trim(),
        descricao: form.descricao.trim(),
        ncm: form.ncm.trim(),
      })
      setOk(editando ? `Produto ${salvo.codigo} atualizado.` : `Produto ${salvo.codigo} incluído.`)
      setForm(salvo)
      setEditando(true)
      await carregar()
    } catch (e) {
      setErro(e instanceof Error ? e.message : 'Não foi possível salvar.')
    }
  }

  return (
    <>
      <h1>Cadastro de produtos</h1>
      <p className="lead">Inclusão e alteração, no mesmo espírito da tela de cadastro do Gestcom.</p>
      <NotaLegado
        programas="cad_produtos.prg, criatabelas.prg"
        texto="A tabela continua sendo a PRODUTOS do legado (CODPROD, DESCRICAO, NCM, PRECO, ESTOQUE, ESTMIN, ATIVO). A carga inicial veio do dados_iniciais.prg, aplicada automaticamente pela API."
      />

      {erro && <div className="alert erro">{erro}</div>}
      {ok && <div className="alert ok">{ok}</div>}

      <div className="grid-2">
        <div className="card">
          <div className="toolbar">
            <strong>Produtos</strong>
            <button type="button" className="btn secondary" onClick={() => void carregar()}>
              Atualizar
            </button>
          </div>
          {carregando ? (
            <p className="empty">Carregando…</p>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>Código</th>
                  <th>Descrição</th>
                  <th className="num">Preço</th>
                  <th className="num">Estoque</th>
                  <th className="num">Mín.</th>
                </tr>
              </thead>
              <tbody>
                {lista.map((p) => (
                  <tr key={p.codigo} className="clickable" onClick={() => selecionar(p)}>
                    <td className="codigo">{p.codigo}</td>
                    <td>
                      {p.descricao}
                      {!p.ativo && <span className="badge danger">Inativo</span>}
                      {destaqueNcm(p.ncm) && <span className="badge warn">{destaqueNcm(p.ncm)}</span>}
                    </td>
                    <td className="num">{brl(p.preco)}</td>
                    <td className="num">{num(p.estoque)}</td>
                    <td className="num">{num(p.estoqueMinimo)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        <form className="card form" onSubmit={(ev) => void salvar(ev)}>
          <div className="toolbar">
            <strong>{editando ? 'Alterar produto' : 'Novo produto'}</strong>
            {editando && (
              <button type="button" className="btn ghost" onClick={novo}>
                Limpar
              </button>
            )}
          </div>
          <label>
            Código
            <input
              value={form.codigo}
              maxLength={10}
              disabled={editando}
              onChange={(e) => setForm({ ...form, codigo: e.target.value })}
            />
          </label>
          <label>
            Descrição
            <input
              value={form.descricao}
              maxLength={50}
              onChange={(e) => setForm({ ...form, descricao: e.target.value })}
            />
          </label>
          <div className="form-row">
            <label>
              NCM
              <input
                value={form.ncm}
                maxLength={8}
                onChange={(e) => setForm({ ...form, ncm: e.target.value })}
              />
            </label>
            <label>
              Preço
              <input
                type="number"
                step="0.01"
                min={0}
                value={form.preco}
                onChange={(e) => setForm({ ...form, preco: Number(e.target.value) })}
              />
            </label>
          </div>
          <div className="form-row">
            <label>
              Estoque
              <input
                type="number"
                step="0.01"
                min={0}
                value={form.estoque}
                onChange={(e) => setForm({ ...form, estoque: Number(e.target.value) })}
              />
            </label>
            <label>
              Estoque mínimo
              <input
                type="number"
                step="0.01"
                min={0}
                value={form.estoqueMinimo}
                onChange={(e) => setForm({ ...form, estoqueMinimo: Number(e.target.value) })}
              />
            </label>
          </div>
          <label>
            <span>
              <input
                type="checkbox"
                checked={form.ativo}
                onChange={(e) => setForm({ ...form, ativo: e.target.checked })}
              />{' '}
              Ativo
            </span>
          </label>
          <div className="actions">
            <button className="btn" type="submit">
              {editando ? 'Salvar alterações' : 'Incluir produto'}
            </button>
          </div>
        </form>
      </div>
    </>
  )
}
