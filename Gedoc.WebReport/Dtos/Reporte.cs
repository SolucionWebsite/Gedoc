using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebReport.Dtos
{
    public class Reporte
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string NombreReporte { get; set; }
        public string tituloReporteActual { get; set; }
    }
}