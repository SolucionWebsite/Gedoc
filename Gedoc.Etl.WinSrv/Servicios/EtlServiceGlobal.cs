using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Etl.Winsrv.Entidades;
using Gedoc.Etl.Winsrv.Logging;
using Gedoc.Etl.Winsrv.Repository;

namespace Gedoc.Etl.Winsrv.Servicios
{
    public class EtlServiceGlobal
    {
        public bool ExecuteEtl(bool manual, string[] destinos = null)
        {
            var logRepo = new LogRepo();
            try
            {

                if (destinos == null || destinos.Length == 0)
                {
                    destinos = new string[] { "REQ", "BIT", "DES", "DIN", "UT", "REG", "CASO", "LOG", "SES" };
                }

                #region Log de inicio carga de datos
                var logId = logRepo.Add(new LogEtl()
                {
                    Tipo = "EJECUTANDO-CARGA-DATOS" + (manual ? "-MANUAL" : "-PLANIFICADA"),
                    Fecha = DateTime.Now,
                    Descripcion = "EJECUTANDO CARGA DE DATOS"
                });
                Logger.Execute().Info("COMENZANDO CARGA " + (manual ? "MANUAL" : "PLANIFICADA"));
                #endregion

                #region Procesamiento de Requerimientos
                var reqSrv = new RequerimientoSrv();
                if (destinos.Contains("REQ"))
                {
                    Logger.Execute().Info("Procesando Requerimientos.");
                    reqSrv.ProcesaData();
                    Logger.Execute().Info("Fin de procesamiento de Requerimientos. ");
                }
                #endregion

                #region Procesamiento de Bitácoras
                if (destinos.Contains("BIT"))
                {
                    Logger.Execute().Info("Procesando Bitácoras.");
                    var bitSrv = new BitacoraSrv();
                    bitSrv.ProcesaData();
                    Logger.Execute().Info("Fin de procesamiento de Bitácoras. ");
                }
                #endregion

                #region Procesamiento de Despachos
                var despSrv = new DespachoSrv();
                if (destinos.Contains("DES"))
                {
                    Logger.Execute().Info("Procesando Despachos.");
                    despSrv.ProcesaData();
                    Logger.Execute().Info("Fin de procesamiento de Despachos. ");
                }
                #endregion

                #region Procesamiento de Despachos Iniciativa
                if (destinos.Contains("DIN"))
                {
                    Logger.Execute().Info("Procesando Despachos Iniciativa.");
                    despSrv.ProcesaDataDespInic();
                    Logger.Execute().Info("Fin de procesamiento de Despachos Iniciativa. ");
                }
                #endregion

                #region Procesamiento de Unidades Técnicas
                if (destinos.Contains("UT"))
                {
                    Logger.Execute().Info("Procesando Unidades Técnicas.");
                    var utSrv = new UnidadTecnSrv();
                    utSrv.ProcesaData();
                    Logger.Execute().Info("Fin de procesamiento de Unidades Técnicas. ");
                }
                #endregion

                #region Procesamiento de Regiones
                if (destinos.Contains("REG"))
                {
                    Logger.Execute().Info("Procesando Regiones y Comunas.");
                    var regSrv = new RegionesComunasSrv();
                    regSrv.ProcesaData();
                    Logger.Execute().Info("Fin de procesamiento de Regiones y Comunas. ");
                }
                #endregion

                #region Procesamiento de Casos
                if (destinos.Contains("CASO"))
                {
                    Logger.Execute().Info("Procesando Mantenedor de Casos.");
                    var casoSrv = new CasosSrv();
                    casoSrv.ProcesaData();
                    Logger.Execute().Info("Fin de procesamiento de Mantenedor de Casos. ");
                }
                #endregion

                #region Procesamiento de LogSistema
                if (destinos.Contains("LOG"))
                {
                    Logger.Execute().Info("Procesando Log de Sistema.");
                    var logSrv = new LogSistemaSrv();
                    logSrv.ProcesaData();
                    Logger.Execute().Info("Fin de procesamiento de Log de Sistema. ");
                }
                #endregion

                #region Procesamiento de Tablas de Sesión
                if (destinos.Contains("SES"))
                {
                    var sesSrv = new SesionTablaSrv();
                    Logger.Execute().Info("Procesando Tablas de Sesión.");
                    sesSrv.ProcesaData();
                    sesSrv.ProcesaDataDet();
                    Logger.Execute().Info("Fin de procesamiento de Tablas de Sesión. ");
                }
                #endregion

                #region Eliminación en BD reporte de registros eliminados en BD principal
                if (!manual)
                {
                    // Si se ejecuta el servicio según su planificación entonces se ejecuta el proceso de eliminar de bd
                    // los registros q han sido eliminado en Gedoc
                    Logger.Execute().Info("Eliminando de BD reportes los registros borrados en BD principal.");
                    reqSrv.EliminaRegistros(0);
                    Logger.Execute().Info("Fin de limpieza de datos. ");
                }
                #endregion

                Logger.Execute().Info("Finalizada la ejecución de ETL.");


                #region Log de inicio carga de datos
                logRepo.Add(new LogEtl()
                {
                    Tipo = "FIN-CARGA-DATOS",
                    Fecha = DateTime.Now,
                    Descripcion = "FIN DE CARGA DE DATOS"
                });
                #endregion

                return true;
            }
            catch (Exception exc)
            {
                Logger.Execute().Error(exc);
                #region Log de inicio carga de datos
                logRepo.Add(new LogEtl()
                {
                    Tipo = "FIN-CARGA-DATOS",
                    Fecha = DateTime.Now,
                    Descripcion = "FIN DE CARGA DE DATOS CON ERROR"
                });
                #endregion
            }

            return false;


        }
    }
}
