SELECT r.rub_nom AS Categoria, ISNULL(s.Descripcion, '') AS SubCategoria, 
       a.art_codbar AS CodigoBarra, a.art_des AS Descripcion, 
       P.pda_mon AS PrecioFinal, a.art_obs AS Observaciones
FROM ARTICULOS AS a
INNER JOIN RUBROS AS r ON a.rub_idesec = r.rub_idesec
LEFT JOIN SUBRUBROS AS s ON s.IdSubRubro = a.IdSubRubro
INNER JOIN PRECIOS_DE_ARTICULOS AS P ON P.art_idesec = a.art_idesec
WHERE LEN(ISNULL(a.art_codbar, '')) > 0 
  AND a.art_pubweb = 1 
  AND a.art_fecbaj IS NULL 
  AND P.tdp_idesec = @tdp_idesec
