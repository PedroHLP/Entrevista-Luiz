# Gestcom .NET 8 — migração do módulo de pedidos e estoque

Reconstrução em ASP.NET Core do mesmo módulo que está na pasta `../Visual FoxPro`, agora com uma tela web para mostrar o fluxo ao gestor. O objetivo da demo é o da vaga: extrair regra do legado, escrever a especificação em BDD, implementar em .NET com paridade, testar e deixar rastreável.

## O que tem aqui

| Pasta / arquivo | Papel |
|---|---|
| `src/Gestcom.Domain` | Regras (arredondamento, desconto, ICMS, estoque) e cálculo do pedido |
| `src/Gestcom.Application` | Casos de uso: registrar pedido, catálogo, reposição |
| `src/Gestcom.Infrastructure` | EF Core (Sqlite por padrão; SQL Server ou InMemory por configuração) |
| `src/Gestcom.Api` | Web API + Swagger. Na primeira subida já cria o banco e carrega os dados de homologação |
| `src/Gestcom.Migrator` | Console opcional que relê os CSV exportados das DBFs |
| `web/` | Front React + Vite + TypeScript (telas equivalentes às do FoxPro) |
| `tests/Gestcom.UnitTests` | Testes de unidade das regras |
| `tests/Gestcom.BddTests` | Reqnroll, features em português (`# language: pt`) |
| `tests/Gestcom.ParityTests` | Compara o .NET com a fixture `paridade-legado.json` |
| `docs/regras-de-negocio.md` | Rastreabilidade: regra → `.prg` → cenário BDD → teste |
| `docs/decisoes.md` | O que a IA gerou e o que a revisão corrigiu |
| `.cursor/rules/` | Regras do Cursor (migração, BDD, revisão de IA, nomenclatura, front-end) |
| `prompts/` | Prompts usados na engenharia reversa |
| `AGENTS.md` | Instruções para o agente de programação |
| `azure-pipelines.yml` | Esteira restore / build / test |

## Pré-requisito

SDK **.NET 8** (ou superior). O `global.json` pede 8.0.428 com `rollForward: latestMajor`, e o `Directory.Build.props` define `<RollForward>Major</RollForward>`, então o SDK 10 com runtime 8 também roda o projeto.

No front: **Node.js** (npm) para `npm install` e `npm run dev`.

## Como rodar a demonstração

Dois terminais, a partir da pasta `.NET/`. **Não é preciso rodar o Migrator** na primeira vez: a API cria o `gestcom.db` e carrega os 7 produtos e 5 clientes sozinha.

**Terminal 1 — API**

```bash
dotnet run --project src/Gestcom.Api
```

A API sobe em [http://localhost:5088](http://localhost:5088) (Swagger em `/swagger`). O CORS libera o front em `http://localhost:5173`.

**Terminal 2 — tela web**

```bash
cd web
npm install
npm run dev
```

Abra [http://localhost:5173](http://localhost:5173). As telas são Menu, Produtos, Novo pedido e Reposição.

Para só conferir o código, sem abrir o navegador:

```bash
dotnet build
dotnet test
cd web && npm install && npm run build
```

Os três projetos de teste precisam passar: unidade, BDD (Reqnroll) e paridade (60 testes no total).

### Migrador (opcional)

Use se quiser recarregar os CSV do legado por cima do banco:

```bash
dotnet run --project src/Gestcom.Migrator -- --fonte "../Visual FoxPro/dados"
```

Para SQL Server (ambiente da Quality):

```bash
dotnet run --project src/Gestcom.Migrator -- \
  --fonte "../Visual FoxPro/dados" \
  --provider SqlServer \
  --conexao "Server=localhost;Database=Gestcom;Trusted_Connection=True;TrustServerCertificate=True"
```

## Roteiro de 2 minutos para o Luiz

1. **Abrir o legado** (`../Visual FoxPro/ped_inclui.prg`) e mostrar o fluxo: valida estoque → desconto → ICMS → desconto de funcionário → baixa. Depois abrir `trg_baixa_estoque.prg` e apontar o bloco “chamado 4412”: a quebra de 1 unidade no NCM `2202` não aparece no manual nem na tela.
2. **Abrir** `docs/regras-de-negocio.md` e mostrar a linha RN-09: fonte FoxPro, cenário BDD e teste de paridade. Essa é a rastreabilidade (card → especificação → código → teste).
3. Se der tempo, um `dotnet test` rápido (ou o filtro da feature do refrigerante). Caso contrário, pule para a tela.
4. **Abrir o front** em [http://localhost:5173](http://localhost:5173). No Menu, mostrar que as opções 1, 2 e 3 são as mesmas do `menu.prg`. Ir em **Novo pedido**, escolher Maria Consumidora, lançar 5 unidades do refrigerante `0004` e fechar. A tela mostra `aplicouQuebra`, estoque de 18 para 12 e sugestão de reposição 36. Opcional: um pedido de 1 água (`0006`) e apontar ICMS **1,13** (o `Math.Round` padrão do .NET daria 1,12). Abrir **Reposição** para o relatório equivalente ao `rel_reposicao.prg`.
5. **Fechar com** `docs/decisoes.md` ou `.cursor/rules/revisao-codigo-ia.mdc`: a IA ajudou a ler o Fox e a gerar código, mas a revisão pegou *banker's rounding*, desconto truncado, ICMS de funcionário e a própria quebra escondida no trigger. É o “senso crítico” que a vaga pede.

O Swagger em `http://localhost:5088/swagger` continua disponível se quiser mostrar o contrato da API.

## Telas do front

| Tela | Rota | Equivale a |
|---|---|---|
| Menu principal | `/` | `menu.prg` |
| Cadastro de produtos (lista, inclusão e alteração) | `/produtos` | `cad_produtos.prg` |
| Inclusão de pedido (cliente, itens, totais, quebra, reposição) | `/pedidos` | `ped_inclui.prg`, `funcoes.prg`, `spc_calc_icms.prg`, `spc_desc_funcionario.prg`, `trg_baixa_estoque.prg` |
| Sugestão de reposição | `/reposicao` | `rel_reposicao.prg` |

Cada tela tem um recado **Legado vs novo** citando o `.prg` correspondente. Os totais do pedido vêm da API; o front não recalcula regra de negócio.

## Configuração do banco

Em `src/Gestcom.Api/appsettings.json`:

```json
"Database": {
  "Provider": "Sqlite",
  "ConnectionString": "Data Source=gestcom.db"
}
```

`Provider` aceita `Sqlite` (padrão), `SqlServer` ou `InMemory`.

## Particularidades do legado reproduzidas de propósito

- Desconto de faixa **truncado** (2,397 → 2,39).
- `ROUND` no estilo VFP (`AwayFromZero`) e `+0.00001` só nos totais de item.
- NCM `1905` com ICMS 7% (circular 14/2012).
- Desconto de funcionário no cabeçalho, sem recalcular ICMS.
- Quebra de 1 unidade em NCM `2202`, depois da validação de estoque.

Detalhes e casos numéricos: `docs/regras-de-negocio.md`.
