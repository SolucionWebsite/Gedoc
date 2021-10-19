using Gedoc.Repositorio.Implementacion;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Maps;
using Gedoc.Repositorio.Maps.Interfaces;
using Gedoc.Service.EmailService;
using Gedoc.Service.Maps;
using Gedoc.Service.Pdf;
using SimpleInjector;

namespace Gedoc.Service
{
    public static class ServiceInitializer
    {
        public static void MapInitialize()
        {
            AutoMapperInitializer.Initialize();
        }

        public static void InjectorInitialize(Container container)
        {
            // Registro DI de repositorios
            container.Register<IRequerimientoRepositorio, RequerimientoRepositorio>(Lifestyle.Scoped);
            container.Register<IUsuarioRepositorio, UsuarioRepositorio>(Lifestyle.Scoped);
            container.Register<IRolRepositorio, RolRepositorio>(Lifestyle.Scoped);
            container.Register<IMantenedorRepositorio, MantenedorRepositorio>(Lifestyle.Scoped);
            container.Register<IDespachoIniciativaRepositorio, DespachoIniciativaRepositorio>(Lifestyle.Scoped);
            container.Register<IDespachoRepositorio, DespachoRepositorio>(Lifestyle.Scoped);
            container.Register<IBitacoraRepositorio, BitacoraRepositorio>(Lifestyle.Scoped);
            container.Register<IAdjuntoRepositorio, AdjuntoRepositorio>(Lifestyle.Scoped);
            container.Register<IPdfService, PdfService>(Lifestyle.Scoped);

            // Registro DI de maps
            container.Register<IRequerimientoMap, RequerimientoMap>(Lifestyle.Scoped);
            container.Register<IUsuarioMap, UsuarioMap>(Lifestyle.Scoped);            
            container.Register<IMantenedorMap, MantenedorMap>(Lifestyle.Scoped);
            container.Register<IGenericMap, GenericMap>(Lifestyle.Scoped);

            container.Register<INotificacionService, NotificacionService>(Lifestyle.Scoped);
        }
    }
}
