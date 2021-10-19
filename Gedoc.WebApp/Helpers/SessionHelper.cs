using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Logging;
using Gedoc.Service.DataAccess;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.Service.Sso;
using Gedoc.Service.WsSso;
using Gedoc.WebApp.Controllers;
using Telerik.Windows.Documents.Fixed.Search;

namespace Gedoc.WebApp.Helpers
{
    public class SessionHelper
    {
        private readonly int _idSistema = WebConfigValues.IdSistemaGedoc;
        private readonly IUsuarioService _usuarioSrv;

        public SessionHelper(IUsuarioService usuarioSrv)
        {
            this._usuarioSrv = usuarioSrv;
        }

        public bool UsuarioValido(ControllerContext context, UsuarioDto datosUsuario)
        {
            return true; // TODO: implementar validación de usuario
//            try
//            {

//                if (_idSistema <= 0)
//                {
//                    return false;
//                }

//                #region Validación de usuario autentificado en SSO

//                #region Cookies de autentificación enviadas por el portal de aplicaciones de Dibam a la aplicación
//                var request = context.RequestContext.HttpContext.Request;
//#if DEBUG
//                // En desarrollo, sin conexión a SSO de Dibam, se crean las cookies de autentificación q en ambiente
//                // de producción envía el portal de aplicaciones a la aplicación Gedoc
//                var tokenCookie = request.Cookies[WebConfigValues.Auth_Token] != null ? request.Cookies[WebConfigValues.Auth_Token].Value : System.Configuration.ConfigurationManager.AppSettings["Desa.TokenCookieValue"];
//                var tokenValidateCookie = request.Cookies[WebConfigValues.Auth_Token_Validate] != null ? request.Cookies[WebConfigValues.Auth_Token_Validate].Value : System.Configuration.ConfigurationManager.AppSettings["Desa.TokenValidateCookieValue"];
//#else
//                var tokenCookie = request.Cookies[WebConfigValues.Auth_Token] == null ? "" : request.Cookies[WebConfigValues.Auth_Token].Value;
//                var tokenValidateCookie = request.Cookies[WebConfigValues.Auth_Token_Validate] == null ? "" : request.Cookies[WebConfigValues.Auth_Token_Validate].Value;
//#endif

//                if (string.IsNullOrWhiteSpace(tokenCookie) || string.IsNullOrWhiteSpace(tokenValidateCookie) || _idSistema <= 0)
//                {
//                    return false;
//                }
//                #endregion

//                var wssoSrvc = new SsoService();
//#if DEBUG
//                var sso = new Identidad()
//                {
//                    Logeado = true,
//                    AtributoAD = System.Configuration.ConfigurationManager.AppSettings["Desa.IdUsuarioSinSSO"]
//                };
//#else
//                // Validación en SSO del usuario logeado. Devuelve Logeado en true si inició sesión y en AtributoAD el id de funcionario de Dibam
//                var sso = wssoSrvc.ValidateSso(tokenCookie, _idSistema);
//#endif

//                if (!sso.Logeado || string.IsNullOrWhiteSpace(sso.AtributoAD))
//                {
//                    return false;
//                }

//                #endregion

//                var isValidUser = CrearVariablesSesionUsuario(context, sso.AtributoAD, datosUsuario);

//                return isValidUser;
//            }
//            catch (Exception ex)
//            {
//                Logger.LogError(ex);
//                return false;
//            }
        }

        private bool CrearVariablesSesionUsuario(ControllerContext context, string atributoAD, UsuarioDto datosUsuario)
        {
            // Obtener de BD datos del usuario logeado
            var idFuncionario = 0;
            if (!int.TryParse(atributoAD, out idFuncionario))
            {
                Logger.LogError("Error en AtributoAD " + atributoAD);
                return false;
            }

            // Sólo si cambia el id de funcionario logeado es q se trae de la BD sus datos, para evitar constantes accesos a BD
            //var sessionIdFunc = (HttpContext.Current.Session["FuncionarioId"] ?? 0).ToString();
            var sessionIdUser = (HttpContext.Current.Session["UsuarioId"] ?? 0).ToString();
            var sessionIdUserInt = 0;
            int.TryParse(sessionIdUser, out sessionIdUserInt);
            //if (sessionIdFunc != idFuncionario.ToString() || sessionIdUserInt <= 0)
            //{ // Es diferente el funcionario actual con el guardado en la sesión
            //    var user = _usuarioSrv.GetUsuarioByFuncionarioId(idFuncionario);
            //    if (user == null || !user.Activo || user.Id <= 0)
            //    {
            //        return false;
            //    }
            //    // Se guardan variables de sesión del usuario logeado
            //    datosUsuario.Id = user.Id;
            //    //datosUsuario.FuncionarioId = user.FuncionarioId;
            //    HttpContext.Current.Session["UsuarioId"] = user.Id;
            //    sessionIdUserInt = user.Id;
            //    //HttpContext.Current.Session["FuncionarioId"] = user.FuncionarioId;
            //    HttpContext.Current.Session["Nombre_Funcionario"] = user.NombresApellidos;
            //    // Login name utilizado por el usuario al logearse en el portal de aplicaciones de Dibam
            //    context.RequestContext.HttpContext.Session["LoginNameAD"] = GetUserLoginData(context);
            //}
            if (context.Controller is BaseController)
            {
                ((BaseController) context.Controller).IdUsuarioActivo = sessionIdUserInt;
            }

            return true;
        }

        private string GetUserLoginData(ControllerContext context)
        {
            var loginName = "";

            try
            {
                var request = context.RequestContext.HttpContext.Request;
                var token = request.Cookies[WebConfigValues.Auth_Token] == null
                    ? ""
                    : request.Cookies[WebConfigValues.Auth_Token].Value;

                var tokenSSO = TraducirToken(token, _idSistema);
                loginName = tokenSSO.usuarioID;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return loginName;
        }


        private TokenUsuarioDto TraducirToken(string tokenString, int sistemaId)
        {
            try
            {
                var parameters = tokenString.Split('#');

                var datosToken = new TokenUsuarioDto
                {
                    IP = parameters.Length >= 1 ? parameters[0] : "",
                    SistemaID = sistemaId,
                    usuarioID = parameters.Length >= 2 ? parameters[1] : "",
                    VersionEncripID = parameters.Length >= 3 ? int.Parse(parameters[2]) : 0,
                    Token = parameters.Length >= 4 ? parameters[3] : ""
                };

                return datosToken;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            // TODO: revisar para qué estos datos
            return new TokenUsuarioDto
            {
                IP = "10.83.215.115",
                SistemaID = sistemaId,
                usuarioID = "usr.intranet54",
                VersionEncripID = 1,
                Token = "f141a496592342c6a025f3bd10cf04ce"
            };
        }

        public void SetSessionVar(string variable, string value)
        {
            HttpContext.Current.Session[variable] = value;
        }

        public string GetSessionVar(string variable)
        {
            return HttpContext.Current.Session[variable].ToString() ?? "";
        }

        #region Autentificación AD

        public ResultadoOperacion CheckLoginAd(string username, string password)
        {
            var mensaje = "Error al validar las credenciales del usuario.";
            try
            {
                var ldapPath = WebConfigValues.LDAPPath;
                using (DirectoryEntry _entry = new DirectoryEntry(
                    ldapPath,
                    username,
                    password,
                    AuthenticationTypes.Secure))
                {
                    DirectorySearcher searcher = new DirectorySearcher(_entry);
                    searcher.Filter = "(objectclass=user)";
                    var sr = searcher.FindOne();
                    return new ResultadoOperacion(1, "OK", null);
                }
            }
            catch (DirectoryServicesCOMException ex)
            {
                // Logger.LogError(ex);
                Logger.LogInfo("Error de acceso. Usuario y/o password incorrecto. Username: " + username);
                mensaje = "Usuario y/o contraseña incorrecto.";
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Logger.LogError(ex);
                if (ex.Message.Contains("El servidor no es funcional") ||
                    ex.Message.Contains("The server is not operational"))
                {
                    mensaje = "Error, no fue posible contactar el servidor LDAP.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return new ResultadoOperacion(-1, mensaje, null);
        }

        public ResultadoOperacion GetUserDetailAD(string username)
        {
            var mensaje = "Error al obtener los datos del usuario.";
            try
            {
                var ldapPath = WebConfigValues.LDAPPath;
                var usernameAdmin = WebConfigValues.ADAdminUser;
                var passwordAdmin = WebConfigValues.ADAdminPassword;

                using (DirectoryEntry _entry = (string.IsNullOrWhiteSpace(usernameAdmin)
                    ? new DirectoryEntry(ldapPath)
                    : new DirectoryEntry(
                        ldapPath,
                        usernameAdmin,
                        passwordAdmin, 
                        AuthenticationTypes.Secure)))
                {
                    DirectorySearcher searcher = new DirectorySearcher(_entry);
                    searcher.Filter = "(&(objectClass=user)(anr=" + username + "))"; // _searcher.Filter = "(SAMAccountName=" + username + ")";
                    searcher.PropertiesToLoad.Add("displayname");   // displayname
                    // _searcher.PropertiesToLoad.Add("sAMAccountName");
                    //_searcher.PropertiesToLoad.Add("givenName");   // first name
                    //_searcher.PropertiesToLoad.Add("sn");          // last name
                    searcher.PropertiesToLoad.Add("mail");        // smtp mail address
                    var sr = searcher.FindOne();

                    if (sr == null)
                    {
                        mensaje = "Usuario no encontrado en AD.";
                    }
                    else
                    {
                        string displayname = sr.Properties.Contains("displayname") ? sr.Properties["displayname"][0].ToString() : "";
                        string mail = sr.Properties.Contains("mail") ? sr.Properties["mail"][0].ToString() : "";

                        var datos = new UsuarioDto
                        {
                            Username = username,
                            Activo = true,
                            Email = mail,
                            NombresApellidos = displayname
                        };
                        return new ResultadoOperacion(1, "OK", datos);
                    }
                }
            }
            catch (DirectoryServicesCOMException ex)
            {
                Logger.LogError(ex);
                mensaje = "Usuario y/o contraseña incorrecto.";
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Logger.LogError(ex);
                if (ex.Message.Contains("El servidor no es funcional") ||
                    ex.Message.Contains("The server is not operational"))
                {
                    mensaje = "Error, no fue posible contactar el servidor LDAP.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return new ResultadoOperacion(-1, mensaje, null);
        }

        #endregion
    }
}