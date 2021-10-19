using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class ProcesoMasivoModel
    {
        public int UnidadTecnica { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int[] DocumentoIngreso { get; set; }
        public int Estado { get; set; }
        public int Etiqueta { get; set; }
        public int ProfesionalUtAsig { get; set; }
        public string Region { get; set; }
        public int CatMonuNac { get; set; }
        public int AccionEjecutar { get; set; }
        public int UnidadTecnicaAsignada { get; set; }
        public int NuevaUnidadTecnica { get; set; }
        public int ProfesionalUTAsignado2 { get; set; }
        public int NuevoProfesionalUT { get; set; }
        public int NuevaEtiqueta { get; set; }        

        public int[] SeleccionGrilla { get; set; }
    }
}