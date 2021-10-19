using Gedoc.Helpers;
using Gedoc.Repositorio.Model;
using Gedoc.WebApp.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Service.DataAccess.Interfaces;

namespace Gedoc.WebApp.Controllers
{
    public class ArchivarRequerimientoController : BaseController
    {

        private readonly IRequerimientoService _requerimientoSrv;

        public ArchivarRequerimientoController(IRequerimientoService requerimientoSrv)
        {
            _requerimientoSrv = requerimientoSrv;
        }

        // GET: ArchivarRequerimiento
        [Authorize]
        public ActionResult Index(int id)
        {
            ViewBag.TipoBusqueda = id;

            if ((ArchivarEnum)id == ArchivarEnum.Archivar)
                ViewBag.TipoForm = "Archivar";
            else if ((ArchivarEnum)id == ArchivarEnum.Restaurar)
                ViewBag.TipoForm = "Restuarar";
            else
                return RedirectToAction("ErrorAnonimo", "Home", new { mensaje = "Acción no válida" });

            var unidadTecnicaList = new List<DropDownListItem>
            {
                new DropDownListItem() { Text = "(Todas las Unidades)", Value = "-1" , Selected = true}
            };
            using (var db = new GedocEntities())
            {
                unidadTecnicaList.AddRange(db.UnidadTecnica.Where(a => a.Activo == true)
                    .Select(a => new DropDownListItem()
                    {
                        Text = a.Titulo,
                        Value = a.Id.ToString()
                    }).ToList());
            }
            ViewBag.UnidadTecnicaList = unidadTecnicaList;

            return View();
        }

        //public ActionResult GetDataGrilla([DataSourceRequest]DataSourceRequest request, ArchivarRequerimientoModel extraData)
        //{
        //    try
        //    {
        //        extraData.FechaHasta = extraData.FechaHasta.AddHours(23).AddMinutes(59).AddSeconds(59);
        //        using (var db = new GedocEntities())
        //        {
        //            var preResult = db.Requerimiento.Where(a => a.Eliminado == false
        //                && a.FechaIngreso >= extraData.FechaDesde
        //                && a.FechaIngreso <= extraData.FechaHasta);

        //            if (extraData.UnidadTecnicaId != 0 && extraData.UnidadTecnicaId != -1)
        //                preResult = preResult.Where(a => a.UtAsignadaId == extraData.UnidadTecnicaId);

        //            if (extraData.TipoBusqueda == ArchivarEnum.Archivar)
        //                preResult = preResult.Where(a => a.EstadoId == (int)EstadoIngreso.Cerrado);
        //            else if (extraData.TipoBusqueda == ArchivarEnum.Restaurar)
        //                preResult = preResult.Where(a => a.EstadoId == (int)EstadoIngreso.Archivado);

        //            var result = preResult.Select(a => new RequerimientoModel()
        //            {
        //                Id = a.Id,
        //                DocumentoIngreso = a.DocumentoIngreso,
        //                FechaIngreso = a.FechaIngreso,
        //                UtAsignadaTitulo = a.UnidadTecnicaAsign.Titulo,
        //                RequiereAcuerdo = a.RequiereAcuerdo,
        //                RequiereRespuesta = a.RequiereRespuesta,
        //                //FechaUltAcuerdoComision = a.FechaUltAcuerdoComision,
        //                //FechaUltAcuerdoSesion = a.FechaUltAcuerdoSesion,
        //                FechaUltAcuerdoComision = a.Bitacora.Where(b => b.TipoBitacoraCod == TipoBitacora.AcuerdoComision.ToString("D")).OrderByDescending(c => c.Fecha).FirstOrDefault().Fecha,
        //                FechaUltAcuerdoSesion = a.Bitacora.Where(b => b.TipoBitacoraCod == TipoBitacora.AcuerdoSesion.ToString("D")).OrderByDescending(c => c.Fecha).FirstOrDefault().Fecha,
        //                EstadoTitulo = a.EstadoRequerimiento.Titulo,
        //            });
        //            return Json(result.ToDataSourceResult(request));
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return Json(null);
        //    }
        //}

        public ActionResult GetDataGrilla(ParametrosGrillaDto<int> param, ArchivarRequerimientoModel extraData)
        {
            
            var datos = _requerimientoSrv.GetDatosBusquedaArchivar(param, extraData.FechaDesde, extraData.FechaHasta, extraData.UnidadTecnicaId, (int)extraData.TipoBusqueda);

            var jsonResult = Json(datos);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        [HttpPost]
        public ActionResult Archivar(string[] ids)
        {
            var result = new ResultadoOperacion();
            try
            {
                using(var db = new GedocEntities())
                {
                    var reqids = ids.Select(a => int.Parse(a));
                    var reqUpdate = db.Requerimiento.Where(a => reqids.Contains(a.Id));
                    foreach (var req in reqUpdate)
                    {
                        //14  Cerrado
                        //15  Archivado 
                        req.EstadoId = 15;
                    }
                    db.SaveChanges();
                    result.Codigo = 1;
                    result.Mensaje = "Requerimientos Archivados.";
                }
            }
            catch (Exception ex)
            {
                result.Codigo = -1;
                result.Mensaje = "Error al cambiar estado Archivado.";
                result.Extra = ex.Message;
            }
            return Json(result); 
        }

        [HttpPost]
        public ActionResult Restaurar(string[] ids)
        {
            var result = new ResultadoOperacion();
            try
            {
                using (var db = new GedocEntities())
                {
                    var reqids = ids.Select(a => int.Parse(a));
                    var reqUpdate = db.Requerimiento.Where(a => reqids.Contains(a.Id));
                    foreach (var req in reqUpdate)
                    {
                        //14  Cerrado
                        //15  Archivado 
                        req.EstadoId = 14;
                    }
                    db.SaveChanges();
                    result.Codigo = 1;
                    result.Mensaje = "Requerimientos Restaurados.";
                }
            }
            catch (Exception ex)
            {
                result.Codigo = -1;
                result.Mensaje = "Error al cambiar estado Archivado.";
                result.Extra = ex.Message;
            }
            return Json(result);
        }
    }
}
