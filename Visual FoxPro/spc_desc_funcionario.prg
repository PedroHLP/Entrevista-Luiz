*=======================================================
* Programa : spc_desc_funcionario.prg
* Sistema  : Gestcom
* Tipo     : rotina / "stored procedure"
* Chamada  : DO spc_desc_funcionario WITH tipo, bruto, @desc, @total
*
* Desconto de funcionario (tipo F).
* Aplicado NO CABECALHO, depois dos itens e do ICMS.
* Se o subtotal (bruto - desc ja aplicado) for >= 50,
* concede mais 5% sobre esse subtotal.
*
* ATENCAO: o ICMS ja calculado nos itens NAO e recalculado.
* (e assim desde a versao de 2014; folha / DP pediu para
* nao mexer).
*=======================================================
LPARAMETERS tcTipo, tnBruto, tnDesc, tnTotal

LOCAL nSubtotal, nDescFunc

IF tcTipo <> "F"
    RETURN
ENDIF

nSubtotal = ROUND(tnBruto - tnDesc, 2)

* piso de R$ 50 - combinado com o DP (chamado 2281)
IF nSubtotal >= 50
    nDescFunc = ROUND(nSubtotal * 0.05, 2)
    tnDesc    = tnDesc + nDescFunc
    tnTotal   = ROUND(tnBruto - tnDesc, 2)
ENDIF

RETURN
