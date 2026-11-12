# language: pt
Funcionalidade: Desconto de funcionário no cabeçalho
  Como analista da migração
  Quero reproduzir o spc_desc_funcionario.prg
  Para manter o desconto de folha e o ICMS como no legado

  Cenário: Funcionário com subtotal de 67,60 recebe 5% e o ICMS não é recalculado
    Dado um cliente funcionário do estado "SP"
    E o catálogo de homologação carregado
    Quando é lançado um pedido com 4 unidades do produto "0005"
    Então o valor bruto deve ser 67,60
    E o valor de desconto deve ser 3,38
    E o valor total deve ser 64,22
    E o valor de ICMS deve ser 12,17

  Cenário: Funcionário abaixo do piso de 50 reais não recebe o desconto
    Dado um cliente funcionário do estado "SP"
    E o catálogo de homologação carregado
    Quando é lançado um pedido com 6 unidades do produto "0003"
    Então o valor bruto deve ser 47,94
    E o valor de desconto deve ser 0,00
    E o valor de ICMS deve ser 3,36
