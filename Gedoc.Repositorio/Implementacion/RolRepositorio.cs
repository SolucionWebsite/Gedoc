using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Maps;
using Gedoc.Repositorio.Maps.Interfaces;
using Gedoc.Repositorio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Repositorio.Implementacion
{
    public class RolRepositorio : RepositorioBase, IRolRepositorio
    {       

        public RolRepositorio()
        {            
        }

        public void CreateRol(Rol rol)
        {
            var obj = new Rol();
            obj.Titulo = rol.Titulo;
            obj.Activo = rol.Activo;
            db.Rol.Add(obj);
            db.SaveChanges();
        }

        public List<Rol> GetRoles()
        {            
            return db.Rol.ToList();
        }

        public bool UpdateRol(Rol rol)
        {
            var obj = db.Rol.FirstOrDefault(r => r.Id == rol.Id);
            if (obj != null)
            {
                obj.Titulo = rol.Titulo;
                obj.Activo = rol.Activo;                
                db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
