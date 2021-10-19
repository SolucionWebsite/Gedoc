using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Enum;

namespace Gedoc.Helpers
{
    public class ResultadoOperacion
    {
        public ResultadoOperacion()
        {
            Codigo = (int) CodigoResultado.Ok;
            Mensaje = "OK";
        }

        public ResultadoOperacion(int codigo, string mensaje, object extra)
        {
            Codigo = codigo;
            Mensaje = mensaje;
            Extra = extra;
        }

        //public ResultadoOperacion(EstadoOperacion estadoOperacion)
        //{
        //    Codigo = estadoOperacion.Estado ? 1 : 0;
        //    Mensaje = estadoOperacion.Mensaje;
        //    Extra = estadoOperacion;
        //}

        public int Codigo { get; set; }
        public string Mensaje { get; set; }
        public object Extra { get; set; }
    }
}
