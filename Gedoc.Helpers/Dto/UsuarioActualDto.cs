using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class UsuarioActualDto
    {
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }
        public string LoginName { get; set; }
        public string NombrePc { get; set; }
        public string DireccionIp { get; set; }
        public string UserAgent { get; set; }
    }
}
