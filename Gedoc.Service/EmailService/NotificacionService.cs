using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Logging;
using Gedoc.Repositorio.Interfaces;

namespace Gedoc.Service.EmailService
{
    public class NotificacionService : INotificacionService
    {
        public Dictionary<string, string> VariablesMensaje { get; set; }
        public string UsuarioActual { get; set; }
        private readonly string destinatarioDesarrollo = WebConfigValues.EsEmailDesarrollo ? WebConfigValues.EmailDesarrollo : "";

        private readonly IRequerimientoRepositorio _repoReq;
        private readonly IMantenedorRepositorio _repoMant;
        private readonly IDespachoRepositorio _repoDesp;
        private readonly IDespachoIniciativaRepositorio _repoDespInic;

        public NotificacionService(IRequerimientoRepositorio repoReq, IMantenedorRepositorio repoMant,
            IDespachoRepositorio repoDesp,
            IDespachoIniciativaRepositorio repoDespInic)
        {
            this._repoReq = repoReq;
            this._repoMant = repoMant;
            this._repoDesp = repoDesp;
            this._repoDespInic = repoDespInic;
        }

        #region Notificaciones Requerimientos
        public ResultadoOperacion NotificacionCierre(RequerimientoDto requerimiento)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Cierre_Req");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Cierre_Req");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Cierre_Req, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }
                var variablesEmail = GetVariablesRequerimiento(requerimiento, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = new Dictionary<string, string>();
                emailsDest = GetEmailsUnidadTecnica(requerimiento.UtAsignadaId.GetValueOrDefault(0));
                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }

                datosEmail.Destinatarios = emailsDest;


                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionAsignacionUt(RequerimientoDto requerimiento, bool esReasignacion)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                if (esReasignacion)
                {
                    return NotificacionReasignacion(requerimiento);
                }
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Asigna_UT_Req");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Asigna_UT_Req");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Asigna_UT_Req, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }
                var variablesEmail = GetVariablesRequerimiento(requerimiento, datosEmail.Asunto + datosEmail.TextoEmail);
                variablesEmail.Add("FechaReasignacion", "");
                variablesEmail.Add("TextoAsignacion", esReasignacion ? "Reasignación" : "Asignación");
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = new Dictionary<string, string>();
                emailsDest = GetEmailsUnidadTecnica(requerimiento.UtAsignadaId.GetValueOrDefault(0));
                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionProcesoMasivo(Dictionary<string, string> variablesEmail,
            int[] idDestinatarios, int[] idUnidadTec, string adjunto)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Proceso_Masivo");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Proceso_Masivo");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Proceso_Masivo, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = new Dictionary<string, string>();
                for (var i = 0; i < idDestinatarios.Length; i++)
                {
                    emailsDest = GetEmailUsuarioById(idDestinatarios[i]);
                }
                for (var i = 0; i < idUnidadTec.Length; i++)
                {
                    emailsDest = GetEmailsUnidadTecnica(idUnidadTec[i]);
                }
                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;
                // Adjuntos
                if (!string.IsNullOrEmpty(adjunto))
                {
                    datosEmail.Adjuntos = new List<string> {adjunto};
                }

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionAsignacionProfUt(RequerimientoDto requerimiento)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Asigna_ProfUT_Req");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Asigna_ProfUT_Req");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Asigna_ProfUT_Req, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }
                var variablesEmail = GetVariablesRequerimiento(requerimiento, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = new Dictionary<string, string>();
                var profId = requerimiento.EnAsignacionTemp
                    ? requerimiento.ProfesionalTempId.GetValueOrDefault(0)
                    : requerimiento.ProfesionalId.GetValueOrDefault(0);
                emailsDest = GetEmailUsuarioById(profId);
                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionAsignacionTemp(RequerimientoDto requerimiento)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Asigna_Temp_Req");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Asigna_Temp_Req");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Asigna_Temp_Req, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }
                var variablesEmail = GetVariablesRequerimiento(requerimiento, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = new Dictionary<string, string>();
                // Destinatarios de UT Asignada
                emailsDest = GetEmailsUnidadTecnica(requerimiento.UtAsignadaId.GetValueOrDefault());
                // Destinatarios de UT Temporal
                var emailsDestTmp = GetEmailsUnidadTecnica(requerimiento.UtTemporalId.GetValueOrDefault());
                emailsDest = emailsDest.Concat(emailsDestTmp.Where(x => !emailsDest.ContainsKey(x.Key)))
                    .ToDictionary(x => x.Key, x => x.Value);

                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionAsignacionUtApoyo(RequerimientoDto requerimiento)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Asigna_UTApoyo_Req");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Asigna_UTApoyo_Req");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Asigna_UTApoyo_Req, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }
                var variablesEmail = GetVariablesRequerimiento(requerimiento, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = new Dictionary<string, string>();
                emailsDest = GetEmailsUnidadTecnica(requerimiento.UtApoyoId.GetValueOrDefault());
                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionAsignacionUtCopia(RequerimientoDto requerimiento, int utNotificar)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Asigna_UTCopia_Req");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Asigna_UTCopia_Req");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Asigna_UTCopia_Req, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }
                var variablesEmail = GetVariablesRequerimiento(requerimiento, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = GetEmailsUnidadTecnica(utNotificar);

                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionAsignacionUtConoc(RequerimientoDto requerimiento)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Asigna_UTConoc_Req");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Asigna_UTConoc_Req");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Asigna_UTConoc_Req, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }
                var variablesEmail = GetVariablesRequerimiento(requerimiento, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = new Dictionary<string, string>();
                emailsDest = GetEmailsUnidadTecnica(requerimiento.UtConocimientoId.GetValueOrDefault());
                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionLiberacionAsignacionTemporal(RequerimientoDto requerimiento)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Liberacion_Req");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Liberacion_Req");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Liberacion_Req, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }
                var variablesEmail = GetVariablesRequerimiento(requerimiento, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = GetEmailsUnidadTecnica(requerimiento.UtAsignadaId.GetValueOrDefault());
                // Emails de UT Temporal q se libera
                var emailsDestTmp = GetEmailsUnidadTecnica(requerimiento.UtTemporalId.GetValueOrDefault());
                emailsDest = emailsDest.Concat(emailsDestTmp.Where(x => !string.IsNullOrWhiteSpace(x.Key) && !emailsDest.ContainsKey(x.Key)))
                    .ToDictionary(x => x.Key, x => x.Value);

                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionReasignacion(RequerimientoDto requerimiento)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {

                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Reasigna_Req");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Reasigna_Req");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Reasigna_Req, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }
                var variablesEmail = GetVariablesRequerimiento(requerimiento, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = new Dictionary<string, string>();
                emailsDest = GetEmailsUnidadTecnica(requerimiento.UtAsignadaId.GetValueOrDefault());
                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();


                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }
        #endregion

        #region Notificaciones Despachos
        public ResultadoOperacion NotificacionDespachoNuevo(DespachoDto despacho)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Nuevo_Despacho");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Nuevo_Despacho");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Nuevo_Despacho, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }

                #region Datos de los requerimientos asociados
                var reqIds = (despacho.Requerimiento ?? new List<GenericoDto>()).Select(r => r.IdInt).ToList();
                var datosReq = GetDatosRequerimientosDespacho(despacho.Id, reqIds);
                #endregion

                var variablesEmail = GetVariablesDespacho(despacho, despacho.RequerimientoMain, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = new Dictionary<string, string>();
                foreach (var req in datosReq)
                {
                    // Correos de responsables de la UT
                    var emailsTmp = GetEmailsUnidadTecnica(req.UtAsignadaId.GetValueOrDefault());
                    emailsDest = emailsDest.Concat(emailsTmp.Where(x => !string.IsNullOrWhiteSpace(x.Key) && !emailsDest.ContainsKey(x.Key)))
                        .ToDictionary(x => x.Key, x => x.Value);
                    // Correo del profesional UT
                    var prof = _repoMant.GetUsuarioById(req.ProfesionalId.GetValueOrDefault());
                    if (!string.IsNullOrEmpty(prof?.Email) && !emailsDest.ContainsKey(prof.Email))
                    {
                        emailsDest.Add(prof.Email, prof.NombresApellidos);
                    }
                }

                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionDespachoInicNuevo(DespachoIniciativaDto despacho)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Nuevo_Despacho_Inic");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Nuevo_Despacho_Inic");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Nuevo_Despacho_Inic, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }

                var variablesEmail = GetVariablesDespacho(despacho, null, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Correos responsables UT:
                var emailsDest = GetEmailsUnidadTecnica(despacho.UtAsignadaId.GetValueOrDefault());
                // Correo del profesional UT
                var prof = _repoMant.GetUsuarioById(despacho.ProfesionalId.GetValueOrDefault());
                if (!string.IsNullOrEmpty(prof?.Email) && !emailsDest.ContainsKey(prof.Email))
                {
                    emailsDest.Add(prof.Email, prof.NombresApellidos);
                }

                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionDestCopiaDespachoNuevo(DespachoDto despacho)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Nuevo_Despacho_DestCopia");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Nuevo_Despacho_DestCopia");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Nuevo_Despacho_DestCopia, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }

                #region Datos de los requerimientos asociados
                var datosReq = GetDatosRequerimientosDespacho(despacho.Id);
                #endregion

                var variablesEmail = GetVariablesDespacho(despacho, despacho.RequerimientoMain, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Email Destinatario y Destinatarios en Copia
                var emailsDest = GetEmailDestinatariosDesp(despacho, true);

                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionDestCopiaDespachoinicNuevo(DespachoIniciativaDto despacho)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Nuevo_Despacho_Inic_DestCopia");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Nuevo_Despacho_Inic_DestCopia");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Nuevo_Despacho_Inic_DestCopia, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }

                var variablesEmail = GetVariablesDespacho(despacho, null, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Email Destinatario y Destinatarios en Copia
                var emailsDest = GetEmailDestinatariosDespInic(despacho, true);

                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionDespachoCierre(DespachoDto despacho)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Cierre_Despacho");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Cierre_Despacho");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Cierre_Despacho, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }

                #region Datos de los requerimientos asociados
                var datosReq = GetDatosRequerimientosDespacho(despacho.Id);
                #endregion

                var variablesEmail = GetVariablesDespacho(despacho, despacho.RequerimientoMain, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = new Dictionary<string, string>();
                foreach (var req in datosReq)
                {
                    var emailsTmp = GetEmailsUnidadTecnica(req.UtAsignadaId.GetValueOrDefault());
                    emailsDest = emailsDest.Concat(emailsTmp.Where(x => !string.IsNullOrWhiteSpace(x.Key) && !emailsDest.ContainsKey(x.Key)))
                        .ToDictionary(x => x.Key, x => x.Value);
                    // Correo del profesional UT
                    var emailProf = GetEmailUsuarioById(req.ProfesionalId.GetValueOrDefault());
                    emailsDest = emailsDest.Concat(emailProf.Where(x => !string.IsNullOrWhiteSpace(x.Key) && !emailsDest.ContainsKey(x.Key)))
                        .ToDictionary(x => x.Key, x => x.Value);
                }
                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }

                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionDestCopiaDespachoCierre(DespachoDto despacho)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Cierre_Despacho_DestCopia");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Cierre_Despacho_DestCopia");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Cierre_Despacho_DestCopia, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }

                #region Datos de los requerimientos asociados
                var datosReq = GetDatosRequerimientosDespacho(despacho.Id);
                #endregion

                var variablesEmail = GetVariablesDespacho(despacho, despacho.RequerimientoMain, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Email Destinatario y Destinatarios en Copia
                var emailsDest = GetEmailDestinatariosDesp(despacho, true);

                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }

                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionDespachoInicCierre(DespachoIniciativaDto despacho)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Cierre_DespachoInic");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Cierre_DespachoInic");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Cierre_DespachoInic, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }

                var variablesEmail = GetVariablesDespacho(despacho, null, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Correos responsables UT:
                var emailsDest = GetEmailsUnidadTecnica(despacho.UtAsignadaId.GetValueOrDefault());
                // Correo del profesional UT
                var prof = _repoMant.GetUsuarioById(despacho.ProfesionalId.GetValueOrDefault());
                if (!string.IsNullOrEmpty(prof?.Email) && !emailsDest.ContainsKey(prof.Email))
                {
                    emailsDest.Add(prof.Email, prof.NombresApellidos);
                }

                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }

                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionDestCopiaDespachoInicCierre(DespachoIniciativaDto despacho)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Cierre_DespachoInic_DestCopia");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Cierre_DespachoInic_DestCopia");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Cierre_DespachoInic_DestCopia, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }

                var variablesEmail = GetVariablesDespacho(despacho, null, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Email Destinatario y Destinatarios en Copia
                var emailsDest = GetEmailDestinatariosDespInic(despacho, true);

                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }

                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionCambioPriorizacion(RequerimientoDto requerimiento)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Cambio_Priorizacion");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Cambio_Priorizacion");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Cambio_Priorizacion, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }
                var variablesEmail = GetVariablesRequerimiento(requerimiento, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = new Dictionary<string, string>();
                // Ut asignada
                emailsDest = GetEmailsUnidadTecnica(requerimiento.UtAsignadaId.GetValueOrDefault());
                // Ut Apoyo
                var emailsDestTmp = GetEmailsUnidadTecnica(requerimiento.UtApoyoId.GetValueOrDefault());
                foreach (var item in emailsDestTmp)
                {
                    if (!emailsDest.ContainsKey(item.Key))
                    {
                        emailsDest.Add(item.Key, item.Value);
                    }
                }
                // Ut en conocimiento
                emailsDestTmp = GetEmailsUnidadTecnica(requerimiento.UtConocimientoId.GetValueOrDefault());
                foreach (var item in emailsDestTmp)
                {
                    if (!emailsDest.ContainsKey(item.Key))
                    {
                        emailsDest.Add(item.Key, item.Value);
                    }
                }
                // Ut(s) en copia
                if (requerimiento.UnidadTecnicaCopia != null)
                {
                    foreach (var utCopia in requerimiento.UnidadTecnicaCopia)
                    {
                        var emailsUt = GetEmailsUnidadTecnica(utCopia.IdInt);
                        emailsDest = emailsDest.Concat(emailsUt.Where(x => !string.IsNullOrWhiteSpace(x.Key) && !emailsDest.ContainsKey(x.Key)))
                            .ToDictionary(x => x.Key, x => x.Value);
                    }
                }

                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }

        public ResultadoOperacion NotificacionForzarPriorizacion(RequerimientoDto requerimiento)
        {
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email
                var datosEmail = GetDatosNotificacion("Notif_Forzar_Prioridad_Req");
                if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                {
                    Logger.LogInfo("No se encontró en el mantenedor de notificaciones la notificación Notif_Forzar_Prioridad_Req");
                    resultadoNotificacion.Codigo = -2;
                    resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                    return resultadoNotificacion;
                }
                else if (!datosEmail.Activo)
                {
                    Logger.LogInfo("No se envió la notificación Notif_Forzar_Prioridad_Req, se encuentra desactivada.");
                    resultadoNotificacion.Codigo = 1;
                    resultadoNotificacion.Mensaje = "Desactivada";
                    return resultadoNotificacion;
                }
                var variablesEmail = GetVariablesRequerimiento(requerimiento, datosEmail.Asunto + datosEmail.TextoEmail);
                // Sustitución de variables en el Asunto del Mensaje:
                datosEmail.Asunto = ReemplazaVariables(datosEmail.Asunto, variablesEmail);
                // Sustitución de variables en el Cuerpo del Mensaje:
                datosEmail.TextoEmail = ReemplazaVariables(datosEmail.TextoEmail, variablesEmail);
                // Destinatarios:
                var emailsDest = new Dictionary<string, string>();
                emailsDest = GetEmailUsuarioById(requerimiento.ProfesionalId.GetValueOrDefault());
                if (!string.IsNullOrEmpty(destinatarioDesarrollo))
                {
                    datosEmail.TextoEmail += GetExtraDataEmailDev(emailsDest);
                    emailsDest = GetEmailDesarrollo();
                }
                datosEmail.Destinatarios = emailsDest;

                // Envío del email
                var emailSender = new EmailSender();
                resultadoNotificacion = emailSender.EnviarMensaje(datosEmail);
            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                Logger.LogError(exc);
            }
            return resultadoNotificacion;
        }
        #endregion

        #region Métodos privados
        private EmailDetail GetDatosNotificacion(string codigoNotificacion)
        {
            var datosEMail = new EmailDetail();
            var notif = _repoMant.GetNotificacionByCodigo(codigoNotificacion);
            if (notif != null)
            {
                datosEMail.Asunto = notif.Asunto;
                datosEMail.TextoEmail = notif.Mensaje;
                datosEMail.Activo = notif.Activo;
            }

            return datosEMail;
        }

        private string ReemplazaVariables(string texto, Dictionary<string, string> variables)
        {
            #region Variables genericas
            if (variables == null)
                variables = new Dictionary<string, string>();
            if (!variables.ContainsKey("FechaAhora"))
                variables.Add("FechaAhora", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            if (!variables.ContainsKey("UsuarioActual"))
                variables.Add("UsuarioActual", UsuarioActual);
            #endregion
            //if (variables != null)
            {
                variables.Keys.ToList().ForEach(key =>
                {
                    var variable = "%" + key + "%";
                    var valorVariable = variables[key];
                    texto = texto.Replace(variable, valorVariable);
                });
            }
            return texto;
        }

        /// <summary>
        /// Extrae las variables (están encerradas entre % %) del texto especificado y para cada variable encontrada obtiene de 
        /// requerimiento el valor de la propiedad que tenga igual nombre a la variable.
        /// </summary>
        /// <param name="requerimiento">RequerimientoDto para tomar el valor de las variables encontradas en texto</param>
        /// <param name="texto">Texto a extraer las variables</param>
        /// <returns>Dictionary con el nombre de la variable encontrada en texto como key y como valor el valor
        /// de la propiedad en requerimiento  que tiene el mismo nombre q la variable</returns>
        private Dictionary<string, string> GetVariablesRequerimiento(RequerimientoDto requerimiento, string texto)
        {

            var variablesEnTexto = texto.Split('%', '%').Where((item, index) => index % 2 != 0).ToList();

            var variables = GetVariablesFromItem(requerimiento, variablesEnTexto);
            return variables;
        }

        private Dictionary<string, string> GetVariablesDespacho<T>(T despacho, RequerimientoDto datosReq, string texto)
        {
            var variablesEnTexto = texto.Split('%', '%').Where((item, index) => index % 2 != 0).ToList();

            var variables = GetVariablesFromItem(despacho, variablesEnTexto);
            if (datosReq != null)
            {
                var varReq = GetVariablesFromItem(datosReq, variablesEnTexto);
                variables = variables.Concat(varReq.Where(x => !variables.ContainsKey(x.Key)))
                    .ToDictionary(x => x.Key, x => x.Value);
            }

            return variables;
        }

        private Dictionary<string, string> GetVariablesFromItem<T>(T itemOrig, List<string> nombresVariable)
        {
            var variablesData = new Dictionary<string, string>();
            var cls = typeof(T);
            PropertyInfo[] propertyInfos = cls.GetProperties();
            foreach (PropertyInfo pInfo in propertyInfos)
            {
                var propName = pInfo.Name;
                var propValue = pInfo.GetValue(itemOrig);
                if (nombresVariable.Contains(propName))
                {
                    if (pInfo.PropertyType == typeof(List<GenericoDto>))
                    {
                        var valor = string.Join("; ", ((List<GenericoDto>)propValue).Select(v => v.Titulo));
                        variablesData.Add(propName, valor);
                    }
                    else if (pInfo.PropertyType == typeof(DateTime) || pInfo.PropertyType == typeof(DateTime?))
                    {
                        // Si es una fecha. De momento se pone para q siempre sea sin hora:
                        var fecha = (DateTime)(propValue ?? new DateTime());
                        var valor = true || DateTime.Compare(fecha, fecha.Date) == 0 ? fecha.ToString("dd/MM/yyyy") : fecha.ToString("dd/MM/yyyy HH:mm");
                        variablesData.Add(propName, valor);
                    }
                    else
                    {
                        variablesData.Add(propName, (propValue ?? "").ToString());
                    }
                }
                //else if (nombresVariable.Any(n => n.StartsWith(propName + "."))) // Para el caso q se especiique una variable q sea para tomar el valor de una propiedad q sea un objeto, por ejemplo en notificaci'on de Despacho la variable %RequerimientoMain.UtAsignadaTitulo%
                //{
                //    var varsName = nombresVariable.Where(n => n.StartsWith(propName + ".")).Select(vn => vn.Split('.')[1]).ToList();

                //    var subVar = GetVariablesFromItem(propValue, varsName); --> No funciona, propValue es de tipo Object por lo q al ejecutarse aquí el GetVariablesFromItem no hay propiedades de Object para recorrer y siempre devolverá vacìo
                //    variablesData = variablesData.Concat(subVar.Where(x => !variablesData.ContainsKey(x.Key)))
                //        .ToDictionary(x => x.Key, x => x.Value);
                //}
            }

            if (VariablesMensaje != null)
            {
                foreach (var key in VariablesMensaje.Keys)
                {
                    if (!variablesData.ContainsKey(key))
                        variablesData.Add(key, VariablesMensaje[key]);
                }
            }

            return variablesData;
        }

        private Dictionary<string, string> GetEmailUsuarioById(int idUsuario)
        {
            var emails = new Dictionary<string, string>();
            var usuario = _repoMant.GetUsuarioById(idUsuario);

            if (!string.IsNullOrWhiteSpace(usuario?.Email))
            {
                emails.Add(usuario.Email, usuario.NombresApellidos);
            }

            return emails;
        }

        private Dictionary<string, string> GetEmailsUnidadTecnica(int unidadId)
        {
            var emails = new Dictionary<string, string>();

            var ut = _repoMant.GetUnidadTecnicaById(unidadId);
            if (ut != null)
            {
                // Email responsable UT
                var email = ut.EmailResponsable;
                if (!string.IsNullOrEmpty(email) && !emails.ContainsKey(email))
                    emails.Add(email, "Responsable de UT");
                // Email secretaria UT
                email = ut.EmailSecretaria;
                if (!string.IsNullOrEmpty(email) && !emails.ContainsKey(email))
                    emails.Add(email, "Secretaria de UT");
                // Email de otros destinatarios
                var otrosEmails = ut.OtrosDestinatariosEmail ?? "";
                otrosEmails = string.IsNullOrEmpty(otrosEmails.Trim()) ? "" : otrosEmails.Trim().Replace(',', ';');
                if (!string.IsNullOrEmpty(otrosEmails))
                {
                    var otrosEmailsArr = otrosEmails.Split(';');
                    for (var i = 0; i < otrosEmailsArr.Length; i++)
                    {
                        if (!emails.ContainsKey(otrosEmailsArr[i]))
                        {
                            emails.Add(otrosEmailsArr[i], "Integrante de UT");
                        }
                    }
                }
            }

            return emails;
        }

        private List<RequerimientoDto> GetDatosRequerimientosDespacho(int idDespacho, List<int> idsReq = null)
        {
            try
            {
                var datosReq = _repoDesp.GetRequerimientosDespacho(idDespacho);
                //var datosReq = (idsReq == null || idsReq.Count == 0)
                //    ? _repoReq.GetRequerimientosDespacho(idDespacho)
                //    : _repoReq.GetRequerimientoByIds(idsReq, false);

                return datosReq;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                return new List<RequerimientoDto>();
            }
        }

        private Dictionary<string, string> GetEmailDesarrollo()
        {
            var result = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(destinatarioDesarrollo))
            {
                var emailsDesa = destinatarioDesarrollo.Replace(',', ';'); // Para permitir tanto , o ; como separador de emails
                var listaEmail = emailsDesa.Split(';').ToList<string>();
                listaEmail.ForEach(email => result.Add(email, "Destinario Desarrollo"));
            }
            return result;
        }

        private string GetExtraDataEmailDev(Dictionary<string, string> emailsDest)
        {
            var extraData = "<br/><br/><br/><p>********************************************************************</p>" +
                "<h4>ATENCIÓN:</h4> Este texto se muestra sólo en desarrollo cuando se tiene habilitada " +
                "la opción DestinatarioEmailDesarrollo del web.config de la aplicación, no se muestra en las notificaciones en producción.<p>" +
                "Destinatarios del email al desactivar la ópción DestinatarioEmailDesarrollo:<br/>" +
                string.Join(" <br/>", emailsDest.Select(e => e.Value + " --- " + e.Key + "<br/>").ToArray()) +
                "</p>********************************************************************";
            return extraData;
        }

        private Dictionary<string, string> GetEmailDestinatariosDesp(DespachoDto despacho, bool incluyeDestCopia)
        {
            var emailsDest = new Dictionary<string, string>();
            var idDest = new List<int>{
                despacho.DestinatarioId.GetValueOrDefault()
            };
            if (incluyeDestCopia)
            {
                foreach (var destCop in (despacho.DestinatarioCopia ?? new List<GenericoDto>()))
                {
                    idDest.Add(destCop.IdInt);
                }
            }
            _repoMant.GetRemitentesByIds(idDest).ForEach(r =>
            {
                if (!string.IsNullOrEmpty(r.Email) && !emailsDest.ContainsKey(r.Email))
                {
                    emailsDest.Add(r.Email, r.Nombre);
                }
            });

            return emailsDest;
        }

        private Dictionary<string, string> GetEmailDestinatariosDespInic(DespachoIniciativaDto despacho, bool incluyeDestCopia)
        {
            var emailsDest = new Dictionary<string, string>();
            var idDest = new List<int>{
                despacho.DestinatarioId.GetValueOrDefault()
            };
            if (incluyeDestCopia)
            {
                foreach (var destCop in (despacho.DestinatarioCopia ?? new List<GenericoDto>()))
                {
                    idDest.Add(destCop.IdInt);
                }
            }
            _repoMant.GetRemitentesByIds(idDest).ForEach(r =>
            {
                if (!string.IsNullOrEmpty(r.Email) && !emailsDest.ContainsKey(r.Email))
                {
                    emailsDest.Add(r.Email, r.Nombre);
                }
            });

            return emailsDest;
        }
        #endregion

        #region Notificación Procesos Masivos

        public ResultadoOperacion EnviarNotificacionProcesosMasivos(int idProceso, int[] idIngresos, 
            int idUtAsign, int idUtReAsign, int idProfAsig, int idProfReAsig)
        {
            var resultado = new ResultadoOperacion(1, "Notificación enviada con éxito", null);
            var variables = new Dictionary<string, string>();

            //generar fichero con datos de los ingresos
            var uts = new List<int>();
            var profesionales = new List<int>();
            var responsables = new List<int>();
            var filePath = CreaFicheroIngresos(idIngresos, uts, profesionales, responsables);
            if (filePath == "")
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "Ha ocurrido un error al enviar la notificación. No fue posible crear el fichero con información sobre los ingresos procesados.";
                return resultado;
            }

            switch (idProceso)
            {
                case 1: // Asignar UT
                    variables["accion"] = "Asignación Unidad Técnica";
                    resultado = NotificacionProcesoMasivo(
                        variables,
                        new int[0],
                        new int[] { idUtAsign },
                        filePath);
                    break;
                case 2: // Reasignar UT
                    variables["accion"] = "Reasignación Unidad Técnica";
                    resultado = NotificacionProcesoMasivo(
                        variables,
                        new int[0],
                        new int[] { idUtReAsign },
                        filePath);
                    break;
                case 3: // Asignar Profesional
                    variables["accion"] = "Asignación Profesional UT";
                    resultado = NotificacionProcesoMasivo(
                        variables,
                        new int[] { idProfAsig },
                        new int[0],
                        filePath);
                    break;
                case 4: // Reasignar Profesional
                    variables["accion"] = "Reasignación Profesional UT";
                    resultado = NotificacionProcesoMasivo(
                        variables,
                        new int[] { idProfReAsig },
                        new int[0],
                        filePath);
                    break;
                case 6:  // Modificar Etiqueta
                    variables["accion"] = "Modificación de Etiqueta";
                    resultado = NotificacionProcesoMasivo(
                        variables,
                        new int[0], // TODO: a quien enviarle la notificación?
                        uts.ToArray(),
                        filePath
                        );
                    break;
                case 7:  // Abrir Ingresos
                    variables["accion"] = "Abrir Ingresos";
                    resultado = NotificacionProcesoMasivo(
                        variables,
                        new int[0], // TODO: a quien enviarle la notificación?
                        uts.ToArray(),
                        filePath
                        );
                    break;
            }

            resultado.Mensaje = $"Se informa que se ha realizado el proceso masivo de {variables["accion"]}, <br/>" +
                                "recibirá un correo informando esto con fichero adjunto con los datos de los ingresos procesados.<br/>" +
                                "Para mayor detalle del proceso, puede revisar el apartado de Reportes del Gestor Documental,<br/>" +
                                "en la opción “Procesos Masivos”.";

            resultado.Codigo = resultado.Codigo == 0 ? 1 : resultado.Codigo;

            return resultado;

        }

        private string CreaFicheroIngresos(int[] idIngresos, List<int> uts, List<int> profesionales, List<int> responsables)
        {
            try
            {

                var fileData = new List<string>();
                var idIngresosList = idIngresos.ToList();
                var datos = _repoReq.GetRequerimientoByIds(idIngresosList, false);

                foreach (var requerimiento in datos)
                {
                    var fileLine = " Doc. Ingreso: " + requerimiento.DocumentoIngreso +
                                   " Fecha Ingreso: " + requerimiento.FechaIngreso.ToString("dd/MM/yyyy HH:mm") +
                                   " Fecha Documento: " + requerimiento.FechaDocumento.ToString("dd/MM/yyyy") +
                                   " Nombre de Proyecto o Programa: " + (requerimiento.NombreProyectoPrograma ?? "");
                    var ut = requerimiento.UtAsignadaId.GetValueOrDefault(0);
                    var prof = requerimiento.ProfesionalId.GetValueOrDefault(0);
                    var resp = requerimiento.ResponsableUtId.GetValueOrDefault(0);
                    if (ut > 0 && !uts.Contains(ut))
                    {
                        uts.Add(ut);
                    }
                    if (prof > 0 && !profesionales.Contains(prof))
                    {
                        profesionales.Add(prof);
                    }
                    if (resp > 0 && !responsables.Contains(resp))
                    {
                        responsables.Add(resp);
                    }
                    fileData.Add(fileLine);
                }
                var pathFileTmp = CrearFicheroTemp(fileData);
                return pathFileTmp;

            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                // TODO: indicar q hubo error enviando la notificación
                return "";

            }
        }

        private string CrearFicheroTemp(List<string> fileData)
        {
            try
            {
                var path = System.IO.Path.GetTempPath() + "DatosIngresos_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".txt";

                if (!System.IO.File.Exists(path))
                {
                    // Create a file to write to.
                    System.IO.File.WriteAllLines(path, fileData.ToArray(), System.Text.Encoding.UTF8);
                }

                return path;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                // TODO: indicar q hubo error enviando la notificación
                return "";

            }
        }

        #endregion


    }
}