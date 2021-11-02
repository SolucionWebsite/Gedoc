IF NOT EXISTS ( SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PlantillaOficio' AND COLUMN_NAME = 'NombreDocumento')
BEGIN
    ALTER TABLE [dbo].[PlantillaOficio]
    ADD [NombreDocumento] NVARCHAR(200);
END;

/*

Commands completed successfully.

*/
