using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Maps.Interfaces;
using Gedoc.Repositorio.Model;
using Gedoc.Service.Maps;

namespace Gedoc.Repositorio.Maps
{


    public class ReporteMap : BaseMap, IReporteMap
    {
        public Reporte MapFromDtoToModel(ReporteDto dto)
        {
            var result = MainMapper.Map<ReporteDto, Reporte>(dto);
            return result;
        }

        public ReporteDto MapFromModelToDto(Reporte model)
        {
            var result = MainMapper.Map<Reporte, ReporteDto>(model);
            return result;
        }
    }
}
