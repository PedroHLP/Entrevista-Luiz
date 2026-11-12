*=======================================================
* Programa : ped_inclui.prg
* Sistema  : Gestcom
* Funcao   : inclusao de pedido de venda (saida de estoque)
* Alt      : 15/03/2018 - desconto por faixa de quantidade
*            22/07/2019 - calculo de ICMS / CFOP
*
* Fluxo:
*   1. identifica cliente
*   2. lanca itens (valida estoque ANTES de gravar)
*   3. aplica desconto de faixa (so varejista)
*   4. calcula ICMS / CFOP  (spc_calc_icms)
*   5. aplica desconto de funcionario (spc_desc_funcionario)
*   6. grava cabecalho + itens, fecha o pedido
*   7. baixa estoque (trg_baixa_estoque) e sugestao de reposicao
*=======================================================
SET TALK OFF
SET DELETED ON
SET EXACT ON
SET CENTURY ON
SET DATE BRITISH

DO AbreTabelas IN funcoes.prg

LOCAL cCodCli, nNumPed, nItem, cCodProd, nQtd
LOCAL nPreco, nPercDesc, nVlrBruto, nVlrDesc, nVlrItem
LOCAL nAliq, nVlrIcms, cCFOP
LOCAL nTotBruto, nTotDesc, nTotIcms, nTotGeral
LOCAL cTipo, cUF, lOk

cCodCli   = SPACE(8)
nTotBruto = 0
nTotDesc  = 0
nTotIcms  = 0
nTotGeral = 0
nItem     = 0

CLEAR
@  1, 2 SAY "INCLUSAO DE PEDIDO DE VENDA"
@  2, 2 SAY REPLICATE("-", 58)
@  4, 2 SAY "Cliente:" GET cCodCli PICTURE "@!K"
READ

cCodCli = PADR(ALLTRIM(cCodCli), 8)
SELECT clientes
SET ORDER TO TAG CODCLI
IF NOT SEEK(cCodCli)
    WAIT WINDOW "Cliente nao encontrado." TIMEOUT 2
    RETURN
ENDIF
cTipo = clientes.tipo
cUF   = clientes.uf
@  5, 2 SAY "Nome : " + ALLTRIM(clientes.nome) + "  (" + cTipo + "/" + cUF + ")"

nNumPed = ProximoPedido()
@  6, 2 SAY "Pedido n. " + TRANSFORM(nNumPed, "99999999")

*-- laco de itens ------------------------------------------------
DO WHILE .T.
    cCodProd = SPACE(10)
    nQtd     = 0
    @  8, 2 SAY "Produto (em branco p/ encerrar):" GET cCodProd PICTURE "@!K"
    READ
    cCodProd = PADR(ALLTRIM(cCodProd), 10)
    IF EMPTY(cCodProd)
        EXIT
    ENDIF

    SELECT produtos
    SET ORDER TO TAG CODPROD
    IF NOT SEEK(cCodProd) OR produtos.ativo = .F.
        WAIT WINDOW "Produto inexistente ou inativo." TIMEOUT 2
        LOOP
    ENDIF

    @  9, 2 SAY "Qtd:" GET nQtd PICTURE "999999.99"
    READ
    IF nQtd <= 0
        WAIT WINDOW "Quantidade invalida." TIMEOUT 2
        LOOP
    ENDIF

    * Validacao de estoque (ANTES da baixa).
    * Importante: a baixa adicional de alguns NCMs e feita
    * depois, no trg_baixa_estoque, e NAO entra nesta checagem.
    IF nQtd > produtos.estoque
        WAIT WINDOW "Estoque insuficiente. Disponivel: " + ;
            TRANSFORM(produtos.estoque, "999,999.99") TIMEOUT 3
        LOOP
    ENDIF

    nPreco    = produtos.preco
    nPercDesc = PercDescontoFaixa(cTipo, nQtd)

    * Bruto do item: ROUND com o ajuste de ponto flutuante
    nVlrBruto = RoundGestcom(nPreco * nQtd, 2)

    * Valor do desconto TRUNCADO em 2 casas (nao arredonda)
    nVlrDesc  = Trunca2(nVlrBruto * nPercDesc / 100)

    * Liquido do item
    nVlrItem  = RoundGestcom(nVlrBruto - nVlrDesc, 2)

    * ICMS / aliquota (pode ter regra especial por NCM)
    nAliq     = 0
    nVlrIcms  = 0
    cCFOP     = ""
    DO spc_calc_icms WITH produtos.ncm, cUF, nVlrItem, nAliq, nVlrIcms, cCFOP

    nItem = nItem + 1
    SELECT itemped
    APPEND BLANK
    REPLACE NUMPED   WITH nNumPed, ;
            ITEM     WITH nItem, ;
            CODPROD  WITH cCodProd, ;
            QTD      WITH nQtd, ;
            PRECO    WITH nPreco, ;
            PERCDESC WITH nPercDesc, ;
            VLRDESC  WITH nVlrDesc, ;
            VLRITEM  WITH nVlrItem, ;
            ALIQICMS WITH nAliq, ;
            VLRICMS  WITH nVlrIcms

    nTotBruto = nTotBruto + nVlrBruto
    nTotDesc  = nTotDesc  + nVlrDesc
    nTotIcms  = nTotIcms  + nVlrIcms

    @ 11, 2 SAY "Item " + TRANSFORM(nItem,"999") + " " + ;
        LEFT(produtos.descricao, 20) + "  " + ;
        TRANSFORM(nVlrItem, "@Z 999,999.99")
ENDDO

IF nItem = 0
    WAIT WINDOW "Pedido cancelado (nenhum item)." TIMEOUT 2
    RETURN
ENDIF

* Totais parciais (antes do desconto de funcionario)
nTotBruto = RoundGestcom(nTotBruto, 2)
nTotDesc  = RoundGestcom(nTotDesc, 2)
nTotIcms  = RoundGestcom(nTotIcms, 2)
nTotGeral = RoundGestcom(nTotBruto - nTotDesc, 2)

* Desconto adicional de funcionario (nao recalcula ICMS)
DO spc_desc_funcionario WITH cTipo, nTotBruto, nTotDesc, nTotGeral

nTotGeral = RoundGestcom(nTotBruto - nTotDesc, 2)

* CFOP do cabecalho: uf do cliente x uf da empresa (SP)
* (a aliquota do item pode ser especial, mas o CFOP segue a UF)
IF cUF = "SP"
    cCFOP = "5102"
ELSE
    cCFOP = "6102"
ENDIF

SELECT pedidos
APPEND BLANK
REPLACE NUMPED   WITH nNumPed, ;
        CODCLI   WITH cCodCli, ;
        DATA     WITH DATE(), ;
        STATUS   WITH "A", ;
        VLRBRUTO WITH nTotBruto, ;
        VLRDESC  WITH nTotDesc, ;
        VLRICMS  WITH nTotIcms, ;
        VLRTOTAL WITH nTotGeral, ;
        CFOP     WITH cCFOP

* Fecha o pedido e dispara a baixa de estoque + sugestao
DO trg_baixa_estoque WITH nNumPed

SELECT pedidos
LOCATE FOR NUMPED = nNumPed
IF FOUND()
    REPLACE STATUS WITH "F"
ENDIF

WAIT WINDOW "Pedido " + TRANSFORM(nNumPed,"99999999") + ;
    " gravado. Total: " + TRANSFORM(nTotGeral, "@K 999,999.99") TIMEOUT 4
RETURN
