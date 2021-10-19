using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Repositorio.Maps.Interfaces
{
    public interface IRolMap
    {
        RolDto MapFromModelToDto(Rol model);        
        Rol MapFromDtoToModel(RolDto dto);
    }
}
