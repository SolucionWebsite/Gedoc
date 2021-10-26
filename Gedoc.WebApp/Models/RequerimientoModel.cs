using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;

namespace Gedoc.WebApp.Models
{
    public class RequerimientoModel
    {
        public int Id { get; set; }
        public int EstadoId { get; set; }
        public string EstadoTitulo { get; set; }
        public int EtapaId { get; set; }
        public string EtapaTitulo { get; set; }
        public int CreadoPorId { get; set; }
        public string CreadoPor { get; set; }
        public System.DateTime CreadoPorFecha { get; set; }
        public string TipoIngreso { get; set; }

        #region Documento
        public string DocumentoIngreso { get; set; }
        public int NumeroIngreso { get; set; }
        public System.DateTime FechaIngreso { get; set; }
        public System.DateTime FechaIngresoFull { get; set; }
        public DateTime? FechaIngresoHistorico { get; set; }
        public int? TipoTramiteId { get; set; }
        public string TipoTramiteTitulo { get; set; }
        public string CanalLlegadaTramiteCod { get; set; }
        public string CanalLlegadaTramiteTitulo { get; set; }
        public string TipoDocumentoCod { get; set; }
        public string TipoDocumentoTitulo { get; set; }
        [AllowHtml]
        public string ObservacionesTipoDoc { get; set; }
        public System.DateTime FechaDocumento { get; set; }
        public bool SoloMesAnno { get; set; }
        #endregion

        #region Adjuntos
        public bool AdjuntaDocumentacion { get; set; }
        public int? CantidadAdjuntos { get; set; }
        public List<string> TipoAdjunto { get; set; }
        public string TipoAdjuntoTitulos { get; set; }
        public List<string> Soporte { get; set; }
        public string SoporteTitulos { get; set; }
        [AllowHtml]
        public string ObservacionesAdjuntos { get; set; }
        #endregion

        #region Remitente
        public int? RemitenteId { get; set; }
        public string RemitenteNombre { get; set; }
        public string RemitenteCargo { get; set; }
        public string RemitenteTelefono { get; set; }
        public string RemitenteInstitucion { get; set; }
        public string RemitenteGenero { get; set; }
        public string RemitenteDireccion { get; set; }
        public string RemitenteRut { get; set; }
        public string RemitenteTipoInstitucion { get; set; }
        public string RemitenteEmail { get; set; }
        #endregion

        #region Proyecto
        public string NombreProyectoPrograma { get; set; }
        public int? CasoId { get; set; }
        public string CasoTitulo { get; set; }
        [AllowHtml]
        public string Materia { get; set; }
        public List<string> Etiqueta { get; set; }
        public string EtiquetaTitulos { get; set; }
        #endregion

        #region Monumento Nacional
        public int MonumentoNacionalId { get; set; }
        public List<string> MonumentoNacionalCategoriaMonumentoNac { get; set; }
        public string MonumentoNacionalCategoriaMonumentoNacCodigo { get; set; }
        public string MonumentoNacionalCodigoMonumentoNac { get; set; }
        public string MonumentoNacionalDenominacionOficial { get; set; }
        public string MonumentoNacionalOtrasDenominaciones { get; set; }
        public string MonumentoNacionalNombreUsoActual { get; set; }
        public string MonumentoNacionalDireccionMonumentoNac { get; set; }
        public string MonumentoNacionalReferenciaLocalidad { get; set; }
        public List<string> MonumentoNacionalRegion { get; set; }
        public List<string> MonumentoNacionalProvincia { get; set; }
        public List<string> MonumentoNacionalComuna { get; set; }
        public string MonumentoNacionalRolSii { get; set; }


        public string CategoriaMonumentoNacTitulo { get; set; }
        public string RegionTitulos { get; set; }
        public string ProvinciaTitulos { get; set; }
        public string ComunaTitulos { get; set; }
        #endregion

        #region General
        public string FormaLlegadaCod { get; set; }
        public string FormaLlegadaTitulo { get; set; }
        [AllowHtml]
        public string ObservacionesFormaLlegada { get; set; }
        [AllowHtml]
        public string ObservacionesCaracter { get; set; }
        public int? CaracterId { get; set; }
        public string CaracterTitulo { get; set; }
        public bool? Redireccionado { get; set; }
        public string NumeroTicket { get; set; }
        [AllowHtml]
        public string ObservacionesHistorico { get; set; }
        #endregion

        #region Asignación
        public bool? EnviarAsignacion { get; set; }
        public int? UtAsignadaId { get; set; }
        public string UtAsignadaTitulo { get; set; }
        public List<int> UnidadTecnicaCopia { get; set; }
        public string UtCopiaTitulos { get; set; }
        public int? UtConocimientoId { get; set; }
        public string UtConocimientoTitulo { get; set; }
        public int? UtTemporalId { get; set; }
        public string UtTemporalTitulo { get; set; }
        public bool? RequiereRespuesta { get; set; }
        [AllowHtml]
        public string ComentarioAsignacion { get; set; }

        #endregion

        #region Priorización
        public string PrioridadCod { get; set; }
        public string PrioridadTitulo { get; set; }
        public int? Plazo { get; set; }
        public int? SolicitanteUrgenciaId { get; set; }
        public string SolicitanteUrgenciaNombre { get; set; }
        public bool? EnviarAsignacionTemp { get; set; }
        public bool? EnviarPriorizacion { get; set; }
        public bool? EnviarUt { get; set; }

        //Forzar Prioridad
        public bool? ForzarPrioridad { get; set; }
        public DateTime? ForzarPrioridadFecha { get; set; }
        public string ForzarPrioridadMotivo { get; set; }
        #endregion

        #region Asignación profesional
        public DateTime? RecepcionUt { get; set; }
        public int? ProfesionalId { get; set; }
        public string ProfesionalNombre { get; set; }
        [AllowHtml]
        public string ComentarioEncargadoUt { get; set; }
        public bool? RequiereTimbrajePlano { get; set; }
        public bool? RequiereAcuerdo { get; set; }
        #endregion

        #region Reasignación UT
        public bool DevolverAsignacion { get; set; }
        [AllowHtml]
        public string ComentarioDevolucion { get; set; }
        #endregion

        #region Otros campos
        public int? RequerimientoAnteriorId { get; set; }
        public string RequerimientoAnteriorDocIng { get; set; }
        public string RequerimientoNoRegistrado { get; set; }
        [AllowHtml]
        public string ProyectoActividad { get; set; }
        public int? UtApoyoId { get; set; }
        public string UtApoyoTitulo { get; set; }
        #endregion

        #region Cierre
        [AllowHtml]
        public string ComentarioCierre { get; set; }
        public DateTime? Cierre { get; set; }
        public int? MotivoCierreId { get; set; }
        public string MotivoCierreTitulo { get; set; }
        public int? CerradoPorId { get; set; }
        public string CerradoPor { get; set; }
        public bool CerrarReq { get; set; }
        #endregion

        #region Ficha

        #endregion


        public DateTime? AsignacionResponsable { get; set; }
        public int? ResponsableUtId { get; set; }
        public string ResponsableNombre { get; set; }
        public int? ResponsableUtTempId { get; set; }
        public string ResponsableUtTempNombresApellidos { get; set; }
        public DateTime? AsignacionUt { get; set; }
        [AllowHtml]
        //public string DescripcionProyectoActcomividad { get; set; }
        //public bool DevolverIngresoCentral { get; set; }
        public DateTime? EnviadoUt { get; set; }
        public DateTime? AsignacionAnterior { get; set; }
        public DateTime? AsignacionProfesionalTemp { get; set; }
        public DateTime? AsignacionUtTemp { get; set; }
        public DateTime? Devolucion { get; set; }
        public DateTime? Liberacion { get; set; }
        public DateTime? Resolucion { get; set; }
        public DateTime? RecepcionUtTemp { get; set; }
        public bool LiberarAsignacionTemp { get; set; }
        public bool LiberarAsignacionTempAnt { get; set; }
        public int ModificadoPorId { get; set; }
        public string ModificadoPor { get; set; }
        public System.DateTime ModificadoPorFecha { get; set; }
        public string DenominacionOficial { get; set; }
        [AllowHtml]
        public string ObservacionesTransparencia { get; set; }
        public int? ProfesionalTranspId { get; set; }
        public string ProfesionalTranspNombresApellidos { get; set; }
        public int? ProfesionalTempId { get; set; }
        public string ProfesionalTempNombresApellidos { get; set; }
        //public RemitenteDto Remitente { get; set; }
        public string RolSii { get; set; }
        public DateTime? FechaUltAcuerdoComision { get; set; }
        public DateTime? FechaUltAcuerdoSesion { get; set; }
        public DateTime? FechaUltOficio { get; set; }
        public string FechasEmisionOficio { get; set; }
        public string UltimoAcuerdoComision { get; set; }
        public string UltimoAcuerdoSesion { get; set; }
        public int? UtAnteriorId { get; set; }
        public string UtAnteriorTitulo { get; set; }
        public int? UtTransparenciaId { get; set; }
        public string UtTransparenciaTitulo { get; set; }
        public bool EsTransparencia { get; set; }
        public bool EsTransparenciaAnt { get; set; }
        public int CantOficiosCmn { get; set; }
        //public int DiasResolucion { get; set; }        
        public int DiasResolucion { get {
                //IIF(req.Resolucion IS NULL, 99, DATEDIFF(DAY, GETDATE(), req.Resolucion) ) AS DiasResolucion
                return this.Resolucion == null ? 99 : (DateTime.Now - this.Resolucion.Value).Days;
            } 
        }
        public string AnnoMes { get; set; }
        public FlujoIngreso Flujo { get; set; }
        public bool EnAsignacionTemp { get; set; }

        // Control de datos modificados en form de Editar Requerimiento, para envío de notificación email
        public RequerimientoBackupDataDto BackupData { get; set; }
        public int? SolicitudTramId { get; set; }
        public string NombreProyecto { get; set; }
        public int EstadoTramitesId { get; set; }
        public string EstadoTramitesTitulo { get; set; }
    }
}