using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class ItemListaModel
    {
        public int IdLista { get; set; }
        public string Codigo { get; set; }
        public string CodigoPadre { get; set; }
        public string Nombre { get; set; }
        public string Nombre2 { get; set; }
        public string Nombre3 { get; set; }
    }
}