using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Helpers;
using Gedoc.WebApp.Helpers.Maps.Interface;
using Gedoc.WebApp.Models;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Gedoc.WebApp.Controllers
{
    public class RequerimientoController : BaseController
    {
        private readonly IGenericMap _mapper;
        private readonly IRequerimientoService _requerimientoSrv;
        private readonly IMantenedorService _mantenedorSrv;

        public RequerimientoController(IRequerimientoService requerimientoSrv, IGenericMap mapper,
            IMantenedorService mantenedorSrv)
        {
            _requerimientoSrv = requerimientoSrv;
            _mapper = mapper;
            _mantenedorSrv = mantenedorSrv;
        }

        public ActionResult Ficha(int id)
        {
            // Datos del requerimiento
            var datos = _requerimientoSrv.GetFichaById(id);

            ViewBag.Resultado = datos.Resultado;
            ViewBag.CurrentUserName = CurrentUserName;
            return PartialView(datos.Data);
        }

        public ActionResult FichaPdf(int id)
        {
            var datos = _requerimientoSrv.GetFichaById(id, true);

            ViewBag.Resultado = datos.Resultado;
            return View(datos.Data);
        }

        [HttpPost]
        public ActionResult FichaPdfExport(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            Response.Headers.Add("Content-Disposition", "inline; filename=" + fileName);
            return File(fileContents, contentType);
        }

        public ActionResult FichaResumen(int id)
        {
            // Datos del requerimiento
            var datos = _requerimientoSrv.GetFichaById(id);

            ViewBag.Resultado = datos.Resultado;
            ViewBag.CurrentUserName = CurrentUserName;
            ViewBag.LogReq = GetLogFlujoRequerimiento(id);
            return PartialView(datos.Data);
        }

        private List<LogSistemaDto> GetLogFlujoRequerimiento(int idReq)
        {
            var resultado = new List<LogSistemaDto>();
            var logFull = _requerimientoSrv.GetLogRequerimiento(idReq);
            // TODO: crear un metodo q traiga de la bd solo el último log, agrupado por Flujo y Acción, del requerimiento, en vez de traer todo
            // el log del requerimiento y dejar solo los ultimos por Flujo y Accion q es lo q se está haciendo aquí
            foreach (var log in logFull)
            {
                if (!resultado.Any(r => r.Flujo == log.Flujo && r.Accion == log.Accion))
                {
                    resultado.Add(log);
                }
            }
            
            return resultado;

        }

        public ActionResult NuevoIngreso()
        {
            var model = new RequerimientoModel();
            var priorizacion = _mantenedorSrv.GetPrioridadAll();
            ViewBag.MatrizPriorizacion = priorizacion.Data;
            ViewBag.Form = FlujoIngreso.NuevoRequerimiento;
            return PartialView("FormIngreso", model);
        }

        public ActionResult AccionIngreso(string idAccion, int idIngreso)
        {
            FlujoIngreso formIng = FlujoIngreso.NuevoRequerimiento;
            switch (idAccion)
            {
                case "IC": // Ingreso Central
                    formIng = FlujoIngreso.IngresoCentral;
                    break;
                case "AU": // Asignar UT
                    formIng = FlujoIngreso.AsignacionUt;
                    break;
                case "RAU": // Re-Asignar UT
                    idAccion = "AU";
                    formIng = FlujoIngreso.ReasignacionUt;
                    break;
                case "PR": //  Priorizar
                    formIng = FlujoIngreso.Priorizacion;
                    break;
                case "AP": // Asignación Profesional
                    formIng = FlujoIngreso.AsignacionProfUt;
                    break;
                case "RP": //  Reasignar Profesional
                    formIng = FlujoIngreso.ReasignacionProfUt;
                    break;
                case "ER": // Edicion Requerimientos UT
                    formIng = FlujoIngreso.EditarCamposUt;
                    break;
                case "ED": //  Editar Requerimiento
                    formIng = FlujoIngreso.EditarIngreso;
                    break;
                case "CE": // Cerrar
                    formIng = FlujoIngreso.RequerimientoCierre;
                    break;
                case "RH": // Requerimiento Histórico
                    formIng = FlujoIngreso.Historico;
                    break;
                //case "AD": // Adjuntos
                //    formIng = FlujoIngreso.;
                //    break;
                //case "BI": // Bitacora
                //    formIng = FlujoIngreso.;
                //    break;
                //case "DE": // Despachos
                //    formIng = FlujoIngreso.;
                //    break;
                //case "DI": //  Nuevo Despacho Iniciativas CMN
                //    formIng = FlujoIngreso.;
                //    break;
                //case "RH": //  Nuevo requerimiento histórico
                //    formIng = FlujoIngreso.;
                //    break;
                //case "BD": //   Bitácora Despacho Iniciativa CMN
                //    formIng = FlujoIngreso.;
                //    break;
                //case "EI": //  Editar Despacho Iniciativa CMN
                //    formIng = FlujoIngreso.;
                //    break;
                //case "CD": //   Cerrar Despacho Iniciativa CMN
                //    formIng = FlujoIngreso.;
                //    break;
                //case
                //    "DO": //  Descargar Oficio
                //    formIng = FlujoIngreso.;
                //    break;

                default:
                    formIng = FlujoIngreso.NuevoRequerimiento;
                    break;
            }

            ViewBag.Accion = idAccion;
            ViewBag.Form = formIng;
            ViewBag.AccesoCampos = GetAspectoCampos(formIng);
            var priorizacion =
                _mantenedorSrv
                    .GetPrioridadAll(); // TODO: ejecutar esto solo para los forms q tengan el panel de priorización
            var motivosCierre = _mantenedorSrv.GetGenericoMatenedor(Mantenedor.MotivoCierre);// TODO: ejecutar esto solo para los forms q tengan la opcion de motivo de cierre
            ViewBag.MatrizPriorizacion = priorizacion.Data;
            ViewBag.MotivosCierre = motivosCierre.Data;
            ViewBag.ChequeaObligatorios = true;
            if (formIng == FlujoIngreso.EditarIngreso)
            {  // Si es el form de Editar Requerimiento se chequea si el usuario no tiene el chequeo de campos obligatorios
                var userSinRestriccion = WebConfigValues.UsuariosEditaReqSinRestriccion;
                if (!string.IsNullOrEmpty(userSinRestriccion) && !string.IsNullOrWhiteSpace(CurrentUserName))
                {
                    var chequeaObligatorios = true;
                    // Sustituye , por ; para permitir , ó ; de separador de nombre de usuarios
                    userSinRestriccion = userSinRestriccion.Replace(",", ";");
                    var usuarios = userSinRestriccion.Split(';');
                    for (int i = 0; i < usuarios.Length; i++)
                    {
                        if (usuarios[i].Equals(CurrentUserName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            chequeaObligatorios = false;
                            break;
                        }
                    }

                    ViewBag.ChequeaObligatorios = chequeaObligatorios;
                }
            }
            #region Datos del formulario
            var dto = _requerimientoSrv.GetById(idIngreso);
            var model = _mapper.MapFromDtoToModel<RequerimientoDto, RequerimientoModel>(dto) ??
                        new RequerimientoModel()
                        {
                            FechaIngreso = formIng != FlujoIngreso.Historico ? DateTime.Now : DateTime.Now.AddYears(2015 - DateTime.Now.Year),
                            FechaDocumento = formIng != FlujoIngreso.Historico ? DateTime.MinValue : DateTime.Now.AddYears(2015 - DateTime.Now.Year),
                            CanalLlegadaTramiteCod = CanalLlegada.Presencial.ToString("D"),
                            CanalLlegadaTramiteTitulo = CanalLlegada.Presencial.ToString(),
                            EnviarAsignacion = false,
                            EnviarPriorizacion = false,
                            EnviarUt = false,
                            RequiereAcuerdo = false,
                            RequiereRespuesta = true,
                            RequiereTimbrajePlano = false,
                            Redireccionado = false
                        };
            model.EnviarAsignacionTemp = false;
            #endregion

            model.EnAsignacionTemp = model.UtTemporalId.GetValueOrDefault(0) > 0 &&
                                     !model.LiberarAsignacionTemp &&
                                     model.EstadoId != (int)EstadoIngreso.Ingresado;
            // Si es form de Editar Requerimiento se almacenan algunos datos del requerimiento para saber
            // si cambiaron y entonces enviar notificación email{
            //if (formIng == FlujoIngreso.EditarIngreso)
            //{
            //    model.BackupData = new RequerimientoBackupDataDto
            //    {
            //        UtAsignadaId = model.UtAsignadaId,
            //        ProfesionalId = model.ProfesionalId,
            //        UtTemporalId = model.UtTemporalId,
            //        UnidadTecnicaCopia = model.UnidadTecnicaCopia.Select(u => new GenericoDto { Id = u }).ToList(),
            //        UtConocimientoId = model.UtConocimientoId,
            //        PrioridadCod = model.PrioridadCod
            //    };
            //}

            ViewBag.AccesoForm = ValidaAccesoForm(formIng, model);
            // Se graba log (bitácora) de acceso al form
            _mantenedorSrv.SaveLogSistema(new LogSistemaDto {
                Accion = "ACCESOFORM",
                EstadoId = model.EstadoId,
                EtapaId = model.EtapaId,
                Fecha = DateTime.Now,
                Flujo = formIng.ToString().ToUpper(),
                Origen = "REQUERIMIENTO",
                OrigenId = model.Id,
                RequerimientoId = model.Id,
                Usuario = CurrentUserName,
                UsuarioId = CurrentUserId, 
                UnidadTecnicaId = model.UtAsignadaId,
                DireccionIp = ClientIpAddress(),
                NombrePc = ClientHostName(),
                UserAgent = ClientAgent(),
                ExtraData = formIng.GetDescription()
            });
            return PartialView("FormIngreso", model);
        }

        private ResultadoOperacion ValidaAccesoForm(FlujoIngreso form, RequerimientoModel ingreso)
        {
            var result = new ResultadoOperacion(1, "OK", null);

            // TODO: chequear si el perfil del usuario tiene acceso al formulario (en la app no debe aparecerle la acción para acceder al form pero hacer el chequeo aquí para aumetar la seguridad)

            if (!HaySesionActiva())
            {
                result.Codigo = -1;
                result.Mensaje = "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.";
                return result;
            }

            // Chequeo de requerimiento cerrado
            if (ingreso.EstadoId == (int)EstadoIngreso.Cerrado && form != FlujoIngreso.EditarIngreso)
            {
                result.Codigo = -1;
                result.Mensaje = string.Format("Atención. El requerimiento {0} se encuentra Cerrado", ingreso.DocumentoIngreso) ;
                return result;
            }
            switch (form)
            {
                case FlujoIngreso.Priorizacion:
                    if (ingreso.EtapaId != (int)EtapaIngreso.Priorizacion)
                    {
                        result.Codigo = -1;
                        result.Mensaje = $"Atención. El requerimiento {ingreso.DocumentoIngreso} no se encuentra en etapa de Priorización. Etapa actual del requerimiento: {ingreso.EtapaTitulo}; Estado: {ingreso.EstadoTitulo}";
                        return result;
                    }
                    break;
                case FlujoIngreso.AsignacionUt:
                    // Chequeo de estado-etapa permitidos para entrar al formulario
                    if ((ingreso.EtapaId != (int)EtapaIngreso.Asignacion && 
                         ingreso.EtapaId != (int)EtapaIngreso.Reasignacion)
                        || ingreso.EnAsignacionTemp)
                    {
                        result.Codigo = -1;
                        result.Mensaje = string.Format("Atención. El requerimiento {0} no se encuentra en etapa de {1}, o {2}. Etapa actual: {3}. Estado actual: {4}",
                            ingreso.DocumentoIngreso,
                            "Asignación",
                            "Reasignación",
                            ingreso.EtapaTitulo,
                            ingreso.EstadoTitulo);
                        return result;
                    }
                    // Se deshabilita la edición si está en Asignación Temporal y el usuario es de la unidad asignada y NO de la ut temporal
                    if (ingreso.EnAsignacionTemp)
                    {
                        // TODO: implementar bien esto, está en el código en producción. 
                        var utsUsuario = new List<int>(); // = GetUtUsuario(idUsuario); // TODO: implementar
                        if (utsUsuario.Contains(ingreso.UtAsignadaId.GetValueOrDefault(0)) && !utsUsuario.Contains(ingreso.UtTemporalId.GetValueOrDefault(0)) )
                        {
                            ViewBag.DisableControls = true;
                        }
                    }
                    break;
                case FlujoIngreso.ReasignacionUt:
                    // Chequeo de estado-etapa permitidos para entrar al formulario
                    if (ingreso.EtapaId != (int)EtapaIngreso.Reasignacion)
                    {
                        result.Codigo = -1;
                        result.Mensaje =
                            $"Atención. El requerimiento {ingreso.DocumentoIngreso} no se encuentra en etapa de {"Reasignación"}. Etapa actual: {ingreso.EtapaTitulo}. Estado actual: {ingreso.EstadoTitulo}";
                        return result;
                    }
                    // Se deshabilita la edición si está en Asignación Temporal y el usuario es de la unidad asignada y NO de la ut temporal
                    if (ingreso.EnAsignacionTemp)
                    {
                        // TODO: implementar bien esto, está en el código en producción. 
                        var utsUsuario = new List<int>(); // = GetUtUsuario(idUsuario); // TODO: implementar
                        if (utsUsuario.Contains(ingreso.UtAsignadaId.GetValueOrDefault(0)) && !utsUsuario.Contains(ingreso.UtTemporalId.GetValueOrDefault(0)))
                        {
                            ViewBag.DisableControls = true;
                        }
                    }
                    break;
                case FlujoIngreso.AsignacionProfUt:
                    // Chequeo de estado-etapa permitidos para entrar al formulario
                    if (ingreso.EtapaId != (int)EtapaIngreso.UnidadTecnica && !ingreso.EnAsignacionTemp)
                    {
                        result.Codigo = -1;
                        result.Mensaje = string.Format("Atención. El requerimiento {0} no se encuentra en etapa de {1} o en {2}. Etapa actual: {3}. Estado actual: {4}",
                            ingreso.DocumentoIngreso,
                            "Unidad Técnica",
                            "Asignación Temporal",
                            ingreso.EtapaTitulo,
                            ingreso.EstadoTitulo);
                        return result;
                    }
                    // Si está en asignación temporal y el usuario es el encargado de la ut principal entonces no se le da acceso
                    if (ingreso.EnAsignacionTemp && !EsAdmin())
                    {
                        var ut = _mantenedorSrv.GetUnidadTecnicaById(ingreso.UtAsignadaId.GetValueOrDefault(0));
                        if (CurrentUserId == ut.ResponsableId.GetValueOrDefault(0))
                        {
                            result.Codigo = -1;
                            result.Mensaje = "Atención. El requerimiento se encuentra en Asignación temporal, usted no tiene acceso a este formulario.";
                            return result;
                        }
                    }
                    break;
                case FlujoIngreso.EditarCamposUt:
                    // Si está en asignación temporal y el usuario es el encargado de la ut principal entonces no se le da acceso
                    if (ingreso.EnAsignacionTemp && !EsAdmin())
                    {
                        var ut = _mantenedorSrv.GetUnidadTecnicaById(ingreso.UtAsignadaId.GetValueOrDefault(0));
                        if (CurrentUserId == ut.ResponsableId.GetValueOrDefault(0))
                        {
                            result.Codigo = -1;
                            result.Mensaje = "Atención. El requerimiento se encuentra en Asignación temporal, usted no tiene acceso a este formulario.";
                            return result;
                        }
                    }
                    break;
            }

            return result;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(RequerimientoModel model)
        {
            var datos = _mapper.MapFromModelToDto<RequerimientoModel, RequerimientoDto>(model);
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioActualId = CurrentUserId;
            datos.DireccionIp = ClientIpAddress();
            datos.NombrePc = ClientHostName();
            datos.UserAgent = ClientAgent();
            var resultadoOper = _requerimientoSrv.Save(datos);

            return Json(resultadoOper);
        }

        [HttpPost]
        public ActionResult EnviaEmailEdicionReq(RequerimientoModel model)
        {
            var datos = _mapper.MapFromModelToDto<RequerimientoModel, RequerimientoDto>(model);
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioActualId = CurrentUserId;
            datos.DireccionIp = ClientIpAddress();
            datos.NombrePc = ClientHostName();
            datos.UserAgent = ClientAgent();
            var resultadoOper = _requerimientoSrv.EnviarNotificacionEmail(datos);

            return Json(resultadoOper);
        }

        #region Cierre múltiple de requerimientos
        public ActionResult CierreMultiple()
        {
            ViewBag.ProfesionalId = null;
            Int32.TryParse((Session["IdUsuario"] ?? "").ToString(), out var idUsuario);

            List<GenericoDto> unidadesTecnicas = null;

            var esAdministrador = Session["MisRolesId"] != null &&
                                     ((IEnumerable<int>)Session["MisRolesId"]).Any(r => r == (int)Rol.Administrador );
            var soloPerfilProfesional = Session["MisRoles"] != null &&
                              ((IEnumerable<int>)Session["MisRolesId"]).All(r => r == (int)Rol.ProfesionalUt);
            if (esAdministrador)
            { // Es administrador, accede a todas las UTs
                var uts = _mantenedorSrv.GetGenericoMatenedor(Mantenedor.UnidadTecnica);
                unidadesTecnicas = uts?.Data;
            }
            else
            { // No es administrador, accede a las UTs a las q pertenece el usuario
                unidadesTecnicas =
                    _mantenedorSrv.GetUnidadTecnicaByUsuario(idUsuario, /*esEncargado*/ false)
                        .Select(d => new GenericoDto { Id = d.Id.ToString(), Titulo = d.Titulo })
                        .ToList();
                if (soloPerfilProfesional)
                    ViewBag.ProfesionalId = idUsuario;
            }

            ViewBag.UnidadesTecnicas = unidadesTecnicas ?? new List<GenericoDto>();
            return View();
        }

        public ActionResult CierreMultiple_Get_Profesionales([DataSourceRequest]DataSourceRequest request, string unidadTecnica, string profesional)
        {
            int idUT = 0;
            int.TryParse(unidadTecnica ?? "0", out idUT);

            int idp = 0;
            int.TryParse(profesional ?? "0", out idp);
            int? idProfesional = null;
            if (idp != 0) idProfesional = idp;

            var usuarios = _mantenedorSrv.GetUsuariosUt(idUT); // _requerimientoSrv.GetProfesionales(idUT, idProfesional);
            

            return Json(usuarios.Data.OrderBy(oby => oby.NombresApellidos).Select(p => new { p.Id, Titulo = p.NombresApellidos }), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CierreMultipleGrid_Read([DataSourceRequest]DataSourceRequest request, DateTime? fDesde, DateTime? fHasta, string unidadTecnica, string profesional)
        {
            int.TryParse(unidadTecnica ?? "0", out var idUt);
            int.TryParse(profesional ?? "0", out var idProfesional);

            if (request.Filters != null && request.Filters.Count > 0)
            {
                ModifyFilters(request.Filters);
            }

            if (fDesde.HasValue && fHasta.HasValue)
            {
                DateTime fechaDesde = fDesde.Value.Date;
                DateTime fechaHasta = fHasta.Value.Date.AddDays(1);

                var requerimientos = _requerimientoSrv.GetDatosCierreMultiple(idUt, idProfesional, fechaDesde, fechaHasta);

                DataSourceResult result = requerimientos.ToDataSourceResult(request, req => new
                {
                    req.Id,
                    req.DocumentoIngreso,
                    req.FechaIngreso,
                    req.RemitenteNombre,
                    req.RemitenteInstitucion,
                    req.Materia,
                    req.EstadoTitulo
                });

                return Json(result);
            }
            else
                return null;
        }

        public ActionResult FormCierreMultiple(List<int> reqIds)
        {
            var motivosCierre = _mantenedorSrv.GetGenericoMatenedor(Mantenedor.MotivoCierre);
            ViewBag.MotivosCierre = motivosCierre.Data;
            var requerimientos = _requerimientoSrv.GetRequerimientoResumenByIds(reqIds, false).Data;
            ViewBag.ReqIds = reqIds;
            ViewBag.ReqDocs = requerimientos.Select(r => r.Titulo).ToList();
            return View("FormCierreMultiple", new RequerimientoModel());
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CerrarRequerimientos(List<int> reqIds, int motivoCierre, bool cerrar, string comentarioCierre)
        {

            var resultado = _requerimientoSrv.CierreMultiple(reqIds, motivoCierre, cerrar, comentarioCierre, GetDatosUsuarioActual());

            return Json(resultado);
        }

        #endregion

        private Dictionary<CampoIngreso, AspectoCampo> GetAspectoCampos(FlujoIngreso form)
        {
            var aspectoCampos = new Dictionary<CampoIngreso, AspectoCampo>();

            // Valores por defectos para los cmpos
            foreach (var campo in Enum.GetValues(typeof(CampoIngreso)).Cast<CampoIngreso>())
            {
                if (campo == CampoIngreso.Doc_DocIng) aspectoCampos[campo] = AspectoCampo.SoloLectura;
                else if (campo == CampoIngreso.Doc_FechaIng) aspectoCampos[campo] = AspectoCampo.SoloLectura;
                else if (campo == CampoIngreso.Doc_CanalLlegada) aspectoCampos[campo] = AspectoCampo.Editable;
                else if (campo == CampoIngreso.Doc_Estado) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.Doc_Etapa) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.Proy_ProyActiv) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.Asig_EnviarAsign) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.Asig_LiberarAsign) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.Prio_EnviarPrio) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.Prio_EnviarUt) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.Prio_EnviarUtTemp) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.Prio_FechaAsigUt) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.PanelAsignacionProfUt) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.PanelUtReasigProf) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.PanelOtrosCampos) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.PanelCierre) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.PanelGeneralHistorico) aspectoCampos[campo] = AspectoCampo.Oculto;
                //else if (campo == CampoIngreso.PanelReasignacionProfUt) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.PanelSolicReasignacion) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.PanelReasignacionUt) aspectoCampos[campo] = AspectoCampo.Oculto;
                else if (campo == CampoIngreso.Adj_ObsAdj) aspectoCampos[CampoIngreso.Adj_ObsAdj] = AspectoCampo.Habilitado;
                else if (campo == CampoIngreso.PanelTransparencia) aspectoCampos[campo] = AspectoCampo.Oculto;
                else aspectoCampos[campo] = AspectoCampo.Habilitado;
            }

            switch (form)
            {
                // config form Nuevo Ingreso
                case FlujoIngreso.NuevoRequerimiento:
                    // Config panel Documento
                    aspectoCampos[CampoIngreso.Doc_DocIng] =
                        AspectoCampo.Oculto; // el aspecto por defecto es solo lectura
                    aspectoCampos[CampoIngreso.Doc_FechaIng] = AspectoCampo.Deshabilitado;
                    // Config panel Asignación
                    aspectoCampos[CampoIngreso.Asig_EnviarAsign] = AspectoCampo.Habilitado;
                    aspectoCampos[CampoIngreso.Asig_UtAsign] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_UtConoc] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_UtCopia] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_UtTemp] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_ReqResp] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_ComentarioAsign] = AspectoCampo.Oculto;
                    // Config panel Priorización
                    aspectoCampos[CampoIngreso.PanelPriorizacion] = AspectoCampo.Oculto;
                    break;
                // config form Ingreso Central
                case FlujoIngreso.IngresoCentral:
                    // Config panel Documento
                    aspectoCampos[CampoIngreso.Doc_FechaIng] = AspectoCampo.Editable;
                    // Config panel Asignación
                    aspectoCampos[CampoIngreso.Asig_EnviarAsign] = AspectoCampo.Habilitado;
                    aspectoCampos[CampoIngreso.Asig_UtAsign] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_UtConoc] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_UtCopia] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_UtTemp] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_ReqResp] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_ComentarioAsign] = AspectoCampo.Oculto;
                    // Config panel Priorización
                    aspectoCampos[CampoIngreso.PanelPriorizacion] = AspectoCampo.Oculto;
                    break;
                // config form Asignación
                case FlujoIngreso.AsignacionUt:
                    // Config panel Priorización
                    aspectoCampos[CampoIngreso.Prio_EnviarPrio] = AspectoCampo.Habilitado;
                    // TODO: los siguientes campos se muestran si se asigna UT Temporal -->> implementar esta lógica
                    // en la vista y aquí ponerlos como Habilitado
                    aspectoCampos[CampoIngreso.Prio_TablaPlazos] = AspectoCampo.Habilitado;  // AspectoCampo.Habilitado;
                    aspectoCampos[CampoIngreso.Prio_Prioridad] = AspectoCampo.Habilitado;  // AspectoCampo.Habilitado;
                    aspectoCampos[CampoIngreso.Prio_Solurgencia] = AspectoCampo.Habilitado;  // AspectoCampo.Habilitado;
                    aspectoCampos[CampoIngreso.Prio_EnviarUtTemp] = AspectoCampo.Habilitado;  // AspectoCampo.Habilitado;
                    aspectoCampos[CampoIngreso.Prio_EnviarUt] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Doc_ObsTipoDoc] = AspectoCampo.Editable;
                    aspectoCampos[CampoIngreso.Doc_FechaDoc] = AspectoCampo.Editable;
                    break;
                // config form Reasignación UT  
                case FlujoIngreso.ReasignacionUt:
                    // Config panel Documento
                    aspectoCampos[CampoIngreso.Doc_ObsTipoDoc] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Doc_FechaDoc] = AspectoCampo.Editable;
                    aspectoCampos[CampoIngreso.Doc_TipoDoc] = AspectoCampo.Deshabilitado;
                    aspectoCampos[CampoIngreso.PanelProyecto] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.PanelAdjuntos] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.PanelRemitente] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.PanelGeneral] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.PanelAsignacion] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.PanelReasignacionUt] = AspectoCampo.Habilitado;
                    //aspectoCampos[CampoIngreso.PanelAsignacion] = AspectoCampo.Oculto;
                    // Config panel Priorización
                    aspectoCampos[CampoIngreso.Prio_TablaPlazos] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Prio_EnviarPrio] = AspectoCampo.Habilitado;
                    aspectoCampos[CampoIngreso.Prio_Prioridad] = AspectoCampo.SoloLectura;  // AspectoCampo.Habilitado;
                    aspectoCampos[CampoIngreso.Prio_Solurgencia] = AspectoCampo.SoloLectura;  // AspectoCampo.Habilitado;
                    aspectoCampos[CampoIngreso.Prio_EnviarUtTemp] = AspectoCampo.Oculto;  // AspectoCampo.Habilitado;
                    aspectoCampos[CampoIngreso.Prio_EnviarPrio] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Prio_EnviarUt] = AspectoCampo.Habilitado;
                    break;
                // config form Asignación Temporal
                //case FlujoIngreso.AsignacionUtTemp:
                //    // Config panel Priorización
                //    aspectoCampos[CampoIngreso.Prio_TablaPlazos] = AspectoCampo.Habilitado; //********************
                //    break;
                // config form priorización
                case FlujoIngreso.Priorizacion:
                    // Config panel Documento
                    aspectoCampos[CampoIngreso.Doc_Siac] = AspectoCampo.Oculto;
                    // Config panel Asignación
                    aspectoCampos[CampoIngreso.Asig_UtAsign] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_UtConoc] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_UtCopia] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_UtTemp] = AspectoCampo.SoloLectura;
                    // Config panel Priorización
                    aspectoCampos[CampoIngreso.Prio_EnviarUt] = AspectoCampo.Habilitado;
                    break;
                // config form Asignación Profesional
                case FlujoIngreso.AsignacionProfUt:
                    // Config panel Documento
                    aspectoCampos[CampoIngreso.Doc_TipoDoc] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Doc_ObsTipoDoc] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Doc_FechaDoc] = AspectoCampo.SoloLectura;
                    // Config panel Adjuntos
                    aspectoCampos[CampoIngreso.Adj_AdjuntaDoc] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Adj_CantAdj] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Adj_TipoAdj] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Adj_Soporte] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Adj_ObsAdj] = AspectoCampo.SoloLectura;
                    // Config panel Remitente
                    aspectoCampos[CampoIngreso.Rem_Remitente] = AspectoCampo.Oculto;
                    // Config panel Proyecto
                    aspectoCampos[CampoIngreso.Proy_Materia] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Proy_ProyActiv] = AspectoCampo.Habilitado;
                    // Config panel General
                    aspectoCampos[CampoIngreso.Gral_FormaLlegada] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Gral_ObsFormaLlegada] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Gral_Caracter] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Gral_ObsCaracter] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Gral_Redireccionado] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Gral_NumTicket] = AspectoCampo.SoloLectura;
                    // Config panel Asignación
                    aspectoCampos[CampoIngreso.Asig_UtAsign] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_UtConoc] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_UtCopia] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_UtTemp] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_ComentarioAsign] = AspectoCampo.SoloLectura;
                    // Config panel Priorización
                    aspectoCampos[CampoIngreso.Prio_TablaPlazos] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Prio_Prioridad] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Prio_Solurgencia] = AspectoCampo.SoloLectura;
                    // Config panel UT Asignacion Profesional
                    aspectoCampos[CampoIngreso.PanelAsignacionProfUt] = AspectoCampo.Habilitado;
                    // Config panel Solicitud de Reasignación 
                    aspectoCampos[CampoIngreso.PanelSolicReasignacion] = AspectoCampo.Habilitado;
                    break;
                // config form Reasignación Profesional
                case FlujoIngreso.ReasignacionProfUt:
                    // Config panel Documento
                    aspectoCampos[CampoIngreso.Doc_TipoTramite] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Doc_CanalLlegada] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Doc_TipoDoc] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Doc_ObsTipoDoc] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Doc_FechaDoc] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Doc_Siac] = AspectoCampo.Oculto;
                    // Config panel Adjuntos
                    aspectoCampos[CampoIngreso.PanelAdjuntos] = AspectoCampo.Oculto;
                    // Config panel Remitente
                    aspectoCampos[CampoIngreso.PanelRemitente] = AspectoCampo.Oculto;
                    // Config panel Proyecto
                    aspectoCampos[CampoIngreso.Proy_NombreProy] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Proy_NombreCaso] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Proy_NumCaso] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Proy_Materia] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Proy_Etiqueta] = AspectoCampo.Oculto;
                    // Config panel Monumento Nacional
                    aspectoCampos[CampoIngreso.Mn_CategoriaMn] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Mn_CodigoMn] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Mn_DenomOf] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Mn_OtrasDenom] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Mn_NombreUsoActual] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Mn_DireccionMn] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Mn_RefLocal] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Mn_Region] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Mn_Provincia] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Mn_Comuna] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Mn_Rol] = AspectoCampo.SoloLectura;
                    // Config panel General
                    aspectoCampos[CampoIngreso.PanelGeneral] = AspectoCampo.Oculto;
                    // Config panel Asignación
                    aspectoCampos[CampoIngreso.Asig_UtAsign] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_UtConoc] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_UtCopia] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_UtTemp] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_ReqResp] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_ComentarioAsign] = AspectoCampo.SoloLectura;
                    // Config panel Priorización
                    aspectoCampos[CampoIngreso.Prio_TablaPlazos] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Prio_Prioridad] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Prio_Solurgencia] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Prio_FechaAsigUt] = AspectoCampo.SoloLectura;
                    // Config panel UT Reasignar Profesional
                    aspectoCampos[CampoIngreso.PanelUtReasigProf] = AspectoCampo.Habilitado;
                    break;
                // config form Editar Campos Ut
                case FlujoIngreso.EditarCamposUt:
                    // Config panel Documento
                    aspectoCampos[CampoIngreso.Doc_DocIng] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Doc_FechaIng] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Doc_TipoDoc] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Doc_Siac] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Doc_ObsTipoDoc] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Doc_FechaDoc] = AspectoCampo.Oculto;
                    // Config panel Adjuntos
                    aspectoCampos[CampoIngreso.PanelAdjuntos] = AspectoCampo.Oculto;
                    // Config panel Remitente
                    aspectoCampos[CampoIngreso.PanelRemitente] = AspectoCampo.Oculto;
                    // Config panel Proyecto
                    aspectoCampos[CampoIngreso.Proy_Materia] = AspectoCampo.SoloLectura;
                    // Config panel General
                    aspectoCampos[CampoIngreso.PanelGeneral] = AspectoCampo.Oculto;
                    // Config panel Asignación
                    aspectoCampos[CampoIngreso.PanelAsignacion] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_LiberarAsign] = AspectoCampo.Habilitado;
                    // Config panel Priorización
                    aspectoCampos[CampoIngreso.PanelPriorizacion] = AspectoCampo.Oculto;
                    // Config panel Otros Campos
                    aspectoCampos[CampoIngreso.PanelOtrosCampos] = AspectoCampo.Habilitado;
                    // Config panel Transparencia
                    aspectoCampos[CampoIngreso.PanelTransparencia] = AspectoCampo.Habilitado;
                    break;
                // config form Editar Requerimiento
                case FlujoIngreso.EditarIngreso:
                    // Config panel Documento
                    aspectoCampos[CampoIngreso.Doc_CanalLlegada] = AspectoCampo.Editable;
                    aspectoCampos[CampoIngreso.Doc_FechaIng] = AspectoCampo.Editable;
                    aspectoCampos[CampoIngreso.Doc_Estado] = AspectoCampo.Editable; // aspecto por defecto Oculto
                    aspectoCampos[CampoIngreso.Doc_Etapa] = AspectoCampo.Editable; // aspecto por defecto Oculto
                    // Config panel Asignación
                    aspectoCampos[CampoIngreso.Asig_LiberarAsign] = AspectoCampo.Habilitado;
                    // Config panel Transparencia
                    aspectoCampos[CampoIngreso.PanelTransparencia] = AspectoCampo.Habilitado;
                    break;
                // config form Cerrar
                case FlujoIngreso.RequerimientoCierre:
                    // Config panel Documento
                    aspectoCampos[CampoIngreso.Doc_CanalLlegada] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Doc_Siac] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Doc_TipoDoc] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Doc_ObsTipoDoc] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Doc_FechaIng] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Doc_FechaDoc] = AspectoCampo.SoloLectura;
                    // Config panel Adjuntos
                    aspectoCampos[CampoIngreso.Adj_AdjuntaDoc] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Adj_CantAdj] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Adj_TipoAdj] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Adj_Soporte] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Adj_ObsAdj] = AspectoCampo.SoloLectura;
                    // Config panel Remitente
                    aspectoCampos[CampoIngreso.Rem_Remitente] = AspectoCampo.Oculto;
                    // Config panel Proyecto
                    aspectoCampos[CampoIngreso.Proy_Materia] = AspectoCampo.SoloLectura;
                    // Config panel General
                    aspectoCampos[CampoIngreso.Gral_FormaLlegada] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Gral_ObsFormaLlegada] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Gral_Caracter] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Gral_ObsCaracter] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Gral_Redireccionado] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Gral_NumTicket] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Gral_ReqAnterior] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Gral_ReqNoRegistrado] = AspectoCampo.Oculto;
                    // Config panel Asignación
                    aspectoCampos[CampoIngreso.Asig_UtAsign] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_UtConoc] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_UtCopia] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_UtTemp] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Asig_ReqResp] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Asig_ComentarioAsign] = AspectoCampo.SoloLectura;
                    // Config panel Priorización
                    aspectoCampos[CampoIngreso.Prio_TablaPlazos] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Prio_Prioridad] = AspectoCampo.SoloLectura;
                    aspectoCampos[CampoIngreso.Prio_Solurgencia] = AspectoCampo.SoloLectura;
                    // Config panel Cierre
                    aspectoCampos[CampoIngreso.PanelCierre] = AspectoCampo.Habilitado;
                    break;
                // config form Requerimientos Históricos
                case FlujoIngreso.Historico:
                    // Config panel Documento
                    aspectoCampos[CampoIngreso.Doc_DocIng] = AspectoCampo.Habilitado;
                    aspectoCampos[CampoIngreso.Doc_FechaIng] = AspectoCampo.Habilitado;
                    aspectoCampos[CampoIngreso.Doc_Siac] = AspectoCampo.Oculto;
                    aspectoCampos[CampoIngreso.Doc_SoloMesAnno] = AspectoCampo.Oculto;
                    // Config panel Adjuntos
                    aspectoCampos[CampoIngreso.Adj_CantAdj] = AspectoCampo.Oculto;
                    // Config panel General
                    aspectoCampos[CampoIngreso.PanelGeneral] = AspectoCampo.Oculto;
                    // Config panel General Histórico
                    aspectoCampos[CampoIngreso.PanelGeneralHistorico] = AspectoCampo.Habilitado;
                    // Config panel Asignación
                    aspectoCampos[CampoIngreso.PanelAsignacion] = AspectoCampo.Oculto;
                    // Config panel Priorización
                    aspectoCampos[CampoIngreso.PanelPriorizacion] = AspectoCampo.Oculto;
                    break;
            }

            return aspectoCampos;
        }

        private void ModifyFilters(IEnumerable<IFilterDescriptor> filters)
        {
            if (filters.Any())
            {
                foreach (var filter in filters)
                {
                    var descriptor = filter as FilterDescriptor;
                    if (descriptor != null)
                        switch (descriptor.Member)
                        {
                            case "Id":
                                {
                                    descriptor.MemberType = typeof(Int32);
                                    int i;
                                    Int32.TryParse(descriptor.Value.ToString(), out i);
                                    descriptor.Value = i;

                                    break;
                                }
                            case "FechaCarga":
                                {
                                    descriptor.MemberType = typeof(DateTime);
                                    DateTime d;
                                    DateTime.TryParse(descriptor.Value.ToString(), out d);
                                    descriptor.Value = d;
                                    break;
                                }
                            case "NombreArchivo":
                                {
                                    descriptor.MemberType = typeof(String);
                                    break;
                                }
                            case "UrlArchivo":
                                {
                                    descriptor.MemberType = typeof(String);
                                    break;
                                }
                            case "DocumentoIngreso":
                                {
                                    descriptor.MemberType = typeof(String);
                                    break;
                                }
                            case "CreadoPor":
                                {
                                    descriptor.MemberType = typeof(String);
                                    break;
                                }
                        }
                    else if (filter is CompositeFilterDescriptor)
                    {
                        ModifyFilters(((CompositeFilterDescriptor)filter).FilterDescriptors);
                    }
                }
            }
        }

        #region Actualización de MN en requerimientos
        public ActionResult ActualizaMonumento()
        {
            return View();
        }
        public ActionResult ActualizarMn(List<int> reqIds, List<int> casoIds, string codigoMn)
        {
            var resultadoOper = new ResultadoOperacion(-1, "Ha ocurrido un error al actualizar los datos.", null);

            // Se obtienen los datos del MN desde Regmon
            var regMonHlp = new RegmonHelper(_mantenedorSrv);
            var datosMn = regMonHlp.GetDatosMn(codigoMn);
            resultadoOper.Extra = datosMn;
            if (datosMn == null)
            {
                resultadoOper.Mensaje = "Ha ocurrido un error al obtener los datos del Monumento Nacional " + codigoMn + " desde Regmon.";
                resultadoOper.Extra = new MonumentoNacionalDto();
                return Json(resultadoOper);
            }
            else if (string.IsNullOrWhiteSpace(datosMn.CodigoMonumentoNac))
            {
                resultadoOper.Mensaje = "No se ha encontrado en Regmon el código " + codigoMn + " de monumento seleccionado.";
                return Json(resultadoOper);
            }
            // Se actualizan los requermientos con los datos del MN de Regmon
            resultadoOper = _requerimientoSrv.ActualizarMnFromRegmon(reqIds, casoIds, datosMn);

            return Json(resultadoOper);
        }
        #endregion

    }
}