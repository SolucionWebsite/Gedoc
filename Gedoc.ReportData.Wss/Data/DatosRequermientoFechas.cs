using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.ReportData.Wss.Data
{
    public class DatosRequerimientoFechas
    {
        public string DocumentoIngreso { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public DateTime? FechaAsignacionUT{ get; set; }
        public DateTime? FechaAsignacionUTCopia { get; set; }
        public DateTime? FechaAsignacionUTConocimiento { get; set; }
        public DateTime? FechaAsignacionUTTemporal { get; set; }
        public DateTime? FechaPriorizacion { get; set; }
        public DateTime? FechaResolucionEstimada { get; set; }
        // ?????????????????   public int Plazo_NumDias { get; set; }
        public DateTime? FechaAsignacionProfesionalTemporal { get; set; }
        public DateTime? FechaAsignacionProfesional { get; set; }
        public DateTime? FechaReasignacionProfesional { get; set; }
        public DateTime? FechaRecepcionUT { get; set; }
        public DateTime? FechaUTReasignada { get; set; }
        public DateTime? FechaSolicitudReasignacion { get; set; }
        public DateTime? LiberacionAsignacionTemporal { get; set; }
        public DateTime? FechaUltimoAcuerdoComision { get; set; }
        public DateTime? FechaUltimoAcuerdoSesion { get; set; }
        public DateTime? FechaEmisionoficio { get; set; }
        public DateTime? FechaIngresoHistorico { get; set; }
        public DateTime? FechaCierreRequerimiento { get; set; }

    }
}