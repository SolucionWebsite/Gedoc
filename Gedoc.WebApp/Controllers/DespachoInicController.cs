using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;
using Gedoc.WebApp.Models;
using Gedoc.WebApp.Helpers.Maps.Interface;

namespace Gedoc.WebApp.Controllers
{
    public class DespachoInicController : BaseController
    {
        private readonly IGenericMap _mapper;
        private readonly IDespachoService _despachoSrv;

        public DespachoInicController(IGenericMap mapper, IDespachoService despachoSrv)
        {
            _mapper = mapper;
            _despachoSrv = despachoSrv;
        }

        public ActionResult NuevoDespacho()
        {
            var model = new DespachoIniciativaModel();
            ViewBag.Accion = "NUEVO";
            return PartialView("FormDespacho", model);
        }

        public ActionResult EditDespacho()
        {
            var model = new DespachoIniciativaModel(); // GetDespacho
            ViewBag.Accion = "ED";
            return PartialView("FormDespacho", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(DespachoIniciativaModel model, IEnumerable<HttpPostedFileBase> files)
        {
            var datos = _mapper.MapFromModelToDto<DespachoIniciativaModel, DespachoIniciativaDto>(model);
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioCreacionId = CurrentUserId;
            var resultadoOper = _despachoSrv.SaveDespachoInic(datos, files);

            return Json(resultadoOper);
        }

        public ActionResult AccionDespacho(string idAccion, int idDespacho)
        {
            FlujoIngreso formIng = FlujoIngreso.Despacho;
            switch (idAccion)
            {
                case "EI": // Editar Despacho
                    formIng = FlujoIngreso.DespachoInicEdicion;
                    break;
                case "CI": // Cerrar Despacho
                    formIng = FlujoIngreso.DespachoInicCierre;
                    break;
                default:
                    formIng = FlujoIngreso.DespachoInic;
                    break;
            }
            ViewBag.Accion = idAccion;
            ViewBag.Form = formIng;
            ViewBag.AccesoForm = ValidaAccesoForm();
            var dto = _despachoSrv.GetDespachoInicById(idDespacho);
            var model = _mapper.MapFromDtoToModel<DespachoIniciativaDto, DespachoIniciativaModel>(dto) ??
                        new DespachoIniciativaModel
                        {
                            AdjuntaDocumentacion = false
                        };

            if (idAccion == "EI")
            {
                var validacionNumOfic = _despachoSrv.ValidaNumeroOficio(model.NumeroDespacho);
                ViewBag.DesdeReservaCorr = validacionNumOfic.Codigo == -3;
            }
            return PartialView("FormDespacho", model);
        }

        public ActionResult CierreDespacho(int idDespacho)
        {
            FlujoIngreso formIng = FlujoIngreso.DespachoInicCierre;
            ViewBag.Accion = "CE";
            ViewBag.Form = formIng;
            ViewBag.AccesoForm = ValidaAccesoForm();

            var dto = _despachoSrv.GetDespachoInicById(idDespacho);
            var model = _mapper.MapFromDtoToModel<DespachoIniciativaDto, DespachoIniciativaModel>(dto) ??
                        new DespachoIniciativaModel();
            return PartialView("FormDespachoCierre", model);
        }

        [IgnoreAccessFilter]
        public ActionResult GetArchivo(int idDespacho)
        {
            var datosAdj = _despachoSrv.GetArchivoDespInic(idDespacho);

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

        public ActionResult DownloadFile(string filePath)
        {
            string fullName = Server.MapPath("~" + filePath);

            byte[] fileBytes = GetFile(fullName);
            return File(
                fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filePath);
        }

        byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }

        private ResultadoOperacion ValidaAccesoForm()
        {
            var result = new ResultadoOperacion(1, "OK", null);

            // TODO: chequear si el perfil del usuario tiene acceso al formulario (en la app no debe aparecerle la acción para acceder al form pero hacer el chequeo aquí para aumetar la seguridad)

            if (!HaySesionActiva())
            {
                result.Codigo = -1;
                result.Mensaje = "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.";
            }

            return result;
        }



    }
}