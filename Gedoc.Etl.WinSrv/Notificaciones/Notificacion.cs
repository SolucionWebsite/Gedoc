using Gedoc.Etl.Winsrv.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Gedoc.Etl.Winsrv.Entidades;
using Gedoc.Etl.Winsrv.Helpers;
using Gedoc.Etl.Winsrv.Repository;
using Gedoc.Etl.Winsrv.Servicios;

namespace Gedoc.Etl.Winsrv.Notificaciones
{
    public class Notificacion
    {
        private ResultadoOperacion resultadoNotificacion = new ResultadoOperacion();

        private EmailDetail GetDatosNotificacion(string tipoNotificacion)
        {
            var _repo = new GenericRepo();
            var datosEmail =
                _repo.GetDatosFromOrigen<EmailDetail>(Queries.SelectNotificacionEmail, new {codigo = tipoNotificacion});
            return datosEmail.Count > 0 ? datosEmail[0] : new EmailDetail();
        }

        #region Métodos privados

        private string ReemplazaVariables(string texto, Dictionary<string, string> variables)
        {
            #region Variables genericas
            if (variables == null)
                variables = new Dictionary<string, string>();
            variables.Add("FechaAhora", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            variables.Add("UsuarioActual", "Servicio GDOC");
            variables.Add("DiasFechaResol​", ConfigurationManager.AppSettings["DiasAvisoFechaResol"]);
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

        private Dictionary<string, string> GetVariablesRequerimiento(Requerimiento requerimiento)
        {
            return new Dictionary<string, string>() {
                    {"DocumentoIngreso" , requerimiento.Documento_x0020_de_x0020_ingreso},
                    {"TipoDocumento" , requerimiento.Tipo_x0020_de_x0020_documento },
                    {"Materia" , requerimiento.Materia},
                    {"Remitente" , requerimiento.Remitente},
                    {"InstitucionRemitente" , requerimiento.Instituci_x00f3_n_x0020__x0028_N},
                    {"FechaCierre" , requerimiento.Fecha_x0020_de_x0020_documento.HasValue ? requerimiento.Fecha_x0020_de_x0020_cierre.Value.ToString("dd/MM/yyyy") : ""},  // DateTime.Today.ToString("dd/MM/yyyy")},
                    {"MotivoCierre" , requerimiento.Motivo_x0020_de_x0020_cierre},
                    {"ComentarioCierre" , requerimiento.Comentario_x0020_de_x0020_cierre},
                    {"Programa" , requerimiento.Nombre_x0020_de_x0020_proyecto}, 
                    {"ComentarioAsignacion" , requerimiento.Comentario_x0020_de_x0020_asigna},
                    {"FechaEmisionOficio" , requerimiento.Fecha_x0020_de_x0020_documento.HasValue ? requerimiento.Fecha_x0020_de_x0020_documento.Value.ToString("dd/MM/yyyy") : ""}, 
                    {"FechaResolEstimada" , requerimiento.Fecha_x0020_de_x0020_Resoluci_x0.HasValue ? requerimiento.Fecha_x0020_de_x0020_Resoluci_x0.Value.ToString("dd/MM/yyyy") : ""},
                    {"FechaDocumento" , requerimiento.Fecha_x0020_de_x0020_documento.HasValue ? requerimiento.Fecha_x0020_de_x0020_documento.Value.ToString("dd/MM/yyyy") : ""},
                    {"FechaIngreso" , requerimiento.Fecha_x0020_de_x0020_ingreso.HasValue ? requerimiento.Fecha_x0020_de_x0020_ingreso.Value.ToString("dd/MM/yyyy") : ""},
                           };
        }
        #endregion


    }
}
