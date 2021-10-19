using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class BitacoraDto
    {
        public int Id { get; set; }
        public DateTime? Fecha { get; set; }
        public DateTime? FechaSolicitudDespacho { get; set; }
        public DateTime? FechaSolicitudRevision { get; set; }
        public string Titulo { get; set; }
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

        public virtual RequerimientoDto Requerimiento { get; set; }

        public string UsuarioCreacionNombresApellidos { get; set; }
        public string UsuarioActual { get; set; }
        public int? UsuarioCreacionId { get; set; }

        public DateTime? FechaUltimoAcuerdoComision { get; set; }
        public string UltimoAcuerdoComision { get; set; }
        public DateTime? FechaUltimoAcuerdoSesion { get; set; }
        public string UltimoAcuerdoSesion { get; set; }
        public DatosArchivo DatosArchivo { get; set; }
    }
}
