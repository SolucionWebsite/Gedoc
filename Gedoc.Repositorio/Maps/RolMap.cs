using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Maps.Interfaces;
using Gedoc.Repositorio.Model;
using Gedoc.Service.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Repositorio.Maps
{
    public class RolMap : BaseMap, IRolMap
    {
        public Rol MapFromDtoToModel(RolDto dto)
        {
            throw new NotImplementedException();
        }

        public RolDto MapFromModelToDto(Rol model)
        {
            throw new NotImplementedException();
        }
    }
}
