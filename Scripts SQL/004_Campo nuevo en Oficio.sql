ALTER TABLE dbo.Oficio
ADD UltimoUsuarioFlujoId int NULL
GO

ALTER TABLE dbo.Oficio
ADD CONSTRAINT FK_Oficio_UltimoUsuarioFlujoId FOREIGN KEY (UltimoUsuarioFlujoId) 
  REFERENCES dbo.Usuario (Id) 
  ON UPDATE NO ACTION
  ON DELETE NO ACTION
GO

UPDATE ofi
  	SET UnidadTecnicaId = re.UtAsignadaId
  FROM Oficio ofi
  JOIN RequerimientosOficios ro ON ofi.Id = ro.OficioId
  JOIN Requerimiento re ON re.Id = ro.RequerimientoId  
 
UPDATE Oficio
	SET UltimoUsuarioFlujoId = UsuarioModificacionId
GO
