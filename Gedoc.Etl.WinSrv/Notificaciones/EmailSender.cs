using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Etl.Winsrv.Helpers;

namespace Gedoc.Etl.Winsrv.Notificaciones
{
    public class EmailSender
    {

        private readonly string _host = ConfigurationManager.AppSettings["SmtpHost"];
        private readonly int _port = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
        private readonly string _usuario = ConfigurationManager.AppSettings["SmtpUsuario"];
        private readonly string _clave = ConfigurationManager.AppSettings["SmtpClave"];
        private readonly string _senderEmail = ConfigurationManager.AppSettings["SmtpSenderEmail"];
        private readonly string _senderName = ConfigurationManager.AppSettings["SmtpSenderName"];
        private readonly SmtpClient _smtpClient;

        public EmailSender()
        {
            _smtpClient = new SmtpClient(_host, _port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_usuario, _clave)
            };

            // Opcional, metodo 'callback' q se llama cuando termina la operación asincronica de envio de correo
            _smtpClient.SendCompleted += EnvioCompletadoCallback;
        }

        public ResultadoOperacion EnviarMensaje(EmailDetail datosEmail)
        {
            var resultado = new ResultadoOperacion(-1, "Error al realizar la operación.");
            try
            {
                var todoOk = true;
                todoOk = EnviarMasivo("", "", datosEmail.Destinatarios, datosEmail.TextoEmail, datosEmail.Asunto).Codigo == 0 && todoOk;

                // fin
                FinalizarConexion();
                resultado.Codigo = todoOk ? 0 : -1;
                resultado.Texto = todoOk ? "Operación realizada con éxito." : "Ocurrió un error con el envío de email.";
            }
            catch (Exception exc)
            {
                // TODO: guardar log de error
                resultado.DataExtra = exc.ToString();
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
            var destinatarios = new Dictionary<string, string>() { { destinatario, destinatarioNombre } };
            return EnviarMasivo(remitente, remitenteNombre, destinatarios, mensaje, asunto);

        }

        private ResultadoOperacion EnviarMasivo(string remitente, string remitenteNombre, Dictionary<string, string> destinatarios, string mensaje, string asunto)
        {
            var resultado = new ResultadoOperacion(0, "Acción exitosa.");
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
                    var destinatarioMail = new MailAddress(d.Key, !string.IsNullOrEmpty(d.Value) ? d.Value : d.Key);
                    mensajeMail.To.Add(destinatarioMail);
                });

                // Identificador q permite identificar en el metodo callback la operacion asincronica de envio actual
                var idOper = destinatarios.Count == 1 ? destinatarios.Keys.First() : "enviomasivo";
                //_smtpClient.SendAsync(mensajeMail, idOper);  // ¿Usar envío asincronico?
                _smtpClient.Send(mensajeMail);

                mensajeMail.Dispose();
            }
            catch (Exception exc)
            {
                // TODO: guardar log de error
                resultado.Codigo = -1;
                resultado.Texto = "Error inesperado al realizar la operación.";
                resultado.DataExtra = exc.ToString();
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
