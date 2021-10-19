using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;
using Gedoc.Service.DataAccess;
using Gedoc.WebApp.Models;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Razor;
using Gedoc.Helpers;
using Gedoc.Helpers.Enum;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.Service.EmailService;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;

namespace Gedoc.WebApp.Controllers
{
    public class ProcesosMasivosController : BaseController
    {
        private readonly INotificacionService _notifSvc;
        private readonly IMantenedorService _mantSvc;
        private readonly int archivado = 15;
        private readonly int cerrado = 14;
        private readonly int ingresado = 8;

        public ProcesosMasivosController(NotificacionService notifSvc, MantenedorService mantSvc)
        {
            _notifSvc = notifSvc;
            _mantSvc = mantSvc;
        }

        // GET: ProcesosMasivos
        [Authorize]
        public ActionResult Index()
        {
            var unidadTecnicaList = new List<DropDownListItem>()
                {
                    new DropDownListItem(){Text = "[Todos]",Value = "-1"}
                };

            unidadTecnicaList.AddRange(_mantSvc.GetGenericoMatenedor(Mantenedor.UnidadTecnica).Data
                .Select(a => new DropDownListItem()
                {
                    Text = a.Titulo,
                    Value = a.Id.ToString()
                }).ToList());

            var etiquetaList = _mantSvc.GetGenericoMatenedor(Mantenedor.Etiqueta).Data
                .Select(a => new DropDownListItem()
                {
                    Text = a.Titulo,
                    Value = a.Id
                }).OrderBy(a => a.Text).ToList();

            var regionList = _mantSvc.GetGenericoMatenedor(Mantenedor.Region).Data
                        .Select(a => new DropDownListItem()
                        {
                            Text = a.Titulo,
                            Value = a.Id.ToString()
                        }).ToList();

            var catMonNacList = _mantSvc.GetGenericoMatenedor(Mantenedor.CategoriaMn).Data
              .Select(a => new DropDownListItem()
              {
                  Text = a.Titulo,
                  Value = a.Id.ToString()
              }).ToList();

            var accionesList = new List<DropDownListItem>
            {
                new DropDownListItem() { Text = "Asignar Unidad Técnica", Value = "1", Selected = true},
                new DropDownListItem() { Text = "Reasignar Unidad Técnica", Value = "2" },
                new DropDownListItem() { Text = "Asignar Profesional UT", Value = "3" },
                new DropDownListItem() { Text = "Reasignar Profesional UT", Value = "4" },
                new DropDownListItem() { Text = "Nuevo Despacho", Value = "5" },
                new DropDownListItem() { Text = "Modificar Etiqueta", Value = "6" },
                new DropDownListItem() { Text = "Abrir Ingresos", Value = "7" }
            };

            ViewBag.UnidadTecnicaList = unidadTecnicaList;
            ViewBag.EtiquetaList = etiquetaList;
            ViewBag.RegionList = regionList;
            ViewBag.CatMonNacList = catMonNacList;
            ViewBag.AccionesList = accionesList;

            return View();
        }

        public ActionResult GetDataGrilla([DataSourceRequest] DataSourceRequest request, ProcesoMasivoModel extraData)
        {
            using (var db = new GedocEntities())
            {
                var query = BusquedaFiltros(db, extraData);
                var preResult = query.Select(a => new RequerimientoModel()
                {
                    Id = a.Id,
                    DocumentoIngreso = a.DocumentoIngreso,
                    FechaIngreso = a.FechaIngreso,
                    RemitenteNombre = a.Remitente.Nombre, // a.RemitenteNombre,
                    RemitenteInstitucion = a.Remitente.Institucion, //a.RemitenteInstitucion,
                    Materia = a.Materia,
                    AsignacionUt = a.AsignacionUt,
                    Resolucion = a.Resolucion,
                    EstadoTitulo = a.EstadoRequerimiento.Titulo,
                    CategoriaMonumentoNacTitulo = a.MonumentoNacional.CategoriaMonumentoNac.FirstOrDefault().Titulo ?? "",
                    DenominacionOficial = a.MonumentoNacional.DenominacionOficial
                    //DiasResolucion = 99, //se cambio a la propiedad de la clase              
                });
                //return Json(ResultGrilla(request, preResult));
                return (Json(preResult.ToDataSourceResult(request)));
            }
        }


        private IQueryable<Requerimiento> BusquedaFiltros(GedocEntities db, ProcesoMasivoModel extraData)
        {
            if (extraData.FechaHasta != null)
                extraData.FechaHasta = extraData.FechaHasta?.AddHours(23).AddMinutes(59).AddSeconds(59);

            var preResult = db.Requerimiento.Where(a => a.EstadoId != archivado);

            //TODO: quizas hay q filtrar por la accion.
            switch (extraData.AccionEjecutar)
            {
                case 1: //Asignar UT
                    preResult = preResult.Where(a => a.UtAsignadaId == null);
                    break;
                case 2: //Reasignar UT
                    preResult = preResult.Where(a => a.UtAsignadaId != null);
                    preResult = preResult.Where(a => a.EstadoId != cerrado && a.EstadoId != ingresado);
                    break;
            }

            if (extraData.UnidadTecnica > 0)
                preResult = preResult.Where(a => a.UtAsignadaId == extraData.UnidadTecnica);
            if (extraData.FechaDesde != null)
                preResult = preResult.Where(a => a.FechaIngreso >= extraData.FechaDesde);
            if (extraData.FechaHasta != null)
                preResult = preResult.Where(a => a.FechaIngreso <= extraData.FechaHasta);
            if (extraData.DocumentoIngreso?.Length > 0)
                preResult = preResult.Where(a => extraData.DocumentoIngreso.Contains(a.Id));
            if (extraData.Estado > 0)
                preResult = preResult.Where(a => a.EstadoId == extraData.Estado);
            if (extraData.Etiqueta > 0)
            {
                var etiqStr = extraData.Etiqueta.ToString();
                preResult = preResult.Where(a => a.Etiqueta.Any(b => b.Codigo == etiqStr));
            }
            if (!string.IsNullOrWhiteSpace(extraData.Region))
                preResult = preResult.Where(a => a.MonumentoNacional.Region.Any(b => b.Codigo == extraData.Region));
            if (extraData.CatMonuNac > 0)
            {
                var catStr = extraData.CatMonuNac.ToString();
                preResult = preResult.Where(a =>
                    a.MonumentoNacional.CategoriaMonumentoNac.Any(b => b.Codigo == catStr));
            }
            if (extraData.ProfesionalUtAsig > 0)
                preResult = preResult.Where(a => a.ProfesionalId == extraData.ProfesionalUtAsig);

            return preResult;
        }

        public ActionResult GetProfesionalUt(ProcesoMasivoModel extraData)
        {
            var result = new List<DropDownListItem>();
            try
            {
                using (var db = new GedocEntities())
                {
                    var preresult = db.Usuario.Where(a => a.UnidadTecnicaIntegrante.Any());
                    if (extraData.UnidadTecnica != -1 && extraData.UnidadTecnica != 0)
                    {
                        preresult = preresult.Where(a => a.UnidadTecnicaIntegrante.Any(b => b.Id == extraData.UnidadTecnica));
                    }
                    result = preresult.Select(a => new DropDownListItem() { Text = a.NombresApellidos, Value = a.Id.ToString() }).ToList();
                }
            }
            catch (Exception)
            {
                result = new List<DropDownListItem>();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUnidadTecnica(ProcesoMasivoModel extraData)
        {
            var result = new List<DropDownListItem>();
            try
            {
                using (var db = new GedocEntities())
                {
                    var preresult = db.UnidadTecnica;
                    result = preresult.Select(a => new DropDownListItem() { Text = a.Titulo, Value = a.Id.ToString() }).ToList();
                }
            }
            catch (Exception)
            {
                result = new List<DropDownListItem>();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEstados(ProcesoMasivoModel extraData)
        {
            var result = new List<DropDownListItem>();
            try
            {
                using (var db = new GedocEntities())
                {
                    var preresult = db.EstadoRequerimiento.Where(a => a.Activo == true && a.Id != archivado);

                    if (extraData.AccionEjecutar == 2) //reasignar UT
                    {
                        preresult = preresult.Where(a => a.Id != cerrado && a.Id != ingresado);
                    }
                    result = preresult.Select(a => new DropDownListItem() { Text = a.Titulo, Value = a.Id.ToString() }).ToList();
                }
            }
            catch (Exception)
            {
                result = new List<DropDownListItem>();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EjecutarAccion(ProcesoMasivoModel extraData)
        {
            var result = new ResultadoOperacion();
            var textoAccion = "";
            var accionLog = "";
            var extraDataLog = "";
            var logSistemaProcMas = new List<LogSistemaDto>();
            try
            {
                if ((extraData.SeleccionGrilla?.Length ?? 0) <= 0)
                {
                    result.Codigo = -1;
                    result.Mensaje = "Debe seleccionar por lo menos un elemento en la Grilla.";
                    return Json(result);
                }
                else
                {
                    UnidadTecnica ut = null;
                    ListaValor etiqueta = null;
                    using (var db = new GedocEntities())
                    {
                        var reqdb = db.Requerimiento.Where(a => extraData.SeleccionGrilla.Contains(a.Id));
                        foreach (var item in reqdb)
                        {
                            switch (extraData.AccionEjecutar)
                            {
                                case 1: //Asignar UT
                                    {
                                        if (extraData.UnidadTecnicaAsignada <= 0)
                                        {
                                            result.Codigo = -1;
                                            result.Mensaje = "Seleccione la unidad técnica a Asignar.";
                                            return Json(result);
                                        }
                                        ut = ut ?? db.UnidadTecnica.FirstOrDefault(a => a.Id == extraData.UnidadTecnicaAsignada);
                                        extraDataLog = extraData.UnidadTecnicaAsignada + "|" + (ut?.Titulo ?? "");
                                        item.UtAsignadaId = extraData.UnidadTecnicaAsignada;
                                        item.AsignacionUt = DateTime.Now;
                                        item.EnviadoUt = DateTime.Now;
                                        item.EnviarUt = true;
                                        item.EstadoId = (int)EstadoIngreso.Asignado; //9	Asignado
                                        item.ResponsableUtId = ut.ResponsableId;
                                        item.EtapaId = (int)EtapaIngreso.UnidadTecnica; //12	Unidad Técnica
                                        accionLog = "ASIGNAR_UT";
                                        textoAccion = "Asignación de UT";
                                    }
                                    break;
                                case 2: //Reasignar UT
                                    {
                                        if (extraData.NuevaUnidadTecnica <= 0)
                                        {
                                            result.Codigo = -1;
                                            result.Mensaje = "Seleccione la unidad técnica a Reasignar.";
                                            return Json(result);
                                        }
                                        var utActual = db.UnidadTecnica.FirstOrDefault(a => a.Id == item.UtAsignadaId);
                                        extraDataLog = (item.AsignacionUt.HasValue ? item.AsignacionUt.Value.ToString("dd/MM/yyyy HH:mm") : "")
                                                    + "|"
                                                    + (utActual?.Titulo ?? "");
                                        ut = ut ?? db.UnidadTecnica.FirstOrDefault(a => a.Id == extraData.NuevaUnidadTecnica);
                                        item.UtAsignadaId = extraData.NuevaUnidadTecnica;
                                        item.Devolucion = DateTime.Now;
                                        item.AsignacionUt = DateTime.Now;
                                        item.EnviadoUt = DateTime.Now;
                                        item.ResponsableUtId = ut.ResponsableId;
                                        if (item.ProfesionalId == null) // Si el ingreso no tiene profesional asignado se modifica Estado y Etapa
                                        {
                                            item.EstadoId = (int)EstadoIngreso.Asignado; //9	Asignado
                                            item.EtapaId = (int)EtapaIngreso.UnidadTecnica; //12	Unidad Técnica
                                            item.EnviarUt = true;
                                        }
                                        accionLog = "REASIGNAR_UT";
                                        textoAccion = "Reasignación de UT";
                                    }
                                    break;
                                case 3: //asignar profesional UT
                                    if (extraData.ProfesionalUTAsignado2 <= 0)
                                    {
                                        result.Codigo = -1;
                                        result.Mensaje = "Seleccione el Profesional a Asignar.";
                                        return Json(result);
                                    }
                                    item.ProfesionalId = extraData.ProfesionalUTAsignado2;
                                    item.AsignacionResponsable = DateTime.Now;
                                    item.EstadoId = (int)EstadoIngreso.EnProcesoEnEstudio;// En proceso -En estudio
                                    accionLog = "ASIGNAR_PROF";
                                    textoAccion = "Asignación de Profesional UT";
                                    break;
                                case 4: //Reasignar Profesional UT
                                    if (extraData.NuevoProfesionalUT <= 0)
                                    {
                                        result.Codigo = -1;
                                        result.Mensaje = "Seleccione el Profesional a Reasignar.";
                                        return Json(result);
                                    }
                                    var profUtActual = db.Usuario.FirstOrDefault(a => a.Id == item.ProfesionalId);
                                    extraDataLog = "RESP_ANT:"
                                                + (profUtActual?.NombresApellidos ?? "")
                                                + "FECHA_ANT:"
                                                + (item.AsignacionResponsable.HasValue ? item.AsignacionResponsable.Value.ToString("dd/MM/yyyy HH:mm") : "");
                                    item.ProfesionalId = extraData.NuevoProfesionalUT;
                                    item.AsignacionResponsable = DateTime.Now;
                                    accionLog = "REASIGNAR_PROF";
                                    textoAccion = "Reasignación de Profesional UT";
                                    break;
                                case 5: //nuevo despacho
                                    //no hace nada aca llama, a la ventana de nuevo despacho desde el script
                                    break;
                                case 6: //Modificar Etiqueta
                                    if (extraData.NuevaEtiqueta <= 0)
                                    {
                                        result.Codigo = -1;
                                        result.Mensaje = "Seleccione la nueva Etiqueta.";
                                        return Json(result);
                                    }

                                    var etiqStr = extraData.NuevaEtiqueta.ToString();
                                    etiqueta = etiqueta ?? db.ListaValor.FirstOrDefault(a => a.Codigo == etiqStr && a.IdLista == 10);
                                    //Hay que borrar las etiquetas anteriores??
                                    item.Etiqueta.Add(etiqueta);
                                    accionLog = "MODIF_ETIQUETA";
                                    textoAccion = "Modificación Etiqueta"; //  " modificación masiva de etiqueta";
                                    break;
                                case 7: //Abrir Ingreso
                                    var userCierre = db.Usuario.FirstOrDefault(a => a.Id == item.CerradoPorId);
                                    extraDataLog = (item.Cierre.HasValue ? item.Cierre.Value.ToString("dd/MM/yyyy HH:mm") : "")
                                                + "|"
                                                + (userCierre?.NombresApellidos ?? "")
                                                + "|"
                                                + (item.MotivoCierreId.HasValue ? item.MotivoCierreId.Value.ToString() : "")
                                                + item.ComentarioCierre;
                                    item.EstadoId = GetEstadoByUltimaBitacora(item.Id);
                                    item.ComentarioCierre = "";
                                    item.CerradoPorId = null;
                                    item.MotivoCierreId = null;
                                    item.Cierre = null;
                                    accionLog = "ABRIR_INGRESOS";
                                    textoAccion = "Abrir Ingresos"; //  " abrir ingresos masivo";
                                    break;
                                default:
                                    result.Codigo = -1;
                                    result.Mensaje = "Acción no valida.";
                                    break;
                            }

                            //Log sistema de Procesos Masivos
                            if (result.Codigo >= 0)
                            {
                                var idProcesoLog = DateTime.Now.ToString("yyyyMMddHHmm");
                                logSistemaProcMas.Add(new LogSistemaDto
                                {
                                    Accion = accionLog,
                                    Flujo = "PROCESOMASIVO_" + idProcesoLog,
                                    EstadoId = item.EstadoId,
                                    EtapaId = item.EtapaId,
                                    Origen = "PROCESOMASIVO",
                                    Fecha = DateTime.Now,
                                    RequerimientoId = item.Id,
                                    UnidadTecnicaId = item.UtAsignadaId,
                                    UsuarioId = CurrentUserId,
                                    Usuario = CurrentUserName,
                                    DireccionIp = ClientIpAddress(),
                                    NombrePc = ClientHostName(),
                                    UserAgent = ClientAgent(),
                                    ExtraData = extraDataLog
                                });
                            }

                        }
                        db.SaveChanges();

                        //Se graba el Log sistema de Procesos Masivos
                        _mantSvc.SaveLogSistemaMulti(logSistemaProcMas);

                        result.Codigo = 1;
                        result.Mensaje = "Se informa que se ha realizado el proceso masivo de " + textoAccion + ". <br/>Para mayor detalle del proceso, puede revisar el apartado de Reportes del Gestor Documental, <br/>en la opción “Procesos Masivos”." +
                                         "<br/><b>¿Desea enviar el correo de notificación informando la realización del proceso masivo de " + textoAccion + "?</b>"; ;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Codigo = -1;
                result.Mensaje = "Ocurrio un error al procesar la acción.";
                result.Extra = ex.Message;
            }
            return Json(result);
        }

        [HttpPost]
        public ActionResult EnviarNotificacion(ProcesoMasivoModel extraData)
        {
            var resultado = new ResultadoOperacion();
            try
            {
                var idsReq = extraData.SeleccionGrilla;
                int idUtAsign = extraData.UnidadTecnicaAsignada;
                int idUtReAsign = extraData.NuevaUnidadTecnica;
                int idProfAsig = extraData.ProfesionalUTAsignado2;
                int idProfReAsig = extraData.NuevoProfesionalUT;
                resultado = _notifSvc.EnviarNotificacionProcesosMasivos(extraData.AccionEjecutar, idsReq, idUtAsign, idUtReAsign, idProfAsig, idProfReAsig);
            }
            catch (Exception ex)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "Ha ocurrido un error al enviar la notificación.";
                resultado.Extra = ex.Message;
            }
            return Json(resultado);
        }

        private int GetEstadoByUltimaBitacora(int requerimientoId)
        {
            /*             
            Id	Titulo	Activo	Orden
            1	Acuerdo Comisión	1	2
            2	Acuerdo Sesión	1	3


            11	En proceso - En estudio
            12	En proceso - Acuerdo de comisión
            13	En proceso - Acuerdo de sesión
             */

            var estado = (int)EstadoIngreso.EnProcesoEnEstudio; //En proceso - En estudio;
            using (var db = new GedocEntities())
            {
                var bitacora = db.Bitacora.Where(a => a.RequerimientoId == requerimientoId).OrderByDescending(a => a.Fecha).FirstOrDefault();
                if (bitacora != null)
                {

                    if (bitacora.TipoBitacoraCod == TipoBitacora.AcuerdoComision.ToString("D")) //"Acuerdo Comisión"
                    {
                        estado = (int)EstadoIngreso.EnProcesoAcuerdoComisión; //En proceso -Acuerdo de comisión
                    }
                    else if (bitacora.TipoBitacoraCod == TipoBitacora.AcuerdoSesion.ToString("D"))//"Acuerdo Sesión"
                    {
                        estado = (int)EstadoIngreso.EnProcesoAcuerdoSesión; //En proceso -Acuerdo de sesión
                    }
                }
            }
            return estado;
        }




        #region Virtualizacion Documentos Ingreso
        private IEnumerable<ComboboxItemModel> GetDocumentos(GedocEntities db)
        {
            return db.Requerimiento
                //.Where(r => r.CasoId == null)
                .Select(req => new ComboboxItemModel()
            {
                Value = req.Id,
                Text = req.DocumentoIngreso,
            });
        }

        public ActionResult VirtualizationDocumentos_Read([DataSourceRequest] DataSourceRequest request)
        {
            using (var db = new GedocEntities())
            {
                return Json(GetDocumentos(db).ToDataSourceResult(request));
            }
        }

        public ActionResult Documentos_ValueMapper(int?[] values)
        {
            var indices = new List<int>();
            using (var db = new GedocEntities())
            {
                if (values != null && values.Any())
                {
                    var index = 0;
                    foreach (var documento in GetDocumentos(db))
                    {
                        if (values.Contains(documento.Value))
                            indices.Add(index);
                        index += 1;
                    }
                }
            }
            return Json(indices, JsonRequestBehavior.AllowGet);
        }


        //Forma de Ernesto
        //public ActionResult DocIngresoPaging([DataSourceRequest] DataSourceRequest param)
        //{
        //    var db = new GedocEntities();
        //    if (param.Filters != null && param.Filters.Count > 0) ((FilterDescriptor)param.Filters[0]).Member = "DocumentoIngreso";
        //    IQueryable<Requerimiento> req = db.Requerimiento;
        //    DataSourceResult result = req.ToDataSourceResult(param, r => new
        //    {
        //        Id = r.Id,
        //        Titulo = r.DocumentoIngreso
        //    });

        //    return Json(result);
        //}

        //public ActionResult DocIngresoByIds(List<int> ids, bool cerrado)
        //{
        //    var db = new GedocEntities();
        //    var query = db.Requerimiento.Where(r => ids.Contains(r.Id));
        //    var datos = query
        //        .OrderBy(r => r.DocumentoIngreso)
        //        .AsEnumerable()
        //        .Select(r => new GenericoDto()
        //        {
        //            Id = r.Id,
        //            Titulo = r.DocumentoIngreso
        //        }).ToList();
        //    return Json(datos);
        //}
        #endregion
    }
}
