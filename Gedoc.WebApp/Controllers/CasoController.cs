using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Gedoc.WebApp.Models;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;
using Gedoc.WebApp.Helpers.Maps.Interface;

namespace Gedoc.WebApp.Controllers
{
    public class CasoController : BaseController
    {
        //private GedocEntities db = new GedocEntities();
        private IMantenedorService _mantSvc;
        private readonly IGenericMap _mapper;

        public CasoController(IMantenedorService mantSvc, IGenericMap mapper)
        {
            _mantSvc = mantSvc;
            _mapper = mapper;
        }

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [IgnoreAccessFilter(UrlBase = "/Caso")]
        public ActionResult AgregarIngreso(int IdCaso)
        {
            ViewBag.IdCaso = IdCaso;
            ViewBag.Caso = _mantSvc.GetCasoById(IdCaso);
            CargaCombos();
            return View();
        }

        [IgnoreAccessFilter(UrlBase = "/Caso")]
        public ActionResult ReqIngresos(int IdCaso)
        {
            ViewBag.IdCaso = IdCaso;
            ViewBag.Caso = _mantSvc.GetCasoById(IdCaso);
            CargaCombos();
            return View();
        }

        //No son necesarios de virtualizar
        public void CargaCombos()
        {
            var estadoEtapas = new Dictionary<string, string>();
            var estados = _mantSvc.GetGenericoMatenedor(Mantenedor.EstadoRequerimiento).Data;
            foreach (var estado in estados)
            {
                foreach (var etapa in ((List<GenericoDto>)estado.ExtraDataObj))
                {
                    estadoEtapas.Add($"{estado.Id}-{etapa.Id}", $"{estado.Titulo}  -  {etapa.Titulo}");
                }
            }
            ViewBag.Estados = estadoEtapas.Select(n => new DropDownListItem { Value = n.Key, Text = n.Value }).ToList();

            ViewBag.TiposTramite = _mantSvc.GetGenericoMatenedor(Mantenedor.TipoTramite).Data
                .Select(r => new DropDownListItem { Text = r.Titulo, Value = r.Id })
                .OrderBy(r => r.Text).ToList();
            ViewBag.CategoriasMonumentosNacionales = _mantSvc.GetGenericoMatenedor(Mantenedor.CategoriaMn).Data
                .Select(r => new DropDownListItem { Text = r.Titulo, Value = r.Id })
                .Distinct().ToList();
            ViewBag.Comunas = _mantSvc.GetGenericoMatenedor(Mantenedor.Comuna, "", "FullTitle2").Data
                .Select(r => new DropDownListItem { Text = r.Titulo, Value = r.Id.ToString() })
                .OrderBy(r => r.Text).ToList();
            ViewBag.UnidadesTecnicas = _mantSvc.GetGenericoMatenedor(Mantenedor.UnidadTecnica).Data
                .Select(r => new DropDownListItem { Text = r.Titulo, Value = r.Id.ToString(), Selected = false })
                .OrderBy(r => r.Text).ToList();
            ViewBag.Etiquetas = _mantSvc.GetGenericoMatenedor(Mantenedor.Etiqueta).Data
                .Select(r => new DropDownListItem { Text = r.Titulo, Value = r.Id.ToString() }).OrderBy(r => r.Text).ToList();

        }

        #region Carga de Grilla Base
        public ActionResult Caso_Read([DataSourceRequest] DataSourceRequest request)
        {
            var casos = _mantSvc.GetCasoAll();
            return Json(casos.ToDataSourceResult(request));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Caso_Create([DataSourceRequest] DataSourceRequest request, Gedoc.WebApp.Models.CasoModel caso)
        {
            if (!HaySesionActiva())
                ModelState.AddModelError("error", "Se perdió la sesión");

            if (ModelState.IsValid)
            {
                var casoDto = _mapper.MapFromModelToDto<CasoModel, CasoDto>(caso);
                casoDto.CreadoPor = CurrentUserId;
                var resultado = _mantSvc.SaveCaso(casoDto);

                if (resultado.Codigo > 0)
                {
                    caso = _mapper.MapFromDtoToModel<CasoDto, CasoModel>(casoDto);
                }
                else
                {
                    ModelState.AddModelError("error", "Error al realizar la operación");
                }
            }
            else
            {
                ModelState.AddModelError("error", "La sesión ha expirado.");
            }
            return Json(new[] { caso }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Caso_Update([DataSourceRequest] DataSourceRequest request, Gedoc.WebApp.Models.CasoModel caso)
        {
            if (!HaySesionActiva())
                ModelState.AddModelError("error", "Se perdió la sesión");

            if (ModelState.IsValid)
            {
                var casoDto = _mapper.MapFromModelToDto<CasoModel, CasoDto>(caso);
                casoDto.ModificadoPor = CurrentUserId;
                var resultado = _mantSvc.SaveCaso(casoDto);

                caso.FechaModificacion = casoDto.FechaModificacion;
                caso.ModificadoPor = CurrentUserId;
            }
            return Json(new[] { caso }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        //Requerimientos Disponibles para asignar
        public ActionResult GetRequerimientosNoAsignados([DataSourceRequest] DataSourceRequest request, CasoFilterDto filtros)
        {
            var requerimientos = _mantSvc.GetRequerimientosNoAsignadoCaso(filtros);
            return Json(requerimientos.ToDataSourceResult(request));
        }

        //Ingresos para un Caso
        public ActionResult GetIngresosByCasoId([DataSourceRequest] DataSourceRequest request, int IdCaso, CasoFilterDto filtros)
        {
            var requerimientos = _mantSvc.GetRequerimientosByCasoId(IdCaso, filtros);

            return Json(requerimientos.ToDataSourceResult(request));
        }

        //Agrega casos segun seleccion de la grilla
        [HttpPost]
        public ActionResult AgregaCasosReqs(List<int> reqs, int idCaso)
        {
            var resultado = _mantSvc.AgregaRequeriminetosCaso(idCaso, reqs);

            return Json(resultado);
        }

        //Elimina casos segun seleccion de la grilla
        [HttpPost]
        public ActionResult EliminarCasosReqs(List<int> reqs)
        {
            var resultado = _mantSvc.EliminaRequerimientosCaso(reqs);

            return Json(resultado);
        }

        [HttpPost]
        public ActionResult EliminarCaso(int idCaso)
        {
            var result = _mantSvc.EliminaCaso(idCaso);
            return Json(result);
        }

        #region Virtualizacion Remitentes

        //public ActionResult RemitentePaging([DataSourceRequest] DataSourceRequest param)
        //{
        //    var db = new GedocEntities();

        //    if (param.Filters != null && param.Filters.Count > 0)
        //        ((FilterDescriptor)param.Filters[0]).Member = "Nombre";

        //    IQueryable<Remitente> req = db.Remitente;
        //    DataSourceResult result = req.ToDataSourceResult(param, r => new
        //    {
        //        Id = r.Id,
        //        Titulo = r.Nombre
        //    });

        //    return Json(result);
        //}

        //public ActionResult RemitenteByIds(List<int> ids, bool cerrado)
        //{
        //    var db = new GedocEntities();
        //    var query = db.Remitente.Where(r => ids.Contains(r.Id));
        //    var datos = query
        //        .OrderBy(r => r.Nombre)
        //        .AsEnumerable()
        //        .Select(r => new GenericoDto()
        //        {
        //            Id = r.Id,
        //            Titulo = r.Nombre
        //        }).ToList();
        //    return Json(datos);
        //}

        private IEnumerable<ComboboxItemModel> GetRemitentes()
        {
            var datos = _mantSvc.GetRemitenteResumenAll().Data;
            return datos
                .Select(req => new ComboboxItemModel()
                {
                    Value = req.IdInt,
                    Text = req.Titulo,
                });
        }

        public ActionResult VirtualizationRemitentes_Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json(GetRemitentes().ToDataSourceResult(request));
        }

        public ActionResult Remitentes_ValueMapper(int?[] values)
        {
            var indices = new List<int>();
            if (values != null && values.Any())
            {
                var index = 0;
                foreach (var remitente in GetRemitentes())
                {
                    if (values.Contains(remitente.Value))
                        indices.Add(index);
                    index += 1;
                }
            }
            return Json(indices, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Virtualizacion Documentos Ingreso
        //private IEnumerable<ComboboxItemModel> GetDocumentos()
        //{
        //    return db.Requerimiento
        //        //.Where(r => r.CasoId == null)
        //        .Select(req => new ComboboxItemModel()
        //    {
        //        Value = req.Id,
        //        Text = req.DocumentoIngreso,
        //    });
        //}

        //public ActionResult VirtualizationDocumentos_Read([DataSourceRequest] DataSourceRequest request)
        //{
        //    return Json(GetDocumentos().ToDataSourceResult(request));
        //}

        //public ActionResult Documentos_ValueMapper(int?[] values)
        //{
        //    var indices = new List<int>();
        //    if (values != null && values.Any())
        //    {
        //        var index = 0;
        //        foreach (var documento in GetDocumentos())
        //        {
        //            if (values.Contains(documento.Value))
        //                indices.Add(index);
        //            index += 1;
        //        }
        //    }
        //    return Json(indices, JsonRequestBehavior.AllowGet);
        //}
        #endregion
          

    }
}
