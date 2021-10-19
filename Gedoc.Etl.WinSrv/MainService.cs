using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Etl.Winsrv.Logging;
using Gedoc.Etl.Winsrv.Planificacion;
using Gedoc.Etl.Winsrv.Servicios;
using Gedoc.Etl.Winsrv.Wcf;

namespace Gedoc.Etl.WinSrv
{
    public partial class MainService : ServiceBase
    {
        public ServiceHost serviceHost = null;

        // Por compatibilidad con el servicio de obtención de datos de reportes y con los reportes se conserva la misma BD de Reportes de
        // Gedoc versión Sharepoint. En esta BD los nombres de campos corresponden al nombre q tenían los campos en las listas de Sharepoint
        // por tanto este servicio tiene q extraer los datos de la BD principal de Gedoc Mvc y "mapear" los campos de las tablas con los campos de las tablas en 
        // la BD de Reportes. Incluso existen campos en la BD de Reportes, tal como estaban en Sharepoint, q ya no existen en la BD de MVC.

        public MainService()
        {
            InitializeComponent();
        }

        public void RunAsConsole(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] { "REQ", "BIT", "DES", "DIN", "UT", "REG", "CASO", "LOG", "SES" };
            }
            var _FormatFecha = "yyyy/MM/dd hh:mm";
            var start = DateTime.Now;
            var ending = DateTime.Now;
            if (args.Contains("REQ"))
            {
                Console.WriteLine(String.Format("[{0}] Procesando Requerimientos: ...", start.ToString(_FormatFecha)));
                var reqSrv = new RequerimientoSrv();
                var resultado = reqSrv.ProcesaData();
                ending = DateTime.Now;
                Console.WriteLine("[{0}] Proceso terminado. Tiempo transcurrido: {1}", ending.ToString(_FormatFecha), (ending - start).ToString(@"hh\:mm\:ss"));

            }

            if (args.Contains("BIT"))
            {
                start = DateTime.Now;
                Console.WriteLine(string.Format("[{0}] Procesando Bitácoras: ...", start.ToString(_FormatFecha)));
                var bitSrv = new BitacoraSrv();
                bitSrv.ProcesaData();
                ending = DateTime.Now;
                Console.WriteLine("[{0}] Proceso terminado. Tiempo transcurrido: {1}", ending.ToString(_FormatFecha), (ending - start).ToString(@"hh\:mm\:ss"));
            }

            if (args.Contains("DES"))
            {
                start = DateTime.Now;
                Console.WriteLine(string.Format("[{0}] Procesando Despachos: ...", start.ToString(_FormatFecha)));
                var despSrv = new DespachoSrv();
                despSrv.ProcesaData();
                ending = DateTime.Now;
                Console.WriteLine("[{0}] Proceso terminado. Tiempo transcurrido: {1}", ending.ToString(_FormatFecha), (ending - start).ToString(@"hh\:mm\:ss"));
            }

            if (args.Contains("DIN"))
            {
                start = DateTime.Now;
                Console.WriteLine(string.Format("[{0}] Procesando Despachos Iniciativa: ...", start.ToString(_FormatFecha)));
                var despSrv = new DespachoSrv();
                despSrv.ProcesaDataDespInic();
                ending = DateTime.Now;
                Console.WriteLine("[{0}] Proceso terminado. Tiempo transcurrido: {1}", ending.ToString(_FormatFecha), (ending - start).ToString(@"hh\:mm\:ss"));
            }

            if (args.Contains("UT"))
            {
                start = DateTime.Now;
                Console.WriteLine(string.Format("[{0}] Procesando UTs: ...", start.ToString(_FormatFecha)));
                var utSrv = new UnidadTecnSrv();
                utSrv.ProcesaData();
                ending = DateTime.Now;
                Console.WriteLine("[{0}] Proceso terminado. Tiempo transcurrido: {1}", ending.ToString(_FormatFecha), (ending - start).ToString(@"hh\:mm\:ss"));
            }

            if (args.Contains("REG"))
            {
                start = DateTime.Now;
                Console.WriteLine(string.Format("[{0}] Procesando Regiones y Comunas: ...", start.ToString(_FormatFecha)));
                var regSrv = new RegionesComunasSrv();
                regSrv.ProcesaData();
                ending = DateTime.Now;
                Console.WriteLine("[{0}] Proceso terminado. Tiempo transcurrido: {1}", ending.ToString(_FormatFecha), (ending - start).ToString(@"hh\:mm\:ss"));
            }

            if (args.Contains("CASO"))
            {
                start = DateTime.Now;
                Console.WriteLine(string.Format("[{0}] Procesando mantenedor de Casos: ...", start.ToString(_FormatFecha)));
                var regSrv = new CasosSrv();
                regSrv.ProcesaData();
                ending = DateTime.Now;
                Console.WriteLine("[{0}] Proceso terminado. Tiempo transcurrido: {1}", ending.ToString(_FormatFecha), (ending - start).ToString(@"hh\:mm\:ss"));
            }

            if (args.Contains("LOG"))
            {
                start = DateTime.Now;
                Console.WriteLine(string.Format("[{0}] Procesando Log Sistema: ...", start.ToString(_FormatFecha)));
                var regSrv = new LogSistemaSrv();
                regSrv.ProcesaData();
                ending = DateTime.Now;
                Console.WriteLine("[{0}] Proceso terminado. Tiempo transcurrido: {1}", ending.ToString(_FormatFecha), (ending - start).ToString(@"hh\:mm\:ss"));
            }

            if (args.Contains("SES"))
            {
                var regSrv = new SesionTablaSrv();
                start = DateTime.Now;
                Console.WriteLine(string.Format("[{0}] Procesando Tablas de Sesión: ...", start.ToString(_FormatFecha)));
                regSrv.ProcesaData();
                ending = DateTime.Now;
                Console.WriteLine("[{0}] Proceso terminado. Tiempo transcurrido: {1}", ending.ToString(_FormatFecha), (ending - start).ToString(@"hh\:mm\:ss"));
                start = DateTime.Now;
                Console.WriteLine(string.Format("[{0}] Procesando detalle de Tablas de Sesión: ...", start.ToString(_FormatFecha)));
                regSrv.ProcesaDataDet();
                ending = DateTime.Now;
                Console.WriteLine("[{0}] Proceso terminado. Tiempo transcurrido: {1}", ending.ToString(_FormatFecha), (ending - start).ToString(@"hh\:mm\:ss"));
            }

            Console.WriteLine("");
            Console.WriteLine("FIN. Presione cualquier tecla para cerrar la ventana.");
            Console.ReadLine();

            this.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            PlanificadorHorarios.NuevaPlanificacionEjecucionAsync();

            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            // Create a ServiceHost for the ServiceInteractSrv type and 
            // provide the base address.
            serviceHost = new ServiceHost(typeof(ServiceInteractSrv));

            // Open the ServiceHostBase to create listeners and start 
            // listening for messages.
            serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }
    }
}
