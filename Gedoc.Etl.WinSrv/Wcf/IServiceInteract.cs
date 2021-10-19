using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Etl.Winsrv.Entidades;

namespace Gedoc.Etl.Winsrv.Wcf
{
    //[ServiceContract(Namespace = "http://Gedoc.Etl.WinSrv.Wcf")]
    [ServiceContract]
    public interface IServiceInteract
    {
        [OperationContract]
        string ExecuteEtl();

        [OperationContract]
        string ExecuteEtlSelectivo(string[] destinos);
        [OperationContract]
        List<LogEtl> GetLastLogs();
        [OperationContract]
        string GetEstadoEjecucion();
    }
}
