using Gestcom.Api;
using Gestcom.Application;
using Gestcom.Domain.Excecoes;
using Gestcom.Infrastructure;
using Gestcom.Infrastructure.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Gestcom API",
        Version = "v1",
        Description = "Migração do módulo de pedidos e estoque do Visual FoxPro 9 para ASP.NET Core 8. " +
                      "Demonstração para a entrevista com a Quality Digital (modernização do legado com apoio de IA)."
    });
});

const string CorsFrontend = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsFrontend, policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddGestcomApplication();
builder.Services.AddGestcomInfra(builder.Configuration);

var app = builder.Build();

app.UseCors(CorsFrontend);

app.UseExceptionHandler(errorApp =>
{
    errorApp.UseCors(CorsFrontend);
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var ex = feature?.Error;
        context.Response.ContentType = "application/json";
        switch (ex)
        {
            case EstoqueInsuficienteException estoque:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new
                {
                    erro = estoque.Message,
                    codigoProduto = estoque.CodigoProduto,
                    disponivel = estoque.EstoqueDisponivel
                });
                break;
            case CadastroNaoEncontradoException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new { erro = ex!.Message });
                break;
            case PedidoInvalidoException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { erro = ex!.Message });
                break;
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { erro = "Erro interno ao processar a requisição." });
                break;
        }
    });
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gestcom API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Gestcom — migração FoxPro → .NET";
});

app.UseAuthorization();
app.MapControllers();
app.MapGet("/saude", () => Results.Ok(new { status = "ok", sistema = "Gestcom", origem = "migracao-foxpro" }));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GestcomDbContext>();
    await CargaHomologacao.GarantirAsync(db);
    if (app.Configuration.GetValue("Demo:PedidosExemplo", false))
        await CargaDemonstracao.GarantirAsync(scope.ServiceProvider);
}

app.Run();

public partial class Program { }
