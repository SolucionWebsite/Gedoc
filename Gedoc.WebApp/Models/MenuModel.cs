using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class MenuModel
    {
        public int IdMenu { get; set; }
        public int? IdMenuPadre { get; set; }
        public string Nombre { get; set; }
        public string NombrePadre { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public bool Activo { get; set; }
        public string Url { get; set; }
        public string Target { get; set; }
        public int Orden { get; set; }
    }
}