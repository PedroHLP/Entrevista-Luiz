using System.Globalization;
using Gestcom.Domain.Entidades;
using Gestcom.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Gestcom.Migrator;

public sealed record ResumoImportacao(int Produtos, int Clientes, int Pedidos, int Itens, int Sugestoes);

/// <summary>
/// Lê os CSV exportados do Visual FoxPro (COPY TO ... TYPE CSV) e grava no banco novo.
/// Mantém os mesmos códigos, preços e saldos — o ponto de partida da paridade.
/// </summary>
public sealed class ImportadorCsv
{
    private readonly GestcomDbContext _db;
    public ImportadorCsv(GestcomDbContext db) => _db = db;

    public async Task<ResumoImportacao> ImportarAsync(string pasta, CancellationToken ct = default)
    {
        var produtos = Ler(Path.Combine(pasta, "produtos.csv"), MapProduto);
        var clientes = Ler(Path.Combine(pasta, "clientes.csv"), MapCliente);
        var pedidos = Ler(Path.Combine(pasta, "pedidos.csv"), MapPedido);
        var itens = Ler(Path.Combine(pasta, "itemped.csv"), MapItem);
        var sugestoes = Ler(Path.Combine(pasta, "sugrepos.csv"), MapSugestao);

        // Recarga limpa para a demonstração
        _db.ItensPedido.RemoveRange(_db.ItensPedido);
        _db.Pedidos.RemoveRange(_db.Pedidos);
        _db.SugestoesReposicao.RemoveRange(_db.SugestoesReposicao);
        _db.Produtos.RemoveRange(_db.Produtos);
        _db.Clientes.RemoveRange(_db.Clientes);
        await _db.SaveChangesAsync(ct);

        _db.Produtos.AddRange(produtos);
        _db.Clientes.AddRange(clientes);
        _db.Pedidos.AddRange(pedidos);
        _db.ItensPedido.AddRange(itens);
        _db.SugestoesReposicao.AddRange(sugestoes);
        await _db.SaveChangesAsync(ct);

        return new ResumoImportacao(produtos.Count, clientes.Count, pedidos.Count, itens.Count, sugestoes.Count);
    }

    private static List<T> Ler<T>(string caminho, Func<IReadOnlyDictionary<string, string>, T?> mapa)
    {
        if (!File.Exists(caminho))
            return new List<T>();

        var linhas = File.ReadAllLines(caminho)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();
        if (linhas.Count <= 1)
            return new List<T>();

        var cabecalho = PartirCsv(linhas[0]).Select(h => h.Trim().Trim('"').ToUpperInvariant()).ToList();
        var lista = new List<T>();
        foreach (var linha in linhas.Skip(1))
        {
            var cols = PartirCsv(linha);
            if (cols.Count == 0 || cols.All(string.IsNullOrWhiteSpace))
                continue;
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < cabecalho.Count && i < cols.Count; i++)
                dict[cabecalho[i]] = Desaspear(cols[i]);
            var item = mapa(dict);
            if (item is not null)
                lista.Add(item);
        }
        return lista;
    }

    private static Produto? MapProduto(IReadOnlyDictionary<string, string> r)
    {
        var codigo = r.Get("CODPROD");
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        return new Produto
        {
            Codigo = codigo.Trim(),
            Descricao = r.Get("DESCRICAO").Trim(),
            Ncm = r.Get("NCM").Trim(),
            Preco = Dec(r.Get("PRECO")),
            Estoque = Dec(r.Get("ESTOQUE")),
            EstoqueMinimo = Dec(r.Get("ESTMIN")),
            Ativo = Logico(r.Get("ATIVO"))
        };
    }

    private static Cliente? MapCliente(IReadOnlyDictionary<string, string> r)
    {
        var codigo = r.Get("CODCLI");
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        return new Cliente
        {
            Codigo = codigo.Trim(),
            Nome = r.Get("NOME").Trim(),
            Tipo = TipoClienteExtensoes.DeCodigo(r.Get("TIPO")),
            Uf = r.Get("UF").Trim().ToUpperInvariant()
        };
    }

    private static Pedido? MapPedido(IReadOnlyDictionary<string, string> r)
    {
        if (!int.TryParse(r.Get("NUMPED"), out var numero) || numero <= 0) return null;
        return new Pedido
        {
            Numero = numero,
            CodigoCliente = r.Get("CODCLI").Trim(),
            Data = Data(r.Get("DATA")),
            Status = string.IsNullOrWhiteSpace(r.Get("STATUS")) ? "F" : r.Get("STATUS").Trim(),
            ValorBruto = Dec(r.Get("VLRBRUTO")),
            ValorDesconto = Dec(r.Get("VLRDESC")),
            ValorIcms = Dec(r.Get("VLRICMS")),
            ValorTotal = Dec(r.Get("VLRTOTAL")),
            Cfop = r.Get("CFOP").Trim()
        };
    }

    private static ItemPedido? MapItem(IReadOnlyDictionary<string, string> r)
    {
        if (!int.TryParse(r.Get("NUMPED"), out var numero) || numero <= 0) return null;
        int.TryParse(r.Get("ITEM"), out var item);
        return new ItemPedido
        {
            NumeroPedido = numero,
            Item = item,
            CodigoProduto = r.Get("CODPROD").Trim(),
            Quantidade = Dec(r.Get("QTD")),
            Preco = Dec(r.Get("PRECO")),
            PercentualDesconto = Dec(r.Get("PERCDESC")),
            ValorDesconto = Dec(r.Get("VLRDESC")),
            ValorItem = Dec(r.Get("VLRITEM")),
            AliquotaIcms = Dec(r.Get("ALIQICMS")),
            ValorIcms = Dec(r.Get("VLRICMS"))
        };
    }

    private static SugestaoReposicao? MapSugestao(IReadOnlyDictionary<string, string> r)
    {
        var codigo = r.Get("CODPROD");
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        return new SugestaoReposicao
        {
            CodigoProduto = codigo.Trim(),
            QuantidadeAtual = Dec(r.Get("QTDATUAL")),
            QuantidadeMinima = Dec(r.Get("QTDMIN")),
            QuantidadeSugerida = Dec(r.Get("QTDSUG")),
            Data = Data(r.Get("DATA"))
        };
    }

    private static decimal Dec(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return 0m;
        valor = valor.Trim();
        if (decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var d))
            return d;
        if (decimal.TryParse(valor, NumberStyles.Number, new CultureInfo("pt-BR"), out d))
            return d;
        return 0m;
    }

    private static DateTime Data(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return DateTime.Today;
        var formatos = new[] { "MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };
        if (DateTime.TryParseExact(valor.Trim(), formatos, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var d))
            return d.Date;
        return DateTime.TryParse(valor, new CultureInfo("pt-BR"), DateTimeStyles.None, out d) ? d.Date : DateTime.Today;
    }

    private static bool Logico(string valor)
        => valor.Trim().StartsWith("T", StringComparison.OrdinalIgnoreCase)
           || valor.Trim() == "1"
           || valor.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)
           || valor.Trim().Equals(".T.", StringComparison.OrdinalIgnoreCase);

    private static string Desaspear(string valor)
    {
        valor = valor.Trim();
        if (valor.Length >= 2 && valor[0] == '"' && valor[^1] == '"')
            return valor[1..^1].Replace("\"\"", "\"");
        return valor;
    }

    private static List<string> PartirCsv(string linha)
    {
        var result = new List<string>();
        var atual = new System.Text.StringBuilder();
        var aspas = false;
        foreach (var ch in linha)
        {
            if (ch == '"')
            {
                aspas = !aspas;
                atual.Append(ch);
            }
            else if (ch == ',' && !aspas)
            {
                result.Add(atual.ToString());
                atual.Clear();
            }
            else
            {
                atual.Append(ch);
            }
        }
        result.Add(atual.ToString());
        return result;
    }
}

internal static class DicExt
{
    public static string Get(this IReadOnlyDictionary<string, string> d, string chave)
        => d.TryGetValue(chave, out var v) ? v : string.Empty;
}
