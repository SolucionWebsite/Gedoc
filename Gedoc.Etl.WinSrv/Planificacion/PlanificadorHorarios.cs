using Gedoc.Etl.Winsrv.Logging;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Planificacion
{
    public class PlanificadorHorarios
    {
        public static async Task<bool> NuevaPlanificacionEjecucionAsync()
        {
            var horarioConfig = ConfigurationManager.AppSettings["HorarioEjecucion"];
            if (string.IsNullOrEmpty(horarioConfig))
            {
                // TODO: indicar q no se ha encontrado la llave en el config
                Logger.Execute().Error("Key 'HorarioEjecucion' no existe en el .config");
                return false;
            }
            var cronExpression = "";
            if (horarioConfig.Contains(":"))
            { // El horario especificado en el .config está en el formato HH:mm
                var hora = horarioConfig.Split(':')[0];
                var minutos = horarioConfig.Split(':')[1];
                cronExpression = GetPlanCronByHoraMinutos(hora, minutos);
            }
            else
            { // Se asume q el horario definido en el .config es una expresión cron completa
                cronExpression = horarioConfig;
            }
            if (!CronExpression.IsValidExpression(cronExpression))
            { // No es válida la expressión cron definida en el .config o la generada por la aplicación
                // TODO: indicar q no es válida la expressión cron del .config
                Logger.Execute().Error("Expressión cron en 'HorarioChequeoFechaRes' no es válida.");
                return false;
            }

            var esOk = true;
            var jobId = "jobEtl";
            var triggerId = "triggerEtl";
            var grupoId = "grupoEtl";
            var cron = new CronExpression(cronExpression);

            var proxEjecucion = cron.GetNextValidTimeAfter(DateTime.Now);
            if (proxEjecucion != null)
            {
                // construct a scheduler factory
                var schedulerFactory = new StdSchedulerFactory();


                // get a scheduler
                IScheduler scheduler = await schedulerFactory.GetScheduler();
                await scheduler.Start();

                IJobDetail job = JobBuilder.Create<JobEtl>()
                    .WithIdentity(jobId, grupoId)
                    .Build();

                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerId, grupoId)
                    .StartNow()
                    .WithCronSchedule(cronExpression, c => c.WithMisfireHandlingInstructionIgnoreMisfires())
                    .Build();

                await scheduler.ScheduleJob(job, trigger);

                Logger.Execute().Info("Se ha planificado con éxito la ejecución de ETL con la siguiente expresión cron: " + cronExpression);
            }
            else
            {
                Logger.Execute().Info("La expresión cron especificada en el config no genera una fecha futura para la ejecución de ETL.");
                esOk = false;
            }

            return esOk;
        }

        private static string GetPlanCronByHoraMinutos(string hora, string minutos)
        {
            var seg = "0"; 
            var diaMes = "?";
            var mes = "*";
            var diaSemana = "*";
            var anno = "*";

            var plan = string.Format("{0} {1} {2} {3} {4} {5} {6}", seg, minutos, hora, diaMes, mes, diaSemana, anno) ;
            return plan;
        }

    }
}
