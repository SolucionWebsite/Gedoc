using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gedoc.Helpers.Dto;
using Gedoc.WebApp.Helpers.Maps.Interface;
using Gedoc.WebApp.Models;

namespace Gedoc.WebApp.Helpers.Maps
{
    public class GenericMap: BaseMap, IGenericMap
    {
        public D MapFromOrigenToDestino<O, D>(O dato)
        {
            var result = MainMapper.Map<O, D>(dato);
            return result;
        }

        public D MapFromDtoToModel<O, D>(O dto)
        {
            return MapFromOrigenToDestino<O, D>(dto);
        }

        public D MapFromModelToDto<O, D>(O model)
        {
            return MapFromOrigenToDestino<O, D>(model);
        }
    }
}