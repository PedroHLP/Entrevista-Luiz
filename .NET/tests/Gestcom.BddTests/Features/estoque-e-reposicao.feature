# language: pt
Funcionalidade: Baixa de estoque, quebra operacional e sugestão de reposição
  Como analista da migração
  Quero reproduzir o trg_baixa_estoque.prg inclusive a quebra do chamado 4412
  Para que o saldo e o pedido de compra sugerido fiquem iguais aos do legado

  Cenário: Venda de 5 refrigerantes baixa 6 e sugere 36 de reposição
    Dado o catálogo de homologação carregado
    E o produto "0004" com estoque 18 e mínimo 24
    Quando a venda de 5 unidades do produto "0004" é baixada no estoque
    Então o estoque final deve ser 12
    E a quebra operacional deve ter sido aplicada
    E a quantidade sugerida de reposição deve ser 36

  Cenário: Venda do estoque inteiro de refrigerante deixa o saldo negativo
    Dado o catálogo de homologação carregado
    E o produto "0004" com estoque 18 e mínimo 24
    Quando a venda de 18 unidades do produto "0004" é baixada no estoque
    Então o estoque final deve ser -1
    E a quantidade sugerida de reposição deve ser 49

  Cenário: Produto com estoque mínimo zero não gera sugestão
    Dado o catálogo de homologação carregado
    E o produto "0007" com estoque 60 e mínimo 0
    Quando a venda de 10 unidades do produto "0007" é baixada no estoque
    Então o estoque final deve ser 50
    E não deve haver sugestão de reposição

  Cenário: Pedido acima do estoque disponível é rejeitado antes da baixa
    Dado um cliente consumidor do estado "SP"
    E o catálogo de homologação carregado
    Quando se tenta lançar um pedido com 20 unidades do produto "0004"
    Então o pedido deve ser rejeitado por estoque insuficiente
    E o estoque do produto "0004" deve permanecer 18
