# language: pt
Funcionalidade: Desconto por faixa e tipo de cliente
  Como analista da migração do Gestcom
  Quero reproduzir o desconto de varejista do funcoes.prg
  Para garantir paridade com o Visual FoxPro

  Cenário: Varejista de SP com 12 unidades recebe 10% e ICMS interno
    Dado um cliente varejista do estado "SP"
    E o catálogo de homologação carregado
    Quando é lançado um pedido com 12 unidades do produto "0001"
    Então o valor bruto deve ser 298,80
    E o valor de desconto deve ser 29,88
    E o valor total deve ser 268,92
    E o CFOP deve ser "5102"
    E o valor de ICMS deve ser 48,41
    E o percentual de desconto do item deve ser 10

  Cenário: Varejista do Paraná com 6 pães trunca o desconto em 2,39
    Dado um cliente varejista do estado "PR"
    E o catálogo de homologação carregado
    Quando é lançado um pedido com 6 unidades do produto "0003"
    Então o valor bruto deve ser 47,94
    E o valor de desconto deve ser 2,39
    E o valor total deve ser 45,55
    E o CFOP deve ser "6102"
    E a alíquota de ICMS do item deve ser 7
    E o valor de ICMS deve ser 3,19

  Cenário: Consumidor não recebe desconto de faixa mesmo com 12 unidades
    Dado um cliente consumidor do estado "SP"
    E o catálogo de homologação carregado
    Quando é lançado um pedido com 12 unidades do produto "0001"
    Então o valor de desconto deve ser 0,00
    E o valor total deve ser 298,80
