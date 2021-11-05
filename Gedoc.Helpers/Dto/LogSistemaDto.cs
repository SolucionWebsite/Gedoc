using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class LogSistemaDto
    {
        public int Id { get; set; }
        public System.DateTime Fecha { get; set; }
        public string Flujo { get; set; }
        public string Origen { get; set; }
        public string Accion { get; set; }
        public Nullable<int> EstadoId { get; set; }
        public Nullable<int> EtapaId { get; set; }
        public Nullable<int> RequerimientoId { get; set; }
        public Nullable<int> OrigenId { get; set; }
        public string Usuario { get; set; }
        public string DireccionIp { get; set; }
        public string NombrePc { get; set; }
        public string UserAgent { get; set; }
        public string ExtraData { get; set; }
        public Nullable<int> UnidadTecnicaId { get; set; }
        public string UnidadTecnica { get; set; }
        public Nullable<int> UsuarioId { get; set; }
        public System.DateTime? OrigenFecha { get; set; }
    }
}
