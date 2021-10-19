using Gedoc.Etl.Winsrv.Logging;
using Gedoc.Etl.Winsrv.Planificacion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.WinSrv
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
#if DEBUG
            //SI SE EJECUTA COMO APLICACIÓN DE CONSOLA (es sólo para Debug de desarrolladores):
            if (Environment.UserInteractive)
            {
                //PlanificadorHorarios.NuevaPlanificacionEjecucion();
                var mainSrv = new MainService();
                mainSrv.RunAsConsole(args);
            }
            else
#endif
            {
                #region Log4net
                try
                {
                    Logger.Configure("ServicioGDOC_ETL");
                    Logger.Execute().Info("LOG INICIADO");
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.ToString());
                }
                #endregion

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new MainService() 
                };
                ServiceBase.Run(ServicesToRun);

            }
        }
    }
}