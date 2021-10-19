using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers.Logging;
using Gedoc.Service;
using Gedoc.Service.DataAccess;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;
using Gedoc.WebApp.Helpers.Maps;

namespace Gedoc.WebApp.App_Start
{
    public static class AppInitializer
    {
        public static void Initialize()
        {
            #region AutoMapper - incialización
            AutoMapperInitializer.Initialize();
            ServiceInitializer.MapInitialize();
            #endregion
            #region DI - inicialización
            DependencyInjectionHelper.InjectorInitialize();
            LogInitialize();
            #endregion
        }

        private static void LogInitialize()
        {
            Logger.Configure("GEDOC.APP.WEB");
            Logger.LogInfo("- INICIO DE LOG -");
        }

    }
}