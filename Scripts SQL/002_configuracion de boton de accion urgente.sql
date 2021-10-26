DECLARE @BANDEJA_ENTRADA_ID INT,
        @ESTADO_OFICIO_ID INT,
        @ETAPA_OFICIO_ID INT,
        @ACCION_BANDEJA_ID INT;

/* CREAR ACCION URGENTE */
IF NOT EXISTS (SELECT 1 FROM AccionBandeja WHERE IdAccion = 'URGENTE')
BEGIN
    INSERT INTO [dbo].[AccionBandeja] ([Titulo], [Icono], [Hint], [TipoAccion], [Orden], [IdAccion], [Onclick])
    VALUES ('Marcar Urgente', 'warning-white.png', 'Urgente', 'O', '2', 'URGENTE', 'marcarUrgente')
END

SET @ACCION_BANDEJA_ID  = (SELECT Id FROM AccionBandeja WHERE IdAccion = 'URGENTE');

/* AGREGAR BANDEJAS, ESTADOS Y ETAPAS */
/*---------------------------------------------------------------------------------------------------------------*/

SET @BANDEJA_ENTRADA_ID = (SELECT Id FROM BandejaEntrada WHERE Titulo = 'Bandeja de Entrada de Profesional UT');
SET @ESTADO_OFICIO_ID   = (SELECT Id FROM EstadoOficio WHERE Titulo = 'Borrador');
SET @ETAPA_OFICIO_ID    = (SELECT Id FROM EtapaOficio WHERE Titulo = 'Revisión Profesional UT');

INSERT INTO [dbo].[AccionesPermitidasBandejaOficio] ([BandejaId], [EstadoId], [EtapaId], [AccionId] )
VALUES(@BANDEJA_ENTRADA_ID, @ESTADO_OFICIO_ID, @ETAPA_OFICIO_ID, @ACCION_BANDEJA_ID);

/*---------------------------------------------------------------------------------------------------------------*/

SET @ESTADO_OFICIO_ID   = (SELECT Id FROM EstadoOficio WHERE Titulo = 'Correcciones Encargado UT');

INSERT INTO [dbo].[AccionesPermitidasBandejaOficio] ([BandejaId], [EstadoId], [EtapaId], [AccionId] )
VALUES(@BANDEJA_ENTRADA_ID, @ESTADO_OFICIO_ID, @ETAPA_OFICIO_ID, @ACCION_BANDEJA_ID);

/*---------------------------------------------------------------------------------------------------------------*/

SET @BANDEJA_ENTRADA_ID = (SELECT Id FROM BandejaEntrada WHERE Titulo = 'Bandeja de Entrada de Encargado UT');
SET @ESTADO_OFICIO_ID   = (SELECT Id FROM EstadoOficio WHERE Titulo = 'Enviado a Encargado UT');
SET @ETAPA_OFICIO_ID    = (SELECT Id FROM EtapaOficio WHERE Titulo = 'Revisión Encargado UT');

INSERT INTO [dbo].[AccionesPermitidasBandejaOficio] ([BandejaId], [EstadoId], [EtapaId], [AccionId] )
VALUES(@BANDEJA_ENTRADA_ID, @ESTADO_OFICIO_ID, @ETAPA_OFICIO_ID, @ACCION_BANDEJA_ID);

/*---------------------------------------------------------------------------------------------------------------*/

SET @ESTADO_OFICIO_ID   = (SELECT Id FROM EstadoOficio WHERE Titulo = 'Correcciones Encargado UT');

INSERT INTO [dbo].[AccionesPermitidasBandejaOficio] ([BandejaId], [EstadoId], [EtapaId], [AccionId] )
VALUES(@BANDEJA_ENTRADA_ID, @ESTADO_OFICIO_ID, @ETAPA_OFICIO_ID, @ACCION_BANDEJA_ID);

/*---------------------------------------------------------------------------------------------------------------*/

SET @BANDEJA_ENTRADA_ID = (SELECT Id FROM BandejaEntrada WHERE Titulo = 'Bandeja de Entrada de Jefatura CMN');
SET @ESTADO_OFICIO_ID   = (SELECT Id FROM EstadoOficio WHERE Titulo = 'Aprobado Visador General');
SET @ETAPA_OFICIO_ID    = (SELECT Id FROM EtapaOficio WHERE Titulo = 'Revisión Secretario Técnico');

INSERT INTO [dbo].[AccionesPermitidasBandejaOficio] ([BandejaId], [EstadoId], [EtapaId], [AccionId] )
VALUES(@BANDEJA_ENTRADA_ID, @ESTADO_OFICIO_ID, @ETAPA_OFICIO_ID, @ACCION_BANDEJA_ID);

/*---------------------------------------------------------------------------------------------------------------*/

SET @BANDEJA_ENTRADA_ID = (SELECT Id FROM BandejaEntrada WHERE Titulo = 'Bandeja de Entrada de Secretaria UT');
SET @ESTADO_OFICIO_ID   = (SELECT Id FROM EstadoOficio WHERE Titulo = 'Enviado a Encargado UT');
SET @ETAPA_OFICIO_ID    = (SELECT Id FROM EtapaOficio WHERE Titulo = 'Revisión Encargado UT');

INSERT INTO [dbo].[AccionesPermitidasBandejaOficio] ([BandejaId], [EstadoId], [EtapaId], [AccionId] )
VALUES(@BANDEJA_ENTRADA_ID, @ESTADO_OFICIO_ID, @ETAPA_OFICIO_ID, @ACCION_BANDEJA_ID);

/*---------------------------------------------------------------------------------------------------------------*/

SET @ESTADO_OFICIO_ID   = (SELECT Id FROM EstadoOficio WHERE Titulo = 'Correcciones Visador General');

INSERT INTO [dbo].[AccionesPermitidasBandejaOficio] ([BandejaId], [EstadoId], [EtapaId], [AccionId] )
VALUES(@BANDEJA_ENTRADA_ID, @ESTADO_OFICIO_ID, @ETAPA_OFICIO_ID, @ACCION_BANDEJA_ID);

/*---------------------------------------------------------------------------------------------------------------*/

SET @BANDEJA_ENTRADA_ID = (SELECT Id FROM BandejaEntrada WHERE Titulo = 'Bandeja de Entrada de Administrador y Visador General');
SET @ESTADO_OFICIO_ID   = (SELECT Id FROM EstadoOficio WHERE Titulo = 'Aprobado Encargado UT');
SET @ETAPA_OFICIO_ID    = (SELECT Id FROM EtapaOficio WHERE Titulo = 'Revisión Visador General');

INSERT INTO [dbo].[AccionesPermitidasBandejaOficio] ([BandejaId], [EstadoId], [EtapaId], [AccionId] )
VALUES(@BANDEJA_ENTRADA_ID, @ESTADO_OFICIO_ID, @ETAPA_OFICIO_ID, @ACCION_BANDEJA_ID);

/*---------------------------------------------------------------------------------------------------------------*/

SET @ESTADO_OFICIO_ID   = (SELECT Id FROM EstadoOficio WHERE Titulo = 'Correcciones Jefatura CMN');

INSERT INTO [dbo].[AccionesPermitidasBandejaOficio] ([BandejaId], [EstadoId], [EtapaId], [AccionId] )
VALUES(@BANDEJA_ENTRADA_ID, @ESTADO_OFICIO_ID, @ETAPA_OFICIO_ID, @ACCION_BANDEJA_ID);

/*

(1 row affected)
(1 row affected)
(1 row affected)
(1 row affected)
(1 row affected)
(1 row affected)
(1 row affected)
(1 row affected)
(1 row affected)
(1 row affected)

*/