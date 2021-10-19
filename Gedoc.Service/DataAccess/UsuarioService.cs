using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Service.DataAccess.Interfaces;

namespace Gedoc.Service.DataAccess
{
    public class UsuarioService: BaseService, IUsuarioService
    {
        private readonly IUsuarioRepositorio _repoUser;

        public UsuarioService(IUsuarioRepositorio repoUser)
        {
            this._repoUser = repoUser;
        }

        #region Usuarios
        //public UsuarioDto GetUsuarioByFuncionarioId(int funcionarioId)
        //{
        //    return _repoUser.GetUsuarioByFuncionarioId(funcionarioId);
        //}

        public List<UsuarioDto> GetUsuarios()
        {
            var datos =  _repoUser.GetUsuarios();
            // Nunca enviar el pin al frontend, se envía una cadena de * de igual longitud al pin.
            datos.ForEach(u =>
            {
                u.FirmaDigitalPin = string.IsNullOrEmpty(u.FirmaDigitalPin)
                    ? ""
                    : "*"; // new String('*', u.FirmaDigitalPin.Length);
                u.FirmaDigitalPinConfirm = u.FirmaDigitalPin;
            });
            return datos;
        }

        public ResultadoOperacion SaveUsuario(UsuarioDto usuario, bool soloUpdateActivo = false)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                // Se graba en BD
                var esNuevo = usuario.Id == 0;
                if (esNuevo)
                {
                    resultado = _repoUser.CreateUsuario(usuario);
                }
                else
                {
                    resultado = _repoUser.UpdateUsuario(usuario, soloUpdateActivo);
                }

                resultado.Mensaje = resultado.Codigo < 0
                    ? resultado.Mensaje
                    : (esNuevo ? "Se ha creado satisfactoriamente el registro." : "Se ha actualizado satisfactoriamente el registro.");
                resultado.Extra = usuario.Id;
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "grabar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public ResultadoOperacion UpdateSolicitanteUrgencia(int usuarioId, bool solicitanteUrg)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                resultado = _repoUser.UpdateSolicitanteUrgencia(usuarioId, solicitanteUrg);
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "grabar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public ResultadoOperacion DeleteUsuario(int id)
        {

            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                _repoUser.DeleteUsuario(id);
                resultado.Codigo = 1;
                resultado.Mensaje = "Registro eliminado con éxito.";
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "eliminar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public DatosAjax<List<UsuarioDto>> GetUsuariosTransparencia()
        {
            var resultado = new DatosAjax<List<UsuarioDto>>(new List<UsuarioDto>(), new ResultadoOperacion());
            try
            {
                var datos = _repoUser.GetUsuariosTransparencia();
                resultado.Data = datos;
            }
            catch (Exception ex)
            {
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los usuarios de Transparencia.");
            }
            return resultado;
        }

        public DatosAjax<List<UsuarioDto>> GetUsuariosUrgencia()
        {
            var resultado = new DatosAjax<List<UsuarioDto>>(new List<UsuarioDto>(), new ResultadoOperacion());
            try
            {
                var datos = _repoUser.GetUsuariosUrgencia();
                resultado.Data = datos;
            }
            catch (Exception ex)
            {
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los usuarios de Transparencia.");
            }
            return resultado;
        }

        public DatosAjax<List<UsuarioDto>> GetUsuariosJefaturaNoUrgencia()
        {
            var resultado = new DatosAjax<List<UsuarioDto>>(new List<UsuarioDto>(), new ResultadoOperacion());
            try
            {
                var datos = _repoUser.GetUsuariosJefaturaNoUrgencia();
                resultado.Data = datos;
            }
            catch (Exception ex)
            {
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los usuarios de Transparencia.");
            }
            return resultado;
        }

        public DatosAjax<List<UsuarioDto>> GetUsuariosByUt(int idUt)
        {
            var resultado = new DatosAjax<List<UsuarioDto>>(new List<UsuarioDto>(), new ResultadoOperacion());
            try
            {
                var datos = _repoUser.GetUsuariosByUt(idUt);
                resultado.Data = datos;
            }
            catch (Exception ex)
            {
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los usuarios de Transparencia.");
            }
            return resultado;
        }

        public DatosAjax<List<UsuarioDto>> GetUsuariosByRol(int idRol)
        {
            var resultado = new DatosAjax<List<UsuarioDto>>(new List<UsuarioDto>(), new ResultadoOperacion());
            try
            {
                var datos = _repoUser.GetUsuariosByRol(idRol);
                resultado.Data = datos;
            }
            catch (Exception ex)
            {
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los usuarios de Transparencia.");
            }
            return resultado;
        }

        public bool AccesoUrlUsuario(int usuarioId, string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url) /*url == "/"
                    || url.Equals("/Index", StringComparison.InvariantCultureIgnoreCase)
                    || url.Equals("/Home/Index", StringComparison.InvariantCultureIgnoreCase)*/)
                {
                    return true;
                }
                url = url.StartsWith("/Reporte/EjecutarReporte", StringComparison.InvariantCultureIgnoreCase)
                    ? "/Reporte"
                    : url;
                return _repoUser.AccesoUrlUsuario(usuarioId, url);
            }
            catch (Exception ex)
            {
                LogError( ex);
            }
            return false;

        }
        #endregion

        #region Rol

        public List<RolDto> GetRolAll()
        {
            try
            {
                return _repoUser.GetRolAll();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return new List<RolDto>();
        }

        public List<RolDto> GetRolesUsuario(int usuarioId, bool incluyeNoTiene)
        {
            return _repoUser.GetRolesUsuario(usuarioId, incluyeNoTiene);
        }

        public List<RolDto> GetRolesMenu(int menuId)
        {
            return _repoUser.GetRolesMenu(menuId);
        }

        public ResultadoOperacion SaveRol(RolDto rol)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                // Se graba en BD
                var esNuevo = rol.Id == 0;
                if (esNuevo)
                    resultado = _repoUser.CreateRol(rol);
                else
                    resultado = _repoUser.UpdateRol(rol);

                resultado.Mensaje = resultado.Codigo < 0
                    ? resultado.Mensaje
                    : (esNuevo ? "Se ha creado satisfactoriamente el registro." : "Se ha actualizado satisfactoriamente el registro.");
                resultado.Extra = rol.Id;
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "grabar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public ResultadoOperacion DeleteRol(int id)
        {

            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                _repoUser.DeleteRol(id);
                resultado.Codigo = 1;
                resultado.Mensaje = "Registro eliminado con éxito.";
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "eliminar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public ResultadoOperacion UpdateRolesUsuario(int usuarioId, List<RolDto> roles)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                // Se graba en BD
               _repoUser.UpdateRolesUsuario(usuarioId, roles);

               resultado.Codigo = 1;
                resultado.Mensaje = "Operación realizada con éxito.";
            }
            catch (DbUpdateException ex)
            {
                resultado = ProcesaExceptionDb(ex, errorId.ToString(), "grabar");
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
            }
            return resultado;
        }

        public List<AccionBandejaDto> GetAccionesActivasrol(int rolId)
        {
            return _repoUser.GetAccionesActivasrol(rolId);
        }

        public ResultadoOperacion UpdateAccionesRol(int rolId, List<AccionBandejaDto> acciones)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var accionesSeleccionadas = acciones.Where(a => a.Activo == true).Select(a => a.Id).ToList();
                resultado = _repoUser.UpdateAccionesRol(rolId, accionesSeleccionadas);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return resultado;
        }

        public ResultadoOperacion UpdateRolesMenu(int menuId, List<RolDto> roles)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var rolesSeleccionados = roles.Where(a => a.Activo == true).Select(a => a.Id).ToList();
                resultado = _repoUser.UpdateRolesMenu(menuId, rolesSeleccionados);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return resultado;
        }
        #endregion

    }

}
