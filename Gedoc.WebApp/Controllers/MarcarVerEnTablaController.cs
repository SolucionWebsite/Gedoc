using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;
using Gedoc.Service.DataAccess;
using Gedoc.WebApp.Models;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers.Enum;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;

namespace Gedoc.WebApp.Controllers
{
    public class MarcarVerEnTablaController : BaseController
    {
        // GET: VerEnTabla
        [Authorize]
        public ActionResult Index()
        {
            var unidadTecnicaList = new SesionTablaService().GetUnidadTecnicaList()
                .Select(a => new DropDownListItem()
                {
                    Text = a.Text,
                    Value = a.Value
                });

            var estadoList = new List<DropDownListItem>
            {
                new DropDownListItem()
                {
                    Text = "[TODOS]",
                    Value = "-1",
                    //Selected = true,
                }
            };
            estadoList.AddRange(new SesionTablaService().GetEstadosList()
                .Select(a => new DropDownListItem()
                {
                    Text = a.Text,
                    Value = a.Value
                }));

            var etiquetaList = new SesionTablaService().GetEtiquetasList()
                .Select(a => new DropDownListItem()
                {
                    Text = a.Text,
                    Value = a.Value
                });

            var etapaList = new SesionTablaService().GetEtapasList()
                .Select(a => new DropDownListItem()
                {
                    Text = a.Text,
                    Value = a.Value
                });

            ViewBag.UnidadTecnicaList = unidadTecnicaList;
            ViewBag.EstadoList = estadoList;
            ViewBag.EtiquetaList = etiquetaList;
            ViewBag.EtapaList = etapaList;
            return View();
        }

        public JsonResult GetNombreTabla(int unidadTecnicaId)
        {
            var sesionTabla = new SesionTablaService().GetSesionTabla(unidadTecnicaId);
            return Json(sesionTabla, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult CreateSesionTabla(string nombreTabla, int unidadTecnicaId)
        {
            int iduser;
            int.TryParse(Session["IdUsuario"]?.ToString(), out iduser);
            var sesionTablaId = new SesionTablaService().CreateSesionTabla(iduser, nombreTabla, unidadTecnicaId);
            return Json(sesionTablaId, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDataGrilla([DataSourceRequest]DataSourceRequest request, MarcarVerEnTablaModel extraData)
        {
            //var datagrilla = new List<RequerimientoModel>();
            //datagrilla = new SesionTablaService().GetRequerimientos(extraData.UnidadTecnicaId, extraData.FechaDesde, 
            //    extraData.FechaHasta, extraData.EstadoId, extraData.Etapa, extraData.Etiqueta,extraData.DocumentoDeIngreso, extraData.Materia)
            //    .Select(a=> new RequerimientoModel() { 
            //        Id = a.Id,
            //        DocumentoIngreso = a.DocumentoIngreso,
            //        FechaIngreso = a.FechaIngreso,
            //        RemitenteNombre = a.RemitenteNombre,
            //        FechaUltAcuerdoComision = a.FechaUltAcuerdoComision,
            //        FechaUltAcuerdoSesion = a.FechaUltAcuerdoSesion,
            //        Materia = a.Materia,                    
            //    }).ToList();


            using (var db = new GedocEntities())
            {
                if (extraData.IsFromTabla)
                {
                    int sesiontablaid = 0;
                    int.TryParse(extraData.NombreTabla, out sesiontablaid);

                    var tipoBitaCom = TipoBitacora.AcuerdoComision.ToString("D");
                    var tipoBitaSes = TipoBitacora.AcuerdoSesion.ToString("D");
                    var result = db.Requerimiento
                        .Where(a => a.SesionTablaDetalle.Any(b => b.SesionTablaId == sesiontablaid))
                        .Select(a => new RequerimientoModel()
                        {
                            Id = a.Id,
                            DocumentoIngreso = a.DocumentoIngreso,
                            FechaIngreso = a.FechaIngreso,
                            RemitenteNombre = a.Remitente.Nombre, //puede que esta relacion haga q la query demore mas tiempo
                            //RemitenteNombre = a.Remitente.Id.ToString(),
                            //FechaUltAcuerdoComision = a.FechaUltAcuerdoComision,
                            //FechaUltAcuerdoSesion = a.FechaUltAcuerdoSesion,
                            FechaUltAcuerdoComision = a.Bitacora.Where(b => b.TipoBitacoraCod == tipoBitaCom).OrderByDescending(c => c.Fecha).FirstOrDefault().Fecha,
                            FechaUltAcuerdoSesion = a.Bitacora.Where(b => b.TipoBitacoraCod == tipoBitaSes).OrderByDescending(c => c.Fecha).FirstOrDefault().Fecha,
                            Materia = a.Materia,
                        });
                    return Json(result.ToDataSourceResult(request));
                }
                else
                {
                    var preResult = BusquedaFiltros(db, extraData);

                    var result = preResult.Select(a => new RequerimientoModel()
                    {
                        Id = a.Id,
                        DocumentoIngreso = a.DocumentoIngreso,
                        FechaIngreso = a.FechaIngreso,
                        RemitenteNombre = a.Remitente.Nombre, //puede que esta relacion haga q la query demore mas tiempo
                        //RemitenteNombre = a.Remitente.Id.ToString(),
                        FechaUltAcuerdoComision = a.FechaUltAcuerdoComision,
                        FechaUltAcuerdoSesion = a.FechaUltAcuerdoSesion,
                        Materia = a.Materia,
                    });
                    //comentario: parece que esto(el filtrado y el paginado automatico) funciona correctamente solo cuando ocupas los objetos como iQueryables
                    return Json(result.ToDataSourceResult(request));
                }
            }
        }                    
                
        [HttpPost]
        public ActionResult MultiSelectAction(MUltiSelectModel extraData)
        {
            var result = new ResultadoOperacion();
            try
            {
                using (var db = new GedocEntities())
                {
                    if (extraData.IsFromTabla)
                    {
                        if (extraData.Ids != null && extraData.Ids.Any())
                        {
                            var tablaId = int.Parse(extraData.NombreTabla);
                            var seleccionActual = extraData.Ids.Select(a => int.Parse(a));

                            //1ro agregamos los que no existen.
                            var yaExisten = db.SesionTablaDetalle.Where(a => a.SesionTablaId == tablaId).Select(a => a.RequerimentoId);
                            foreach (var id in seleccionActual.Where(a => !yaExisten.Contains(a)))
                            {
                                db.SesionTablaDetalle.Add(new SesionTablaDetalle()
                                {
                                    //DocumentoIngreso = extraData.DocIngreso,
                                    RequerimentoId = id,
                                    SesionTablaId = tablaId
                                });
                            }
                            db.SaveChanges();

                            //2do eliminamos todos los que no estan en la seleccion actual                           
                            var fordelete = db.SesionTablaDetalle.Where(a => a.SesionTablaId == tablaId && !seleccionActual.Contains(a.RequerimentoId));
                            if (fordelete.Any())
                            {
                                db.SesionTablaDetalle.RemoveRange(fordelete);
                                db.SaveChanges();
                            }

                            result.Codigo = 1;
                            result.Mensaje = "OK";
                        }
                        else
                        {
                            var tablaId = int.Parse(extraData.NombreTabla);
                            var fordelete = db.SesionTablaDetalle.Where(a => a.SesionTablaId == tablaId);
                            if (fordelete.Any())
                            {
                                db.SesionTablaDetalle.RemoveRange(fordelete);
                                db.SaveChanges();

                                //var yaExisten = db.SesionTablaDetalle.Where(a => a.SesionTablaId == tablaId).Select(a => a.RequerimentoId);
                                result.Codigo = 1;
                                result.Mensaje = "OK";
                                // result.Extra = yaExisten.Select(a => a.ToString()).ToArray();
                            }
                        }
                    }
                    else
                    {
                        if (extraData.Ids != null && extraData.Ids.Any())
                        {
                            var tablaId = int.Parse(extraData.NombreTabla);
                            var seleccionActual = extraData.Ids.Select(a => int.Parse(a));

                            //1ro agregamos los que no existen.
                            var yaExisten = db.SesionTablaDetalle.Where(a => a.SesionTablaId == tablaId).Select(a => a.RequerimentoId);
                            foreach (var id in seleccionActual.Where(a => !yaExisten.Contains(a)))
                            {
                                db.SesionTablaDetalle.Add(new SesionTablaDetalle()
                                {
                                    //DocumentoIngreso = extraData.DocIngreso,
                                    RequerimentoId = id,
                                    SesionTablaId = tablaId
                                });
                            }
                            db.SaveChanges();

                            //2do eliminamos todos los que no estan en la seleccion actual                           
                            var preResult = BusquedaFiltros(db, extraData);
                            var reqIds = preResult.Where(a=> !seleccionActual.Contains(a.Id)).Select(a => a.Id);
                            var fordelete = db.SesionTablaDetalle.Where(a => a.SesionTablaId == tablaId && reqIds.Contains(a.RequerimentoId));
                            if (fordelete.Any())
                            {
                                db.SesionTablaDetalle.RemoveRange(fordelete);
                                db.SaveChanges();
                            }                            
                            result.Codigo = 1;
                            result.Mensaje = "OK";                            
                        }
                        else 
                        {
                            var tablaId = int.Parse(extraData.NombreTabla);
                            var preResult = BusquedaFiltros(db, extraData);

                            var reqIds = preResult.Select(a => a.Id);
                            var fordelete = db.SesionTablaDetalle.Where(a => a.SesionTablaId == tablaId && reqIds.Contains(a.RequerimentoId));
                            if (fordelete.Any())
                            {
                                db.SesionTablaDetalle.RemoveRange(fordelete);
                                db.SaveChanges();
                                                                
                                result.Codigo = 1;
                                result.Mensaje = "OK";                               
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Codigo = -1;
                result.Mensaje = "FAIL";
                result.Extra = ex.Message;
            }
            return Json(result);
        }

        private IQueryable<Requerimiento> BusquedaFiltros(GedocEntities db, MarcarVerEnTablaModel extraData)
        {
            extraData.FechaHasta = extraData.FechaHasta.AddHours(23).AddMinutes(59).AddSeconds(59);

            extraData.Etapa = extraData.Etapa ?? new int[0];
            var ignoraEtapa = extraData.Etapa?.Length == 0;
            extraData.EstadoId = extraData.EstadoId ?? new int[0];
            var ignoraEstado = extraData.EstadoId?.Length == 0 || extraData.EstadoId.Contains(-1);
            var ignoraEtiqueta = (extraData.Etiqueta?.Length ?? 0) == 0;
            var etiqStr = (extraData.Etiqueta ?? new int[0]).Select(e => e.ToString()).ToList();
            extraData.DocumentoDeIngreso = extraData.DocumentoDeIngreso ?? new int[0];
            var ignoraDocIng = extraData.DocumentoDeIngreso?.Length == 0;
            var ignoraMateria = string.IsNullOrEmpty(extraData.Materia);
            var preResult = db.Requerimiento
                        .Where(a =>
                            (extraData.DocumentoDeIngreso.Contains(a.Id))
                            || (
                                a.UtAsignadaId == extraData.UnidadTecnicaId
                                && a.FechaIngreso >= extraData.FechaDesde
                                && a.FechaIngreso <= extraData.FechaHasta
                                && (ignoraEtapa || extraData.Etapa.Contains(a.EtapaId))
                                && (ignoraEstado || extraData.EstadoId.Contains(a.EstadoId))
                                && (ignoraEtiqueta || a.Etiqueta.Any(b => etiqStr.Contains(b.Codigo)))
                                && (ignoraMateria || a.Materia.Contains(extraData.Materia))
                            )
                        );

            //if (extraData.Etapa?.Length > 0)
            //    preResult = preResult.Where(a => extraData.Etapa.Contains(a.EtapaId));

            //if (extraData.EstadoId?.Length > 0 && !extraData.EstadoId.Contains(-1))
            //    preResult = preResult.Where(a => extraData.EstadoId.Contains(a.EstadoId));

            //if (extraData.Etiqueta?.Length > 0)
            //{
            //    var etiqStr = extraData.Etiqueta.Select(e => e.ToString()).ToList();
            //    preResult = preResult.Where(a => a.Etiqueta.Any(b => etiqStr.Contains(b.Codigo)));
            //}

            //if (extraData.DocumentoDeIngreso?.Length > 0)
            //    preResult = preResult.Where(a => extraData.DocumentoDeIngreso.Contains(a.Id));

            //if (!string.IsNullOrEmpty(extraData.Materia))
            //    preResult = preResult.Where(a => a.Materia.Contains(extraData.Materia));

            return preResult;
        }

        #region Virtualizacion Documentos Ingreso
        public ActionResult DocIngresoPaging([DataSourceRequest] DataSourceRequest param, int utId)
        {
            var db = new GedocEntities();
            if (param.Filters != null && param.Filters.Count > 0) ((FilterDescriptor)param.Filters[0]).Member = "DocumentoIngreso";
            var req = db.Requerimiento
                .Where(r => !r.Eliminado && r.UtAsignadaId == utId)
                .OrderBy(r => r.DocumentoIngreso)
                .ToList();
            DataSourceResult result = req.ToDataSourceResult(param, r => new
            {
                Id = r.Id,
                Titulo = r.DocumentoIngreso
            });

            return Json(result);
        }
          
        public ActionResult DocIngresoByIds(List<int> ids, bool cerrado)
        {
            var db = new GedocEntities();
            var query = db.Requerimiento.Where(r => ids.Contains(r.Id));
            var datos = query
                .OrderBy(r => r.DocumentoIngreso)
                .AsEnumerable()
                .Select(r => new GenericoDto()
                {
                    IdInt = r.Id,
                    Titulo = r.DocumentoIngreso
                }).ToList();
            return Json(datos);
        }

        //[HttpPost]
        //public JsonResult GetDocIngreso(int unidadTecnicaId, DateTime fechaDesde, DateTime fechaHasta)
        //{
        //    var result = new List<SelectListItemDto>();
        //    if (unidadTecnicaId != 0 && fechaDesde != null && fechaHasta != null)
        //    {
        //        result = new SesionTablaService().GetDocumentosIngresos(unidadTecnicaId, fechaDesde, fechaHasta);
        //    }
        //    return Json(result);
        //}
        #endregion


        public ActionResult Mantenedor()
        {
            var unidadTecnicaList = new List<DropDownListItem>
            {
                new DropDownListItem() { Text = "[TODOS]", Value = "-1" }
            };
            unidadTecnicaList
                .AddRange(new SesionTablaService().GetUnidadTecnicaList()
                .Select(a => new DropDownListItem()
                {
                    Text = a.Text,
                    Value = a.Value
                }));

            var usuariosList = new SesionTablaService().GetUsuariosTablas()
               .Select(a => new DropDownListItem()
               {
                   Text = a.Text,
                   Value = a.Value
               });

            ViewBag.UnidadTecnicaList = unidadTecnicaList;
            ViewBag.UsuariosList = usuariosList;

            return View();
        }

        public ActionResult GetDataGrillaMantenedor([DataSourceRequest] DataSourceRequest request, TablaSesionFilterModel extraData)
        {
            using (var db = new GedocEntities())
            {
                extraData.FechaHasta = extraData.FechaHasta.AddHours(23).AddMinutes(59).AddSeconds(59);
                var query = db.SesionTabla
                    .Where(a => a.FechaCreacion >= extraData.FechaDesde && a.FechaCreacion <= extraData.FechaHasta);
                if (extraData.UnidadTecnicaId != 0 && extraData.UnidadTecnicaId != -1)
                    query = query.Where(a => a.UnidadTecnicaId == extraData.UnidadTecnicaId);
                if (extraData.UsuarioCreadorId != 0)
                    query = query.Where(a => a.CreadoPorId == extraData.UsuarioCreadorId);

                var result = query.Select(a => new TablaSesionModel()
                {
                    Id = a.Id,
                    FechaCreacion = a.FechaCreacion,
                    CreadoPor = a.Usuario.NombresApellidos,
                    NombreTabla = a.Nombre,
                    UnidadTecnica = a.UnidadTecnica.Titulo
                });
                return Json(result.ToDataSourceResult(request));                              
            }
        }       
    

        [HttpPost]
        public ActionResult EliminarSeleccionados(int[] ids)
        {
            var result = new ResultadoOperacion();
            try
            {
                if(ids == null || ids.Length == 0)
                {
                    result.Codigo = -1;
                    result.Mensaje = "Seleccione un registro al menos.";
                }
                else 
                {
                    using (var db = new GedocEntities())
                    {
                        foreach(var id in ids)
                        {
                            var forDelete = db.SesionTabla.Find(id);
                            if (forDelete != null)
                            {
                                if (forDelete.SesionTablaDetalle.Count() > 0)
                                {
                                    db.SesionTablaDetalle.RemoveRange(forDelete.SesionTablaDetalle);
                                    forDelete.SesionTablaDetalle.Clear();
                                }
                                db.SesionTabla.Remove(forDelete);
                            }
                        }
                        db.SaveChanges();
                        result.Codigo = 1;
                        result.Mensaje = "Registros Eliminados.";                       
                    }   
                }
            }
            catch (Exception ex)
            {
                result.Codigo = -1;
                result.Mensaje = "Error al ejecutar la acción Eliminar.";
                result.Extra = ex.Message;
            }
            return Json(result);
        }
    }
}
