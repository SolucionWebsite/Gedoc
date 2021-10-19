using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Servicios
{
    public static class Queries
    {
        #region Requerimientos
        public static string SelectRequerimientosOrigen = "SELECT * FROM etl.vw_RequerimientoMigrar WHERE Modified >= @fechaD AND Modified <= @fechaH";

        public static string SelectRequerimientosOrigenResumen = "SELECT Id FROM dbo.Requerimiento";

        public static string InsertRequerimientoCarga =
  "INSERT INTO " +
    " dbo.RequerimientosCarga " +
    "( " +
    " Adjunta_x0020_documentaci_x00f3_, " +
    " G_x00e9_nero_x0020__x0028_Nuevo_, " +
    " Car_x00e1_cter, " +
    " Redireccionado, " +
    " Requiere_x0020_respuesta, " +
    " Requiere_x0020_acuerdo, " +
    " Enviar_x0020_a_x0020_Priorizaci_, " +
    " Etapa, " +
    " Prioridad_x0020_del_x0020_requer, " +
    " Enviar_x0020_a_x0020_Asignaci_x0, " +
    " Devolver_x0020_a_x0020_Ingreso_x, " +
    " Enviar_x0020_a_x0020_UT_x003A_, " +
    " Tipo_x0020_Instituci_x00f3_n, " +
    " Requiere_x0020_timbraje_x0020_de, " +
    " Estado_x0020_del_x0020_requerimi, " +
    " Motivo_x0020_de_x0020_cierre, " +
    " Tipo_x0020_Ingreso, " +
    " Cerrar_x0020_requerimiento, " +
            //" ContentType, " +
            //" BaseName, " +
            //" ContentTypeId, " +
    " ID, " +
    " Fecha_x0020_de_x0020_ingreso, " +
    " Fecha_x0020_de_x0020_documento, " +
    " Fecha_x0020_de_x0020_asignaci_x0, " +
    " Fecha_x0020_de_x0020_asignaci_x00, " +
    " Entrega_x0020_a_x0020_C_x002d_DO, " +
    " Fecha_x0020_de_x0020_devoluci_x0, " +
    " Fecha_x0020_asignaci_x00f3_n_x00, " +
    " Asignado_x0020_a_x0020_UT, " +
    " Asignado_x0020_a_x0020_Responsab, " +
    " Enviado_x0020_a_x0020_UT, " +
    " Fecha_x0020_de_x0020_respuesta, " +
    " Fecha_x0020_de_x0020_acuerdo_x00, " +
    " Fecha_x0020_de_x0020_acuerdo_x000, " +
    " Fecha_x0020_de_x0020_oficio, " +
    " Fecha_x0020_de_x0020_env_x00ed_o, " +
    " Fecha_x0020_de_x0020_cierre, " +
    " Notificado, " +
    " Prof_fecha, " +
    " Fecha_x0020_de_x0020_ingreso_x00, " +
    " Fecha_x0020_vac_x00ed_a, " +
    " Fecha_x0020_de_x0020_Recepci_x00, " +
    " Fecha_x0020_de_x0020_liberaci_x0, " +
    " Fecha_x0020_de_x0020_Resoluci_x00, " +
    " Modified, " +
    " Created, " +
    " Tipo_x0020_de_x0020_documento, " +
    " Tipo_x0020_de_x0020_documento_x0, " +
    " Remitente, " +
    " Remitente_x003A_T_x00ed_tulo, " +
    " Forma_x0020_de_x0020_llegada, " +
    " Forma_x0020_de_x0020_llegada_x00, " +
    " Unidad_x0020_T_x00e9_cnica_x0020, " +
    " Unidad_x0020_T_x00e9_cnica_x00200, " +
    " Reasignaci_x00f3_n, " +
    " Remitente_x003a_Cargo_x0020_o_x0, " +
    " Remitente_x003a_Instituci_x00f3_, " +
    " UT_x0020_asignada_x0020_anterior, " +
    " Listado_x0020_de_x0020_solicitan, " +
    " Remitente_x003a_G_x00e9_nero_x00, " +
            //" Tipo_x0020_de_x0020_documento_x00, " +
    " Unidad_x0020_T_x00e9_cnica_x00204, " +
    " UT_x0020_Responsable_x0020_Trans, " +
    " Unidad_x0020_T_x00e9_cnica_x00205, " +
    " Unidad_x0020_T_x00e9_cnica_x00206, " +
    " Unidad_x0020_T_x00e9_cnica_x00207, " +
    " Requerimiento_x0020_anterior, " +
    " Requerimiento_x0020_anterior_x00, " +
    " FileRef, " +
    " FileDirRef, " +
    " Last_x0020_Modified, " +
    " Created_x0020_Date, " +
    " Tipos_x0020_de_x0020_adjuntos, " +
    " Tipos_x0020_de_x0020_adjuntos_x0, " +
    " Soporte, " +
    " Soporte_x003A_T_x00ed_tulo, " +
    " Tipo_x0020_de_x0020_monumento, " +
    " Tipo_x0020_de_x0020_monumento_x0, " +
    " Etiqueta, " +
    " Etiqueta_x003A_T_x00ed_tulo, " +
    " Unidad_x0020_T_x00e9_cnica_x00201, " +
    " Unidad_x0020_T_x00e9_cnica_x00202, " +
    " Unidad_x0020_T_x00e9_cnica_x00203, " +
    " Nombre_x0020_Comuna, " +
    " Nombre_x0020_Comuna_x003A_Nombre, " +
    " Nombre_x0020_Comuna_x003A_C_x00f, " +
    " Nombre_x0020_Comuna_x003A_Nombre0, " +
    " Nombre_x0020_Comuna_x003A_C_x00f0, " +
    " Nombre_x0020_Comuna_x003A_Nombre1, " +
    " Nombre_x0020_Comuna_x003A_C_x00f1, " +
    " Regi_x00f3_n, " +
    " Observaciones_x0020_al_x0020_tip, " +
    " Observaciones_x0020_de_x0020_adj, " +
    " Materia, " +
    " Observaciones_x0020_de_x0020_la_, " +
    " Observaciones_x0020_del_x0020_ca, " +
    " Comentario_x0020_de_x0020_asigna, " +
    " Comentario_x0020_de_x0020_devolu, " +
    " Comentario_x0020_de_x0020_cierre, " +
    " Observaciones_x0020_del_x0020_in, " +
    " Ultimo_x0020_acuerdo_x0020_sesio, " +
    " Ultimo_x0020_acuerdo_x0020_comis, " +
    " txt_Etiqueta, " +
    " txt_Soporte, " +
    " txt_TipoDeAdjuntos, " +
    " txt_FormaDeLlegada, " +
    " txt_UTenCopia, " +
    " Descripci_x00f3_n_x0020_de_x0020, " +
    " Comentario_x0020_de_x0020_Encarg, " +
    " Observaciones_x0020_de_x0020_Tra, " +
    " Comentario_x0020_de_x0020_Encarg0, " +
    " N_x00fa_mero_x0020_de_x0020_ingr, " +
    " Cantidad_x0020_de_x0020_adjuntos, " +
    " Plazo, " +
    " Title, " +
    " File_x0020_Type, " +
    " Documento_x0020_de_x0020_ingreso, " +
    " Nombre_x0020__x0028_Nuevo_x0020_, " +
    " Direcci_x00f3_n_x0020__x0028_Nue, " +
    " Correo_x0020_electr_x00f3_nico_x, " +
    " Tel_x00e9_fono_x0020__x0028_Nuev, " +
    " Requerimiento_x0020_no_x0020_reg, " +
    " Monumento_x0020_Nacional_x0020_i, " +
    " Direcci_x00f3_n_x0020_Monumento_, " +
    " Referencia_x0020_en_x0020_C_x002, " +
    " Cargo_x0020_o_x0020_Profesi_x00f, " +
    " Instituci_x00f3_n_x0020__x0028_N, " +
    " Nombre_x0020_de_x0020_proyecto, " +
    " UT, " +
    " N_x00fa_mero_x0020_de_x0020_tick, " +
    " txt_RemitenteNombre, " +
    " txt_RemitenteInstitucion, " +
    " txt_TipoDeDocumento, " +
    " Usuario_x0020_notificado, " +
    " Prof_correo, " +
    " txtUTenCopiaCorreos, " +
    " txtProfesionalEnArea, " +
    " Rol_x0020_MN, " +
    " Requerimiento_x0020_Anterior_x000, " +
    " _CopySource, " +
    " _UIVersionString, " +
    " URL_x0020_DocSet_x0020_Adjuntos, " +
    " URL_x0020_DocSet_x0020_Despachos, " +
    " Ficha, " +
    " URL_x0020_Cierre_x0020_individua, " +
    " Solicitante_x0020_de_x0020_urgen, " +
    " Responsable_x0020_del_x0020_requ, " +
    " Responsable_x0020_en_x0020_Arque, " +
    " Responsable_x0020_en_x0020_Arqui, " +
    " Responsable_x0020_en_x0020_Coord, " +
    " Responsable_x0020_en_x0020_Educa, " +
    " Responsable_x0020_en_x0020_GIE, " +
    " Responsable_x0020_en_x0020_Ingre, " +
    " Responsable_x0020_en_x0020_Jefat, " +
    " Responsable_x0020_en_x0020_Jur_x, " +
    " Responsable_x0020_en_x0020_P_x00, " +
    " Responsable_x0020_en_x0020_P_x000, " +
    " Responsable_x0020_en_x0020_P_x001, " +
    " Responsable_x0020_en_x0020_Plan_, " +
    " Responsable_x0020_en_x0020_Regio, " +
    " Responsable_x0020_en_x0020_SEIA, " +
    " Responsable_x0020_UT, " +
    " Cerrado_x0020_por, " +
    " Profesional_x0020_en_x0020_Gesti, " +
    " Profesional_x0020_en_x0020_Terri, " +
    " Responsable_x0020_Transparencia, " +
    " Profesional_x0020_UT_x0020_Respo, " +
    " Profesional_x0020_UT_x0020_Tempo, " +
    " Author, " +
    " Editor, " +
    " IdCarga, " +
    " UniqueId, " +
    " Fecha_x0020_de_x0020_resoluci_x0, " +
   "Canal_x0020_de_x0020_llegada_x00, " +
   "Referencia_x0020_de_x0020_locali, " +
   "Nombre_x0020_o_x0020_uso_x0020_a, " +
   "Otras_x0020_denominaciones, " +
   "C_x00f3_digo_x0020_Monumento_x00, " +
   "Provincia, " +
   "N_x00fa_mero_x0020_de_x0020_caso, " +
   "Tipo_x0020_de_x0020_tr_x00e1_mit, " +
   "Remitente_x003a_Rut " +
  ") " +
  "VALUES ( " +
    " @Adjunta_x0020_documentaci_x00f3_, " +
    " @G_x00e9_nero_x0020__x0028_Nuevo_, " +
    " @Car_x00e1_cter, " +
    " @Redireccionado, " +
    " @Requiere_x0020_respuesta, " +
    " @Requiere_x0020_acuerdo, " +
    " @Enviar_x0020_a_x0020_Priorizaci_, " +
    " @Etapa, " +
    " @Prioridad_x0020_del_x0020_requer, " +
    " @Enviar_x0020_a_x0020_Asignaci_x0, " +
    " @Devolver_x0020_a_x0020_Ingreso_x, " +
    " @Enviar_x0020_a_x0020_UT_x003A_, " +
    " @Tipo_x0020_Instituci_x00f3_n, " +
    " @Requiere_x0020_timbraje_x0020_de, " +
    " @Estado_x0020_del_x0020_requerimi, " +
    " @Motivo_x0020_de_x0020_cierre, " +
    " @Tipo_x0020_Ingreso, " +
    " @Cerrar_x0020_requerimiento, " +
            //" @ContentType, " +
            //" @BaseName, " +
            //" @ContentTypeId, " +
    " @ID, " +
    " @Fecha_x0020_de_x0020_ingreso, " +
    " @Fecha_x0020_de_x0020_documento, " +
    " @Fecha_x0020_de_x0020_asignaci_x0, " +
    " @Fecha_x0020_de_x0020_asignaci_x00, " +
    " @Entrega_x0020_a_x0020_C_x002d_DO, " +
    " @Fecha_x0020_de_x0020_devoluci_x0, " +
    " @Fecha_x0020_asignaci_x00f3_n_x00, " +
    " @Asignado_x0020_a_x0020_UT, " +
    " @Asignado_x0020_a_x0020_Responsab, " +
    " @Enviado_x0020_a_x0020_UT, " +
    " @Fecha_x0020_de_x0020_respuesta, " +
    " @Fecha_x0020_de_x0020_acuerdo_x00, " +
    " @Fecha_x0020_de_x0020_acuerdo_x000, " +
    " @Fecha_x0020_de_x0020_oficio, " +
    " @Fecha_x0020_de_x0020_env_x00ed_o, " +
    " @Fecha_x0020_de_x0020_cierre, " +
    " @Notificado, " +
    " @Prof_fecha, " +
    " @Fecha_x0020_de_x0020_ingreso_x00, " +
    " @Fecha_x0020_vac_x00ed_a, " +
    " @Fecha_x0020_de_x0020_Recepci_x00, " +
    " @Fecha_x0020_de_x0020_liberaci_x0, " +
    " @Fecha_x0020_de_x0020_Resoluci_x00, " +
    " @Modified, " +
    " @Created, " +
    " @Tipo_x0020_de_x0020_documento, " +
    " @Tipo_x0020_de_x0020_documento_x0, " +
    " @Remitente, " +
    " @Remitente_x003A_T_x00ed_tulo, " +
    " @Forma_x0020_de_x0020_llegada, " +
    " @Forma_x0020_de_x0020_llegada_x00, " +
    " @Unidad_x0020_T_x00e9_cnica_x0020, " +
    " @Unidad_x0020_T_x00e9_cnica_x00200, " +
    " @Reasignaci_x00f3_n, " +
    " @Remitente_x003a_Cargo_x0020_o_x0, " +
    " @Remitente_x003a_Instituci_x00f3_, " +
    " @UT_x0020_asignada_x0020_anterior, " +
    " @Listado_x0020_de_x0020_solicitan, " +
    " @Remitente_x003a_G_x00e9_nero_x00, " +
            //" @Tipo_x0020_de_x0020_documento_x00, " +
    " @Unidad_x0020_T_x00e9_cnica_x00204, " +
    " @UT_x0020_Responsable_x0020_Trans, " +
    " @Unidad_x0020_T_x00e9_cnica_x00205, " +
    " @Unidad_x0020_T_x00e9_cnica_x00206, " +
    " @Unidad_x0020_T_x00e9_cnica_x00207, " +
    " @Requerimiento_x0020_anterior, " +
    " @Requerimiento_x0020_anterior_x00, " +
    " @FileRef, " +
    " @FileDirRef, " +
    " @Last_x0020_Modified, " +
    " @Created_x0020_Date, " +
    " @Tipos_x0020_de_x0020_adjuntos, " +
    " @Tipos_x0020_de_x0020_adjuntos_x0, " +
    " @Soporte, " +
    " @Soporte_x003A_T_x00ed_tulo, " +
    " @Tipo_x0020_de_x0020_monumento, " +
    " @Tipo_x0020_de_x0020_monumento_x0, " +
    " @Etiqueta, " +
    " @Etiqueta_x003A_T_x00ed_tulo, " +
    " @Unidad_x0020_T_x00e9_cnica_x00201, " +
    " @Unidad_x0020_T_x00e9_cnica_x00202, " +
    " @Unidad_x0020_T_x00e9_cnica_x00203, " +
    " @Nombre_x0020_Comuna, " +
    " @Nombre_x0020_Comuna_x003A_Nombre, " +
    " @Nombre_x0020_Comuna_x003A_C_x00f, " +
    " @Nombre_x0020_Comuna_x003A_Nombre0, " +
    " @Nombre_x0020_Comuna_x003A_C_x00f0, " +
    " @Nombre_x0020_Comuna_x003A_Nombre1, " +
    " @Nombre_x0020_Comuna_x003A_C_x00f1, " +
    " @Regi_x00f3_n, " +
    " @Observaciones_x0020_al_x0020_tip, " +
    " @Observaciones_x0020_de_x0020_adj, " +
    " @Materia, " +
    " @Observaciones_x0020_de_x0020_la_, " +
    " @Observaciones_x0020_del_x0020_ca, " +
    " @Comentario_x0020_de_x0020_asigna, " +
    " @Comentario_x0020_de_x0020_devolu, " +
    " @Comentario_x0020_de_x0020_cierre, " +
    " @Observaciones_x0020_del_x0020_in, " +
    " @Ultimo_x0020_acuerdo_x0020_sesio, " +
    " @Ultimo_x0020_acuerdo_x0020_comis, " +
    " @txt_Etiqueta, " +
    " @txt_Soporte, " +
    " @txt_TipoDeAdjuntos, " +
    " @txt_FormaDeLlegada, " +
    " @txt_UTenCopia, " +
    " @Descripci_x00f3_n_x0020_de_x0020, " +
    " @Comentario_x0020_de_x0020_Encarg, " +
    " @Observaciones_x0020_de_x0020_Tra, " +
    " @Comentario_x0020_de_x0020_Encarg0, " +
    " @N_x00fa_mero_x0020_de_x0020_ingr, " +
    " @Cantidad_x0020_de_x0020_adjuntos, " +
    " @Plazo, " +
    " @Title, " +
    " @File_x0020_Type, " +
    " @Documento_x0020_de_x0020_ingreso, " +
    " @Nombre_x0020__x0028_Nuevo_x0020_, " +
    " @Direcci_x00f3_n_x0020__x0028_Nue, " +
    " @Correo_x0020_electr_x00f3_nico_x, " +
    " @Tel_x00e9_fono_x0020__x0028_Nuev, " +
    " @Requerimiento_x0020_no_x0020_reg, " +
    " @Monumento_x0020_Nacional_x0020_i, " +
    " @Direcci_x00f3_n_x0020_Monumento_, " +
    " @Referencia_x0020_en_x0020_C_x002, " +
    " @Cargo_x0020_o_x0020_Profesi_x00f, " +
    " @Instituci_x00f3_n_x0020__x0028_N, " +
    " @Nombre_x0020_de_x0020_proyecto, " +
    " @UT, " +
    " @N_x00fa_mero_x0020_de_x0020_tick, " +
    " @txt_RemitenteNombre, " +
    " @txt_RemitenteInstitucion, " +
    " @txt_TipoDeDocumento, " +
    " @Usuario_x0020_notificado, " +
    " @Prof_correo, " +
    " @txtUTenCopiaCorreos, " +
    " @txtProfesionalEnArea, " +
    " @Rol_x0020_MN, " +
    " @Requerimiento_x0020_Anterior_x000, " +
    " @_CopySource, " +
    " @_UIVersionString, " +
    " @URL_x0020_DocSet_x0020_Adjuntos, " +
    " @URL_x0020_DocSet_x0020_Despachos, " +
    " @Ficha, " +
    " @URL_x0020_Cierre_x0020_individua, " +
    " @Solicitante_x0020_de_x0020_urgen, " +
    " @Responsable_x0020_del_x0020_requ, " +
    " @Responsable_x0020_en_x0020_Arque, " +
    " @Responsable_x0020_en_x0020_Arqui, " +
    " @Responsable_x0020_en_x0020_Coord, " +
    " @Responsable_x0020_en_x0020_Educa, " +
    " @Responsable_x0020_en_x0020_GIE, " +
    " @Responsable_x0020_en_x0020_Ingre, " +
    " @Responsable_x0020_en_x0020_Jefat, " +
    " @Responsable_x0020_en_x0020_Jur_x, " +
    " @Responsable_x0020_en_x0020_P_x00, " +
    " @Responsable_x0020_en_x0020_P_x000, " +
    " @Responsable_x0020_en_x0020_P_x001, " +
    " @Responsable_x0020_en_x0020_Plan_, " +
    " @Responsable_x0020_en_x0020_Regio, " +
    " @Responsable_x0020_en_x0020_SEIA, " +
    " @Responsable_x0020_UT, " +
    " @Cerrado_x0020_por, " +
    " @Profesional_x0020_en_x0020_Gesti, " +
    " @Profesional_x0020_en_x0020_Terri, " +
    " @Responsable_x0020_Transparencia, " +
    " @Profesional_x0020_UT_x0020_Respo, " +
    " @Profesional_x0020_UT_x0020_Tempo, " +
    " @Author, " +
    " @Editor," +
    " @IdCarga," +
    " @UniqueId," +
    " @Fecha_x0020_de_x0020_resoluci_x0, " +
      "@Canal_x0020_de_x0020_llegada_x00, " +
      "@Referencia_x0020_de_x0020_locali, " +
      "@Nombre_x0020_o_x0020_uso_x0020_a, " +
      "@Otras_x0020_denominaciones, " +
      "@C_x00f3_digo_x0020_Monumento_x00, " +
      "@Provincia, " +
      "@N_x00fa_mero_x0020_de_x0020_caso, " +
      "@Tipo_x0020_de_x0020_tr_x00e1_mit, " +
      "@Remitente_x003a_Rut " +
  ")";

#endregion

        #region Log
        public static string InsertLog =
            "INSERT INTO " +
            "dbo.LogEtl " +
            "( " +
            "    Tipo, " +
            "    Fecha, " +
            "    Descripcion, " +
            "    ParentLogId " +
            ") " +
            "VALUES ( " +
            "@Tipo, " +
            "@Fecha, " +
            "@Descripcion, " +
            "@ParentLogId " +
            ") ";

        public static string UltimaFechaLog = "SELECT TOP 1 Fecha FROM LogEtl WHERE Tipo=@Tipo ORDER BY Id desc";

        public static string UltimosLog = "SELECT * FROM UltimosLogs ORDER BY Id asc";

        public static string EstadoEjecucion = "select top 1 * from LogEtl " +
              " where tipo like 'EJECUTANDO-CARGA-DATOS%' OR tipo like 'FIN-CARGA-DATOS' order by id desc ";


        #endregion

        #region Bitacora
        public static string SelectBitacorasOrigen = "SELECT * FROM etl.vw_BitacoraMigrar WHERE Modified >= @fechaD AND Modified <= @fechaH";

        public static string SelectBitacorasOrigenResumen = "SELECT Id FROM dbo.Bitacora";

        public static string InsertBitacoraCarga =
            "   INSERT INTO" +
            " dbo.BitacorasCarga" +
            "   (" +
            " JCMN_notif, " +
            " Desp_notif, " +
            " SolRev_notif, " +
            " solRev_EsperarRespuesta, " +
            //" _HasCopyDestinations, " +
            //" _IsCurrentVersion, " +
            " Estado_x0020_del_x0020_registro, " +
            " Enviar_x0020_solicitud, " +
            " ContentType, " +
            //" Edit, " +
            //" LinkTitleNoMenu, " +
            //" LinkTitle, " +
            //" LinkTitle2, " +
            //" LinkFilenameNoMenu, " +
            //" LinkFilename, " +
            //" LinkFilename2, " +
            //" ServerUrl, " +
            //" EncodedAbsUrl, " +
            //" BaseName, " +
            " ContentTypeId, " +
            " ID, " +
            " Fecha, " +
            " Fecha_x0020_solicitud_x0020_revi, " +
            " Fecha_x0020_solicitud_x0020_desp, " +
            " JCMN_fecha, " +
            " Desp_fecha, " +
            " SolRev_fecha, " +
            " Modified, " +
            " Created, " +
            " Requerimiento, " +
            " Documento_x0020_ingreso_x003a_ID, " +
            " Documento_x0020_ingreso_x003A_Fe, " +
            " Documento_x0020_ingreso_x003a_N_, " +
            " Numero_x0020_de_x0020_Despacho, " +
            " FileRef, " +
            " FileDirRef, " +
            " Last_x0020_Modified, " +
            " Created_x0020_Date, " +
            " Observaciones, " +
            " _x00da_ltimo_x0020_comentario, " +
            " Title, " +
            " File_x0020_Type, " +
            " JCMN_correo, " +
            " Desp_correo, " +
            " SolRev_correo, " +
            " Solicitar_x0020_revisi_x00f3_n_x, " +
            " Solicitar_x0020_Despacho_x0020_a, " +
            " Usuario_x0020_solicitud_x0020_re, " +
            " Usuario_x0020_solicitud_x0020_de, " +
            " Author, " +
            " Editor, " +
            " UniqueId, " +
            " IdCarga" +
            "   ) " +
            "   VALUES ( " +
            " @JCMN_notif, " +
            " @Desp_notif, " +
            " @SolRev_notif, " +
            " @solRev_EsperarRespuesta, " +
            //" @_HasCopyDestinations, " +
            //" @_IsCurrentVersion, " +
            " @Estado_x0020_del_x0020_registro, " +
            " @Enviar_x0020_solicitud, " +
            " @ContentType, " +
            //" @Edit, " +
            //" @LinkTitleNoMenu, " +
            //" @LinkTitle, " +
            //" @LinkTitle2, " +
            //" @LinkFilenameNoMenu, " +
            //" @LinkFilename, " +
            //" @LinkFilename2, " +
            //" @ServerUrl, " +
            //" @EncodedAbsUrl, " +
            //" @BaseName, " +
            " @ContentTypeId, " +
            " @ID, " +
            " @Fecha, " +
            " @Fecha_x0020_solicitud_x0020_revi, " +
            " @Fecha_x0020_solicitud_x0020_desp, " +
            " @JCMN_fecha, " +
            " @Desp_fecha, " +
            " @SolRev_fecha, " +
            " @Modified, " +
            " @Created, " +
            " @Requerimiento, " +
            " @Documento_x0020_ingreso_x003a_ID, " +
            " @Documento_x0020_ingreso_x003A_Fe, " +
            " @Documento_x0020_ingreso_x003a_N_, " +
            " @Numero_x0020_de_x0020_Despacho, " +
            " @FileRef, " +
            " @FileDirRef, " +
            " @Last_x0020_Modified, " +
            " @Created_x0020_Date, " +
            " @Observaciones, " +
            " @_x00da_ltimo_x0020_comentario, " +
            " @Title, " +
            " @File_x0020_Type, " +
            " @JCMN_correo, " +
            " @Desp_correo, " +
            " @SolRev_correo, " +
            " @Solicitar_x0020_revisi_x00f3_n_x, " +
            " @Solicitar_x0020_Despacho_x0020_a, " +
            " @Usuario_x0020_solicitud_x0020_re, " +
            " @Usuario_x0020_solicitud_x0020_de, " +
            " @Author, " +
            " @Editor," +
            " @UniqueId," +
            " @IdCarga" +
            "   )";

        #endregion

        #region Despachos
        public static string SelectDespachosOrigen = "SELECT * FROM etl.vw_DespachoMigrar WHERE Modified >= @fechaD AND Modified <= @fechaH";

        public static string SelectDespachosOrigenResumen = "SELECT Id FROM dbo.Despacho";

        public static string InsertDespachoCarga =
    "INSERT INTO dbo.DespachosCarga" +
  "(" +
    "Estado_x0020_del_x0020_registro," +
    "Estado_x0020_del_x0020_despacho," +
    "G_x00e9_nero_x0020_destinatario_x0020_nuevo," +
    "Tipo_x0020_de_x0020_Instituci_x00f3_n_x0020_destinatario_x0020_nuevo," +
    "ContentType," +
    "LinkFilenameNoMenu," +
    "LinkFilename," +
    "LinkFilename2," +
    "ServerUrl," +
    "BaseName," +
    "FileSizeDisplay," +
    "SelectTitle," +
    "SelectFilename," +
    "ContentTypeId," +
    "ID," +
    "Fecha_x0020_de_x0020_recepci_x00f3_n," +
    "Fecha_x0020_emisi_x00f3_n_x0020_oficio," +
    "Fecha_x0020_de_x0020_env_x00ed_o1," +
    "Fecha," +
    "Created," +
    "Modified," +
    "GUID," +
    "Proveedor_x0020_de_x0020_despacho," +
    "Medio_x0020_de_x0020_despacho," +
    "Medio_x0020_de_x0020_verificaci_x00f3_n," +
    "Proveedor_x0020_de_x0020_despacho_x003A_T_x00ed_tulo," +
    "Medio_x0020_de_x0020_despacho_x003A_T_x00ed_tulo," +
    "Medio_x0020_de_x0020_verificaci_x00f3_n_x003A_T_x00ed_tulo," +
    "Destinatario," +
    "Destinatario_x003A_Correo_x0020_electr_x00f3_nico," +
    "Destinatario_x003a_Nombre," +
    "Destinatario_x003a_Instituci_x00f3_n," +
    "Unidad_x0020_T_x00e9_cnica_x0020_Asignada," +
    "FileRef," +
    "FileDirRef," +
    "Last_x0020_Modified," +
    "Created_x0020_Date," +
    "File_x0020_Size," +
    "UniqueId," +
    "Tipos_x0020_de_x0020_adjuntos," +
    "Tipos_x0020_de_x0020_adjuntos_x003A_T_x00ed_tulo," +
    "Soporte," +
    "Soporte_x003A_T_x00ed_tulo," +
    "Requerimiento," +
    "Documento_x0020_ingreso_x003A_Fecha_x0020_de_x0020_ingreso," +
    "Requerimiento_x003A_T_x00ed_tulo," +
    "Documento_x0020_ingreso_x003A_ID," +
    "Destinatarios_x0020_en_x0020_copia," +
    "Destinatarios_x0020_en_x0020_copia_x003A_Correo_x0020_electr_x00f3_nico," +
    "Documento_x0020_ingreso_x003a_UT," +
    "Documento_x0020_ingreso_x003a_txtProfesionalEnArea," +
    "Etiqueta," +
    "Comuna," +
    "Regi_x00f3_n," +
    "Materia_x0020_de_x0020_despacho," +
    "Observaciones_x0020_de_x0020_adjuntos," +
    "Observaciones_x0020_medio_x0020_de_x0020_verificaci_x00f3_n," +
    "Observaciones_x0020_del_x0020_despacho," +
    "Observaciones_x002f_Acuerdos_x002f_Comentarios," +
    "Descripci_x00f3_n_x0020_de_x0020_proyecto_x0020_o_x0020_actividad," +
    "N_x00fa_mero_x0020_gu_x00ed_a_x0020_de_x0020_despacho," +
    "Cantidad_x0020_de_x0020_adjuntos," +
    "Modified_x0020_By," +
    "Created_x0020_By," +
    "File_x0020_Type," +
    "HTML_x0020_File_x0020_Type," +
    "Title," +
    "N_x00fa_mero_x0020_de_x0020_despacho," +
    "Correo_x0020_electr_x00f3_nico_x0020_destinatario_x0020_nuevo," +
    "Direcci_x00f3_n_x0020_destinatario_x0020_nuevo," +
    "Nombre_x0020_destinatario_x0020_nuevo," +
    "Tel_x00e9_fono_x0020_destinatario_x0020_nuevo," +
    "Cargo_x0020_o_x0020_profesi_x00f3_n_x0020_destinatario_x0020_nuevo," +
    "Instituci_x00f3_n_x0020_destinatario_x0020_nuevo," +
    "Dest_correo," +
    "Today," +
    "Rol_x0020_MN," +
    "Responsable_x0020_del_x0020_requerimiento," +
    "Author," +
    "Editor," +
    "IdCarga," +
    "FileLeafRef" +
  ")" +
  "VALUES (" +
    "@Estado_x0020_del_x0020_registro," +
    "@Estado_x0020_del_x0020_despacho," +
    "@G_x00e9_nero_x0020_destinatario_x0020_nuevo," +
    "@Tipo_x0020_de_x0020_Instituci_x00f3_n_x0020_destinatario_x0020_nuevo," +
    "@ContentType," +
    "@LinkFilenameNoMenu," +
    "@LinkFilename," +
    "@LinkFilename2," +
    "@ServerUrl," +
    "@BaseName," +
    "@FileSizeDisplay," +
    "@SelectTitle," +
    "@SelectFilename," +
    "@ContentTypeId," +
    "@ID," +
    "@Fecha_x0020_de_x0020_recepci_x00f3_n," +
    "@Fecha_x0020_emisi_x00f3_n_x0020_oficio," +
    "@Fecha_x0020_de_x0020_env_x00ed_o1," +
    "@Fecha," +
    "@Created," +
    "@Modified," +
    "@GUID," +
    "@Proveedor_x0020_de_x0020_despacho," +
    "@Medio_x0020_de_x0020_despacho," +
    "@Medio_x0020_de_x0020_verificaci_x00f3_n," +
    "@Proveedor_x0020_de_x0020_despacho_x003A_T_x00ed_tulo," +
    "@Medio_x0020_de_x0020_despacho_x003A_T_x00ed_tulo," +
    "@Medio_x0020_de_x0020_verificaci_x00f3_n_x003A_T_x00ed_tulo," +
    "@Destinatario," +
    "@Destinatario_x003A_Correo_x0020_electr_x00f3_nico," +
    "@Destinatario_x003a_Nombre," +
    "@Destinatario_x003a_Instituci_x00f3_n," +
    "@Unidad_x0020_T_x00e9_cnica_x0020_Asignada," +
    "@FileRef," +
    "@FileDirRef," +
    "@Last_x0020_Modified," +
    "@Created_x0020_Date," +
    "@File_x0020_Size," +
    "@UniqueId," +
    "@Tipos_x0020_de_x0020_adjuntos," +
    "@Tipos_x0020_de_x0020_adjuntos_x003A_T_x00ed_tulo," +
    "@Soporte," +
    "@Soporte_x003A_T_x00ed_tulo," +
    "@Requerimiento," +
    "@Documento_x0020_ingreso_x003A_Fecha_x0020_de_x0020_ingreso," +
    "@Requerimiento_x003A_T_x00ed_tulo," +
    "@Documento_x0020_ingreso_x003A_ID," +
    "@Destinatarios_x0020_en_x0020_copia," +
    "@Destinatarios_x0020_en_x0020_copia_x003A_Correo_x0020_electr_x00f3_nico," +
    "@Documento_x0020_ingreso_x003a_UT," +
    "@Documento_x0020_ingreso_x003a_txtProfesionalEnArea," +
    "@Etiqueta," +
    "@Comuna," +
    "@Regi_x00f3_n," +
    "@Materia_x0020_de_x0020_despacho," +
    "@Observaciones_x0020_de_x0020_adjuntos," +
    "@Observaciones_x0020_medio_x0020_de_x0020_verificaci_x00f3_n," +
    "@Observaciones_x0020_del_x0020_despacho," +
    "@Observaciones_x002f_Acuerdos_x002f_Comentarios," +
    "@Descripci_x00f3_n_x0020_de_x0020_proyecto_x0020_o_x0020_actividad," +
    "@N_x00fa_mero_x0020_gu_x00ed_a_x0020_de_x0020_despacho," +
    "@Cantidad_x0020_de_x0020_adjuntos," +
    "@Modified_x0020_By," +
    "@Created_x0020_By," +
    "@File_x0020_Type," +
    "@HTML_x0020_File_x0020_Type," +
    "@Title," +
    "@N_x00fa_mero_x0020_de_x0020_despacho," +
    "@Correo_x0020_electr_x00f3_nico_x0020_destinatario_x0020_nuevo," +
    "@Direcci_x00f3_n_x0020_destinatario_x0020_nuevo," +
    "@Nombre_x0020_destinatario_x0020_nuevo," +
    "@Tel_x00e9_fono_x0020_destinatario_x0020_nuevo," +
    "@Cargo_x0020_o_x0020_profesi_x00f3_n_x0020_destinatario_x0020_nuevo," +
    "@Instituci_x00f3_n_x0020_destinatario_x0020_nuevo," +
    "@Dest_correo," +
    "@Today," +
    "@Rol_x0020_MN," +
    "@Responsable_x0020_del_x0020_requerimiento," +
    "@Author," +
    "@Editor," +
    "@IdCarga," +
    "@FileLeafRef" +
    ")" ;
        #endregion

        #region Despachos Iniciativa
        public static string SelectDespachosInicOrigen = "SELECT * FROM etl.vw_DespachoInicMigrar WHERE Modified >= @fechaD AND Modified <= @fechaH";

        public static string SelectDespachosInicOrigenResumen = "SELECT Id FROM dbo.DespachoIniciativa";

        public static string InsertDespachoInicCarga =
                "INSERT INTO " +
        "dbo.DespachosIniciativaCarga " +
      "( " +
      "    ID, " +
        "Estado_x0020_del_x0020_registro, " +
        "Adjunta_x0020_documentaci_x00f3_n, " +
        "Estado_x0020_del_x0020_despacho, " +
        "Enviar_x0020_notificaci_x00f3_n, " +
        "G_x00e9_nero_x0020_destinatario_x0020_nuevo, " +
        "Tipo_x0020_de_x0020_Instituci_x00f3_n_x0020_destinatario_x0020_nuevo, " +
        "ContentType, " +
        "LinkFilenameNoMenu, " +
        "LinkFilename, " +
        "LinkFilename2, " +
        "ServerUrl, " +
        "EncodedAbsUrl, " +
        "BaseName, " +
        "FileSizeDisplay, " +
        "ContentTypeId, " +
        "Fecha_x0020_de_x0020_recepci_x00f3_n, " +
        "Fecha_x0020_emisi_x00f3_n_x0020_oficio, " +
        "Fecha_x0020_de_x0020_env_x00ed_o1, " +
        "Fecha, " +
        "Created, " +
        "Modified, " +
        "FileLeafRef, " +
        "Proveedor_x0020_de_x0020_despacho, " +
        "Medio_x0020_de_x0020_despacho, " +
        "Medio_x0020_de_x0020_verificaci_x00f3_n, " +
        "Proveedor_x0020_de_x0020_despacho_x003A_T_x00ed_tulo, " +
        "Medio_x0020_de_x0020_despacho_x003A_T_x00ed_tulo, " +
        "Medio_x0020_de_x0020_verificaci_x00f3_n_x003A_T_x00ed_tulo, " +
        "Requerimiento, " +
        "Documento_x0020_ingreso_x003A_Fecha_x0020_de_x0020_ingreso, " +
        "Requerimiento_x003A_T_x00ed_tulo, " +
        "Documento_x0020_ingreso_x003A_ID, " +
        "Destinatario, " +
        "Destinatario_x003A_Correo_x0020_electr_x00f3_nico, " +
        "Unidad_x0020_T_x00e9_cnica_x0020_Asignada, " +
        "FileRef, " +
        "FileDirRef, " +
        "Last_x0020_Modified, " +
        "Created_x0020_Date, " +
        "File_x0020_Size, " +
        "Tipos_x0020_de_x0020_adjuntos, " +
        "Tipos_x0020_de_x0020_adjuntos_x003A_T_x00ed_tulo, " +
        "Soporte, " +
        "Soporte_x003A_T_x00ed_tulo, " +
        "Destinatarios_x0020_en_x0020_copia, " +
        "Destinatarios_x0020_en_x0020_copia_x003A_Correo_x0020_electr_x00f3_nico, " +
        "Etiqueta, " +
        "Etiqueta_x003A_T_x00ed_tulo, " +
        "Comuna, " +
        "Regi_x00f3_n, " +
        "Materia_x0020_de_x0020_despacho, " +
        "Observaciones_x0020_de_x0020_adjuntos, " +
        "Observaciones_x0020_medio_x0020_de_x0020_verificaci_x00f3_n, " +
        "Observaciones_x0020_del_x0020_despacho, " +
        "Observaciones_x002f_Acuerdos_x002f_Comentarios, " +
        "Descripci_x00f3_n_x0020_de_x0020_proyecto_x0020_o_x0020_actividad, " +
        "N_x00fa_mero_x0020_gu_x00ed_a_x0020_de_x0020_despacho, " +
        "Cantidad_x0020_de_x0020_adjuntos, " +
        "Modified_x0020_By, " +
        "Created_x0020_By, " +
        "File_x0020_Type, " +
        "Title, " +
        "N_x00fa_mero_x0020_de_x0020_despacho, " +
        "Correo_x0020_electr_x00f3_nico_x0020_destinatario_x0020_nuevo, " +
        "Direcci_x00f3_n_x0020_destinatario_x0020_nuevo, " +
        "Nombre_x0020_destinatario_x0020_nuevo, " +
        "Tel_x00e9_fono_x0020_destinatario_x0020_nuevo, " +
        "Cargo_x0020_o_x0020_profesi_x00f3_n_x0020_destinatario_x0020_nuevo, " +
        "Instituci_x00f3_n_x0020_destinatario_x0020_nuevo, " +
        "Dest_correo, " +
        "Documento_x0020_de_x0020_ingreso, " +
        "Rol_x0020_MN, " +
        "Antecedente_x0020_o_x0020_Acuerdo, " +
        "Responsable_x0020_del_x0020_requerimiento, " +
        "Author, " +
        "Editor, " +
        "UniqueId, " +
        "IdCarga ," +
        "Tipo_x0020_de_x0020_monumento," +
        "Monumento_x0020_Nacional_x0020_involucrado," +
        "Direcci_x00f3_n_x0020_Monumento_x0020_Nacional," +
        "N_x00fa_mero_x0020_de_x0020_caso," +
        "Referencia_x0020_de_x0020_localizaci_x00f3_n_x0020_o_x0020_localidad," +
        "Nombre_x0020_o_x0020_uso_x0020_actual," +
        "Otras_x0020_denominaciones," +
        "C_x00f3_digo_x0020_Monumento_x0020_Nacional," +
        "Provincia," +
        "Tipo_x0020_de_x0020_tr_x00e1_mite," +
        "Canal_x0020_de_x0020_llegada_x0020_del_x0020_tr_x00e1_mite" +
      ") " +
      "VALUES ( " +
        "@ID, " +
        "@Estado_x0020_del_x0020_registro, " +
        "@Adjunta_x0020_documentaci_x00f3_n, " +
        "@Estado_x0020_del_x0020_despacho, " +
        "@Enviar_x0020_notificaci_x00f3_n, " +
        "@G_x00e9_nero_x0020_destinatario_x0020_nuevo, " +
        "@Tipo_x0020_de_x0020_Instituci_x00f3_n_x0020_destinatario_x0020_nuevo, " +
        "@ContentType, " +
        "@LinkFilenameNoMenu, " +
        "@LinkFilename, " +
        "@LinkFilename2, " +
        "@ServerUrl, " +
        "@EncodedAbsUrl, " +
        "@BaseName, " +
        "@FileSizeDisplay, " +
        "@ContentTypeId, " +
        "@Fecha_x0020_de_x0020_recepci_x00f3_n, " +
        "@Fecha_x0020_emisi_x00f3_n_x0020_oficio, " +
        "@Fecha_x0020_de_x0020_env_x00ed_o1, " +
        "@Fecha, " +
        "@Created, " +
        "@Modified, " +
        "@FileLeafRef, " +
        "@Proveedor_x0020_de_x0020_despacho, " +
        "@Medio_x0020_de_x0020_despacho, " +
        "@Medio_x0020_de_x0020_verificaci_x00f3_n, " +
        "@Proveedor_x0020_de_x0020_despacho_x003A_T_x00ed_tulo, " +
        "@Medio_x0020_de_x0020_despacho_x003A_T_x00ed_tulo, " +
        "@Medio_x0020_de_x0020_verificaci_x00f3_n_x003A_T_x00ed_tulo, " +
        "@Requerimiento, " +
        "@Documento_x0020_ingreso_x003A_Fecha_x0020_de_x0020_ingreso, " +
        "@Requerimiento_x003A_T_x00ed_tulo, " +
        "@Documento_x0020_ingreso_x003A_ID, " +
        "@Destinatario, " +
        "@Destinatario_x003A_Correo_x0020_electr_x00f3_nico, " +
        "@Unidad_x0020_T_x00e9_cnica_x0020_Asignada, " +
        "@FileRef, " +
        "@FileDirRef, " +
        "@Last_x0020_Modified, " +
        "@Created_x0020_Date, " +
        "@File_x0020_Size, " +
        "@Tipos_x0020_de_x0020_adjuntos, " +
        "@Tipos_x0020_de_x0020_adjuntos_x003A_T_x00ed_tulo, " +
        "@Soporte, " +
        "@Soporte_x003A_T_x00ed_tulo, " +
        "@Destinatarios_x0020_en_x0020_copia, " +
        "@Destinatarios_x0020_en_x0020_copia_x003A_Correo_x0020_electr_x00f3_nico, " +
        "@Etiqueta, " +
        "@Etiqueta_x003A_T_x00ed_tulo, " +
        "@Comuna, " +
        "@Regi_x00f3_n, " +
        "@Materia_x0020_de_x0020_despacho, " +
        "@Observaciones_x0020_de_x0020_adjuntos, " +
        "@Observaciones_x0020_medio_x0020_de_x0020_verificaci_x00f3_n, " +
        "@Observaciones_x0020_del_x0020_despacho, " +
        "@Observaciones_x002f_Acuerdos_x002f_Comentarios, " +
        "@Descripci_x00f3_n_x0020_de_x0020_proyecto_x0020_o_x0020_actividad, " +
        "@N_x00fa_mero_x0020_gu_x00ed_a_x0020_de_x0020_despacho, " +
        "@Cantidad_x0020_de_x0020_adjuntos, " +
        "@Modified_x0020_By, " +
        "@Created_x0020_By, " +
        "@File_x0020_Type, " +
        "@Title, " +
        "@N_x00fa_mero_x0020_de_x0020_despacho, " +
        "@Correo_x0020_electr_x00f3_nico_x0020_destinatario_x0020_nuevo, " +
        "@Direcci_x00f3_n_x0020_destinatario_x0020_nuevo, " +
        "@Nombre_x0020_destinatario_x0020_nuevo, " +
        "@Tel_x00e9_fono_x0020_destinatario_x0020_nuevo, " +
        "@Cargo_x0020_o_x0020_profesi_x00f3_n_x0020_destinatario_x0020_nuevo, " +
        "@Instituci_x00f3_n_x0020_destinatario_x0020_nuevo, " +
        "@Dest_correo, " +
        "@Documento_x0020_de_x0020_ingreso, " +
        "@Rol_x0020_MN, " +
        "@Antecedente_x0020_o_x0020_Acuerdo, " +
        "@Responsable_x0020_del_x0020_requerimiento, " +
        "@Author, " +
        "@Editor, " +
        "@UniqueId, " +
        "@IdCarga, " +
        "@Tipo_x0020_de_x0020_monumento," +
        "@Monumento_x0020_Nacional_x0020_involucrado," +
        "@Direcci_x00f3_n_x0020_Monumento_x0020_Nacional," +
        "@N_x00fa_mero_x0020_de_x0020_caso," +
        "@Referencia_x0020_de_x0020_localizaci_x00f3_n_x0020_o_x0020_localidad," +
        "@Nombre_x0020_o_x0020_uso_x0020_actual," +
        "@Otras_x0020_denominaciones," +
        "@C_x00f3_digo_x0020_Monumento_x0020_Nacional," +
        "@Provincia," +
        "@Tipo_x0020_de_x0020_tr_x00e1_mite," +
        "@Canal_x0020_de_x0020_llegada_x0020_del_x0020_tr_x00e1_mite" +
      ") ";
        #endregion

        #region Unidad Tecnica
        public static string SelectUnidadTecnicaOrigen = "SELECT * FROM etl.vw_UnidadTecnicaMigrar WHERE Modified >= @fechaD AND Modified <= @fechaH";


        public static string InsertUnidadTecnicaCarga =
            "INSERT INTO " +
            "dbo.UnidadesTecnicaCarga" +
            "(" +
            "ID," +
            "Modified," +
            "Created," +
            "GUID," +
            "Last_x0020_Modified," +
            "Created_x0020_Date," +
            "Title," +
            "Correo_x0020_Responsable_x0020_U," +
            "Correo_x0020_Secretaria_x0020_UT," +
            "Nombre_x0020_Grupo," +
            "Responsable_x0020_UT," +
            "Subrogante," +
            "UniqueId," +
            "IdCarga" +
            ")" +
            "VALUES (" +
            "@ID," +
            "@Modified," +
            "@Created," +
            "@GUID," +
            "@Last_x0020_Modified," +
            "@Created_x0020_Date," +
            "@Title," +
            "@Correo_x0020_Responsable_x0020_U," +
            "@Correo_x0020_Secretaria_x0020_UT," +
            "@Nombre_x0020_Grupo," +
            "@Responsable_x0020_UT," +
            "@Subrogante," +
            "@UniqueId," +
            "@IdCarga" +
            ")";

        #endregion

        #region Regiones y Comunas
        public static string SelectRegionesOrigen = "SELECT * FROM etl.vw_RegionMigrar WHERE Modified >= @fechaD AND Modified <= @fechaH";


        public static string InsertRegionesCarga =
            "INSERT INTO " +
        "dbo.RegionesComunasCarga " +
            "( " +
            "ID, " +
            "C_x00f3_digo_x0020_comuna, " +
            "C_x00f3_digo_x0020_provincia, " +
            "C_x00f3_digo_x0020_regi_x00f3_n, " +
            "Created, " +
            "Created_x0020_Date, " +
            "Modified, " +
            "Last_x0020_Modified, " +
            "Title, " +
            "Nombre_x0020_provincia, " +
            " Nombre_x0020_regi_x00f3_n, " +
        "     Orden_x0020_regi_x00f3_n, " +
        "     UniqueId, " +
        "     IdCarga " +
        "   ) " +
        "   VALUES ( " +
        "       @ID, " +
        "       @C_x00f3_digo_x0020_comuna, " +
        "       @C_x00f3_digo_x0020_provincia, " +
        "       @C_x00f3_digo_x0020_regi_x00f3_n, " +
        "       @Created, " +
        "       @Created_x0020_Date, " +
        "      @Modified, " +
        "      @Last_x0020_Modified, " +
        "        @Title, " +
        "        @Nombre_x0020_provincia, " +
        "        @Nombre_x0020_regi_x00f3_n, " +
        "        @Orden_x0020_regi_x00f3_n, " +
        "        @UniqueId, " +
        "        @IdCarga " +
        "    ) " ;


        #endregion

        #region Casos
        public static string SelectCasosOrigen = "SELECT * FROM etl.vw_CasoMigrar WHERE Modified >= @fechaD AND Modified <= @fechaH";

        public static string InsertCasosCarga =
            "INSERT INTO " +
            "      dbo.CasosCarga " +
            "    ( " +
            "      ID, " +
            "      IdCarga, " +
            "      Modified, " +
            "      Created, " +
            "      GUID, " +
            "      Last_x0020_Modified, " +
            "      Created_x0020_Date, " +
            "      Title, " +
            "      UniqueId, " +
            "      Cantidad_x0020_Casos, " +
            "      Fecha_x0020_Referencia_x0020_del " +
            "    )  " +
            "    VALUES ( " +
            "      @ID, " +
            "      @IdCarga, " +
            "      @Modified, " +
            "      @Created, " +
            "      @GUID, " +
            "      @Last_x0020_Modified, " +
            "      @Created_x0020_Date, " +
            "      @Title, " +
            "      @UniqueId, " +
            "      @Cantidad_x0020_Casos, " +
            "      @Fecha_x0020_Referencia_x0020_del " +
            "    );";
        #endregion

        #region Notificaciones EMail
        public static string SelectNotificacionEmail = "SELECT TOP 1 Asunto, Mensaje FROM NotificacionEmail WHERE Codigo = @codigo";
        #endregion


        #region TB_Log (LogSistema)
        public static string SelectLogSistemaOrigen = "SELECT * FROM etl.vw_LogSistemaMigrar WHERE Fecha >= @fechaD AND Fecha <= @fechaH";

        public static string InsertLogSistemaCarga =
            "INSERT INTO " +
            "      dbo.TB_LogCarga " +
            "    ( " +
            "      Id_LogSistema, " +
            "      IdCarga, " +
            "      Formulario, " +
            "      Accion, " +
            "      Estado, " +
            "      Etapa, " +
            "      Fecha, " +
            "      Usuario, " +
            "      DocumentoIngreso, " +
            "      DireccionIp, " +
            "      NombrePc, " +
            "      UserAgent, " +
            "      ExtraData " +
            "    )  " +
            "    VALUES ( " +
            "      @Id_LogSistema, " +
            "      @IdCarga, " +
            "      @Formulario, " +
            "      @Accion, " +
            "      @Estado, " +
            "      @Etapa, " +
            "      @Fecha, " +
            "      @Usuario, " +
            "      @DocumentoIngreso, " +
            "      @DireccionIp, " +
            "      @NombrePc, " +
            "      @UserAgent, " +
            "      @ExtraData " +
            "    );";
        #endregion

        #region Sesion tabla
        public static string SelectSesionTablaOrigen = "SELECT * FROM etl.vw_SesionTablaMigrar WHERE Modified >= @fechaD AND Modified <= @fechaH";

        public static string SelectSesionTablaDetOrigen = "SELECT * FROM etl.vw_SesionTablaDetMigrar WHERE Modified >= @fechaD AND Modified <= @fechaH";

        public static string InsertSesionTablaCarga =
            "INSERT INTO " +
            "      dbo.TB_SesionTablaCarga " +
            "    ( " +
            "      Sesion_Id, " +
            "      Nombre, " +
            "      UnidadTecnica_Id, " +
            "      CreadoPor, " +
            "      FechaCreacion, " +
            "      IdCarga " +
            "    )  " +
            "    VALUES ( " +
            "      @Sesion_Id, " +
            "      @Nombre, " +
            "      @UnidadTecnica_Id, " +
            "      @CreadoPor, " +
            "      @FechaCreacion, " +
            "      @IdCarga " +
            "    );";

        public static string InsertSesionTablaDetCarga =
            "INSERT INTO " +
            "      dbo.TB_SesionTablaDetalleCarga " +
            "    ( " +
            "      SesionDetalle_Id, " +
            "      Sesion_Id, " +
            "      Requerimento_Id, " +
            "      DocumentoIngreso, " +
            "      IdCarga " +
            "    )  " +
            "    VALUES ( " +
            "      @SesionDetalle_Id, " +
            "      @Sesion_Id, " +
            "      @Requerimento_Id, " +
            "      @DocumentoIngreso, " +
            "      @IdCarga " +
            "    );";
        #endregion
    }
}
