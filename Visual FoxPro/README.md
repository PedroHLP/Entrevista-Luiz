# Gestcom (legado) — Visual FoxPro 9

Este é o **sistema legado** de pedidos e estoque que a Quality vai modernizar. O código é realista (sintaxe VFP 9, tabelas livres `.dbf`, notação húngara, comentários no estilo da equipe de 2009–2019), mas **não precisa ser executado** nesta demonstração. A referência para rodar e testar é a pasta `.NET`.

## O que o módulo faz

Controle de estoque e pedido de venda de um varejo/supermercado, no espírito do Gestcom citado na vaga:

- cadastro de produtos (código, descrição, NCM, preço, estoque, estoque mínimo);
- inclusão de pedido com validação de estoque;
- desconto por faixa de quantidade (cliente varejista);
- cálculo de ICMS e CFOP (empresa em SP: 5102 / 18% interno, 6102 / 12% interestadual);
- sugestão automática de reposição (base do pedido de compra) quando o estoque cai abaixo do mínimo.

## Estrutura

| Arquivo | Papel |
|---|---|
| `menu.prg` | Menu principal |
| `criatabelas.prg` | `CREATE TABLE … FREE` das DBFs + índices |
| `dados_iniciais.prg` | Carga de homologação e exportação `TYPE CSV` |
| `cad_produtos.prg` | Cadastro de produtos |
| `ped_inclui.prg` | Inclusão do pedido (fluxo principal) |
| `funcoes.prg` | `RoundGestcom`, `Trunca2`, `PercDescontoFaixa`, abertura de tabelas |
| `spc_calc_icms.prg` | “Stored procedure” de ICMS/CFOP |
| `spc_desc_funcionario.prg` | “Stored procedure” de desconto de funcionário |
| `trg_baixa_estoque.prg` | “Trigger” de baixa de estoque e sugestão de reposição |
| `rel_reposicao.prg` | Relatório da sugestão (pedido de compra) |
| `dados/*.csv` | Exportação das tabelas, entrada da migração para o .NET |

## Tabelas

- **PRODUTOS** — `CODPROD`, `DESCRICAO`, `NCM`, `PRECO`, `ESTOQUE`, `ESTMIN`, `ATIVO`
- **CLIENTES** — `CODCLI`, `NOME`, `TIPO` (`C` consumidor, `V` varejista, `F` funcionário), `UF`
- **PEDIDOS** — cabeçalho, totais, `CFOP`, `STATUS` (`A` aberto / `F` fechado)
- **ITEMPED** — itens (preço, desconto, alíquota e valor de ICMS)
- **SUGREPOS** — sugestão de reposição

## Regras visíveis no código “oficial”

1. Desconto de varejista no **item**: 5% a partir de 6 unidades, 10% a partir de 12. Não é cumulativo. Consumidor e funcionário não usam essa faixa.
2. O **valor** do desconto de faixa é truncado com `INT(n * 100) / 100` (nunca arredonda para cima). Pedido do comercial em 2011.
3. Totais de item usam `ROUND(valor + 0.00001, 2)` — o `+0.00001` existe desde 2009 para “corrigir” ponto flutuante. Não é o `ROUND` padrão do .NET (que usa *banker's rounding*).
4. ICMS: `ROUND(base * aliquota / 100, 2)` **sem** o `+0.00001`. Base = valor líquido do item.
5. A validação de estoque acontece **antes** de gravar o item. Só rejeita se `QTD > ESTOQUE`.
6. Sugestão de reposição: `QTDSUG = ESTMIN * 2 - ESTOQUE`, apenas se `ESTMIN > 0` e estoque atual `< ESTMIN`.

## Particularidades **não documentadas** no manual

Estas regras estão escondidas em rotinas que funcionam como trigger/procedure. Foram descobertas na engenharia reversa (ver `../.NET/docs/regras-de-negocio.md` e `../.NET/prompts/`). O .NET **reproduz o comportamento de propósito**, para manter paridade com o legado.

1. **Alíquota reduzida de padaria** (`spc_calc_icms.prg`, bloco “circular 14/2012”). Se o NCM começa com `1905`, a alíquota de ICMS é **7%**, qualquer que seja a UF. O CFOP do cabeçalho **não muda** (continua 5102 ou 6102 conforme a UF).
2. **Desconto de funcionário** (`spc_desc_funcionario.prg`, chamado 2281). Cliente tipo `F`, se o subtotal (bruto − desconto já aplicado) for **≥ R$ 50**, aplica mais **5%** sobre esse subtotal. É desconto de **cabeçalho**: o ICMS dos itens **não é recalculado**.
3. **Quebra operacional de bebidas** (`trg_baixa_estoque.prg`, chamado 4412 / 2016). Se o NCM começa com `2202`, a baixa retira **1 unidade a mais** do estoque. Essa unidade extra **não entra** na validação de estoque do `ped_inclui.prg`. Consequência: vender o estoque inteiro de um refrigerante deixa o saldo em **−1**.
4. A sugestão de reposição usa o estoque **já depois da quebra**. Vender 5 unidades do produto `0004` (estoque 18, mínimo 24) deixa 12 (5 + 1 de quebra) e sugere **36**.

## Dados de homologação

| Código | Produto | NCM | Preço | Estoque | Mínimo | Destaque |
|---|---|---|---:|---:|---:|---|
| 0001 | Arroz tipo 1 5 kg | 10063021 | 24,90 | 50 | 20 | faixa de desconto |
| 0002 | Feijão carioca 1 kg | 07133319 | 8,50 | 30 | 15 | — |
| 0003 | Pão de forma 500 g | 19059090 | 7,99 | 40 | 25 | ICMS 7% |
| 0004 | Refrigerante cola 2 L | 22021000 | 6,50 | 18 | 24 | quebra + reposição |
| 0005 | Café torrado 500 g | 09012100 | 16,90 | 8 | 10 | já abaixo do mínimo |
| 0006 | Água mineral 1,5 L | 22019000 | 6,25 | 25 | 12 | arredondamento 1,125 → 1,13 |
| 0007 | Açúcar refinado 1 kg | 17019900 | 4,19 | 60 | 0 | nunca gera sugestão |

Clientes: Mercado São João (V/SP), Supermercado Paraná (V/PR), Maria Consumidora (C/SP), Pedro Funcionário (F/SP), Comercial Minas (V/MG).

## Como este código seria compilado no VFP 9

```foxpro
DO criatabelas
DO dados_iniciais
DO menu
```

Nesta pasta de demonstração o FoxPro **não roda**. A paridade com o .NET está nos testes (`dotnet test`) e no arquivo `../.NET/tests/Gestcom.ParityTests/fixtures/paridade-legado.json`.
