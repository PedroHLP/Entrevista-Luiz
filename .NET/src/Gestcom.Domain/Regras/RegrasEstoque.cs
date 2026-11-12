using Gestcom.Domain.Entidades;

namespace Gestcom.Domain.Regras;

/// <summary>
/// Baixa de estoque e sugestão de reposição. Fonte: trg_baixa_estoque.prg e ped_inclui.prg.
/// A quebra operacional de NCM 2202 (chamado 4412 / 2016) não está no manual
/// e não entra na validação de estoque feita antes de gravar o item.
/// </summary>
public static class RegrasEstoque
{
    public const string PrefixoNcmQuebra = "2202";
    public const decimal UnidadesQuebra = 1m;

    public static bool TemQuebraOperacional(string ncm)
        => (ncm ?? string.Empty).Trim().StartsWith(PrefixoNcmQuebra, StringComparison.Ordinal);

    public static decimal QuantidadeBaixa(string ncm, decimal quantidadeVendida)
        => quantidadeVendida + (TemQuebraOperacional(ncm) ? UnidadesQuebra : 0m);

    public static bool PrecisaReposicao(decimal estoqueAtual, decimal estoqueMinimo)
        => estoqueMinimo > 0m && estoqueAtual < estoqueMinimo;

    /// <summary>QTDSUG = ESTMIN * 2 - ESTOQUE. Só se ESTMIN &gt; 0 e estoque &lt; mínimo.</summary>
    public static decimal? QuantidadeSugerida(decimal estoqueAtual, decimal estoqueMinimo)
    {
        if (!PrecisaReposicao(estoqueAtual, estoqueMinimo))
            return null;
        return estoqueMinimo * 2m - estoqueAtual;
    }

    public static SugestaoReposicao? CriarSugestao(Produto produto, DateTime data)
    {
        var qtd = QuantidadeSugerida(produto.Estoque, produto.EstoqueMinimo);
        if (qtd is null)
            return null;

        return new SugestaoReposicao
        {
            CodigoProduto = produto.Codigo,
            QuantidadeAtual = produto.Estoque,
            QuantidadeMinima = produto.EstoqueMinimo,
            QuantidadeSugerida = qtd.Value,
            Data = data.Date
        };
    }
}
