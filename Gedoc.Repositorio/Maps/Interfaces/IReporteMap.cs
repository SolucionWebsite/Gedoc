using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Repositorio.Maps.Interfaces
{
    public interface IReporteMap
    {
        ReporteDto MapFromModelToDto(Reporte model);
        Reporte MapFromDtoToModel(ReporteDto dto);

    }
}
