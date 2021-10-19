using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class AccionBandejaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Icono { get; set; }
        public string Hint { get; set; }
        public string TipoAccion { get; set; }
        public int Orden { get; set; }
        public string IdAccion { get; set; }
        public string Onclick { get; set; }
        public bool Activo { get; set; }
    }
}
