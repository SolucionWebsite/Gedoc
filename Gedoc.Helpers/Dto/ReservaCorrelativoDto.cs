using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class ReservaCorrelativoDto
    {
        public int Id { get; set; }
        public int Correlativo { get; set; }

        public string CorrelativoStr
        {
            get { return Correlativo.ToString().PadLeft(5, '0'); }
        }

        public int UsuarioCreacionId { get; set; }
        public string UsuarioCreacionNombresApellidos { get; set; }
        public string UsuarioCreacionUsername { get; set; }
        public System.DateTime FechaCreacion { get; set; }
        public string Observaciones { get; set; }
    }
}
