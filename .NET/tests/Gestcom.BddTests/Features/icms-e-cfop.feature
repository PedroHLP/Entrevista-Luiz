# language: pt
Funcionalidade: Cálculo de ICMS e CFOP
  Como analista fiscal da migração
  Quero reproduzir o spc_calc_icms.prg inclusive a alíquota reduzida da circular 14/2012
  Para que o .NET cobre o mesmo imposto que o legado

  Cenário: Água em SP arredonda 1,125 para 1,13 e não para 1,12
    Dado um cliente consumidor do estado "SP"
    E o catálogo de homologação carregado
    Quando é lançado um pedido com 1 unidade do produto "0006"
    Então o valor total deve ser 6,25
    E a alíquota de ICMS do item deve ser 18
    E o valor de ICMS deve ser 1,13

  Cenário: Pão em SP usa alíquota reduzida de 7% e CFOP interno
    Dado um cliente consumidor do estado "SP"
    E o catálogo de homologação carregado
    Quando é lançado um pedido com 1 unidade do produto "0003"
    Então o CFOP deve ser "5102"
    E a alíquota de ICMS do item deve ser 7
    E o valor de ICMS deve ser 0,56

  Cenário: Arroz para Minas usa CFOP interestadual e ICMS 12%
    Dado um cliente varejista do estado "MG"
    E o catálogo de homologação carregado
    Quando é lançado um pedido com 5 unidades do produto "0001"
    Então o CFOP deve ser "6102"
    E a alíquota de ICMS do item deve ser 12
    E o valor de ICMS deve ser 14,94
