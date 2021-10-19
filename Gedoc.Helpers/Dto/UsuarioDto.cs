using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public bool Activo { get; set; }        
        public string NombresApellidos { get; set; }
        public bool SolicitanteUrgencia { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string TokenSeguridad { get; set; }
        public string Rut { get; set; }
        public string FirmaDigitalPin { get; set; }
        public string FirmaDigitalPinConfirm { get; set; }
        public List<UnidadTecnicaDto> UnidadTecnicaIntegrante { get; set; }
        public List<RolDto> Rol { get; set; }
    }
}
