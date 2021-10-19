using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.ReportData.Wss.Data
{
    [Serializable]
    public class ResumenGrupo
    {
        public string Desde { get; set; }
        public string Hasta { get; set; }
        public double PromedioDias { get; set; }
        public int SumaDias { get; set; }
        public int CantidadProcesados { get; set; }
    }
}