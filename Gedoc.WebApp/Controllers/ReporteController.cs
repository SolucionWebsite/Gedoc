﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Gedoc.Repositorio.Model;
 using Gedoc.Service.DataAccess.Interfaces;
 using Gedoc.WebApp.Helpers;

 namespace Gedoc.WebApp.Controllers
{
    public class ReporteController : BaseController
    {
        private GedocEntities db = new GedocEntities();
        private readonly IMantenedorService _mantenedorSrv;

        public ReporteController(IMantenedorService mantenedorSrv)
        {
            _mantenedorSrv = mantenedorSrv;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Reporte_Read([DataSourceRequest]DataSourceRequest request)
        {
            var rep = _mantenedorSrv.GetReporteAll().Where(r => r.Id != 14);
            //IQueryable<Reporte> reporte = db.Reporte;
            //DataSourceResult result = reporte.ToDataSourceResult(request, reporte1 => new {
            //    Id = reporte1.Id,
            //    Nombre = reporte1.Nombre,
            //    Descripcion = reporte1.Descripcion,
            //    NombreReporte = reporte1.NombreReporte
            //});
            var result = rep.ToDataSourceResult(request);

            return Json(result);
        }

        public ActionResult EjecutarReporte(int id, int? sesion, int? ut)
        {
            var reporte = _mantenedorSrv.GetReporteById(id);
            if (reporte == null)
            {
                return RedirectToAction("ErrorAnonimo", "Home", new { mensaje = "El reporte especificado no se enuentra en la aplicación." });
            }

            // Si es reporte de Marcar Ver en Tabla entonces se lanza el proceso de cargar de datos del servicio ETL, pero sólo para los datos de Marcar Ver en Tabla (datos de Sesión y Comisión)
            if (reporte.NombreReporte == "Reporte_Tabla_de_Comision.rdl")
            {
                var resultado = EtlReporteHelper.ProcesaPeticionSrvEtlSelectivo(new []{"SES"});
                ViewBag.ResultadoCargaEtl = resultado;
            }

            ViewBag.sesion = sesion; //.GetValueOrDefault(0);
            ViewBag.ut = ut; //.GetValueOrDefault(0);

            return View(reporte);
        }

        [HttpPost]
        public ActionResult Excel_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }

        [HttpGet]
        public ActionResult ProcesaEtl(int opcion)
        {
            var resultado = EtlReporteHelper.ProcesaPeticionSrvEtl(opcion);

            return Json(resultado, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
