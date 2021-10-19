using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Gedoc.Etl.Winsrv.Entidades;
using Gedoc.Etl.Winsrv.Helpers;
using Gedoc.Etl.Winsrv.Logging;
using Gedoc.Etl.Winsrv.Servicios;

namespace Gedoc.Etl.Winsrv.Repository
{
    public class GenericRepo
    {
        private readonly int timeOut = Convert.ToInt32(ConfigurationManager.AppSettings["DapperExecuteTimeOutSeconds"] ?? "120");


        /// <summary>
        /// Obtiene los datos de la BD principal de Gedoc MVC y los devuelve en una lista.
        /// </summary>
        /// <param name="fechaD">Fecha desde para filtrar los datos a obtener</param>
        /// <param name="fechaH">Fecha hasta para filtrar los datos a obtener</param>
        /// <param name="resumido">Si true entonces devuelve solo el Id de cada requerimiento</param>
        /// <returns>Lista de requermientos obtenidos de la consulta ejecutada</returns>
        public List<T> GetDatosFromOrigen<T>(string query, object param)
        {
            try
            {
                using (var connection = ConnectionFactory.ConnectionOrig())
                {
                    connection.Open();
                    var datos = connection.Query<T>(query, param, commandTimeout: timeOut);
                    return datos.ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Inserta los datos en la tabla temporal de carga de la BD de destino
        /// </summary>
        /// <param name="datos">Datos a insertar</param>
        /// <param name="query">Query para insertar los datos</param>
        /// <returns>ResultadoOperacion</returns>
        public ResultadoOperacion InsertaEnDestinoTemp<T>(IList<T> datos, string query)
        {
            var resultado = new ResultadoOperacion(1, "Proceso realizado con éxito.");
            try
            {
                using (var connection = ConnectionFactory.ConnectionDest())
                {
                    connection.Open();
                    var result = connection.Execute(query, datos, commandTimeout: timeOut);
                    resultado.Codigo = result >= 0 ? 1 : -1;
                    resultado.Texto = result >= 0 ? resultado.Texto : "Error realizando la operación.";
                }
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                resultado.Codigo = -1;
                resultado.Texto = "Error realizando la operación.";
            }
            return resultado;
        }

        /// <summary>
        /// Ejecuta el procedimiento indicado de parametro que es el que copia los datos de la tabla temporal de carga hacia
        /// la tabla final.
        /// </summary>
        /// <param name="param">Parametros a pasarle al procedimiento a ejecutar</param>
        /// <param name="nombreSp">Stored Procedure a ejecutar</param>
        /// <returns>ResultadoOperacion</returns>
        public ResultadoOperacion SpCopiaDatosDest(object param, string nombreSp)
        {
            var resultado = new ResultadoOperacion(1, "Proceso realizado con éxito.");
            try
            {
                ExecuteSP(nombreSp, param);
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                resultado.Codigo = -1;
                resultado.Texto = "Error realizando la operación.";
            }
            return resultado;
        }

        /// <summary>
        /// Elimina en la BD destinos los registros q fueron eliminados en la BD origen
        /// </summary>
        /// <returns>ResultadoOperacion</returns>
        public ResultadoOperacion EliminaRegistrosDest(string nombreSpElimina)
        {
            var resultado = new ResultadoOperacion(1, "Proceso realizado con éxito.");
            try
            {
                var genericRepo = new GenericRepo();
                genericRepo.ExecuteSP(nombreSpElimina, null);
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                resultado.Codigo = -1;
                resultado.Texto = "Error realizando la operación.";
            }
            return resultado;
        }

        public int ExecuteSP(string nombreSp, object parametros)
        {
            try
            {
                using (var connection = ConnectionFactory.ConnectionDest())
                {
                    connection.Open();
                    var affectedRows = connection.Execute(nombreSp,
                        parametros,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: timeOut);
                    return affectedRows;
                }
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                return -1;
            }
        }

        public object ExecuteSP<T>(string nombreSp, object parametros, bool propagaError = false)
        {
            try
            {
                using (var connection =  ConnectionFactory.ConnectionDest())
                {
                    connection.Open();
                    var result = connection.Query<T>(nombreSp, parametros, commandType: CommandType.StoredProcedure, commandTimeout: timeOut).ToList();

                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                if (propagaError)
                    throw;
                return new List<T>();
            }
        }


    }
}
