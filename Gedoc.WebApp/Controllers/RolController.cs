using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Gedoc.Helpers.Dto;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;

namespace Gedoc.WebApp.Controllers
{
    public class RolController : BaseController
    {
        private readonly IUsuarioService _usuarioSvc;

        public RolController(IUsuarioService usuarioSvc)
        {
            _usuarioSvc = usuarioSvc;
        }

        [Authorize]
        public ActionResult Index()
        {
            ViewBag.Title = "Mantenedor de Perfiles";
            ViewBag.Message = "Esta es la página de mantenedor de roles";

            return View();
        }

        public ActionResult Roles_Read([DataSourceRequest]DataSourceRequest request)
        {
            var result = _usuarioSvc.GetRolAll();

            var dsResult = result.ToDataSourceResult(request, ModelState);
            return Json(dsResult);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Rol_Create([DataSourceRequest]DataSourceRequest request, RolDto rol)
        {
            if (ModelState.IsValid)
            {
                var resultado = _usuarioSvc.SaveRol(rol);
                rol.Id = resultado.Codigo > 0 ? Convert.ToInt32(resultado.Extra) : rol.Id;
                if (resultado.Codigo < 0)
                    ModelState.AddModelError("Error", resultado.Mensaje);
            }

            return Json(new[] { rol }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Rol_Update([DataSourceRequest]DataSourceRequest request, RolDto rol)
        {
            if (ModelState.IsValid)
            {
                var resultado = _usuarioSvc.SaveRol(rol);
                if (resultado.Codigo < 0)
                    ModelState.AddModelError("Error", resultado.Mensaje);
            }

            return Json(new[] { rol }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Rol_Destroy([DataSourceRequest]DataSourceRequest request, RolDto rol)
        {
            if (ModelState.IsValid)
            {
                var resultado = _usuarioSvc.DeleteRol(rol.Id);
                if (resultado.Codigo < 0)
                    ModelState.AddModelError("Error", resultado.Mensaje);
            }

            return Json(new[] { rol }.ToDataSourceResult(request, ModelState));
        }

        [IgnoreAccessFilter(UrlBase = "/Usuario")]
        public ActionResult RolesUsuario(int userId)
        {
            var roles = _usuarioSvc.GetRolesUsuario(userId, true);
            return View(roles);
        }

        [HttpPost]
        public ActionResult GuardarRolesUsuario(int userId, List<RolDto> roles)
        {
            var rolIds = roles?.Count() > 0 ? roles.Select(r => r.Id).ToList() : new List<int>();
            var resultado = _usuarioSvc.UpdateRolesUsuario(userId, roles);
            return Json(true, JsonRequestBehavior.AllowGet);
        }


        [IgnoreAccessFilter(UrlBase = "/Rol")]
        public ActionResult MenuRoles(int menuId)
        {
            var roles = _usuarioSvc.GetRolesMenu(menuId);
            return View(roles);
        }

        [HttpPost]
        public ActionResult GuardarMenusRoles(int menuId, List<RolDto> roles)
        {
            var resultado = _usuarioSvc.UpdateRolesMenu(menuId, roles);
            return Json(resultado, JsonRequestBehavior.AllowGet);
        }

        [IgnoreAccessFilter(UrlBase = "/Rol")]
        public ActionResult AccionesRol(int IdRol)
        {
            var grupoAcciones = _usuarioSvc.GetAccionesActivasrol(IdRol);
            return View(grupoAcciones);
        }

        [HttpPost]
        public ActionResult GuardarAccionesRol(int rolId, List<AccionBandejaDto> acciones)
        {
            var datos = _usuarioSvc.UpdateAccionesRol(rolId, acciones);
            return Json(datos, JsonRequestBehavior.AllowGet);
        }
    }
}
