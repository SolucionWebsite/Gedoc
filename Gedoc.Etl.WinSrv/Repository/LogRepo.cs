using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Gedoc.Etl.Winsrv.Entidades;
using Gedoc.Etl.Winsrv.Logging;
using Gedoc.Etl.Winsrv.Servicios;

namespace Gedoc.Etl.Winsrv.Repository
{
    public class LogRepo
    {
        private readonly int timeOut = Convert.ToInt32(ConfigurationManager.AppSettings["DapperExecuteTimeOutSeconds"] ?? "120");

        public int Add(LogEtl dato)
        {
            try
            {
                var sql = Queries.InsertLog + " SELECT CAST(SCOPE_IDENTITY() as int)";

                using (var connection = ConnectionFactory.ConnectionDest())
                {
                    connection.Open();
                    var returnId = connection.Query<int>(sql, dato, commandTimeout: timeOut).SingleOrDefault();
                    dato.Id = returnId;
                }
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                return -1;
            }
            return dato.Id;
        }

        public DateTime? GetFechaUltimaCargaOk(string tipoCarga)
        {
            try
            {
                var minFecha = new DateTime(2015, 1, 1);

                var sql = Queries.UltimaFechaLog;

                using (var connection = ConnectionFactory.ConnectionDest())
                {
                    connection.Open();
                    var valor = connection.Query<DateTime?>(sql, new { Tipo = tipoCarga }, commandTimeout: timeOut).SingleOrDefault() ?? minFecha;

                    // Sept-2019 - Después del cambio de horario hay registros q no se cargan cuando se hacen cargas manuales seguidas, es algún
                    // tema con el cambio de horario. Para evitar esto se toman todos los registros modificados o creados a partir de las 00:00 horas
                    valor = valor.Date;
#if DEBUG
                    Console.WriteLine("Procesando a partir de la fecha " + valor.ToString("dd/MM/yyyy HH:mm:ss"));
#endif
                    return valor;
                }
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                return null;
            }
        }

        public List<LogEtl> GetLastLogs()
        {
            var logs = new List<LogEtl>();
            try
            {
                var sql = Queries.UltimosLog;

                using (var connection = ConnectionFactory.ConnectionDest())
                {
                    connection.Open();
                    logs = connection.Query<LogEtl>(sql).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
            }
            return logs;
        }

        public string GetEstadoEjecucion()
        {
            var estado = "";
            try
            {
                var sql = Queries.EstadoEjecucion;

                using (var connection = ConnectionFactory.ConnectionDest())
                {
                    connection.Open();
                    var logs = connection.Query<LogEtl>(sql).ToList();
                    if (logs.Count == 0)
                    {
                        estado = "En espera de ejecución de carga de datos.";
                    }
                    else
                    {
                        var ultLog = logs[0];
                        if (ultLog.Tipo.StartsWith("EJECUTANDO-CARGA-DATOS"))
                        {
                            estado = "Ejecutando carga de datos.";
                        }
                        else if (ultLog.Tipo.StartsWith("FIN-CARGA-DATOS"))
                        {
                            estado = "Última carga: " + ultLog.Fecha.ToString("dd/MM/yyyy H:mm") + ". En espera de próxima carga de datos.";
                        }
                        else
                        {
                            estado = "En espera de próxima carga de datos.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
            }
            return estado;
        }


    }
}
