using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;

namespace Gedoc.Repositorio.Interfaces
{
    public interface IRolRepositorio
    {
        List<Rol> GetRoles();
        bool UpdateRol(Rol rol);
        void CreateRol(Rol rol);
    }
}
