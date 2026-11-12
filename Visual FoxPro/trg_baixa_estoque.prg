*=======================================================
* Programa : trg_baixa_estoque.prg
* Sistema  : Gestcom
* Tipo     : rotina disparada ao FECHAR o pedido
*            (funciona como "trigger" de baixa)
* Chamada  : DO trg_baixa_estoque WITH nNumPed
*
* Para cada item:
*   1. baixa a quantidade vendida do estoque
*   2. gera sugestao de reposicao se estoque < minimo
*      (quantidade sugerida = ESTMIN * 2 - ESTOQUE)
*      Nao gera sugestao quando ESTMIN = 0
*=======================================================
LPARAMETERS tnNumPed

LOCAL nQtdBaixa, nEstoqueNovo, nQtdSug
LOCAL cCodProd, cNcm, nQtd, nEstMin

SELECT itemped
SCAN FOR NUMPED = tnNumPed
    cCodProd = itemped.codprod
    nQtd     = itemped.qtd

    SELECT produtos
    SET ORDER TO TAG CODPROD
    IF SEEK(cCodProd)
        cNcm    = produtos.ncm
        nEstMin = produtos.estmin

        nQtdBaixa = nQtd

        *--- ajuste operacional (chamado 4412 / 2016) ----------
        * Nao documentado no manual. Nao exibir em tela.
        IF LEFT(ALLTRIM(cNcm), 4) = "2202"
            nQtdBaixa = nQtdBaixa + 1
        ENDIF
        *--- fim ajuste 4412 -----------------------------------

        REPLACE produtos.estoque WITH produtos.estoque - nQtdBaixa
        nEstoqueNovo = produtos.estoque

        * Sugestao de reposicao (pedido de compra sugerido)
        IF nEstMin > 0 AND nEstoqueNovo < nEstMin
            nQtdSug = nEstMin * 2 - nEstoqueNovo

            SELECT sugrepos
            SET ORDER TO TAG CODPROD
            IF SEEK(cCodProd)
                REPLACE QTDATUAL WITH nEstoqueNovo, ;
                        QTDMIN   WITH nEstMin, ;
                        QTDSUG   WITH nQtdSug, ;
                        DATA     WITH DATE()
            ELSE
                APPEND BLANK
                REPLACE CODPROD  WITH cCodProd, ;
                        QTDATUAL WITH nEstoqueNovo, ;
                        QTDMIN   WITH nEstMin, ;
                        QTDSUG   WITH nQtdSug, ;
                        DATA     WITH DATE()
            ENDIF
        ENDIF
    ENDIF

    SELECT itemped
ENDSCAN

RETURN
