# Rastreabilidade das regras de negócio

Cada regra abaixo liga o código FoxPro de origem, o cenário BDD e o teste automatizado. É o equivalente, nesta demonstração, ao card do Azure DevOps apontando para especificação, código e teste.

A UF da empresa no legado é **SP** (constante em `spc_calc_icms.prg` e `RegrasFiscais.UfEmpresa`).

## Regras oficiais (visíveis no manual / no fluxo principal)

| ID | Regra | Fonte FoxPro | Cenário BDD | Teste |
|---|---|---|---|---|
| RN-01 | Varejista: 5% a partir de 6 un., 10% a partir de 12 un. Faixas não cumulativas. Consumidor e funcionário não usam essa faixa. | `funcoes.prg` — `PercDescontoFaixa` | `desconto-por-faixa.feature` — “Varejista de SP com 12 unidades…” e “Consumidor não recebe desconto…” | `RegrasDescontoTests`, `CalculadoraPedidoTests.Varejista_sp_12_arroz…`, paridade **PED-001** |
| RN-02 | O **valor** do desconto de faixa é truncado com `INT(n * 100) / 100` (nunca arredonda para cima). 2,397 vira **2,39**, não 2,40. | `funcoes.prg` — `Trunca2`; uso em `ped_inclui.prg` | `desconto-por-faixa.feature` — “Varejista do Paraná com 6 pães trunca o desconto em 2,39” | `ArredondamentoLegadoTests.Trunca2_nao_arredonda_para_cima`, paridade **PED-002** |
| RN-03 | Totais de item usam `ROUND(valor + 0.00001, 2)` (`RoundGestcom`). O `+0.00001` existe desde 2009 para compensar ponto flutuante. | `funcoes.prg` — `RoundGestcom` | Cálculo de todos os cenários de pedido | `ArredondamentoLegadoTests.RoundGestcom_aplica_o_ajuste…` |
| RN-04 | Mesma UF da empresa (SP): CFOP **5102**, ICMS **18%**. Outra UF: CFOP **6102**, ICMS **12%**. Base = valor líquido do item. ICMS = `ROUND(base * aliq / 100, 2)` **sem** o `+0.00001`. | `spc_calc_icms.prg`; CFOP do cabeçalho também em `ped_inclui.prg` | `icms-e-cfop.feature` — arroz para Minas; cenários de SP | `RegrasFiscaisTests`, paridade **PED-001** e **PED-008** |
| RN-05 | Validação de estoque ocorre **antes** de gravar o item: rejeita se `QTD > ESTOQUE`. A quebra operacional não entra nesta checagem. | `ped_inclui.prg` (bloco “Estoque insuficiente”) | `estoque-e-reposicao.feature` — “Pedido acima do estoque disponível é rejeitado…” | `ProcessadorBaixaEstoqueTests.Validacao_aceitaria_os_18…` |
| RN-06 | Sugestão de reposição: `QTDSUG = ESTMIN * 2 - ESTOQUE`, somente se `ESTMIN > 0` e estoque atual `< ESTMIN`. Usa o saldo **depois** da baixa (e da quebra). | `trg_baixa_estoque.prg`; relatório em `rel_reposicao.prg` | `estoque-e-reposicao.feature` — venda de 5 refrigerantes e açúcar com mínimo zero | `ProcessadorBaixaEstoqueTests`, paridade **PED-006** |

## Particularidades não documentadas no manual (reproduzidas de propósito)

| ID | Particularidade | Onde estava escondida | Efeito prático | Cenário BDD | Teste |
|---|---|---|---|---|---|
| RN-07 | **Alíquota reduzida de padaria.** Se o NCM começa com `1905`, ICMS = **7%**, qualquer UF. O CFOP do cabeçalho **não muda**. | `spc_calc_icms.prg`, bloco “circular 14/2012” | 6 pães para o PR: CFOP 6102, alíquota 7%, ICMS 3,19 | `icms-e-cfop.feature` — “Pão em SP usa alíquota reduzida…” e cenário dos pães no PR | `RegrasFiscaisTests.Ncm_1905_tem_aliquota_7…`, paridade **PED-002** e **PED-004** |
| RN-08 | **Desconto de funcionário (tipo F).** Se o subtotal (bruto − desconto já aplicado) ≥ **R$ 50**, aplica mais **5%** sobre esse subtotal, no **cabeçalho**. O ICMS dos itens **não é recalculado**. | `spc_desc_funcionario.prg` (chamado 2281 / DP) | 4 cafés: total 64,22, mas ICMS 12,17 (sobre 67,60), não 11,56 | `desconto-funcionario.feature` | `CalculadoraPedidoTests.Funcionario_4_cafes…`, paridade **PED-003** e **PED-004** |
| RN-09 | **Quebra operacional de bebidas.** Se o NCM começa com `2202`, a baixa retira **1 unidade a mais**. Não aparece em tela e não entra na validação de estoque. Vender o estoque inteiro deixa saldo **−1**. | `trg_baixa_estoque.prg`, bloco “chamado 4412 / 2016” | 5 refrigerantes: estoque 18 → 12; sugestão 36. 18 refrigerantes: estoque → −1; sugestão 49 | `estoque-e-reposicao.feature` | `ProcessadorBaixaEstoqueTests`, paridade **PED-006** e **PED-007** |
| RN-10 | **Arredondamento do meio (x,xx5) para longe de zero.** O `ROUND` do VFP não usa *banker's rounding*. 1,125 vira **1,13**, não 1,12. | Comportamento nativo do `ROUND()` no VFP 9; usado em `spc_calc_icms.prg` | 1 água de R$ 6,25: ICMS 1,13 | `icms-e-cfop.feature` — “Água em SP arredonda 1,125 para 1,13…” | `ArredondamentoLegadoTests.RoundVfp_meio_vai_para_longe_de_zero…`, paridade **PED-005** |

## Ordem de cálculo (igual ao `ped_inclui.prg`)

1. Bruto do item = `RoundGestcom(preço × quantidade)`.
2. Percentual de faixa (só varejista) e valor truncado (`Trunca2`).
3. Líquido do item = `RoundGestcom(bruto − desconto)`.
4. Alíquota e ICMS do item (`spc_calc_icms`, com a exceção do NCM 1905).
5. Soma dos totais no cabeçalho.
6. Desconto de funcionário no cabeçalho, **sem** recalcular ICMS.
7. Total = `RoundGestcom(bruto − desconto)`.
8. Ao fechar: `trg_baixa_estoque` (quebra 2202 + sugestão de reposição). O pedido vai para status `F`.

## Dados de homologação usados nos testes

Ver `Visual FoxPro/dados/*.csv` e `Visual FoxPro/dados_iniciais.prg`. Os produtos 0003 (pão, NCM 1905), 0004 (refrigerante, NCM 2202) e 0006 (água, preço 6,25) existem especificamente para expor RN-07, RN-09 e RN-10.
