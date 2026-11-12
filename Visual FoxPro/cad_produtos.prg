*=======================================================
* Programa : cad_produtos.prg
* Sistema  : Gestcom
* Funcao   : inclusao / alteracao de produtos
*=======================================================
SET TALK OFF
SET DELETED ON
SET EXACT ON

DO AbreTabelas IN funcoes.prg

LOCAL cCod, cDesc, cNcm, nPreco, nEstoque, nEstMin, lAtivo
cCod     = SPACE(10)
cDesc    = SPACE(50)
cNcm     = SPACE(8)
nPreco   = 0
nEstoque = 0
nEstMin  = 0
lAtivo   = .T.

CLEAR
@  1, 2 SAY "CADASTRO DE PRODUTOS"
@  2, 2 SAY REPLICATE("-", 58)
@  4, 2 SAY "Codigo      :" GET cCod     PICTURE "@!K"
@  5, 2 SAY "Descricao   :" GET cDesc    PICTURE "@!"
@  6, 2 SAY "NCM         :" GET cNcm     PICTURE "99999999"
@  7, 2 SAY "Preco       :" GET nPreco   PICTURE "999,999.99"
@  8, 2 SAY "Estoque     :" GET nEstoque PICTURE "999,999.99"
@  9, 2 SAY "Estoque min.:" GET nEstMin  PICTURE "999,999.99"
@ 10, 2 SAY "Ativo (S/N) :" GET lAtivo
READ

cCod = PADR(ALLTRIM(cCod), 10)
IF EMPTY(cCod)
    WAIT WINDOW "Codigo obrigatorio." TIMEOUT 2
    RETURN
ENDIF
IF EMPTY(cDesc)
    WAIT WINDOW "Descricao obrigatoria." TIMEOUT 2
    RETURN
ENDIF
IF EMPTY(cNcm) OR LEN(ALLTRIM(cNcm)) < 8
    WAIT WINDOW "NCM deve ter 8 digitos." TIMEOUT 2
    RETURN
ENDIF
IF nPreco < 0 OR nEstoque < 0 OR nEstMin < 0
    WAIT WINDOW "Valores nao podem ser negativos." TIMEOUT 2
    RETURN
ENDIF

SELECT produtos
SET ORDER TO TAG CODPROD
IF SEEK(cCod)
    REPLACE DESCRICAO WITH cDesc, ;
            NCM       WITH cNcm, ;
            PRECO     WITH nPreco, ;
            ESTOQUE   WITH nEstoque, ;
            ESTMIN    WITH nEstMin, ;
            ATIVO     WITH lAtivo
    WAIT WINDOW "Produto " + ALLTRIM(cCod) + " atualizado." TIMEOUT 2
ELSE
    APPEND BLANK
    REPLACE CODPROD   WITH cCod, ;
            DESCRICAO WITH cDesc, ;
            NCM       WITH cNcm, ;
            PRECO     WITH nPreco, ;
            ESTOQUE   WITH nEstoque, ;
            ESTMIN    WITH nEstMin, ;
            ATIVO     WITH lAtivo
    WAIT WINDOW "Produto " + ALLTRIM(cCod) + " incluido." TIMEOUT 2
ENDIF

RETURN
