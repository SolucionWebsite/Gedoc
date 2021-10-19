using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class OficioObservacionDto
    {
        public int Id { get; set; }
        public int OficioId { get; set; }
        public string Observaciones { get; set; }
        public System.DateTime Fecha { get; set; }
        public int UsuarioId { get; set; }
        public int EstadoId { get; set; }
        public int EtapaId { get; set; }
        public string UsuarioNombresApellidos { get; set; }
        public string EstadoOficioTitulo { get; set; }
        public string EtapaOficioTitulo { get; set; }
    }
}
