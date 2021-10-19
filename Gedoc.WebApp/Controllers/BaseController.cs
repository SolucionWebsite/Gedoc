using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Services.Description;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Logging;
using Gedoc.Helpers.NotificacionesWeb;
using Gedoc.Repositorio.Model;
using Gedoc.Service.DataAccess;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;

namespace Gedoc.WebApp.Controllers
{
    public class BaseController : Controller
    {
        public int IdUsuarioActivo = 0;
        public int IdFuncionarioActivo = 0;

        [Inject]
        protected IUsuarioService UsuarioSrv { get; set; } // = new UsuarioService(); //TODO: usuar DI

        //public BaseController(IUsuarioService usuarioSrv)
        //{
        //    this._usuarioSrv = usuarioSrv;
        //}

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.FormatoLargo = "dd/MM/yyyy HH:mm";
            LoginValidate(filterContext);

            if (filterContext.ActionDescriptor.ActionName != "ErrorAnonimo" && !BrowserCompatible())
            {
                filterContext.Result = RedirectToAction("ErrorAnonimo", "Home", new RouteValueDictionary() { { "mensaje", "Lo sentimos, el navegador utilizado no es compatible con la aplicación. Se recomienda utilizar Google Chrome, Mozilla Firefox o Microsoft Edge." } });
                return;
            }
            base.OnActionExecuting(filterContext);
        }
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonpResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }

        public void LoginValidate(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(IgnoreLoginFilter), false).Any())
            {
                return;
            }
            var sessionHelper = new SessionHelper(UsuarioSrv);
            var datosUsuario = new UsuarioDto(); // TODO: quitar si no es necesario
            if (!HaySesionActiva()) // !sessionHelper.UsuarioValido(filterContext, datosUsuario)) // 
            { // Usuario no válido o sesión expirada
                if (EsLlamadaAjax(filterContext))
                {
                    if (((System.Web.Mvc.ReflectedActionDescriptor) filterContext.ActionDescriptor).MethodInfo
                        .ReturnType == typeof(System.Web.Mvc.JsonResult))
                    {
                        var result = new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null);
                        filterContext.Result = Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        filterContext.Result = base.RedirectToAction("SessionExpired", "Error"); 
                    }
                }
                else
                {
                    filterContext.Result = base.RedirectToAction("Login", "Home");
                }

                //if (filterContext.HttpContext.Request.Path.IndexOf("/usuarios", StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                //    filterContext.HttpContext.Request.Path.IndexOf("/saveusuario", StringComparison.InvariantCultureIgnoreCase) >= 0)
                //{
                //    // Si se accede a la página de usuarios y no hay usuarios en BD se permite acceder a la página de usuarios
                //    var users = UsuarioSrv.GetUsuarios();
                //    if (users.Count > 0)
                //    {
                //        filterContext.Result = new RedirectResult(WebConfigValues.UrlAuthentication);
                //    }
                //}
                //else
                //{
                //    filterContext.Result = new RedirectResult(WebConfigValues.UrlAuthentication);
                //}
            }
            else
            {
                // Se valida q el usuario tenga permiso para acceder a la url
                if (!EsLlamadaAjax(filterContext))
                {
                    var url = this.Request.Url.PathAndQuery;
                    if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(IgnoreAccessFilter), false).Any())
                    {
                        var filter = filterContext.ActionDescriptor.GetCustomAttributes(typeof(IgnoreAccessFilter),
                            false);
                        var filterUrl = ((Gedoc.WebApp.Helpers.IgnoreAccessFilter[]) filter)[0].UrlBase;
                        url = filterUrl;
                    }
                    if (!UsuarioSrv.AccesoUrlUsuario(CurrentUserId.GetValueOrDefault(), url))
                    {
                        filterContext.Result = base.RedirectToAction("Index", "Home");
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }

        protected void NotificarResultado(ResultadoOperacion res)
        {
            Notificaciones.Enqueue(new NotificacionUi(res.Codigo <= 0 ? TipoNotificacionEnum.Error : TipoNotificacionEnum.Exito, res.Mensaje));
        }

        protected void NotificarResultado(ResultadoOperacion res, string jscript)
        {
            Notificaciones.Enqueue(new NotificacionUi(res.Codigo <= 0 ? TipoNotificacionEnum.Error : TipoNotificacionEnum.Exito, res.Mensaje, jscript));
        }

        public Queue<NotificacionUi> Notificaciones
        {
            get
            {
                if (Session["Notificaciones"] == null)
                {
                    Session["Notificaciones"] = new Queue<NotificacionUi>();
                }
                return Session["Notificaciones"] as Queue<NotificacionUi>;
            }
            set
            {
                Session["Notificaciones"] = value;
            }
        }

        protected bool EsLlamadaAjax(ActionExecutingContext filterContext)
        {
            if (filterContext == null)
                return AjaxRequestExtensions.IsAjaxRequest(this.Request);
            else
                return filterContext.HttpContext.Request.IsAjaxRequest();
        }

        protected ActionResult GetRespuesta(ResultadoOperacion resultado, string view, bool notificarResultadoAjax = false)
        {
            if (EsLlamadaAjax(null))
            {
                if (notificarResultadoAjax)
                    NotificarResultado(resultado);
                return Json(new
                {
                    data = "",
                    resultado = resultado

                }, JsonRequestBehavior.AllowGet);
            }
            NotificarResultado(resultado);
            return RedirectToAction(view);
        }

        protected void GetDatosPerfil()
        {
            //var appSrvc = new CommonAppServices();
            //var rol = appSrvc.GetRolUsuario(IdUsuarioActivo);

            //#region Datos del perfil
            //ViewBag.RolId = rol.Id;
            //ViewBag.NombreRol = rol.Nombre ?? "";

            //#endregion
        }

        protected bool AccesoRol(string accion, string tipoAcc, string permisosRol = null)
        {
            var accionesBandeja = permisosRol ?? "";
            var permisosAdmin = permisosRol ?? "";
            var permisosRep = permisosRol ?? "";
            //if (string.IsNullOrWhiteSpace(permisosRol))
            //{
            //    var appSrvc = new CommonAppServices();
            //    var rol = appSrvc.GetRolUsuario(IdUsuarioActivo);
            //    accionesBandeja = rol.AccionesBandeja;
            //    permisosAdmin = rol.PermisosAdmin;
            //    permisosRep = rol.PermisosReportes;
            //}

            var accionesRol = "";
            if (tipoAcc == "B")
            {
                accionesRol = ";" + accionesBandeja + ";";

            }
            if (tipoAcc == "A")
            {
                accionesRol = ";" + permisosAdmin + ";";

            }
            if (tipoAcc == "R")
            {
                accionesRol = ";" + permisosRep + ";";

            }

            if (accion.Contains(";"))
            {
                var tieneAcceso = false;
                var accionesArr = accion.Split(';');
                foreach (string acc in accionesArr)
                {
                    tieneAcceso = accionesRol.Contains(";" + acc + ";");
                    if (tieneAcceso)
                        break;
                }
                return tieneAcceso;
            }
            accion = ";" + accion + ";";
            return accionesRol.Contains(accion);

        }

        private bool BrowserCompatible()
        {
            var esCompatible = true;
            return esCompatible;

            //try
            //{
            //    var esCompatibleSession = (string)System.Web.HttpContext.Current.Session["NavegadorCompatible"];
            //    if (!string.IsNullOrWhiteSpace(esCompatibleSession))
            //    {
            //        if (esCompatibleSession == "true")
            //            return true;
            //        if (esCompatibleSession == "false")
            //            return false;
            //    }
            //    System.Web.HttpContext.Current.Session["NavegadorCompatible"] = "true";
            //    var navIncompatibles = ""; // WebConfigValues.NavegadoresNoCompatibles;
            //    if (!string.IsNullOrWhiteSpace(navIncompatibles))
            //    {
            //        var navIncompatiblesArr = navIncompatibles.Split(';');
            //        var browser = Request.Browser;
            //        var versionNavNum = 1000f;
            //        float.TryParse(browser.Version, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out versionNavNum);
            //        foreach (var nav in navIncompatiblesArr)
            //        {
            //            // La key NavegadoresNoCompatibles del web.config debe ser de la forma "NombreNavegador1-version;NombreNavegador1-version;...;NombreNavegadorN-version"
            //            var nombreNavIncomp = nav.Substring(0, nav.IndexOf('-')).Trim().ToLower();
            //            var versionNavIncomp = nav.Substring(nav.IndexOf('-') + 1, nav.Length - nombreNavIncomp.Length - 1).Trim();
            //            var versionNavIncompNum = 0f;
            //            float.TryParse(versionNavIncomp, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out versionNavIncompNum);
            //            if (nombreNavIncomp == browser.Browser.ToLower() && versionNavNum < versionNavIncompNum)
            //            {
            //                esCompatible = false;
            //                System.Web.HttpContext.Current.Session["NavegadorCompatible"] = "false";
            //                break;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    System.Web.HttpContext.Current.Session["NavegadorCompatible"] = "";
            //    Logger.LogError(ex);
            //}

            //return esCompatible;
        }

        public bool EsAdmin()
        {
            return (Session["MisRoles"] != null &&
                    ((IEnumerable<string>)Session["MisRoles"]).Contains("Propietarios gestor documental CMN"));
        }

        public bool HaySesionActiva(ActionExecutingContext filterContext = null)
        { // Se utiliza también en algunos formularios
            //var sessionHelper = new SessionHelper(UsuarioSrv);
            //var datosUsuario = new UsuarioDto(); // TODO: quitar si no es necesario
            //return sessionHelper.UsuarioValido(filterContext, datosUsuario);
            return User.Identity.IsAuthenticated && Session["IdUsuario"] != null;
        }

        public string CurrentUserName
        {
            get { return (Session["Username"] ?? "").ToString(); }
        }

        public int? CurrentUserId
        {
            get
            {
                var ok = Int32.TryParse((Session["IdUsuario"] ?? "").ToString(), out var idUsuario);
                return ok ? idUsuario : (int?)null;
            }
        }

        public int[] CurrentUserRoles
        {
            get { return (int[])(Session["MisRolesId"] ?? new int[0]); }
        }

        public string BaseUrl
        {
            get
            {
                if (Request.Url != null && Request.ApplicationPath != null)
                    return Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
                else
                    return "";
            }
        }

        public string ClientIpAddress()
        {
            try
            {
                string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ip))
                {
                    ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }

                return ip;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                return "";
            }
        }

        public string ClientHostName()
        {
            try
            {
                string host = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_HOST"];

                return host;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                return "";
            }
        }

        public string ClientAgent()
        {
            try
            {
                string agent = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT:"];

                return agent;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                return "";
            }
        }

        public UsuarioActualDto GetDatosUsuarioActual()
        {
            return new UsuarioActualDto()
            {
                UsuarioId = CurrentUserId.GetValueOrDefault(),
                DireccionIp = ClientIpAddress(),
                UsuarioNombre = "",
                LoginName = CurrentUserName,
                NombrePc = ClientHostName(),
                UserAgent = ClientAgent()
            };
        }

        public bool UserIsInAction(int userId, string accionId)
        {
            try
            {
                using (var db = new GedocEntities())
                {
                    var tieneAccion = db.Usuario.Any(a => a.Id == userId
                        && a.Rol.Any(b => b.AccionBandeja.Any(c => c.IdAccion == accionId)));
                    return tieneAccion;
                }
            }
            catch (Exception)
            {
                return false;
            }           
        }




    }
}
