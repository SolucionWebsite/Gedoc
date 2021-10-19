using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using Gedoc.Helpers;
using Gedoc.Helpers.Enum;
using Gedoc.Helpers.Logging;
using Gedoc.Repositorio.Model;

namespace Gedoc.Service.DataAccess
{
    public class BaseService
    {

        public BaseService()
        {
        }

        protected void LogError(ResultadoOperacion resultadoOper, Exception ex, string mensaje)
        {
            Logger.LogError(mensaje, ex);
            LogDetalle(ex);
            if (resultadoOper != null)
            {
                resultadoOper.Codigo = (int)CodigoResultado.Error;
                resultadoOper.Mensaje = mensaje;
            }
        }

        protected void LogError(Exception ex)
        {
            Logger.LogError(ex);
            LogDetalle(ex);
        }

        private void LogDetalle(Exception ex)
        {

            if (ex is System.Data.Entity.Validation.DbEntityValidationException)
            {
                foreach (var dbex in ((System.Data.Entity.Validation.DbEntityValidationException)ex).EntityValidationErrors)
                {
                    foreach (var valErr in dbex.ValidationErrors)
                    {
                        Logger.LogError(valErr.ErrorMessage + " --- " + dbex.Entry.Entity.ToString(), ex);
                    }
                }
            }
        }

        protected ResultadoOperacion ProcesaExceptionDb(DbUpdateException ex, string errorId, string operacion)
        {
            ResultadoOperacion resultado;
            if (ex.InnerException != null && ex.InnerException.InnerException is SqlException innerException && (innerException.Number == 2627 || innerException.Number == 2601))
            {
                resultado = new ResultadoOperacion(-1,
                    innerException.Message.Contains("PRIMARY KEY")
                        ? "Error, clave primaria duplicada."
                        : "Ya existe un registro con igual título.", null);
            }
            else if (ex.InnerException != null && ex.InnerException.InnerException is SqlException innerException2 && (innerException2.Number == 547))
            {
                resultado = new ResultadoOperacion(-1,
                    (innerException2.Message.Contains(" INSERT ") || innerException2.Message.Contains(" UPDATE "))
                        ? "Se ha especificado un valor que no existe en la tabla de enlace (error de Foreign Key)."
                        : "No se puede eliminar el registro, se encuentra referenciado en <br/>registros de otras tablas de la base de datos.", null);
            }
            else
            {
                var texto =
                    $"Lo sentimos, ha ocurrido un error al {operacion} el registro.<br/>Por favor, chequee el log de error de la aplicación." +
                    (string.IsNullOrWhiteSpace(errorId) ? "" : $"<br/>{{ID de Error: {errorId} }}");
                resultado = new ResultadoOperacion(-1, texto, null);
            }
            LogError(null, ex, resultado.Mensaje);
            return resultado;
        }

    }
}
