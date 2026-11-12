using System.Text.Json;
using FluentAssertions;
using Gestcom.Domain.Entidades;
using Gestcom.Domain.Servicos;

namespace Gestcom.ParityTests;

public class ParidadeLegadoTests
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IEnumerable<object[]> Casos()
    {
        foreach (var caso in Carregar().Casos)
            yield return new object[] { caso.Id, caso };
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void Totais_reproduzem_o_resultado_capturado_do_legado(string id, CasoParidade caso)
    {
        _ = id;
        var cliente = Catalogo.Cliente(caso.Cliente);
        var entradas = caso.Itens
            .Select(i => new ItemCalculoEntrada
            {
                Produto = Catalogo.Produto(i.CodigoProduto),
                Quantidade = i.Quantidade
            })
            .ToList();

        var pedido = new CalculadoraPedido().Calcular(cliente, entradas);

        pedido.Cfop.Should().Be(caso.Esperado.Cfop);
        pedido.ValorBruto.Should().Be(caso.Esperado.ValorBruto);
        pedido.ValorDesconto.Should().Be(caso.Esperado.ValorDesconto);
        pedido.ValorDescontoFuncionario.Should().Be(caso.Esperado.ValorDescontoFuncionario);
        pedido.ValorIcms.Should().Be(caso.Esperado.ValorIcms);
        pedido.ValorTotal.Should().Be(caso.Esperado.ValorTotal);
        pedido.Itens.Should().HaveCount(caso.Esperado.Itens.Count);

        for (var i = 0; i < caso.Esperado.Itens.Count; i++)
        {
            var obtido = pedido.Itens[i];
            var esperado = caso.Esperado.Itens[i];
            obtido.CodigoProduto.Should().Be(esperado.CodigoProduto);
            obtido.PercentualDesconto.Should().Be(esperado.PercentualDesconto);
            obtido.ValorDesconto.Should().Be(esperado.ValorDesconto);
            obtido.ValorItem.Should().Be(esperado.ValorItem);
            obtido.AliquotaIcms.Should().Be(esperado.AliquotaIcms);
            obtido.ValorIcms.Should().Be(esperado.ValorIcms);
        }
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void Baixa_de_estoque_reproduz_quebra_e_reposicao_quando_o_caso_informa(string id, CasoParidade caso)
    {
        _ = id;
        if (caso.Estoque is null)
            return;

        var produto = Catalogo.Produto(caso.Estoque.CodigoProduto);
        produto.Estoque.Should().Be(caso.Estoque.EstoqueInicial);

        var baixa = ProcessadorBaixaEstoque.Aplicar(produto, caso.Estoque.QuantidadeVendida, DateTime.Today);

        baixa.AplicouQuebra.Should().Be(caso.Estoque.AplicouQuebra);
        baixa.EstoqueNovo.Should().Be(caso.Estoque.EstoqueFinal);
        if (caso.Estoque.QuantidadeSugerida is null)
            baixa.Sugestao.Should().BeNull();
        else
            baixa.Sugestao!.QuantidadeSugerida.Should().Be(caso.Estoque.QuantidadeSugerida.Value);
    }

    private static FixtureParidade Carregar()
    {
        var caminho = LocalizarFixture();
        var json = File.ReadAllText(caminho);
        return JsonSerializer.Deserialize<FixtureParidade>(json, JsonOpts)
               ?? throw new InvalidOperationException("Fixture de paridade vazia.");
    }

    private static string LocalizarFixture()
    {
        var candidatos = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "fixtures", "paridade-legado.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "paridade-legado.json"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "fixtures", "paridade-legado.json"))
        };
        return candidatos.FirstOrDefault(File.Exists)
               ?? throw new FileNotFoundException("paridade-legado.json não encontrado. Confira CopyToOutputDirectory.");
    }

    private static class Catalogo
    {
        public static Produto Produto(string codigo) => codigo switch
        {
            "0001" => new Produto { Codigo = "0001", Descricao = "ARROZ TIPO 1 5KG",     Ncm = "10063021", Preco = 24.90m, Estoque = 50m, EstoqueMinimo = 20m, Ativo = true },
            "0002" => new Produto { Codigo = "0002", Descricao = "FEIJAO CARIOCA 1KG",   Ncm = "07133319", Preco = 8.50m,  Estoque = 30m, EstoqueMinimo = 15m, Ativo = true },
            "0003" => new Produto { Codigo = "0003", Descricao = "PAO DE FORMA 500G",    Ncm = "19059090", Preco = 7.99m,  Estoque = 40m, EstoqueMinimo = 25m, Ativo = true },
            "0004" => new Produto { Codigo = "0004", Descricao = "REFRIGERANTE COLA 2L", Ncm = "22021000", Preco = 6.50m,  Estoque = 18m, EstoqueMinimo = 24m, Ativo = true },
            "0005" => new Produto { Codigo = "0005", Descricao = "CAFE TORRADO 500G",    Ncm = "09012100", Preco = 16.90m, Estoque = 8m,  EstoqueMinimo = 10m, Ativo = true },
            "0006" => new Produto { Codigo = "0006", Descricao = "AGUA MINERAL 1,5L",    Ncm = "22019000", Preco = 6.25m,  Estoque = 25m, EstoqueMinimo = 12m, Ativo = true },
            "0007" => new Produto { Codigo = "0007", Descricao = "ACUCAR REFINADO 1KG",  Ncm = "17019900", Preco = 4.19m,  Estoque = 60m, EstoqueMinimo = 0m,  Ativo = true },
            _ => throw new ArgumentOutOfRangeException(nameof(codigo), codigo, "Produto de homologação desconhecido.")
        };

        public static Cliente Cliente(string codigo) => codigo switch
        {
            "0001" => new Cliente { Codigo = "0001", Nome = "MERCADO SAO JOAO LTDA",     Tipo = TipoCliente.Varejista,   Uf = "SP" },
            "0002" => new Cliente { Codigo = "0002", Nome = "SUPERMERCADO PARANA LTDA",  Tipo = TipoCliente.Varejista,   Uf = "PR" },
            "0003" => new Cliente { Codigo = "0003", Nome = "MARIA CONSUMIDORA",         Tipo = TipoCliente.Consumidor,  Uf = "SP" },
            "0004" => new Cliente { Codigo = "0004", Nome = "PEDRO FUNCIONARIO",         Tipo = TipoCliente.Funcionario, Uf = "SP" },
            "0005" => new Cliente { Codigo = "0005", Nome = "COMERCIAL MINAS ALIMENTOS", Tipo = TipoCliente.Varejista,   Uf = "MG" },
            _ => throw new ArgumentOutOfRangeException(nameof(codigo), codigo, "Cliente de homologação desconhecido.")
        };
    }
}

public sealed class FixtureParidade
{
    public List<CasoParidade> Casos { get; set; } = new();
}

public sealed class CasoParidade
{
    public string Id { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public List<string> Fontes { get; set; } = new();
    public string Cliente { get; set; } = string.Empty;
    public List<ItemEntrada> Itens { get; set; } = new();
    public EsperadoPedido Esperado { get; set; } = new();
    public EsperadoEstoque? Estoque { get; set; }
}

public sealed class ItemEntrada
{
    public string CodigoProduto { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
}

public sealed class EsperadoPedido
{
    public string Cfop { get; set; } = string.Empty;
    public decimal ValorBruto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorDescontoFuncionario { get; set; }
    public decimal ValorIcms { get; set; }
    public decimal ValorTotal { get; set; }
    public List<EsperadoItem> Itens { get; set; } = new();
}

public sealed class EsperadoItem
{
    public int Item { get; set; }
    public string CodigoProduto { get; set; } = string.Empty;
    public decimal PercentualDesconto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorItem { get; set; }
    public decimal AliquotaIcms { get; set; }
    public decimal ValorIcms { get; set; }
}

public sealed class EsperadoEstoque
{
    public string CodigoProduto { get; set; } = string.Empty;
    public decimal EstoqueInicial { get; set; }
    public decimal QuantidadeVendida { get; set; }
    public decimal EstoqueFinal { get; set; }
    public bool AplicouQuebra { get; set; }
    public decimal? QuantidadeSugerida { get; set; }
}
