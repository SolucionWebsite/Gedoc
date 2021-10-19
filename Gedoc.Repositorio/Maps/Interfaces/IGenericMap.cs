using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;

namespace Gedoc.Repositorio.Maps.Interfaces
{
    public interface IGenericMap
    {
        D MapFromDtoToModel<O,D>(O dto);
        D MapFromModelToDto<O, D>(O model);
        D MapFromOrigenToDestino<O, D>(O origen, D destino);
    }
}
