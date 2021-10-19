using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class ListaDto
    {

        public int IdLista { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int IdEstadoRegistro { get; set; }
        public string EstadoRegistroNombre { get; set; }
        public int? IdListaPadre { get; set; }
        public bool Activo { get; set; }

        public List<ListaDto> ListaHijas { get; set; }
        public List<ListaValorDto> ListaValor { get; set; }
    }
}