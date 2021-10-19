using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class ConfigBandejaDto
    {
        public int Id { get; set; }
        public int IdBandeja { get; set; }
        public string Titulo { get; set; }
        public int DiasAtras { get; set; }
        public int CantRegistros { get; set; }
        public bool Habilitado { get; set; }
        public List<AccionPermitidaBandejaDto> Acciones { get; set; }
    }
}
