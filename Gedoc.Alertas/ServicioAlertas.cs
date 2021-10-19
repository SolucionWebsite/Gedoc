using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gedoc.Helpers.Logging;

namespace Gedoc.Alertas
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ServicioAlertas : ServiceBase
    {
        private System.Timers.Timer _timer;

        public ServicioAlertas()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ServerClass serverObject = new ServerClass();
            Thread oi = new Thread(new ThreadStart(serverObject.ProcesaAlertas));
            oi.Start();

            double seconds = double.Parse(ConfigurationManager.AppSettings["FrequencyExecution"].ToString());
            _timer = new System.Timers.Timer();
            _timer.Interval = seconds * 1000; // 1000 =  1 seconds
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            _timer.Start();
        }

        protected override void OnStop()
        {
            _timer.Stop();
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            try
            {
                _timer.Stop();
                string resultadoPropceso = string.Empty;

                ServerClass serverObject = new ServerClass();

                Thread oi = new Thread(new ThreadStart(serverObject.ProcesaAlertas));

                // Start the thread.
                oi.Start();
                
                //// Start the thread.
                //Thread ec = new Thread(new ThreadStart(serverObject.ProcesarNegocios));
                //ec.Start();

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            finally
            {
                _timer.Start();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ServerClass
    {
        public void ProcesaAlertas()
        {
            Gedoc.Alertas.Class.ServiceLogic.ProcesaAlertas();
        }
    }
}