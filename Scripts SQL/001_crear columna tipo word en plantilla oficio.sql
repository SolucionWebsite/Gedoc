IF NOT EXISTS ( SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PlantillaOficio' AND COLUMN_NAME = 'TipoWord')
BEGIN
    ALTER TABLE [dbo].[PlantillaOficio]
    ADD [TipoWord] BIT NOT NULL DEFAULT 0;
END;

/*

Commands completed successfully.

*/
