using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;

namespace Gedoc.Helpers.Dto
{
    public class RequerimientoDto
    {
        public int Id { get; set; }
        public bool AdjuntaDocumentacion { get; set; }
        public DateTime? AsignacionResponsable { get; set; }
        public DateTime? AsignacionResponsableAnt { get; set; }
        public int? ResponsableUtId { get; set; }
        public string ResponsableNombre { get; set; }
        public int? ResponsableUtTempId { get; set; }
        public string ResponsableUtTempNombresApellidos { get; set; }
        public DateTime? AsignacionUt { get; set; }
        public int? CantidadAdjuntos { get; set; }
        public int? CaracterId { get; set; }
        public string CaracterTitulo { get; set; }
        public int? CerradoPorId { get; set; }
        public string CerradoPor { get; set; }
        public bool CerrarReq { get; set; }
        public string ComentarioAsignacion { get; set; }
        public string ComentarioCierre { get; set; }
        public string ComentarioDevolucion { get; set; }
        public string ComentarioEncargadoUt { get; set; }
        public bool SoloMesAnno { get; set; }
        public int CreadoPorId { get; set; }
        public string CreadoPor { get; set; }
        public System.DateTime CreadoPorFecha { get; set; }
        public string DescripcionProyectoActividad { get; set; }
        public bool DevolverIngresoCentral { get; set; }
        public bool DevolverAsignacion { get; set; }
        public string DocumentoIngreso { get; set; }
        public DateTime? EnviadoUt { get; set; }
        public bool? EnviarAsignacion { get; set; }
        public bool? EnviarAsignacionTemp { get; set; }
        public bool? EnviarPriorizacion { get; set; }
        public bool? EnviarUt { get; set; }
        public int EstadoId { get; set; }
        public string EstadoTitulo { get; set; }
        public int EtapaId { get; set; }
        public string EtapaTitulo { get; set; }
        public DateTime? AsignacionAnterior { get; set; }
        public DateTime? AsignacionProfesionalTemp { get; set; }
        public DateTime? AsignacionUtTemp { get; set; }
        public DateTime? Cierre { get; set; }
        public DateTime? Devolucion { get; set; }
        public System.DateTime FechaDocumento { get; set; }
        public System.DateTime FechaIngreso { get; set; }
        public System.DateTime FechaIngresoFull { get; set; }
        public DateTime? FechaIngresoHistorico { get; set; }
        public DateTime? Liberacion { get; set; }
        public DateTime? RecepcionUt { get; set; }
        public DateTime? Resolucion { get; set; }
        public DateTime? RecepcionUtTemp { get; set; }
        public string FormaLlegadaCod { get; set; }
        public string FormaLlegadaTitulo { get; set; }
        public bool LiberarAsignacionTemp { get; set; }
        public bool LiberarAsignacionTempAnt { get; set; }
        public string Materia { get; set; }
        public int ModificadoPorId { get; set; }
        public string ModificadoPor { get; set; }
        public System.DateTime ModificadoPorFecha { get; set; }
        public string DenominacionOficial { get; set; }
        public int? MotivoCierreId { get; set; }
        public string MotivoCierreTitulo { get; set; }
        public string NombreProyectoPrograma { get; set; }
        public int NumeroIngreso { get; set; }
        public string NumeroTicket { get; set; }
        public string ObservacionesTipoDoc { get; set; }
        public string ObservacionesAdjuntos { get; set; }
        public string ObservacionesFormaLlegada { get; set; }
        public string ObservacionesTransparencia { get; set; }
        public string ObservacionesCaracter { get; set; }
        public string ObservacionesHistorico { get; set; }
        public int? Plazo { get; set; }
        public string PrioridadCod { get; set; }
        public string PrioridadTitulo { get; set; }
        public int? ProfesionalId { get; set; }
        public int? ProfesionalIdAnt { get; set; }
        public string ProfesionalNombre { get; set; }
        public string ProfesionalNombreAnt { get; set; }
        public int? ProfesionalTranspId { get; set; }
        public string ProfesionalTranspNombresApellidos { get; set; }
        public int? ProfesionalTempId { get; set; }
        public string ProfesionalTempNombresApellidos { get; set; }
        public bool? Redireccionado { get; set; }
        //public RemitenteDto Remitente { get; set; }
        public int RemitenteId { get; set; }
        public string RemitenteNombre { get; set; }
        public string RemitenteCargo { get; set; }
        public string RemitenteTelefono { get; set; }
        public string RemitenteInstitucion { get; set; }
        public string RemitenteGenero { get; set; }
        public string RemitenteDireccion { get; set; }
        public string RemitenteRut { get; set; }
        public string RemitenteTipoInstitucion { get; set; }
        public string RemitenteEmail { get; set; }
        public int? RequerimientoAnteriorId { get; set; }
        public string RequerimientoAnteriorDocIng { get; set; }
        public string RequerimientoNoRegistrado { get; set; }
        public bool? RequiereAcuerdo { get; set; }
        public bool? RequiereRespuesta { get; set; }
        public bool? RequiereTimbrajePlano { get; set; }
        public UsuarioDto ResponsableTransp { get; set; }
        public UsuarioDto ResponsableUt { get; set; }
        public UsuarioDto ResponsableUtTemp { get; set; }
        public string RolSii { get; set; }
        public int? SolicitanteUrgenciaId { get; set; }
        public string SolicitanteUrgenciaNombre { get; set; }
        public string TipoDocumentoCod { get; set; }
        public string TipoDocumentoTitulo { get; set; }
        public string TipoIngreso { get; set; }
        public DateTime? FechaUltAcuerdoComision { get; set; }
        public DateTime? FechaUltAcuerdoSesion { get; set; }
        public DateTime? FechaUltOficio { get; set; }
        public string FechasEmisionOficio { get; set; }
        public string UltimoAcuerdoComision { get; set; }
        public string UltimoAcuerdoSesion { get; set; }
        public UnidadTecnicaDto UnidadTecnicaAsign { get; set; }
        public UnidadTecnicaDto UnidadTecnicaApoyo { get; set; }
        public UnidadTecnicaDto UnidadTecnicaConoc { get; set; }
        public UnidadTecnicaDto UnidadTecnicaTemp { get; set; }
        public UnidadTecnicaDto UnidadTecnicaAnterior { get; set; }
        public UnidadTecnicaDto UnidadTecnicaTransp { get; set; }

        public int? UtAsignadaId { get; set; }
        public string UtAsignadaTitulo { get; set; }
        public int? UtApoyoId { get; set; }
        public string UtApoyoTitulo { get; set; }
        public int? UtConocimientoId { get; set; }
        public string UtConocimientoTitulo { get; set; }
        public int? UtTemporalId { get; set; }
        public string UtTemporalTitulo { get; set; }
        public int? UtAnteriorId { get; set; }
        public string UtAnteriorTitulo { get; set; }
        public int? UtTransparenciaId { get; set; }
        public string UtTransparenciaTitulo { get; set; }
        public bool EsTransparencia { get; set; }
        public bool EsTransparenciaAnt { get; set; }
        public string ProyectoActividad { get; set; }
        public int? TipoTramiteId { get; set; }
        public string TipoTramiteTitulo { get; set; }
        public MonumentoNacionalDto MonumentoNacional { get; set; }
        public string CanalLlegadaTramiteCod { get; set; }
        public string CanalLlegadaTramiteTitulo { get; set; }
        public int? CasoId { get; set; }
        public string CasoTitulo { get; set; }
        public List<GenericoDto> TipoAdjunto { get; set; }
        public string TipoAdjuntoTitulos { get; set; }
        public List<GenericoDto> Soporte { get; set; }
        public string SoporteTitulos { get; set; }
        public int CantOficiosCmn { get; set; }
        public int DiasResolucion { get; set; }
        public List<GenericoDto> Etiqueta { get; set; }
        public string EtiquetaTitulos { get; set; }
        public string UtCopiaTitulos { get; set; }
        public string CategoriaMonumentoNacTitulo { get; set; }
        public string RegionTitulos { get; set; }
        public string ProvinciaTitulos { get; set; }
        public string ComunaTitulos { get; set; }
        public string AnnoMes
        {
            get
            {
                var nombreMes = CultureInfo.CreateSpecificCulture("es").DateTimeFormat.GetMonthName(FechaIngreso.Month);
                return FechaIngreso.Year.ToString() + "-(" + FechaIngreso.ToString("MM") + " " + nombreMes + ")";
            }
            set { }
        }
        public FlujoIngreso Flujo { get; set; }
        public string UsuarioActual { get; set; }
        public int? UsuarioActualId { get; set; }
        public string DireccionIp { get; set; }
        public string NombrePc { get; set; }
        public string UserAgent { get; set; }
        public List<GenericoDto> UnidadTecnicaCopia { get; set; }
        public bool EnAsignacionTemp { get; set; }
        public bool EsReasignacion { get; set; }

        // Para la Ficha de requerimiento:
        public List<DespachoDto> Despachos { get; set; }
        public List<AdjuntoDto> Adjuntos { get; set; }
        public List<BitacoraDto> Bitacoras { get; set; }
        public List<LogBitacoraDto> LogBitacoras { get; set; }
        public int? SolicitudTramId { get; set; }

        // Nueva Bitácora (log de modificaciones en Requerimientos)
        public ControlCambiosEntidad ControlCambios { get; set; }

        // Control de datos modificados en form de Editar Requerimiento, para envío de notificación email
        public RequerimientoBackupDataDto BackupData { get; set; }


        //Forzar Prioridad
        public bool? ForzarPrioridad { get; set; }
        public DateTime? ForzarPrioridadFecha { get; set; }
        public string ForzarPrioridadMotivo { get; set; }

        // Vista por genero
        public string AnnoMesInt { get; set; }
        public int TotalAnnoMes { get; set; }
        public int TotalAnnoMesGenero { get; set; }


    }
}
