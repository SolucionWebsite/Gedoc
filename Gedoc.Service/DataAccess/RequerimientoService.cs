using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Helpers.Logging;
using Gedoc.Repositorio.Implementacion;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.Service.EmailService;
using GD = Gedoc.Helpers.GeneralData;

namespace Gedoc.Service.DataAccess
{
    public class RequerimientoService: BaseService, IRequerimientoService
    {
        private readonly IRequerimientoRepositorio _repoReq;
        private readonly IMantenedorRepositorio _repoMant;
        private readonly INotificacionService _notifSrv;

        public RequerimientoService(IRequerimientoRepositorio repoReq, IMantenedorRepositorio repoMant,
            INotificacionService notifSrv)
        {
            _repoReq = repoReq;
            _repoMant = repoMant;
            _notifSrv = notifSrv;
        }

        public ResultadoOperacion Save(RequerimientoDto ingreso)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            var resultadoEnvioEmail = new ResultadoOperacion();
            var resultadoEnvioEmail2 = new ResultadoOperacion(); 
            var msg = "";
            try
            {
                var logSistema = new LogSistemaDto()
                {
                    Flujo = ingreso.Flujo.ToString().ToUpper(),
                    Fecha = DateTime.Now,
                    Origen = "REQUERIMIENTO",
                    Usuario = ingreso.UsuarioActual ?? "<desconocido>",
                    UsuarioId = ingreso.UsuarioActualId,
                    DireccionIp = ingreso.DireccionIp,
                    NombrePc = ingreso.NombrePc,
                    UserAgent = ingreso.UserAgent
                };

                _notifSrv.UsuarioActual = ingreso.UsuarioActual;
                ingreso.BackupData = ingreso.BackupData ?? new RequerimientoBackupDataDto();
                if (ingreso.Id == 0)
                {
                    // Nuevo Requerimiento / Nuevo Requerimiento Histórico
                    if (ingreso.FechaDocumento == DateTime.MinValue)
                    { 
                        ingreso.FechaDocumento = DateTime.Now;
                    }
                    ingreso.DocumentoIngreso = ingreso.Flujo == FlujoIngreso.Historico ? ingreso.DocumentoIngreso : "";
                    ingreso.FechaIngreso = ingreso.Flujo == FlujoIngreso.Historico ? ingreso.FechaIngreso : DateTime.Now;
                    ingreso.TipoIngreso = ingreso.Flujo == FlujoIngreso.Historico
                        ? "Ingreso historico" 
                        : (ingreso.EsTransparencia ? GD.TIPO_INGRESO_TRANSP : "Nuevo ingreso");
                    if (!ingreso.AdjuntaDocumentacion)
                    {
                        ingreso.TipoAdjunto = null;
                        ingreso.ObservacionesAdjuntos = "";
                        ingreso.CantidadAdjuntos = 0;
                    }

                    if (ingreso.Flujo == FlujoIngreso.NuevoRequerimientoWss)
                    {  // Nuevo requerimiento inyectado desde el Wss de interoperación con el sistema de Trámites
                        //logSistema.Flujo = FlujoIngreso.NuevoRequerimiento.ToString().ToUpper();
                        //logSistema.Origen = "REQUERIMIENTO_WSS";
                        if (!string.IsNullOrWhiteSpace(ingreso.PrioridadCod))
                        {
                            // Cálculo del plazo y fecha resolución estimada en base a la prioridad seleccionada
                            var prior = _repoMant.GetPrioridadById(ingreso.PrioridadCod);
                            if (prior == null)
                            {
                                resultado.Codigo = -1;
                                resultado.Mensaje = "No se encontró información de la prioridad especificada.";
                                return resultado;
                            }
                            ingreso.Plazo = prior.Plazo;
                            ingreso.Resolucion = _repoMant.GetFechaDiasHabiles(ingreso.FechaIngreso, prior.Plazo);
                            ingreso.EnviadoUt = DateTime.Now;
                        }
                    }
                    else
                    {
                        if (ingreso.Flujo == FlujoIngreso.Historico)
                        {
                            ingreso.EstadoId = (int)EstadoIngreso.EnProcesoEnEstudio;
                            ingreso.EtapaId = (int)EtapaIngreso.UnidadTecnica;
                        } else
                        {
                            ingreso.EstadoId = (int)EstadoIngreso.Ingresado;
                            ingreso.EtapaId = !ingreso.EnviarAsignacion.GetValueOrDefault(false)
                                ? (int)EtapaIngreso.IngresoCentral
                                : (int)EtapaIngreso.Asignacion;
                        }
                        //ingreso.CanalLlegadaTramiteCod = CanalLlegada.Presencial.ToString("D");
                    }

                    resultado = _repoReq.New(ingreso);
                    resultado.Mensaje =
                        "Se ha creado satisfactoriamente el requerimiento. <br><br>Número de documento: " +
                        ingreso.DocumentoIngreso;

                    logSistema.Accion = ingreso.EnviarAsignacion.GetValueOrDefault(false)
                        ? "ENVIAR_ASIGNACION"
                        : "CREACION";
                }
                else
                {
                    ingreso.TipoIngreso = null;
                    if (ingreso.EsTransparencia != ingreso.EsTransparenciaAnt && ingreso.TipoIngreso != "Ingreso historico")
                    {
                        // Se marcó o desmarcó el check de Siac/Transparencia
                        ingreso.TipoIngreso = ingreso.EsTransparencia
                            ? GD.TIPO_INGRESO_TRANSP
                            : "Nuevo ingreso";
                    } else if (ingreso.EsTransparencia)
                    {  // Se marca el check de Transparencia entonces se deshabilita la Asignación Temporal
                        ingreso.UtTemporalId = null;
                        ingreso.ResponsableUtTempId = null;
                        ingreso.ProfesionalTempId = null;
                    }
                    /*ingreso.TipoIngreso = ingreso.EsTransparencia && ingreso.TipoIngreso == "Nuevo ingreso"
                        ? GD.TIPO_INGRESO_TRANSP
                        : "Nuevo ingreso";*/
                    ingreso.EnAsignacionTemp = ingreso.UtTemporalId.GetValueOrDefault(0) > 0 &&
                                            !ingreso.LiberarAsignacionTemp && !ingreso.LiberarAsignacionTempAnt;
                    ingreso.EsReasignacion = ingreso.EtapaId == (int) EtapaIngreso.Reasignacion; /* ingreso.UtAnteriorId.GetValueOrDefault() > 0 &&
                                             ingreso.UtAnteriorId.GetValueOrDefault() != ingreso.UtAsignadaId.GetValueOrDefault();*/
                    switch (ingreso.Flujo)
                    {
                        // INGRESO CENTRAL
                        case FlujoIngreso.IngresoCentral:
                            var enviarAsignacion = ingreso.EnviarAsignacion.GetValueOrDefault(false);
                            ingreso.EstadoId = (int)EstadoIngreso.Ingresado;
                            ingreso.EtapaId = !ingreso.EnviarAsignacion.GetValueOrDefault(false) 
                                ? (int)EtapaIngreso.IngresoCentral 
                                : (int)EtapaIngreso.Asignacion;

                            resultado = _repoReq.UpdateIngresoCentral(ingreso);

                            // Notificación email
                            resultadoEnvioEmail = EnviarNotificacionEmail(ingreso);

                            resultado.Mensaje = enviarAsignacion
                                ? "Requerimiento enviado satisfactoriamente a Asignación." 
                                : "Se han grabado satisfactoriamente los datos.";

                            logSistema.Accion = enviarAsignacion ? "ENVIAR_ASIGNACION" : "EDICION" ;
                            break;
                        // ASIGNACIÓN UT
                        case FlujoIngreso.AsignacionUt:
                            var enviarPriorizacion = ingreso.EnviarPriorizacion.GetValueOrDefault(false);
                            msg = "Se ha asignado satisfactoriamente el requerimiento. Aún no se ha enviado a Priorización.";
                            // Se asigna responsable de la ut
                            var utAsign = _repoMant.GetUnidadTecnicaById(ingreso.UtAsignadaId.GetValueOrDefault(0));
                            ingreso.ResponsableUtId = utAsign.ResponsableId;
                            ingreso.AsignacionUt = DateTime.Now;

                            if (ingreso.EnAsignacionTemp)
                            {  // SE ASIGNA UT TEMPORAL
                                // Cálculo del plazo y fecha resolución estimada en base a la prioridad seleccionada
                                var priorT = _repoMant.GetPrioridadById(ingreso.PrioridadCod);
                                if (priorT == null)
                                {
                                    resultado.Codigo = -1;
                                    resultado.Mensaje = "No se encontró información de la prioridad especificada.";
                                    return resultado;
                                }
                                ingreso.Plazo = priorT.Plazo;
                                ingreso.Resolucion = _repoMant.GetFechaDiasHabiles(ingreso.FechaIngreso, priorT.Plazo);
                                msg = "Se han grabado con éxito los datos. Aún no se ha enviado a Asignación Temporal.";
                                if (ingreso.EnviarAsignacionTemp.GetValueOrDefault(false))
                                {  // SE ENVÍA A ASIGNACIÓN TEMPORAL:
                                    msg = "Se ha asignado temporalmente el requerimiento.";
                                    ingreso.EstadoId = (int)EstadoIngreso.AsignacionTemporal;
                                    ingreso.EtapaId = (int)EtapaIngreso.Asignacion;
                                    // Se asigna responsable de la ut temporal
                                    var utTemp = _repoMant.GetUnidadTecnicaById(ingreso.UtTemporalId.GetValueOrDefault(0));
                                    ingreso.ResponsableUtTempId = utTemp.ResponsableId;
                                    ingreso.AsignacionUtTemp = DateTime.Now;
                                }
                            }
                            else if (enviarPriorizacion /*|| 
                                     ingreso.EnviarUt.GetValueOrDefault(false)*/) /*TODO: revisar esto de EnviarUt, está puesto en el código en producción pero en el form de Asignación UT no está esta opción*/
                            {  // SE ENVÍA A PRIORIZACIÓN
                                msg = "Se ha asignado satisfactoriamente el requerimiento.";
                                ingreso.EstadoId = (int)EstadoIngreso.Asignado;
                                ingreso.EtapaId = (int)EtapaIngreso.Priorizacion;
                                // TODO: revisar esto de EnviarUt, está puesto en el código en producción pero en el form de Asignación UT no está esta opción*/
                                if (ingreso.EnviarUt.GetValueOrDefault(false))
                                {
                                    ingreso.EnviadoUt = DateTime.Now;
                                }
                            }

                            resultado = _repoReq.UpdateAsignacionUt(ingreso);
                            if (resultado.Codigo < 0)
                            {
                                logSistema.Accion = "ERROR";
                                break;
                            }

                            // Notificación email
                            resultadoEnvioEmail = EnviarNotificacionEmail(ingreso);

                            resultado.Mensaje = msg + resultadoEnvioEmail.Mensaje;

                            logSistema.Accion = enviarPriorizacion
                                ? "ENVIAR_PRIORIZACION"
                                : (ingreso.EnviarAsignacionTemp.GetValueOrDefault(false) ? "ENVIAR_ASIGN_TEMPORAL" : "EDICION");
                            if (ingreso.EnviarAsignacionTemp.GetValueOrDefault(false))
                            {
                                // si se envía el requerimiento a ASignación Temporal entonces se graba el log de priorización
                                logSistema.RequerimientoId = ingreso.Id;
                                logSistema.EstadoId = ingreso.EstadoId;
                                logSistema.EtapaId = ingreso.EtapaId;
                                logSistema.UnidadTecnicaId = ingreso.UtAsignadaId;
                                _repoMant.SaveLogSistema(logSistema);
                                logSistema.Flujo = FlujoIngreso.Priorizacion.ToString().ToUpper(); 
                                logSistema.Accion = "ENVIAR_UT";
                            }
                            break;
                        // RE-ASIGNACIÓN UT
                        case FlujoIngreso.ReasignacionUt:
                            var enviarUT = ingreso.EnviarUt.GetValueOrDefault(false);
                            msg = "Se ha asignado satisfactoriamente el requerimiento.";
                            // Se asigna responsable de la ut
                            var utAsign2 = _repoMant.GetUnidadTecnicaById(ingreso.UtAsignadaId.GetValueOrDefault(0));
                            ingreso.ResponsableUtId = utAsign2.ResponsableId;
                            ingreso.AsignacionUt = DateTime.Now;

                            if (enviarUT)
                            {
                                ingreso.EstadoId = (int)EstadoIngreso.Asignado;
                                ingreso.EtapaId = (int)EtapaIngreso.UnidadTecnica;
                            }

                            resultado = _repoReq.UpdateReasignacionUt(ingreso);
                            if (resultado.Codigo < 0)
                            {
                                logSistema.Accion = "ERROR";
                                break;
                            }

                            // Notificación email
                            resultadoEnvioEmail = EnviarNotificacionEmail(ingreso);

                            resultado.Mensaje = msg + resultadoEnvioEmail.Mensaje;

                            logSistema.Accion = enviarUT ? "REASIGNACION-UT" : "EDICION";
                            break;
                        // PRIORIZACIÓN
                        case FlujoIngreso.Priorizacion:
                            // Cálculo del plazo y fecha resolución estimada en base a la prioridad seleccionada
                            var prior = _repoMant.GetPrioridadById(ingreso.PrioridadCod);
                            if (prior == null)
                            {
                                resultado.Codigo = -1;
                                resultado.Mensaje = "No se encontró información de la prioridad especificada.";
                                return resultado;
                            }
                            ingreso.Plazo = prior.Plazo;
                            ingreso.Resolucion = _repoMant.GetFechaDiasHabiles(ingreso.FechaIngreso, prior.Plazo);
                            // Cambio de Estado y Etapa si es q se marca Enviar a UT
                            if (ingreso.EnviarUt.GetValueOrDefault(false))
                            {
                                ingreso.EstadoId = (int)EstadoIngreso.Asignado;
                                ingreso.EtapaId = (int)EtapaIngreso.UnidadTecnica;
                            }

                            resultado = _repoReq.UpdatePriorizacion(ingreso);
                            if (resultado.Codigo < 0)
                            {
                                logSistema.Accion = "ERROR";
                                break;
                            }

                            // Notificación email
                            resultadoEnvioEmail = EnviarNotificacionEmail(ingreso);

                            msg = "Se ha priorizado satisfactoriamente el requerimiento. "
                                        + (ingreso.EnviarUt.GetValueOrDefault(false)
                                            ? ""
                                            : "Aún no ha sido enviado a UT.");
                            resultado.Mensaje = msg + resultadoEnvioEmail.Mensaje;

                            logSistema.Accion = ingreso.EnviarUt.GetValueOrDefault(false) ? "EDICION" : "ENVIAR_UT";
                            break;
                        // ASIGNACIÓN PROFESIONAL UT
                        case FlujoIngreso.AsignacionProfUt:
                            if (ingreso.EnAsignacionTemp) 
                            { // El ingreso está en Asignación temporal y se asigna el Profesional de la UT Temporal
                                // Estado se mantiene en Asignación Temporal
                                ingreso.EtapaId = (int)EtapaIngreso.AsignacionProfTemporal;
                                ingreso.AsignacionProfesionalTemp = DateTime.Now;
                                ingreso.RecepcionUtTemp = DateTime.Now;
                            }
                            else
                            {
                                ingreso.EstadoId = (int)EstadoIngreso.EnProcesoEnEstudio;
                                ingreso.EtapaId = (int)EtapaIngreso.UnidadTecnica;
                                // ingreso.RecepcionUt = DateTime.Now;
                                ingreso.AsignacionResponsable = DateTime.Now;
                            }
                            if (ingreso.DevolverAsignacion)
                            {
                                ingreso.EstadoId = (int)EstadoIngreso.Ingresado;
                                ingreso.EtapaId = (int)EtapaIngreso.Reasignacion;
                                ingreso.Devolucion = DateTime.Now;
                                ingreso.AsignacionAnterior = ingreso.AsignacionUt;
                                ingreso.UtAnteriorId = ingreso.UtAsignadaId;
                                ingreso.ProfesionalId = null;
                                ingreso.ProfesionalTempId = null;
                                ingreso.ProfesionalTranspId = null;
                                // ingreso.AsignacionResponsable = null; // En el código de producción no se vacía este campo aunque lo lógico es dejarlo vacío también
                                ingreso.ResponsableUtId = null;
                                ingreso.UtAsignadaId = null;
                                ingreso.UtTemporalId = null;
                                ingreso.UnidadTecnicaCopia = new List<GenericoDto>();
                                ingreso.UtApoyoId = null;
                                ingreso.UtConocimientoId = null;
                                ingreso.UtTransparenciaId = null;
                                ingreso.LiberarAsignacionTemp = false;
                                ingreso.EnviadoUt = null;
                                ingreso.RecepcionUt = null;
                                ingreso.AsignacionResponsable = null;
                                ingreso.ComentarioEncargadoUt = null;
                            }
                            resultado = _repoReq.UpdateAsignarProfesional(ingreso);
                            if (resultado.Codigo < 0)
                            {
                                logSistema.Accion = "ERROR";
                                break;
                            }
                            var fechaPrioOld = ingreso.ForzarPrioridadFecha;

                            // Notificación email
                            resultadoEnvioEmail = EnviarNotificacionEmail(ingreso);

                            resultado.Mensaje = (!ingreso.DevolverAsignacion
                                    ? "Se ha asignado correctamente un Profesional UT." :
                                    "Requerimiento devuelto a Asignación") + resultadoEnvioEmail.Mensaje;

                            // Si se devuelve a asignación se graban dos logs: uno con la acción DEVOLVER_ASIGNACION q es el de siempre y es
                            // para indicar q se devolvió y está pendiente de reasignación; y otro con la acción ASIGNACION_ANTERIOR q es para
                            // mostrar en el log de bitácora los datos de la asignación anterior
                            if (ingreso.DevolverAsignacion)
                            {
                                logSistema.ExtraData = $"UT asignada anterior: {ingreso.UtAsignadaTitulo}. " +
                                                       $"Fecha asignación anterior: {(ingreso.AsignacionAnterior.HasValue ? ingreso.AsignacionAnterior.GetValueOrDefault().ToString(GeneralData.FORMATO_FECHA_CORTO) : "")}. " +
                                                       $"Fecha de devolución: {ingreso.Devolucion.GetValueOrDefault().ToString(GeneralData.FORMATO_FECHA_CORTO)}";
                                logSistema.Accion = "ASIGNACION_ANTERIOR";
                                logSistema.RequerimientoId = ingreso.Id;
                                logSistema.EstadoId = ingreso.EstadoId;
                                logSistema.EtapaId = ingreso.EtapaId;
                                logSistema.UnidadTecnicaId = ingreso.UtAsignadaId;
                                _repoMant.SaveLogSistema(logSistema);
                            }
                            // Si se especifica o cambia la Nueva Fecha de Resolución Estimada se guarda el Log de Bitácora
                            if (!ingreso.DevolverAsignacion && fechaPrioOld.GetValueOrDefault()
                                    .CompareTo(ingreso.ForzarPrioridadFecha.GetValueOrDefault()) != 0)
                            {
                                logSistema.OrigenFecha = ingreso.ForzarPrioridadFecha;
                                logSistema.ExtraData = ingreso.ForzarPrioridadMotivo;
                                logSistema.Accion = "FORZAR_PIORIDAD";
                                logSistema.RequerimientoId = ingreso.Id;
                                logSistema.EstadoId = ingreso.EstadoId;
                                logSistema.EtapaId = ingreso.EtapaId;
                                logSistema.UnidadTecnicaId = ingreso.UtAsignadaId;
                                _repoMant.SaveLogSistema(logSistema);
                                logSistema.OrigenFecha = null;
                            }

                            logSistema.Accion = !ingreso.DevolverAsignacion
                                ? (ingreso.EnAsignacionTemp ? "ASIGNACION_PROF_TEMP" : "ASIGNACION_PROF") : 
                                "DEVOLVER_ASIGNACION";
                            break;
                        // REASIGNACIÓN PROFESIONAL UT
                        case FlujoIngreso.ReasignacionProfUt:
                            ingreso.EstadoId = (int)EstadoIngreso.Asignado;
                            ingreso.EtapaId = (int)EtapaIngreso.UnidadTecnica;
                            ingreso.AsignacionResponsable = DateTime.Now;
                            ingreso.EstadoId = (int)EstadoIngreso.EnProcesoEnEstudio;

                            resultado = _repoReq.UpdateReasignarProfesional(ingreso);
                            if (resultado.Codigo < 0)
                            {
                                logSistema.Accion = "ERROR";
                                break;
                            }

                            // Notificación email
                            resultadoEnvioEmail = EnviarNotificacionEmail(ingreso);

                            resultado.Mensaje = "Se ha reasignado correctamente el Profesional UT." + resultadoEnvioEmail.Mensaje;

                            logSistema.ExtraData = "RESP_ANT:" +
                                                   ingreso.ProfesionalNombreAnt +
                                                   "FECHA_ANT:" +
                                                   (ingreso.AsignacionResponsableAnt.HasValue ? ingreso.AsignacionResponsableAnt.Value.ToString(GeneralData.FORMATO_FECHA_LARGO) : "");
                            logSistema.Accion = "EDICION";
                            break;
                        // EDITAR REQUERIMIENTO
                        case FlujoIngreso.EditarIngreso:
                            //var liberaTemp = false;
                            if (ingreso.LiberarAsignacionTemp &&
                                !ingreso.LiberarAsignacionTempAnt &&
                                ingreso.UtTemporalId.GetValueOrDefault(0) > 0)
                            { // Se libera la asignación temporal
                                //liberaTemp = true;
                                ingreso.EstadoId = (int)EstadoIngreso.Asignado;
                                ingreso.EtapaId = (int)EtapaIngreso.UnidadTecnica;
                                #region Asignación de Responsable de UT
                                var respUt = GetResponsableUt(ingreso.UtAsignadaId.GetValueOrDefault(0));
                                ingreso.ResponsableUtId = respUt.Id;
                                #endregion
                                ingreso.EnviadoUt = DateTime.Now;
                                ingreso.Liberacion = DateTime.Now;
                            } else if (ingreso.UtAsignadaId.HasValue)
                            {
                                // Se obtiene el responsable de la ut asignada
                                var respUt = GetResponsableUt(ingreso.UtAsignadaId.GetValueOrDefault(0));
                                ingreso.ResponsableUtId = respUt.Id;
                            }
                            // Cálculo del plazo y fecha resolución estimada en base a la prioridad seleccionada
                            // TODO: actualizar el Plazo y Fecha Resolución solo si se modificó la prioridad en el formulario, y además guardar un log de priorización
                            if (!string.IsNullOrWhiteSpace(ingreso.PrioridadCod))
                            {
                                var prior2 = _repoMant.GetPrioridadById(ingreso.PrioridadCod);
                                if (prior2 == null)
                                {
                                    resultado.Codigo = -1;
                                    resultado.Mensaje = "No se encontró información de la prioridad especificada.";
                                    return resultado;
                                }
                                ingreso.Plazo = prior2.Plazo;
                                ingreso.Resolucion = _repoMant.GetFechaDiasHabiles(ingreso.FechaIngreso, prior2.Plazo);
                            }
                            else
                            {
                                ingreso.Plazo = null;
                                ingreso.Resolucion = null;
                            }

                            resultado = _repoReq.UpdateEditarRequerimiento(ingreso);
                            if (resultado.Codigo < 0)
                            {
                                logSistema.Accion = "ERROR";
                                break;
                            }

                            // La notificación de campos modificados en el form de Editar Requerimiento se envía después
                            // de grabar, no aquí, y si el usuario acepta enviar la notificación

                            msg = GetModificadosEditarRequerimiento(ingreso, null);
                            resultado.Codigo = string.IsNullOrWhiteSpace(msg) ?  1 : 2; // 2 para identificar en el view q la respuesta es Ok al grabar en Editar Requerimiento
                            resultado.Mensaje = "Se ha grabado satisfactoriamente el registro." + (string.IsNullOrWhiteSpace(msg) ? "" : "¿Desea enviar notificación por cambio de: <br/>" + msg + " ?");
                            resultado.Extra = ingreso.BackupData;

                            logSistema.Accion = "EDICION";
                            break;
                        // EDITAR CAMPOS UT
                        case FlujoIngreso.EditarCamposUt:
                            if (ingreso.LiberarAsignacionTemp &&
                                !ingreso.LiberarAsignacionTempAnt &&
                                ingreso.UtTemporalId.GetValueOrDefault(0) > 0)
                            { // Se libera la asignación temporal
                                ingreso.EstadoId = (int)EstadoIngreso.Asignado;
                                ingreso.EtapaId = (int)EtapaIngreso.UnidadTecnica;
                                #region Asignación de Responsable de UT
                                var respUt = GetResponsableUt(ingreso.UtAsignadaId.GetValueOrDefault(0));
                                ingreso.ResponsableUtId = respUt.Id;
                                #endregion
                                ingreso.EnviadoUt = DateTime.Now;
                                ingreso.Liberacion = DateTime.Now;
                                // Se graba Log de Liberación de Ut Temporal
                                logSistema.Accion = "LIBERACION_TEMPORAL";
                                logSistema.RequerimientoId = ingreso.Id;
                                logSistema.EstadoId = ingreso.EstadoId;
                                logSistema.EtapaId = ingreso.EtapaId;
                                logSistema.UnidadTecnicaId = ingreso.UtAsignadaId;
                                _repoMant.SaveLogSistema(logSistema);
                            }

                            resultado = _repoReq.UpdateEditarCamposUt(ingreso);
                            if (resultado.Codigo < 0)
                            {
                                logSistema.Accion = "ERROR";
                                break;
                            }

                            // Notificación email
                            resultadoEnvioEmail = EnviarNotificacionEmail(ingreso);

                            resultado.Mensaje = "Se ha grabado satisfactoriamente el registro." + resultadoEnvioEmail.Mensaje; ;

                            // Si se modificó o asignó la UT de Apoyo se graba el log
                            if (ingreso.UtApoyoId.GetValueOrDefault(0) > 0 &&
                                ingreso.BackupData.UtApoyoIdAnt != ingreso.UtApoyoId.GetValueOrDefault(0))
                            {
                                logSistema.Accion = "ASIGNA-UT-APOYO";
                                logSistema.RequerimientoId = ingreso.Id;
                                logSistema.EstadoId = ingreso.EstadoId;
                                logSistema.EtapaId = ingreso.EtapaId;
                                logSistema.UnidadTecnicaId = ingreso.UtAsignadaId;
                                _repoMant.SaveLogSistema(logSistema);
                            }

                            // Log de edición
                            logSistema.Accion = "EDICION";
                            break;
                        // CIERRE
                        case FlujoIngreso.RequerimientoCierre:
                            if (ingreso.CerrarReq)
                            { // Se cierra el requerimiento
                                ingreso.EstadoId = (int)EstadoIngreso.Cerrado;
                                ingreso.Cierre = DateTime.Now;
                                ingreso.CerradoPor = ingreso.UsuarioActual;
                                ingreso.CerradoPorId = ingreso.UsuarioActualId > 0 ? ingreso.UsuarioActualId : 0;
                            }
                            resultado = _repoReq.UpdateCierre(ingreso);
                            if (resultado.Codigo < 0)
                            {
                                logSistema.Accion = "ERROR";
                                break;
                            }

                            // Notificación email
                            if (ingreso.CerrarReq)
                            {
                                resultadoEnvioEmail = EnviarNotificacionEmail(ingreso);
                            }

                            msg = ingreso.CerrarReq ? "Se ha cerrado satisfactoriamente el requerimiento. " : "Se han grabado con éxito los cambios. El requerimiento no ha sido cerrado aún. ";
                            resultado.Mensaje = msg + resultadoEnvioEmail.Mensaje;

                            logSistema.Accion = ingreso.CerrarReq ? "CIERRE" : "EDICION";
                            break;
                    }
                }

                // Grabar log de la operación realizada
                logSistema.RequerimientoId = ingreso.Id;
                logSistema.EstadoId = ingreso.EstadoId;
                logSistema.EtapaId = ingreso.EtapaId;
                logSistema.UnidadTecnicaId = ingreso.UtAsignadaId;
                _repoMant.SaveLogSistema(logSistema);

                // Grabar el log (bitácora) de campos modificados
                var logBit = new List<LogSistemaDto>();
                foreach (var cambio in (ingreso.ControlCambios?.CamposModificados ?? new Dictionary<string, object>()) )
                {
                    var valorCampo = (cambio.Value ?? "").ToString();
                    DateTime? valorFecha = cambio.Key == "RecepcionUt" && !string.IsNullOrWhiteSpace(valorCampo)
                        ? DateTime.ParseExact(valorCampo, GeneralData.FORMATO_FECHA_CORTO, System.Globalization.CultureInfo.InvariantCulture)
                        : (DateTime?) null;
                    var nombreCampo = GeneralData.CamposControlCambio.ContainsKey(cambio.Key) ? GeneralData.CamposControlCambio[cambio.Key].ToUpper() : cambio.Key.ToUpper();
                    logSistema = new LogSistemaDto()
                    {
                        Flujo = ingreso.Flujo.ToString().ToUpper(),
                        Accion = "EDICIONCAMPO",
                        Origen = nombreCampo,
                        OrigenFecha = valorFecha,
                        ExtraData = valorCampo,
                        Fecha = DateTime.Now,
                        Usuario = ingreso.UsuarioActual ?? "<desconocido>",
                        UsuarioId = ingreso.UsuarioActualId,
                        RequerimientoId = ingreso.Id,
                        EstadoId = ingreso.EstadoId,
                        EtapaId = ingreso.EtapaId,
                        UnidadTecnicaId = ingreso.UtAsignadaId
                    };
                    logBit.Add(logSistema);
                }
                _repoMant.SaveLogSistemaMulti(logBit);
            }
            catch (DbUpdateException ex)
            {
                LogError(null, ex, resultado.Mensaje);
                SqlException innerException = ex.InnerException.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {
                    if (innerException.Message.Contains("'Requerimiento_uq'"))
                    {
                        resultado.Mensaje = "El Documento de Ingreso especificado ya existe.";
                    } else
                    {
                        resultado.Mensaje = "Se está intentando adicionar datos duplicados en la base de datos. Por favor, revise el log de errores.";
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
            }
            return resultado;

        }

        public ResultadoOperacion MarcaEliminado(int reqId, UsuarioActualDto usuario)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var ingreso = _repoReq.MarcaEliminado(reqId, usuario.UsuarioId);
                if (ingreso == null)
                {
                    resultado.Mensaje = "No se encontró el requerimiento a eliminar.";
                } else
                {
                    var logSistema = new LogSistemaDto()
                    {
                        Flujo = "ELIMINARINGRESO",
                        Fecha = DateTime.Now,
                        Origen = "REQUERIMIENTO",
                        Usuario = usuario?.LoginName ?? "<desconocido>",
                        UsuarioId = usuario.UsuarioId,
                        DireccionIp = usuario.DireccionIp,
                        NombrePc = usuario.NombrePc,
                        UserAgent = usuario.UserAgent,
                        Accion = "ELIMINADO",
                        EstadoId = ingreso.EstadoId,
                        EtapaId = ingreso.EtapaId,
                        OrigenFecha = ingreso.FechaIngreso,
                        OrigenId = ingreso.Id,
                        RequerimientoId = ingreso.Id,
                        UnidadTecnicaId = ingreso.UtAsignadaId
                    };
                    _repoMant.SaveLogSistema(logSistema);

                    resultado.Codigo = 1;
                    resultado.Mensaje = "El requerimiento ha sido eliminado.";
                }
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
            }
            return resultado;

        }

        public ResultadoOperacion SaveForzarPrioridad(ParametrosGrillaDto<int> param, UsuarioActualDto usuario, DateTime? nuevaFecha, int? idRequerimiento)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito. ", null);
            try
            {
                var ids = new List<int>();
                var extraTexto = "";
                if (idRequerimiento.GetValueOrDefault(0) > 0)
                { // Se especifica solo 1 requerimiento a actualizar
                    ids.Add(idRequerimiento.GetValueOrDefault());
                }
                else
                {  // Actualizar todos los requerimientos q hay en la grilla
                    // La grilla de priorizados funciona del lado del servidor por lo q no tengo en la grilla todos los requerimientos
                    // a actualizar cuando hay varias páginas y se selecciona la opción de Actualizar Todos. Hay q ejecutar el método de
                    //obtención de datos para la grilla, pero sin tener en cuenta el skip y el take para q abarque todos los registros, y con esos ids de 
                    //requerimientos q se obtienen entonces actualizar el Nueva Fecha de Resolución Estimada
                    param.ExtraData = "IDREQ";
                    var datos = GetDatosBandejaPriorizados(param, usuario.UsuarioId);
                    if (datos.Resultado.Codigo < 0)
                    {
                        resultado.Codigo = -1;
                        resultado.Mensaje = "Ha ocurrido un error al actualizar los datos.";
                        return resultado;
                    }
                    ids = datos.Data.Any() ? ((datos.Data[0].Items as List<int>) ?? new List<int>()) : new List<int>();
                    extraTexto = $" Se actualizaron {ids.Count} registros.";
                }
                resultado.Mensaje += extraTexto;
                var reqs = _repoReq.UpdateForzarPrioridad(ids, nuevaFecha);

                // Envio de Notificación de Forzar Prioridad
                if (nuevaFecha.HasValue)
                {
                    foreach (var ingreso in reqs)
                    {
                        _repoReq.GetRequirimientoDetalle(ingreso); // Obtener de Bd los datos necesarios para mostrar en el email de notificación
                        var resultadoEnvioEmail = _notifSrv.NotificacionForzarPriorizacion(ingreso);
                    }
                }

                /* Log de bitácora */
                var logsSist = new List<LogSistemaDto>();
                foreach (var req in reqs)
                {
                    var logSistema = new LogSistemaDto()
                    {
                        Flujo = "BANDEJA_PRIORIZADOS",
                        Accion = "FORZAR_PIORIDAD",
                        Fecha = DateTime.Now,
                        OrigenFecha = nuevaFecha,
                        Origen = "REQUERIMIENTO",
                        Usuario = "",
                        UsuarioId = usuario.UsuarioId,
                        DireccionIp = usuario.DireccionIp,
                        NombrePc = usuario.NombrePc,
                        UserAgent = usuario.UserAgent,
                        ExtraData = nuevaFecha.HasValue ? "" : "Se eliminó la Nueva Fecha de Resolución Estimada",
                        RequerimientoId = req.Id,
                        EstadoId = req.EstadoId,
                        EtapaId = req.EtapaId,
                        UnidadTecnicaId = req.UtAsignadaId,
                        OrigenId = req.Id
                    };
                    logsSist.Add(logSistema);
                }
                _repoMant.SaveLogSistemaMulti(logsSist) ;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                resultado.Codigo = -1;
                resultado.Mensaje = "Ha ocurrido un error al actualizar los datos.";
            }

            return resultado;
        }

        public ResultadoOperacion EnviarNotificacionEmail(RequerimientoDto ingreso)
        {
            var resultadoEnvioEmail = new ResultadoOperacion(1, "", null);
            var resultadoEnvioEmail2 = new ResultadoOperacion(1, "", null);
            try
            {
                switch (ingreso.Flujo)
                {
                    case FlujoIngreso.IngresoCentral:
                        break;
                    case FlujoIngreso.AsignacionUt:
                        _repoReq.GetRequirimientoDetalle(ingreso); // Obtener de Bd los datos necesarios para mostrar en el email de notificación
                        if (ingreso.EnAsignacionTemp && ingreso.EnviarAsignacionTemp.GetValueOrDefault(false))
                        { // Envio de Notificación de asignación temporal
                            resultadoEnvioEmail = _notifSrv.NotificacionAsignacionTemp(ingreso);
                        }
                        else
                        {  // Envio de Notificación de Reasignación de ut (la notificación de Asignacion de Ut se envía cuando se prioriza, no aquí)
                            //if (ingreso.EsReasignacion)
                            //{
                            //    resultadoEnvioEmail = _notifSrv.NotificacionReasignacion(ingreso);
                            //}
                        }
                        resultadoEnvioEmail.Mensaje = resultadoEnvioEmail.Codigo == -2
                            ? " (Error al enviar notificación: " + resultadoEnvioEmail.Mensaje + ")"
                            : (resultadoEnvioEmail.Codigo < 0
                                ? " (Ha ocurrido un error al enviar el email de notificación)"
                                : "");
                        break;
                    case FlujoIngreso.ReasignacionUt:
                        _repoReq.GetRequirimientoDetalle(ingreso); // Obtener de Bd los datos necesarios para mostrar en el email de notificación
                        resultadoEnvioEmail = _notifSrv.NotificacionReasignacion(ingreso);
                        resultadoEnvioEmail.Mensaje = resultadoEnvioEmail.Codigo == -2
                            ? " (Error al enviar notificación: " + resultadoEnvioEmail.Mensaje + ")"
                            : (resultadoEnvioEmail.Codigo < 0
                                ? " (Ha ocurrido un error al enviar el email de notificación)"
                                : "");
                        break;
                    case FlujoIngreso.Priorizacion:
                        if (ingreso.EnviarUt.GetValueOrDefault(false))
                        {
                            _repoReq.GetRequirimientoDetalle(ingreso); // Obtener de Bd los datos necesarios para mostrar en el email de notificación
                            // Notificación a UT Asignada
                            resultadoEnvioEmail = _notifSrv.NotificacionAsignacionUt(ingreso, false);
                            // Notificación a UTs en Copia si hay
                            var utCopiaTitulos = "";
                            if (ingreso.UnidadTecnicaCopia?.Count > 0) // TODO: revisar si ingreso.UnidadTecnicaCopia toiene elementos
                            {
                                utCopiaTitulos = string.Join(";",
                                    ingreso.UnidadTecnicaCopia.Select(u => u.Titulo).ToList());
                                // Se envía una notificación por separado para cada UT en copia para q en el mensaje y asunto sea directamente para la UT
                                foreach (var ut in ingreso.UnidadTecnicaCopia)
                                {
                                    _notifSrv.VariablesMensaje = new Dictionary<string, string>() {
                                        {"UT_Copia", ut.Titulo}
                                    };
                                    var resultadoTmp = _notifSrv.NotificacionAsignacionUtCopia(ingreso, ut.IdInt);
                                    resultadoEnvioEmail = resultadoEnvioEmail.Codigo < 0 ? resultadoEnvioEmail : resultadoTmp; //si hubo error antes en el envío enton
                                }
                            }
                            // Notificación a UT en Conocimiento
                            if (ingreso.UtConocimientoId.GetValueOrDefault() > 0)
                            {
                                _notifSrv.VariablesMensaje = new Dictionary<string, string>() {
                                    {"UT_Copia", utCopiaTitulos}
                                };
                                var resultadoTmp = _notifSrv.NotificacionAsignacionUtConoc(ingreso);
                                resultadoEnvioEmail = resultadoEnvioEmail.Codigo < 0 ? resultadoEnvioEmail : resultadoTmp; //si hubo error antes en el envío entonces conservo el error
                            }
                        }
                        resultadoEnvioEmail.Mensaje = resultadoEnvioEmail.Codigo == -2
                            ? " (Error al enviar notificación: " + resultadoEnvioEmail.Mensaje + ")"
                            : (resultadoEnvioEmail.Codigo < 0
                                ? " (Ha ocurrido un error al enviar el email de notificación)"
                                : "");
                        break;
                    case FlujoIngreso.AsignacionProfUt:
                        if (!ingreso.DevolverAsignacion && (ingreso.ProfesionalId.GetValueOrDefault() > 0 || ingreso.ProfesionalTempId.GetValueOrDefault() > 0))
                        { 
                            // Envio de Notificación de  Asignación de profesiona
                            _repoReq.GetRequirimientoDetalle(ingreso); // Obtener de Bd los datos necesarios para mostrar en el email de notificación
                            resultadoEnvioEmail = _notifSrv.NotificacionAsignacionProfUt(ingreso);

                            // Envio de Notificación de Forzar Prioridad
                            if (ingreso.ProfesionalId.GetValueOrDefault() > 0 && ingreso.ForzarPrioridadFecha.HasValue)
                            {
                                resultadoEnvioEmail2 = _notifSrv.NotificacionForzarPriorizacion(ingreso);
                                resultadoEnvioEmail =
                                    resultadoEnvioEmail.Codigo < 0 ? resultadoEnvioEmail : resultadoEnvioEmail2;
                            }
                        }
                        resultadoEnvioEmail.Mensaje = resultadoEnvioEmail.Codigo == -2
                            ? " (Error al enviar notificación: " + resultadoEnvioEmail.Mensaje + ")"
                            : (resultadoEnvioEmail.Codigo < 0
                                ? " (Ha ocurrido un error al enviar el email de notificación)"
                                : "");
                        break;
                    case FlujoIngreso.ReasignacionProfUt:

                        break;
                    case FlujoIngreso.EditarIngreso:
                        var tipoNotif = new List<TipoNotificacionEmail>();
                        GetModificadosEditarRequerimiento(ingreso, tipoNotif);
                        if (tipoNotif?.Count > 0)
                        {
                            _repoReq.GetRequirimientoDetalle(ingreso); // Obtener de Bd los datos necesarios para mostrar en el email de notificación
                        }

                        foreach (var notif in tipoNotif)
                        {
                            switch (notif)
                            {
                                case TipoNotificacionEmail.UT_ASIGNACION:
                                    resultadoEnvioEmail = _notifSrv.NotificacionAsignacionUt(ingreso, false);
                                    break;
                                case TipoNotificacionEmail.UT_REASIGNADA:
                                    resultadoEnvioEmail = _notifSrv.NotificacionReasignacion(ingreso);
                                    break;
                                case TipoNotificacionEmail.UT_COPIA_MODIF:
                                    var backUtCopiaId = (ingreso.BackupData?.UnidadTecnicaCopia ?? new List<GenericoDto>())
                                        .Select(u => u.Id).ToList();
                                    var utCopiaNuevas = (ingreso.UnidadTecnicaCopia ?? new List<GenericoDto>())
                                        .Where(u => !backUtCopiaId.Contains(u.Id))
                                        .ToList();
                                    // Se envía una notificación por separado para cada UT en copia para q en el mensaje y asunto sea directamente para la UT
                                    foreach (var ut in utCopiaNuevas)
                                    {
                                        _notifSrv.VariablesMensaje = new Dictionary<string, string>() {
                                            {"UT_Copia", ut.Titulo}
                                        };
                                        var resultadoTmp = _notifSrv.NotificacionAsignacionUtCopia(ingreso, ut.IdInt);
                                        resultadoEnvioEmail = resultadoEnvioEmail.Codigo < 0 ? resultadoEnvioEmail : resultadoTmp; //si hubo error antes en el envío enton
                                    }
                                    break;
                                case TipoNotificacionEmail.UT_CONOCIMIENTO_MODIF:
                                    resultadoEnvioEmail = _notifSrv.NotificacionAsignacionUtConoc(ingreso);
                                    break;
                                case TipoNotificacionEmail.UT_TEMPORAL_REASIG:
                                    resultadoEnvioEmail = _notifSrv.NotificacionAsignacionTemp(ingreso);
                                    break;
                                case TipoNotificacionEmail.LIBERACION_UT_TEMP:
                                    resultadoEnvioEmail = _notifSrv.NotificacionLiberacionAsignacionTemporal(ingreso);
                                    break;
                                //case Notificacion.UT_PROFESIONAL_REASIG:
                                //    resultadoEnvioEmail = _notifSrv.NotificacionAsignacionProfUt(ingreso);
                                //    break;
                                case TipoNotificacionEmail.PRIORIZACION:
                                    resultadoEnvioEmail = _notifSrv.NotificacionCambioPriorizacion(ingreso);
                                    break;
                                case TipoNotificacionEmail.PRIORIDAD_MODIF:
                                    resultadoEnvioEmail = _notifSrv.NotificacionCambioPriorizacion(ingreso);
                                    break;
                            }
                        }
                        resultadoEnvioEmail.Mensaje = resultadoEnvioEmail.Codigo == -2
                            ? " (Error al enviar notificación: " + resultadoEnvioEmail.Mensaje + ")"
                            : (resultadoEnvioEmail.Codigo < 0
                                ? " (Ha ocurrido un error al enviar el email de notificación)"
                                : "Se ha enviado con éxito las notificaciones.");
                        break;
                    case FlujoIngreso.EditarCamposUt:
                        _repoReq.GetRequirimientoDetalle(ingreso); // Obtener de Bd los datos necesarios para mostrar en el email de notificación
                        // Notificación de asignación de Ut de Apoyo si procede
                        if (ingreso.UtApoyoId.GetValueOrDefault(0) > 0 &&
                            ingreso.BackupData.UtApoyoIdAnt != ingreso.UtApoyoId.GetValueOrDefault(0))
                        {
                            resultadoEnvioEmail = _notifSrv.NotificacionAsignacionUtApoyo(ingreso);
                        }
                        // Notificación de liberación de UT Temporal
                        if (ingreso.LiberarAsignacionTemp &&
                            !ingreso.LiberarAsignacionTempAnt &&
                            ingreso.UtTemporalId.GetValueOrDefault(0) > 0)
                        {
                            resultadoEnvioEmail2 = _notifSrv.NotificacionLiberacionAsignacionTemporal(ingreso);
                            resultadoEnvioEmail =
                                resultadoEnvioEmail.Codigo < 0 ? resultadoEnvioEmail : resultadoEnvioEmail2;
                        }
                        resultadoEnvioEmail.Mensaje = resultadoEnvioEmail.Codigo == -2
                            ? " (Error al enviar notificación: " + resultadoEnvioEmail.Mensaje + ")"
                            : (resultadoEnvioEmail.Codigo < 0
                                ? " (Ha ocurrido un error al enviar el email de notificación)"
                                : "");
                        break;
                    case FlujoIngreso.RequerimientoCierre:
                        _repoReq.GetRequirimientoDetalle(ingreso); // Obtener de Bd los datos necesarios para mostrar en el email de notificación
                        _notifSrv.VariablesMensaje = new Dictionary<string, string>() {
                                    {"FechaEmisionOficio", _repoReq.GetFechasEmisionOficioReq(ingreso.Id) }
                                };
                        resultadoEnvioEmail = _notifSrv.NotificacionCierre(ingreso);

                        resultadoEnvioEmail.Mensaje = resultadoEnvioEmail.Codigo == -2
                            ? " (Error al enviar notificación: " + resultadoEnvioEmail.Mensaje + ")"
                            : (resultadoEnvioEmail.Codigo < 0
                                ? " (Ha ocurrido un error al enviar el email de notificación)"
                                : "");
                        break;
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                resultadoEnvioEmail.Codigo = -1;
                resultadoEnvioEmail.Mensaje = "(Ha ocurrido un error al enviar el email de notificación)";
            }

            return resultadoEnvioEmail;
        }

        private string GetModificadosEditarRequerimiento(RequerimientoDto requerimiento, List<TipoNotificacionEmail> tipoNotif)
        {
            List<string> texto = new List<string>();
            if (requerimiento.UtAsignadaId != requerimiento.BackupData?.UtAsignadaId)
            {
                texto.Add("UT Asignada");
                if (tipoNotif != null) tipoNotif.Add(requerimiento.UtAsignadaId.GetValueOrDefault() > 0 ? TipoNotificacionEmail.UT_REASIGNADA : TipoNotificacionEmail.UT_ASIGNACION);
            }
            if (requerimiento.LiberarAsignacionTemp &&
                !requerimiento.LiberarAsignacionTempAnt &&
                requerimiento.UtTemporalId.GetValueOrDefault(0) > 0)
            {
                texto.Add("Liberación Temporal");
                if (tipoNotif != null) tipoNotif.Add(TipoNotificacionEmail.LIBERACION_UT_TEMP);
            }
            if (requerimiento.PrioridadCod != requerimiento.BackupData?.PrioridadCod)
            {
                texto.Add("Prioridad");
                if (tipoNotif != null) tipoNotif.Add(!string.IsNullOrWhiteSpace(requerimiento.PrioridadCod) ? TipoNotificacionEmail.PRIORIDAD_MODIF : TipoNotificacionEmail.PRIORIZACION);
            }
            if (requerimiento.UtTemporalId != requerimiento.BackupData?.UtTemporalId)
            {
                texto.Add("UT Temporal");
                if (tipoNotif != null) tipoNotif.Add(TipoNotificacionEmail.UT_TEMPORAL_REASIG);
            }
            var backUtCopiaId = (requerimiento.BackupData?.UnidadTecnicaCopia ?? new List<GenericoDto>())
                .Select(u => u.Id).ToList();
            var utCopiaId = (requerimiento.UnidadTecnicaCopia ?? new List<GenericoDto>())
                .Select(u => u.Id).ToList();
            if (utCopiaId.Any(id => !backUtCopiaId.Contains(id)) || backUtCopiaId.Any(id => !utCopiaId.Contains(id)))
            {
                texto.Add("UT en Copia");
                if (tipoNotif != null) tipoNotif.Add(TipoNotificacionEmail.UT_COPIA_MODIF);
            }
            if (requerimiento.UtConocimientoId != requerimiento.BackupData?.UtConocimientoId)
            {
                texto.Add("UT en Conocimiento");
                if (tipoNotif != null) tipoNotif.Add(TipoNotificacionEmail.UT_CONOCIMIENTO_MODIF);
            }

            return string.Join(", ", texto);
        }

        public UsuarioDto GetResponsableUt(int utId)
        {
            if (utId == 0)
            {
                return new UsuarioDto();
            }
            return _repoMant.GetResponsableUt(utId);
        }

        public DatosAjax<List<RequerimientoDto>> GetIngresosUltimos(int diasAtras)
        {
            var resultado = new DatosAjax<List<RequerimientoDto>>(new List<RequerimientoDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
            try
            {
                resultado = _repoReq.GetDatosUltimos(diasAtras);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return resultado;
        }

        //public DatosAjax<List<RequerimientoDto>> GetByUnidadTecnica(int idUT, int? idProfesional)
        //{
        //    var resultado = new DatosAjax<List<RequerimientoDto>>(new List<RequerimientoDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
        //    try
        //    {
        //        resultado = _repoReq.GetByUnidadTecnica(idUT, idProfesional);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogError(ex);
        //    }
        //    return resultado;
        //}

        public List<RequerimientoDto> GetDatosCierreMultiple(int idUT, int idProfesional, DateTime fechaDesde, DateTime fechaHasta)
        {
            try
            {
                return _repoReq.GetDatosCierreMultiple(idUT, idProfesional, fechaDesde, fechaHasta);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return new List<RequerimientoDto>();
        }

        public DatosAjax<List<UsuarioDto>> GetProfesionales(int idUT, int? idProfesional)
        {
            var resultado = new DatosAjax<List<UsuarioDto>>(new List<UsuarioDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
            try
            {
                resultado = _repoReq.GetProfesionales(idUT, idProfesional);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return resultado;
        }

        public DatosAjax<List<RequerimientoDto>> GetDatosBandejaEntrada(ParametrosGrillaDto<int> param, int idUsuario)
        {
            var resultado = new DatosAjax<List<RequerimientoDto>>(new List<RequerimientoDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos de la bandeja.", null));
            try
            {
                var idBandeja = param.Dato;
                var configBandeja = _repoMant.GetConfigBandeja(idBandeja, true);
                if (configBandeja == null)
                {
                    resultado.Resultado.Codigo = -1;
                    resultado.Resultado.Mensaje = "No se pueden mostrar los datos de la Bandeja de Entrada, no se encontró la configuración de la bandeja.";
                    return resultado;
                }

                // TODO: si el usuario no tiene dentro de los permisos acceder a la bandeja entonces devolver
                // error para evitar q se acceda especificando directamente la Url de la bandeja
                var take = param.Take > 0 ? param.Take : param.PageSize;
                var skip = param.Skip == 0 && param.Page > 0 ? ((param.Page - 1) * take) : param.Skip;
                var sort = param.Sort == null
                    ? new SortParam() { Field = "FechaIngreso", Dir = "DESC" }
                    : new SortParam() { Field = param.Sort.Split('-')[0], Dir = param.Sort.Contains("-") ? param.Sort.Split('-')[1] : "DESC" };

                var fechaDesde = param.FechaDesde ?? DateTime.Now.AddDays(configBandeja.DiasAtras > 0 ? -1 * configBandeja.DiasAtras : configBandeja.DiasAtras);

                resultado = _repoReq.GetDatosBandejaEntrada(idBandeja, configBandeja.IdBandeja, skip, take, sort, 
                    param.FilterText, param.Filter, param.FilterParameters, fechaDesde,
                    idUsuario, param.DocumentoIngreso, param.FechaHasta, param.UnidadTecnica, param.Estado);
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de la Bandeja de Entrada.");
            }
            return resultado;
        }

        public DatosAjax<List<GroupResult>> GetDatosBandejaPriorizados(ParametrosGrillaDto<int> param, int idUsuario)
        {
            var resultado = new DatosAjax<List<GroupResult>>(new List<GroupResult>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos de la bandeja.", null));
            try
            {
                var idBandeja = param.Dato;
                var configBandeja = _repoMant.GetConfigBandeja(idBandeja, true);
                if (configBandeja == null)
                {
                    resultado.Resultado.Codigo = -1;
                    resultado.Resultado.Mensaje = "No se pueden mostrar los datos de la pestaña Priorizados, no se encontró la configuración de la bandeja.";
                    return resultado;
                }

                // TODO: si el usuario no tiene dentro de los permisos acceder a la bandeja entonces devolver
                // error para evitar q se acceda especificando directamente la Url de la bandeja
                var take = param.Take > 0 ? param.Take : param.PageSize;
                var skip = param.Skip == 0 && param.Page > 0 ? ((param.Page - 1) * take) : param.Skip;
                var sort = param.Sort == null
                    ? new SortParam() { Field = "FechaIngreso", Dir = "DESC" }
                    : new SortParam() { Field = param.Sort.Split('-')[0], Dir = param.Sort.Contains("-") ? param.Sort.Split('-')[1] : "DESC" };

                var fechaDesde = param.FechaDesde ?? DateTime.Now.AddDays(configBandeja.DiasAtras > 0 ? -1 * configBandeja.DiasAtras : configBandeja.DiasAtras);

                resultado = _repoReq.GetDatosBandejaPriorizados(idBandeja, configBandeja.IdBandeja, skip, take, sort, 
                    param.FilterText, param.Filter, param.FilterParameters, fechaDesde,
                    idUsuario, param.DocumentoIngreso, param.FechaHasta, param.UnidadTecnica, param.Estado, param.ExtraData);
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de la pestaña Priorizados.");
            }
            return resultado;
        }

        public RequerimientoDto GetById(int id)
        {
            var datos = _repoReq.GetById(id);
            if (datos != null)
            {
                datos.EnviarAsignacion = datos.EnviarAsignacion.GetValueOrDefault(false);
                datos.EnviarPriorizacion = datos.EnviarPriorizacion.GetValueOrDefault(false);
                datos.EnviarUt = datos.EnviarUt.GetValueOrDefault(false);
                datos.RequiereAcuerdo = datos.RequiereAcuerdo.GetValueOrDefault(false);
                datos.RequiereRespuesta = datos.RequiereRespuesta.GetValueOrDefault(true);
                datos.RequiereTimbrajePlano = datos.RequiereTimbrajePlano.GetValueOrDefault(false);
                datos.Redireccionado = datos.Redireccionado.GetValueOrDefault(false);
                datos.EsTransparencia = datos.TipoIngreso == GD.TIPO_INGRESO_TRANSP;
                datos.EsTransparenciaAnt = datos.EsTransparencia;
                datos.LiberarAsignacionTempAnt = datos.LiberarAsignacionTemp;
                if (datos.EsTransparencia && datos.UtAsignadaId == null)
                {
                    datos.UtAsignadaId =
                        WebConfigValues.Ut_Transparencia == 0 
                            ? 77 
                            : WebConfigValues.Ut_Transparencia; // TODO: poner un campo en mantenedor de UT q identifique la UT de Transparencia
                }
            }
            return datos;
        }

        public RequerimientoDto GetByDocumentoIngreso(string docIngreso)
        {
            try
            {
                return _repoReq.GetByDocumentoIngreso(docIngreso);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new RequerimientoDto();
            }
        }

        public RequerimientoDto GetResumenByDocumentoIngreso(string docIngreso)
        {
            try
            {
                return _repoReq.GetResumenByDocumentoIngreso(docIngreso);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new RequerimientoDto();
            }
        }

        public RequerimientoDto GetBySolicitudId(int idSolic)
        {
            try
            {
                return _repoReq.GetBySolicitudId(idSolic) ?? new RequerimientoDto();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public DatosAjax<RequerimientoDto> GetFichaById(int id, bool fullDatos = false)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<RequerimientoDto>(new RequerimientoDto(), resultadoOper);
            try
            {
                var datos = _repoReq.GetFichaById(id, fullDatos);
                if (datos == null)
                {
                    resultado.Resultado.Codigo = -1;
                    resultado.Resultado.Mensaje = "No se encontró el requerimiento especificado.";
                } else
                {
                    datos.EnAsignacionTemp = datos.UtTemporalId.GetValueOrDefault(0) > 0 && !datos.LiberarAsignacionTemp;
                    datos.EsTransparencia = datos.TipoIngreso == GD.TIPO_INGRESO_TRANSP;
                    resultado.Data = datos;
                }
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos del requerimiento.<br/>Por favor, revise el fichero log de errores.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public string GetFechasEmisionOficioReq(int idReq)
        {
            try
            {
                return _repoReq.GetFechasEmisionOficioReq(idReq);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return "";
            }
        }

        public DatosAjax<List<RequerimientoDto>> GetResumenAll(bool soloCerrados)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<RequerimientoDto>>(new List<RequerimientoDto>(), resultadoOper);
            try
            {
                var datos = _repoReq.GetResumenAll(soloCerrados);
                resultado.Data = datos;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los requerimientos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public DatosAjax<List<GenericoDto>> GetRequerimientoResumenByIds(List<int> ids, bool cerrado)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<GenericoDto>>(new List<GenericoDto>(), resultadoOper);
            try
            {
                var datos = _repoReq.GetRequerimientoResumenByIds(ids, cerrado);
                resultado.Data = datos;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de requerimientos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        #region Vistas

        public DatosAjax<List<RequerimientoDto>> GetDatosVistas(ParametrosGrillaDto<int> param)
        {
            var resultado = new DatosAjax<List<RequerimientoDto>>(new List<RequerimientoDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
            try
            {
                var idVista = param.Dato;
                var take = param.Take > 0 ? param.Take : param.PageSize;
                var skip = param.Skip == 0 && param.Page > 0 ? ((param.Page - 1) * take) : param.Skip;
                var sort = param.Sort == null
                    ? new SortParam() { Field = "FechaIngreso", Dir = "DESC" }
                    : new SortParam() { Field = param.Sort.Split('-')[0], Dir = param.Sort.Contains("-") ? param.Sort.Split('-')[1] : "DESC" };

                switch (idVista)
                {
                    case 1: // Requerimientos por etiqueta
                        resultado = _repoReq.GetDatosVistaEtiqueta(skip, take, sort, param.FilterText);
                        break;
                    case 2: // Requerimientos por remitente
                        resultado = _repoReq.GetDatosVistaRemitente(skip, take, sort, param.FilterText);
                        break;
                    case 3: // Requerimientos por género
                        /*resultado = _repoReq.GetDatosVistaGenero(skip, take, sort, param.FilterText);
                        break;*/
                    default:
                        resultado.Resultado.Mensaje = "Id de vista incorrecto.";
                        break;
                }
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de la vista.");
            }
            return resultado;
        }

        public DatosAjax<List<GroupResult>> GetDatosVistaGenero(ParametrosGrillaDto<int> param)
        {
            var resultado = new DatosAjax<List<GroupResult>>(new List<GroupResult>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
            try
            {
                var idVista = param.Dato;
                var take = param.Take > 0 ? param.Take : param.PageSize;
                var skip = param.Skip == 0 && param.Page > 0 ? ((param.Page - 1) * take) : param.Skip;
                var sort = param.Sort == null
                    ? new SortParam() { Field = "FechaIngreso", Dir = "DESC" }
                    : new SortParam() { Field = param.Sort.Split('-')[0], Dir = param.Sort.Contains("-") ? param.Sort.Split('-')[1] : "DESC" };
                resultado = _repoReq.GetDatosVistaGenero(skip, take, sort, param.FilterText, param.Filter, param.FilterParameters);
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de la vista.");
            }
            return resultado;
        }

        public List<VistaGeneroDto> GetDatosVistaGeneroSinGrupos(string filterText, string filtroSql, object[] filtroSqlParams)
        {
            return _repoReq.GetDatosVistaGeneroSinGrupos(filterText, filtroSql, filtroSqlParams);
        }
        #endregion

        #region Archivar y restaurar requerimientos
        public DatosAjax<List<RequerimientoDto>> GetDatosBusquedaArchivar(ParametrosGrillaDto<int> param,
            DateTime fechaDesde, DateTime fechaHasta, int unidadTecnicaId, int tipoBusqueda)
        {
            var resultado = new DatosAjax<List<RequerimientoDto>>(new List<RequerimientoDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
            try
            {
                var take = param.Take > 0 ? param.Take : param.PageSize;
                var skip = param.Skip == 0 && param.Page > 0 ? ((param.Page - 1) * take) : param.Skip;
                var sort = param.Sort == null
                    ? new SortParam() { Field = "FechaIngreso", Dir = "ASC" }
                    : new SortParam() { Field = param.Sort.Split('-')[0], Dir = param.Sort.Contains("-") ? param.Sort.Split('-')[1] : "DESC" };
                
                fechaHasta = fechaHasta.AddHours(23).AddMinutes(59).AddSeconds(59);

                resultado = _repoReq.GetDatosBusquedaArchivar(skip, take, sort, "", 
                    fechaDesde, fechaHasta, unidadTecnicaId, tipoBusqueda);
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            return resultado;

        }
        #endregion


        #region Log de requermiento
        public List<LogSistemaDto> GetLogRequerimiento(int idReq)
        {
            var resultado = new List<LogSistemaDto>();
            try
            {
                resultado = _repoReq.GetLogRequerimiento(idReq);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return resultado;
        }

        public DatosAjax<List<LogBitacoraDto>> GetLogBitacoraRequerimiento(int idReq)
        {
            var resultado = new DatosAjax<List<LogBitacoraDto>>(new List<LogBitacoraDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));

            try
            {
                var datos = _repoReq.GetLogBitacoraRequerimiento(idReq);

                resultado = new DatosAjax<List<LogBitacoraDto>>(datos, new ResultadoOperacion(1, "OK", null))
                {
                    Total = datos.Count
                };
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }

            return resultado;
        }
        #endregion

        #region Actualizar datos de MN en requermientos

        public ResultadoOperacion ActualizarMnFromRegmon(List<int> reqIds, List<int> casoIds, MonumentoNacionalDto datosMn)
        {
            var resultado = new ResultadoOperacion(-1, "Ha ocurrido un error al actualizar los datos, por favor, revise el log de errores de la aplicación.", null);
            try
            {
                // Se actualiza los datos del MN en los Requerimientos
                resultado = _repoReq.ActualizarMnReq(reqIds, casoIds, datosMn);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return resultado;
        }
        #endregion

        #region Caso
        #endregion

        #region Cierre múltiple de requerimientos

        public ResultadoOperacion CierreMultiple(List<int> reqIds, int motivoCierre, bool cerrar, string comentarioCierre, UsuarioActualDto usuario)
        {
            var resultado = new ResultadoOperacion(-1, "Ha ocurrido un error inesperado al actualizar los datos, por favor, revise el log de errores de la aplicación.", null);
            try
            {
                var logSistema = new LogSistemaDto()
                {
                    Flujo = "CIERRE MULTIPLE",
                    Fecha = DateTime.Now,
                    Origen = "CIERRE MULTIPLE",
                    Usuario = usuario.UsuarioNombre,
                    UsuarioId = usuario.UsuarioId,
                    DireccionIp = usuario.DireccionIp,
                    NombrePc = usuario.NombrePc,
                    UserAgent = usuario.UserAgent
                };

                var resultadosCierre = new List<ResultadoOperacion>();
                var resultadosEnvioNotif = new List<ResultadoOperacion>();
                foreach (var id in reqIds)
                {
                    var ingreso = new RequerimientoDto
                    {
                        Id = id, 
                        ComentarioCierre = comentarioCierre, 
                        MotivoCierreId = motivoCierre, 
                        EstadoId = -1
                    };
                    if (cerrar)
                    { // Se cierran los requerimmientos
                        ingreso.EstadoId = (int)EstadoIngreso.Cerrado;
                        ingreso.Cierre = DateTime.Now;
                        ingreso.CerradoPorId = usuario?.UsuarioId ?? 0;
                    }

                    resultado = _repoReq.CierreMultiple(ingreso, usuario);
                    resultadosCierre.Add(resultado);

                    if (resultado.Codigo < 0)
                    {
                        logSistema.Accion = "ERROR";
                        Logger.LogInfo(resultado.Mensaje);
                    }
                    else
                    {
                        var datosIngreso = resultado.Extra as RequerimientoDto;

                        // Notificación email
                        if (cerrar)
                        {
                            datosIngreso.Flujo = FlujoIngreso.RequerimientoCierre;
                            var resultadoEnvioEmail = EnviarNotificacionEmail(datosIngreso);
                            resultadosEnvioNotif.Add(resultadoEnvioEmail);
                        }
                        
                        // Grabar log de la operación realizada
                        logSistema.Accion = cerrar ? "CIERRE" : "EDICION";
                        logSistema.RequerimientoId = datosIngreso.Id;
                        logSistema.EstadoId = datosIngreso.EstadoId;
                        logSistema.EtapaId = datosIngreso.EtapaId;
                        logSistema.UnidadTecnicaId = datosIngreso.UtAsignadaId;
                        _repoMant.SaveLogSistema(logSistema);
                    }
                }

                var msg = "";
                if (cerrar)
                {
                    msg = resultadosCierre.All(r => r.Codigo > 0) ? "Se han cerrado satisfactoriamente los requerimientos."
                        : "Ha ocurrido error en el cierre de al menos un requerimento, por favor, repita la operación. ";
                } else
                {
                    msg = resultadosCierre.All(r => r.Codigo > 0) ? "Se han grabado con éxito los cambios. Los requerimientoso no han sido cerrados aún."
                        : "Ha ocurrido error al grabar los datos de al menos un requerimento, por favor, repita la operación. ";
                }

                resultado.Mensaje = msg + (resultadosEnvioNotif.Any(r => r.Codigo <= 0) ? "(Ocurrió error al enviar la notificación de cierre de al menos un requerimiento.)" : "");
                
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return resultado;
        }
        #endregion
    }
}
