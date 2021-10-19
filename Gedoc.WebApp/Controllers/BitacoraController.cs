using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Repositorio.Model;
using Gedoc.Service.DataAccess;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;
using Gedoc.WebApp.Models;
using Gedoc.WebApp.Helpers.Maps.Interface;

namespace Gedoc.WebApp.Controllers
{
    public class BitacoraController : BaseController
    {
        private readonly IGenericMap _mapper;
        private readonly IBitacoraService _bitacoraSrv;
        private readonly IRequerimientoService _requerimientoService;
        private readonly IMantenedorService _mantService;
        private readonly IDespachoService _despService;

        public BitacoraController(IGenericMap mapper, IBitacoraService bitacoraSrv, 
            IRequerimientoService requerimientoService,
            IMantenedorService mantService,
            IDespachoService despService)
        {
            _mapper = mapper;
            _bitacoraSrv = bitacoraSrv;
            _requerimientoService = requerimientoService;
            _mantService = mantService;
            _despService = despService;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(BitacoraModel model, IEnumerable<HttpPostedFileBase> files)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }
            model.UltimoComentario = model.ObsAcuerdoComentario;
            var datos = _mapper.MapFromModelToDto<BitacoraModel, BitacoraDto>(model);
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioCreacionId = CurrentUserId;
            var resultadoOper = _bitacoraSrv.Save(datos, files);

            return Json(resultadoOper);
        }

        #region Bitacora de requermiento

        public ActionResult BitacorasIngresoForm(int idIngreso, int idBandeja)
        {
            var datosIng = _requerimientoService.GetById(idIngreso);
            ViewBag.IdRequerimiento = idIngreso;
            ViewBag.DocIngreso = datosIng != null ? datosIng.DocumentoIngreso : "";
            ViewBag.AccesoForm = ValidaAccesoForm();

            int userId;
            int.TryParse(Session["IdUsuario"].ToString(), out userId);
            ViewBag.DeleteAccion = UserIsInAction(userId, "DB");
            ViewBag.AccionBit = idBandeja == -5 ? "BIBUSQAV" : "BI";

            return View("GrillaBitacoras");
        }

        public ActionResult BitacorasIngreso(int reqId) 
        {
            var datos = _bitacoraSrv.GetBitacorasDoc(reqId, 'R');

            var jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        #endregion

        #region Bitacora de despacho iniciativas
        public ActionResult BitacorasDespachoForm(int idDesp, int idBandeja)
        {
            var datosDesp = _despService.GetDespachoInicById(idDesp);
            ViewBag.IdDespacho = idDesp;
            ViewBag.NumeroDesp = datosDesp?.NumeroDespacho ?? "";

            int userId;
            int.TryParse(Session["IdUsuario"].ToString(), out userId);
            ViewBag.DeleteAccion = UserIsInAction(userId, "DB");

            return View("GrillaBitacorasDespInic");
        }

        public ActionResult BitacorasDespacho(int idDesp)
        {
            var datos = _bitacoraSrv.GetBitacorasDoc(idDesp, 'D');

            var jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        #endregion

        public ActionResult AccionBitacora(string idAccion, int idBitacora, int idDoc)
        {
            ViewBag.Accion = idAccion;
            var model = new BitacoraModel();
            ViewBag.AccesoForm = ValidaAccesoForm();
            if (idAccion == "BI" || idAccion == "BIBUSQAV")
            {
                // Bitácoras de requermiento
                var datosIng = _requerimientoService.GetById(idDoc);
                var dto = _bitacoraSrv.GetBitacoraById(idBitacora);
                var tipoHistorial = _mantService.GetGenericoMatenedor(Mantenedor.TipoBitacora).Data
                    .FirstOrDefault(t => t.Titulo == "Historial");
                model = _mapper.MapFromDtoToModel<BitacoraDto, BitacoraModel>(dto) ??
                        new BitacoraModel
                        {
                            TipoBitacoraCod = tipoHistorial?.Id,
                            RequerimientoId = idDoc
                        };
                model.DocIngreso = datosIng?.DocumentoIngreso ?? "";
                return View("FormBitacora", model);
            }
            else
            {
                // Bitácora de Despacho iniciativas
                var datosDesp = _despService.GetDespachoInicById(idDoc);
                var dto = _bitacoraSrv.GetBitacoraById(idBitacora);
                var tipoDefault = _mantService.GetGenericoMatenedor(Mantenedor.TipoBitacora).Data
                    .FirstOrDefault(t => t.Id == TipoBitacora.AcuerdoComision.ToString("D"));
                model = _mapper.MapFromDtoToModel<BitacoraDto, BitacoraModel>(dto) ??
                        new BitacoraModel
                        {
                            TipoBitacoraCod = tipoDefault?.Id,
                            DespachoInicId = idDoc
                        };
                model.NumeroDesp = datosDesp?.NumeroDespacho ?? "";
                return View("FormBitacoraDespInic", model);
            }
        }

        [IgnoreAccessFilter]
        public ActionResult GetArchivo(int idBitacora)
        {
            var datosAdj = _bitacoraSrv.GetArchivo(idBitacora);

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

        public ActionResult FechasDisponibles(int tipoBitacoraId)
        {
            ViewBag.TipoBit = tipoBitacoraId;
            return View();
        }

        public ActionResult UltimoComentario(string tipoBitacoraId, int reqId)
        {
            var datos = _bitacoraSrv.GetUltimoComentarioByTipo(tipoBitacoraId, reqId);
            return Json(datos);
        }

        public ActionResult CalendarioBitacoraByTipo(string tipoBitacoraId, int anno)
        {
            var datos = _mantService.GetCalendarioBitacoraByTipo(tipoBitacoraId, anno);

            var jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult CalendarioBitacoraAnnos(string tipoBitacoraId, bool? checkAnnoActual)
        {
            var datos = _mantService.GetCalendarioBitacoraAnnos(tipoBitacoraId);
            #region Para form de Calendario Bitácoras
            if (checkAnnoActual.GetValueOrDefault(false))
            {
                var annos = datos.Data;
                // Si el año actual no aparece disponible entonces se agrega
                if (!annos.Any(a => a.IdInt == DateTime.Now.Year))
                {
                    annos.Add(new GenericoDto { IdInt = DateTime.Now.Year, Titulo= DateTime.Now.Year.ToString()} );
                }

                // Si la fecha de hoy es del segundo semestre del año entonces aparece disponible en calendario bitacoras el próximo año
                var refDate = new DateTime(DateTime.Today.Year, 6, 1);
                var proxAnno = DateTime.Compare(DateTime.Today, refDate) >= 0 ? DateTime.Today.AddYears(1).Year : 0;
                if (proxAnno > 0 && !annos.Any(a => a.IdInt == proxAnno))
                {
                    annos.Add(new GenericoDto { IdInt = proxAnno, Titulo = proxAnno.ToString() });
                }
            }
            #endregion
            return Json(datos);
        }

        [HttpPost]
        public ActionResult BitacoraEliminar(int id)
        {
            var result = new ResultadoOperacion();
            var userid = Session["IdUsuario"]?.ToString();
            if (!string.IsNullOrEmpty(userid))
            {
                result = _bitacoraSrv.EliminarBitacora(id, int.Parse(userid));
            }
            else
            {
                result.Codigo = -1;
                result.Mensaje = "Se perdió la sesión";
            }
            return Json(result);
        }

        #region Mantenedor de Calendario Bitácora
        public ActionResult Calendario()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveCalendario(CalendarioBitacoraModel model)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }
            var datos = _mapper.MapFromModelToDto<CalendarioBitacoraModel, CalendarioBitacoraDto>(model);
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioCreacionId = CurrentUserId;
            var resultadoOper = _mantService.SaveCalendarioBitacora(datos);

            return Json(resultadoOper);
        }

        [HttpPost]
        public ActionResult EliminaCalendario(int calendarioId)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }
            var resultadoOper = _mantService.DeleteCalendarioBitacora(calendarioId);

            return Json(resultadoOper);
        }

        public ActionResult FormCalendario(int calendarioId, int? anno, string tipoBitacoraId, string tipoBitacoraTitulo)
        {
            ViewBag.AccesoForm = ValidaAccesoCalendarioForm();
            var calendario = new CalendarioBitacoraDto()
            {
                TipoBitacoraCod = tipoBitacoraId,
                TipoBitacoraTitulo = tipoBitacoraTitulo
            };
            if (calendarioId > 0)
            {
                calendario = _mantService.GetCalendarioBitacoraById(calendarioId);
            }
            var model = _mapper.MapFromDtoToModel<CalendarioBitacoraDto, CalendarioBitacoraModel>(calendario);
            model.Anno = model.Anno ?? anno;
            return View(model);
        }
        #endregion

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

        private ResultadoOperacion ValidaAccesoCalendarioForm()
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