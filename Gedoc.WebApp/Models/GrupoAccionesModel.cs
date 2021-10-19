using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class GrupoAccionesModel
    {
        public int AccionId { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
    }
}