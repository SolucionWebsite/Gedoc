using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class CasoDto
    {
        public int Id { get; set; }
        public int IdCaso { get; set; }
        public string Titulo { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public int? CreadoPor { get; set; }
        public int? ModificadoPor { get; set; }
        public int CantidadReq { get; set; }
        public string UsuarioCreacionNombresApellidos { get; set; }
        public string UsuarioModificacionNombresApellidos { get; set; }

    }
}
