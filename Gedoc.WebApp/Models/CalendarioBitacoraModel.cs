using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class CalendarioBitacoraModel
    {
        public int Id { get; set; }
        public string TipoBitacoraCod { get; set; }
        public string TipoBitacoraTitulo { get; set; }
        public string CreadorPor { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public DateTime? Fecha { get; set; }
        public int? Anno { get; set; }
    }
}