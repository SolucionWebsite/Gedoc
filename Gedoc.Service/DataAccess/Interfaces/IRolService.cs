using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Service.DataAccess.Interfaces
{
    public interface IRolService
    {
        List<Rol> GetRoles();
        void CreateRol(Rol entity);
        bool UpdateRol(Rol entity);

    }
}
