IF NOT EXISTS ( SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Oficio' AND COLUMN_NAME = 'Urgente')
BEGIN
    ALTER TABLE [dbo].[Oficio]
    ADD [Urgente] BIT NOT NULL DEFAULT 0;
END;

/*

Commands completed successfully.

*/