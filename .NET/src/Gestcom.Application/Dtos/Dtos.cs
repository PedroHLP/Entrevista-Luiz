using Gestcom.Domain.Entidades;
using Gestcom.Domain.Servicos;

namespace Gestcom.Application.Dtos;

public record ItemPedidoEntradaDto(string CodigoProduto, decimal Quantidade);

public record RegistrarPedidoDto(string CodigoCliente, IReadOnlyList<ItemPedidoEntradaDto> Itens);

public record ItemPedidoDto(
    int Item,
    string CodigoProduto,
    decimal Quantidade,
    decimal Preco,
    decimal PercentualDesconto,
    decimal ValorDesconto,
    decimal ValorItem,
    decimal AliquotaIcms,
    decimal ValorIcms);

public record BaixaEstoqueDto(
    string CodigoProduto,
    decimal EstoqueAnterior,
    decimal EstoqueNovo,
    decimal QuantidadeVendida,
    decimal QuantidadeBaixada,
    bool AplicouQuebra);

public record SugestaoReposicaoDto(
    string CodigoProduto,
    decimal QuantidadeAtual,
    decimal QuantidadeMinima,
    decimal QuantidadeSugerida,
    DateTime Data);

public record PedidoDto(
    int Numero,
    string CodigoCliente,
    DateTime Data,
    string Status,
    string Cfop,
    decimal ValorBruto,
    decimal ValorDesconto,
    decimal ValorDescontoFuncionario,
    decimal ValorIcms,
    decimal ValorTotal,
    IReadOnlyList<ItemPedidoDto> Itens,
    IReadOnlyList<BaixaEstoqueDto>? Baixas,
    IReadOnlyList<SugestaoReposicaoDto>? Sugestoes);

public record ProdutoDto(
    string Codigo,
    string Descricao,
    string Ncm,
    decimal Preco,
    decimal Estoque,
    decimal EstoqueMinimo,
    bool Ativo);

public record ClienteDto(string Codigo, string Nome, string Tipo, string Uf);

public static class Mapeamentos
{
    public static ItemPedidoDto ParaDto(this ItemPedido item) => new(
        item.Item,
        item.CodigoProduto,
        item.Quantidade,
        item.Preco,
        item.PercentualDesconto,
        item.ValorDesconto,
        item.ValorItem,
        item.AliquotaIcms,
        item.ValorIcms);

    public static PedidoDto ParaDto(
        this Pedido pedido,
        IReadOnlyList<ResultadoBaixaEstoque>? baixas = null)
    {
        var baixasDto = baixas?.Select(b => new BaixaEstoqueDto(
            b.CodigoProduto, b.EstoqueAnterior, b.EstoqueNovo,
            b.QuantidadeVendida, b.QuantidadeBaixada, b.AplicouQuebra)).ToList();

        var sugestoes = baixas?
            .Where(b => b.Sugestao is not null)
            .Select(b => b.Sugestao!.ParaDto())
            .ToList();

        return new PedidoDto(
            pedido.Numero,
            pedido.CodigoCliente,
            pedido.Data,
            pedido.Status,
            pedido.Cfop,
            pedido.ValorBruto,
            pedido.ValorDesconto,
            pedido.ValorDescontoFuncionario,
            pedido.ValorIcms,
            pedido.ValorTotal,
            pedido.Itens.Select(i => i.ParaDto()).ToList(),
            baixasDto,
            sugestoes);
    }

    public static SugestaoReposicaoDto ParaDto(this SugestaoReposicao s) => new(
        s.CodigoProduto, s.QuantidadeAtual, s.QuantidadeMinima, s.QuantidadeSugerida, s.Data);

    public static ProdutoDto ParaDto(this Produto p) => new(
        p.Codigo, p.Descricao, p.Ncm, p.Preco, p.Estoque, p.EstoqueMinimo, p.Ativo);

    public static ClienteDto ParaDto(this Cliente c) => new(
        c.Codigo, c.Nome, c.Tipo.ParaCodigo(), c.Uf);
}
