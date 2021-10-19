using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gedoc.Service.EmailService
{
    public class EmailDetail
    {
        public string Asunto { get; set; }
        public string TextoEmail { get; set; }

        public Dictionary<string, string> Destinatarios { get; set; }

        public string RemitenteEmail { get; set; }

        public string RemitenteNombre { get; set; }

        public List<string> Adjuntos { get; set; }

        public int? Periodicidad { get; set; }

        public bool Activo { get; set; }
    }
}