using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;

namespace Gedoc.Repositorio.Maps.Interfaces
{
    public interface IMantenedorMap
    {
        D MapFromOrigenToDestino<O, D>(O dato);

        #region Tipo Trámite
        TipoTramite MapTipoTramiteFromDtoToModel(GenericoDto dto);
        GenericoDto MapTipoTramiteFromModelToDto(TipoTramite model);
        #endregion

        #region Remitente
        Remitente MapRemitenteFromDtoToModel(RemitenteDto dto);
        GenericoDto MapRemitenteFromModelToGenericoDto(Remitente model);
        RemitenteDto MapRemitenteFromModelToDto(Remitente model);
        #endregion

        #region Bandejas de entrada
        BandejaEntrada MapConfiBandejaFromDtoToModel(ConfigBandejaDto dto);
        ConfigBandejaDto MapConfiBandejaFromModelToDto(BandejaEntrada model);
        AccionBandejaDto MapAccionBandejaFromModelToDto(AccionBandeja model);
        #endregion
    }
}
