using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebReport.Models
{
    public class LogTransaccionModel
    {

        public string id_log { get; set; }
        public string Formulario { get; set; }

        public string Accion { get; set; }

        public string Estado { get; set; }

        public string   Etapa { get; set; }

        public string Fecha { get; set; }

        public string Usuario { get; set; }

        public string DocumentoIngreso { get; set; }
        public string DireccionIp { get; set; }

        public string TipoTramite { get; set; }

        public string CanalLlegadaTramite { get; set; }

        public string CategoriaMonNac { get; set; }

        public string CodigoMonNac { get; set; }

        public string Denomincionofi { get; set; }

        public string OtrasDenonimaciones { get; set; }
        public string NombreUsoActual { get; set; }
        public string DireccionMonNac { get; set; }
        public string ReferenciaLocalidad { get; set; }

        public string Comuna { get; set; }

        public string Region { get; set; }
        public string Provincia { get; set; }
        public string Rol { get; set; }
        public string NumeroCaso { get; set; }
        public string NombreCaso { get; set; }
        public string FechaReferenciaCaso { get; set; }












    }
}