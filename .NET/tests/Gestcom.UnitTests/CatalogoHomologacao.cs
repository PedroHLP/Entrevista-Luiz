using Gestcom.Domain.Entidades;

namespace Gestcom.UnitTests;

/// <summary>Mesmos dados de dados_iniciais.prg / dados/*.csv.</summary>
public static class CatalogoHomologacao
{
    public static Produto Produto(string codigo) => Produtos().First(p => p.Codigo == codigo);

    public static Cliente Cliente(string codigo) => Clientes().First(c => c.Codigo == codigo);

    public static IReadOnlyList<Produto> Produtos() => new[]
    {
        new Produto { Codigo = "0001", Descricao = "ARROZ TIPO 1 5KG",     Ncm = "10063021", Preco = 24.90m, Estoque = 50m, EstoqueMinimo = 20m, Ativo = true },
        new Produto { Codigo = "0002", Descricao = "FEIJAO CARIOCA 1KG",   Ncm = "07133319", Preco = 8.50m,  Estoque = 30m, EstoqueMinimo = 15m, Ativo = true },
        new Produto { Codigo = "0003", Descricao = "PAO DE FORMA 500G",    Ncm = "19059090", Preco = 7.99m,  Estoque = 40m, EstoqueMinimo = 25m, Ativo = true },
        new Produto { Codigo = "0004", Descricao = "REFRIGERANTE COLA 2L", Ncm = "22021000", Preco = 6.50m,  Estoque = 18m, EstoqueMinimo = 24m, Ativo = true },
        new Produto { Codigo = "0005", Descricao = "CAFE TORRADO 500G",    Ncm = "09012100", Preco = 16.90m, Estoque = 8m,  EstoqueMinimo = 10m, Ativo = true },
        new Produto { Codigo = "0006", Descricao = "AGUA MINERAL 1,5L",    Ncm = "22019000", Preco = 6.25m,  Estoque = 25m, EstoqueMinimo = 12m, Ativo = true },
        new Produto { Codigo = "0007", Descricao = "ACUCAR REFINADO 1KG",  Ncm = "17019900", Preco = 4.19m,  Estoque = 60m, EstoqueMinimo = 0m,  Ativo = true },
    };

    public static IReadOnlyList<Cliente> Clientes() => new[]
    {
        new Cliente { Codigo = "0001", Nome = "MERCADO SAO JOAO LTDA",     Tipo = TipoCliente.Varejista,    Uf = "SP" },
        new Cliente { Codigo = "0002", Nome = "SUPERMERCADO PARANA LTDA",  Tipo = TipoCliente.Varejista,    Uf = "PR" },
        new Cliente { Codigo = "0003", Nome = "MARIA CONSUMIDORA",         Tipo = TipoCliente.Consumidor,   Uf = "SP" },
        new Cliente { Codigo = "0004", Nome = "PEDRO FUNCIONARIO",         Tipo = TipoCliente.Funcionario,  Uf = "SP" },
        new Cliente { Codigo = "0005", Nome = "COMERCIAL MINAS ALIMENTOS", Tipo = TipoCliente.Varejista,    Uf = "MG" },
    };
}
