using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.Interop.Wss.Data
{
    public class LogWssIntegracion
    {
        public int Id { get; set; }
        public DateTime? Fecha { get; set; }
        public string IdSolicitud { get; set; }
        public string NumeroIngreso { get; set; }
        public string Operacion { get; set; }
        public string Resultado { get; set; }
        public string Observaciones { get; set; }
    }
}