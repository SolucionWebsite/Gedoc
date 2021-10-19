using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class RemitenteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Rut { get; set; }
        public string Genero { get; set; }
        public string Cargo { get; set; }
        public string Institucion { get; set; }
        public string TipoInstitucion { get; set; }
        public string Direccion { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public int? UsuarioCreacionId { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int? UsuarioModificacionId { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string UsuarioCreacionNombre { get; set; }
        public string UsuarioModificacionNombre { get; set; }
        public bool Activo { get; set; }
    }
}
