using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;
using Gedoc.WebApp.Helpers.Maps.Interface;
using Gedoc.WebApp.Models;
using Kendo.Mvc.Export;
using Kendo.Mvc.Infrastructure;

namespace Gedoc.WebApp.Controllers
{
    public class OficioController : BaseController
    {
        private readonly IDespachoService _despachoSrv;
        private readonly IMantenedorService _mantenedorSrv;
        private readonly IOficioService _oficioSrv;
        private readonly IAdjuntoService _adjuntoSrv;
        private readonly IGenericMap _mapper;

        public OficioController(
            IDespachoService despachoSrv,
            IMantenedorService mantenedorSrv,
            IOficioService oficioSrv,
            IAdjuntoService adjuntoSrv,
            IGenericMap mapper)
        {
            _despachoSrv = despachoSrv;
            _mantenedorSrv = mantenedorSrv;
            _adjuntoSrv = adjuntoSrv;
            _oficioSrv = oficioSrv;
            _mapper = mapper;
        }

        #region Plantilla Oficio

        [Route("plantillaoficio")]
        public ActionResult IndexPlantillaOficio()
        {
            return View("IndexPlantillaOficio");
        }

        public ActionResult GetPlantillaOficioAll()
        {
            var datos = _oficioSrv.GetPlantillaOficioAll();
            return Json(datos);
        }

        public ActionResult GetContenidoPlantillaConDatosById(int id, List<int> reqIds, int? reqMainId)
        {
            var resultado = _oficioSrv.GetPlantillaConDatos(id, reqIds, reqMainId);
            var oficio = new OficioDto() { Contenido = resultado.Extra.ToString()};
            _oficioSrv.SeparaEncabezadoPiePagina(oficio);
            resultado.Extra = new
            {
                Contenido = oficio.Contenido,
                Encabezado = oficio.Encabezado,
                Pie = oficio.Pie
            };
            return Json(resultado);
        }

        public ActionResult GetContenidoOficioActualizado(int oficioId, string contenido, List<int> reqIds, int? reqMainId)
        {
            // Se obtiene el contenido del oficio pero actualizando los valores q tiene del requerimiento principal 
            var resultado = _oficioSrv.GetContenidoOficioActualizado(oficioId, contenido, reqIds, reqMainId.GetValueOrDefault(0));

            return Json(resultado);
        }

        public ActionResult GetDatosRequerimientoMain(List<int> reqIds)
        {
            var reqMain = _oficioSrv.GetDatosRequerimientoMainOficio(0, reqIds, false);
            return Json(reqMain);
        }

        [HttpPost]
        public ActionResult DatosPlantilla(int id)
        {
            if (id <= 0)
            {
                return Json(false);
            }
            
            var plantilla = _oficioSrv.GetPlantillaOficioById(id);
            var plantillaModel = _mapper.MapFromDtoToModel<PlantillaOficioDto, PlantillaOficioModel>(plantilla);

            bool result = plantillaModel != null && plantilla.TipoWord;

            return Json(result);
        }

        public ActionResult FormPlantillaOficio(int id, int tipoWord)
        {
            ViewBag.AccesoForm = ValidaAccesoUtForm();
            var tipoPlant = (int) TipoPlantillaOficio.Despacho;
            var datos = new PlantillaOficioDto
            {
                Activo = true
            };
            if (id > 0)
            {
                datos = _oficioSrv.GetPlantillaOficioById(id);
                tipoPlant = datos.TipoPlantillaId.GetValueOrDefault((int)TipoPlantillaOficio.Despacho);
            }
            ViewBag.CamposSeleccionables = _oficioSrv.GetCamposSeleccionablePorGrupos(tipoPlant);
            var model = _mapper.MapFromDtoToModel<PlantillaOficioDto, PlantillaOficioModel>(datos);
            model.TipoWord = tipoWord == 1;

            if (model.TipoWord)
            {
                string url = "Adjuntos\\Adjuntos de Plantilla Oficio\\" + datos.NombreDocumento;
                model.Documento = _adjuntoSrv.GetArchivo(url);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult SaveActivo(PlantillaOficioModel model)
        {
            var datos = _mapper.MapFromModelToDto<PlantillaOficioModel, PlantillaOficioDto>(model);
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioActualId = CurrentUserId.GetValueOrDefault(0);
            datos.UsuarioCreacionId = CurrentUserId;
            datos.UsuarioModificacionId = CurrentUserId;
            var resultadoOper = _oficioSrv.SavePlantillaOficio(datos, true);

            return Json(resultadoOper);
        }

        [HttpPost]
        public ActionResult SavePlantilla(PlantillaOficioModel model)
        {
            var datos = _mapper.MapFromModelToDto<PlantillaOficioModel, PlantillaOficioDto>(model);
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioActualId = CurrentUserId.GetValueOrDefault(0);
            datos.UsuarioCreacionId = CurrentUserId;
            datos.Contenido = WebUtility.HtmlDecode(datos.Contenido);
            var resultadoOper = _oficioSrv.SavePlantillaOficio(datos, false);

            return Json(resultadoOper);
        }

        [HttpPost]
        public ActionResult SavePlantillaWord(PlantillaOficioModel model, AdjuntoModel adjuntoModel, IEnumerable<HttpPostedFileBase> files)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }

            _ = new ResultadoOperacion();
            var resultadoSubidaDocumento = new ResultadoOperacion();

            if (model.Id != 0 && files.FirstOrDefault() != null || (adjuntoModel.Id == 0 && files.FirstOrDefault() != null))
            {
                adjuntoModel.DocIngreso = "Plantilla Oficio";
                AdjuntoDto datos = _mapper.MapFromModelToDto<AdjuntoModel, AdjuntoDto>(adjuntoModel);
                datos.CreadoPor = CurrentUserName;
                datos.UsuarioActual = CurrentUserName;
                datos.UsuarioCreacionId = CurrentUserId;
                datos.DatosArchivo = new DatosArchivo
                {
                    TipoArchivo = TiposArchivo.AdjuntoWord
                };
                datos.Id = 0;
                resultadoSubidaDocumento = _adjuntoSrv.Save(datos, files);
            }

            ResultadoOperacion resultadoPlantilla;
            if (resultadoSubidaDocumento.Codigo == 1)
            {
                var datosPlantilla = _mapper.MapFromModelToDto<PlantillaOficioModel, PlantillaOficioDto>(model);
                datosPlantilla.UsuarioActual = CurrentUserName;
                datosPlantilla.UsuarioActualId = CurrentUserId.GetValueOrDefault(0);
                datosPlantilla.UsuarioCreacionId = CurrentUserId;
                datosPlantilla.Contenido = WebUtility.HtmlDecode(datosPlantilla.Contenido);
                datosPlantilla.NombreDocumento = files.FirstOrDefault() == null ? _oficioSrv.GetPlantillaOficioById(model.Id).NombreDocumento : files.FirstOrDefault().FileName;
                resultadoPlantilla = _oficioSrv.SavePlantillaOficio(datosPlantilla, false);
            }
            else
            {
                return Json(resultadoSubidaDocumento);
            }

            return Json(resultadoPlantilla);
        }

        [HttpPost]
        public ActionResult EliminaPlantilla(int id)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }
            var resultadoOper = _oficioSrv.DeletePlantilla(id);

            return Json(resultadoOper);
        }

        [HttpPost]
        public ActionResult MarcaEliminaPlantilla(int id)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }

            var datosUsuario = GetDatosUsuarioActual();
            var resultadoOper = _oficioSrv.MarcaDeletePlantilla(id, datosUsuario);

            return Json(resultadoOper);
        }

        //private List<CampoSeleccionableDto> GetCamposSeleccionables()
        //{
        //    var datos = _oficioSrv.GetCamposSeleccionablePorGrupos();

        //    return datos;
        //}

        #endregion

        private ResultadoOperacion ValidaAccesoUtForm()
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

        #region Exportación/Importación del editor de plantilla

        [IgnoreAccessFilter]
        [HttpPost]
        public ActionResult Pdf_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }

        [IgnoreAccessFilter]
        [HttpPost]
        public ActionResult ExportEditor(EditorExportData data)
        {
            var settings = new EditorDocumentsSettings();
            settings.HtmlImportSettings.LoadFromUri += HtmlImportSettings_LoadFromUri;
            try
            {
                return EditorExport.Export(data, settings);
            }
            catch
            {
                return null;
                //return RedirectToAction("exportas", "editor");
            }
        }

        private void HtmlImportSettings_LoadFromUri(object sender, Telerik.Windows.Documents.Flow.FormatProviders.Html.LoadFromUriEventArgs e)
        {
            var uri = e.Uri;
            var absoluteUrl = uri.StartsWith("http://") || uri.StartsWith("www.");
            if (!absoluteUrl)
            {

                var filePath = Server.MapPath(uri);
                using (var fileStream = System.IO.File.OpenRead(filePath))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        e.SetData(memoryStream.ToArray());
                    }
                }
            }
        }

        [IgnoreAccessFilter]
        public ActionResult ImportEditor(HttpPostedFileBase file)
        {
            var settings = new EditorImportSettings();
            string htmlResult;
            switch (Path.GetExtension(file.FileName))
            {
                case ".docx":
                    htmlResult = EditorImport.ToDocxImportResult(file, settings);
                    break;
                case ".rtf":
                    htmlResult = EditorImport.ToRtfImportResult(file, settings);
                    break;
                default:
                    htmlResult = EditorImport.GetTextContent(file);
                    break;
            }

            return Json(new { html = htmlResult });
        }
        #endregion

        #region Grilla y formularios de Oficio
        public ActionResult DatosBandejaEntradaOfic(ParametrosGrillaDto<int> param)
        {
            /* Filtros aplicados en las columnas de la grilla*/
            var filterSqlParams = new List<object>();
            var filterSql = KendoHelper.FiltersToParameterizedQuery(FilterDescriptorFactory.Create(param.Filter), paramValues: filterSqlParams);
            param.Filter = filterSql;
            param.FilterParameters = filterSqlParams.ToArray();

            var datos = _oficioSrv.GetDatosBandejaOficio(param, CurrentUserId.GetValueOrDefault()); //idBandeja);

            if (datos != null && datos.Data.Count > 0)
            {
                foreach (var dato in datos.Data)
                {
                    if (dato.PlantillaId > 0)
                    {
                        var plantilla = _oficioSrv.GetPlantillaOficioById(dato.PlantillaId.GetValueOrDefault());

                        if (plantilla != null && plantilla.NombreDocumento != null)
                        {
                            dato.NombreDocumento = plantilla.NombreDocumento;
                            dato.TipoWord = true;
                            string url = "Adjuntos\\Adjuntos de Plantilla Oficio\\" + dato.NombreDocumento;
                            dato.IdAdjunto = _adjuntoSrv.GetArchivo(url).OrigenId;
                        }
                    }
                }
            }

            var jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult GrillaOficioObservaciones(int oficioId)
        {
            ViewBag.OficioId = oficioId;
            return View();
        }

        public ActionResult ObservacionesOficio(int oficioId)
        {
            var datos = _oficioSrv.GetObservacionesOficio(oficioId);

            var jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult FormSeleccionarPlantillaOficio()
        {
            ViewBag.AccesoForm = ValidaAccesoUtForm();
            ViewBag.UsuarioId = EsAdmin() ? -1 : CurrentUserId;
            return View();
        }

        public ActionResult FormOficio(int id, int? tramId, int? plantId, string reqsId, int? utId)
        {
            ViewBag.AccesoForm = ValidaAccesoUtForm();
            //ViewBag.CamposSeleccionables = _oficioSrv.GetCamposSeleccionablePorGrupos();
            var oficio = new OficioDto();
            if (id > 0)
            {
                oficio = _oficioSrv.GetOficoById(id, true);
                if (oficio.EstadoId == (int) EstadoOficio.Firmado)
                {
                    ViewBag.AccesoForm = new ResultadoOperacion(-1, "El oficio se encuentra en estado Firmado.", null);
                }
            } else
            { // Nuevo Oficio
                oficio.PlantillaId = plantId;
                var datosPlant = _oficioSrv.GetPlantillaOficioById(plantId.GetValueOrDefault(0)); // Para obtener el Tipo de Plantilla
                oficio.TipoPlantillaId = datosPlant.TipoPlantillaId;
                // Requerimientos asociados al oficio, si es de Despacho
                var reqsIdArr = string.IsNullOrWhiteSpace(reqsId) ? new List<string>() : reqsId.Split(';').ToList();
                var reqs = reqsIdArr.Select(r => new GenericoDto() { IdInt = Convert.ToInt32(r), Titulo = ""}).ToList();
                oficio.Requerimiento = reqs;
                // Unidad Técnica asociada al oficio, si es de Despacho Iniciativa
                oficio.UnidadTecnicaId = utId;
                // Plantilla del oficio
                var resultado = _oficioSrv.GetPlantillaConDatos(plantId.GetValueOrDefault(0), reqs.Select(r => r.IdInt).ToList(), null);
                oficio.Contenido = resultado.Codigo > 0 ? resultado.Extra.ToString() : "";
                _oficioSrv.SeparaEncabezadoPiePagina(oficio);
            }
            var model = _mapper.MapFromDtoToModel<OficioDto, OficioModel>(oficio);
            ViewBag.UsuarioId = EsAdmin() ? -1 : CurrentUserId;
            return View(model);
        }

        public ActionResult FormOficioPdf(int id)
        {
            ViewBag.AccesoForm = ValidaAccesoUtForm();

            var oficio = _oficioSrv.GetOficoById(id, true);
            var model = _mapper.MapFromDtoToModel<OficioDto, OficioModel>(oficio);
            return View(model);
        }

        /// <summary>
        /// Obsoleto. Se utilizaba para editar el Número de Oficio y Fecha de Emisión de Oficio de un oficio firmado. Y para
        /// enviar a firma un oficio no firmado.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="accion"></param>
        /// <returns></returns>
        public ActionResult FormOficioEdicionFirmado(int id, string accion)
        {
            ViewBag.AccesoForm = ValidaAccesoUtForm();
            var oficio = _oficioSrv.GetOficoById(id, true);
            if (oficio.EstadoId != (int)EstadoOficio.AprobadoVisadorGen && oficio.EstadoId != (int)EstadoOficio.Firmado)
            {
                ViewBag.AccesoForm = new ResultadoOperacion(-1, "El oficio no se encuentra en estado Aprobado Visador General.", null);
            }
            var model = _mapper.MapFromDtoToModel<OficioDto, OficioModel>(oficio);
            model.Accion = accion;
            return View(model);
        }

        public ActionResult GetCamposSeleccionablesTipoPlantilla(int tipoPlant, int? id)
        {
            var datos = _oficioSrv.GetCamposSeleccionableByPadre(id,tipoPlant);
            var result = TransformDsDropDownTree(datos);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private List<object> TransformDsDropDownTree(List<CampoSeleccionableDto> items)
        {
            if (items == null || items.Count == 0)
                return new List<object>();

            var result = items.Select(item => new
            {
                id = item.Id.ToString(),
                text = item.Titulo,
                value = item.Variable, // Html.Raw(item.Variable).ToHtmlString(),
                hasChildren = item.HasChildren,
                spriteCssClass = "",
                url = "",
                expanded = true,
                padreId = item.PadreId,
                items = TransformDsDropDownTree(item.Hijos) 
            }).ToList<object>();

            return result;
        }

        [HttpPost]
        public ActionResult SaveOficio(OficioModel model)
        {
            /* Para las acciones de Nuevo Oficio y Editar Oficio. El resto de las acciones se graban en EjecutaAccionOficio */
            var datos = _mapper.MapFromModelToDto<OficioModel, OficioDto>(model);
            datos.Contenido = WebUtility.HtmlDecode(datos.Contenido);
            datos.Flujo = FlujoIngreso.Oficio;
            datos.DatosUsuarioActual = GetDatosUsuarioActual();
            //// Obsoleto, se utilizaba cuando se firmaba el oficio desde la ventana de Editar Oficio Firmado: Se decodifica el PDF q está en Base64
            //var pdfBase64 = string.IsNullOrWhiteSpace(model.PdfBase64) ? "" : model.PdfBase64.Substring(model.PdfBase64.IndexOf("base64") + 7);  // se elimina data:application/pdf;base64,
            byte[] pdfBytes = new byte[0]; // string.IsNullOrWhiteSpace(model.PdfBase64) ? new byte[0] : Convert.FromBase64String(pdfBase64);
            
            var resultadoOper = _oficioSrv.SaveOficio(datos, pdfBytes);

            return Json(resultadoOper);
        }

        [HttpPost]
        public JsonResult EjecutaAccionOficio(string accion, OficioModel model)
        {
            var datos = _mapper.MapFromModelToDto<OficioModel, OficioDto>(model);
            datos.Flujo = model.Flujo == 0 ? FlujoIngreso.Oficio : model.Flujo;
            datos.DatosUsuarioActual = GetDatosUsuarioActual();
            datos.Accion = accion;
            datos.BaseUrl = BaseUrl;
            var resultadoOper = _oficioSrv.SaveOficio(datos, null);

            return Json(resultadoOper);
        }

        /// <summary>
        /// Devuelve el archivo PDF del oficio q se encuentra almacenado en el repositorio de archivos de la aplicación.<br/>
        /// Se utiliza para descargar el archivo PDF del oficio después que se encuentra firmado y su PDF almacenado
        /// en el repositorio de archivos.
        /// </summary>
        /// <param name="oficioId">Id de Oficio</param>
        /// <returns>FileResult con el PDF del oficio</returns>
        [IgnoreAccessFilter]
        public ActionResult GetArchivoOficio(int oficioId)
        {
            var datosAdj = _oficioSrv.GetArchivo(oficioId);

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

        /// <summary>
        /// Devuelve el archivo PDF del oficio, en base al contenido del oficio. Convierte el contenido Html del oficio
        /// a PDF y lo devuelve al navegador para descargarlo.<br/>Se utiliza en la visualización de oficios q no han sido firmados.
        /// </summary>
        /// <param name="oficioId">Id de Oficio</param>
        /// <returns>FileResult con el PDF del oficio</returns>
        [IgnoreAccessFilter]
        public ActionResult GetOficioPdf(int oficioId)
        {

            byte[] pdfBuffer = _oficioSrv.GetOficioPdfFromHtmlById(oficioId, BaseUrl);

            FileResult fileResult = File(pdfBuffer,
                System.Net.Mime.MediaTypeNames.Application.Octet,
                $"Oficio{oficioId}.pdf");

            return fileResult;
            //var fileResult = new FileContentResult(pdfBuffer, "application/pdf");
            //Response.AppendHeader("Content-Disposition", " filename=" + $"Oficio{oficioId}.pdf");
            //return fileResult;
        }

        /// <summary>
        /// Devuelve un archivo PDF en base al contenido Html especificado.<br/>
        /// Se utiliza para generar el PDF de una plantilla de oficio o de un oficio
        /// desde el editor de texto enriquecido.
        /// </summary>
        /// <param name="contenido">Contenido Html a convertir a PDF</param>
        /// <returns>FileResult con el PDF del oficio</returns>
        [IgnoreAccessFilter]
        public ActionResult GetPdfFromContenidoHtml(string contenido, string nombreArchivo)
        {
            contenido = System.Net.WebUtility.HtmlDecode(contenido);
            byte[] pdfBuffer = _oficioSrv.GetPdfFromHtml(contenido, BaseUrl);

            FileResult fileResult = File(pdfBuffer,
                System.Net.Mime.MediaTypeNames.Application.Octet,
                string.IsNullOrWhiteSpace(nombreArchivo) ? "Documento.pdf" : nombreArchivo);

            return fileResult;
            //var fileResult = new FileContentResult(pdfBuffer, "application/pdf");
            //Response.AppendHeader("Content-Disposition", " filename=" + $"Oficio{oficioId}.pdf");
            //return fileResult;
        }
        #endregion

        #region Reserva de correlativo

        [Route("reservacorrelativo")]
        public ActionResult IndexReservaCorrelativo()
        {
            return View("IndexReservaCorrelativo");
        }

        public ActionResult GetReservaCorrelativoAll()
        {
            var datos = _mantenedorSrv.GetReservaCorrelativoAll();
            return Json(datos);
        }

        [HttpPost]
        public ActionResult ReservarCorrelativoOficio(string observaciones)
        {
            var resultadoOper = _mantenedorSrv.ReservarCorrelativoOficio(CurrentUserId.GetValueOrDefault(), observaciones);

            return Json(resultadoOper);
        }
        #endregion


        [HttpPost]
        public ActionResult ActualizarOficiosFirmadosDig()
        {
            var result = new ResultadoOperacion();
            try
            {
                var datosUsuario = GetDatosUsuarioActual();
                if (datosUsuario.UsuarioId <= 0)
                {
                    result.Codigo = -1;
                    result.Mensaje = "Se perdió la sesión";
                }
                else
                {
                    result = _oficioSrv.UpdateOficiosPendienteFirma(datosUsuario);
                }
            }
            catch (Exception ex)
            {
                result.Codigo = -1;
                result.Mensaje = "Error al actualizar los datos.";
                result.Extra = ex.Message;
            }
            return Json(result);
        }
    }
}