using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers.Maps.Interface;
using Gedoc.WebApp.Models;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.WebApp.Helpers;

namespace Gedoc.WebApp.Controllers
{
    public class AdjuntoController : BaseController
    {
        private readonly IGenericMap _mapper;
        private readonly IAdjuntoService _adjuntoSrv;
        private readonly IRequerimientoService _requerimientoService;

        public AdjuntoController(IGenericMap mapper, IAdjuntoService adjuntoSrv, IRequerimientoService requerimientoService, IMantenedorService mantenedorSrv)
        {
            _mapper = mapper;
            _adjuntoSrv = adjuntoSrv;
            _requerimientoService = requerimientoService;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(AdjuntoModel model, IEnumerable<HttpPostedFileBase> files)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }
            AdjuntoDto datos = _mapper.MapFromModelToDto<AdjuntoModel, AdjuntoDto>(model);
            datos.CreadoPor = CurrentUserName;
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioCreacionId = CurrentUserId;
            var resultadoOper = _adjuntoSrv.Save(datos, files);
            return Json(resultadoOper);
        }

        public ActionResult AdjuntosIngresoForm(int idIngreso, int idBandeja)
        {
            RequerimientoDto datosIng = _requerimientoService.GetById(idIngreso);
            ViewBag.IdRequerimiento = idIngreso;
            ViewBag.IdBandeja = idBandeja;
            ViewBag.DocIngreso = datosIng != null ? datosIng.DocumentoIngreso : "";

            int userId;
            int.TryParse(Session["IdUsuario"].ToString(), out userId);
            ViewBag.DeleteAccion = UserIsInAction(userId, "DA");

            return View("GrillaAdjunto");
        }

        public ActionResult AdjuntosIngreso(int reqId)
        {
            DatosAjax<List<AdjuntoDto>> datos = _adjuntoSrv.GetAdjuntosIngreso(reqId);

            JsonResult jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [IgnoreAccessFilter]
        public ActionResult GetArchivo(int idAdjunto)
        {
            var datosAdj = _adjuntoSrv.GetArchivo(idAdjunto);

            if (datosAdj.FileStream != null)
            {
                FileResult fileResult = File(datosAdj.FileStream,
                    System.Net.Mime.MediaTypeNames.Application.Octet,
                    datosAdj.FileName);

                return fileResult;
            }
            else
            {
                return RedirectToAction("ErrorAnonimo", "Home",
                    new { mensaje = "Ocurrió un error al descargar el archivo. " + datosAdj.Mensaje });
            }
        }

        public ActionResult AccionAdjunto(string idAccion, int idAdjunto, int idIngreso, int idBandeja)
        {
            RequerimientoDto datosIng = _requerimientoService.GetById(idIngreso);
            AdjuntoDto dto = _adjuntoSrv.GetAdjuntoById(idAdjunto);
            AdjuntoModel model = _mapper.MapFromDtoToModel<AdjuntoDto, AdjuntoModel>(dto) ??
                        new AdjuntoModel()
                        {
                            RequerimientoId = idIngreso
                        };
            ViewBag.Accion = idAccion;
            ViewBag.AccesoForm = ValidaAccesoForm();
            model.DocIngreso = datosIng != null ? datosIng.DocumentoIngreso : "";
            model.BandejaId = idBandeja;
            return View("FormAdjunto", model);
        }

        public ActionResult EliminarAdjunto()
        {
            var resultado = ValidaAccesoForm();
            if (resultado.Codigo < 0)
                return Json(resultado);
            return View("EliminarAdjunto");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Grid_Read([DataSourceRequest]DataSourceRequest request, DateTime? fDesde, DateTime? fHasta)
        {
            if (!fDesde.HasValue || !fHasta.HasValue)
            {
                return null;
            }

            var result = _adjuntoSrv.GetAdjuntosUsuario(fDesde.GetValueOrDefault(), fHasta.GetValueOrDefault(),
                CurrentUserId.GetValueOrDefault());

            return Json(result);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EliminaAdjunto(int[] adjuntoIds)
        {
            var resultado = ValidaAccesoForm();
            if (resultado.Codigo > 0)
                resultado = _adjuntoSrv.MarcaAdjuntosEliminado(adjuntoIds, CurrentUserId.GetValueOrDefault(0));

            return Json(resultado);
        }

        private ResultadoOperacion ValidaAccesoForm()
        {
            var result = new ResultadoOperacion(1, "OK", null);

            // TODO: chequear si el perfil del usuario tiene acceso al formulario (en la app no debe aparecerle la acción para acceder al form pero hacer el chequeo aquí para aumetar la seguridad)

            if (!HaySesionActiva())
            {
                result.Codigo = -1;
                result.Mensaje = "La sessión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.";
            }

            return result;
        }


        }
}