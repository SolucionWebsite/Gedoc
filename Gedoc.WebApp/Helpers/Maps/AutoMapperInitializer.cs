using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;

namespace Gedoc.WebApp.Helpers.Maps
{
    public static class AutoMapperInitializer
    {
        public static MapperConfiguration MapConfig { get; private set; }

        public static void Initialize()
        {
            MapConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MantenedorMapProfile>();
                cfg.AddProfile<RequerimientoMapProfile>();
                cfg.AddProfile<DespachoMapProfile>();
            });
        }
    }
}