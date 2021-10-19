using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Gedoc.Interop.Wss.Data
{
    [DataContract]
    public class DatosIngresoCreado
    {


        [DataMember(Order = 1)]
        public string NumeroIngreso { get; set; }

        [DataMember(Order = 2)]
        public DateTime? FechaIngreso { get; set; }

        [DataMember(Order = 3)]
        public string FechaIngresoStr { get; set; }

        [DataMember(Order = 4)]
        public string IdSolicitud { get; set; }
    }
}