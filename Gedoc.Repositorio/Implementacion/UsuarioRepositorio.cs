using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Maps;
using Gedoc.Repositorio.Maps.Interfaces;
using Gedoc.Repositorio.Model;

namespace Gedoc.Repositorio.Implementacion
{
    public class UsuarioRepositorio: RepositorioBase, IUsuarioRepositorio
    {
        private readonly IGenericMap _mapper;

        public UsuarioRepositorio(IGenericMap mapper)
        {
            this._mapper = mapper;
        }

        #region Usuario
        //public UsuarioDto GetUsuarioByFuncionarioId(int funcionarioId)
        //{
        //    // TODO: implementar correctamente
        //    var datos = db.Usuario.FirstOrDefault(u => u.FuncionarioId == funcionarioId);
        //    return _mapper.MapFromModelToDto(datos);
        //}

        public List<UsuarioDto> GetUsuarios()
        {
            var datos = db.Usuario;
            return datos.Select(_mapper.MapFromModelToDto<Usuario, UsuarioDto>).ToList();
        }

        public ResultadoOperacion CreateUsuario(UsuarioDto usuario)
        {
            //var usu = new Usuario
            //{
            //    Email = usuario.Email,
            //    Username = usuario.Username,
            //    Password = usuario.Password,
            //    Activo = usuario.Activo,
            //    NombresApellidos = usuario.NombresApellidos,
            //    TokenSeguridad = usuario.TokenSeguridad
            //};
            var usu = _mapper.MapFromDtoToModel<UsuarioDto, Usuario>(usuario);
            db.Usuario.Add(usu);
            db.SaveChanges();
            usuario.Id = usu.Id;

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);
        }

        public ResultadoOperacion UpdateUsuario(UsuarioDto usuario, bool soloUpdateActivo)
        {
            var usu = db.Usuario.FirstOrDefault(u => u.Id == usuario.Id);
            if (usu != null)
            {
                if (!soloUpdateActivo)
                {
                    //usu.Username = usuario.Username;
                    usu.Email = usuario.Email;
                    //usu.Password = usuario.Password;
                    usu.NombresApellidos = usuario.NombresApellidos;
                    usu.TokenSeguridad = usuario.TokenSeguridad;
                    usu.Rut = usuario.Rut;
                    if ((usuario.FirmaDigitalPin ?? string.Empty).Replace("*", "") != string.Empty)
                    {
                        var pinEncr = AESThenHMAC.SimpleEncryptWithPassword(usuario.FirmaDigitalPin, GeneralData.SEC_KEY);
                        usu.FirmaDigitalPin = pinEncr;
                    } else if (string.IsNullOrWhiteSpace(usuario.FirmaDigitalPin))
                    {
                        usu.FirmaDigitalPin = null;
                    }
                }
                usu.Activo = usuario.Activo;
                db.SaveChanges();
            }
            else
            {

                return new ResultadoOperacion(-1, "No se encontró el usuario a actualizar.", null);
            }

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);

        }

        public ResultadoOperacion UpdateSolicitanteUrgencia(int usuarioId, bool solicitanteUrg)
        {
            var usu = db.Usuario.FirstOrDefault(u => u.Id == usuarioId);
            if (usu != null)
            {
                usu.SolicitanteUrgencia = solicitanteUrg;
                db.SaveChanges();
            }
            else
            {

                return new ResultadoOperacion(-1, "No se encontró el usuario a actualizar.", null);
            }

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);

        }

        public void DeleteUsuario(int id)
        {
            var model = db.Usuario.FirstOrDefault(b => b.Id == id);
            if (model != null)
            {
                db.Usuario.Remove(model);
                db.SaveChanges();
            }
        }

        public List<UsuarioDto> GetUsuariosTransparencia()
        {
            // TODO: implementar correctamente
            var datos = db.vw_Usuario;
            return datos.Select(_mapper.MapFromModelToDto<vw_Usuario, UsuarioDto>).ToList();
        }

        public List<UsuarioDto> GetUsuariosUrgencia()
        {
            var datos = db.Usuario.Where(x => x.Activo && x.SolicitanteUrgencia);
            return datos.AsEnumerable().Select(_mapper.MapFromModelToDto<Usuario, UsuarioDto>).ToList();
        }

        public List<UsuarioDto> GetUsuariosJefaturaNoUrgencia()
        {
            var datos = db.Usuario.Where(u =>
                u.Rol.Any(r => r.Id == (int)Gedoc.Helpers.Enum.Rol.IntegrantesJefaturaCMN)
                && !u.SolicitanteUrgencia
            );
            return datos.AsEnumerable().Select(_mapper.MapFromModelToDto<Usuario, UsuarioDto>).ToList();
        }

        public List<UsuarioDto> GetUsuariosByUt(int idUt)
        {
            // TODO: implementar correctamente
            var datos = db.vw_Usuario;
            return datos.Select(_mapper.MapFromModelToDto<vw_Usuario, UsuarioDto>).ToList();
        }

        public List<UsuarioDto> GetUsuariosByRol(int idRol)
        {
            // TODO: implementar correctamente
            var datos = db.vw_Usuario;
            return datos.Select(_mapper.MapFromModelToDto<vw_Usuario, UsuarioDto>).ToList();
        }

        public bool AccesoUrlUsuario(int usuarioId, string url)
        {
            // Si la url q intenta acceder el usuario es alguna de las configuradas en el mantenedor de Menú de la aplicación 
            // pero ninguno de los perfiles del usuario tiene acceso entonces se retorna false negandosele el acceso.
            // Si la url no está dentro del mantenedor de Menú entonces retorna false denegando el acceso.
            // TODO: precisar y mejorar esto. Si en algún momento hay una página con acceso sin restricción a todos los usuarios
            // y q no exista en el mantenedor de Menú entonces hay q evitar en duro el chequeo para esa página (como está
            // en el UsuarioServico  para la página de inicio). Además, si desde una página se accede a otra entonces tiene q
            // acceder por la misma url definida en el mantenedorde menú. A su vez si se cambia la condición, y se otorga acceso
            // a una url q no esté en el mantenedor (asumiendo q es una página sin restricción para todos los usuarios entonces
            // ese chequeo se puede saltar si la url configurada en el mantenedor de Menú q se intenta acceder tiene parametros en la query.
            // Por ejemplo si hay una opción de menú con la url http://localhost/bandejaentrada?id=1  y a la q el usuario
            // no tiene acceso entonces podría acceder si en el navegador se especifica la url http://localhost/bandejaentrada?cualquiercosa=1&id=1
            // ya q no exisitiría en el mantenedor.

            var urlSinIndex = url.Contains("/Index?") // PAra comprobar quitando el /Index a la url (es la misma página http://localhost/Home q http://localhost/Home/Index)
                ? url.Replace("/Index?", "?")
                : (url.EndsWith("/Index")
                    ? url.Replace("/Index", "")
                    : url);
            var urlConIndex = url.Contains("?") && !url.Contains("/Index?") // Para comprobar agregando el /Index a la url (es la misma página http://localhost/Home q http://localhost/Home/Index)
                ? url.Replace("?", "/Index?")
                : (!url.EndsWith("/Index")
                    ? url += "/Index"
                    : url);

            //var menusTodos = db.Menu
            //    .Where(m => m.Url != null)
            //    .ToList();
            //// Comprobación si está la url en el mantenedor de Menús. Si no está devuelve true permitiendo el acceso.
            //if (menusTodos.Any(m =>
            //    url.StartsWith(m.Url, StringComparison.CurrentCultureIgnoreCase) // StartsWith para evitar q al poner parametros en la query de la url se salte este chequeo si fuera con Equals
            //    || urlSinIndex.StartsWith(m.Url, StringComparison.CurrentCultureIgnoreCase)
            //    || urlConIndex.StartsWith(m.Url, StringComparison.CurrentCultureIgnoreCase)))
            {
                var menusConAcceso = db.Menu
                    .Where(m => m.Url != null && m.Rol.Any(r => r.Usuario.Any(u => u.Id == usuarioId)))
                    .ToList();
                return
                    // Comprobación q la url esté dentro de las opciones de menú permitidas en los perfiles del usuario
                    menusConAcceso.Any(m => m.Url.Equals(url, StringComparison.CurrentCultureIgnoreCase)
                                            || m.Url.Equals(urlSinIndex, StringComparison.CurrentCultureIgnoreCase)
                                            || m.Url.Equals(urlConIndex, StringComparison.CurrentCultureIgnoreCase));
            }

        }
        #endregion

        #region Rol
        public List<RolDto> GetRolAll()
        {
            var datos = db.Rol.Where(r => r.Activo);
            return datos.Select(_mapper.MapFromModelToDto<Rol, RolDto>).ToList();
        }

        public List<RolDto> GetRolesUsuario(int usuarioId, bool incluyeNoTiene)
        {
            var usuario = db.Usuario
                .Include(u => u.Rol)
                .FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null)
                return new List<RolDto>();
            var rolesUser = usuario.Rol.ToList();

            if (!incluyeNoTiene)
                return rolesUser.Select(_mapper.MapFromModelToDto<Rol, RolDto>).ToList();

            var rolesUserIds = rolesUser.Select(r => r.Id).ToList();
            var rolesNoUser = db.Rol.Where(r => r.Activo && rolesUserIds.All(id => id != r.Id)).ToList();
            rolesNoUser.ForEach(r => r.Activo = false);
            rolesUser.AddRange(rolesNoUser);
            var rolesList = rolesUser.OrderBy(r => r.Titulo).Select(_mapper.MapFromModelToDto<Rol, RolDto>).ToList();

            return rolesList;

        }

        public List<RolDto> GetRolesMenu(int menuId)
        {

            var roles = db.Rol.ToList();
            var menu = db.Menu.Include(m => m.Rol).FirstOrDefault(u => u.IdMenu == menuId);
            var menuRoles = menu.Rol.ToList();
            foreach (var rol in roles)
            {
                var tieneEsteRol = menuRoles.Any(mr => mr.Id == rol.Id);
                rol.Activo = tieneEsteRol;
            }

            return roles.Select(_mapper.MapFromModelToDto<Rol, RolDto>).ToList();

        }

        public ResultadoOperacion CreateRol(RolDto rol)
        {
            var model = _mapper.MapFromDtoToModel<RolDto, Rol>(rol);
            db.Rol.Add(model);
            db.SaveChanges();
            rol.Id = model.Id;

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);
        }

        public ResultadoOperacion UpdateRol(RolDto rol)
        {
            var model = db.Rol.FirstOrDefault(c => c.Id == rol.Id);
            if (model != null)
            {
                model.Titulo = rol.Titulo;
                model.Activo = rol.Activo;
                db.SaveChanges();
            }
            else
            {

                return new ResultadoOperacion(-1, "No se encontró el rol a actualizar.", null);
            }

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);
        }

        public void DeleteRol(int id)
        {
            var model = db.Rol.FirstOrDefault(b => b.Id == id);
            if (model != null)
            {
                db.Rol.Remove(model);
                db.SaveChanges();
            }
        }

        public void UpdateRolesUsuario(int usuarioId, List<RolDto> roles)
        {
            var usuario = db.Usuario.Include(u => u.Rol).FirstOrDefault(u => u.Id == usuarioId);

            //foreach (var rol in usuario.Rol.ToList())
            //{
            //    usuario.Rol.Remove(rol);
            //}

            usuario.Rol.Clear();

            foreach (var rol in roles)
            {
                if (rol.Activo)
                {
                    var rolAgregar = db.Rol.FirstOrDefault(r => r.Id == rol.Id);
                    usuario.Rol.Add(rolAgregar);
                }
            }
            db.SaveChanges();
        }

        public List<AccionBandejaDto> GetAccionesActivasrol(int rolId)
        {
            var rol = db.Rol.Include(r => r.AccionBandeja).First(r => r.Id == rolId);
            var accionesDelRol = rol.AccionBandeja.Select(a => a);
            var accionespredeterminadas = db.AccionBandeja.Where(a => a.TipoAccion != "O").Select(a => a);
            var grupoAcciones = new List<AccionBandejaDto>();
            foreach (var ap in accionespredeterminadas)
            {
                var ga = new AccionBandejaDto();
                ga.Id = ap.Id;
                ga.Titulo = ap.Titulo;
                if (accionesDelRol.Any(a => a.Id == ap.Id))
                    ga.Activo = true;
                grupoAcciones.Add(ga);
            }

            return grupoAcciones;
        }

        public ResultadoOperacion UpdateAccionesRol(int rolId, List<int> accionesId)
        {
            var rol = db.Rol.Include(r => r.AccionBandeja).FirstOrDefault(q => q.Id == rolId);
            if (rol != null)
            {
                rol.AccionBandeja.Clear();
                foreach (var accionId in accionesId)
                {
                    var newAccion = db.AccionBandeja.FirstOrDefault(ap => ap.Id == accionId);
                    if (newAccion != null)
                        rol.AccionBandeja.Add(newAccion);
                }
                db.SaveChanges();
            } else
            {
                return new ResultadoOperacion(-1, "No se encontró el Rol a actualizar.", null);
            }

            return new ResultadoOperacion(1, "Las acciones del perfil se han actualizado exitosamente.", null);
        }

        public ResultadoOperacion UpdateRolesMenu(int menuId, List<int> roles)
        {
            var menu = db.Menu.Include(m => m.Rol).FirstOrDefault(u => u.IdMenu == menuId);
            if (menu != null)
            {
                menu.Rol.Clear();

                foreach (var rolId in roles)
                {
                    var rolAgregar = db.Rol.FirstOrDefault(r => r.Id == rolId);
                    if (rolAgregar != null)
                        menu.Rol.Add(rolAgregar);
                }
                db.SaveChanges();
            } else
            {
                return new ResultadoOperacion(-1, "No se encontró el Menú a actualizar.", null);
            }

            return new ResultadoOperacion(1, "Los roles del menú se han actualizado exitosamente.", null);
        }

        #endregion

    }
}
