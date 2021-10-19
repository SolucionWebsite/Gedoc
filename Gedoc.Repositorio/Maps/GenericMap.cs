using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Maps.Interfaces;

namespace Gedoc.Service.Maps
{
    public class GenericMap: BaseMap, IGenericMap
    {
        public D MapFromOrigenToDestino<O, D>(O dato)
        {
            var result = MainMapper.Map<O, D>(dato);
            return result;
        }
        public D MapFromOrigenToDestino<O, D>(O origen, D destino)
        {
            return MainMapper.Map<O, D>(origen, destino);
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