using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Model;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.Service.Sharepoint;
using Microsoft.SharePoint.Client;

namespace Gedoc.Service.DataAccess
{
    public class BitacoraService : BaseService, IBitacoraService
    {
        private readonly IBitacoraRepositorio _repoBitacora;
        private readonly IMantenedorRepositorio _repoMant;
        private readonly IRequerimientoRepositorio _reqRepo;


        public BitacoraService(IBitacoraRepositorio repoBitacora, IMantenedorRepositorio repoMant,
            IDespachoRepositorio repoDespacho, IRequerimientoRepositorio reqRepo)
        {
            _repoBitacora = repoBitacora;
            _repoMant = repoMant;
            _reqRepo = reqRepo;
        }

        public DatosAjax<List<BitacoraDto>> GetBitacorasDoc(int idDoc, char tipoDoc)
        {
            var resultado = new DatosAjax<List<BitacoraDto>>(new List<BitacoraDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
            try
            {
                resultado = tipoDoc == 'R' 
                    ? _repoBitacora.GetBitacorasIngreso(idDoc)
                    : _repoBitacora.GetBitacorasDespachoInic(idDoc);
                if (!WebConfigValues.EsRepositorioLocal)
                {
                    foreach (var bitacora in resultado.Data)
                    {
                        if (!string.IsNullOrWhiteSpace(bitacora.UrlArchivo) &&
                            string.IsNullOrWhiteSpace(bitacora.NombreArchivo))
                        {
                            // Para las bitacoras existentes en Sharepoint, antes de migrarse a MVC, se tiene en los datos migrados
                            // la url de la carpeta del archivo de la bitácora pero no el nombre del archivo, y no todas las bitácoras
                            // tiene archivo y aún así tienen valor en la url de archivo. Aquí se chequea el repositorio de Sharepoint
                            // para saber si la carpeta de la bitácora tiene archivo adjunto, si no tiene entonces no se muestra 
                            // el link de descargar
                            var fileHandler = new FileHandler();
                            bitacora.NombreArchivo = fileHandler.GetNombreArchivoEnRepoSp(bitacora.UrlArchivo);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            return resultado;
        }

        public BitacoraDto GetBitacoraById(int id)
        {
            var datos = _repoBitacora.GetById(id);
            return datos;
        }

        public DatosAjax<BitacoraDto> GetUltimoComentarioByTipo(string TipoBitacoraCod, int reqId)
        {
            var resultado = new DatosAjax<BitacoraDto>(new BitacoraDto(), new ResultadoOperacion(1, "OK", null));
            try
            {
                var datos = _repoBitacora.GetUltimoComentarioByTipo(TipoBitacoraCod, reqId);
                resultado.Data = datos;
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            return resultado;
        }

        public ResultadoOperacion Save(BitacoraDto bitacora, IEnumerable<HttpPostedFileBase> files)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var logSistema = new LogSistemaDto()
                {
                    Flujo = bitacora.DespachoInicId.HasValue 
                        ? FlujoIngreso.BitacoraDespachoInic.ToString().ToUpper()
                        : FlujoIngreso.Bitacora.ToString().ToUpper(),
                    Fecha = DateTime.Now,
                    Origen = bitacora.DespachoInicId.HasValue ? "BITACORADESPACHOINIC" : "BITACORA",
                    OrigenFecha = bitacora.Fecha,
                    RequerimientoId = bitacora.RequerimientoId.HasValue 
                        ? bitacora.RequerimientoId.GetValueOrDefault(0)
                        : bitacora.DespachoInicId.GetValueOrDefault(0),
                    Usuario = bitacora.UsuarioActual ?? "<desconocido>",
                    UsuarioId = bitacora.UsuarioCreacionId
                };

                if (bitacora.Id == 0)
                {  // Nueva Bitácora
                    var datosArchivoAdj = new DatosArchivo
                    {
                        TipoArchivo = TiposArchivo.Bitacora,
                        OrigenCodigo = bitacora.NumeroDesp ?? bitacora.DocIngreso ?? "General"
                    };
                    bitacora.DatosArchivo = datosArchivoAdj;

                    var file = files != null && files.Count() > 0 ? files.First() : null;
                    // Delegate para subir el archivo adjunto
                    ProcesaArchivo procesaArch = (DatosArchivo datosArchivo, bool subirARepoGedoc, bool eliminar) =>
                    {
                        // Se guarda en repositorio el archivo
                        datosArchivo.File = file;
                        datosArchivo.FileName = file?.FileName;

                        var resultadoArch = new ResultadoOperacion(1, "OK", null);
                        if (datosArchivo.File != null)
                        {
                            var fileHandler = new FileHandler();
                            var filePath = fileHandler.SubirArchivoRepositorio(datosArchivo);

                            if (string.IsNullOrWhiteSpace(filePath))
                            { // ocurrió error al subir el archivo al repositorio
                                resultadoArch.Codigo = -1;
                                resultadoArch.Mensaje = "Ha ocurrido un error al subir el archivo al repositorio de Gedoc, <br/>por favor, revise el log de errores de la aplicación.";
                            }
                            else
                            {
                                resultadoArch.Extra = new[] { filePath, datosArchivo.FileName ?? datosArchivo.File.FileName };
                            }
                        };

                        return resultadoArch;
                    };

                    // Se graba en BD la nueva bitácora
                    var tipBit = _repoMant.GetTipoBitacoraById(bitacora.TipoBitacoraCod);
                    // var req = _reqRepo.GetById(bitacora.RequerimientoId.GetValueOrDefault(0));
                    if (bitacora.Fecha.GetValueOrDefault().TimeOfDay.TotalSeconds == 0)
                    {
                        bitacora.Fecha += new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    }
                    bitacora.TipoContenido = tipBit?.Titulo;
                    bitacora.Titulo = string.Format("{0}-{1}", tipBit?.Titulo ?? "", (bitacora.NumeroDesp ?? bitacora.DocIngreso ?? "") );
                    bitacora.UltimoComentario = bitacora.ObsAcuerdoComentario;
                    resultado = _repoBitacora.New(bitacora, procesaArch);

                    // Se actualiza el estado y etapa del requerimiento si corresponde
                    if (resultado.Codigo > 0 && bitacora.Requerimiento != null)
                    {
                        logSistema.EstadoId = bitacora.Requerimiento.EstadoId;
                        logSistema.EtapaId = bitacora.Requerimiento.EtapaId;
                        logSistema.UnidadTecnicaId = bitacora.Requerimiento.UtAsignadaId;
                    }

                    //// Se actualiza el estado y etapa del requerimiento si corresponde
                    //if (resultado.Codigo > 0 && bitacora.RequerimientoId.HasValue)
                    //{
                    //    var req = _reqRepo.UpdateByBitacora(bitacora.TipoBitacoraCod, bitacora.RequerimientoId.GetValueOrDefault(0));
                    //    logSistema.EstadoId = req?.EstadoId;
                    //    logSistema.EtapaId = req?.EtapaId;
                    //    logSistema.UnidadTecnicaId = req?.UtAsignadaId;
                    //}

                    //// Se guarda en repositorio el archivo de la bitácora si se especificó uno
                    //var file = files != null && files.Count() > 0 ? files.First() : null;
                    //var datosArchivo = new DatosArchivo();
                    //if (file != null)
                    //{
                    //    datosArchivo.TipoArchivo = TiposArchivo.Bitacora;
                    //    datosArchivo.File = file;
                    //    datosArchivo.OrigenCodigo = bitacora.NumeroDesp ?? bitacora.DocIngreso ?? "General";
                    //    datosArchivo.OrigenId = bitacora.Id;
                    //    datosArchivo.FileName = file.FileName;
                    //    var fileHandler = new FileHandler();
                    //    var filePath = fileHandler.SubirArchivoRepositorio(datosArchivo);

                    //    if (string.IsNullOrWhiteSpace(filePath))
                    //    { // ocurrió error al subir el archivo al repositorio
                    //        resultado.Codigo = -2;
                    //        // Se informa q hubo error subiendo el archivo y se conserva la bitácora creada
                    //        resultado.Mensaje = "¡Atención! Se ha creado la bitácora pero ocurrió un error al subir el archivo adjunto.";

                    //        // TODO: ahora si hay error al subir el archivo del despacho se deja el despacho y se informa q no se subió el archivo. Precisar si así o si eliminar el despacho.
                    //        //resultado.Mensaje = "Error al crear el despacho. Ocurrió un error al subir el archivo del despacho.";
                    //        // _repoDespacho.Eliminar<Despacho>(d => d.Id == despacho.Id);
                    //    }
                    //    else
                    //    {
                    //        _repoBitacora.UpdateDatosArchivo(bitacora.Id, file.FileName, filePath);
                    //        bitacora.UrlArchivo = filePath;
                    //        bitacora.NombreArchivo = file.FileName;
                    //    }
                    //}
                    resultado.Mensaje = resultado.Codigo < 0
                        ? resultado.Mensaje
                        : "Se ha creado satisfactoriamente la bitácora.";
                    resultado.Codigo = resultado.Codigo == -2 ? 1 : resultado.Codigo; // Si error cargando archivo entonces se ignora el error

                    logSistema.Accion = "CREACION_" + bitacora.TipoContenido.ToUpper() 
                                                    + (string.IsNullOrWhiteSpace(bitacora.NombreArchivo) ? "" : "_ADJ");
                }
                else
                {
                    // No hay edición de bitácora por ahora
                }

                if (resultado.Codigo > 0)
                {
                    logSistema.OrigenId = bitacora.Id;
                    logSistema.ExtraData = string.Format("{0}.{1} FECHA: {2}"
                        , bitacora.ObsAcuerdoComentario
                        , " ADJUNTO: " + bitacora.NombreArchivo
                        , bitacora.Fecha.Value.ToString(GeneralData.FORMATO_FECHA_CORTO));
                    _repoMant.SaveLogSistema(logSistema);
                    // TODO: enviar notificación según corresponda
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

        public DatosArchivo GetArchivo(int bitacoraId)
        {
            var result = new DatosArchivo
            {
                FileStream = null,
                Mensaje = "Error"
            };
            try
            {
                // Datos de la bitácora
                var bitacora = _repoBitacora.GetById(bitacoraId);
                if (bitacora == null)
                {
                    result.Mensaje = "No se encontró la Bitácora especificada.";
                    return result;
                }

                result.OrigenId = bitacoraId;
                result.OrigenCodigo = bitacora.DocIngreso;
                result.FileName = bitacora.NombreArchivo;
                result.FilePath = bitacora.UrlArchivo;
                result.TipoArchivo = TiposArchivo.Bitacora;
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

        public ResultadoOperacion EliminarBitacora(int id, int userId)
        {
            var result = new ResultadoOperacion();
            try
            {
                using (var db = new GedocEntities())
                {
                    var fordelete = db.Bitacora.FirstOrDefault(a => a.Id == id);
                    if (fordelete != null)
                    {
                        fordelete.EliminacionFecha = DateTime.Now;
                        fordelete.Eliminado = true;
                        fordelete.UsuarioEliminacionId = userId;                       
                        db.SaveChanges();
                        result.Codigo = 1;
                        result.Mensaje = "Bitácora Eliminada";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Codigo = -1;
                result.Mensaje = "Error al Eliminar Bitácora";
                result.Extra = ex.Message;
            }
            return result;
        }
    }
}
