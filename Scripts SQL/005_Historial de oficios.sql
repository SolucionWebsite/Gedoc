
/* Se actualiza el log de oficios para reflejar las observaciones */
UPDATE lg
  SET ExtraData = oo.Observaciones  
  FROM LogSistema lg
  JOIN OficioObservacion oo 
      ON lg.OrigenId = oo.OficioId AND lg.Fecha = oo.Fecha
  WHERE lg.Origen = 'OFICIO'
 GO         


/* Se crea acción Ver Historial Oficio */      
DECLARE @ACCION_BANDEJA_ID INT;
IF NOT EXISTS (SELECT 1 FROM AccionBandeja WHERE IdAccion = 'HISTORIALOFICIO')
BEGIN
    INSERT INTO [dbo].[AccionBandeja] ([Titulo], [Icono], [Hint], [TipoAccion], [Orden], [IdAccion], [Onclick])
    VALUES ('Ver Historial Oficio', 'k-i-search', 'Ver Historial Oficio', 'O', '13', 'HISTORIALOFICIO', 'verHistorialOficio')
END

SET @ACCION_BANDEJA_ID  = (SELECT Id FROM AccionBandeja WHERE IdAccion = 'HISTORIALOFICIO');


INSERT INTO 
  dbo.AccionesPermitidasBandejaOficio
	( BandejaId, EstadoId, EtapaId, AccionId ) 
  SELECT DISTINCT band.BandejaId, esta.EstadoId, eta.EtapaId, @ACCION_BANDEJA_ID FROM 
   (SELECT DISTINCT BandejaId FROM AccionesPermitidasBandejaOficio) as band
    JOIN (SELECT DISTINCT EstadoId FROM AccionesPermitidasBandejaOficio) esta ON 1=1
    JOIN (SELECT DISTINCT EtapaId FROM AccionesPermitidasBandejaOficio) eta ON 1=1
    WHERE EXISTS (SELECT BandejaId FROM AccionesPermitidasBandejaOficio 
                      WHERE EstadoId = esta.EstadoId AND EtapaId = eta.EtapaId AND (band.BandejaId = 1 OR BandejaId > band.BandejaId))
GO

/* Inidces para optimizar la query de historial de oficios */
CREATE NONCLUSTERED INDEX IDX_LogSistema_Origen ON dbo.LogSistema
  (Origen)
WITH (
  PAD_INDEX = OFF,
  DROP_EXISTING = OFF,
  STATISTICS_NORECOMPUTE = OFF,
  SORT_IN_TEMPDB = OFF,
  ONLINE = OFF,
  ALLOW_ROW_LOCKS = ON,
  ALLOW_PAGE_LOCKS = ON)
GO