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
    public class UsuarioMap: BaseMap, IUsuarioMap
    {
        public UsuarioDto MapFromModelToDto(Usuario model)
        {
            var result = MainMapper.Map<Usuario, UsuarioDto>(model);
            return result;
        }
        public UsuarioDto MapFromModelViewToDto(vw_Usuario model)
        {
            var result = MainMapper.Map<vw_Usuario, UsuarioDto>(model);
            return result;
        }

        public Usuario MapFromDtoToModel(UsuarioDto dto)
        {
            var result = MainMapper.Map<UsuarioDto, Usuario>(dto);
            return result;
        }
    }
}
