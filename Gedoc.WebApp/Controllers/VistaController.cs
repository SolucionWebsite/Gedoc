using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Helpers.Logging;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.Infrastructure;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using WebGrease.Css.Extensions;

namespace Gedoc.WebApp.Controllers
{
    public class VistaController : BaseController
    {
        private readonly IRequerimientoService _requerimientoSrv;
        private readonly IDespachoService _despachoSrv;
        private readonly IMantenedorService _mantenedorSrv;

        public VistaController(IRequerimientoService requerimientoSrv, IDespachoService despachoSrv, IMantenedorService mantenedorSrv)
        {
            this._requerimientoSrv = requerimientoSrv;
            this._despachoSrv = despachoSrv;
            this._mantenedorSrv = mantenedorSrv;
        }

        public ActionResult Index(int? id)
        {
            var estados = _mantenedorSrv.GetGenericoMatenedor(Mantenedor.EstadoRequerimiento);
            ViewBag.Estados = estados.Data.Where(e => e.IdInt != ((int)EstadoIngreso.Archivado)).Select(d => new { EstadoTitulo = d.Titulo }).ToList();

            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            ViewBag.IdVista = id.Value;
            return View();
        }

        public ActionResult DatosVista(ParametrosGrillaDto<int> param) // [DataSourceRequest] DataSourceRequest request) // 
        {
            JsonResult jsonResult;
            jsonResult = param != null && param.Dato != 4
                ? Json(_requerimientoSrv.GetDatosVistas(param))
                : Json(_despachoSrv.GetDatosVistas(param));

            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult DatosVistaGenero(ParametrosGrillaDto<int> param) // ([DataSourceRequest] DataSourceRequest request)
        {
            /* Filtros aplicados en las columnas de la grilla*/
            var filterSqlParams = new List<object>();
            var filterSql = KendoHelper.FiltersToParameterizedQuery(FilterDescriptorFactory.Create(param.Filter), paramValues: filterSqlParams);
            param.Filter = filterSql;
            param.FilterParameters = filterSqlParams.ToArray();

            JsonResult jsonResult;
            jsonResult = Json(_requerimientoSrv.GetDatosVistaGenero(param));

            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
            //}
        }


        #region Exportación Excel
        [IgnoreAccessFilter]
        public ActionResult ExcelVistaGenero(string filter)
        {
            try
            {
                /* Filtros aplicados en las columnas de la grilla*/
                var filterSqlParams = new List<object>();
                var filterSql = KendoHelper.FiltersToParameterizedQuery(FilterDescriptorFactory.Create(filter), paramValues: filterSqlParams);
                filter = filterSql;
                var filterParameters = filterSqlParams.ToArray();

                var data = _requerimientoSrv.GetDatosVistaGeneroSinGrupos("", filter, filterParameters);

                var datos = ExcelExportEPPlus.ConvertToDataTable(data);
                if (datos.Columns.Contains("Fecha Cierre"))
                {
                    datos.Columns["Fecha Cierre"].ExtendedProperties.Add("DateFormat", "dd/mm/yyyy");
                }

                if (datos != null)
                {
                    byte[] content = null;
                    content = ExcelExportEPPlus.CreateExcelDocument(datos);
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    if (content != null)
                    {
                        var fileName = "VistaGenero_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx";
                        FileResult fileResult = File(content,
                            System.Net.Mime.MediaTypeNames.Application.Octet,
                            fileName);

                        return fileResult;

                        //return File(content, contentType, fileName);
                    }
                }
                return RedirectToAction("ErrorAnonimo", "Home", new RouteValueDictionary() { { "mensaje", "Ha ocurrido un error inesperado al realizar la exportación a Excel." } });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return RedirectToAction("ErrorAnonimo", "Home", new RouteValueDictionary() { { "mensaje", "Ha ocurrido un error inesperado al realizar la exportación a Excel: " + ex.Message } });
            }
        }
        #endregion

    }
}