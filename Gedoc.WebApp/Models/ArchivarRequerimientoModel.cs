using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class ArchivarRequerimientoModel
    {
        [Required]
        public int UnidadTecnicaId { get; set; }
        [Required]
        public DateTime FechaDesde { get; set; }
        [Required]
        public DateTime FechaHasta { get; set; }
        public ArchivarEnum TipoBusqueda { get; set; }
    }

    public enum ArchivarEnum
    {
        Archivar = 1,
        Restaurar = 2,
    }
}