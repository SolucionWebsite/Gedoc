using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;
using Gedoc.ReportData.Wss.Data;
using Gedoc.ReportData.Wss.Logging;

namespace Gedoc.ReportData.Wss.Repository
{
    public class GenericRepo
    {
        private readonly int timeOut =
            Convert.ToInt32(ConfigurationManager.AppSettings["DapperExecuteTimeOutSeconds"] ?? "120");

        public int ExecuteSP(string nombreSp, object parametros)
        {
            try
            {
                using (var connection = ConnectionFactory.Connection())
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

        public List<T> ExecuteSP<T>(string nombreSp, object parametros, bool propagaError = false)
        {
            try
            {
                using (var connection = ConnectionFactory.Connection())
                {
                    connection.Open();
                    var result = connection.Query<T>(nombreSp, parametros, commandType: CommandType.StoredProcedure,
                        commandTimeout: timeOut).ToList();

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

        public List<T> ExecuteQuery<T>(string sql, object parametros, bool propagaError = false)
        {
            try
            {
                using (var connection = ConnectionFactory.Connection())
                {
                    connection.Open();
                    var result = connection.Query<T>(sql, param: parametros).ToList();
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

        public List<dynamic> ExecuteQuery(string sql, object parametros, bool propagaError = false)
        {
            try
            {
                using (var connection = ConnectionFactory.Connection())
                {
                    connection.Open();
                    var result = connection.Query(sql, param: parametros).ToList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Execute().Error(ex);
                if (propagaError)
                    throw;
                return new List<dynamic>();
            }
        }




    }
}
