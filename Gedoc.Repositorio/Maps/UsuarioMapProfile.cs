using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;

namespace Gedoc.Repositorio.Maps
{
    public class UsuarioMapProfile : AutoMapper.Profile
    {
        public UsuarioMapProfile()
        {
            #region Configuración maps de Usuario
            CreateMap<Usuario, UsuarioDto>()
                .ReverseMap();
            CreateMap<vw_Usuario, UsuarioDto>()
                .ReverseMap();
            #endregion
            CreateMap<Rol, RolDto>()
                .ReverseMap();
        }
    }
}
