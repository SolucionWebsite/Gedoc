using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;

namespace Gedoc.WebApp.Helpers.Maps
{
    public class BaseMap
    {
        protected IMapper MainMapper { get; private set; }

        public BaseMap()
        {
            MainMapper = AutoMapperInitializer.MapConfig.CreateMapper();
        }
    }
}