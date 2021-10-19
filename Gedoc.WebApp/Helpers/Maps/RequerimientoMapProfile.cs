using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gedoc.Helpers.Dto;
using Gedoc.WebApp.Models;

namespace Gedoc.WebApp.Helpers.Maps
{
    public class RequerimientoMapProfile : AutoMapper.Profile
    {
        public RequerimientoMapProfile()
        {
            CreateMap<RequerimientoDto, RequerimientoModel>()
                .ForMember(dst => dst.MonumentoNacionalCategoriaMonumentoNacCodigo,
                    opt => opt.MapFrom(src => string.Join("; ", src.MonumentoNacional.CategoriaMonumentoNac.Select(ta => ta.Id))))
                .ForMember(dst => dst.CategoriaMonumentoNacTitulo,
                    opt => opt.MapFrom(src => string.Join("; ", src.MonumentoNacional.CategoriaMonumentoNac.Select(ta => ta.Titulo))))
                .ForMember(dst => dst.RegionTitulos,
                    opt => opt.MapFrom(src => string.Join("; ", src.MonumentoNacional.Region.Select(ta => ta.Titulo))))
                .ForMember(dst => dst.ProvinciaTitulos,
                    opt => opt.MapFrom(src => string.Join("; ", src.MonumentoNacional.Provincia.Select(ta => ta.Titulo))))
                .ForMember(dst => dst.ComunaTitulos,
                    opt => opt.MapFrom(src => string.Join("; ", src.MonumentoNacional.Comuna.Select(ta => ta.Titulo))))
                .ReverseMap();
        }
    }
}