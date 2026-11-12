# Prompt — gerar cenários BDD a partir do legado

```
Com base em docs/regras-de-negocio.md e nos .prg do Visual FoxPro, escreva
arquivos .feature em português (# language: pt) para Reqnroll.

Regras:
- Um arquivo por tema (desconto, ICMS, funcionário, estoque/reposição).
- Cada Cenário deve ser executável com os produtos e clientes de
  dados_iniciais.prg (códigos 0001–0007 e 0001–0005).
- Use valores no formato 298,80.
- Inclua pelo menos um cenário para cada particularidade oculta
  (NCM 1905, NCM 2202, desconto de funcionário com ICMS não recalculado,
  arredondamento 1,125 → 1,13, ESTMIN = 0).
- Não descreva cliques de tela. Descreva o resultado do cálculo ou da baixa.
- Depois de gerar, atualize a coluna "Cenário BDD" em
  docs/regras-de-negocio.md.
```
