using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Gedoc.Helpers;
using Gedoc.Helpers.Logging;

namespace Gedoc.Service.EmailService
{
    public class EmailSender
    {

        private readonly string _host = WebConfigValues.SmtpClientHost;
        private readonly int _port = WebConfigValues.SmtpClientPort;
        private readonly string _usuario = WebConfigValues.SmtpClientUser;
        private readonly string _clave = WebConfigValues.SmtpClientPassword;
        private readonly string _senderEmail = WebConfigValues.RemitenteEmail;
        private readonly string _senderName = WebConfigValues.RemitenteNombre;
        private readonly bool _ssl = WebConfigValues.SmtpClientEnableSsl;
        private readonly SmtpClient _smtpClient;

        public EmailSender()
        {
            _smtpClient = new SmtpClient(_host, _port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_usuario, _clave),
                EnableSsl = _ssl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            ServicePointManager.ServerCertificateValidationCallback =
            delegate(object s, X509Certificate certificate,
                     X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
            // Opcional, metodo 'callback' q se llama cuando termina la operación asincronica de envio de correo
            _smtpClient.SendCompleted += EnvioCompletadoCallback;
        }

        public ResultadoOperacion EnviarMensaje(EmailDetail datosEmail)
        {
            var resultado = new ResultadoOperacion(-1, "Error al realizar la operación.", null);
            // Se eliminan destinatarios q no tenga email para evitar error luego al enviar el email
            datosEmail.Destinatarios.Remove("");
            if (datosEmail.Destinatarios.Count() == 0)
            {
                resultado.Codigo = -2;
                resultado.Mensaje = "El email no se ha enviado, no se ha encontrado destinatarios para el email.";
                Logger.LogInfo("El email '" + datosEmail.Asunto + "' no se ha enviado, no se ha encontrado destinatarios para el email.");
                return resultado;
            }
            try
            {
                var todoOk = true;
                todoOk = EnviarMasivo("", "", datosEmail.Destinatarios, datosEmail.TextoEmail, datosEmail.Asunto, datosEmail.Adjuntos).Codigo == 1 && todoOk;

                // fin
                FinalizarConexion();
                resultado.Codigo = todoOk ? 1 : -1;
                resultado.Mensaje = todoOk ? "Operación realizada con éxito." : "Ocurrió un error con el envío de email.";
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                resultado.Extra = exc.ToString();
            }
            return resultado;
        }

        private string ReemplazaVariables(string texto, Dictionary<string, string> variables)
        {
            if (variables != null)
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

        private ResultadoOperacion Enviar(string remitente, string remitenteNombre, string destinatario, string destinatarioNombre, string mensaje, string asunto)
        {
            var destinatarios = new Dictionary<string, string>(){{destinatario, destinatarioNombre}};
            return EnviarMasivo(remitente, remitenteNombre, destinatarios, mensaje, asunto, null); 

        }

        private ResultadoOperacion EnviarMasivo(string remitente, string remitenteNombre, Dictionary<string, string> destinatarios, 
            string mensaje, string asunto, List<string> attachments)
        {
            var resultado = new ResultadoOperacion(1, "Acción exitosa.", null);
            try
            {
                // Remitente del mensaje
                var remitenteMail = new MailAddress(string.IsNullOrEmpty(remitente) ? _senderEmail : remitente, string.IsNullOrEmpty(remitenteNombre) ? _senderName : remitenteNombre, System.Text.Encoding.UTF8);
                var mensajeMail = new MailMessage()
                {
                    From = remitenteMail,
                    // Contenido del mensaje
                    Body = mensaje,
                    BodyEncoding = System.Text.Encoding.UTF8,
                    IsBodyHtml = true,
                    // Asunto
                    Subject = asunto,
                    SubjectEncoding = System.Text.Encoding.UTF8
                };
                destinatarios.ToList().ForEach(d =>
                {
                    try
                    {
                        var destinatarioMail = new MailAddress(d.Key, !string.IsNullOrEmpty(d.Value) ? d.Value : d.Key);
                        mensajeMail.To.Add(destinatarioMail);
                    }
                    catch (Exception exc)
                    {
                        Logger.LogError(exc);
                        Logger.LogInfo("Observación. No fue posible agregar como destinatario el email '" + d.Key + "' en la notificación de asunto [" + asunto  + "]");
                    }
                });
                // Se asignan los destinatarios configurados en web.config (en el campo Para)
                if (!string.IsNullOrWhiteSpace(WebConfigValues.DestinatariosEmailsPara))
                {
                    var emails = WebConfigValues.DestinatariosEmailsPara.Split(';');
                    emails.ToList().ForEach(d =>
                    {
                        try
                        {
                            var destinatarioMail = new MailAddress(d, d);
                            mensajeMail.To.Add(destinatarioMail);
                        }
                        catch (Exception exc)
                        {
                            Logger.LogError(exc);
                            Logger.LogInfo("Observación. No fue posible agregar como destinatario en copia el email '" + d + "' en la notificación de asunto [" + asunto + "]");
                        }
                    });
                }
                // Se asignan los destinatarios en copia configurados en web.config (en el campo Con Copia)
                if (!string.IsNullOrWhiteSpace(WebConfigValues.DestinatariosEmailsCopia))
                {
                    var emails = WebConfigValues.DestinatariosEmailsCopia.Split(';');
                    emails.ToList().ForEach(d =>
                    {
                        try
                        {
                            var destinatarioMail = new MailAddress(d, d);
                            mensajeMail.CC.Add(destinatarioMail);
                        }
                        catch (Exception exc)
                        {
                            Logger.LogError(exc);
                            Logger.LogInfo("Observación. No fue posible agregar como destinatario en copia el email '" + d + "' en la notificación de asunto [" + asunto + "]");
                        }
                    });
                }
                // Se asignan los destinatarios en copia oculta configurados en web.config  (en el campo Copia Oculta)
                if (!string.IsNullOrWhiteSpace(WebConfigValues.DestinatariosEmailsCopiaOculta))
                {
                    var emails = WebConfigValues.DestinatariosEmailsCopiaOculta.Split(';');
                    emails.ToList().ForEach(d =>
                    {
                        try
                        {
                            var destinatarioMail = new MailAddress(d, d);
                            mensajeMail.Bcc.Add(destinatarioMail);
                        }
                        catch (Exception exc)
                        {
                            Logger.LogError(exc);
                            Logger.LogInfo("Observación. No fue posible agregar como destinatario en copia oculta el email '" + d + "' en la notificación de asunto [" + asunto + "]");
                        }
                    });
                }

                if (attachments != null)
                {
                    foreach (var attach in attachments)
                    {
                        mensajeMail.Attachments.Add( new System.Net.Mail.Attachment(attach) );
                    }
                }

                // Identificador q permite identificar en el metodo callback la operacion asincronica de envio actual
                var idOper = destinatarios.Count == 1 ? destinatarios.Keys.First() : "enviomasivo";
                //_smtpClient.SendAsync(mensajeMail, idOper);  // ¿Usar envío asincronico?
                _smtpClient.Send(mensajeMail);

                mensajeMail.Dispose();
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                resultado.Codigo = -1;
                resultado.Mensaje = "Error inesperado al realizar la operación.";
                resultado.Extra = exc.ToString();
            }
            return resultado;
        }

        public void FinalizarConexion()
        {
            //********* _smtpClient.Dispose();
        }

        private static void EnvioCompletadoCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Identificador de la operacion
            var token = (string)e.UserState;

            if (e.Cancelled)
            {
                // Envio cancelado
            }
            if (e.Error != null)
            {
                // Error en envio.
                var msgError = e.Error.Message;
            }
            else
            {
                // Envio exitoso
            }
        }


    }


}
