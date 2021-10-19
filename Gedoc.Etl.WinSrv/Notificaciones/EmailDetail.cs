using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Notificaciones
{
    public class EmailDetail
    {
        public string Asunto { get; set; }
        public string TextoEmail { get; set; }

        public Dictionary<string, string> Destinatarios { get; set; }

        public string RemitenteEmail { get; set; }

        public string RemitenteNombre { get; set; }
    }
}