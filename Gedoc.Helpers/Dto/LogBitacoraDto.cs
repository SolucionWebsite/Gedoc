using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class LogBitacoraDto
    {
        public int Id { get; set; }
        public DateTime FechaLog { get; set; }
        public DateTime? FechaFormulario { get; set; }
        public int? EstadoId { get; set; }
        public string EstadoTitulo { get; set; }
        public int? EtapaId { get; set; }
        public string EtapaTitulo { get; set; }
        public int? UnidadTecnicaId { get; set; }
        public string UnidadTecnicaTitulo { get; set; }
        public string Actividad { get; set; }
        public int? UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }
        public string TextoDefecto { get; set; }
        public string Origen { get; set; }
        public int? OrigenId { get; set; }
        public string ExtraData { get; set; }
        public string Accion { get; set; }

    }
}
