using AutoMapper;
using Gedoc.Repositorio.Maps;

namespace Gedoc.Service.Maps
{
    public static class AutoMapperInitializer
    {
        public static MapperConfiguration MapConfig { get; private set;  }

        public static void Initialize()
        {
            MapConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<RequerimientoMapProfile>();
                cfg.AddProfile<UsuarioMapProfile>();
                cfg.AddProfile<UnidadTecnicaMapProfile>();
                cfg.AddProfile<MantenedorMapProfile>();
                cfg.AddProfile<DespachoMapProfile>();
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
            });

        }
    }
}
