using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Gedoc.Interop.Wss.Data
{
    [DataContract]
    public class DatosIngresoNuevo
    {

        [DataMember(Order = 1)]
        public string NumeroIngreso { get; set; }

        [DataMember(Order = 2)]
        public DateTime FechaIngreso { get; set; }

        [DataMember(Order = 3)]
        public string FechaIngresoStr { get; set; }

        [DataMember(Order = 4)]
        public string Estado { get; set; }

        [DataMember(Order = 5)]
        public string Etapa { get; set; }

        [DataMember(Order = 6)]
        public string ProfesionalAsignado { get; set; }

        [DataMember(Order = 7)]
        public string UnidadTecnica { get; set; }

        [DataMember(Order = 8)]
        public int UnidadTecnicaId { get; set; }

        [DataMember(Order = 50)]
        public List<DatosDespacho> Despachos { get; set; }
    }
}