using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Gedoc.Service;
using Gedoc.Service.DataAccess;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.Service.Pdf;
using Gedoc.WebApp.Helpers.Maps;
using Gedoc.WebApp.Helpers.Maps.Interface;
using SimpleInjector;
using SimpleInjector.Advanced;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;

namespace Gedoc.WebApp.Helpers
{
    public static class DependencyInjectionHelper
    {
        public static void InjectorInitialize()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebRequestLifestyle(); 
            // PAra permitir inyección en propiedades con el atributo [Inject]
            container.Options.PropertySelectionBehavior = new InjectPropertySelectionBehavior();

            #region Registro de tipos
            // TODO: utilizar Batch-Registration / Auto-Registration
            container.Register<IRequerimientoService, RequerimientoService>(Lifestyle.Scoped);
            container.Register<IUsuarioService, UsuarioService>(Lifestyle.Scoped);            
            container.Register<IRolService, RolService>(Lifestyle.Scoped);
            container.Register<IMantenedorService, MantenedorService>(Lifestyle.Scoped);
            container.Register<IDespachoService, DespachoService>(Lifestyle.Scoped);
            container.Register<IBitacoraService, BitacoraService>(Lifestyle.Scoped);
            container.Register<IAdjuntoService, AdjuntoService>(Lifestyle.Scoped);
            container.Register<IOficioService, OficioService>(Lifestyle.Scoped);
            container.Register<IPapeleraService, PapeleraService>(Lifestyle.Scoped);

            //container.Register<IListaRepositorio, ListaService>(Lifestyle.Scoped);
            //container.Register<IParametroRepositorio, RepositorioService>(Lifestyle.Scoped);

            container.Register<IMantenedorMap, MantenedorMap>(Lifestyle.Scoped);
            container.Register<IGenericMap, GenericMap>(Lifestyle.Scoped);
            #endregion
            // Inicializar DI de clases utilizadas en Gedoc.Service:
            ServiceInitializer.InjectorInitialize(container);

            //container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.Verify();

            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));

        }
    }

    class InjectPropertySelectionBehavior : IPropertySelectionBehavior
    {
        public bool SelectProperty(Type implementationType, PropertyInfo prop) =>
            prop.GetCustomAttributes(typeof(Inject)).Any();
    }

    /***
     * Summary:
     * Atributo que permite inyectar dependencia en una propiedad
     */
    class Inject : System.Attribute
    {
        // Sólo para identificar atributo para inyección de dependencia en propiedades
    }
}