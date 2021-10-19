using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Logging
{
    public class Logger
    {
        private static ActionLogger actionLogger = null;
        public static void Configure(string LoggerName)
        {
            actionLogger = new ActionLogger();
            actionLogger.LoadConfig(LoggerName);
        }


        public static ActionLogger Execute()
        {
            if (actionLogger == null)
            {
                Configure("DefaultLogger");
            }
            return actionLogger;
        }
    }
}