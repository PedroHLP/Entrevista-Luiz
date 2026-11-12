import type {
  Cliente,
  ErroApi,
  ItemPedidoEntrada,
  Pedido,
  Produto,
  SugestaoReposicao,
} from './types'

const API = import.meta.env.VITE_API_URL ?? 'http://localhost:5088'

async function http<T>(path: string, init?: RequestInit): Promise<T> {
  let resposta: Response
  try {
    resposta = await fetch(`${API}${path}`, {
      ...init,
      headers: {
        'Content-Type': 'application/json',
        ...(init?.headers ?? {}),
      },
    })
  } catch {
    throw new Error(
      'Não foi possível conectar à API. Suba o Gestcom em http://localhost:5088 (dotnet run --project src/Gestcom.Api).',
    )
  }

  if (!resposta.ok) {
    let mensagem = `Erro ${resposta.status}`
    try {
      const corpo = (await resposta.json()) as ErroApi
      if (corpo.erro) mensagem = corpo.erro
    } catch {
      /* corpo não é JSON */
    }
    throw new Error(mensagem)
  }

  if (resposta.status === 204) return undefined as T
  return (await resposta.json()) as T
}

export const api = {
  produtos: () => http<Produto[]>('/api/produtos'),
  produto: (codigo: string) => http<Produto>(`/api/produtos/${encodeURIComponent(codigo)}`),
  salvarProduto: (produto: Produto) =>
    http<Produto>('/api/produtos', { method: 'POST', body: JSON.stringify(produto) }),
  clientes: () => http<Cliente[]>('/api/clientes'),
  pedidos: () => http<Pedido[]>('/api/pedidos'),
  pedido: (numero: number) => http<Pedido>(`/api/pedidos/${numero}`),
  registrarPedido: (codigoCliente: string, itens: ItemPedidoEntrada[]) =>
    http<Pedido>('/api/pedidos', {
      method: 'POST',
      body: JSON.stringify({ codigoCliente, itens }),
    }),
  reposicao: () => http<SugestaoReposicao[]>('/api/reposicao'),
}

export const brl = (n: number) =>
  n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })

export const num = (n: number, casas = 2) =>
  n.toLocaleString('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas })

export function rotuloTipo(tipo: string): string {
  switch (tipo) {
    case 'V':
      return 'Varejista'
    case 'F':
      return 'Funcionário'
    default:
      return 'Consumidor'
  }
}

export function destaqueNcm(ncm: string): string | null {
  if (ncm.startsWith('2202')) return 'Quebra operacional (+1 un. na baixa)'
  if (ncm.startsWith('1905')) return 'ICMS reduzido 7% (padaria)'
  return null
}
