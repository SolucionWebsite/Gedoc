using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Gedoc.Helpers.Logging;
using Gedoc.Interop.Wss.Services;
using Gedoc.Service;
using Gedoc.Service.DataAccess;
using Gedoc.Service.DataAccess.Interfaces;
using SimpleInjector;
using SimpleInjector.Integration.Wcf;
using SimpleInjector.Lifestyles;

namespace Gedoc.Interop.Wss.App_Code
{
    public class AppInitializer
    {
        public static void AppInitialize()
        {
            LogInitialize();
            ServiceInitializer.MapInitialize();
            InjectorInitialize();
        }

        private static void LogInitialize()
        {
            Logger.Configure("GEDOC.INTEROP.WSS");
            Logger.LogInfo("- INICIO DE LOG -");
        }

        private static void InjectorInitialize()
        {
            // Create the container as usual.
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            // Register WCF services.
            container.RegisterWcfServices(Assembly.GetExecutingAssembly());

            // Register your types, for instance:
            container.Register<IRequerimientoService, RequerimientoService>();
            //container.Register<IRequerimientoService, RequerimientoService>(Lifestyle.Scoped);
            container.Register<IDespachoService, DespachoService>(Lifestyle.Scoped);
            container.Register<IMantenedorService, MantenedorService>(Lifestyle.Scoped);
            //container.Register<IIngresoSrv, IngresoSrv>(Lifestyle.Scoped);

            // Inicializar DI de clases utilizadas en Gedoc.Service:
            ServiceInitializer.InjectorInitialize(container);

            // Register the container to the SimpleInjectorServiceHostFactory.
            SimpleInjectorServiceHostFactory.SetContainer(container);
        }

    }
}