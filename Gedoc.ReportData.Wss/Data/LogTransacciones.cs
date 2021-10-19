using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.ReportData.Wss.Data
{
    [Serializable]
    public class LogTransacciones
    {
        public int Id_Log { get; set; }
        public string Formulario { get; set; }
        public string Accion { get; set; }
        public string Estado { get; set; }
        public string Etapa { get; set; }
        public DateTime? Fecha { get; set; }
        public string Usuario { get; set; }
        public string DocumentoIngreso { get; set; }
        public string DireccionIp { get; set; }
        public string TipoTramite { get; set; }
        public string CanalLlegadaTramite { get; set; }
        public string CategoriaMonNac { get; set; }
        public string CodigoMonNac { get; set; }
        public string DenominacionOf { get; set; }
        public string OtrasDenominaciones { get; set; }
        public string NombreUsoActual { get; set; }
        public string DireccionMonNac { get; set; }
        public string ReferenciaLocalidad { get; set; }
        public string Comuna { get; set; }
        public string Region { get; set; }
        public string Rol { get; set; }
        public string NumeroCaso { get; set; }
        public string NombreCaso { get; set; }
        public DateTime? FechaReferenciaCaso { get; set; }

        public LogTransacciones()
        {
        }
    }
}