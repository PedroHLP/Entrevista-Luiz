using Gestcom.Application;
using Gestcom.Infrastructure;
using Gestcom.Infrastructure.Persistencia;
using Gestcom.Migrator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var fonte = ObterValor(args, "--fonte") 
    ?? CaminhoPadraoFonte();
var provider = ObterValor(args, "--provider") ?? "Sqlite";
var conexao = ObterValor(args, "--conexao") ?? "Data Source=gestcom.db";

Console.WriteLine("Gestcom — migrador DBF/CSV → SQL");
Console.WriteLine($"  Fonte : {fonte}");
Console.WriteLine($"  Banco : {provider} ({conexao})");

if (!Directory.Exists(fonte))
{
    Console.Error.WriteLine($"Pasta de origem não encontrada: {fonte}");
    return 1;
}

var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Database:Provider"] = provider,
        ["Database:ConnectionString"] = conexao
    })
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddGestcomApplication();
services.AddGestcomInfra(configuration);
services.AddSingleton<ImportadorCsv>();
var sp = services.BuildServiceProvider();

using var scope = sp.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<GestcomDbContext>();
await db.Database.EnsureCreatedAsync();

var importador = scope.ServiceProvider.GetRequiredService<ImportadorCsv>();
var resumo = await importador.ImportarAsync(fonte);

Console.WriteLine();
Console.WriteLine("Importação concluída:");
Console.WriteLine($"  Produtos  : {resumo.Produtos}");
Console.WriteLine($"  Clientes  : {resumo.Clientes}");
Console.WriteLine($"  Pedidos   : {resumo.Pedidos}");
Console.WriteLine($"  Itens     : {resumo.Itens}");
Console.WriteLine($"  Sugestões : {resumo.Sugestoes}");
return 0;

static string? ObterValor(string[] args, string nome)
{
    var i = Array.FindIndex(args, a => string.Equals(a, nome, StringComparison.OrdinalIgnoreCase));
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}

static string CaminhoPadraoFonte()
{
    // Parte de src/Gestcom.Migrator (ou do diretório de execução) até "../Visual FoxPro/dados"
    var atuais = new[]
    {
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Visual FoxPro", "dados")),
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Visual FoxPro", "dados")),
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Visual FoxPro", "dados")),
    };
    return atuais.FirstOrDefault(Directory.Exists) ?? atuais[0];
}
