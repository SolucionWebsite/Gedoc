using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class DatosAjax<T>
    {
        public T Data { get; set; }
        public int Total { get; set; }
        public ResultadoOperacion Resultado { get; set; }

        public DatosAjax(T data, ResultadoOperacion resultado)
        {
            this.Data = data;
            this.Resultado = resultado;
            this.Total = 0;
        }
    }
}
