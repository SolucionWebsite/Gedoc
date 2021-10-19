using Gedoc.Helpers.Dto;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;
using Gedoc.WebApp.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Gedoc.Repositorio.Model;
using Gedoc.Helpers;
using Gedoc.Helpers.Enum;
using Gedoc.Service.FirmaDigital;
using Kendo.Mvc.Infrastructure;

namespace Gedoc.WebApp.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IRequerimientoService _requerimientoSrv;
        private readonly IDespachoService _despachoSrv;
        private readonly IMantenedorService _mantenedorSrv;
     //   private Gedoc.Repositorio.Model.GedocEntities db = new Gedoc.Repositorio.Model.GedocEntities();

        public HomeController(IRequerimientoService requerimientoSrv, IDespachoService despachoSrv, IMantenedorService mantenedorSrv)
        {
            this._requerimientoSrv = requerimientoSrv;
            this._despachoSrv = despachoSrv;
            this._mantenedorSrv = mantenedorSrv;
        }

        protected override void Dispose(bool disposing)
        {
          //  db.Dispose();
            base.Dispose(disposing);
        }

        [IgnoreLoginFilter]
        public ActionResult ErrorAnonimo(string mensaje)
        {
            ViewBag.mensaje = mensaje.Replace("/r/n", "<br/>");
            return View("../Error/ErrorAnonimo");
        }

        [Authorize] 
        [IgnoreAccessFilter]
        public ActionResult Index()
        {

            return View();
        }

        #region Bandeja de Entrada

        [Authorize]
        public ActionResult BandejaEntrada(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            if (id == 13)
            {
                return RedirectToAction("BandejaEntradaDespInic");
            }

            ViewBag.CodigoBandeja = id;
            // Datos de todas las acciones permitidas en las bandejas
            var datosAcciones = _mantenedorSrv.GetDatosAccionesBandeja();
            ViewBag.DatosAcciones = datosAcciones;
            // Acciones permitidas en la bandeja por Estado y Etapa de ingresos
            var roles = CurrentUserRoles;
            var configBandeja = _mantenedorSrv.GetAccionesBandeja(id.Value, roles);
            if (configBandeja == null)
            {
                return RedirectToAction("ErrorAnonimo", new { mensaje = "La Bandeja de Entrada a la que intenta acceder no se encuentra en la aplicación." });
            }
            else
            {
                ViewBag.CurrentUserRoles = CurrentUserRoles;
                ViewBag.Acciones = configBandeja.Acciones;
                ViewBag.IdBandeja = configBandeja.Id;
                ViewBag.TituloBandeja = configBandeja.Titulo;
                ViewBag.DiasBandeja = configBandeja.DiasAtras;
                var accionesNoGrilla = datosAcciones.Where(acc => acc.TipoAccion == "B").ToList();
                var accionesOtras = accionesNoGrilla.Where(acc =>
                    configBandeja.Acciones.SelectMany(a => a.Acciones).Contains(acc.Id)).ToList();
                ViewBag.AccionesOtras = accionesOtras;

                #region Acciones para pestaña Priorizados
                var mostrarTabPrio = PermiteTabPriorizados(id.Value, configBandeja.Acciones, accionesNoGrilla);
                ViewBag.MostrarTabPrio = mostrarTabPrio;
                if (mostrarTabPrio)
                {
                    var idBandPrio = GetIdBandejaPrioFromBandejaMainId(id.GetValueOrDefault());
                    // Acciones permitidas en la pestaña Priorizados por Estado y Etapa de ingresos
                    configBandeja = _mantenedorSrv.GetAccionesBandeja(idBandPrio, roles);
                    if (configBandeja == null)
                    {
                        return RedirectToAction("ErrorAnonimo", new { mensaje = "La pestaña Priorizados a la que intenta acceder no se encuentra configurada en la aplicación." });
                    }
                    else
                    {
                        ViewBag.AccionesPrio = configBandeja.Acciones;
                        ViewBag.IdBandejaPrio = configBandeja.Id;
                        ViewBag.DiasBandejaPrio = configBandeja.DiasAtras;
                    }
                }
                #endregion

                #region Acciones para pestaña Oficios
                var idBandOfic = (int)Bandeja.Oficio;
                // Acciones permitidas en la pestaña Oficio por Estado y Etapa del oficio
                configBandeja = _mantenedorSrv.GetAccionesBandeja(idBandOfic, roles, bandejaMainId: id);
                var mostrarTabOfic = PermiteTabOficios(id.Value, configBandeja?.Acciones ?? new List<AccionPermitidaBandejaDto>());
                ViewBag.MostrarTabOfic = mostrarTabOfic;
                if (mostrarTabOfic)
                {
                    if (configBandeja == null)
                    {
                        return RedirectToAction("ErrorAnonimo", new { mensaje = "La pestaña Oficios a la que intenta acceder no se encuentra configurada en la aplicación." });
                    }
                    else
                    {
                        ViewBag.AccionesOfic = configBandeja.Acciones;
                        ViewBag.IdBandejaOfic = configBandeja.Id;
                        ViewBag.DiasBandejaOfic = configBandeja.DiasAtras;
                    }
                }
                #endregion

                return View();
            }
        }

        private bool PermiteTabPriorizados(int idBandeja, List<AccionPermitidaBandejaDto> accionesBand, List<AccionBandejaDto> acciones)
        {
            // Se chequea si alguno de los perfiles del usuario permite mostrar la pestaña de priorizados
            // y además si se está en alguna de las bandejas q permitan la pestaña. En accionesBand están
            // las acciones permitidas en la bandeja y en el perfil por tanto con verificar en accionesBand
            // es suficiente para saber si la bandeja y el perfil tienen permisos para mostrar la pestaña de Priorizados
            var idAccForzarPrio = acciones.FirstOrDefault(a => a.IdAccion == "FPR")?.Id ?? 0;
            return accionesBand.SelectMany(a => a.Acciones).Any(idAcc => idAcc == idAccForzarPrio);
        }

        private bool PermiteTabOficios(int idBandeja, List<AccionPermitidaBandejaDto> acciones)
        {
            // Se chequea si alguno de los perfiles del usuario permite mostrar la pestaña de oficios
            // y además si se está en alguna de las bandejas q permitan la pestaña. En accionesBand están
            // las acciones permitidas en la bandeja y en el perfil por tanto con verificar en accionesBand
            // es suficiente para saber si la bandeja y el perfil tienen permisos para mostrar la pestaña de oficios

            return acciones.Any(a => a.BandejaId == idBandeja);
            //return idBandeja == (int)Bandeja.SecretariaUt ||
            //       idBandeja == (int)Bandeja.ProfesionalUt ||
            //       idBandeja == (int)Bandeja.EncargadoUt ||
            //       idBandeja == (int)Bandeja.JefaturaUt ||
            //       idBandeja == (int)Bandeja.Despachos ||
            //       idBandeja == (int)Bandeja.Transparencia ||
            //       idBandeja == (int)Bandeja.Administracion; // TODO: precisar si la pestaña Oficios tendrá control por perfil. Ahora siempre se activa.

        }

        private int GetIdBandejaPrioFromBandejaMainId(int bandejaMainId)
        {
            return bandejaMainId == (int)Bandeja.SecretariaUt
                ? (int)Bandeja.PriorizacionSecretaria
                : (bandejaMainId == (int)Bandeja.ProfesionalUt
                    ? (int)Bandeja.PriorizacionProfesional
                    : (int)Bandeja.PriorizacionEncargado);
        }

        public ActionResult DatosBandejaEntrada(ParametrosGrillaDto<int> param) // [DataSourceRequest] DataSourceRequest request) // 
        {
            /* Filtros aplicados en las columnas de la grilla*/
            var filterSqlParams = new List<object>();
            var filterSql = KendoHelper.FiltersToParameterizedQuery(FilterDescriptorFactory.Create(param.Filter), paramValues: filterSqlParams);
            param.Filter = filterSql;
            param.FilterParameters = filterSqlParams.ToArray();

            var datos = _requerimientoSrv.GetDatosBandejaEntrada(param, CurrentUserId.GetValueOrDefault()); //idBandeja);

            var jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult DatosBandejaEntradaPrio(ParametrosGrillaDto<int> param) // [DataSourceRequest] DataSourceRequest request) // 
        {
            /* Filtros aplicados en las columnas de la grilla No se utiliza el array de parametros al transformar el filtro, se obtiene el filtro literal par q se conserve como tal al enviarlo a la página y luego ocuparlo al salvar la nueva fecha de resolución estimada*/
            var filterSql = KendoHelper.FiltersToParameterizedQuery(FilterDescriptorFactory.Create(param.Filter) );
            param.Filter = filterSql;

            var datos = _requerimientoSrv.GetDatosBandejaPriorizados(param, CurrentUserId.GetValueOrDefault()); //idBandeja);
            datos.Resultado.Extra = param; // para guardar en la página los últimos parametros utilizados para obtener los datos de la grilla. Se utiliza luego cuando se vaya a actualizar la Nueva Fecha Resolucion al utilizar la opción Aplicar a Todos

            var jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        #endregion

        #region Bandeja de Entrada de Despachos Iniciativas CMN
        public ActionResult BandejaEntradaDespInic()
        {
            var idBandeja = 13;
            // Datos de todas las acciones permitidas en las bandejas
            var datosAcciones = _mantenedorSrv.GetDatosAccionesBandeja();
            ViewBag.DatosAcciones = datosAcciones;
            // Acciones permitidas en la bandeja por Estado y Etapa de ingresos
            var roles = (int[])(Session["MisRolesId"] ?? new int[0]);
            var configBandeja = _mantenedorSrv.GetAccionesBandeja(idBandeja, roles);
            if (configBandeja == null)
            {
                return RedirectToAction("ErrorAnonimo", new { mensaje = "La Bandeja de Entrada a la que intenta acceder no se encuentra en la aplicación." });
            }
            else
            {
                ViewBag.Acciones = configBandeja.Acciones;
                ViewBag.IdBandeja = configBandeja.Id;
                ViewBag.TituloBandeja = configBandeja.Titulo;
                ViewBag.DiasBandeja = configBandeja.DiasAtras;
                var accionesNoGrilla = datosAcciones.Where(acc => acc.TipoAccion == "B").ToList();
                var accionesOtras = accionesNoGrilla.Where(acc =>
                    configBandeja.Acciones.SelectMany(a => a.Acciones).Contains(acc.Id)).ToList();
                ViewBag.AccionesOtras = accionesOtras;
                return View();
            }
        }

        public ActionResult DatosBandejaEntradaDespInic(ParametrosGrillaDto<int> param) // [DataSourceRequest] DataSourceRequest request) // 
        {
            var datos = _despachoSrv.GetDatosBandejaEntradaInic(param, CurrentUserId.GetValueOrDefault());

            var jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpPost]
        public ActionResult EliminarDespInic(int id)
        {
            var result = new ResultadoOperacion();
            try
            {
                var userid = Session["IdUsuario"].ToString();
                int.TryParse(userid, out var userIdInt);
                if (userIdInt <= 0)
                {
                    result.Codigo = -1;
                    result.Mensaje = "Se perdió la sesión";
                }
                else
                {
                    result = _despachoSrv.MarcaEliminadoDespInic(id, userIdInt);
                }
            }
            catch (Exception ex)
            {
                result.Codigo = -1;
                result.Mensaje = "Error al eliminar el Despacho Iniciativa";
                result.Extra = ex.Message;
            }
            return Json(result);
        }
        #endregion

        public ActionResult BusquedaBandeja(int bandejaId)
        {
            ViewBag.IdBandeja = bandejaId;

            return View("../Modals/BusquedaBandeja");
        }

        public ActionResult DocumentosIngresoBusqueda([DataSourceRequest] DataSourceRequest request, int? idBandeja)
        {
            var docsIng = _mantenedorSrv.GetDatosBandejaResumen(idBandeja.GetValueOrDefault(0));

            return this.Jsonp(docsIng.ToDataSourceResult(request));
        }

        public ActionResult DocumentosIngresoVM(string[] values, int? idBandeja)
        {
            var indices = new List<int>();

            if (values != null && values.Any())
            {
                var index = 0;
                var docsIng = _mantenedorSrv.GetDatosBandejaResumen(idBandeja.GetValueOrDefault(0));

                foreach (var di in docsIng)
                {
                    if (!string.IsNullOrEmpty(values[0]) && di.Title.Contains(values[0]))
                    {
                        indices.Add(index);
                    }

                    index += 1;
                }
            }

            return this.Jsonp(indices);
        }

        public ActionResult UnidadTecnicaBusqueda([DataSourceRequest] DataSourceRequest request, int? idBandeja)
        {
            List<GenericoDto> uts;

            //uts = _mantenedorSrv.GetGenericoMatenedor(Mantenedor.UnidadTecnica, EsAdmin() ? "" : CurrentUserId.GetValueOrDefault(0).ToString()).Data;
            if (EsAdmin())
            {
                uts = _mantenedorSrv.GetGenericoMatenedor(Mantenedor.UnidadTecnica).Data;
            }
            else
            {
                var esEncargado = idBandeja == (int) Gedoc.Helpers.Enum.Bandeja.EncargadoUt; // CurrentUserRoles.Contains((int)Gedoc.Helpers.Enum.Rol.EncargadoUt);
                uts = _mantenedorSrv.GetUnidadTecnicaByUsuario(CurrentUserId.GetValueOrDefault(), esEncargado)
                    .Select(d => new GenericoDto { Id = d.Id.ToString(), Titulo = d.Titulo })
                    .ToList();
            }

            return this.Jsonp(uts.ToDataSourceResult(request));
        }

        public ActionResult UnidadTecnicaVM(string[] values, int? idBandeja)
        {
            var indices = new List<int>();

            if (values != null && values.Any())
            {
                var index = 0;
                var uts = _mantenedorSrv.GetDatosBandejaResumenUt(idBandeja.GetValueOrDefault(0));

                foreach (var ut in uts)
                {
                    if (!string.IsNullOrEmpty(values[0]) && ut.Title.Contains(values[0]))
                    {
                        indices.Add(index);
                    }

                    index += 1;
                }
            }

            return this.Json(indices, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EstadoBusqueda([DataSourceRequest] DataSourceRequest request, int? idBandeja)
        {
            var estados = _mantenedorSrv.GetDatosBandejaResumenEstado(idBandeja.GetValueOrDefault(0));

            return this.Jsonp(estados.ToDataSourceResult(request));
        }

        public ActionResult EstadoVM(string[] values, int? idBandeja)
        {
            var indices = new List<int>();

            if (values != null && values.Any())
            {
                var index = 0;
                var estados = _mantenedorSrv.GetDatosBandejaResumenEstado(idBandeja.GetValueOrDefault(0));

                foreach (var estado in estados)
                {
                    if (!string.IsNullOrEmpty(values[0]) && estado.Title.Contains(values[0]))
                    {
                        indices.Add(index);
                    }

                    index += 1;
                }
            }

            return this.Json(indices, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UltimosIngresos()
        {
            var diasUltimosIng = WebConfigValues.DiasHabilesUltRegistros;
            var datos = _requerimientoSrv.GetIngresosUltimos(diasUltimosIng);

            var jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult UltimosDespachos()
        {
            var diasUltimosIng = WebConfigValues.DiasHabilesUltRegistros;
            var datos = _despachoSrv.GetDespachosUltimos(diasUltimosIng);

            var jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [IgnoreLoginFilter]
        public ActionResult Login()
        {
            if (HaySesionActiva())
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [IgnoreLoginFilter]
        public ActionResult Login(LoginModel model, string ReturnUrl)
        {
            var sessionHelper = new SessionHelper(UsuarioSrv);

            // Se comprueba las credenciales del usuario en AD
            var loginAd = sessionHelper.CheckLoginAd(model.Username, model.Password);
            if (loginAd.Codigo > 0)
            {
                // Se busca el usuario en Gedoc
                var res = _mantenedorSrv.GetUsuarioByUserName(model.Username);

                if (res != null && res.Id > 0)
                {
                    Session["IdUsuario"] = res.Id;
                    Session["NombreUsuario"] = res.NombresApellidos;
                    Session["Username"] = res.Username;
                    Session["MisRoles"] = res.Rol.Where(r => r.Activo).Select(r => r.Titulo).ToArray();
                    Session["MisRolesId"] = res.Rol.Where(r => r.Activo).Select(r => r.Id).ToArray();
                    Session["MisUnidadesTecn"] = res.UnidadTecnicaIntegrante.Where(r => r.Activo).Select(r => r.Titulo).ToArray();
                    Session["MisUnidadesTecnId"] = res.UnidadTecnicaIntegrante.Where(r => r.Activo).Select(r => r.Id).ToArray();
                    FormsAuthentication.SetAuthCookie(res.Username, true);
                    return Redirect(ReturnUrl ?? "/Home/Index");
                }
                model.Mensaje = "El usuario no tiene acceso a Gedoc.";
            }
            else
            {
                model.Mensaje = loginAd.Mensaje;
            }

            return View(model);
        }

        [IgnoreLoginFilter]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Remove("IdUsuario");
            Session.Remove("NombreUsuario");
            Session.Remove("MisRoles");
            Session.Remove("MisRolesId");
            Session.Remove("Username");
            Session.Remove("MisUnidadesTecn");
            Session.Remove("MisUnidadesTecnId");
            return RedirectToAction("Login");
        }

        [IgnoreAccessFilter]
        public ActionResult BuscadorRegmon()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveForzarPrioridad(ParametrosGrillaDto<int> param, DateTime? nuevaFecha, int? idRequerimiento)
        {
            var resultado = new ResultadoOperacion();
            if (!HaySesionActiva())
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.";
                return Json(resultado);
            }

            var usuarioActual = new UsuarioActualDto()
            {
                UsuarioId = CurrentUserId.GetValueOrDefault(),
                DireccionIp = ClientIpAddress(),
                UsuarioNombre = "",
                LoginName = CurrentUserName,
                NombrePc = ClientHostName(),
                UserAgent = ClientAgent()
            };
            resultado = _requerimientoSrv.SaveForzarPrioridad(param, usuarioActual, nuevaFecha,
                idRequerimiento);

            return Json(resultado);
        }

        [HttpGet]
        public ActionResult GetCantidadPriorizados(ParametrosGrillaDto<int> param)
        {
            var idBandPrio = GetIdBandejaPrioFromBandejaMainId(param.Dato);
            var band = _mantenedorSrv.GetBandejaById(idBandPrio, true);
            param.Dato = band?.Id ?? 0;
            param.ExtraData = "NOTIF";
            var datos = _requerimientoSrv.GetDatosBandejaPriorizados(param, CurrentUserId.GetValueOrDefault());
            return Json((datos?.Data?.Count ?? 0) > 0 
                    ? ((List<int>)datos.Data[0].Items ?? new List<int>()).Count
                    : 0
                , JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult EliminarRequerimiento(int id)
        {
            var result = new ResultadoOperacion();
            try
            {
                var userid = Session["IdUsuario"].ToString();
                int.TryParse(userid, out var userIdInt);
                if (userIdInt <= 0)
                {
                    result.Codigo = -1;
                    result.Mensaje = "Se perdió la sesión";
                }
                else
                {
                    result = _requerimientoSrv.MarcaEliminado(id, GetDatosUsuarioActual());
                }
            }
            catch (Exception ex)
            {
                result.Codigo = -1;
                result.Mensaje = "Error al eliminar el Despacho Iniciativa";
                result.Extra = ex.Message;
            }
            return Json(result);
        }

    }
}