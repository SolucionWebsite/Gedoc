using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.ReportData.Wss.Data
{
    [Serializable]
    public class RequerimientoBL
    {
        public DateTime? FechaIngreso { get; set; }
        public string TipoMonumento { get; set; }
        public string ObservacionesTipoDocumento { get; set; }
        public DateTime? FechaDocumento { get; set; }
        public string NombreComuna { get; set; }
        public string NombreRegion { get; set; }
        public string Etiqueta { get; set; }
        public string MonumentoNacionalinvolucrado { get; set; }
        public string DirMonumentoNacionalinvolucrado { get; set; }
        public string UT { get; set; }
        public string UTAsignada { get; set; }
        public string DocumentoIngreso { get; set; }
        public string RemitenteNombre { get; set; }
        public string RemitenteCargoProfesión { get; set; }
        public string Adjuntos { get; set; }
        public string ProfesionalAsignado { get; set; }
        public string Estado { get; set; }
        public string Etapa { get; set; }
        public string Materia { get; set; }
        public string RemitenteInstitucion { get; set; }
        public string ultima_fecha_acuerdo_comisión { get; set; }
        public string ultimo_acuerdo_comision { get; set; }
        public string ultima_fecha_acuerdo_sesion { get; set; }
        public string ultima_fecha_oficio { get; set; }
        public string ultima_fecha_despacho { get; set; }
        public string ultima_fecha_respuesta { get; set; }
        public string FechaCierre { get; set; }
        public string MotivoCierre { get; set; }
        public string CerradoPor { get; set; }
        public string NombreProgramaProyecto { get; set; }
        public string ProyectoActividad { get; set; }
        public string ultimo_acuerdo_sesion { get; set; }
        public string TipoDocumento { get; set; }
        public string TipoAdjuntos { get; set; }
        public string ObservacionTipoAdjunto { get; set; }
        public string NombreArchivo { get; set; }
        public DateTime? FechaResolEstimada { get; set; }
        public DateTime? FechaUltimoAcuerdoComision { get; set; }
        public DateTime? FechaUltimoAcuerdoSesion { get; set; }
        public DateTime? FechaEmisionUltimoOficio { get; set; }
        public DateTime? FechaUltimaRespuesta { get; set; }
        public string RequiereRespuesta { get; set; }
        public string ResponsableUt { get; set; }
        public string UtCopia { get; set; }
        public string ComentarioCierre { get; set; }
        public string AsignadoResponsable { get; set; }
        public string RequiereTimbraje { get; set; }
        public DateTime? FechaAsignacionUt { get; set; }
        public DateTime? FechaLiberacionUtTemporal { get; set; }
        public DateTime? FechaRecepcionUt { get; set; }

        #region Resumen reporte auditoria
        public string Desde { get; set; }
        public string Hasta { get; set; }
        public double PromedioDias { get; set; }
        public int SumaDias { get; set; }
        public int CantidadProcesados { get; set; }
        #endregion
        public string PrioridaddelRequerimiento { get; set; }
        public string NumeroIngreso { get; set; }
        public string Plazo { get; set; }
        public string UtTemporal { get; set; }
        public string UtConocimiento { get; set; }
        public string NumeroDespacho { get; set; }
        public string DestinatarioDesp { get; set; }
        public string InstDestinatarioDesp { get; set; }
        public string MateriaDesp { get; set; }
        public string ComentarioAsignacion { get; set; }
        public String FechaAsignacionProfesional { get; set; }
        public String FechaAsignacionProfAnterior { get; set; }
        public String FechaAsignacionUtAnterior { get; set; }
        public String ProfesionalUtAnterior { get; set; }
        public String UnidadTecnicaAnterior { get; set; }
        public String ModalidadProceso { get; set; }
        public String CanalTramite { get; set; }
        public String extraData { get; set; }
        public String Formulario { get; set; }


        #region Para reporte procesos masivos
        public string FechaCierreAnt { get; set; }
        public string CerradoPorAnt { get; set; }
        public string MotivoCierreAnt { get; set; }
        public string ComentarioCierreAnt { get; set; }
        public string EtiquetaAnt { get; set; }
        public string TipoReporte { get; set; }
        #endregion

        public string AnnoIngreso
        {
            get { return FechaIngreso.GetValueOrDefault().Year.ToString(); }
            set
            {

            }
        }

        public string FechaIngresoStr { get { return DataUtil.ToDateTime(FechaIngreso); } set {} }
        public string FechaDocumentoStr { get { return DataUtil.ToDateTime(FechaDocumento); } set {} }
        public string FechaResolEstimadaStr { get { return DataUtil.ToDateTime(FechaResolEstimada); } set {} }
        public string FechaUltimoAcuerdoComisionStr { 
            get {
                return FechaUltimoAcuerdoComision.HasValue ? DataUtil.ToDateTime(FechaUltimoAcuerdoComision) : (ultima_fecha_acuerdo_comisión ?? ""); 
            } 
            set {} 
        }
        public string FechaUltimoAcuerdoSesionStr { 
            get { 
                return FechaUltimoAcuerdoSesion.HasValue ? DataUtil.ToDateTime(FechaUltimoAcuerdoSesion) : (ultima_fecha_acuerdo_sesion ?? ""); 
            } 
            set {} 
        }
        public string FechaEmisionUltimoOficioStr { get { return DataUtil.ToDateTime(FechaEmisionUltimoOficio); } set {} }
        public string FechaUltimaRespuestaStr { get { return DataUtil.ToDateTime(FechaUltimaRespuesta); } set {} }
        public string FechaAsignacionUtStr { get { return DataUtil.ToDateTime(FechaAsignacionUt); } set {} }
        public string FechaLiberacionUtTemporalStr  {  get { return DataUtil.ToDateTime(FechaLiberacionUtTemporal); }  set {} }
        public string FechaRecepcionUtStr { get { return DataUtil.ToDateTime(FechaIngreso); } set { } }
        public string TipoTramite { get; set; }
        public string CanalLlegadaTramite { get; set; }
        public string CanalLlegada { get; set; }
        public string CategoriaMonNac { get; set; }
        public string CodigoMonNac { get; set; }
        public string DenominacionOf { get; set; }
        public string OtrasDenominaciones { get; set; }
        public string NombreUsoActual { get; set; }
        public string DireccionMonNac { get; set; }
        public string ReferenciaLocalidad { get; set; }
        public string Provincia { get; set; }
        public string Rol { get; set; }
        public string NumeroCaso { get; set; }
        public string NombreCaso { get; set; }
        public string FechaReferenciaCaso { get; set; }

        #region Para reporte de Trámites y casos
        public string EstadoRequerimiento { get; set; }
        public string Remitente { get; set; }
        public string CargoProfesionremitente { get; set; }
        public string Institucionremitente { get; set; }
        public string NombreProyectoActividad { get; set; }
        public string CategoriaMonumento { get; set; }
        public string CodigoMonumento { get; set; }
        public string DenominacionOficial { get; set; }
        public string DireccionMonumento { get; set; }
        public string Region { get; set; }
        public string Comuna { get; set; }
        public string UnidadTecnica { get; set; }
        public string ProfesionalUtAsignado { get; set; }
        public string ProyectooActividad { get; set; }
        public DateTime? FechaUltimaComision { get; set; }
        public string UltimoAcuerdoComision { get; set; }
        public DateTime? FechaUltimaSesion { get; set; }
        public string UltimoAcuerdoSesion { get; set; }
        public string NumeroOficio { get; set; }
        public DateTime? FechaEmisionOficio { get; set; }
        public string Destinatario { get; set; }
        public string MateriaDespacho { get; set; }
        public string AdjuntaDocumentacion { get; set; }
        public string EstadoDespacho { get; set; }
        #endregion

        public RequerimientoBL()
        {
            TipoMonumento = "";
            ObservacionesTipoDocumento = "";
            NombreComuna = "";
            NombreRegion = "";
            Etiqueta = "";
            MonumentoNacionalinvolucrado = "";
            UT = "";
            UTAsignada = "";
            DocumentoIngreso = "";
            RemitenteNombre = "";
            RemitenteCargoProfesión = "";
            Adjuntos = "";
            ProfesionalAsignado = "";
            Estado = "";
            Etapa = "";
            Materia = "";
            RemitenteInstitucion = "";
            ultima_fecha_acuerdo_comisión = "";
            ultimo_acuerdo_comision = "";
            ultima_fecha_acuerdo_sesion = "";
            ultima_fecha_oficio = "";
            ultima_fecha_despacho = "";
            ultima_fecha_respuesta = "";
            FechaCierre = "";
            MotivoCierre = "";
            CerradoPor = "";
            NombreProgramaProyecto = "";
            ProyectoActividad = "";
            ultimo_acuerdo_sesion = "";
            TipoDocumento = "";
            TipoAdjuntos = "";
            ObservacionTipoAdjunto = "";
            NombreArchivo = "";
            RequiereRespuesta = "";
            ResponsableUt = "";
            UtCopia = "";
            ComentarioCierre = "";
            AsignadoResponsable = "";
            RequiereTimbraje = "";
            MateriaDesp = "";
            NumeroDespacho = "";
            FechaCierreAnt = "";
            MotivoCierreAnt = "";
            CerradoPorAnt = "";
            ComentarioCierreAnt = "";
            DestinatarioDesp = "";
            InstDestinatarioDesp = "";
            EtiquetaAnt = "";
            ProfesionalUtAnterior = "";
            FechaAsignacionProfAnterior = "";
            FechaAsignacionUtAnterior = "";
            UnidadTecnicaAnterior = "";
            FechaEmisionUltimoOficioStr = "";
            FechaAsignacionProfesional = "";
            
            Desde = "";
            Hasta = "";
            PromedioDias = 0;
            SumaDias = 0;
            CantidadProcesados = 0;

            TipoTramite = "";
            CanalLlegadaTramite = "";
            CanalLlegada = "";
            CategoriaMonNac = "";
            CodigoMonNac = "";
            DenominacionOf = "";
            OtrasDenominaciones = "";
            NombreUsoActual = "";
            DireccionMonNac = "";
            ReferenciaLocalidad = "";
            Provincia = "";
            Rol = "";
            NumeroCaso = "";
            NombreCaso = "";
            FechaReferenciaCaso = "";
        }
    }
}