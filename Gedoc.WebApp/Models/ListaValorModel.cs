using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class ListaValorModel
    {
        public int IdLista { get; set; }
        public string Codigo { get; set; }
        public string Titulo { get; set; }
        public string CodigoPadre { get; set; }
        public string ValorExtra1 { get; set; }
        public string ValorExtra2 { get; set; }
        public int IdEstadoRegistroValor { get; set; }
        public string EstadoRegistroNombre { get; set; }
        public int? Orden { get; set; }
        public bool EnUso { get; internal set; }
        public bool Activo { get; set; }
        public bool EsNuevo { get; set; }
    }
}