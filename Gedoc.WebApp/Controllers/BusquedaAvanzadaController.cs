using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;
using Gedoc.Repositorio.Maps.Interfaces;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;
using Gedoc.WebApp.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Web.WebPages;
using Gedoc.Helpers;
using Gedoc.Helpers.Enum;

namespace Gedoc.WebApp.Controllers
{
    public class BusquedaAvanzadaController : BaseController
    {
        private readonly IRequerimientoService _requerimientoSrv;
        private readonly IDespachoService _despachoSrv;
        private readonly IMantenedorService _mantenedorSrv;
        private readonly Gedoc.Repositorio.Model.GedocEntities db = new Gedoc.Repositorio.Model.GedocEntities();

        public BusquedaAvanzadaController(IRequerimientoService requerimientoSrv, IDespachoService despachoSrv, IMantenedorService mantenedorSrv)
        {
            this._requerimientoSrv = requerimientoSrv;
            this._despachoSrv = despachoSrv;
            this._mantenedorSrv = mantenedorSrv;
            db.Database.CommandTimeout = WebConfigValues.SqlCommandTimeOut;
        }

        protected override void Dispose(bool disposing)
        {
            //db.Dispose();
            base.Dispose(disposing);
        }

        //[Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetProfesionalesUT(string unidadTecnica, string profesional)
        {

            var usuarios = _mantenedorSrv.GetGenericoMatenedor(Mantenedor.Profesional, unidadTecnica, "");
            if (!string.IsNullOrEmpty(profesional))
            {
                usuarios.Data = usuarios.Data.Where(d => d.Titulo.ToLower().Contains(profesional.ToLower())).ToList();
            }

            return Json(usuarios.Data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Resultado(BusquedaAvanzadaModel baParams)
        {
            return View();
        }

        public ActionResult IngresosTab(BusquedaAvanzadaModel baParams)
        {
            return View("~/Views/BusquedaAvanzada/Partial/IngresosTab.cshtml");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult IngresosTab_Read([DataSourceRequest] DataSourceRequest request, BusquedaAvanzadaModel baParams)
        {
            try
            {
                if (request.Filters != null)
                {
                    //ModifyFilters(request.Filters);
                }

                //verifica que vanga almenos 1 filtro
                if (TieneFiltroIngreso(baParams))
                {
                    var datos = (
                       from req in QueryIngreso(baParams)
                       join prof in db.Usuario on req.ProfesionalId equals prof.Id into profReq
                       from pr in profReq.DefaultIfEmpty()
                       select new { Req = req, Prof = pr }).ToList();

                    var requerimientos = (from r in datos
                                          select new RequerimientoDto()
                                          {
                                              Id = r.Req.Id,
                                              DocumentoIngreso = r.Req.DocumentoIngreso,
                                              FechaIngreso = r.Req.FechaIngreso,
                                              FechaDocumento = r.Req.FechaDocumento,
                                              ObservacionesTipoDoc = (r.Req.TipoDocumento != null ? (r.Req.TipoDocumento.Titulo + " ") : "") + r.Req.ObservacionesTipoDoc,
                                              ObservacionesAdjuntos = (r.Req.TipoAdjunto != null && r.Req.TipoAdjunto.Any() ? (r.Req.TipoAdjunto.FirstOrDefault().Titulo + " ") : "") + r.Req.ObservacionesAdjuntos,
                                              RemitenteNombre = r.Req.Remitente != null ? r.Req.Remitente.Nombre : "",
                                              RemitenteInstitucion = r.Req.Remitente != null ? r.Req.Remitente.Institucion : "",
                                              Materia = r.Req.Materia,
                                              EtiquetaTitulos = string.Join(", ", r.Req.Etiqueta.Select(e => e.Titulo)),
                                              CategoriaMonumentoNacTitulo = string.Join(", ", r.Req.MonumentoNacional.CategoriaMonumentoNac.Select(e => e.Titulo)),
                                              DenominacionOficial = r.Req.MonumentoNacional.DenominacionOficial,
                                              RegionTitulos = string.Join(", ", r.Req.MonumentoNacional.Region.Select(e => e.Titulo)),
                                              ComunaTitulos = string.Join(", ", r.Req.MonumentoNacional.Comuna.Select(e => e.Titulo)),
                                              UtAsignadaTitulo = r.Req.UnidadTecnicaAsign != null ? r.Req.UnidadTecnicaAsign.Titulo : "",
                                              ProfesionalNombre = r.Prof != null ? r.Prof.NombresApellidos : "",
                                              EstadoTitulo = r.Req.EstadoRequerimiento.Titulo,
                                              ComentarioCierre = r.Req.ComentarioCierre
                                          }).ToDataSourceResult(request);

                    return Json(requerimientos);
                }

                return null;
            }
            catch (Exception exc)
            {
                Gedoc.Helpers.Logging.Logger.LogError(exc);
                return null;
            }
        }

        public ActionResult DespachosTab(BusquedaAvanzadaModel baParams)
        {
            return View("~/Views/BusquedaAvanzada/Partial/DespachosTab.cshtml");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DespachosTab_Read([DataSourceRequest] DataSourceRequest request, BusquedaAvanzadaModel baParams)
        {
            try
            {
                if (request.Filters != null)
                {
                    //ModifyFilters(request.Filters);
                }

                //verifica que vanga almenos 1 filtro
                if (TieneFiltroDespacho(baParams))
                {
                    //var regiones = db.Region.ToList();
                    //var comunas = db.Comuna.ToList();

                    var datos = QueryDespacho(baParams).ToList();

                    DataSourceResult despachos = datos.ToDataSourceResult(request);
                    //= (from d in datos
                    //  select new RequerimientoDespachoDto()
                    //  {
                    //      Id = d.Id,
                    //      DespachoId = d.DespachoId,
                    //      DocumentoIngreso = d.DocumentoIngreso,
                    //      FechaIngreso = d.FechaIngreso,
                    //      UtAsignadaTitulo = d.UtAsignadaTitulo,
                    //      ProfesionalNombre = d.ProfesionalNombre,
                    //      NumeroDespacho = d.NumeroDespacho,
                    //      FechaEmisionOficio = d.FechaEmisionOficio,
                    //      DestinatarioNombre = d.DestinatarioNombre,
                    //      DestinatarioInstitucion = d.DestinatarioInstitucion,
                    //      MateriaOficioCMN = d.MateriaOficioCMN,
                    //      RegionTitulo = d.RegionTitulo,
                    //      ComunaTitulo = d.ComunaTitulo,
                    //      Etiqueta = d.Etiqueta,
                    //      TipoAdjuntoTitulos = string.Join("; ", d.TiposAdjuntos),
                    //      CantidadAdjuntos = d.CantidadAdjuntos,
                    //      MedioDespachoTitulo = d.MedioDespachoTitulo,
                    //      FechaRecepcion = d.FechaRecepcion
                    //  }).ToDataSourceResult(request);

                    return Json(despachos);
                }

                return null;
            } 
            catch(Exception exc)
            {
                Gedoc.Helpers.Logging.Logger.LogError(exc);
                return null;
            }
        }

        public ActionResult DespachosIniciativasTab(BusquedaAvanzadaModel baParams)
        {
            return View("~/Views/BusquedaAvanzada/Partial/DespachosIniciativasTab.cshtml");
        }

        public ActionResult DespachosIniciativasTab_Read([DataSourceRequest] DataSourceRequest request, BusquedaAvanzadaModel baParams)
        {
            try
            {
                if (request.Filters != null)
                {
                    //ModifyFilters(request.Filters);
                }

                //verifica que vanga almenos 1 filtro
                if (TieneFiltroDespachoIniciativa(baParams))
                {
                    var datos = QueryDespachoIniciativa(baParams).ToList();

                    DataSourceResult despachos = (from d in datos
                                                  select new DespachoIniciativaDto()
                                                  {
                                                      Id = d.Id,
                                                      NumeroDespacho = d.NumeroDespacho,
                                                      FechaEmisionOficio = d.FechaEmisionOficio,
                                                      RemitenteNombre = d.Remitente?.Nombre,
                                                      RemitenteInstitucion = d.Remitente?.Institucion,
                                                      Materia = d.Materia,
                                                      MonumentoNacional = d.MonumentoNacional == null ? null : new MonumentoNacionalDto()
                                                      {
                                                          DenominacionOficial = d.MonumentoNacional.DenominacionOficial,
                                                          CategoriaMonumentoNacCod = string.Join("; ", d.MonumentoNacional.CategoriaMonumentoNac.Select(s => s.Titulo)),
                                                          //CategoriaMonumentoNac = new List<GenericoDto>()
                                                          //{
                                                          //     new GenericoDto() {
                                                          //         Activo = true,
                                                          //         Id = d.MonumentoNacional.CategoriaMonumentoNac.Count > 0 ? d.MonumentoNacional.CategoriaMonumentoNac.FirstOrDefault().Id : 0,
                                                          //         Titulo = d.MonumentoNacional.CategoriaMonumentoNac.Count > 0 ? d.MonumentoNacional.CategoriaMonumentoNac.FirstOrDefault().Titulo : ""
                                                          //     }
                                                          //},
                                                          RegionTitulo = string.Join("; ", d.MonumentoNacional.Region.Select(s => s.Titulo)),
                                                          ComunaTitulo = string.Join("; ", d.MonumentoNacional.Comuna.Select(s => s.Titulo))
                                                      },
                                                      UtAsignadaTitulo = d.UtAsignadaId != null ? db.UnidadTecnica.Find(d.UtAsignadaId)?.Titulo : "",
                                                      ProfesionalNombre = d.ProfesionalId != null ? db.Usuario.Find(d.ProfesionalId)?.NombresApellidos : "",
                                                      EtiquetaTitulos = string.Join("; ", d.Etiqueta.Select(e => e.Titulo)),
                                                      TipoAdjuntoTitulos = string.Join("; ", d.TipoAdjunto.Select(e => e.Titulo)),
                                                      CantidadAdjuntos = d.CantidadAdjuntos,
                                                      MedioDespachoTitulo = d.FormaLlegada != null ? d.FormaLlegada.Titulo : "",
                                                      FechaRecepcion = d.FechaRecepcion
                                                  }).ToDataSourceResult(request);

                    return Json(despachos);
                }
                
                return null;
            }
            catch (Exception exc)
            {
                Gedoc.Helpers.Logging.Logger.LogError(exc);
                return null;
            }
        }

        public ActionResult BitacorasTab(BusquedaAvanzadaModel baParams)
        {
            return View("~/Views/BusquedaAvanzada/Partial/BitacorasTab.cshtml");
        }

        public ActionResult BitacorasTab_Read([DataSourceRequest] DataSourceRequest request, BusquedaAvanzadaModel baParams)
        {
            try
            {
                if (request.Filters != null)
                {
                    //ModifyFilters(request.Filters);
                }

                //verifica que vanga almenos 1 filtro
                if (TieneFiltroBitacora(baParams))
                {
                    var datos = QueryBitacora(baParams).ToList();

                    var bitacoras = from b in datos
                                    select new BitacoraDto()
                                    {
                                        Id = b.Id,
                                        DocIngreso = b.Requerimiento?.DocumentoIngreso ?? "",
                                        RequerimientoId = b.RequerimientoId,
                                        Requerimiento = b.Requerimiento == null ? null : new RequerimientoDto()
                                        {
                                            FechaIngreso = b.Requerimiento.FechaIngreso,
                                            RemitenteNombre = b.Requerimiento.Remitente != null ? b.Requerimiento.Remitente.Nombre : "",
                                            RemitenteInstitucion = b.Requerimiento.Remitente != null ? b.Requerimiento.Remitente.Institucion : "",
                                            Materia = b.Requerimiento.Materia,
                                            MonumentoNacional = b.Requerimiento.MonumentoNacional == null ? null : new MonumentoNacionalDto()
                                            {
                                                //CategoriaMonumentoNac = new List<GenericoDto>()
                                                //{
                                                //     new GenericoDto() {
                                                //         Titulo = b.Requerimiento.MonumentoNacional.CategoriaMonumentoNac.Count > 0 ? b.Requerimiento.MonumentoNacional.CategoriaMonumentoNac.FirstOrDefault().Titulo : ""
                                                //     }
                                                //},
                                                CategoriaMonumentoNacCod = string.Join("; ", b.Requerimiento.MonumentoNacional.CategoriaMonumentoNac.Select(s => s.Titulo)),
                                                DenominacionOficial = b.Requerimiento.MonumentoNacional.DenominacionOficial,
                                            },
                                            RegionTitulos = b.Requerimiento.MonumentoNacional != null ? string.Join("; ", b.Requerimiento.MonumentoNacional.Region.Select(s => s.Titulo)) : "",
                                            ComunaTitulos = b.Requerimiento.MonumentoNacional != null ? string.Join("; ", b.Requerimiento.MonumentoNacional.Comuna.Select(s => s.Titulo)) : "",
                                            UnidadTecnicaAsign = new UnidadTecnicaDto()
                                            {
                                                Titulo = b.Requerimiento.UnidadTecnicaAsign != null ? b.Requerimiento.UnidadTecnicaAsign.Titulo : ""
                                            },
                                            ProfesionalNombre = b.Requerimiento.ProfesionalId != null ? db.Usuario.Find(b.Requerimiento.ProfesionalId).NombresApellidos : ""
                                        },
                                        UsuarioCreacionNombresApellidos = b.UsuarioCreacion != null ? b.UsuarioCreacion.NombresApellidos : "",
                                        TipoBitacoraTitulo = b.TipoBitacora.Titulo,
                                        Fecha = b.Fecha,
                                        FechaSolicitudRevision = b.Requerimiento?.FechaIngreso, //Temporalmente usar este campo para pasar fecha de ingreso a la grid
                                        ObsAcuerdoComentario = b.ObsAcuerdoComentario,
                                        FechaUltimoAcuerdoComision = b.Requerimiento == null
                                            ? null
                                            : (b.Requerimiento.Bitacora.Any(bi => bi.TipoBitacoraCod == TipoBitacora.AcuerdoComision.ToString("D"))
                                                ? b.Requerimiento.Bitacora.Where(bi => bi.TipoBitacoraCod == TipoBitacora.AcuerdoComision.ToString("D")).OrderByDescending(o => o.Fecha).FirstOrDefault()?.Fecha
                                                : null), //--> 1 = Acuerdo de Comisión
                                        UltimoAcuerdoComision = b.Requerimiento == null
                                            ? null
                                            : (b.Requerimiento.Bitacora.Any(bi => bi.TipoBitacoraCod == TipoBitacora.AcuerdoComision.ToString("D"))
                                                ? b.Requerimiento.Bitacora.Where(bi => bi.TipoBitacoraCod == TipoBitacora.AcuerdoComision.ToString("D")).OrderByDescending(o => o.Fecha).FirstOrDefault()?.UltimoComentario
                                                : ""), //--> 1 = Acuerdo de Comisión,
                                        FechaUltimoAcuerdoSesion = b.Requerimiento == null
                                            ? null
                                            : (b.Requerimiento.Bitacora.Any(bi => bi.TipoBitacoraCod == TipoBitacora.AcuerdoSesion.ToString("D"))
                                                ? b.Requerimiento.Bitacora.Where(bi => bi.TipoBitacoraCod == TipoBitacora.AcuerdoSesion.ToString("D")).OrderByDescending(o => o.Fecha).FirstOrDefault()?.Fecha
                                                : null), //--> 2 = Acuerdo de Sesión
                                        UltimoAcuerdoSesion = b.Requerimiento == null
                                            ? null
                                            : (b.Requerimiento.Bitacora.Any(bi => bi.TipoBitacoraCod == TipoBitacora.AcuerdoSesion.ToString("D"))
                                                ? b.Requerimiento.Bitacora.Where(bi => bi.TipoBitacoraCod == TipoBitacora.AcuerdoSesion.ToString("D")).OrderByDescending(o => o.Fecha).FirstOrDefault()?.UltimoComentario
                                                : "") //--> 2 = Acuerdo de Sesión

                                    };

                    DataSourceResult result = bitacoras.ToDataSourceResult(request, bitacora => new BitacoraDto()
                    {
                        Id = bitacora.Id,
                        DocIngreso = bitacora.DocIngreso,
                        RequerimientoId = bitacora.RequerimientoId,
                        Requerimiento = bitacora.Requerimiento,
                        UsuarioCreacionNombresApellidos = bitacora.UsuarioCreacionNombresApellidos,
                        TipoBitacoraTitulo = bitacora.TipoBitacoraTitulo,
                        Fecha = bitacora.Fecha,
                        ObsAcuerdoComentario = bitacora.ObsAcuerdoComentario,
                        FechaUltimoAcuerdoComision = bitacora.FechaUltimoAcuerdoComision,
                        UltimoAcuerdoComision = bitacora.UltimoAcuerdoComision,
                        FechaUltimoAcuerdoSesion = bitacora.FechaUltimoAcuerdoSesion,
                        UltimoAcuerdoSesion = bitacora.UltimoAcuerdoSesion,
                        FechaSolicitudRevision = bitacora.FechaSolicitudRevision //wth

                    });

                    return Json(result);
                }
                
                return null;
            }
            catch (Exception exc)
            {
                Gedoc.Helpers.Logging.Logger.LogError(exc);
                return null;
            }
        }

        [HttpPost]
        public ActionResult GetPreview(BusquedaAvanzadaModel baParams)
        {
            int iCount = 0;
            int dCount = 0;
            int diCount = 0;
            int bCount = 0;

            switch (baParams.TipoBusquedaBusqAv)
            {
                case "T":
                    {
                        if (TieneFiltroIngreso(baParams))
                            iCount = QueryIngreso(baParams).Count();
                        if (TieneFiltroDespacho(baParams))
                            dCount = QueryCountDespacho(baParams);
                        if (TieneFiltroDespachoIniciativa(baParams))
                            diCount = QueryDespachoIniciativa(baParams).Count();
                        if (TieneFiltroBitacora(baParams))
                            bCount = QueryBitacora(baParams).Count();

                        break;
                    }
                case "I":
                    {
                        if (TieneFiltroIngreso(baParams))
                            iCount = QueryIngreso(baParams).Count();
                        break;
                    }
                case "D":
                    {
                        if (TieneFiltroDespacho(baParams))
                            dCount = QueryCountDespacho(baParams);
                        break;
                    }
                case "DI":
                    {
                        if (TieneFiltroDespachoIniciativa(baParams))
                            diCount = QueryDespachoIniciativa(baParams).Count();
                        break;
                    }
                case "B":
                    {
                        if (TieneFiltroBitacora(baParams))
                            bCount = QueryBitacora(baParams).Count();
                        break;
                    }
            }

            return Json(new
            {
                I = iCount,
                D = dCount,
                DI = diCount,
                B = bCount

            });
        }

        private IQueryable<Requerimiento> QueryIngreso(BusquedaAvanzadaModel baParams)
        {
            baParams.CategoriaMonumentoNacionalBusqAv = baParams.CategoriaMonumentoNacionalBusqAv ?? "";
            //rangos fechas
            DateTime? fechaIngresoH = null;
            DateTime? fechaDocumentoIngresoH = null;
            if (baParams.FechaIngresoBusqAv.HasValue) fechaIngresoH = baParams.FechaIngresoBusqAv.Value.Date.AddDays(1);
            if (baParams.FechaDocumentoIngresoBusqAv.HasValue) fechaDocumentoIngresoH = baParams.FechaDocumentoIngresoBusqAv.Value.Date.AddDays(1);

            return db.Requerimiento.Where(x => !x.Eliminado && (!baParams.FechaIngresoBusqAv.HasValue || (x.FechaIngreso >= baParams.FechaIngresoBusqAv && x.FechaIngreso < fechaIngresoH)) &&
                (!baParams.DocumentoIngresoIdBusqAv.HasValue || x.Id == baParams.DocumentoIngresoIdBusqAv) &&
                (!baParams.FechaDocumentoIngresoBusqAv.HasValue || (x.FechaDocumento >= baParams.FechaDocumentoIngresoBusqAv && x.FechaDocumento < fechaDocumentoIngresoH)) &&
                (!baParams.RequerimientoAnteriorIdBusqAv.HasValue || x.RequerimientoAnteriorId == baParams.RequerimientoAnteriorIdBusqAv) &&
                (!baParams.RemitenteIdBusqAv.HasValue || x.RemitenteId == baParams.RemitenteIdBusqAv) &&
                (string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv) || x.Remitente.Institucion.Contains(baParams.InstitucionRemitenteBusqAv)) &&
                (string.IsNullOrEmpty(baParams.CargoProfesionRemitenteBusqAv) || x.Remitente.Cargo.Contains(baParams.CargoProfesionRemitenteBusqAv)) &&
                (string.IsNullOrEmpty(baParams.MateriaBusqAv) || x.Materia.Contains(baParams.MateriaBusqAv)) &&
                (string.IsNullOrEmpty(baParams.RegionBusqAv) || x.MonumentoNacional.Region.Any(a => a.Codigo == baParams.RegionBusqAv)) &&
                (string.IsNullOrEmpty(baParams.ComunaBusqAv) || x.MonumentoNacional.Comuna.Any(a => a.Codigo == baParams.ComunaBusqAv)) &&
                (string.IsNullOrEmpty(baParams.EtiquetaBusqAv) || x.Etiqueta.Any(a => a.Codigo == baParams.EtiquetaBusqAv && a.IdLista == 10)) &&
                (string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv) || x.MonumentoNacional.DenominacionOficial.Contains(baParams.DenominacionOficialBusqAv)) &&
                (string.IsNullOrEmpty(baParams.CategoriaMonumentoNacionalBusqAv) || x.MonumentoNacional.CategoriaMonumentoNac.Any(a => a.Codigo == baParams.CategoriaMonumentoNacionalBusqAv && a.IdLista == (int)Mantenedor.CategoriaMn)) &&
                (string.IsNullOrEmpty(baParams.RolSIIPropiedadBusqAv) || x.MonumentoNacional.RolSii.Contains(baParams.DenominacionOficialBusqAv)) &&
                (!baParams.UnidadTecnicaAsignadaBusqAv.HasValue || x.UtAsignadaId == baParams.UnidadTecnicaAsignadaBusqAv) &&
                (!baParams.ProfesionalUTAsignadoBusqAv.HasValue || x.ProfesionalId == baParams.ProfesionalUTAsignadoBusqAv) &&
                (!baParams.EstadoBusqAv.HasValue || x.EstadoId == baParams.EstadoBusqAv)
            );
        }

        private ICollection<RequerimientoDespachoDto> QueryDespacho(BusquedaAvanzadaModel baParams)
        {
            var usuarios = db.Usuario.ToList();

            //rangos fechas
            DateTime? fechaIngresoH = null;
            DateTime? fechaDocumentoIngresoH = null;
            DateTime? fechaEmisionOficioH = null;
            if (baParams.FechaIngresoBusqAv.HasValue) fechaIngresoH = baParams.FechaIngresoBusqAv.Value.Date.AddDays(1);
            if (baParams.FechaDocumentoIngresoBusqAv.HasValue) fechaDocumentoIngresoH = baParams.FechaDocumentoIngresoBusqAv.Value.Date.AddDays(1);
            if (baParams.FechaEmisionOficioCMNBusqAv.HasValue) fechaEmisionOficioH = baParams.FechaEmisionOficioCMNBusqAv.Value.Date.AddDays(1);

            baParams.CategoriaMonumentoNacionalBusqAv = baParams.CategoriaMonumentoNacionalBusqAv ?? "";
            var tieneFiltroRequerimientos = baParams.FechaIngresoBusqAv.HasValue
                                            || baParams.DocumentoIngresoIdBusqAv.HasValue
                                            || baParams.FechaDocumentoIngresoBusqAv.HasValue
                                            //|| !string.IsNullOrEmpty(baParams.MateriaBusqAv)
                                            || !string.IsNullOrEmpty(baParams.RegionBusqAv)
                                            || !string.IsNullOrEmpty(baParams.ComunaBusqAv)
                                            || !string.IsNullOrEmpty(baParams.EtiquetaBusqAv)
                                            || !string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv)
                                            || baParams.CategoriaMonumentoNacionalBusqAv != ""
                                            || baParams.UnidadTecnicaAsignadaBusqAv.HasValue
                                            || baParams.ProfesionalUTAsignadoBusqAv.HasValue;

            var tieneFiltroDesp = baParams.RemitenteIdBusqAv.HasValue
                                    || !string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv)
                                    || !string.IsNullOrEmpty(baParams.NumeroOficioCMNBusqAv)
                                    || baParams.FechaEmisionOficioCMNBusqAv.HasValue
                                    || !string.IsNullOrEmpty(baParams.MateriaBusqAv)
                                    //|| baParams.EtiquetaBusqAv.HasValue
                                    || !string.IsNullOrEmpty(baParams.FormaLlegadaBusqAv)
                                    || baParams.EstadoDespachoBusqAv.HasValue;

            IQueryable<Requerimiento> reqs = null;
            if (tieneFiltroRequerimientos)
            {
                reqs = db.Requerimiento.Where(r => !r.Eliminado && (!baParams.FechaIngresoBusqAv.HasValue || (r.FechaIngreso >= baParams.FechaIngresoBusqAv && r.FechaIngreso < fechaIngresoH)) &&
                    (!baParams.DocumentoIngresoIdBusqAv.HasValue || r.Id == baParams.DocumentoIngresoIdBusqAv) &&
                    (!baParams.FechaDocumentoIngresoBusqAv.HasValue || (r.FechaDocumento >= baParams.FechaDocumentoIngresoBusqAv && r.FechaDocumento < fechaDocumentoIngresoH)) &&
                    //(string.IsNullOrEmpty(baParams.MateriaBusqAv) || r.Materia.Contains(baParams.MateriaBusqAv)) &&
                    (string.IsNullOrEmpty(baParams.EtiquetaBusqAv) || r.Etiqueta.Any(a => a.Titulo.Contains(baParams.EtiquetaBusqAv))) &&
                    (string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv) || r.MonumentoNacional.DenominacionOficial.Contains(baParams.DenominacionOficialBusqAv)) &&
                    (string.IsNullOrEmpty(baParams.RegionBusqAv) || r.MonumentoNacional.Region.Any(rg => rg.Codigo == baParams.RegionBusqAv)) &&
                    (string.IsNullOrEmpty(baParams.ComunaBusqAv) || r.MonumentoNacional.Comuna.Any(co => co.Codigo == baParams.ComunaBusqAv)) &&
                    (baParams.CategoriaMonumentoNacionalBusqAv == "" || r.MonumentoNacional.CategoriaMonumentoNac.Any(n => n.Codigo == baParams.CategoriaMonumentoNacionalBusqAv && n.IdLista == (int)Mantenedor.CategoriaMn)) &&
                    (!baParams.UnidadTecnicaAsignadaBusqAv.HasValue || r.UtAsignadaId == baParams.UnidadTecnicaAsignadaBusqAv) &&
                    (!baParams.ProfesionalUTAsignadoBusqAv.HasValue || r.ProfesionalId == baParams.ProfesionalUTAsignadoBusqAv) &&
                    (!tieneFiltroDesp || (tieneFiltroDesp &&
                        r.Despacho.Any(d => !d.Eliminado && (!baParams.RemitenteIdBusqAv.HasValue || d.DestinatarioId == baParams.RemitenteIdBusqAv) &&
                                            (string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv) || d.Remitente.Institucion.Contains(baParams.InstitucionRemitenteBusqAv)) &&
                                            (string.IsNullOrEmpty(baParams.NumeroOficioCMNBusqAv) || d.NumeroDespacho == baParams.NumeroOficioCMNBusqAv) &&
                                            (!baParams.FechaEmisionOficioCMNBusqAv.HasValue || (d.FechaEmisionOficio >= baParams.FechaEmisionOficioCMNBusqAv && d.FechaEmisionOficio < fechaEmisionOficioH)) &&
                                            (string.IsNullOrEmpty(baParams.MateriaBusqAv) || d.Materia.Contains(baParams.MateriaBusqAv)) &&
                                            //(!baParams.EtiquetaBusqAv.HasValue || d.Etiqueta.Contains(bpes)) &&
                                            (string.IsNullOrEmpty(baParams.FormaLlegadaBusqAv) || d.MedioDespachoCod == baParams.FormaLlegadaBusqAv) &&
                                            (!baParams.EstadoDespachoBusqAv.HasValue || d.EstadoId == baParams.EstadoDespachoBusqAv)))));
                //reqs = db.Requerimiento.Where(r => (!baParams.FechaIngresoBusqAv.HasValue || (r.FechaIngreso >= baParams.FechaIngresoBusqAv && r.FechaIngreso < fechaIngresoH)) &&
                //    (!baParams.DocumentoIngresoIdBusqAv.HasValue || r.Id == baParams.DocumentoIngresoIdBusqAv) &&
                //    (!baParams.FechaDocumentoIngresoBusqAv.HasValue || (r.FechaDocumento >= baParams.FechaDocumentoIngresoBusqAv && r.FechaDocumento < fechaDocumentoIngresoH)) &&
                //    //(string.IsNullOrEmpty(baParams.MateriaBusqAv) || r.Materia.Contains(baParams.MateriaBusqAv)) &&
                //    (string.IsNullOrEmpty(baParams.EtiquetaBusqAv) || r.Etiqueta.Any(a => a.Titulo.Contains(bpes))) &&
                //    (string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv) || r.MonumentoNacional.DenominacionOficial.Contains(baParams.DenominacionOficialBusqAv)) &&
                //    (!baParams.RegionBusqAv.HasValue || r.MonumentoNacional.Region.Any(rg => rg.Id == baParams.RegionBusqAv.Value)) &&
                //    (!baParams.ComunaBusqAv.HasValue || r.MonumentoNacional.Comuna.Any(co => co.Id == baParams.ComunaBusqAv.Value)) &&
                //    (baParams.CategoriaMonumentoNacionalBusqAv == "" || r.MonumentoNacional.CategoriaMonumentoNac.Any(n => n.Codigo == baParams.CategoriaMonumentoNacionalBusqAv && n.IdLista == (int)Mantenedor.CategoriaMn)) &&
                //    (!baParams.UnidadTecnicaAsignadaBusqAv.HasValue || r.UtAsignadaId == baParams.UnidadTecnicaAsignadaBusqAv) &&
                //    (!baParams.ProfesionalUTAsignadoBusqAv.HasValue || r.ProfesionalId == baParams.ProfesionalUTAsignadoBusqAv) &&
                //    (!tieneFiltroDesp || (tieneFiltroDesp &&
                //        r.Despacho.Where(d => (!baParams.RemitenteIdBusqAv.HasValue || d.DestinatarioId == baParams.RemitenteIdBusqAv) &&
                //            (string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv) || d.Remitente.Institucion.Contains(baParams.InstitucionRemitenteBusqAv)) &&
                //            (string.IsNullOrEmpty(baParams.NumeroOficioCMNBusqAv) || d.NumeroDespacho == baParams.NumeroOficioCMNBusqAv) &&
                //            (!baParams.FechaEmisionOficioCMNBusqAv.HasValue || (d.FechaEmisionOficio >= baParams.FechaEmisionOficioCMNBusqAv && d.FechaEmisionOficio < fechaEmisionOficioH)) &&
                //            (string.IsNullOrEmpty(baParams.MateriaBusqAv) || d.Materia.Contains(baParams.MateriaBusqAv)) &&
                //            //(!baParams.EtiquetaBusqAv.HasValue || d.Etiqueta.Contains(bpes)) &&
                //            (string.IsNullOrEmpty(baParams.FormaLlegadaBusqAv) || d.MedioDespachoCod == baParams.FormaLlegadaBusqAv) &&
                //            (!baParams.EstadoDespachoBusqAv.HasValue || d.EstadoId == baParams.EstadoDespachoBusqAv)
                //    ).Count() > 0)));
            }

            IQueryable<Despacho> desps = null;
            if (!tieneFiltroRequerimientos)
                if (tieneFiltroDesp)
                {
                    desps = db.Despacho.Where(d => !d.Eliminado && (!baParams.RemitenteIdBusqAv.HasValue || d.DestinatarioId == baParams.RemitenteIdBusqAv) &&
                         (string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv) || d.Remitente.Institucion.Contains(baParams.InstitucionRemitenteBusqAv)) &&
                         (string.IsNullOrEmpty(baParams.NumeroOficioCMNBusqAv) || d.NumeroDespacho == baParams.NumeroOficioCMNBusqAv) &&
                         (!baParams.FechaEmisionOficioCMNBusqAv.HasValue || (d.FechaEmisionOficio >= baParams.FechaEmisionOficioCMNBusqAv && d.FechaEmisionOficio < fechaEmisionOficioH)) &&
                         (string.IsNullOrEmpty(baParams.MateriaBusqAv) || d.Materia.Contains(baParams.MateriaBusqAv)) &&
                         //(!baParams.EtiquetaBusqAv.HasValue || d.Etiqueta.Contains(bpes)) &&
                         (string.IsNullOrEmpty(baParams.FormaLlegadaBusqAv) || d.MedioDespachoCod == baParams.FormaLlegadaBusqAv) &&
                         (!baParams.EstadoDespachoBusqAv.HasValue || d.EstadoId == baParams.EstadoDespachoBusqAv)
                    );
                }

            ICollection<RequerimientoDespachoDto> reqdesp = new Collection<RequerimientoDespachoDto>();

            if (reqs != null)
                foreach (Requerimiento req in reqs)
                {
                    foreach (Despacho desp in req.Despacho.ToList())
                    {
                        reqdesp.Add(new RequerimientoDespachoDto()
                        {
                            Id = req.Id,
                            DespachoId = desp.Id,
                            DocumentoIngreso = req.DocumentoIngreso,
                            FechaIngreso = req.FechaIngreso,
                            NumeroDespacho = desp.NumeroDespacho,
                            FechaEmisionOficio = desp.FechaEmisionOficio,
                            DestinatarioNombre = desp.Remitente != null ? desp.Remitente.Nombre : "",
                            DestinatarioInstitucion = desp.Remitente != null ? desp.Remitente.Institucion : "",
                            MateriaOficioCMN = desp.Materia,
                            RegionTitulo = req.MonumentoNacional != null ? string.Join("; ", req.MonumentoNacional.Region.Select(s => s.Titulo)) : "",
                            ComunaTitulo = req.MonumentoNacional != null ? string.Join("; ", req.MonumentoNacional.Comuna.Select(s => s.Titulo)) : "",
                            Etiqueta = string.Join("; ", req.Etiqueta.Select(s => s.Titulo)),
                            UtAsignadaTitulo = req.UnidadTecnicaAsign != null ? req.UnidadTecnicaAsign.Titulo : "",
                            ProfesionalNombre = req.ProfesionalId != null ? usuarios.Where(x => x.Id == req.ProfesionalId.Value).FirstOrDefault().NombresApellidos : "",
                            TiposAdjuntos = desp.TipoAdjunto.Select(ta => ta.Titulo),
                            TipoAdjuntoTitulos = string.Join("; ", desp.TipoAdjunto.Select(ta => ta.Titulo)),
                            CantidadAdjuntos = desp.CantidadAdjuntos,
                            MedioDespachoTitulo = desp.FormaLlegada != null ? desp.FormaLlegada.Titulo : "",
                            FechaRecepcion = desp.FechaRecepcion
                        });
                    }
                }

            if (desps != null)
                foreach (Despacho desp in desps)
                {
                    foreach (Requerimiento req in desp.Requerimiento)
                    {
                        if (!reqdesp.Any(x => x.Id == req.Id && x.DespachoId == desp.Id))
                            reqdesp.Add(new RequerimientoDespachoDto()
                            {
                                Id = req.Id,
                                DespachoId = desp.Id,
                                DocumentoIngreso = req.DocumentoIngreso,
                                FechaIngreso = req.FechaIngreso,
                                NumeroDespacho = desp.NumeroDespacho,
                                FechaEmisionOficio = desp.FechaEmisionOficio,
                                DestinatarioNombre = desp.Remitente != null ? desp.Remitente.Nombre : "",
                                DestinatarioInstitucion = desp.Remitente != null ? desp.Remitente.Institucion : "",
                                MateriaOficioCMN = desp.Materia,
                                RegionTitulo = req.MonumentoNacional != null ? string.Join("; ", req.MonumentoNacional.Region.Select(s => s.Titulo)) : "",
                                ComunaTitulo = req.MonumentoNacional != null ? string.Join("; ", req.MonumentoNacional.Comuna.Select(s => s.Titulo)) : "",
                                Etiqueta = string.Join("; ", req.Etiqueta.Select(s => s.Titulo)),
                                UtAsignadaTitulo = req.UnidadTecnicaAsign != null ? req.UnidadTecnicaAsign.Titulo : "",
                                ProfesionalNombre = req.ProfesionalId != null ? usuarios.Where(x => x.Id == req.ProfesionalId.Value).FirstOrDefault().NombresApellidos : "",
                                TiposAdjuntos = desp.TipoAdjunto.Select(ta => ta.Titulo),
                                CantidadAdjuntos = req.Adjunto.Count, // desp.CantidadAdjuntos,
                                TipoAdjuntoTitulos = string.Join("; ", desp.TipoAdjunto.Select(ta => ta.Titulo)),
                                MedioDespachoTitulo = desp.FormaLlegada != null ? desp.FormaLlegada.Titulo : "",
                                FechaRecepcion = desp.FechaRecepcion
                            });
                    }
                }

            return reqdesp;
        }

        private int QueryCountDespacho(BusquedaAvanzadaModel baParams)
        {
            //rangos fechas
            DateTime? fechaIngresoH = null;
            DateTime? fechaDocumentoIngresoH = null;
            DateTime? fechaEmisionOficioH = null;
            if (baParams.FechaIngresoBusqAv.HasValue) fechaIngresoH = baParams.FechaIngresoBusqAv.Value.Date.AddDays(1);
            if (baParams.FechaDocumentoIngresoBusqAv.HasValue) fechaDocumentoIngresoH = baParams.FechaDocumentoIngresoBusqAv.Value.Date.AddDays(1);
            if (baParams.FechaEmisionOficioCMNBusqAv.HasValue) fechaEmisionOficioH = baParams.FechaEmisionOficioCMNBusqAv.Value.Date.AddDays(1);

            var tieneFiltroRequerimientos = baParams.FechaIngresoBusqAv.HasValue
                                            || baParams.DocumentoIngresoIdBusqAv.HasValue
                                            || baParams.FechaDocumentoIngresoBusqAv.HasValue
                                            //|| !string.IsNullOrEmpty(baParams.MateriaBusqAv)
                                            || !string.IsNullOrEmpty(baParams.RegionBusqAv)
                                            || !string.IsNullOrEmpty(baParams.ComunaBusqAv)
                                            || !string.IsNullOrEmpty(baParams.EtiquetaBusqAv)
                                            || !string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv)
                                            || !string.IsNullOrEmpty(baParams.CategoriaMonumentoNacionalBusqAv)
                                            || baParams.UnidadTecnicaAsignadaBusqAv.HasValue
                                            || baParams.ProfesionalUTAsignadoBusqAv.HasValue;

            var tieneFiltroDesp = baParams.RemitenteIdBusqAv.HasValue
                                    || !string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv)
                                    || !string.IsNullOrEmpty(baParams.NumeroOficioCMNBusqAv)
                                    || baParams.FechaEmisionOficioCMNBusqAv.HasValue
                                    || !string.IsNullOrEmpty(baParams.MateriaBusqAv)
                                    //|| baParams.EtiquetaBusqAv.HasValue
                                    || !string.IsNullOrEmpty(baParams.FormaLlegadaBusqAv)
                                    || baParams.EstadoDespachoBusqAv.HasValue;

            IQueryable<Requerimiento> reqs = null;
            if (tieneFiltroRequerimientos)
            {
                reqs = db.Requerimiento.Where(r => !r.Eliminado && (!baParams.FechaIngresoBusqAv.HasValue || (r.FechaIngreso >= baParams.FechaIngresoBusqAv && r.FechaIngreso < fechaIngresoH)) &&
                        (!baParams.DocumentoIngresoIdBusqAv.HasValue || r.Id == baParams.DocumentoIngresoIdBusqAv) &&
                        (!baParams.FechaDocumentoIngresoBusqAv.HasValue || (r.FechaDocumento >= baParams.FechaDocumentoIngresoBusqAv && r.FechaDocumento < fechaDocumentoIngresoH)) &&
                        //(string.IsNullOrEmpty(baParams.MateriaBusqAv) || r.Materia.Contains(baParams.MateriaBusqAv)) &&
                        (string.IsNullOrEmpty(baParams.EtiquetaBusqAv) || r.Etiqueta.Any(a => a.Titulo.Contains(baParams.EtiquetaBusqAv))) &&
                        (string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv) || r.MonumentoNacional.DenominacionOficial.Contains(baParams.DenominacionOficialBusqAv)) &&
                        (string.IsNullOrEmpty(baParams.RegionBusqAv) || r.MonumentoNacional.Region.Any(rg => rg.Codigo == baParams.RegionBusqAv)) &&
                        (string.IsNullOrEmpty(baParams.ComunaBusqAv) || r.MonumentoNacional.Comuna.Any(co => co.Codigo == baParams.ComunaBusqAv)) &&
                        (string.IsNullOrEmpty(baParams.CategoriaMonumentoNacionalBusqAv) || r.MonumentoNacional.CategoriaMonumentoNac.Any(n => n.Codigo == baParams.CategoriaMonumentoNacionalBusqAv)) &&
                        (!baParams.UnidadTecnicaAsignadaBusqAv.HasValue || r.UtAsignadaId == baParams.UnidadTecnicaAsignadaBusqAv) &&
                        (!baParams.ProfesionalUTAsignadoBusqAv.HasValue || r.ProfesionalId == baParams.ProfesionalUTAsignadoBusqAv) &&
                        (!tieneFiltroDesp || (tieneFiltroDesp &&
                            r.Despacho.Any(d => !d.Eliminado && (!baParams.RemitenteIdBusqAv.HasValue || d.DestinatarioId == baParams.RemitenteIdBusqAv) &&
                                                (string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv) || d.Remitente.Institucion.Contains(baParams.InstitucionRemitenteBusqAv)) &&
                                                (string.IsNullOrEmpty(baParams.NumeroOficioCMNBusqAv) || d.NumeroDespacho == baParams.NumeroOficioCMNBusqAv) &&
                                                (!baParams.FechaEmisionOficioCMNBusqAv.HasValue || (d.FechaEmisionOficio >= baParams.FechaEmisionOficioCMNBusqAv && d.FechaEmisionOficio < fechaEmisionOficioH)) &&
                                                (string.IsNullOrEmpty(baParams.MateriaBusqAv) || d.Materia.Contains(baParams.MateriaBusqAv)) &&
                                                //(!baParams.EtiquetaBusqAv.HasValue || d.Etiqueta.Contains(bpes)) &&
                                                (string.IsNullOrEmpty(baParams.FormaLlegadaBusqAv) || d.MedioDespachoCod == baParams.FormaLlegadaBusqAv) &&
                                                (!baParams.EstadoDespachoBusqAv.HasValue || d.EstadoId == baParams.EstadoDespachoBusqAv))))
                    );
                //reqs = db.Requerimiento.Where(r => (!baParams.FechaIngresoBusqAv.HasValue || (r.FechaIngreso >= baParams.FechaIngresoBusqAv && r.FechaIngreso < fechaIngresoH)) &&
                //        (!baParams.DocumentoIngresoIdBusqAv.HasValue || r.Id == baParams.DocumentoIngresoIdBusqAv) &&
                //        (!baParams.FechaDocumentoIngresoBusqAv.HasValue || (r.FechaDocumento >= baParams.FechaDocumentoIngresoBusqAv && r.FechaDocumento < fechaDocumentoIngresoH)) &&
                //        //(string.IsNullOrEmpty(baParams.MateriaBusqAv) || r.Materia.Contains(baParams.MateriaBusqAv)) &&
                //        (string.IsNullOrEmpty(baParams.EtiquetaBusqAv) || r.Etiqueta.Any(a => a.Titulo.Contains(bpes))) &&
                //        (string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv) || r.MonumentoNacional.DenominacionOficial.Contains(baParams.DenominacionOficialBusqAv)) &&
                //        (!baParams.RegionBusqAv.HasValue || r.MonumentoNacional.Region.Any(rg => rg.Id == baParams.RegionBusqAv.Value)) &&
                //        (!baParams.ComunaBusqAv.HasValue || r.MonumentoNacional.Comuna.Any(co => co.Id == baParams.ComunaBusqAv.Value)) &&
                //        (string.IsNullOrEmpty(baParams.CategoriaMonumentoNacionalBusqAv) || r.MonumentoNacional.CategoriaMonumentoNac.Any(n => n.Codigo == baParams.CategoriaMonumentoNacionalBusqAv)) &&
                //        (!baParams.UnidadTecnicaAsignadaBusqAv.HasValue || r.UtAsignadaId == baParams.UnidadTecnicaAsignadaBusqAv) &&
                //        (!baParams.ProfesionalUTAsignadoBusqAv.HasValue || r.ProfesionalId == baParams.ProfesionalUTAsignadoBusqAv) &&
                //        (!tieneFiltroDesp || (tieneFiltroDesp &&
                //            r.Despacho.Where(d => (!baParams.RemitenteIdBusqAv.HasValue || d.DestinatarioId == baParams.RemitenteIdBusqAv) &&
                //             (string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv) || d.Remitente.Institucion.Contains(baParams.InstitucionRemitenteBusqAv)) &&
                //             (string.IsNullOrEmpty(baParams.NumeroOficioCMNBusqAv) || d.NumeroDespacho == baParams.NumeroOficioCMNBusqAv) &&
                //             (!baParams.FechaEmisionOficioCMNBusqAv.HasValue || (d.FechaEmisionOficio >= baParams.FechaEmisionOficioCMNBusqAv && d.FechaEmisionOficio < fechaEmisionOficioH)) &&
                //             (string.IsNullOrEmpty(baParams.MateriaBusqAv) || d.Materia.Contains(baParams.MateriaBusqAv)) &&
                //             //(!baParams.EtiquetaBusqAv.HasValue || d.Etiqueta.Contains(bpes)) &&
                //             (string.IsNullOrEmpty(baParams.FormaLlegadaBusqAv) || d.MedioDespachoCod == baParams.FormaLlegadaBusqAv) &&
                //             (!baParams.EstadoDespachoBusqAv.HasValue || d.EstadoId == baParams.EstadoDespachoBusqAv)
                //        ).Count() > 0))
                //    );
            }

            IQueryable<Despacho> desps = null;
            if (!tieneFiltroRequerimientos)
                if (tieneFiltroDesp)
                {
                    desps = db.Despacho.Where(d => !d.Eliminado && (!baParams.RemitenteIdBusqAv.HasValue || d.DestinatarioId == baParams.RemitenteIdBusqAv) &&
                             (string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv) || d.Remitente.Institucion.Contains(baParams.InstitucionRemitenteBusqAv)) &&
                             (string.IsNullOrEmpty(baParams.NumeroOficioCMNBusqAv) || d.NumeroDespacho == baParams.NumeroOficioCMNBusqAv) &&
                             (!baParams.FechaEmisionOficioCMNBusqAv.HasValue || (d.FechaEmisionOficio >= baParams.FechaEmisionOficioCMNBusqAv && d.FechaEmisionOficio < fechaEmisionOficioH)) &&
                             (string.IsNullOrEmpty(baParams.MateriaBusqAv) || d.Materia.Contains(baParams.MateriaBusqAv)) &&
                             //(!baParams.EtiquetaBusqAv.HasValue || d.Etiqueta.Contains(bpes)) &&
                             (string.IsNullOrEmpty(baParams.FormaLlegadaBusqAv) || d.MedioDespachoCod == baParams.FormaLlegadaBusqAv) &&
                             (!baParams.EstadoDespachoBusqAv.HasValue || d.EstadoId == baParams.EstadoDespachoBusqAv)
                        );
                }

            var total = reqs != null && reqs.Count() > 0 ? reqs.Sum(c => c.Despacho.Count) : 0;
            total += desps != null && desps.Count() > 0 ? desps.Sum(d => d.Requerimiento.Count) : 0;

            return total;
        }

        private IQueryable<DespachoIniciativa> QueryDespachoIniciativa(BusquedaAvanzadaModel baParams)
        {
            //rangos fechas
            DateTime? fechaEmisionOficioH = null;
            if (baParams.FechaEmisionOficioCMNBusqAv.HasValue) fechaEmisionOficioH = baParams.FechaEmisionOficioCMNBusqAv.Value.Date.AddDays(1);

            return db.DespachoIniciativa.Where(x => !x.Eliminado && (!baParams.RemitenteIdBusqAv.HasValue || x.DestinatarioId == baParams.RemitenteIdBusqAv) &&
                (string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv) || x.Remitente.Institucion.Contains(baParams.InstitucionRemitenteBusqAv)) &&
                (string.IsNullOrEmpty(baParams.NumeroOficioCMNBusqAv) || x.NumeroDespacho.Contains(baParams.NumeroOficioCMNBusqAv)) &&
                (!baParams.FechaEmisionOficioCMNBusqAv.HasValue || (x.FechaEmisionOficio >= baParams.FechaEmisionOficioCMNBusqAv && x.FechaEmisionOficio < fechaEmisionOficioH)) &&
                (string.IsNullOrEmpty(baParams.MateriaBusqAv) || x.Materia.Contains(baParams.MateriaBusqAv)) &&
                (string.IsNullOrEmpty(baParams.RegionBusqAv) || x.MonumentoNacional.Region.Any(a => a.Codigo == baParams.RegionBusqAv)) &&
                (string.IsNullOrEmpty(baParams.ComunaBusqAv) || x.MonumentoNacional.Comuna.Any(a => a.Codigo == baParams.ComunaBusqAv)) &&
                (string.IsNullOrEmpty(baParams.EtiquetaBusqAv) || x.Etiqueta.Any(a => a.Codigo == baParams.EtiquetaBusqAv)) &&
                (string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv) || x.MonumentoNacional.DenominacionOficial.Contains(baParams.DenominacionOficialBusqAv)) &&
                (string.IsNullOrEmpty(baParams.CategoriaMonumentoNacionalBusqAv) || x.MonumentoNacional.CategoriaMonumentoNac.Any(n => n.Codigo == baParams.CategoriaMonumentoNacionalBusqAv)) &&
                (string.IsNullOrEmpty(baParams.RolSIIPropiedadBusqAv) || x.MonumentoNacional.RolSii.Contains(baParams.DenominacionOficialBusqAv)) &&
                (!baParams.UnidadTecnicaAsignadaBusqAv.HasValue || x.UtAsignadaId == baParams.UnidadTecnicaAsignadaBusqAv) &&
                (!baParams.ProfesionalUTAsignadoBusqAv.HasValue || x.ProfesionalId == baParams.ProfesionalUTAsignadoBusqAv) &&
                (string.IsNullOrEmpty(baParams.FormaLlegadaBusqAv) || x.MedioDespachoCod == baParams.FormaLlegadaBusqAv) &&
                (!baParams.EstadoDespachoBusqAv.HasValue || x.EstadoId == baParams.EstadoDespachoBusqAv)
            );
        }

        private IQueryable<Bitacora> QueryBitacora(BusquedaAvanzadaModel baParams)
        {
            //rangos fechas
            DateTime? fechaBitacoraH = null;
            DateTime? fechaIngresoH = null;
            if (baParams.FechaBitacoraBusqAv.HasValue)
            {
                fechaBitacoraH = baParams.FechaBitacoraBusqAv.Value.Date.AddDays(1);
            }
            else if (baParams.FechaIngresoBusqAv.HasValue) //-> si es que viene con fecha de ingreso desde la búsqueda TODOS
            {
                fechaIngresoH = baParams.FechaIngresoBusqAv.Value.Date.AddDays(1);
            }

            return db.Bitacora.Where(x => !x.Eliminado && (string.IsNullOrEmpty(baParams.TipoBitacoraBusqAv) || x.TipoBitacoraCod == baParams.TipoBitacoraBusqAv) &&
                (!baParams.DocumentoIngresoIdBusqAv.HasValue || x.RequerimientoId == baParams.DocumentoIngresoIdBusqAv) &&
                (!baParams.RequerimientoAnteriorIdBusqAv.HasValue || x.Requerimiento.RequerimientoAnteriorId == baParams.RequerimientoAnteriorIdBusqAv) &&
                (!baParams.RemitenteIdBusqAv.HasValue || x.Requerimiento.RemitenteId == baParams.RemitenteIdBusqAv) &&
                (string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv) || x.Requerimiento.Remitente.Institucion.Contains(baParams.InstitucionRemitenteBusqAv)) &&
                (string.IsNullOrEmpty(baParams.CargoProfesionRemitenteBusqAv) || x.Requerimiento.Remitente.Cargo.Contains(baParams.CargoProfesionRemitenteBusqAv)) &&
                (string.IsNullOrEmpty(baParams.MateriaBusqAv) || x.Requerimiento.Materia.Contains(baParams.MateriaBusqAv)) &&
                (string.IsNullOrEmpty(baParams.EtiquetaBusqAv) || x.Requerimiento.Etiqueta.Any(a => a.Codigo == baParams.EtiquetaBusqAv)) &&
                (string.IsNullOrEmpty(baParams.RegionBusqAv) || x.Requerimiento.MonumentoNacional.Region.Any(a => a.Codigo == baParams.RegionBusqAv)) &&
                (string.IsNullOrEmpty(baParams.ComunaBusqAv) || x.Requerimiento.MonumentoNacional.Comuna.Any(a => a.Codigo == baParams.ComunaBusqAv)) &&
                (string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv) || x.Requerimiento.MonumentoNacional.DenominacionOficial.Contains(baParams.DenominacionOficialBusqAv)) &&
                (!baParams.FechaBitacoraBusqAv.HasValue || (x.Fecha >= baParams.FechaBitacoraBusqAv && x.Fecha < fechaBitacoraH)) && //-> Si viene por la búsqueda BITACORA podría venir esta fecha pero no la de ingreso
                (!baParams.FechaIngresoBusqAv.HasValue || (x.Requerimiento.FechaIngreso >= baParams.FechaIngresoBusqAv && x.Requerimiento.FechaIngreso < fechaIngresoH)) && //-> Si viene por la búsqueda TODOS, podría venir esta fecha pero no la de bitácora
                (string.IsNullOrEmpty(baParams.RolSIIPropiedadBusqAv) || x.Requerimiento.MonumentoNacional.RolSii.Contains(baParams.RolSIIPropiedadBusqAv)) &&
                (!baParams.UnidadTecnicaAsignadaBusqAv.HasValue || x.Requerimiento.UtAsignadaId == baParams.UnidadTecnicaAsignadaBusqAv) &&
                (!baParams.ProfesionalUTAsignadoBusqAv.HasValue || x.Requerimiento.ProfesionalId == baParams.ProfesionalUTAsignadoBusqAv) &&
                (!baParams.CreadorBitacoraBusqAv.HasValue || x.UsuarioCreacionId == baParams.CreadorBitacoraBusqAv) &&
                (!baParams.EstadoBusqAv.HasValue || x.Requerimiento.EstadoId == baParams.EstadoBusqAv) &&
                (string.IsNullOrEmpty(baParams.ObservacionAcuerdoComentarioBusqAv) || x.ObsAcuerdoComentario == baParams.ObservacionAcuerdoComentarioBusqAv)
            );
        }

        private bool TieneFiltroIngreso(BusquedaAvanzadaModel baParams)
        {
            return baParams.FechaIngresoBusqAv.HasValue
                || baParams.DocumentoIngresoIdBusqAv.HasValue
                || baParams.FechaDocumentoIngresoBusqAv.HasValue
                || baParams.RequerimientoAnteriorIdBusqAv.HasValue
                || baParams.RemitenteIdBusqAv.HasValue
                || !string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv)
                || !string.IsNullOrEmpty(baParams.CargoProfesionRemitenteBusqAv)
                || !string.IsNullOrEmpty(baParams.MateriaBusqAv)
                || !string.IsNullOrEmpty(baParams.RegionBusqAv)
                || !string.IsNullOrEmpty(baParams.ComunaBusqAv)
                || !string.IsNullOrEmpty(baParams.EtiquetaBusqAv)
                || !string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv)
                || !string.IsNullOrEmpty(baParams.CategoriaMonumentoNacionalBusqAv)
                || !string.IsNullOrEmpty(baParams.RolSIIPropiedadBusqAv)
                || baParams.UnidadTecnicaAsignadaBusqAv.HasValue
                || baParams.ProfesionalUTAsignadoBusqAv.HasValue
                || baParams.EstadoBusqAv.HasValue;
        }

        private bool TieneFiltroDespacho(BusquedaAvanzadaModel baParams)
        {
            return baParams.FechaIngresoBusqAv.HasValue
                || baParams.DocumentoIngresoIdBusqAv.HasValue
                || baParams.FechaDocumentoIngresoBusqAv.HasValue
                || baParams.RemitenteIdBusqAv.HasValue
                || !string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv)
                || !string.IsNullOrEmpty(baParams.NumeroOficioCMNBusqAv)
                || baParams.FechaEmisionOficioCMNBusqAv.HasValue
                || !string.IsNullOrEmpty(baParams.MateriaBusqAv)
                || !string.IsNullOrEmpty(baParams.RegionBusqAv)
                || !string.IsNullOrEmpty(baParams.ComunaBusqAv)
                || !string.IsNullOrEmpty(baParams.EtiquetaBusqAv)
                || !string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv)
                || !string.IsNullOrEmpty(baParams.CategoriaMonumentoNacionalBusqAv)
                || !string.IsNullOrEmpty(baParams.RolSIIPropiedadBusqAv)
                || baParams.UnidadTecnicaAsignadaBusqAv.HasValue
                || baParams.ProfesionalUTAsignadoBusqAv.HasValue
                || !string.IsNullOrEmpty(baParams.FormaLlegadaBusqAv)
                || baParams.EstadoDespachoBusqAv.HasValue;
        }

        private bool TieneFiltroDespachoIniciativa(BusquedaAvanzadaModel baParams)
        {
            return baParams.RemitenteIdBusqAv.HasValue
                || !string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv)
                || !string.IsNullOrEmpty(baParams.NumeroOficioCMNBusqAv)
                || baParams.FechaEmisionOficioCMNBusqAv.HasValue
                || !string.IsNullOrEmpty(baParams.MateriaBusqAv)
                || !string.IsNullOrEmpty(baParams.RegionBusqAv)
                || !string.IsNullOrEmpty(baParams.ComunaBusqAv)
                || !string.IsNullOrEmpty(baParams.EtiquetaBusqAv)
                || !string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv)
                || !string.IsNullOrEmpty(baParams.CategoriaMonumentoNacionalBusqAv)
                || !string.IsNullOrEmpty(baParams.RolSIIPropiedadBusqAv)
                || baParams.UnidadTecnicaAsignadaBusqAv.HasValue
                || baParams.ProfesionalUTAsignadoBusqAv.HasValue
                || !string.IsNullOrEmpty(baParams.FormaLlegadaBusqAv)
                || baParams.EstadoDespachoBusqAv.HasValue;
        }

        private bool TieneFiltroBitacora(BusquedaAvanzadaModel baParams)
        {
            return !string.IsNullOrEmpty(baParams.TipoBitacoraBusqAv)
                || baParams.DocumentoIngresoIdBusqAv.HasValue
                || baParams.RequerimientoAnteriorIdBusqAv.HasValue
                || baParams.RemitenteIdBusqAv.HasValue
                || !string.IsNullOrEmpty(baParams.InstitucionRemitenteBusqAv)
                || !string.IsNullOrEmpty(baParams.CargoProfesionRemitenteBusqAv)
                || !string.IsNullOrEmpty(baParams.MateriaBusqAv)
                || !string.IsNullOrEmpty(baParams.DenominacionOficialBusqAv)
                || !string.IsNullOrEmpty(baParams.RegionBusqAv)
                || !string.IsNullOrEmpty(baParams.ComunaBusqAv)
                || !string.IsNullOrEmpty(baParams.EtiquetaBusqAv)
                || baParams.FechaIngresoBusqAv.HasValue //-> si es que viene con fecha de ingreso desde la búsqueda TODOS
                || baParams.FechaBitacoraBusqAv.HasValue
                || !string.IsNullOrEmpty(baParams.RolSIIPropiedadBusqAv)
                || baParams.UnidadTecnicaAsignadaBusqAv.HasValue
                || baParams.ProfesionalUTAsignadoBusqAv.HasValue
                || baParams.CreadorBitacoraBusqAv.HasValue
                || !string.IsNullOrEmpty(baParams.ObservacionAcuerdoComentarioBusqAv)
                || baParams.EstadoBusqAv.HasValue;
        }
    }
}