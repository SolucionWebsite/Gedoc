using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Helpers.Logging;
using Gedoc.Repositorio.Model;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;
using Gedoc.WebApp.WssRegMonSrvReference;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
//using Gedoc.WebApp.RegMonServiceRef;
using Newtonsoft.Json;
using Rol = Gedoc.Helpers.Enum.Rol;

namespace Gedoc.WebApp.Controllers
{
    public class JsonController : BaseController
    {
        private GedocEntities db = new GedocEntities();

        private readonly IMantenedorService _mantenedorSrv;
        private readonly IRequerimientoService _requerimientoSrv;

        public JsonController(IMantenedorService mantenedorSrv, IRequerimientoService requerimientoSrv)
        {
            _mantenedorSrv = mantenedorSrv;
            _requerimientoSrv = requerimientoSrv;
        }

        [HttpPost]
        [IgnoreLoginFilter]
        public JsonResult KeepSessionAlive()
        {
            return new JsonResult { Data = "Success" };
        }

        #region Datos de mantenedores para mostrar en controles web
        //public ActionResult TipoTramite()
        //{
        //    var datos = _mantenedorSrv.GetGenericoMatenedor(Mantenedor.TipoTramite);
        //    return Json(datos);
        //}

        //public ActionResult TipoDocumento()
        //{
        //    var datos = _mantenedorSrv.GetGenericoMatenedor(Mantenedor.TipoDocumento);
        //    return Json(datos);
        //}

        public ActionResult MantenedorGenerico(int idM, string extra, string extra2)
        {
            var datos = _mantenedorSrv.GetGenericoMatenedor((Mantenedor)idM, extra, extra2);
            return Json(datos);
        }

        public ActionResult RemitenteResumen()
        {
            var datos = _mantenedorSrv.GetRemitenteResumenAll();
            return Json(datos);
        }

        public ActionResult RemitenteResumenByIds(List<int> ids)
        {
            var datos = _mantenedorSrv.GetRemitenteResumenByIds(ids);
            return Json(datos);
        }

        public ActionResult RemitenteResumenPaging([DataSourceRequest]DataSourceRequest param) // ParametrosGrillaDto<int> param) // 
        {
            if (param.Filters != null && param.Filters.Count > 0) ((FilterDescriptor)param.Filters[0]).Member = "Nombre";
            param.Filters.Add(new FilterDescriptor
            {
                Member = "Activo",
                Value = 1,
                Operator = FilterOperator.IsEqualTo
            });
            param.Sorts.Clear();
            param.Sorts.Add(new SortDescriptor() {Member = "Nombre", SortDirection = ListSortDirection.Ascending});
            IQueryable<Remitente> remitentes = db.Remitente;
            DataSourceResult result = remitentes.ToDataSourceResult(param, r => new {
                Id = r.Id,
                Titulo = r.Nombre
            });

            return Json(result);
        }

        public ActionResult RemitenteById(int id)
        {
            var datos = _mantenedorSrv.GetRemitenteById(id);
            return Json(datos);
        }
        #endregion

        #region Requerimiento
        public ActionResult RequerimientoResumen(bool cerrado)
        {
            var datos = _requerimientoSrv.GetResumenAll(cerrado);

            var jsonResult = new JsonpResult
            {
                Data = datos,
                //JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = { NullValueHandling = NullValueHandling.Ignore }
            }; //= Json(datos);
            //jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult RequerimientoResumenByIds(List<int> ids, bool cerrado)
        {
            var datos = _requerimientoSrv.GetRequerimientoResumenByIds(ids, cerrado);
            return Json(datos);
        }

        public ActionResult RequerimientoResumenPaging([DataSourceRequest]DataSourceRequest param)
        {
            if (param.Filters != null && param.Filters.Count > 0) ((FilterDescriptor)param.Filters[0]).Member = "DocumentoIngreso";

            //param.Sorts.Clear();
            //param.Sorts.Add(new SortDescriptor() { Member = "DocumentoIngreso", SortDirection = ListSortDirection.Ascending });
            //IQueryable<Requerimiento> req = db.Requerimiento;
            //DataSourceResult result = req.ToDataSourceResult(param, r => new {
            //    Id = r.Id,
            //    Titulo = r.DocumentoIngreso
            //});

            var datos = _requerimientoSrv.GetResumenAll(false);
            DataSourceResult result = datos.Data.ToDataSourceResult(param, r => new {
                Id = r.Id,
                Titulo = r.DocumentoIngreso
            });

            return Json(result);

        }

        public ActionResult RequerimientoUsuarioResumenPaging([DataSourceRequest]DataSourceRequest param)
        {
            if (param.Filters != null && param.Filters.Count > 0) 
                ((FilterDescriptor)param.Filters[0]).Member = "DocumentoIngreso";

            if (EsAdmin())
            {
                // Se hace nada, se muestran los ingresos de todas las UTs
            }else if (CurrentUserRoles.Contains((int)Rol.ProfesionalUt))
            { // Si el usuario es Profesional UT se muestran sólo los requeriminetos de los q el usuario es profesional asignado
                param.Filters.Add(new FilterDescriptor("ProfesionalId", FilterOperator.IsEqualTo, CurrentUserId));
            } else
            { // Si el usuario es Encargado UT se muestran sólo los requerimientos de las UT q él sea encargado UT.
              // Si no es encargado, ni tampoco profesional ut, se muestran sólo los requerimientos de las UT a las q pertenece el usuario
                var esEncargado = CurrentUserRoles.Contains((int) Rol.EncargadoUt);
                var uts = _mantenedorSrv.GetUnidadTecnicaByUsuario(CurrentUserId.GetValueOrDefault(), esEncargado);
                var compFilter = new CompositeFilterDescriptor {LogicalOperator = FilterCompositionLogicalOperator.Or};
                if ((uts?.Count ?? 0) == 0)
                { // No hay UT para el usuario, se aplica un filtro q no devolverá datos.
                    param.Filters.Add(new FilterDescriptor("UtAsignadaId", FilterOperator.IsEqualTo, -99)  );
                }
                else
                {
                    foreach (var ut in uts)
                    {
                        compFilter.FilterDescriptors.Add(new FilterDescriptor("UtAsignadaId", FilterOperator.IsEqualTo, ut.Id));
                    }
                    param.Filters.Add(compFilter);
                }
            } 
            param.Sorts.Clear();
            param.Sorts.Add(new SortDescriptor() { Member = "DocumentoIngreso", SortDirection = ListSortDirection.Ascending });
            IQueryable<Requerimiento> req = db.Requerimiento;
            DataSourceResult result = req.ToDataSourceResult(param, r => new {
                Id = r.Id,
                Titulo = r.DocumentoIngreso
            });

            return Json(result);
        }

        public ActionResult RequerimientoAntResumenPaging([DataSourceRequest]DataSourceRequest param)
        {
            if (param.Filters != null && param.Filters.Count > 0) ((FilterDescriptor)param.Filters[0]).Member = "DocumentoIngreso";
            IQueryable<Requerimiento> req = db.Requerimiento.Where(x=>x.EstadoId == (int)EstadoIngreso.Cerrado);
            DataSourceResult result = req.ToDataSourceResult(param, r => new {
                Id = r.Id,
                Titulo = r.DocumentoIngreso
            });

            return Json(result);
        }

        public ActionResult NumeroDespachoResumen()
        {
            var datos = _mantenedorSrv.GetNumeroDespachoResumenAll();
            return Json(datos);
        }

        public ActionResult NumeroDespachoResumenByIds(List<string> ids)
        {
            var datos = _mantenedorSrv.GetNumeroDespachoResumenByIds(ids);
            return Json(datos);
        }

        public ActionResult NumeroDespachoResumenPaging([DataSourceRequest] DataSourceRequest param) // ParametrosGrillaDto<int> param) // 
        {
            if (param.Filters != null && param.Filters.Count > 0) ((FilterDescriptor)param.Filters[0]).Member = "Titulo";

            #region Número despachos
            IQueryable<string> numDespachos = db.Despacho
                .Where(d => !d.Eliminado)
                .OrderBy(r => r.NumeroDespacho)
                .Select(s => s.NumeroDespacho)
                .Distinct();
            var despachos = numDespachos
                .Select(s => new GenericoDto {
                    Id = s,
                    Titulo = s
                })
                .ToList();
            #endregion

            #region Número despachos inciativas CMN
            var numDespachosInic = db.DespachoIniciativa
                .Where(d => !d.Eliminado)
                .OrderBy(r => r.NumeroDespacho)
                .Select(s => s.NumeroDespacho)
                .Distinct()
                .ToList();
            var despachosInic = numDespachosInic
                .Where(numDespInici => despachos.All(d => d.Id != numDespInici))
                .Select(s => new GenericoDto
                {
                    Id = s,
                    Titulo = s
                })
                .ToList();
            #endregion

            var datos = despachos.Concat(despachosInic).OrderBy(d => d.Id).ToList();

            DataSourceResult result = datos.ToDataSourceResult(param, r => new {
                r.Id,
                r.Titulo
            });

            return Json(result);
        }

        public ActionResult LogBitacoraRequerimiento(int reqId)
        {
            var datos = _requerimientoSrv.GetLogBitacoraRequerimiento(reqId);
            return Json(datos);
        }

        #endregion

        #region Interoperación con Regmon
        public ActionResult GetDatosRemonMn(string codigo)
        {
            var result = new ResultadoOperacion(-1, "Error al realizar la operación.", null);
            try
            {
                var regMonHlp = new RegmonHelper(_mantenedorSrv);
                var datosMn = regMonHlp.GetDatosMn(codigo);
                result.Extra = datosMn;
                if (datosMn == null)
                {
                    result.Mensaje = "Ha ocurrido un error al obtener los datos del Monumento Nacional " + codigo + " desde Regmon.";
                    result.Extra = new MonumentoNacionalDto();
                    return Json(result);
                } else if (string.IsNullOrWhiteSpace(datosMn.CodigoMonumentoNac))
                {
                    result.Mensaje = "No se ha encontrado en Regmon el código " + codigo + " de monumento seleccionado.";
                    return Json(result);
                }
                else
                {
                    result.Codigo = 1;
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                result.Mensaje = "Ha ocurrido un error al cargar los datos del Monumento Nacional seleccionado, <br/>por favor, chequee el log de errores de la aplicación.";
                result.Extra = ex.Message;
            }

            return Json(result);
        }
        #endregion 


    }
}