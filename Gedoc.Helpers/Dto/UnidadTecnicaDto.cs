using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class UnidadTecnicaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public int? ResponsableId { get; set; }
        public string ResponsableNombresApellidos { get; set; }
        public int? SubroganteId { get; set; }
        public string SubroganteNombresApellidos { get; set; }
        public string EmailResponsable { get; set; }
        public string EmailSecretaria { get; set; }
        public string OtrosDestinatariosEmail { get; set; }
        public bool Activo { get; set; }
        public int? IdUtTramites { get; set; }
        public string UsuarioActual { get; set; }
        public int? UsuarioCreacionId { get; set; }
    }
}
