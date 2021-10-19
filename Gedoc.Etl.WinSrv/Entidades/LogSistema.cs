using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Entidades
{
    public class LogSistema : BaseEntity
    {
        public int Id_LogSistema { get; set; }
        //public int IdCarga { get; set; }
        public DateTime Fecha { get; set; }
        public string Formulario { get; set; }
        public string Accion { get; set; }
        public string Estado { get; set; }
        public string Etapa { get; set; }
        public string Usuario { get; set; }
        public string DocumentoIngreso { get; set; }
        public string DireccionIp { get; set; }
        public string NombrePc { get; set; }
        public string UserAgent { get; set; }
        public string ExtraData { get; set; }
        public string TipoLog { get; set; }
    }
}
