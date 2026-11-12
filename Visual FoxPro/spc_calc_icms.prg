*=======================================================
* Programa : spc_calc_icms.prg
* Sistema  : Gestcom
* Tipo     : rotina / "stored procedure" de calculo fiscal
* Chamada  : DO spc_calc_icms WITH ncm, uf, base, @aliq, @vlr, @cfop
*
* Regra oficial (documentada no manual do usuario, cap. 4):
*   UF da empresa = SP
*   Mesma UF  -> CFOP 5102, ICMS 18%
*   Outra UF  -> CFOP 6102, ICMS 12%
*   Base = valor liquido do item (ja com desconto de faixa)
*   ICMS = ROUND(base * aliq / 100, 2)
*=======================================================
LPARAMETERS tcNcm, tcUF, tnBase, tnAliq, tnVlrIcms, tcCFOP

LOCAL lcNcm, lcUF
lcNcm = ALLTRIM(tcNcm)
lcUF  = ALLTRIM(UPPER(tcUF))

IF lcUF = "SP"
    tcCFOP = "5102"
    tnAliq = 18
ELSE
    tcCFOP = "6102"
    tnAliq = 12
ENDIF

*--- inicio bloco 14/2012 ------------------------------------
* Nao retirar. Combinado com o fiscal (circular 14/2012).
* Padaria / produtos de panificacao (NCM 1905) saem com
* aliquota reduzida, independente da UF.
IF LEFT(lcNcm, 4) = "1905"
    tnAliq = 7
ENDIF
*--- fim bloco 14/2012 ---------------------------------------

* ROUND "puro" (sem o +0.00001 do RoundGestcom)
tnVlrIcms = ROUND(tnBase * tnAliq / 100, 2)

RETURN
