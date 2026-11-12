# Decisões técnicas e revisão do código gerado por IA

Esta pasta é um **exemplo de migração** feito com apoio de IA (Cursor), no espírito da vaga: a IA lê o legado, sugere especificação e código, e a pessoa desenvolvedora revisa com senso crítico antes de abrir o PR.

Abaixo, o que a IA gerou, o que a revisão corrigiu e o que foi mantido de propósito para preservar paridade com o FoxPro.

## O que a IA gerou (aceito depois da revisão)

- Extração inicial das regras de `ped_inclui.prg`, `funcoes.prg`, `spc_calc_icms.prg`, `spc_desc_funcionario.prg` e `trg_baixa_estoque.prg`.
- Esqueleto da solução .NET 8 (Domain, Application, Infrastructure, Api, Migrator).
- Feature files BDD em português (`# language: pt`) no formato Dado/Quando/Então.
- Camada de rastreabilidade (`docs/regras-de-negocio.md`) e o pipeline `azure-pipelines.yml`.
- Migrador CSV (exportação `COPY TO … TYPE CSV` do VFP) para Sqlite / SQL Server.
- Sugestão de usar EF Core com provedor configurável (`Database:Provider`).

## O que a revisão humana corrigiu

Estas correções são o tipo de coisa que a vaga pede: “revisar criticamente todo código gerado por IA”.

1. **Arredondamento *banker's* vs. ROUND do VFP.**  
   A primeira versão gerada usava `Math.Round(valor, 2)` sem modo — no .NET isso é `MidpointRounding.ToEven`. O caso da água (ICMS 1,125) ficaria **1,12** no .NET e **1,13** no FoxPro. Corrigido para `MidpointRounding.AwayFromZero`. Teste que trava a regressão: `ArredondamentoLegadoTests.RoundVfp_meio_vai_para_longe_de_zero…` e PED-005.

2. **O `+0.00001` só vale para totais de item.**  
   A IA aplicou `RoundGestcom` (com o ajuste) também no ICMS. No legado, `spc_calc_icms.prg` usa `ROUND` puro. A revisão separou `RoundGestcom` e `RoundIcms`.

3. **Desconto de faixa: `ROUND` vs. `INT`.**  
   A IA arredondou o valor do desconto. O comercial, em 2011, pediu truncamento (`Trunca2`). O caso dos 6 pães (2,397) falharia: 2,40 no .NET “ingênuo”, 2,39 no legado. Corrigido.

4. **Desconto de funcionário aplicado antes do ICMS.**  
   A primeira geração recalculava o ICMS depois do 5%. No legado a rotina roda **depois** dos itens e **não** mexa no imposto. 4 cafés teriam ICMS 11,56 em vez de **12,17**. Mantido o comportamento antigo e documentado em RN-08. Uma melhoria futura (recalcular o ICMS) seria mudança de regra, não migração.

5. **Quebra de NCM 2202 quase passou batido.**  
   Como está só no “trigger” (`trg_baixa_estoque.prg`), com comentário “não exibir em tela”, a primeira passagem pela pasta não listou essa regra. Foi encontrada numa segunda leitura, guiada pelo prompt de `prompts/engenharia-reversa-foxpro.md`. Sem isso, o estoque do refrigerante ficaria errado em 1 unidade e a sugestão de reposição também.

6. **Nomes de tabela e coluna.**  
   A IA quis anglicizar tudo (`Products`, `UnitPrice`). A revisão manteve os nomes do legado nas tabelas (`PRODUTOS`, `CODPROD`, `ESTMIN`) para a migração de dados ser direta, e usou português no domínio (`Produto.EstoqueMinimo`). Ver regra em `.cursor/rules/nomenclatura.mdc`.

7. **SQL Server na demo local.**  
   O `appsettings` gerado apontava só para SQL Server. A revisão deixou Sqlite como padrão (e InMemory como opção), com a connection string de SQL Server documentada para o ambiente da Quality.

## O que foi mantido de propósito (paridade primeiro)

- Validação de estoque **por item**, contra o saldo atual, **sem** acumular reserva quando o mesmo produto aparece duas vezes no pedido — é o que o `ped_inclui.prg` faz. Risco conhecido, documentado; correção seria melhoria posterior.
- Saldo negativo no NCM 2202 quando a venda zera o estoque (efeito colateral da quebra).
- ICMS “a maior” no pedido de funcionário (RN-08).
- CFOP do cabeçalho determinado só pela UF, mesmo quando um item tem alíquota especial.

## Próximos passos (fora do escopo desta demo)

- Discutir com o fiscal se a circular 14/2012 e o ICMS do desconto de funcionário devem ser corrigidos no sistema novo.
- Tornar visível a quebra operacional (hoje silenciosa) ou transformá-la em parâmetro por NCM.
- Acumular a reserva de estoque entre itens do mesmo pedido.
- BDD de ponta a ponta subindo a API (aqui os cenários exercitam o domínio, que é onde as regras vivem).
