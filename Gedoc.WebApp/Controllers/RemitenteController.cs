using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers.Maps.Interface;
using Gedoc.WebApp.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Gedoc.WebApp.Controllers
{
    public class RemitenteController : BaseController
    {
        private readonly IMantenedorMap _mapper;
        private readonly IMantenedorService _mantenedorSrv;

        public RemitenteController(IMantenedorService mantenedorSrv, IMantenedorMap mapper)
        {
            _mantenedorSrv = mantenedorSrv;
            _mapper = mapper;
        }

        public class SearchParams { 
            public string sFilter { get; set; }
        }

        public ActionResult Index()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GridBuscarRemitente_Read([DataSourceRequest] DataSourceRequest request, string sFilter/*, SearchParams bParams*/)
        {
            if (request.Filters != null)
            {
                //ModifyFilters(request.Filters);
            }

            //verifica que venga almenos 1 filtro
            var datos = _mantenedorSrv.GetRemitenteByFilter(sFilter);
            var remitentes = datos.ToDataSourceResult(request);
            return Json(remitentes);
        }

        public ActionResult NuevoRemitente()
        {
            var model = new RemitenteModel();
            return View("FormRemitente", model);
        }

        public ActionResult EditarRemitente(int Id)
        {
            var remitente = _mantenedorSrv.GetRemitenteById(Id).Data;

            var model = _mapper.MapFromDtoToModel(remitente);

            return View("FormRemitente", model);
        }

        [HttpPost]
        public ActionResult DesactivarRemitente(int? Id)
        {
            var resultado = _mantenedorSrv.DesactivaRemitente(Id.GetValueOrDefault(0), GetDatosUsuarioActual());

            return Json(resultado.Codigo > 0, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(RemitenteModel model)
        {
            var datos = _mapper.MapFromModelToDto(model);

            if (model.Id == 0)
            {
                datos.FechaCreacion = DateTime.Now;
                datos.UsuarioCreacionId = (int)Session["IdUsuario"];
                datos.Activo = true;
            }
            else
            {
                datos.Activo = true;
                datos.FechaModificacion = DateTime.Now;
                datos.UsuarioModificacionId = (int)Session["IdUsuario"];
            }

            var resultadoOper = _mantenedorSrv.SaveRemitente(datos);

            return Json(resultadoOper);
        }
    }
}