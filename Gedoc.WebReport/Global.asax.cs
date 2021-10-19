using System;
using System.Web.Http;
using Gedoc.WebReport.App_Start;
using Gedoc.WebReport.Logging;

namespace Gedoc.WebReport
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            LogInitialize();
        }

        private static void LogInitialize()
        {
            Logger.Configure("GEDOC.REPORT");
            Logger.LogInfo("- INICIO DE LOG -");
        }
    }
}
