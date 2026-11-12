using Gestcom.Domain.Entidades;
using Gestcom.Domain.Regras;

namespace Gestcom.Domain.Servicos;

/// <summary>
/// Equivale a trg_baixa_estoque.prg: baixa a quantidade vendida, aplica a quebra
/// oculta de NCM 2202 e gera a sugestão de reposição em cima do estoque já baixado.
/// </summary>
public static class ProcessadorBaixaEstoque
{
    public static ResultadoBaixaEstoque Aplicar(Produto produto, decimal quantidadeVendida, DateTime data)
    {
        var anterior = produto.Estoque;
        var quebra = RegrasEstoque.TemQuebraOperacional(produto.Ncm);
        var baixa = RegrasEstoque.QuantidadeBaixa(produto.Ncm, quantidadeVendida);
        produto.Estoque -= baixa;

        return new ResultadoBaixaEstoque
        {
            CodigoProduto = produto.Codigo,
            EstoqueAnterior = anterior,
            EstoqueNovo = produto.Estoque,
            QuantidadeVendida = quantidadeVendida,
            QuantidadeBaixada = baixa,
            AplicouQuebra = quebra,
            Sugestao = RegrasEstoque.CriarSugestao(produto, data)
        };
    }
}
