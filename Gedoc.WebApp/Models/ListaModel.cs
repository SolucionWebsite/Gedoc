using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers.Enum;

namespace Gedoc.WebApp.Models
{
    public class ListaModel
    {

        public int IdLista { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int? IdEstadoRegistro { get; set; }
        public int? IdListaPadre { get; set; }
        public EstadoRegistroEnum EstadoRegistro { get; set; }
        public bool Activo { get; set; }
        public static ListaModel Vacio
        {
            get
            {
                return new ListaModel()
                {
                    ListaValor = new List<ListaValorModel>()
                };
            }
        }

        public List<ListaValorModel> ListaValor { get; set; }
    }
}