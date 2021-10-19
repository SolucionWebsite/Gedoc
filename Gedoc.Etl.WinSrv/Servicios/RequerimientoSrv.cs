using System;
using System.Collections.Generic;
using System.Linq;
using Gedoc.Etl.Winsrv.Entidades;
using Gedoc.Etl.Winsrv.Helpers;
using Gedoc.Etl.Winsrv.Logging;
using Gedoc.Etl.Winsrv.Repository;

namespace Gedoc.Etl.Winsrv.Servicios
{
    public class RequerimientoSrv
    {
        private const int DiasCarga = 30;
        private readonly LogRepo logRepo = new LogRepo();
        private readonly GenericRepo _repo = new GenericRepo();

        public ResultadoOperacion ProcesaData()
        {
            var resultado = new ResultadoOperacion(1, "Operación realizada con éxito.");

            #region Log de carga de datos
            var logId = logRepo.Add(new LogEtl()
            {
                Tipo = "REQUERIMIENTO-CARGA",
                Fecha = DateTime.Now,
                Descripcion = "CARGA DE DATOS DE REQUERIMIENTOS"
            });
            #endregion

            #region Fecha de Última Carga
            var fechaUltimaCarga = logRepo.GetFechaUltimaCargaOk("REQUERIMIENTO-CARGA-FIN");
            if (fechaUltimaCarga == null)
            {
                logRepo.Add(new LogEtl()
                {
                    Tipo = "REQUERIMIENTO-CARGA-ERROR",
                    Fecha = DateTime.Now,
                    Descripcion = "Ocurrió un error al obtener la fecha de la carga anterior, por favor, revisé el fichero log de error de la aplicación.",
                    ParentLogId = logId
                });
                resultado.Codigo = -1;
                resultado.Texto = "Error";
                return resultado;
            }
            #endregion

            #region Carga de datos desde BD Principal hacia BD Reportes
            var fechaDesde = fechaUltimaCarga.GetValueOrDefault();
            Logger.Execute().Info($"..... tomando registros modificados/creados a partir de la fecha {fechaDesde.ToString("dd/MM/yyyy HH:mm")}");
            // Para evitar sobrecarga de memoria se toman grupos de registros en un rango de 30 días. Esto es fundamentalmente
            // para la importación inicial pues después las cargas se deben hacer diarias
            var fechaHasta = fechaDesde.AddDays(DiasCarga).Date.AddSeconds(-1);
            var esOk = true;
            while (fechaDesde.CompareTo(DateTime.Now) <= 0)
            {
                esOk = CargaFromOrigenToDestino(logId, fechaDesde, fechaHasta) && esOk; // Copia los datos de origen a destino en una tabla tempora
                fechaDesde = fechaHasta.AddSeconds(1);
                fechaHasta = fechaDesde.AddDays(DiasCarga + 1).Date.AddSeconds(-1);
            }

            if (!esOk)
            {
                logRepo.Add(new LogEtl()
                {
                    Tipo = "REQUERIMIENTO-CARGA-ERROR",
                    Fecha = DateTime.Now,
                    Descripcion = "Ocurrió al menos un error durante la carga de datos, por favor, revisé el fichero log de error de la aplicación.",
                    ParentLogId = logId
                });
                resultado.Codigo = -1;
                resultado.Texto = "Ocurrió al menos un error durante la carga de datos.";
            }
            #endregion

            #region SP Carga desde tabla temporal a tabla de requerimientos

            var resultadoSProc = _repo.SpCopiaDatosDest(new { idCarga = logId }, "SpCopiaRequerimientos"); // gatilla el procedimiento q copia de la tabla temporal a la tabla final
            if (resultadoSProc.Codigo < 0)
                esOk = false;
            #endregion

            logRepo.Add(new LogEtl()
            {
                Tipo = "REQUERIMIENTO-CARGA-FIN" + (esOk ? "" : "-ERROR"),
                Fecha = DateTime.Now,
                Descripcion = "PROCESO DE CARGA FINALIZADO " + (esOk ? "" : "CON ERROR(ES)."),
                ParentLogId = logId
            });

            return resultado;

        }

        /// <summary>
        /// Carga desde la BD origen los requerimientos entre el rango de fechas especificados y los inserta en la tabla
        /// RequerimientosCarga de la BD destino.
        /// </summary>
        /// <param name="idCarga">Id de carga</param>
        /// <param name="fechaD">Fecha desde para filtrar los datos a obtener</param>
        /// <param name="fechaH">Fecha hasta para filtrar los datos a obtener</param>
        /// <returns>true si no hubo error, false si hubo error</returns>
        private bool CargaFromOrigenToDestino(int idCarga, DateTime fechaDesde, DateTime fechaHasta)
        {
            var esOk = true;
            var datos = _repo.GetDatosFromOrigen<Requerimiento>(Queries.SelectRequerimientosOrigen, new { fechaD = fechaDesde, fechaH = fechaHasta });
            if (datos == null)
            {
                logRepo.Add(new LogEtl()
                {
                    Tipo = "REQUERIMIENTO-CARGA-ERROR-ORIG",
                    Fecha = DateTime.Now,
                    Descripcion = "Ocurrió un error al cargar los datos desde la BD de origen, por favor, revisé el fichero log de error de la aplicación.",
                    ParentLogId = idCarga
                });
                return false;
            }

            var datosInsertar = new List<Requerimiento>();
            foreach (var dato in datos)
            {
                dato.IdCarga = idCarga;
                datosInsertar.Add(dato);
                if (datosInsertar.Count >= 5000)
                {
                    // Se insertan de 5000 en 5000 registros, esto sólo debe ocurrir la primera vez q se ejecuta el servicio cuando no hay datos en la BD                    var InsertaEnBdTemp(datos);
                    var result = _repo.InsertaEnDestinoTemp(datosInsertar, Queries.InsertRequerimientoCarga);
                    if (result.Codigo < 0)
                    {
                        // Hubo error insertando en BD
                        // TODO: se detiene, se reintenta, o se sigue con los siguientes registros????
                        // Ahora se sigue
                        esOk = false;
                    }
                    datosInsertar.Clear();
                }
            }

            #region Inserta datos en tabla temporal en BD
            if (datosInsertar.Count > 0)
            {
                var result = _repo.InsertaEnDestinoTemp(datosInsertar, Queries.InsertRequerimientoCarga);
                if (result.Codigo < 0)
                { // Hubo error insertando en BD
                    // TODO: se reintenta????
                    // Ahora se sigue
                    esOk = false;
                }
            }
            datos.Clear();
            datosInsertar.Clear();
            #endregion

            return esOk;
        }

        /// <summary>
        /// Carga del origen todos los despachos, despachos iniciativas, bitácoras y requerimientos
        /// y elimina en el destino aquellos registros q no están en el origen y q por tanto fueron eliminados
        /// </summary>
        /// <param name="logId">Log de carga</param>
        /// <returns>ResultadoOperacion</returns>
        public ResultadoOperacion EliminaRegistros(int logId)
        {
            var resultado = new ResultadoOperacion(1, "Operación realizada con éxito.");

            #region Elimina en BD las BITÁCORAS borradas en la bd origen
            var parentId = logRepo.Add(new LogEtl()
            {
                Tipo = "BITACORA-ELIMINADOS",
                Fecha = DateTime.Now,
                Descripcion = "Elimina de la BD de reportes los registros eliminados en la BD principal",
                ParentLogId = 0
            });
            var resultDel = CargaTodoFromOrigenToDestino<Bitacora>(parentId, "BITACORA",
                Queries.SelectBitacorasOrigenResumen,
                Queries.InsertBitacoraCarga);
            if (resultDel.Codigo > 0)
            {
                resultado = _repo.EliminaRegistrosDest("SpEliminaBitacoras");
                logRepo.Add(new LogEtl()
                {
                    Tipo = "BITACORA-ELIMINADOS-FIN",
                    Fecha = DateTime.Now,
                    Descripcion = "PROCESO DE LIMPIEZA FINALIZADO ",
                    ParentLogId = parentId
                });
            }
            #endregion

            #region Elimina en BD los DESPACHOS INICIATIVAS borrados en la bd origen
            parentId = logRepo.Add(new LogEtl()
            {
                Tipo = "DESPACHOSINIC-ELIMINADOS",
                Fecha = DateTime.Now,
                Descripcion = "Elimina de la BD de reportes los registros eliminados en la BD principal",
                ParentLogId = 0
            });
            resultDel = CargaTodoFromOrigenToDestino<DespachosIniciativa>(parentId, "DESPACHOSINIC",
                Queries.SelectDespachosInicOrigenResumen,
                Queries.InsertDespachoInicCarga);
            if (resultDel.Codigo > 0)
            {
                resultado = _repo.EliminaRegistrosDest("SpEliminaDespachosInic");
                logRepo.Add(new LogEtl()
                {
                    Tipo = "DESPACHOSINIC-ELIMINADOS-FIN",
                    Fecha = DateTime.Now,
                    Descripcion = "PROCESO DE LIMPIEZA FINALIZADO ",
                    ParentLogId = parentId
                });
            }
            #endregion

            #region Elimina en BD los DESPACHOS borrados en la bd origen
            parentId = logRepo.Add(new LogEtl()
            {
                Tipo = "DESPACHOS-ELIMINADOS",
                Fecha = DateTime.Now,
                Descripcion = "Elimina de la BD de reportes los registros eliminados en la BD principal",
                ParentLogId = 0
            });
            resultDel = CargaTodoFromOrigenToDestino<Despacho>(parentId, "DESPACHOS",
                Queries.SelectDespachosOrigenResumen,
                Queries.InsertDespachoCarga);
            if (resultDel.Codigo > 0)
            {
                resultado = _repo.EliminaRegistrosDest("SpEliminaDespachos");
                logRepo.Add(new LogEtl()
                {
                    Tipo = "DESPACHOS-ELIMINADOS-FIN",
                    Fecha = DateTime.Now,
                    Descripcion = "PROCESO DE LIMPIEZA FINALIZADO ",
                    ParentLogId = parentId
                });
            }
            #endregion

            #region Elimina en BD los REQUERIMIENTOS borrados en la bd origen
            parentId = logRepo.Add(new LogEtl()
            {
                Tipo = "REQUERIMIENTO-ELIMINADOS",
                Fecha = DateTime.Now,
                Descripcion = "Elimina de la BD de reportes los registros eliminados en la BD principal",
                ParentLogId = 0
            });
            resultDel = CargaTodoFromOrigenToDestino<Requerimiento>(parentId, "REQUERIMIENTO",
                Queries.SelectRequerimientosOrigenResumen,
                Queries.InsertRequerimientoCarga);
            if (resultDel.Codigo > 0)
            {
                resultado = _repo.EliminaRegistrosDest("SpEliminaRequerimientos");
                logRepo.Add(new LogEtl()
                {
                    Tipo = "REQUERIMIENTO-ELIMINADOS-FIN",
                    Fecha = DateTime.Now,
                    Descripcion = "PROCESO DE LIMPIEZA FINALIZADO ",
                    ParentLogId = parentId
                });
            }
            #endregion

            return resultado;
        }

        /// <summary>
        /// Carga desde la BD origen los datos indicados según la query querySelectResumenOrig, pero solo con dato en el campo Id, y los inserta en la tabla
        /// de Carga de la BD destino.
        /// </summary>
        /// <param name="idCarga">Id de carga</param>
        /// <param name="tipo">Tipo de datos a cargar: BITACORA, DESPACHO, DESPACHOINIC o REQUERIMIENTO</param>
        /// <param name="querySelectResumenOrig">Query para obtener los datos en el origen</param>
        /// <param name="queryInsertDst">Query para insertar los datos en el destino</param>
        /// <returns>ResultadoOperacion</returns>
        private ResultadoOperacion CargaTodoFromOrigenToDestino<T>(int idCarga, string tipo, string querySelectResumenOrig, string queryInsertDst ) where T: BaseEntity
        {
            var resultado = new ResultadoOperacion(1, "Operación realizada con éxito.");
            try
            {
                var datos = _repo.GetDatosFromOrigen<T>(querySelectResumenOrig, new { });
                if (datos == null)
                {
                    logRepo.Add(new LogEtl()
                    {
                        Tipo = $"{tipo}-CARGA-ERROR-ORIG",
                        Fecha = DateTime.Now,
                        Descripcion = "Ocurrió un error al cargar los datos desde la BD de origen, por favor, revisé el fichero log de error de la aplicación.",
                        ParentLogId = idCarga
                    });
                    return new ResultadoOperacion(-1, "Error cargando datos desde la BD origen.");
                }

                var datosInsertar = new List<T>();
                foreach (var dato in datos)
                {
                    dato.IdCarga = idCarga;
                    datosInsertar.Add(dato);
                    if (datosInsertar.Count >= 5000)
                    {
                        // Se insertan de 5000 en 5000 registros, esto sólo debe ocurrir la primera vez q se ejecuta el servicio cuando no hay datos en la BD                    var InsertaEnBdTemp(datos);
                        var result = _repo.InsertaEnDestinoTemp(datosInsertar, queryInsertDst);
                        if (result.Codigo < 0)
                        {
                            // Hubo error insertando en BD
                            return new ResultadoOperacion(-1, "Error insertando datos en BD.");
                        }
                        datosInsertar.Clear();
                    }
                }

                #region Inserta datos en tabla temporal en BD
                if (datosInsertar.Count > 0)
                {
                    var result = _repo.InsertaEnDestinoTemp(datos, queryInsertDst);
                    if (result.Codigo < 0)
                    { // Hubo error insertando en BD
                        return new ResultadoOperacion(-1, "Error insertando datos en BD.");
                    }
                    datos.Clear();
                }
                datos.Clear();
                datosInsertar.Clear();
                #endregion
            }
            catch (Exception ex)
            {
                //Logger.Execute().Error(ex);
                resultado.Codigo = -1;
                resultado.Texto = ex.Message;
            }

            return resultado;
        }

    }
}
