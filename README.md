# Entrevista Luiz — Quality Digital

Demonstração para a conversa de **quinta, 12/11, às 10h (horário de Brasília)**, sobre a vaga **Pessoa Desenvolvedora Sênior (Back-end)**: modernizar um sistema de varejo/supermercado em Visual FoxPro, reconstruindo em .NET com apoio de IA (Cursor), no espírito da metodologia Traction e do Gestcom.

As duas pastas implementam **a mesma funcionalidade**: pedido de venda com controle de estoque, desconto, ICMS/CFOP e sugestão automática de reposição (base do pedido de compra). O `.NET` agora também tem telas web equivalentes às do legado.

```
Entrevista Luiz/
├── README.md                 ← você está aqui
├── Visual FoxPro/            ← legado (não precisa rodar)
└── .NET/                     ← API, testes e front-end
    └── web/                  ← telas React (Menu, Produtos, Pedido, Reposição)
```

## Como subir para a conversa

Dois terminais, a partir de `.NET/`. A API já carrega os dados na primeira execução; o Migrator é opcional.

```bash
# Terminal 1
dotnet run --project src/Gestcom.Api

# Terminal 2
cd web
npm install
npm run dev
```

Abra o front em [http://localhost:5173](http://localhost:5173). A API (e o Swagger) fica em [http://localhost:5088](http://localhost:5088). O roteiro de 2 minutos está no `README.md` da pasta `.NET`.

No Windows, se o `dotnet` for o SDK 10, tudo bem: o `global.json` usa `rollForward: latestMajor`.

## O que mostrar em cada pasta

### `Visual FoxPro/`

Código no estilo VFP 9 (tabelas `FREE`, `.prg`, notação húngara, comentários de 2009–2019). **Não é necessário executar o FoxPro.** Serve para a engenharia reversa.

- Comece por `README.md` e `ped_inclui.prg` (fluxo principal).
- As regras “escondidas” estão em:
  - `spc_calc_icms.prg` — alíquota de 7% para NCM `1905` (circular 14/2012);
  - `spc_desc_funcionario.prg` — 5% no cabeçalho se subtotal ≥ 50, **sem** recalcular ICMS;
  - `trg_baixa_estoque.prg` — quebra de 1 unidade em NCM `2202` (chamado 4412), fora da validação de estoque.
- `dados/*.csv` é a exportação das DBFs, entrada do migrador .NET (opcional).

### `.NET/`

Solução `Gestcom.sln` (.NET 8) com Domain / Application / Infrastructure / API, front em `web/`, migrador CSV → banco, testes xUnit, BDD em português (Reqnroll) e testes de paridade.

Artefatos de IA, de propósito visíveis:

- `.cursor/rules/*.mdc` — convenções de migração, BDD, revisão de código de IA, nomenclatura e front-end;
- `prompts/` — prompts usados para ler o FoxPro, extrair ICMS e gerar os cenários;
- `AGENTS.md` — instruções para o agente;
- `docs/decisoes.md` — o que a IA gerou e o que a revisão humana corrigiu;
- `docs/regras-de-negocio.md` — rastreabilidade regra → fonte → BDD → teste (o equivalente ao card do Azure DevOps);
- `azure-pipelines.yml` — esteira de build e teste.

## Pontos para a conversa com o Luiz

Use estes pontos como roteiro, não como script decorado.

1. **O problema da vaga, em miniatura.** Um módulo de varejo (cadastro, pedido, estoque, fiscal, reposição) com regras espalhadas em telas, procedures e um “trigger”. É o tipo de coisa que a Traction quer extrair para BDD e reimplementar em .NET com testes de paridade.
2. **Engenharia reversa antes do código novo.** A quebra de refrigerante e o ICMS da padaria não estão no `ped_inclui.prg`. Só aparecem em `trg_` / `spc_`, com comentários “não exibir em tela”. Se a IA (ou a pessoa) ler só o fluxo principal, a migração sai errada em produção.
3. **Paridade primeiro, melhoria depois.** O .NET **copia** o arredondamento do VFP, o desconto truncado, o ICMS não recalculado do funcionário e o estoque negativo da quebra. Está documentado. Corrigir isso é decisão de negócio com o fiscal / o PO, não um “ajuste técnico” silencioso.
4. **A IA acelera, a revisão decide.** Em `docs/decisoes.md` estão erros típicos de código gerado: `MidpointRounding.ToEven` (1,125 → 1,12 em vez de 1,13), desconto arredondado para 2,40, ICMS recalculado depois do desconto de folha, trigger ignorado. A checklist está em `.cursor/rules/revisao-codigo-ia.mdc`.
5. **BDD em português.** Os `.feature` usam Dado / Quando / Então, no vocabulário do PO e do dev do cliente. Dá para validar a regra com eles antes de implementar — exatamente o que a vaga descreve.
6. **Rastreabilidade.** Cada RN em `docs/regras-de-negocio.md` aponta o `.prg`, o cenário e o teste. O `azure-pipelines.yml` roda essa bateria a cada build.
7. **Demo ao vivo nas telas.** No front, as opções do menu são as mesmas do FoxPro. Um pedido de 5 refrigerantes mostra a quebra e a sugestão de compra 36, visíveis na tela — não só no JSON do Swagger. Cada tela traz um recado “Legado vs novo” com o `.prg` que ela substitui.
8. **Como eu usaria isso no cliente.** O Cursor (ou outra ferramenta liberada pela Quality) leria os `.prg` e as DBF no ambiente combinado, eu validaria as features com o dev que conhece o Fox, implementaria com testes de paridade e só então abriria o PR. Código do cliente não sai do ambiente autorizado.

## Funcionalidade coberta (igual nos dois lados)

- Cadastro de produto (código, descrição, NCM, preço, estoque, estoque mínimo).
- Pedido de venda com validação de estoque.
- Desconto por faixa (varejista: 5% a partir de 6 un., 10% a partir de 12) e desconto de funcionário.
- ICMS / CFOP (5102/18% interno, 6102/12% interestadual, 7% para NCM 1905).
- Sugestão de reposição quando o estoque cai abaixo do mínimo (`mínimo × 2 − saldo`).

Produtos de homologação escolhidos de propósito: pão (NCM 1905), refrigerante (NCM 2202, quebra) e água (R$ 6,25, para o arredondamento 1,125).

## Observação

O Visual FoxPro da pasta é ilustrativo e não está compilado. A referência executável é o `.NET` (API + front). Depois da entrevista, este material é só um exemplo — em um projeto real, o código do cliente permanece no ambiente que a Quality autorizar.
