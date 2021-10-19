using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class RequerimientoBackupDataDto
    {
        public int? UtAsignadaId { get; set; }
        public int? ProfesionalId { get; set; }
        public List<GenericoDto> UnidadTecnicaCopia { get; set; }
        public int? UtConocimientoId { get; set; }
        public int? UtTemporalId { get; set; }
        public string PrioridadCod { get; set; }
        public int? UtApoyoIdAnt { get; set; }
    }
}
