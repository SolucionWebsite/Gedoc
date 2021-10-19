using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gedoc.Helpers.Dto;
using Gedoc.WebApp.Helpers.Maps.Interface;
using Gedoc.WebApp.Models;

namespace Gedoc.WebApp.Helpers.Maps
{
    public class MantenedorMap: BaseMap, IMantenedorMap
    {
        public D MapFromOrigenToDestino<O, D>(O dato)
        {
            var result = MainMapper.Map<O, D>(dato);
            return result;
        }

        #region Remitente
        public RemitenteModel MapFromDtoToModel(RemitenteDto dto)
        {
            return MapFromOrigenToDestino<RemitenteDto, RemitenteModel>(dto);
        }

        public RemitenteDto MapFromModelToDto(RemitenteModel model)
        {
            return MapFromOrigenToDestino<RemitenteModel, RemitenteDto>(model);
        }
        #endregion
    }
}