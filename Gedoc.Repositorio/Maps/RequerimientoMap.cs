using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Maps.Interfaces;
using Gedoc.Repositorio.Model;

namespace Gedoc.Service.Maps
{
    public class RequerimientoMap : BaseMap, IRequerimientoMap
    {

        public RequerimientoDto MapFromSpUltimosToDto(sp_IngresosUltimos_Result model)
        {
            var result = MainMapper.Map<sp_IngresosUltimos_Result, RequerimientoDto>(model);
            return result;
        }

        public RequerimientoDto MapFromSpBandejaToDto(sp_DatosBandejaEntrada_Result model)
        {
            var result = MainMapper.Map<sp_DatosBandejaEntrada_Result, RequerimientoDto>(model);
            return result;
        }

        public RequerimientoDto MapFromViewBandejaToDto(vw_BandejaEntrada model)
        {
            var result = MainMapper.Map<vw_BandejaEntrada, RequerimientoDto>(model);
            return result;
        }

        public Requerimiento MapFromDtoToModel(RequerimientoDto dto)
        {
            var result = MainMapper.Map<RequerimientoDto, Requerimiento>(dto);
            return result;
        }

        public RequerimientoDto MapFromModelToDto(Requerimiento model)
        {
            var result = MainMapper.Map<Requerimiento, RequerimientoDto>(model);
            return result;
        }

    }
}
