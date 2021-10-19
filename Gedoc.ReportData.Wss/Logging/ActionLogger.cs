using log4net;
using System;

namespace Gedoc.ReportData.Wss.Logging
{
    public class ActionLogger
    {
        private ILog _log = null;

        public void LoadConfig(string nameLogger)
        {
            log4net.Config.XmlConfigurator.Configure();
            _log = log4net.LogManager.GetLogger(nameLogger);
        }


        public void Error(Exception ex)
        {
            _log.Error(ex.Message, ex);
#if DEBUG
            //SI SE EJECUTA COMO APLICACIÓN DE CONSOLA (es sólo para Debug de desarrolladores):
            if (Environment.UserInteractive)
            {
                    Console.WriteLine(ex.ToString());
            }
#endif

        }

        public void Error(string message)
        {
            _log.Error(new Exception(message));
        }


        public void Info(string message)
        {
            _log.Info(message);
#if DEBUG
            //SI SE EJECUTA COMO APLICACIÓN DE CONSOLA (es sólo para Debug de desarrolladores):
            if (Environment.UserInteractive)
            {
                    Console.WriteLine(message);
            }
#endif
        }
    }
}
