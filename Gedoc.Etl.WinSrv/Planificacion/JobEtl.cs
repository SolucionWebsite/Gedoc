using System.Threading.Tasks;
using Gedoc.Etl.Winsrv.Logging;
using Quartz;
using Gedoc.Etl.Winsrv.Servicios;

namespace Gedoc.Etl.Winsrv.Planificacion
{
    public class JobEtl : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Logger.Execute().Info("Alcanzado horario de ejecución de ETL. Ejecutando proceso...");
            return Task.Factory.StartNew(() =>
            {
                EjecutarEtl();
            });
            //EjecutarEtl();
            //return Task.CompletedTask;
        }

        private void EjecutarEtl()
        {
			var etlSrv = new EtlServiceGlobal();
            etlSrv.ExecuteEtl(false);
        }

    }
}
