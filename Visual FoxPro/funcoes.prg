*=======================================================
* Programa : funcoes.prg
* Sistema  : Gestcom
* Funcoes  de uso geral (arredondamento, abertura de
* tabelas, sequencial de pedido)
*=======================================================

*-------------------------------------------------------
FUNCTION RoundGestcom
* Equivale ao ROUND usado nos totais de item.
* O +0.00001 existe desde 2009 para "corrigir" ponto
* flutuante (N() do VFP). NAO remover.
*-------------------------------------------------------
LPARAMETERS tnValor, tnCasas
IF EMPTY(tnCasas)
    tnCasas = 2
ENDIF
RETURN ROUND(tnValor + 0.00001, tnCasas)


*-------------------------------------------------------
FUNCTION Trunca2
* Trunca em 2 casas, sem arredondar.
* Usado no VALOR do desconto por faixa (nao na aliquota).
* Pedido do comercial em 2011: desconto nunca arredonda
* para cima, para nao "dar desconto a mais".
*-------------------------------------------------------
LPARAMETERS tnValor
RETURN INT(tnValor * 100) / 100


*-------------------------------------------------------
FUNCTION AbreTabelas
*-------------------------------------------------------
LOCAL lcDir
lcDir = ADDBS(JUSTPATH(SYS(16))) + "dados"
IF NOT DIRECTORY(lcDir)
    lcDir = ADDBS(GETENV("GESTCOMDIR")) + "dados"
ENDIF

IF NOT USED("produtos")
    USE (lcDir + "produtos") IN 0 SHARED ALIAS produtos
ENDIF
IF NOT USED("clientes")
    USE (lcDir + "clientes") IN 0 SHARED ALIAS clientes
ENDIF
IF NOT USED("pedidos")
    USE (lcDir + "pedidos") IN 0 SHARED ALIAS pedidos
ENDIF
IF NOT USED("itemped")
    USE (lcDir + "itemped") IN 0 SHARED ALIAS itemped
ENDIF
IF NOT USED("sugrepos")
    USE (lcDir + "sugrepos") IN 0 SHARED ALIAS sugrepos
ENDIF
RETURN .T.


*-------------------------------------------------------
FUNCTION ProximoPedido
*-------------------------------------------------------
LOCAL nProx
SELECT pedidos
CALCULATE MAX(NUMPED) TO nProx
IF EMPTY(nProx)
    nProx = 0
ENDIF
RETURN nProx + 1


*-------------------------------------------------------
FUNCTION PercDescontoFaixa
* Desconto por tipo de cliente e quantidade do ITEM.
* Apenas varejista (V). Faixas nao sao cumulativas:
*   >= 12 un  -> 10%
*   >=  6 un  ->  5%
* Consumidor e funcionario nao entram aqui.
*-------------------------------------------------------
LPARAMETERS tcTipo, tnQtd
LOCAL nPerc
nPerc = 0
IF tcTipo = "V"
    IF tnQtd >= 12
        nPerc = 10
    ELSE
        IF tnQtd >= 6
            nPerc = 5
        ENDIF
    ENDIF
ENDIF
RETURN nPerc
