using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class AccionPermitidaBandejaDto
    {
        public int BandejaId { get; set; }
        public int EstadoId { get; set; }
        public int EtapaId { get; set; }
        public List<int> Acciones { get; set; }
        public List<AccionBandejaDto> AccionesDetalle { get; set; }
    }
}
