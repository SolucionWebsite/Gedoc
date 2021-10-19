using Gedoc.Helpers.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Alertas
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main()
        {
            Logger.Configure("GEDOC.ALERTAS");
            Logger.LogInfo("- INICIO DE LOG -");

#if DEBUG
            Gedoc.Alertas.Class.ServiceLogic.ProcesaAlertas();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ServicioAlertas()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
