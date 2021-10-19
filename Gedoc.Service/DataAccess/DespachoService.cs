using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Helpers.Logging;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Model;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.Service.EmailService;
using Gedoc.Service.Sharepoint;
using EstadoDespacho = Gedoc.Helpers.Enum.EstadoDespacho;

namespace Gedoc.Service.DataAccess
{
    public class DespachoService : BaseService, IDespachoService
    {

        private readonly IDespachoIniciativaRepositorio _repoDespInic;
        private readonly IDespachoRepositorio _repoDespacho;
        private readonly IMantenedorRepositorio _repoMant;
        private readonly IRequerimientoRepositorio _repoReq;
        private readonly INotificacionService _notifSrv;

        public DespachoService(IDespachoIniciativaRepositorio repoDespInic, IMantenedorRepositorio repoMant,
            IDespachoRepositorio repoDespacho,
            INotificacionService notificacionService,
            IRequerimientoRepositorio repoReq)
        {
            _repoDespInic = repoDespInic;
            _repoMant = repoMant;
            _repoDespacho = repoDespacho;
            _notifSrv = notificacionService;
            _repoReq = repoReq;
        }

        #region Despachos Iniciativas CMN
        public DatosAjax<List<DespachoIniciativaDto>> GetDatosBandejaEntradaInic(ParametrosGrillaDto<int> param, int idUsuario)
        {
            var resultado = new DatosAjax<List<DespachoIniciativaDto>>(new List<DespachoIniciativaDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos de la bandeja.", null));
            try
            {
                var idBandeja = param.Dato;
                var configBandeja = _repoMant.GetConfigBandeja(idBandeja, true);
                if (configBandeja == null)
                {
                    resultado.Resultado.Codigo = -1;
                    resultado.Resultado.Mensaje = "No se pueden mostrar los datos, no se encontró la configuración de la bandeja de entrada.";
                    return resultado;
                }

                // TODO: revisar si de acuerdo a los roles del usuario se aplican determinados filtros a los datos de la bandeja
                // TODO: además si el usuario no tiene dentro de los permisos acceder a la bandeja entonces devolver
                // error para evitar q se acceda especificando directamente la Url de la bandeja
                var take = param.Take > 0 ? param.Take : param.PageSize;
                var skip = param.Skip == 0 && param.Page > 0 ? ((param.Page - 1) * take) : param.Skip;
                var sort = param.Sort == null
                    ? new SortParam() { Field = "FechaEmisionOficio", Dir = "DESC" }
                    : new SortParam() { Field = param.Sort.Split('-')[0], Dir = param.Sort.Contains("-") ? param.Sort.Split('-')[1] : "DESC" };
                sort.Field = sort.Field == "EstadoDespachoTitulo" ? "EstadoTitulo" : sort.Field;
                sort.Field = sort.Field == "RemitenteNombre" ? "DestinatarioNombre" : sort.Field;
                var fechaDesde = DateTime.Now.AddDays(configBandeja.DiasAtras > 0 ? -1 * configBandeja.DiasAtras : configBandeja.DiasAtras);

                resultado = _repoDespInic.GetDatosBandejaEntrada(idBandeja, skip, take, sort, param.FilterText, fechaDesde);
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de la bandeja.");
            }
            return resultado;
        }

        public DespachoIniciativaDto GetDespachoInicById(int id)
        {
            var datos = id == 0 ? new DespachoIniciativaDto() { AdjuntaDocumentacion = false } : _repoDespInic.GetById(id);
            return datos;
        }

        public ResultadoOperacion SaveDespachoInic(DespachoIniciativaDto despacho, IEnumerable<HttpPostedFileBase> files)
        {
            var errorId = Guid.NewGuid();
            var resultadoEnvioEmail = new ResultadoOperacion();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var logSistema = new LogSistemaDto()
                {
                    Flujo = FlujoIngreso.DespachoInic.ToString().ToUpper(),
                    Fecha = DateTime.Now,
                    Origen = "DESPACHOINIC",
                    Usuario = despacho.UsuarioActual ?? "<desconocido>",
                    UsuarioId = despacho.UsuarioCreacionId
                };

                if (despacho.Id == 0)
                {  // Nuevo Despacho
                    var file = files != null && files.Count() > 0 ? files.First() : null;

                    if (!despacho.DesdeOficio)
                    { 
                        // Los siguientes valores ya fueron asignados cuando se genera el despacho desde oficio
                        despacho.FechaEmisionOficio = despacho.FechaEmisionOficio ?? DateTime.Now;
                        // Si se especificó el Número de despacho en el formulario se comprueba q se pueda utilizar
                        if (!string.IsNullOrWhiteSpace(despacho.NumeroDespacho))
                        {
                            var validaNumOf = _repoDespacho.ValidaNumeroOficio(despacho.NumeroDespacho);
                            if (validaNumOf.Codigo < 0)
                            {
                                return validaNumOf;
                            }
                            despacho.FolioDespacho = despacho.NumeroDespacho;
                        }
                    }
                    despacho.EstadoId = (int)EstadoDespacho.EnProceso;

                    // Datos del archivo adjunto al Despacho
                    despacho.DatosArchivo = despacho.DatosArchivo ?? new DatosArchivo();
                    despacho.DatosArchivo.TipoArchivo = TiposArchivo.DespachoIniciativa;
                    despacho.DatosArchivo.File = file;
                    // Delegate para subir el archivo adjunto
                    ProcesaArchivo procesaArch = (DatosArchivo datosArchivo, bool subirARepoGedoc, bool eliminar) =>
                    {
                        // Se guarda en repositorio el archivo del despacho
                        var resultadoArch = new ResultadoOperacion(1, "OK", null);
                        if (datosArchivo.File != null)
                        {
                            var fileHandler = new FileHandler();
                            var filePath = fileHandler.SubirArchivoRepositorio(datosArchivo);

                            if (string.IsNullOrWhiteSpace(filePath))
                            { // ocurrió error al subir el archivo al repositorio
                                resultadoArch.Codigo = -1;
                                resultadoArch.Mensaje = "Error subiendo el archivo.";
                            }
                            else
                            {
                                resultadoArch.Extra = new[] { filePath, datosArchivo.FileName ?? datosArchivo.File.FileName };
                            }
                        };

                        return resultadoArch;
                    };

                    // Se graba en BD el nuevo despacho
                    resultado = _repoDespInic.NewDespachoInic(despacho, procesaArch);

                    // Notificación email
                    despacho.Flujo = FlujoIngreso.DespachoInic;
                    if (resultado.Codigo > 0)
                        resultadoEnvioEmail = EnviarNotificacionEmail(despacho, null);

                    resultado.Mensaje = resultado.Codigo < 0 
                        ? resultado.Mensaje 
                        : $"Se ha creado satisfactoriamente el despacho. <br/>Número de Oficio: {despacho.NumeroDespacho}. <br/> {resultadoEnvioEmail.Mensaje} ";
                    resultado.Codigo = resultado.Codigo == -2 ? 1 : resultado.Codigo; // Si error cargando archivo entonces se ignora el error

                    logSistema.Accion = "CREACION";
                }
                else
                {
                    switch (despacho.Flujo)
                    {
                        case FlujoIngreso.DespachoInicEdicion:
                            // Si se modificó el Número de despacho en el formulario se comprueba q se pueda utilizar (esté en los correlativos reservaodos y sin utilizar)
                            if (!string.IsNullOrWhiteSpace(despacho.NumeroDespacho))
                            {
                                var despachoBd = _repoDespInic.GetById(despacho.Id);
                                if (despachoBd.NumeroDespacho != despacho.NumeroDespacho)
                                {
                                    var validaNumOf = ValidaNumeroOficio(despacho.NumeroDespacho);
                                    if (validaNumOf.Codigo < 0)
                                    {
                                        return validaNumOf;
                                    }
                                }
                            }
                            resultado = _repoDespInic.Update(despacho);
                            logSistema.Accion = "EDICION";

                            // Notificación email
                            despacho.Flujo = FlujoIngreso.DespachoInic;
                            if (resultado.Codigo > 0)
                                resultadoEnvioEmail = EnviarNotificacionEmail(despacho, null);
                            break;
                        case FlujoIngreso.DespachoInicCierre:
                            despacho.EstadoId = (int)EstadoDespacho.Despachado;
                            resultado = _repoDespInic.UpdateCierre(despacho);

                            // Notificación email
                            if (resultado.Codigo > 0)
                                resultadoEnvioEmail = EnviarNotificacionEmail(despacho, null);

                            resultado.Mensaje = $"Se ha cerrado satisfactoriamente el despacho. {resultadoEnvioEmail.Mensaje}";

                            // Log
                            logSistema.Accion = "CIERRE";
                            break;
                    }
                }

                // Log sistema
                if (resultado.Codigo > 0)
                {
                    logSistema.OrigenId = despacho.Id;
                    logSistema.EstadoId = null; // despacho.EstadoId;
                    _repoMant.SaveLogSistema(logSistema);
                }
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                if (resultado.Codigo > 0)
                {
                    resultado = new ResultadoOperacion(-1,
                        "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
                }
            }
            return resultado;
        }

        public ResultadoOperacion MarcaEliminadoDespInic(int despachoId, int usuarioId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                resultado = _repoDespInic.MarcaEliminado(despachoId, usuarioId);
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
            }
            return resultado;

        }

        public ResultadoOperacion EliminarDespInic(int despachoId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                resultado = _repoDespInic.EliminarDespInic(despachoId);
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
            }
            return resultado;

        }

        public DatosArchivo GetArchivoDespInic(int despachoId)
        {
            var result = new DatosArchivo
            {
                FileStream = null,
                Mensaje = "Error"
            };
            try
            {
                // Datos del despacho
                var despacho = _repoDespInic.GetById(despachoId);
                if (despacho == null)
                {
                    result.Mensaje = "No se encontró el Despacho especificado.";
                    return result;
                }

                result.OrigenId = despachoId;
                result.OrigenCodigo = despacho.NumeroDespacho;
                result.FileName = despacho.NombreArchivo;
                result.FilePath = despacho.UrlArchivo;
                result.TipoArchivo = TiposArchivo.Despacho;
                var fileHandler = new FileHandler();
                fileHandler.GetFileStream(result);
            }
            catch (Exception ex)
            {
                LogError(ex);
                result.Mensaje = "Ha ocurrido un error al descargar el archivo, por favor chequee el log de errores de la aplicación.";
            }
            return result;
        }
        #endregion

        #region Despachos
        public DatosAjax<List<DespachoDto>> GetDespachosUltimos(int diasAtras)
        {
            var resultado = new DatosAjax<List<DespachoDto>>(new List<DespachoDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
            try
            {
                resultado = _repoDespacho.GetDatosUltimos(diasAtras);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return resultado;
        }

        public DatosAjax<List<DespachoDto>> GetDespachosIngreso(ParametrosGrillaDto<int> param)
        {
            var resultado = new DatosAjax<List<DespachoDto>>(new List<DespachoDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
            try
            {
                var idIngreso = param.Dato;
                var take = param.Take > 0 ? param.Take : param.PageSize;
                var skip = param.Skip == 0 && param.Page > 0 ? ((param.Page - 1) * take) : param.Skip;
                var sort = param.Sort == null
                    ? new SortParam() { Field = "FechaEmisionOficio", Dir = "DESC" }
                    : new SortParam() { Field = param.Sort.Split('-')[0], Dir = param.Sort.Contains("-") ? param.Sort.Split('-')[1] : "DESC" };
                resultado = _repoDespacho.GetDespachosIngreso(idIngreso, skip, take, sort);
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            return resultado;
        }

        public DespachoDto GetDespachoById(int id)
        {
            var datos = id == 0 ? new DespachoDto { AdjuntaDocumentacion = false } : _repoDespacho.GetById(id);

            if (datos.Id > 0)
            {
                datos.Requerimiento = _repoDespacho.GetRequerimientosDespacho(datos.Id).Select(r => new GenericoDto() { IdInt = r.Id, Titulo = r.DocumentoIngreso}).ToList();
            }
            return datos;
        }

        public ResultadoOperacion Save(DespachoDto despacho, IEnumerable<HttpPostedFileBase> files)
        {
            var errorId = Guid.NewGuid();
            var resultadoEnvioEmail = new ResultadoOperacion();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var logSistema = new LogSistemaDto()
                {
                    Flujo = FlujoIngreso.Despacho.ToString().ToUpper(),
                    Fecha = DateTime.Now,
                    Origen = "DESPACHO",
                    Usuario = despacho.UsuarioActual ?? "<desconocido>",
                    UsuarioId = despacho.UsuarioCreacionId
                };

                despacho.RequerimientoPrincipalId = despacho.RequerimientoPrincipalId.GetValueOrDefault(0) > 0
                    ? despacho.RequerimientoPrincipalId
                    : (despacho.Requerimiento?.Count > 0 ? despacho.Requerimiento[0].IdInt : 0);
                if (despacho.Id == 0)
                {  // Nuevo Despacho
                    var file = files != null && files.Count() > 0 ? files.First() : null;

                    if (file == null)
                    {
                        // Se obliga a q el despacho tenga un adjunto. Esto se valida en el formulario, pero ocurrió con fecha 22/02/2021
                        // q se crearon unos despachos sin apenas datos (no tenían remitente, ni materia, ni archivo adjunto, etc)
                        // por eso se realiza la validación aquí.
                        Logger.LogInfo("ERROR: Error creando el despacho, es obligatorio especificar un archivo adjunto. Usuario creación: " + despacho.UsuarioCreacionId.GetValueOrDefault(0));
                        resultado.Codigo = -1;
                        resultado.Mensaje = "Error creando el despacho, <br/>es obligatorio especificar un archivo adjunto.";
                        return resultado;
                    }

                    if (!despacho.DesdeOficio)
                    { 
                        // Los siguientes valores ya fueron asignados cuando se genera el despacho desde oficio
                        despacho.FechaEmisionOficio = despacho.FechaEmisionOficio ?? DateTime.Now;
                        // Si se especificó el Número de despacho en el formulario se comprueba q se pueda utilizar
                        if (!string.IsNullOrWhiteSpace(despacho.NumeroDespacho))
                        {
                            // Se especificó el número de oficio en el formulario, se comprueba q esté reservado
                            var validaNumOf = _repoDespacho.ValidaNumeroOficio(despacho.NumeroDespacho);
                            if (validaNumOf.Codigo < 0)
                            {
                                return validaNumOf;
                            }
                            despacho.FolioDespacho = despacho.NumeroDespacho;
                        }
                    }
                    despacho.EstadoId = (int)EstadoDespacho.EnProceso;

                    // Datos del archivo adjunto al Despacho
                    despacho.DatosArchivo = despacho.DatosArchivo ?? new DatosArchivo();
                    despacho.DatosArchivo.TipoArchivo = TiposArchivo.Despacho;
                    despacho.DatosArchivo.File = file;
                    // Delegate para subir el archivo adjunto
                    ProcesaArchivo procesaArch = (DatosArchivo datosArchivo, bool subirARepoGedoc, bool eliminar) =>
                    {
                        // Se guarda en repositorio el archivo del despacho
                        var resultadoArch = new ResultadoOperacion(1, "OK", null);
                        if (datosArchivo.File != null)
                        {
                            var fileHandler = new FileHandler();
                            var filePath = fileHandler.SubirArchivoRepositorio(datosArchivo);

                            if (string.IsNullOrWhiteSpace(filePath))
                            { // ocurrió error al subir el archivo al repositorio
                                resultadoArch.Codigo = -1;
                                resultadoArch.Mensaje = "Ha ocurrido un error al subir el archivo del despacho al repositorio, <br/>por favor, revise el log de errores de la aplicación.";
                            }
                            else
                            {
                                resultadoArch.Extra = new[] { filePath, datosArchivo.FileName ?? datosArchivo.File.FileName };
                            }
                        };

                        return resultadoArch;
                    };

                    // Se graba en BD el nuevo despacho
                    resultado = _repoDespacho.NewDespacho(despacho, procesaArch);

                    // Notificación email
                    despacho.Flujo = FlujoIngreso.Despacho;
                    if (resultado.Codigo > 0)
                        resultadoEnvioEmail = EnviarNotificacionEmail(null, despacho);

                    resultado.Mensaje = resultado.Codigo < 0 
                        ? resultado.Mensaje 
                        : $"Se ha creado satisfactoriamente el despacho. <br/>Número de Oficio: {despacho.NumeroDespacho}. <br/>{resultadoEnvioEmail.Mensaje}";
                    resultado.Codigo = resultado.Codigo == -2 ? 1 : resultado.Codigo; // Si error cargando archivo entonces se ignora el error

                    logSistema.ExtraData =
                        $"SE RESPONDE CON OFICIO CMN {despacho.NumeroDespacho} CON FECHA DE EMISIÓN {despacho.FechaEmisionOficio.Value.ToString(GeneralData.FORMATO_FECHA_CORTO)}";
                    logSistema.Accion = "CREACION";
                    logSistema.OrigenFecha = despacho.FechaEmisionOficio;
                }
                else
                {
                    switch (despacho.Flujo)
                    {
                        case FlujoIngreso.DespachoEdicion:
                            // Si se modificó el Número de despacho en el formulario se comprueba q se pueda utilizar (esté en los correlativos reservaodos y sin utilizar)
                            if (!string.IsNullOrWhiteSpace(despacho.NumeroDespacho))
                            {
                                var despachoBd = _repoDespacho.GetById(despacho.Id);
                                if (despachoBd.NumeroDespacho != despacho.NumeroDespacho)
                                {
                                    var validaNumOf = ValidaNumeroOficio(despacho.NumeroDespacho);
                                    if (validaNumOf.Codigo < 0)
                                    {
                                        return validaNumOf;
                                    }
                                }
                            }
                            resultado = _repoDespacho.Update(despacho);
                            logSistema.Accion = "EDICION";
                            logSistema.OrigenFecha = despacho.FechaEmisionOficio;
                            break;
                        case FlujoIngreso.DespachoCierre:
                            despacho.EstadoId = (int)EstadoDespacho.Despachado;
                            resultado = _repoDespacho.UpdateCierre(despacho);

                            // Notificación email
                            resultadoEnvioEmail = EnviarNotificacionEmail(null, despacho);

                            resultado.Mensaje = resultado.Codigo < 0 
                                ? resultado.Mensaje 
                                : $"Se ha cerrado satisfactoriamente el despacho. <br/>{resultadoEnvioEmail.Mensaje}";
                            resultado.Codigo = resultado.Codigo == -2 ? 1 : resultado.Codigo; // Si error cargando archivo entonces se ignora el error

                            // Log
                            var medioDesp = _repoMant.GetGenericoMantenedor(Mantenedor.FormaLlegada, "", "")
                                .FirstOrDefault(d => d.Id == despacho.MedioDespachoCod);
                            var medioVerif = _repoMant.GetGenericoMantenedor(Mantenedor.MedioVerificacion, "", "")
                                .FirstOrDefault(d => d.Id == despacho.MedioVerificacionCod);
                            logSistema.ExtraData =
                                $"SE REGISTRA COMO MEDIO DE DESPACHO '{medioDesp?.Titulo ?? ""}' CON FECHA DE RECEPCIÓN {despacho.FechaEmisionOficio.Value.ToString(GeneralData.FORMATO_FECHA_CORTO)} Y COMO MEDIO DE VERIFICACIÓN '{medioVerif?.Titulo ?? ""}'";
                            logSistema.Accion = "CIERRE";
                            logSistema.OrigenFecha = despacho.FechaRecepcion;
                            break;
                    }
                }

                // Log de sistema
                var resultLog = new ResultadoOperacion(1, "OK", null);
                if (resultado.Codigo > 0)
                {
                    logSistema.OrigenId = despacho.Id;
                    // Se graba un log para cada requerimiento asociado al despacho
                    if (despacho.Requerimiento?.Count > 0) // Siempre debe ser true, pero por si acaso
                    {
                        var idsReq = despacho.Requerimiento.Select(r => r.IdInt).ToList();
                        var reqsDesp = _repoReq.GetRequerimientoByIds(idsReq, false);
                        var idProcMasivoLog = DateTime.Now.ToString("yyyyMMddHHmm");
                        foreach (var req in reqsDesp)
                        {
                            logSistema.Accion = despacho.EsProcesoMasivo ? "DESPACHO" : logSistema.Accion;
                            logSistema.Flujo = despacho.EsProcesoMasivo ? "PROCESOMASIVO_" + idProcMasivoLog : logSistema.Flujo;
                            logSistema.RequerimientoId = req.Id;
                            logSistema.EstadoId = req.EstadoId;
                            logSistema.EtapaId = req.EtapaId;
                            logSistema.UnidadTecnicaId = req.UtAsignadaId;
                            resultLog = _repoMant.SaveLogSistema(logSistema);
                        }
                    }
                    else
                    {
                        logSistema.RequerimientoId = 0;
                        logSistema.EstadoId = 0;
                        logSistema.EtapaId = 0;
                        resultLog = _repoMant.SaveLogSistema(logSistema);
                    }
                }

                resultado.Mensaje += resultLog.Codigo > 0 ? "" : " (Ha ocurrido un error al grabar el log de transacciones, por favor, revise el fichero log de errores.)";
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                if (resultado.Codigo > 0)
                {
                    resultado = new ResultadoOperacion(-1,
                        "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
                }
            }
            return resultado;
        }

        public ResultadoOperacion ValidaNumeroOficio(string numeroOficio)
        {
            return _repoDespacho.ValidaNumeroOficio(numeroOficio);
        }

        public DatosArchivo GetArchivo(int despachoId)
        {
            var result = new DatosArchivo
            {
                FileStream = null,
                Mensaje = "Error"
            };
            try
            {
                // Datos del despacho
                var despacho = _repoDespacho.GetById(despachoId);
                if (despacho == null)
                {
                    result.Mensaje = "No se encontró el Despacho especificado.";
                    return result;
                }

                result.OrigenId = despachoId;
                result.OrigenCodigo = despacho.NumeroDespacho;
                result.FileName = despacho.NombreArchivo;
                result.FilePath = despacho.UrlArchivo;
                result.TipoArchivo = TiposArchivo.Despacho;
                var fileHandler = new FileHandler();
                fileHandler.GetFileStream(result);
            }
            catch (Exception ex)
            {
                LogError(ex);
                result.Mensaje = "Ha ocurrido un error al descargar el archivo, por favor chequee el log de errores de la aplicación.";
            }
            return result;
        }

        public ResultadoOperacion EnviarNotificacionEmail(DespachoIniciativaDto despachoInic, DespachoDto despacho)
        {
            var resultadoEnvioEmail = new ResultadoOperacion(1, "", null);
            var resultadoEnvioEmail2 = new ResultadoOperacion(1, "", null);
            try
            {
                #region Notificaciones Despachos Iniciativas
                switch (despachoInic?.Flujo)
                {
                    case FlujoIngreso.DespachoInic:
                        _repoDespInic.GetDespachoDetalle(despachoInic); // Obtener de Bd los datos necesarios para mostrar en el email de notificación
                        // Notificación a los responsables de la UT
                        resultadoEnvioEmail = _notifSrv.NotificacionDespachoInicNuevo(despachoInic);
                        // Notificación a destinatarios en copia
                        if (despachoInic.EnviarNotificacion)
                        {
                            resultadoEnvioEmail2 = _notifSrv.NotificacionDestCopiaDespachoinicNuevo(despachoInic);
                        }

                        resultadoEnvioEmail.Mensaje = ""; /* resultadoEnvioEmail.Codigo == -2 ? " (" + resultadoEnvioEmail.Texto + ")" : (
                         resultadoEnvioEmail.Codigo < 0 ? " (Ha ocurrido un error al enviar el email de notificación)" : ""); */
                        resultadoEnvioEmail.Mensaje = resultadoEnvioEmail2.Codigo == -2
                            ? " (Notificación a destinatarios: " + resultadoEnvioEmail2.Mensaje + ")"
                            : (resultadoEnvioEmail2.Codigo < 0
                                ? " (Ha ocurrido un error al enviar el email de notificación a destinatarios)"
                                : "");
                        break;
                    case FlujoIngreso.DespachoInicEdicion:
                        break;
                    case FlujoIngreso.DespachoInicCierre:
                        // Notificación Cierre Despacho Iniciativas
                        if (despachoInic.EnviarNotificacion)
                        {
                            _repoDespInic.GetDespachoDetalle(despachoInic); // Obtener de Bd los datos necesarios para mostrar en el email de notificación
                            // Notificación a los responsables de la UT
                            resultadoEnvioEmail = _notifSrv.NotificacionDespachoInicCierre(despachoInic);
                            // Notificación a destinatarios en copia
                            resultadoEnvioEmail2 = _notifSrv.NotificacionDestCopiaDespachoInicCierre(despachoInic);

                            resultadoEnvioEmail.Mensaje =
                                resultadoEnvioEmail.Codigo == -2
                                    ? "(" + resultadoEnvioEmail.Mensaje + ")"
                                    : (resultadoEnvioEmail.Codigo < 0
                                        ? "(Ha ocurrido un error al enviar el email de notificación)"
                                        : "");
                        }
                        break;
                }
                #endregion

                #region Notificaciones Despachos
                switch (despacho?.Flujo)
                {
                    case FlujoIngreso.Despacho:
                        _repoDespacho.GetDespachoDetalle(despacho); // Obtener de Bd los datos necesarios para mostrar en el email de notificación
                        // Notificación a responsables de la UT
                        resultadoEnvioEmail = _notifSrv.NotificacionDespachoNuevo(despacho);
                        // Notificación a destinatarios en copia
                        if (despacho.EnviarNotificacion)
                        {
                            resultadoEnvioEmail2 = _notifSrv.NotificacionDestCopiaDespachoNuevo(despacho);
                        }

                        resultadoEnvioEmail.Mensaje = resultadoEnvioEmail.Codigo == -2
                            ? " (" + resultadoEnvioEmail.Mensaje + ")"
                            : (resultadoEnvioEmail.Codigo < 0
                                ? " (Ha ocurrido un error al enviar el email de notificación)"
                                : "");
                        resultadoEnvioEmail2.Mensaje = resultadoEnvioEmail2.Codigo == -2
                            ? " (Notificación a destinatarios: " + resultadoEnvioEmail2.Mensaje + ")"
                            : (resultadoEnvioEmail2.Codigo < 0
                                ? " (Ha ocurrido un error al enviar el email de notificación a destinatarios)"
                                : "");
                        resultadoEnvioEmail.Mensaje += resultadoEnvioEmail2.Mensaje;
                        break;
                    case FlujoIngreso.DespachoEdicion:
                        // Notificación email
                        despacho.Flujo = FlujoIngreso.Despacho;
                        resultadoEnvioEmail = EnviarNotificacionEmail(null, despacho);
                        break;
                    case FlujoIngreso.DespachoCierre:
                        // Notificación Cierre de Despacho
                        if (despacho.EnviarNotificacion)
                        {
                            _repoDespacho.GetDespachoDetalle(despacho); // Obtener de Bd los datos necesarios para mostrar en el email de notificación
                            // Notificación a los responsables de la UT
                            resultadoEnvioEmail = _notifSrv.NotificacionDespachoCierre(despacho);
                            // Notificación a destinatarios en copia
                            resultadoEnvioEmail2 = _notifSrv.NotificacionDestCopiaDespachoCierre(despacho);

                            resultadoEnvioEmail.Mensaje =
                                resultadoEnvioEmail.Codigo == -2
                                    ? "(" + resultadoEnvioEmail.Mensaje + ")"
                                    : (resultadoEnvioEmail.Codigo < 0
                                        ? "(Ha ocurrido un error al enviar el email de notificación)"
                                        : "");
                        }
                        break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                resultadoEnvioEmail.Codigo = -1;
                resultadoEnvioEmail.Mensaje = "(Ha ocurrido un error al enviar el email de notificación)";
            }

            return resultadoEnvioEmail;
        }

        public ResultadoOperacion MarcaEliminadoDesp(int despachoId, int usuarioId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                resultado = _repoDespacho.MarcaEliminado(despachoId, usuarioId);
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
            }
            return resultado;

        }

        public ResultadoOperacion EliminarDespp(int despachoId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                resultado = _repoDespInic.EliminarDespInic(despachoId);
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
            }
            return resultado;

        }
        #endregion



        #region Vistas

        public DatosAjax<List<DespachoDto>> GetDatosVistas(ParametrosGrillaDto<int> param)
        {
            var resultado = new DatosAjax<List<DespachoDto>>(new List<DespachoDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
            try
            {
                var idVista = param.Dato;
                var take = param.Take > 0 ? param.Take : param.PageSize;
                var skip = param.Skip == 0 && param.Page > 0 ? ((param.Page - 1) * take) : param.Skip;
                var sort = param.Sort == null
                    ? new SortParam() { Field = "FechaIngreso", Dir = "DESC" }
                    : new SortParam() { Field = param.Sort.Split('-')[0], Dir = param.Sort.Contains("-") ? param.Sort.Split('-')[1] : "DESC" };

                switch (idVista)
                {
                    case 4: // Despachos por destinatarios
                        resultado = _repoDespacho.GetDatosVistaDestinatario(skip, take, sort, param.FilterText);
                        break;
                    default:
                        resultado.Resultado.Mensaje = "Id de vista incorrecto.";
                        break;
                }
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            return resultado;
        }
        #endregion


    }
}
