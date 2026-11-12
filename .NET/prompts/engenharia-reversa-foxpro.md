# Prompt — engenharia reversa do Gestcom (FoxPro)

Use este prompt (ou uma variação) ao pedir para a IA ler o legado.

```
Você está migrando o módulo de pedidos e estoque do Gestcom (Visual FoxPro 9) para .NET 8.

Leia TODOS os .prg desta pasta, inclusive os que parecem "rotina interna"
(prefixos spc_ e trg_). Para cada arquivo, liste:

1. Entradas, saídas e efeitos colaterais (REPLACE em estoque, APPEND em
   sugestão de reposição, etc.).
2. Regras de cálculo com a fórmula exata (ROUND, INT, +0.00001, LEFT(ncm,4)).
3. Ordem de execução (o que roda antes de gravar o item, o que roda só no
   fechamento do pedido).
4. Qualquer bloco com comentário do tipo "não retirar", "não exibir em tela",
   "circular", "chamado" — trate como regra oculta até prova em contrário.
5. O que o manual provavelmente NÃO descreve.

Não "corrija" o comportamento. Documente primeiro, proponha o equivalente
em .NET depois, e indique testes de paridade (entradas e saídas esperadas).

Comece por ped_inclui.prg, funcoes.prg, spc_calc_icms.prg,
spc_desc_funcionario.prg e trg_baixa_estoque.prg.
```

## Resultado desta passagem

Foi assim que a quebra de NCM 2202 (chamado 4412) e a alíquota de 7% do NCM 1905 (circular 14/2012) apareceram. A primeira leitura só do `ped_inclui.prg` não era suficiente.
