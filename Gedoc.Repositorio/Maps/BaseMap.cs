using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Gedoc.Service.Maps
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
