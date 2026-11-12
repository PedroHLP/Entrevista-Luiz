export type Produto = {
  codigo: string
  descricao: string
  ncm: string
  preco: number
  estoque: number
  estoqueMinimo: number
  ativo: boolean
}

export type Cliente = {
  codigo: string
  nome: string
  tipo: string
  uf: string
}

export type ItemPedido = {
  item: number
  codigoProduto: string
  quantidade: number
  preco: number
  percentualDesconto: number
  valorDesconto: number
  valorItem: number
  aliquotaIcms: number
  valorIcms: number
}

export type BaixaEstoque = {
  codigoProduto: string
  estoqueAnterior: number
  estoqueNovo: number
  quantidadeVendida: number
  quantidadeBaixada: number
  aplicouQuebra: boolean
}

export type SugestaoReposicao = {
  codigoProduto: string
  quantidadeAtual: number
  quantidadeMinima: number
  quantidadeSugerida: number
  data: string
}

export type Pedido = {
  numero: number
  codigoCliente: string
  data: string
  status: string
  cfop: string
  valorBruto: number
  valorDesconto: number
  valorDescontoFuncionario: number
  valorIcms: number
  valorTotal: number
  itens: ItemPedido[]
  baixas?: BaixaEstoque[] | null
  sugestoes?: SugestaoReposicao[] | null
}

export type ItemPedidoEntrada = {
  codigoProduto: string
  quantidade: number
}

export type ErroApi = {
  erro?: string
  codigoProduto?: string
  disponivel?: number
}
