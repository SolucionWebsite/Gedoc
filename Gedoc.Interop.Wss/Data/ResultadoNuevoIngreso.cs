using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Gedoc.Interop.Wss.Data
{

    [DataContract]
    public class ResultadoNuevoIngreso
    {

        [DataMember(Order = 1)]
        public string Resultado { get; set; }

        [DataMember(Order = 2)]
        public string Observaciones { get; set; }

        [DataMember(Order = 3)]
        public DatosIngresoCreado Datos { get; set; }
    }
}