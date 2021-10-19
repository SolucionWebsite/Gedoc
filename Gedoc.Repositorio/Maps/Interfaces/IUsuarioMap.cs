using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;

namespace Gedoc.Repositorio.Maps.Interfaces
{
    public interface IUsuarioMap
    {
        UsuarioDto MapFromModelToDto(Usuario model);
        UsuarioDto MapFromModelViewToDto(vw_Usuario model);
        Usuario MapFromDtoToModel(UsuarioDto dto);
    }
}
