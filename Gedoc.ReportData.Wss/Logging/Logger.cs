namespace Gedoc.ReportData.Wss.Logging
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