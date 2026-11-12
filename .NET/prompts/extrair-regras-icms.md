# Prompt — extrair regras de ICMS e CFOP

```
Analise spc_calc_icms.prg e o trecho de ped_inclui.prg que grava o CFOP
do cabeçalho.

Quero uma tabela com: condição (UF, NCM), CFOP do item (se houver),
CFOP do cabeçalho, alíquota, fórmula do valor, e se o ROUND usa ou não
o +0.00001 do RoundGestcom.

Liste também os casos de teste mínimos que provam:

- venda interna (UF = SP);
- venda interestadual (UF = PR ou MG);
- um produto com NCM 1905 em SP e outro fora de SP (a alíquota cai para 7%,
  mas o CFOP do cabeçalho permanece 5102/6102);
- um item cujo ICMS dê exatamente x,xx5 (ex.: 6,25 * 18% = 1,125) para
  travar o arredondamento AwayFromZero.

Não misture o desconto de funcionário neste prompt — ele é outra rotina
e não recalcula o ICMS.
```
