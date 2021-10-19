using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class LogWssIntegracionDto
    {
        public int Id { get; set; }
        public DateTime? Fecha { get; set; }
        public int SolicitudTramId { get; set; }
        public int? RequerimientoId { get; set; }
        public string Operacion { get; set; }
        public string Resultado { get; set; }
        public string Observaciones { get; set; }
    }
}
