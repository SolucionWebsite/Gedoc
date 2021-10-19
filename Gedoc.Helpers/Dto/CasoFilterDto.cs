using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{


    public class CasoFilterDto
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int Estado { get; set; }
        public int Etapa { get; set; }
        public int?[] Remitente { get; set; }
        public string Institucion { get; set; }
        public int TipoTramite { get; set; }
        public List<string> CategoriaMonumentoNacional { get; set; }
        public string MonumentoNacional { get; set; }
        public int?[] UnidadTecnica { get; set; }
        public int Etiqueta { get; set; }
        public string Comuna { get; set; }
        public int?[] DocumentoIngreso { get; set; }
        public string Materia { get; set; }
    }
}
