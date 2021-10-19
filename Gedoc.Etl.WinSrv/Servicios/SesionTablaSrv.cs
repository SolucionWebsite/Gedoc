using System;
using System.Collections.Generic;
using System.Linq;
using Gedoc.Etl.Winsrv.Entidades;
using Gedoc.Etl.Winsrv.Helpers;
using Gedoc.Etl.Winsrv.Logging;
using Gedoc.Etl.Winsrv.Repository;

namespace Gedoc.Etl.Winsrv.Servicios
{
    public class SesionTablaSrv
    {
        private const int DiasCarga = 30;
        private readonly GenericRepo _repo = new GenericRepo();
        private readonly LogRepo logRepo = new LogRepo();

        public ResultadoOperacion ProcesaData()
        {
            var resultado = new ResultadoOperacion(1, "Operación realizada con éxito.");

            #region Log de carga de datos
            var logId = logRepo.Add(new LogEtl()
            {
                Tipo = "SESIONTABLA-CARGA",
                Fecha = DateTime.Now,
                Descripcion = "CARGA DE DATOS DE SESION TABLA"
            });
            #endregion

            #region Fecha de Última Carga
            var fechaUltimaCarga = logRepo.GetFechaUltimaCargaOk("SESIONTABLA-CARGA-FIN");
            if (fechaUltimaCarga == null)
            {
                logRepo.Add(new LogEtl()
                {
                    Tipo = "SESIONTABLA-CARGA-ERROR",
                    Fecha = DateTime.Now,
                    Descripcion = "Ocurrió un error al obtener la fecha de la carga anterior, por favor, revisé el fichero log de error de la aplicación.",
                    ParentLogId = logId
                });
                resultado.Codigo = -1;
                resultado.Texto = "Error";
                return resultado;
            }
            #endregion

            #region Carga de datos SP hacia BD
            var fechaDesde = fechaUltimaCarga.GetValueOrDefault();
            Logger.Execute().Info($"..... tomando registros modificados/creados a partir de la fecha {fechaDesde.ToString("dd/MM/yyyy HH:mm")}");
            // Para evitar sobrecarga de memoria se toman grupos de registros en un rango de 30 días. Esto es fundamentalmente
            // para la importación inicial pues después las cargas se deben hacer diarias
            var fechaHasta = fechaDesde.AddDays(DiasCarga).Date.AddSeconds(-1);
            var esOk = true;
            while (fechaDesde.CompareTo(DateTime.Now) <= 0)
            {
                esOk = CargaFromOrigenToDestino(logId, fechaDesde, fechaHasta) && esOk;
                fechaDesde = fechaHasta.AddSeconds(1);
                fechaHasta = fechaDesde.AddDays(DiasCarga + 1).Date.AddSeconds(-1);
            }

            if (!esOk)
            {
                logRepo.Add(new LogEtl()
                {
                    Tipo = "SESIONTABLA-CARGA-ERROR",
                    Fecha = DateTime.Now,
                    Descripcion = "Ocurrió al menos un error durante la carga de datos, por favor, revisé el fichero log de error de la aplicación.",
                    ParentLogId = logId
                });
                resultado.Codigo = -1;
                resultado.Texto = "Ocurrió al menos un error durante la carga de datos.";
            }
            #endregion

            #region SP Carga desde tabla temporal a tabla de TB_SesionTabla

            var resultadoSProc = _repo.SpCopiaDatosDest(new { idCarga = logId }, "SpCopiaSesionTabla");
            if (resultadoSProc.Codigo < 0)
                esOk = false;
            #endregion

            logRepo.Add(new LogEtl()
            {
                Tipo = "SESIONTABLA-CARGA-FIN" + (esOk ? "" : "-ERROR"),
                Fecha = DateTime.Now,
                Descripcion = "PROCESO DE CARGA FINALIZADO " + (esOk ? "" : "CON ERROR(ES)."),
                ParentLogId = logId
            });

            return resultado;

        }

        /// <summary>
        /// Carga desde la BD origen las sesiontabla entre el rango de fechas especificados y los inserta en la tabla
        /// TB_SesionTablaCarga de la BD destino.
        /// </summary>
        /// <param name="idCarga">Id de carga</param>
        /// <param name="fechaD">Fecha desde para filtrar los datos a obtener</param>
        /// <param name="fechaH">Fecha hasta para filtrar los datos a obtener</param>
        /// <returns>true si no hubo error, false si hubo error</returns>
        private bool CargaFromOrigenToDestino(int idCarga, DateTime fechaDesde, DateTime fechaHasta)
        {
            var esOk = true;
            var datos = _repo.GetDatosFromOrigen<SesionTabla>(Queries.SelectSesionTablaOrigen, new { fechaD = fechaDesde, fechaH = fechaHasta });
            if (datos == null)
            {
                logRepo.Add(new LogEtl()
                {
                    Tipo = "SESIONTABLA-CARGA-ERROR-ORIG",
                    Fecha = DateTime.Now,
                    Descripcion = "Ocurrió un error al cargar los datos desde la BD de origen, por favor, revisé el fichero log de error de la aplicación.",
                    ParentLogId = idCarga
                });
                return false;
            }

            var datosInsertar = new List<SesionTabla>();
            foreach (var dato in datos)
            {
                dato.IdCarga = idCarga;
                datosInsertar.Add(dato);
                if (datosInsertar.Count >= 5000)
                {
                    // Se insertan de 5000 en 5000 registros, esto sólo debe ocurrir la primera vez q se ejecuta el servicio cuando no hay datos en la BD                    var InsertaEnBdTemp(datos);
                    var result = _repo.InsertaEnDestinoTemp(datosInsertar, Queries.InsertSesionTablaCarga);
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
                var result = _repo.InsertaEnDestinoTemp(datosInsertar, Queries.InsertSesionTablaCarga);
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

        public ResultadoOperacion ProcesaDataDet()
        {
            var resultado = new ResultadoOperacion(1, "Operación realizada con éxito.");

            #region Log de carga de datos
            var logId = logRepo.Add(new LogEtl()
            {
                Tipo = "SESIONTABLADET-CARGA",
                Fecha = DateTime.Now,
                Descripcion = "CARGA DE DATOS DE SESION TABLA"
            });
            #endregion

            #region Fecha de Última Carga
            var fechaUltimaCarga = logRepo.GetFechaUltimaCargaOk("SESIONTABLADET-CARGA-FIN");
            if (fechaUltimaCarga == null)
            {
                logRepo.Add(new LogEtl()
                {
                    Tipo = "SESIONTABLADET-CARGA-ERROR",
                    Fecha = DateTime.Now,
                    Descripcion = "Ocurrió un error al obtener la fecha de la carga anterior, por favor, revisé el fichero log de error de la aplicación.",
                    ParentLogId = logId
                });
                resultado.Codigo = -1;
                resultado.Texto = "Error";
                return resultado;
            }
            #endregion

            #region Carga de datos SP hacia BD
            var fechaDesde = fechaUltimaCarga.GetValueOrDefault();
            Logger.Execute().Info($"..... tomando registros modificados/creados a partir de la fecha {fechaDesde.ToString("dd/MM/yyyy HH:mm")}");
            // Para evitar sobrecarga de memoria se toman grupos de registros en un rango de 30 días. Esto es fundamentalmente
            // para la importación inicial pues después las cargas se deben hacer diarias
            var fechaHasta = fechaDesde.AddDays(DiasCarga).Date.AddSeconds(-1);
            var esOk = true;
            while (fechaDesde.CompareTo(DateTime.Now) <= 0)
            {
                esOk = CargaFromOrigenToDestinoDet(logId, fechaDesde, fechaHasta) && esOk;
                fechaDesde = fechaHasta.AddSeconds(1);
                fechaHasta = fechaDesde.AddDays(DiasCarga + 1).Date.AddSeconds(-1);
            }

            if (!esOk)
            {
                logRepo.Add(new LogEtl()
                {
                    Tipo = "SESIONTABLADET-CARGA-ERROR",
                    Fecha = DateTime.Now,
                    Descripcion = "Ocurrió al menos un error durante la carga de datos, por favor, revisé el fichero log de error de la aplicación.",
                    ParentLogId = logId
                });
                resultado.Codigo = -1;
                resultado.Texto = "Ocurrió al menos un error durante la carga de datos.";
            }
            #endregion

            #region SP Carga desde tabla temporal a tabla de TB_SesionTablaDetalle

            var resultadoSProc = _repo.SpCopiaDatosDest(new { idCarga = logId }, "SpCopiaSesionTablaDet");
            if (resultadoSProc.Codigo < 0)
                esOk = false;
            #endregion

            logRepo.Add(new LogEtl()
            {
                Tipo = "SESIONTABLADET-CARGA-FIN" + (esOk ? "" : "-ERROR"),
                Fecha = DateTime.Now,
                Descripcion = "PROCESO DE CARGA FINALIZADO " + (esOk ? "" : "CON ERROR(ES)."),
                ParentLogId = logId
            });

            return resultado;

        }

        /// <summary>
        /// Carga desde la BD origen las sesiontabla detalle entre el rango de fechas especificados y los inserta en la tabla
        /// TB_SesionTablaDetalleCarga de la BD destino.
        /// </summary>
        /// <param name="idCarga">Id de carga</param>
        /// <param name="fechaD">Fecha desde para filtrar los datos a obtener</param>
        /// <param name="fechaH">Fecha hasta para filtrar los datos a obtener</param>
        /// <returns>true si no hubo error, false si hubo error</returns>
        private bool CargaFromOrigenToDestinoDet(int idCarga, DateTime fechaDesde, DateTime fechaHasta)
        {
            var esOk = true;
            var datos = _repo.GetDatosFromOrigen<SesionTablaDet>(Queries.SelectSesionTablaDetOrigen, new { fechaD = fechaDesde, fechaH = fechaHasta });
            if (datos == null)
            {
                logRepo.Add(new LogEtl()
                {
                    Tipo = "SESIONTABLADET-CARGA-ERROR-ORIG",
                    Fecha = DateTime.Now,
                    Descripcion = "Ocurrió un error al cargar los datos desde la BD de origen, por favor, revisé el fichero log de error de la aplicación.",
                    ParentLogId = idCarga
                });
                return false;
            }

            var datosInsertar = new List<SesionTablaDet>();
            foreach (var dato in datos)
            {
                dato.IdCarga = idCarga;
                datosInsertar.Add(dato);
                if (datosInsertar.Count >= 5000)
                {
                    // Se insertan de 5000 en 5000 registros, esto sólo debe ocurrir la primera vez q se ejecuta el servicio cuando no hay datos en la BD                    var InsertaEnBdTemp(datos);
                    var result = _repo.InsertaEnDestinoTemp(datosInsertar, Queries.InsertSesionTablaDetCarga);
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
                var result = _repo.InsertaEnDestinoTemp(datosInsertar, Queries.InsertSesionTablaDetCarga);
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

    }
}
