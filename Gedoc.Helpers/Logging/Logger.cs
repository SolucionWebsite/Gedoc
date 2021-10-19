using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Logging
{
    public class Logger
    {
        private static ActionLogger actionLogger = null;
        public static void Configure(string LoggerName)
        {
            actionLogger = new ActionLogger();
            actionLogger.LoadConfig(LoggerName);
        }

        public static void LogError(Exception ex)
        {
            actionLogger.Error(ex);
        }

        public static void LogError(string message)
        {
            actionLogger.Error(message);
        }

        public static void LogError(string message, Exception ex)
        {
            actionLogger.Error(message, ex);
        }

        public static void LogInfo(string message)
        {
            actionLogger.Info(message);
        }

    }
}
