using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Script.Serialization;
using Gedoc.Alertas.Models;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers;
using Gedoc.Helpers.Enum;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.Service.Maps;
using Gedoc.Repositorio.Model;
using Gedoc.Service.EmailService;
using Gedoc.Helpers.Logging;

namespace Gedoc.Alertas.Class
{
    public class ServiceLogic
    {
        public static Dictionary<string, string> VariablesMensaje { get; set; }

        private static readonly string destinatarioDesarrollo = WebConfigValues.EsEmailDesarrollo ? WebConfigValues.EmailDesarrollo : "";

        public static void ProcesaAlertas()
        {
            var srvLogic = new DAL();
            var reqsPF = srvLogic.GetRequerimientosPrioridadForzada();

            Logger.LogInfo("Procesando " + reqsPF.Count + " registros.");

            foreach (var req in reqsPF)
            {
                var resultadoEnvioEmail = NotificacionForzarPrioridad(req);

                if (resultadoEnvioEmail.Codigo != 1 && resultadoEnvioEmail.Codigo != 0)
                    Logger.LogError(resultadoEnvioEmail.Mensaje + " Extra:" + resultadoEnvioEmail.Extra + ".");
            }

        }

        public static ResultadoOperacion NotificacionForzarPrioridad(RequerimientoDto requerimiento)
        {
            int enviosCorrectos = 0;
            var resultadoNotificacion = new ResultadoOperacion();
            try
            {
                // Datos del email en SP
                var datosEmail = GetDatosNotificacion("Notif_Forzar_Prioridad_Req");

                if (!datosEmail.Periodicidad.HasValue)
                {
                    resultadoNotificacion.Codigo = -3;
                    resultadoNotificacion.Mensaje = "No existe periodicidad definida para la notificación Notif_Forzar_Prioridad_Req.";
                    return resultadoNotificacion;
                }

                //Validar si se debe enviar 
                DAL srvLogic = new DAL();
                DateTime? fue = srvLogic.GetUltimaNotificacionRegistroByReqId("Notif_Forzar_Prioridad_Req", requerimiento.Id);
                Logger.LogInfo("Fecha último envío:" + (fue.HasValue ? fue.ToString() : ""));

                if (!fue.HasValue || (fue.HasValue && fue.Value.AddMinutes(datosEmail.Periodicidad.Value) <= DateTime.Now))
                {
                    if (string.IsNullOrEmpty(datosEmail.TextoEmail))
                    {
                        resultadoNotificacion.Codigo = -2;
                        resultadoNotificacion.Mensaje = "Formato de notificación desconocido.";
                        return resultadoNotificacion;
                    }
                    else if (!datosEmail.Activo)
                    {
                        resultadoNotificacion.Codigo = -1;
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
                else
                {
                    resultadoNotificacion.Codigo = 0;
                    resultadoNotificacion.Mensaje = "No corresponde enviar notificación por periodicidad.";
                    Logger.LogInfo("No corresponde enviar notificación por periodicidad (cada " + datosEmail.Periodicidad.Value + " minutos)");
                }

                if (resultadoNotificacion.Codigo == 1)
                {
                    enviosCorrectos++;
                    srvLogic.SetUltimaNotificacionRegistro("Notif_Forzar_Prioridad_Req", requerimiento.Id);
                    Logger.LogInfo("Email \"" + datosEmail.Asunto + "\", de:" + datosEmail.RemitenteEmail + ", para:" + datosEmail.Destinatarios.FirstOrDefault() + ", enviado exitosamente");
                }
                else if (resultadoNotificacion.Codigo != 0)
                    Logger.LogError("Email \"" + datosEmail.Asunto + "\", de:" + datosEmail.RemitenteEmail + ", para:" + datosEmail.Destinatarios.FirstOrDefault() + ", no se pudo enviar (" + resultadoNotificacion.Mensaje + ". " + resultadoNotificacion.Extra + ")");

            }
            catch (Exception exc)
            {
                resultadoNotificacion.Codigo = -1;
                resultadoNotificacion.Mensaje = "Error al enviar notificación.";
                resultadoNotificacion.Extra = exc.Message;
            }

            if (resultadoNotificacion.Extra == null) resultadoNotificacion.Extra = enviosCorrectos;
            return resultadoNotificacion;
        }

        #region Métodos privados
        private static EmailDetail GetDatosNotificacion(string codigoNotificacion)
        {
            var srvLogic = new DAL();
            var datosEMail = new EmailDetail();
            var notif = srvLogic.GetNotificacionByCodigo(codigoNotificacion);
            if (notif != null)
            {
                datosEMail.Asunto = notif.Asunto;
                datosEMail.TextoEmail = notif.Mensaje;
                datosEMail.Activo = notif.Activo;
                datosEMail.Periodicidad = notif.Periodicidad;
            }

            return datosEMail;
        }

        private static string ReemplazaVariables(string texto, Dictionary<string, string> variables)
        {
            #region Variables genericas
            if (variables == null)
                variables = new Dictionary<string, string>();
            if (!variables.ContainsKey("FechaAhora"))
                variables.Add("FechaAhora", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            if (!variables.ContainsKey("UsuarioActual"))
                variables.Add("UsuarioActual", "Servicio de Notificación");
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

        private static Dictionary<string, string> GetVariablesRequerimiento(RequerimientoDto requerimiento, string texto)
        {

            var variablesEnTexto = texto.Split('%', '%').Where((item, index) => index % 2 != 0).ToList();

            var variables = GetVariablesFromItem(requerimiento, variablesEnTexto);
            return variables;
        }

        private static Dictionary<string, string> GetVariablesDespacho<T>(T despacho, RequerimientoDto datosReq, string texto)
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

        private static Dictionary<string, string> GetVariablesFromItem<T>(T itemOrig, List<string> nombresVariable)
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
                    else if (pInfo.PropertyType == typeof(DateTime))
                    {
                        // Si es una fecha. De momento se pone para q siempre sea sin hora:
                        var fecha = (DateTime)propValue;
                        var valor = true || DateTime.Compare(fecha, fecha.Date) == 0 ? fecha.ToString("dd/MM/yyyy") : fecha.ToString("dd/MM/yyyy HH:mm");
                        variablesData.Add(propName, valor);
                    }
                    else
                    {
                        variablesData.Add(propName, (propValue ?? "").ToString());
                    }
                }
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

        private static Dictionary<string, string> GetEmailUsuarioById(int idUsuario)
        {
            var srvLogic = new DAL();

            var emails = new Dictionary<string, string>();
            var usuario = srvLogic.GetUsuarioById(idUsuario);

            if (!string.IsNullOrWhiteSpace(usuario?.Email))
            {
                emails.Add(usuario.Email, usuario.NombresApellidos);
            }

            return emails;
        }

        private static Dictionary<string, string> GetEmailDesarrollo()
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

        private static string GetExtraDataEmailDev(Dictionary<string, string> emailsDest)
        {
            var extraData = "<br/><br/><br/><p>********************************************************************</p>" +
                "<h4>ATENCIÓN:</h4> Este texto se muestra sólo en desarrollo cuando se tiene habilitada " +
                "la opción DestinatarioEmailDesarrollo del web.config de la aplicación, no se muestra en las notificaciones en producción.<p>" +
                "Destinatarios del email al desactivar la ópción DestinatarioEmailDesarrollo:<br/>" +
                string.Join(" <br/>", emailsDest.Select(e => e.Value + " --- " + e.Key + "<br/>").ToArray()) +
                "</p>********************************************************************";
            return extraData;
        }
        #endregion

    }
}
