using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Gedoc.Interop.Wss.Data
{
    [DataContract]
    public class DatosDespacho
    {

        [DataMember(Order = 1)]
        public string Numero { get; set; }

        [DataMember(Order = 2)]
        public DateTime FechaEmision { get; set; }

        [DataMember(Order = 3)]
        public string FechaEmisionStr { get; set; }

        [DataMember(Order = 4)]
        public string Materia { get; set; }

        [DataMember(Order = 5)]
        public string UrlArchivo { get; set; }
    }
}