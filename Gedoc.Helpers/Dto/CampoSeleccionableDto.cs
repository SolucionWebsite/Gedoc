using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class CampoSeleccionableDto
    {
        public int Id { get; set; }
        public int? PadreId { get; set; }
        public string PadreTitulo { get; set; }
        public string Nombre { get; set; }
        public string Titulo { get; set; }
        public string Variable { get; set; }
        public string Origen { get; set; }
        public int Orden { get; set; }
        public bool Expanded { get; set; }
        public string ImageUrl { get; set; }
        public bool HasChildren { get; set; }
        public List<CampoSeleccionableDto> Hijos { get; set; }
    }
}
