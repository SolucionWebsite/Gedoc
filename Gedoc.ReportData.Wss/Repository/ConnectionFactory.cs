using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;

namespace Gedoc.ReportData.Wss.Repository
{
    public static class ConnectionFactory
    {
        public static string ConnectionString = ConfigurationManager.ConnectionStrings["GDOCConnectionString"].ConnectionString;
        
        public static Func<DbConnection> Connection = () => new SqlConnection(ConnectionString);
    }
}
