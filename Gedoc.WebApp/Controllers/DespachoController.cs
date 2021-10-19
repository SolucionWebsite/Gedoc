using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.Service.Sharepoint;
using Gedoc.WebApp.Models;
using Gedoc.WebApp.Helpers.Maps.Interface;
using Gedoc.Repositorio.Model;
using Gedoc.WebApp.Helpers;

namespace Gedoc.WebApp.Controllers
{
    public class DespachoController : BaseController
    {
        private readonly IGenericMap _mapper;
        private readonly IDespachoService _despachoSrv;
        private readonly IRequerimientoService _requerimientoSrv;
        private readonly IMantenedorService _mantenedorSrv;

        public DespachoController(IGenericMap mapper, IDespachoService despachoSrv, IRequerimientoService requerimientoSrv,
            IMantenedorService mantenedorSrv)
        {
            _mapper = mapper;
            _despachoSrv = despachoSrv;
            _requerimientoSrv = requerimientoSrv;
            _mantenedorSrv = mantenedorSrv;
        }

        public ActionResult NuevoDespacho()
        {
            var model = new DespachoModel();
            ViewBag.Accion = "NUEVO";
            return PartialView("FormDespacho", model);
        }

        public ActionResult EditDespacho()
        {
            var model = new DespachoModel(); // GetDespacho
            ViewBag.Accion = "EDIT";
            return PartialView("FormDespacho", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(DespachoModel model, IEnumerable<HttpPostedFileBase> files)
        {
            var datos = _mapper.MapFromModelToDto<DespachoModel, DespachoDto>(model);
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioCreacionId = CurrentUserId;
            var resultadoOper = _despachoSrv.Save(datos, files);

            return Json(resultadoOper);
        }

        public ActionResult DespachosIngresoForm(int idRequerimiento)
        {
            int userId;
            int.TryParse(Session["IdUsuario"].ToString(), out userId);
            ViewBag.DeleteAccion = UserIsInAction(userId, "DD");

            ViewBag.IdRequerimiento = idRequerimiento;
            return View("GrillaDespachos");
        }

        public ActionResult DespachosIngreso(ParametrosGrillaDto<int> param)
        {
            var datos = _despachoSrv.GetDespachosIngreso(param);

            var jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult AccionDespacho(string idAccion, int idDespacho)
        {
            int[] requerimientos = new int[] { idDespacho };
            return AccionDespachoResult(idAccion, requerimientos, false);
        }

        public ActionResult AccionDespacho2(string idAccion, string requerimientos)
        {
            var reqArray = requerimientos.Split(';').Select(a=> int.Parse(a)).ToArray();
            return AccionDespachoResult(idAccion, reqArray, true);
        }       

        private ActionResult AccionDespachoResult(string idAccion, int[] requerimientos, bool esProcesoMasivo)
        {
            var idDespacho = requerimientos.First();  // TODO: falta precisar por el cliente cual es el requerimiento principal del despacho cdo se crea el despacho desde Procesos Masivos para ás de 1 requerimiento        

            FlujoIngreso formIng = FlujoIngreso.Despacho;
            switch (idAccion)
            {
                case "EDESP": // Editar Despacho
                    formIng = FlujoIngreso.DespachoEdicion;
                    break;
                case "CDESP": // Cerrar Despacho
                    formIng = FlujoIngreso.DespachoCierre;
                    break;
                default:
                    formIng = FlujoIngreso.Despacho;
                    break;
            }
            ViewBag.Accion = idAccion;
            ViewBag.Form = formIng;
            ViewBag.AccesoForm = ValidaAccesoForm();
            var dto = _despachoSrv.GetDespachoById(idAccion == "DE" ? 0 : idDespacho);
            var model = _mapper.MapFromDtoToModel<DespachoDto, DespachoModel>(dto) ??
                        new DespachoModel
                        {
                            AdjuntaDocumentacion = false,
                            EnviarNotificacion = false
                        };
            model.EsProcesoMasivo = esProcesoMasivo;

            if (idAccion == "EDESP")
            {
                var validacionNumOfic = _despachoSrv.ValidaNumeroOficio(model.NumeroDespacho);
                ViewBag.DesdeReservaCorr = validacionNumOfic.Codigo == -2;
            }
            // Se obtienen los datos del requerimiento principal asociado al despacho para mostrar sus datos en el form
            model.RequerimientoPrincipalId = idAccion == "DE" // cdo es nuevo despacho idDespacho tiene el id del requerimiento asociado al despacho
                ? idDespacho
                : model.RequerimientoPrincipalId ?? (model.Requerimiento?.Count > 0 ? model.Requerimiento[0] : 0);
            var idreq = model.RequerimientoPrincipalId.GetValueOrDefault();
            var req = _requerimientoSrv.GetById(idreq) ?? new RequerimientoDto();
            model.RequerimientoMain = _mapper.MapFromDtoToModel<RequerimientoDto, RequerimientoModel>(req);
            model.Requerimiento = idAccion == "DE" 
                ? requerimientos.ToList() // si es nuevo despacho entonces el único ingreso asociado al despaho es al q se le está agregando el despacho
                : model.Requerimiento;
            // En el caso de q el despacho tenga asociado más de un requerimiento entonces se deja de primero el q tenga Id igual a RequerimientoPrincipalId
            if (model.Requerimiento?.Count > 1 && idreq > 0)
            {
                var oldIndex = model.Requerimiento.FindIndex(id => id == idreq);
                model.Requerimiento.RemoveAt(oldIndex);
                model.Requerimiento.Insert(0, idreq);
            }

            // Se graba log (bitácora) de acceso al form
            _mantenedorSrv.SaveLogSistema(new LogSistemaDto
            {
                Accion = "ACCESOFORM",
                EstadoId = req?.EstadoId,
                EtapaId = req?.EtapaId,
                Fecha = DateTime.Now,
                Flujo = formIng.ToString().ToUpper(),
                Origen = "DESPACHO",
                OrigenId = model.Id,
                RequerimientoId = model.Id,
                Usuario = CurrentUserName,
                UsuarioId = CurrentUserId,
                UnidadTecnicaId = req?.UtAsignadaId
            });
            
            return PartialView("FormDespacho", model);
        }
        


        public ActionResult CierreDespacho(int idDespacho)
        {
            FlujoIngreso formIng = FlujoIngreso.DespachoCierre;
            ViewBag.Accion = "CDESP";
            ViewBag.Form = formIng;
            ViewBag.AccesoForm = ValidaAccesoForm();

            var dto = _despachoSrv.GetDespachoById(idDespacho);
            var model = _mapper.MapFromDtoToModel<DespachoDto, DespachoModel>(dto) ??
                        new DespachoModel();

            return PartialView("FormDespachoCierre", model);
        }

        [IgnoreAccessFilter]
        public ActionResult GetArchivo(int idDespacho)
        {
            var datosAdj = _despachoSrv.GetArchivo(idDespacho);

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


        [HttpPost]
        public ActionResult EliminarDespacho(int id)
        {
            var result = new ResultadoOperacion();
            try
            {
                var userid = Session["IdUsuario"]?.ToString();
                if (string.IsNullOrEmpty(userid))
                {
                    result.Codigo = -1;
                    result.Mensaje = "Se perdió la sesión";
                }
                else
                {
                    result = _despachoSrv.MarcaEliminadoDesp(id, int.Parse(userid));
                }
            }
            catch (Exception ex)
            {
                result.Codigo = -1;
                result.Mensaje = "Error al eliminar el Despacho";
                result.Extra = ex.Message;
            }
            return Json(result);
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