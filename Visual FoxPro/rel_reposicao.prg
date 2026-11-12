*=======================================================
* Programa : rel_reposicao.prg
* Sistema  : Gestcom
* Funcao   : relatorio de sugestao de reposicao
*            (base para o pedido de compra)
*=======================================================
SET TALK OFF
SET DELETED ON
SET CENTURY ON
SET DATE BRITISH

DO AbreTabelas IN funcoes.prg

LOCAL nLin, nPag, nTotSug

nLin    = 99
nPag    = 0
nTotSug = 0

SELECT sugrepos
SET ORDER TO TAG CODPROD
IF RECCOUNT() = 0
    WAIT WINDOW "Nenhuma sugestao de reposicao pendente." TIMEOUT 2
    RETURN
ENDIF

SCAN
    IF nLin > 55
        nPag = nPag + 1
        CLEAR
        ? "GESTCOM - SUGESTAO DE REPOSICAO (pedido de compra)"
        ? "Data: " + DTOC(DATE()) + "   Pagina: " + TRANSFORM(nPag, "999")
        ? REPLICATE("-", 72)
        ? PADR("CODIGO", 10), PADR("DESCRICAO", 28), ;
          PADL("ATUAL", 8), PADL("MINIMO", 8), PADL("SUGERIDO", 10), "EMISSAO"
        ? REPLICATE("-", 72)
        nLin = 6
    ENDIF

    SELECT produtos
    SET ORDER TO TAG CODPROD
    SEEK sugrepos.codprod

    ? PADR(sugrepos.codprod, 10), ;
      PADR(IIF(FOUND(), produtos.descricao, "?"), 28), ;
      TRANSFORM(sugrepos.qtdatual, "99999.99"), ;
      TRANSFORM(sugrepos.qtdmin,   "99999.99"), ;
      TRANSFORM(sugrepos.qtdsug,   "999999.99"), ;
      DTOC(sugrepos.data)

    nTotSug = nTotSug + sugrepos.qtdsug
    nLin    = nLin + 1
    SELECT sugrepos
ENDSCAN

? REPLICATE("-", 72)
? "Itens sugeridos (soma qtd): " + TRANSFORM(nTotSug, "999,999.99")
WAIT WINDOW "Fim do relatorio." TIMEOUT 3
RETURN
