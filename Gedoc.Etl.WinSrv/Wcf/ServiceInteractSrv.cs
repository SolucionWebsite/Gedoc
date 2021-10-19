using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Etl.Winsrv.Entidades;
using Gedoc.Etl.Winsrv.Repository;
using Gedoc.Etl.Winsrv.Servicios;

namespace Gedoc.Etl.Winsrv.Wcf
{
    public class ServiceInteractSrv : IServiceInteract
    {
        public string ExecuteEtl()
        {
            var etlSrv = new EtlServiceGlobal();
            var resultado = etlSrv.ExecuteEtl(true);
            return resultado ? "OK" : "ERROR";
        }

        public string ExecuteEtlSelectivo(string[] destinos)
        {
            var etlSrv = new EtlServiceGlobal();
            var resultado = etlSrv.ExecuteEtl(true, destinos);
            return resultado ? "OK" : "ERROR";
        }

        public List<LogEtl> GetLastLogs()
        {
            var logRepo = new LogRepo();
            var data = logRepo.GetLastLogs();
            return data;
        }

        public string GetEstadoEjecucion()
        {
            var logRepo = new LogRepo();
            var data = logRepo.GetEstadoEjecucion();
            return data;
        }
    }
}
