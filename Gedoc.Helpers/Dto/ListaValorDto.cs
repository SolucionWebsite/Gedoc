using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class ListaValorDto
    {
        public int IdLista { get; set; }
        public string Codigo { get; set; }
        public string Titulo { get; set; }
        public string CodigoPadre { get; set; }
        public string ValorExtra1 { get; set; }
        public string ValorExtra2 { get; set; }
        public int IdEstadoRegistro { get; set; }
        public string EstadoRegistroNombre { get; set; }
        public int? Orden { get; set; }
        public bool Activo { get; set; }
        public bool EsNuevo { get; set; }
    }
}
