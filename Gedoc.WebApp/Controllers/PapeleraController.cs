using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Service.DataAccess;
using Gedoc.WebApp.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.Service.DataAccess.Interfaces;

namespace Gedoc.WebApp.Controllers
{
    public class PapeleraController : Controller
    {
        private readonly IPapeleraService _papeleraSrv;

        public PapeleraController(IPapeleraService papeleraSrv)
        {
            _papeleraSrv = papeleraSrv;
        }
        //[Authorize]
        public ActionResult Index(string idAccion, int idIngreso, int idBandeja)
        {            
            return View();
        }

        public ActionResult PapeleraGetData(ParametrosGrillaDto<int> param)
        {           
            int iduser;
            int.TryParse(Session["IdUsuario"]?.ToString(), out iduser);

            var dataSrv = _papeleraSrv.GetDataPapelera(param, iduser);
            var datos = dataSrv.Data.Select(a =>
                new PapeleraModel()
                {
                    CreadoPor = a.CreadoPor,
                    EliminadoPor = a.EliminadoPor,
                    FechaEliminacion = a.FechaEliminacion,
                    Id = a.Id,
                    Nombre = a.Nombre,
                    OrigenId = a.OrigenId,
                    Tamaño = a.Tamaño,
                    TipoObjetoId = a.TipoObjetoId,
                    UbicacionOriginal = a.UbicacionOriginal
                }).ToList();
            var resultado = new DatosAjax<List<PapeleraModel>>(datos, dataSrv.Resultado);
            resultado.Total = datos.Count;

            JsonResult jsonResult = Json(resultado);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpPost]
        public ActionResult RestoreTrashItems(PapeleraModel[] items)
        {
            var dataSrv = _papeleraSrv.RestoreTrashItems(items.Select(a => new PapeleraItemDto()
            {
                CreadoPor = a.CreadoPor,
                EliminadoPor = a.EliminadoPor,
                FechaEliminacion = a.FechaEliminacion,
                Id = a.Id,
                Nombre = a.Nombre,
                OrigenId = a.OrigenId,
                Tamaño = a.Tamaño,
                TipoObjetoId = a.TipoObjetoId,
                UbicacionOriginal = a.UbicacionOriginal
            }).ToList());

            JsonResult jsonResult = Json(dataSrv);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpPost]
        public ActionResult DeleteTrashItems(PapeleraModel[] items)
        {
            var dataSrv = _papeleraSrv.DeleteTrashItems(items.Select(a => new PapeleraItemDto()
            {
                CreadoPor = a.CreadoPor,
                EliminadoPor = a.EliminadoPor,
                FechaEliminacion = a.FechaEliminacion,
                Id = a.Id,
                Nombre = a.Nombre,
                OrigenId = a.OrigenId,
                Tamaño = a.Tamaño,
                TipoObjetoId = a.TipoObjetoId,
                UbicacionOriginal = a.UbicacionOriginal
            }).ToList());

            JsonResult jsonResult = Json(dataSrv);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpPost]
        public ActionResult EmptyTrash()
        {

            var result = new ResultadoOperacion();            
            int iduser;
            if (int.TryParse(Session["IdUsuario"]?.ToString(), out iduser))
            {
                result = _papeleraSrv.EmptyTrash(iduser);                 
            }
            else
            {
                result.Codigo = -1;
                result.Mensaje = "Se perdio la Sesión";
            }
            return Json(result);
        }
    }
}
