using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;

namespace Gedoc.Etl.Winsrv.Servicios
{
    public static class ConnectionFactory
    {
        public static string ConnectionStringOrigen = ConfigurationManager.ConnectionStrings["GedocOrigen"].ConnectionString;
        public static string ConnectionStringDestino = ConfigurationManager.ConnectionStrings["GedocDestino"].ConnectionString;

        public static Func<DbConnection> ConnectionOrig = () => new SqlConnection(ConnectionStringOrigen);
        public static Func<DbConnection> ConnectionDest = () => new SqlConnection(ConnectionStringDestino);
    }
}
