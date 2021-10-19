using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Model;
using Gedoc.Service.DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Service.DataAccess
{
    public class RolService : BaseService, IRolService
    {
        private readonly IRolRepositorio _repoRol;

        public RolService(IRolRepositorio repoRol)
        {
            this._repoRol = repoRol;
        }

        public List<Rol> GetRoles()
        {
            return _repoRol.GetRoles();
        }

        public bool UpdateRol(Rol rol)
        {
            return _repoRol.UpdateRol(rol);
        }

        public void CreateRol(Rol rol)
        {
            _repoRol.CreateRol(rol);
        }
    }
}
