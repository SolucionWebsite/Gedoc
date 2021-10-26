SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vw_BandejaEntrada]/* WITH SCHEMABINDING*/
AS SELECT DISTINCT
  req.Id
 ,req.DocumentoIngreso
 ,req.FechaIngreso
 ,req.RemitenteId
 ,rem.Nombre AS RemitenteNombre
 ,rem.Institucion AS RemitenteInstitucion
 ,req.Materia
 ,req.AsignacionUt
 ,Convert(date, req.Resolucion) as Resolucion
 ,(SELECT COUNT(RequerimientoId)
    FROM [dbo].[RequerimientosDespachos] reqdesp
	INNER JOIN [dbo].[Despacho] desp on desp.Id = reqdesp.DespachoId
    WHERE reqdesp.RequerimientoId = req.Id AND desp.Eliminado = 0)
  AS CantOficiosCmn
 ,req.EstadoId
 ,es.Titulo AS EstadoTitulo
 ,req.EtapaId
 ,eta.Titulo AS EtapaTitulo
 ,req.UtAsignadaId AS UtAsignadaId
 ,ut.Titulo as UtAsignadaTitulo
 ,req.UtTemporalId AS UtTemporalId
 ,utt.Titulo as UtTemporalTitulo
 ,req.UtTransparenciaId AS UtTransparenciaId
 ,uttr.Titulo as UtTransparenciaTitulo
 ,req.ProfesionalId
 ,prof.NombresApellidos AS ProfesionalNombre
 ,req.ProfesionalTempId
 ,req.ProfesionalTranspId
 ,ut.ResponsableId as ResponsableUtId
 ,utt.ResponsableId as ResponsableUtTempId
 ,uttr.ResponsableId as ResponsableTranspId
 ,'' AS Acciones
 ,acc.BandejaId
 ,CASE
    WHEN (req.UtTemporalId IS NOT NULL AND
      (req.LiberarAsignacionTemp IS NULL OR req.LiberarAsignacionTemp = 0) AND
       es.Id <> 8) THEN 'TEMPORAL'
    WHEN etiq.RequerimientoId IS NOT NULL THEN 'SIAC/TRANSPARENCIA'
    ELSE req.TipoIngreso
  END AS TipoIngreso
 ,IIF(req.Resolucion IS NULL, 99, DATEDIFF(DAY, GETDATE(), req.Resolucion) ) AS DiasResolucion
 ,req.Liberacion
 ,req.LiberarAsignacionTemp
 ,req.ForzarPrioridad
 ,req.ForzarPrioridadFecha
 ,req.ForzarPrioridadMotivo
 ,req.SolicitudTramId
FROM dbo.Requerimiento req
LEFT JOIN dbo.Remitente rem ON req.RemitenteId = rem.Id
LEFT JOIN dbo.EstadoRequerimiento es ON req.EstadoId = es.Id
LEFT JOIN dbo.EtapaRequerimiento eta ON req.EtapaId = eta.Id
LEFT JOIN dbo.UnidadTecnica ut ON req.UtAsignadaId = ut.Id
LEFT JOIN dbo.UnidadTecnica utt ON req.UtTemporalId = utt.Id
LEFT JOIN dbo.UnidadTecnica uttr ON req.UtTransparenciaId = uttr.Id
LEFT JOIN dbo.Usuario prof ON req.ProfesionalId = prof.Id
JOIN (SELECT DISTINCT BandejaId, EstadoId, EtapaId from dbo.AccionesPermitidasBandejas) as acc
  ON req.EstadoId = acc.EstadoId AND req.EtapaId = acc.EtapaId
LEFT JOIN (SELECT TOP 1 re.RequerimientoId
  FROM dbo.RequerimientosEtiquetas re
  JOIN dbo.ListaValor e
    ON re.EtiquetaCod = e.Codigo AND e.IdLista = re.EtiquetaListaId
  WHERE e.Titulo LIKE '%Transparencia%' OR e.Titulo LIKE '%OIRS%') AS etiq
  ON etiq.RequerimientoId = req.Id
WHERE req.Eliminado = 0
GO