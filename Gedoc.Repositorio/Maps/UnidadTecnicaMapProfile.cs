using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;

namespace Gedoc.Repositorio.Maps
{
    public class UnidadTecnicaMapProfile : AutoMapper.Profile
    {
        public UnidadTecnicaMapProfile()
        {
            CreateMap<UnidadTecnica, UnidadTecnicaDto>()
                .ReverseMap();
        }
    }
}
