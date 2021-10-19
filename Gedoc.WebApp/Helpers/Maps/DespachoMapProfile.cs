using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gedoc.Helpers.Dto;
using Gedoc.WebApp.Models;

namespace Gedoc.WebApp.Helpers.Maps
{
    public class DespachoMapProfile : AutoMapper.Profile
    {
        public DespachoMapProfile()
        {
            CreateMap<DespachoModel, DespachoDto>()
                .ReverseMap();
            CreateMap<DespachoIniciativaDto, DespachoIniciativaModel>()
                .ForMember(dest => dest.DestinatarioCopiaData, opts => opts.MapFrom(src => src.DestinatarioCopia))
                .ForMember(dest => dest.DestinatarioCopia, opts => opts.MapFrom(src => src.DestinatarioCopia))
                .ForMember(dst => dst.MonumentoNacionalCategoriaMonumentoNacCodigo,
                    opt => opt.MapFrom(src => string.Join("; ", src.MonumentoNacional.CategoriaMonumentoNac.Select(ta => ta.ExtraData))))
                .ReverseMap();
            CreateMap<OficioModel, OficioDto>()
                //.ForMember(dest => dest.RequerimientosDatos, opts => opts.MapFrom(src => src.Requerimiento))
                .ReverseMap();
        }
    }
}