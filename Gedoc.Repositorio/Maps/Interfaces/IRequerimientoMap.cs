using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;

namespace Gedoc.Repositorio.Maps.Interfaces
{
    public interface IRequerimientoMap
    {
        RequerimientoDto MapFromSpUltimosToDto(sp_IngresosUltimos_Result model);
        RequerimientoDto MapFromSpBandejaToDto(sp_DatosBandejaEntrada_Result model);
        RequerimientoDto MapFromViewBandejaToDto(vw_BandejaEntrada model);
        RequerimientoDto MapFromModelToDto(Requerimiento model);
        Requerimiento MapFromDtoToModel(RequerimientoDto dto);
    }
}
