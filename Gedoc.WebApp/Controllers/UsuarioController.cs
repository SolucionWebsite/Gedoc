using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Gedoc.Helpers.Dto;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;

namespace Gedoc.WebApp.Controllers
{
    public class UsuarioController : BaseController
    {
        private readonly IUsuarioService _usuarioSvc;

        public UsuarioController(IUsuarioService usuarioSvc)
        {
            _usuarioSvc = usuarioSvc;
        }

        [Authorize]
        public ActionResult Index()
        {
            ViewBag.Title = "Mantenedor de Usuarios";
            return View();
        }

        public ActionResult Usuario_Read([DataSourceRequest]DataSourceRequest request)
        {
            var usuarios = _usuarioSvc.GetUsuarios();

            var dsResult = usuarios.ToDataSourceResult(request, ModelState);
            return Json(dsResult);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Usuario_Create([DataSourceRequest]DataSourceRequest request, UsuarioDto usuario)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(usuario.Username))
                {
                    ModelState.AddModelError("Error", "El campo Username es obligatorio.");
                }
                else
                {
                    var sessionHlp = new SessionHelper(_usuarioSvc);
                    var resultado = sessionHlp.GetUserDetailAD(usuario.Username);
                    if (resultado.Codigo < 0)
                    {
                        ModelState.AddModelError("Error", resultado.Mensaje);
                    }
                    else
                    {
                        var usuarioAD = (resultado.Extra as UsuarioDto) ?? new UsuarioDto();
                        usuario.NombresApellidos = usuarioAD.NombresApellidos;
                        usuario.Email = usuarioAD.Email;
                        resultado = _usuarioSvc.SaveUsuario(usuario);
                        if (resultado.Codigo < 0)
                            ModelState.AddModelError("Error", resultado.Mensaje);
                    }
                }

            }            

            return Json(new[] { usuario }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Usuario_Update([DataSourceRequest]DataSourceRequest request, UsuarioDto usuario)
        {
            if (ModelState.IsValid)
            {
                var sessionHlp = new SessionHelper(_usuarioSvc);
                var resultadoAd = sessionHlp.GetUserDetailAD(usuario.Username);
                if (resultadoAd.Codigo > 0)
                {
                    // Se toman los datos del usuario (Nombres y apellidos, y Email) desde el AD para actualizarlo en Gedoc
                    var usuarioAD = (resultadoAd.Extra as UsuarioDto) ?? new UsuarioDto();
                    usuario.NombresApellidos = !string.IsNullOrEmpty(usuarioAD.NombresApellidos) ? usuarioAD.NombresApellidos : usuario.NombresApellidos;
                    usuario.Email = usuarioAD.Email;
                }
                var resultado = _usuarioSvc.SaveUsuario(usuario);
                if (resultado.Codigo < 0)
                    ModelState.AddModelError("Error", resultado.Mensaje);
                else if (resultadoAd.Codigo < 0)
                    ModelState.AddModelError("ErrorAD", "Atención. Los datos especificados se grabaron con éxito, pero no fue posible <br/> " +
                                                      "contactar el Active Directory para traer los datos actualizados del usuario.<br/>");

            }            

            return Json(new[] { usuario }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        public ActionResult UpdateActivo(UsuarioDto usuario)
        {
            var resultadoOper = _usuarioSvc.SaveUsuario(usuario, true);

            return Json(resultadoOper);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Usuario_Destroy([DataSourceRequest]DataSourceRequest request, UsuarioDto usuario)
        {
            if (ModelState.IsValid)
            {
                var resultado = _usuarioSvc.DeleteUsuario(usuario?.Id ?? 0);
                if (resultado.Codigo < 0)
                    ModelState.AddModelError("Error", resultado.Mensaje);
            }

            return Json(new[] { usuario }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult SolicitanteUrgencia()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GridSolicitanteUrgencia_Read([DataSourceRequest] DataSourceRequest request, string sFilter/*, SearchParams bParams*/)
        {
            if (request.Filters != null)
            {
                //ModifyFilters(request.Filters);
            }

            var datos = _usuarioSvc.GetUsuariosUrgencia().Data;

            var usuarioSU = datos.ToDataSourceResult(request);

            return Json(usuarioSU);
        }

        public ActionResult NuevoSolicitanteUrgencia()
        {
            return View("FormSolicitanteUrgencia");
        }

        public ActionResult EditarSolicitanteUrgencia(int Id)
        {
            //var usuarioSU = db.Usuario.Find(Id);
            //var model = new UsuarioDto()
            //{
            //    Id = usuarioSU.Id,
            //    NombresApellidos = usuarioSU.NombresApellidos
            //};

            //return View("FormRemitente", model);

            return null;
        }

        public ActionResult GetUsuariosJefaturaNoUrgencia()
        {
            var result = _usuarioSvc.GetUsuariosJefaturaNoUrgencia().Data;

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DesactivarSolicitanteUrgencia(int? Id)
        {
            var resultado = _usuarioSvc.UpdateSolicitanteUrgencia(Id.GetValueOrDefault(), false);

            return Json(resultado, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ActivarSolicitanteUrgencia(int idUsuario)
        {
            var resultado = _usuarioSvc.UpdateSolicitanteUrgencia(idUsuario, true);

            return Json(resultado);
        }

        public ActionResult GetUsuarioFromAD(string userName)
        {
            var sessionHlp = new SessionHelper(_usuarioSvc);
            var result = sessionHlp.GetUserDetailAD(userName);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}