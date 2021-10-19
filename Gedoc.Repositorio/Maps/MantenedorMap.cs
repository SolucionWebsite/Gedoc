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
    public class MantenedorMap: BaseMap, IMantenedorMap
    {
        public D MapFromOrigenToDestino<O, D>(O dato)
        {
            var result = MainMapper.Map<O, D>(dato);
            return result;
        }

        #region Maps de Tipo de Trámite
        public TipoTramite MapTipoTramiteFromDtoToModel(GenericoDto dto)
        {
            return MapFromOrigenToDestino<GenericoDto, TipoTramite>(dto);
        }

        public GenericoDto MapTipoTramiteFromModelToDto(TipoTramite model)
        {
            return MapFromOrigenToDestino<TipoTramite, GenericoDto>(model);
        }
        #endregion

        #region Maps de Unidad Ténica
        //public UnidadTecnica MapUnidadTecnicaFromDtoToModel(UnidadTecnicaDto dto)
        //{
        //    return MapFromOrigenToDestino<UnidadTecnicaDto, UnidadTecnica>(dto);
        //}

        //public UnidadTecnicaDto MapUnidadTecnicaFromModelToDto(UnidadTecnica model)
        //{
        //    return MapFromOrigenToDestino<UnidadTecnica, UnidadTecnicaDto>(model);
        //}
        #endregion

        #region Maps de Remitente

        public Remitente MapRemitenteFromDtoToModel(RemitenteDto dto)
        {
            return MapFromOrigenToDestino<RemitenteDto, Remitente>(dto);
        }

        public GenericoDto MapRemitenteFromModelToGenericoDto(Remitente model)
        {
            return MapFromOrigenToDestino<Remitente, GenericoDto>(model);
        }

        public RemitenteDto MapRemitenteFromModelToDto(Remitente model)
        {
            return MapFromOrigenToDestino<Remitente, RemitenteDto>(model);
        }
        #endregion

        #region Maps Bandejas de entrada
        public BandejaEntrada MapConfiBandejaFromDtoToModel(ConfigBandejaDto dto)
        {
            var result = MainMapper.Map<ConfigBandejaDto, BandejaEntrada>(dto);
            return result;
        }

        public ConfigBandejaDto MapConfiBandejaFromModelToDto(BandejaEntrada model)
        {
            var result = MainMapper.Map<BandejaEntrada, ConfigBandejaDto>(model);
            return result;
        }

        public AccionBandejaDto MapAccionBandejaFromModelToDto(AccionBandeja model)
        {
            var result = MainMapper.Map<AccionBandeja, AccionBandejaDto>(model);
            return result;
        }
        #endregion
    }
}
