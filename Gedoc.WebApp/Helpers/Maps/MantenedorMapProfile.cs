using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gedoc.Helpers.Dto;
using Gedoc.WebApp.Models;

namespace Gedoc.WebApp.Helpers.Maps
{
    public class MantenedorMapProfile : AutoMapper.Profile
    {
        public MantenedorMapProfile()
        {
            CreateMap<int, GenericoDto>()
                .ForMember(dest => dest.IdInt, opts => opts.MapFrom(src => src));
                //.ReverseMap()
                //.ConstructUsing(source => source.IdInt);
            CreateMap<GenericoDto, int>().ConstructUsing(source => source.IdInt);
            CreateMap<string, GenericoDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src));
                //.ReverseMap()
                //.ConstructUsing(source => source.Id);
            CreateMap<GenericoDto, string>().ConstructUsing(source => source.Id);
            #region Configuración maps de mantenedores
            CreateMap<RemitenteModel, RemitenteDto>()
                .ReverseMap();
            CreateMap<BitacoraModel, BitacoraDto>()
                .ReverseMap();
            CreateMap<CalendarioBitacoraModel, CalendarioBitacoraDto>()
                .ReverseMap();
            CreateMap<AdjuntoModel, AdjuntoDto>()
                .ReverseMap();
            CreateMap<NotificacionEmailModel, NotificacionEmailDto>()
                .ReverseMap();
            CreateMap<UnidadTecnicaModel, UnidadTecnicaDto>()
                .ReverseMap();

            CreateMap<PlantillaOficioModel, PlantillaOficioDto>()
                .ReverseMap();

            CreateMap<TipoTramiteModel, TipoTramiteDto>()
                .ReverseMap();

            CreateMap<ListaModel, ListaDto>()
                .ReverseMap();
            CreateMap<ListaValorModel, ListaValorDto>()
                .ForMember(dst => dst.IdEstadoRegistro, opts => opts.MapFrom(src => src.IdEstadoRegistroValor))
                .ReverseMap()
                .ForMember(dst => dst.IdEstadoRegistroValor, opts => opts.MapFrom(src => src.IdEstadoRegistro));

            CreateMap<CasoDto, CasoModel>()
                .ReverseMap();
            #endregion
        }
    }
}