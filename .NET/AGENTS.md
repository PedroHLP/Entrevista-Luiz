# AGENTS.md — Gestcom (migração FoxPro → .NET)

Instruções para agentes de programação (Cursor e similares) que trabalhem neste repositório.

## Objetivo

Reconstruir em .NET 8 o módulo de pedidos e estoque do Gestcom (Visual FoxPro 9), **com paridade** das regras de negócio, inclusive as que estão escondidas em “triggers” e “procedures”. Só depois de haver testes vermelhos/verdes para uma regra é permitido discuti-la como melhoria.

Há também um front em `web/` (React + Vite). Ele só consome a API; não reimplementa regra de negócio. Convenções em `.cursor/rules/frontend.mdc`.

## O que ler primeiro

1. `../Visual FoxPro/README.md` e os `.prg` citados.
2. `docs/regras-de-negocio.md` — mapa regra → fonte → BDD → teste.
3. `docs/decisoes.md` — o que a IA errou e o que foi corrigido na revisão.
4. As regras em `.cursor/rules/*.mdc`.
5. Os prompts de `prompts/` antes de reler o legado.

## Como trabalhar

- Não “melhorar” arredondamento, ICMS ou estoque sem um teste de paridade que mostre o comportamento atual e uma decisão explícita em `docs/decisoes.md`.
- Toda regra nova precisa de: (a) citação do `.prg` e da linha/rotina; (b) cenário em `tests/Gestcom.BddTests/Features` em português; (c) entrada em `docs/regras-de-negocio.md`.
- Código de domínio não referencia EF, HTTP nem CSV.
- Nomes de tabela/coluna do banco seguem o legado (`PRODUTOS`, `CODPROD`, `ESTMIN`). No C#, use português para o negócio (`EstoqueMinimo`) e inglês para tipos técnicos (`Repository` já foi evitado em favor de `Repositorio`, por consistência com o restante).
- Antes de abrir PR: `dotnet test` na solução tem que passar, inclusive os cenários Reqnroll.

## Comandos

```bash
dotnet build
dotnet test
dotnet run --project src/Gestcom.Migrator -- --fonte "../Visual FoxPro/dados"
dotnet run --project src/Gestcom.Api
```

A API sobe em `http://localhost:5088` (Swagger em `/swagger`) e já carrega os dados de homologação. O front sobe com `npm install && npm run dev` em `web/`, em `http://localhost:5173`.
