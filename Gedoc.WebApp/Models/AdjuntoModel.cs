using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class AdjuntoModel
    {
        public int Id { get; set; }
        public List<string> TipoAdjunto { get; set; }
        public string UrlArchivo { get; set; }
        public int? FileSize { get; set; }
        public string FilType { get; set; }
        public string FileType { get; set; }
        public int? RequerimientoId { get; set; }
        public int? OficioId { get; set; }
        public string DocIngreso { get; set; }
        public string NumeroOficio { get; set; }
        public string CreadoPor { get; set; }
        public DateTime FechaCarga { get; set; }
        public string NombreArchivo { get; set; }
        public string OrigenAdjunto { get; set; }

        public string UsuarioCreacionNombresApellidos { get; set; }
        public string UsuarioActual { get; set; }
        public int? UsuarioCreacionId { get; set; }
        public int? BandejaId { get; set; }
    }
}