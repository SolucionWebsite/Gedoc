using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class PapeleraModel
    {
        public int Id { get; set; }
        public int OrigenId { get; set; }
        public int TipoObjetoId { get; set; }
        public string Nombre { get; set; }
        public string UbicacionOriginal { get; set; }
        public string EliminadoPor { get; set; }
        public string CreadoPor { get; set; }
        public DateTime FechaEliminacion { get; set; }
        public string Tamaño { get; set; }
    }    
}