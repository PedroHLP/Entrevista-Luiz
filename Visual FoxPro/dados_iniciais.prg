*=======================================================
* Programa : dados_iniciais.prg
* Sistema  : Gestcom
* Autor    : Equipe Sistemas
* Funcao   : carga inicial / homologacao
*            (tambem exporta CSV em dados\)
*=======================================================
SET TALK OFF
SET SAFETY OFF
SET DELETED ON
SET CENTURY ON
SET DATE BRITISH
SET EXACT ON

LOCAL lcDir
lcDir = ADDBS(JUSTPATH(SYS(16))) + "dados"
CD (lcDir)

IF NOT FILE("produtos.dbf")
    DO (ADDBS(JUSTPATH(SYS(16))) + "criatabelas.prg")
    CD (lcDir)
ENDIF

DO abre_tabelas IN (ADDBS(JUSTPATH(SYS(16))) + "funcoes.prg")

SELECT produtos
ZAP
INSERT INTO produtos VALUES ("0001", "ARROZ TIPO 1 5KG",        "10063021", 24.90, 50.00, 20.00, .T.)
INSERT INTO produtos VALUES ("0002", "FEIJAO CARIOCA 1KG",      "07133319",  8.50, 30.00, 15.00, .T.)
INSERT INTO produtos VALUES ("0003", "PAO DE FORMA 500G",       "19059090",  7.99, 40.00, 25.00, .T.)
INSERT INTO produtos VALUES ("0004", "REFRIGERANTE COLA 2L",    "22021000",  6.50, 18.00, 24.00, .T.)
INSERT INTO produtos VALUES ("0005", "CAFE TORRADO 500G",       "09012100", 16.90,  8.00, 10.00, .T.)
INSERT INTO produtos VALUES ("0006", "AGUA MINERAL 1,5L",       "22019000",  6.25, 25.00, 12.00, .T.)
INSERT INTO produtos VALUES ("0007", "ACUCAR REFINADO 1KG",     "17019900",  4.19, 60.00,  0.00, .T.)

SELECT clientes
ZAP
INSERT INTO clientes VALUES ("0001", "MERCADO SAO JOAO LTDA",      "V", "SP")
INSERT INTO clientes VALUES ("0002", "SUPERMERCADO PARANA LTDA",   "V", "PR")
INSERT INTO clientes VALUES ("0003", "MARIA CONSUMIDORA",          "C", "SP")
INSERT INTO clientes VALUES ("0004", "PEDRO FUNCIONARIO",          "F", "SP")
INSERT INTO clientes VALUES ("0005", "COMERCIAL MINAS ALIMENTOS",  "V", "MG")

SELECT pedidos
ZAP
SELECT itemped
ZAP
SELECT sugrepos
ZAP

* Exporta CSV para a migracao (COPY TO TYPE CSV)
SELECT produtos
COPY TO produtos.csv TYPE CSV

SELECT clientes
COPY TO clientes.csv TYPE CSV

SELECT pedidos
COPY TO pedidos.csv TYPE CSV

SELECT itemped
COPY TO itemped.csv TYPE CSV

SELECT sugrepos
COPY TO sugrepos.csv TYPE CSV

? "Carga inicial concluida e CSV gerados em " + lcDir
RETURN
