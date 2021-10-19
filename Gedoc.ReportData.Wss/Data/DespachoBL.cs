using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.ReportData.Wss.Data
{

    [Serializable]
    public class DespachoBL
    {
        public DateTime? FechaIngreso { get; set; }
        public DateTime? FechaEmisionOficio { get; set; }
        public string Destinatario { get; set; }
        public string MateriaDespacho { get; set; }
        public string ProveedorDespacho { get; set; }
        public string Etiqueta { get; set; }
        public string Region { get; set; }
        public string Comuna { get; set; }
        public string UT { get; set; }
        public string DocumentoIngreso { get; set; }
        public string DestinatarioCopia { get; set; }
        public string TipoAdjunto { get; set; }
        public string Adjuntos { get; set; }
        public string ProfesionalAsignado { get; set; }
        public string Estado { get; set; }
        public string InstitucionDestinario { get; set; }
        public string Nombre { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public DateTime? FechaDespacho { get; set; }
        public string NumeroDespacho { get; set; }
        public string Folio { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string ComentarioCierre { get; set; }
        public string EstadoRequerimiento { get; set; }
        public int DiasRespuesta { get; set; }
        public string TipoInstitucionDestinario { get; set; }
        public string MedioVerificacion { get; set; }
        public string TipoTramite { get; set; }
        public string CanalLlegadaTramite { get; set; }
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

        public string FechaIngresoStr
        {
            get { return DataUtil.ToDateTime(FechaIngreso); }
            set { }
        }
        public string FechaEmisionOficioStr
        {
            get { return DataUtil.ToDateTime(FechaEmisionOficio); }
            set { }
        }
        public string FechaRecepcionStr
        {
            get { return DataUtil.ToDateTime(FechaRecepcion); }
            set { }
        }
        public string FechaDespachoStr
        {
            get { return DataUtil.ToDateTime(FechaDespacho); }
            set { }
        }
        public string FechaResolucionStr
        {
            get { return DataUtil.ToDateTime(FechaResolucion); }
            set { }
        }
        public string FechaCierreStr
        {
            get { return DataUtil.ToDateTime(FechaCierre); }
            set { }
        }

        public DespachoBL()
        {
            Destinatario = "";
            MateriaDespacho = "";
            ProveedorDespacho = "";
            Etiqueta = "";
            Region = "";
            Comuna = "";
            UT = "";
            DocumentoIngreso = "";
            DestinatarioCopia = "";
            TipoAdjunto = "";
            Adjuntos = "";
            ProfesionalAsignado = "";
            Estado = "";
            InstitucionDestinario = "";
            Nombre = "";
            NumeroDespacho = "";
            Folio = "";
            EstadoRequerimiento = "";
            DiasRespuesta = 0;
            TipoInstitucionDestinario = "";
            ComentarioCierre = "";

            TipoTramite = "";
            CanalLlegadaTramite = "";
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