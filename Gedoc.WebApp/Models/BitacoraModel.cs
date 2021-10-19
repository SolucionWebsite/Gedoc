using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class BitacoraModel
    {
        public int Id { get; set; }
        public DateTime? Fecha { get; set; }
        public string FechaFromSelecc { get; set; }
        public string Titulo { get; set; }
        public DateTime? FechaSolicitudDespacho { get; set; }
        public DateTime? FechaSolicitudRevision { get; set; }
        public string TipoContenido { get; set; }
        public string TipoBitacoraCod { get; set; }
        public string TipoBitacoraTitulo { get; set; }
        public string ObsAcuerdoComentario { get; set; }
        public int Orden { get; set; }
        public string UrlArchivo { get; set; }
        public string NombreArchivo { get; set; }
        public string UltimoComentario { get; set; }
        public int? RequerimientoId { get; set; }
        public int? DespachoInicId { get; set; }
        public string DocIngreso { get; set; }
        public string NumeroDesp { get; set; }

        public virtual RequerimientoModel Requerimiento { get; set; }
        public int? UsuarioCreacionId { get; set; }
        public string UsuarioCreacionNombresApellidos { get; set; }
    }
}