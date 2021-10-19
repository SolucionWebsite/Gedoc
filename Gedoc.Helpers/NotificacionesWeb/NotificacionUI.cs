using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.NotificacionesWeb
{
    public class NotificacionUi
    {
        public NotificacionUi(TipoNotificacionEnum tipo, string mensaje)
        {
            Tipo = tipo;
            Mensaje = mensaje;
        }

        public NotificacionUi(TipoNotificacionEnum tipo, string mensaje, string jscript)
        {
            Tipo = tipo;
            Mensaje = mensaje;
            JScript = jscript;
        }
        public TipoNotificacionEnum Tipo { get; set; }
        public string Mensaje { get; set; }
        public string JScript { get; set; }
    }
}
