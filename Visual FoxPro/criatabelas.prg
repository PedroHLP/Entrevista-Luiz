*=======================================================
* Programa : criatabelas.prg
* Sistema  : Gestcom - modulo de pedidos e estoque
* Autor    : Equipe Sistemas
* Criado   : 12/08/2009
* Alt      : 03/11/2016 - campo ESTMIN em produtos
*            15/03/2018 - tabela SUGREPOS
* Obs      : tabelas livres (FREE), sem DBC
*=======================================================
SET TALK OFF
SET SAFETY OFF
SET EXCLUSIVE ON
SET DELETED ON
SET CENTURY ON
SET DATE BRITISH

LOCAL lcDir
lcDir = ADDBS(JUSTPATH(SYS(16))) + "dados"
IF NOT DIRECTORY(lcDir)
    MD (lcDir)
ENDIF
CD (lcDir)

*--- PRODUTOS ------------------------------------------------
IF FILE("produtos.dbf")
    DELETE FILE produtos.dbf
    IF FILE("produtos.cdx")
        DELETE FILE produtos.cdx
    ENDIF
ENDIF

CREATE TABLE produtos FREE ( ;
    CODPROD    C(10), ;
    DESCRICAO  C(50), ;
    NCM        C(8), ;
    PRECO      N(12,2), ;
    ESTOQUE    N(10,2), ;
    ESTMIN     N(10,2), ;
    ATIVO      L ;
)
INDEX ON CODPROD TAG CODPROD
INDEX ON DESCRICAO TAG DESCRICAO
USE

*--- CLIENTES ------------------------------------------------
IF FILE("clientes.dbf")
    DELETE FILE clientes.dbf
    IF FILE("clientes.cdx")
        DELETE FILE clientes.cdx
    ENDIF
ENDIF

CREATE TABLE clientes FREE ( ;
    CODCLI     C(8), ;
    NOME       C(50), ;
    TIPO       C(1), ;
    UF         C(2) ;
)
* TIPO: C = Consumidor, V = Varejista, F = Funcionario
INDEX ON CODCLI TAG CODCLI
INDEX ON NOME TAG NOME
USE

*--- PEDIDOS (cabecalho) -------------------------------------
IF FILE("pedidos.dbf")
    DELETE FILE pedidos.dbf
    IF FILE("pedidos.cdx")
        DELETE FILE pedidos.cdx
    ENDIF
ENDIF

CREATE TABLE pedidos FREE ( ;
    NUMPED     N(8), ;
    CODCLI     C(8), ;
    DATA       D, ;
    STATUS     C(1), ;
    VLRBRUTO   N(12,2), ;
    VLRDESC    N(12,2), ;
    VLRICMS    N(12,2), ;
    VLRTOTAL   N(12,2), ;
    CFOP       C(4) ;
)
* STATUS: A = Aberto, F = Fechado (baixado no estoque)
INDEX ON NUMPED TAG NUMPED
INDEX ON CODCLI TAG CODCLI
USE

*--- ITEMPED (itens do pedido) -------------------------------
IF FILE("itemped.dbf")
    DELETE FILE itemped.dbf
    IF FILE("itemped.cdx")
        DELETE FILE itemped.cdx
    ENDIF
ENDIF

CREATE TABLE itemped FREE ( ;
    NUMPED     N(8), ;
    ITEM       N(4), ;
    CODPROD    C(10), ;
    QTD        N(10,2), ;
    PRECO      N(12,2), ;
    PERCDESC   N(5,2), ;
    VLRDESC    N(12,2), ;
    VLRITEM    N(12,2), ;
    ALIQICMS   N(5,2), ;
    VLRICMS    N(12,2) ;
)
INDEX ON STR(NUMPED,8)+STR(ITEM,4) TAG PEDITEM
INDEX ON NUMPED TAG NUMPED
INDEX ON CODPROD TAG CODPROD
USE

*--- SUGREPOS (sugestao de reposicao / pedido de compra) -----
IF FILE("sugrepos.dbf")
    DELETE FILE sugrepos.dbf
    IF FILE("sugrepos.cdx")
        DELETE FILE sugrepos.cdx
    ENDIF
ENDIF

CREATE TABLE sugrepos FREE ( ;
    CODPROD    C(10), ;
    QTDATUAL   N(10,2), ;
    QTDMIN     N(10,2), ;
    QTDSUG     N(10,2), ;
    DATA       D ;
)
INDEX ON CODPROD TAG CODPROD
USE

SET EXCLUSIVE OFF
SET SAFETY ON
SET TALK ON

? "Tabelas criadas em " + lcDir
RETURN
