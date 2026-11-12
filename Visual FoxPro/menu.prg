*=======================================================
* Programa : menu.prg
* Sistema  : Gestcom - menu principal (modulo pedidos)
* Autor    : Equipe Sistemas
* Criado   : 12/08/2009
*=======================================================
SET TALK OFF
SET CONFIRM ON
SET DELETED ON
SET EXCLUSIVE OFF
SET CENTURY ON
SET DATE BRITISH
SET EXACT OFF
SET NEAR ON
SET REPROCESS TO AUTOMATIC
SET POINT TO ","
SET SEPARATOR TO "."
SET CURRENCY TO "R$"

DO AbreTabelas IN funcoes.prg

CLEAR
@  1, 2 SAY "GESTCOM  -  Pedidos e Estoque          (VFP 9.0)"
@  2, 2 SAY REPLICATE("-", 58)
@  4, 2 SAY "  1. Cadastro de produtos"
@  5, 2 SAY "  2. Inclusao de pedido de venda"
@  6, 2 SAY "  3. Relatorio de sugestao de reposicao"
@  7, 2 SAY "  9. Sair"
@  9, 2 SAY "Opcao: "

LOCAL nOpcao
nOpcao = 0
@  9, 10 GET nOpcao PICTURE "9" RANGE 1, 9
READ

DO CASE
    CASE nOpcao = 1
        DO cad_produtos
    CASE nOpcao = 2
        DO ped_inclui
    CASE nOpcao = 3
        DO rel_reposicao
    CASE nOpcao = 9
        CLOSE DATABASES ALL
        RETURN
ENDCASE

* volta ao menu
DO menu
RETURN
