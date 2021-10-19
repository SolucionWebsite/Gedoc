using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;
using Gedoc.WebApp.Models;

namespace Gedoc.WebApp.Helpers.Maps.Interface
{
    public interface IGenericMap
    {
        D MapFromDtoToModel<O,D>(O dto);
        D MapFromModelToDto<O, D>(O model);
    }
}
