using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.Service.Sharepoint;

namespace Gedoc.Service.DataAccess
{
    public class AdjuntoService : BaseService, IAdjuntoService
    {
        private readonly IAdjuntoRepositorio _repoAdjunto;
        private readonly IMantenedorRepositorio _repoMant;


        public AdjuntoService(IAdjuntoRepositorio repoAdjunto, IMantenedorRepositorio repoMant,
            IDespachoRepositorio repoDespacho)
        {
            _repoAdjunto = repoAdjunto;
            _repoMant = repoMant;
        }

        public DatosAjax<List<AdjuntoDto>> GetAdjuntosIngreso(int idIngreso)
        {
            var resultado = new DatosAjax<List<AdjuntoDto>>(new List<AdjuntoDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
            try
            {
                resultado = _repoAdjunto.GetAdjuntosIngreso(idIngreso);
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            return resultado;
        }

        public AdjuntoDto GetAdjuntoById(int id)
        {
            var datos = _repoAdjunto.GetById(id);
            return datos;
        }

        public DatosArchivo GetArchivo(int adjuntoId)
        {
            var result = new DatosArchivo
            {
                FileStream = null,
                Mensaje = "Error"
            };
            try
            {
                // Datos del adjunto
                var adjunto = _repoAdjunto.GetById(adjuntoId);
                if (adjunto == null)
                {
                    result.Mensaje = "No se encontró el Adjunto especificado.";
                    return result;
                }

                result.OrigenId = adjuntoId;
                result.OrigenCodigo = adjunto.DocIngreso;
                result.FileName = adjunto.NombreArchivo;
                result.FilePath = adjunto.UrlArchivo;
                result.TipoArchivo = TiposArchivo.Adjunto;
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

        public ResultadoOperacion Save(AdjuntoDto adjunto, IEnumerable<HttpPostedFileBase> files)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var logSistema = new LogSistemaDto()
                {
                    Flujo = "ADJUNTO",
                    Fecha = DateTime.Now,
                    Origen = "ADJUNTO",
                    RequerimientoId = adjunto.RequerimientoId.GetValueOrDefault(0),
                    Usuario = adjunto.UsuarioActual ?? "<desconocido>",
                    UsuarioId =adjunto.UsuarioCreacionId
                };

                if (adjunto.Id == 0)
                {  // Nuevo Adjunto
                    var file = files != null && files.Count() > 0 ? files.First() : null;
                    if (file != null)
                    {
                        var fileHandlerAdj = new FileHandler();
                        var datosArchivoAdj = new DatosArchivo();
                        datosArchivoAdj.TipoArchivo = TiposArchivo.Adjunto;
                        datosArchivoAdj.File = file;
                        datosArchivoAdj.OrigenCodigo = adjunto.DocIngreso;
                        datosArchivoAdj.OrigenId = adjunto.Id; // adjunto.RequerimientoId.GetValueOrDefault(0); //
                        adjunto.DatosArchivo = datosArchivoAdj;

                        // Se chequea q el documento adjunto no exista para el requerimiento
                        if (fileHandlerAdj.ExisteArchivoEnRepo(datosArchivoAdj))
                        {
                            // Si existe en el repositorio se chequea además si existe o no un adjunto en la base de datos con ese fichero asociado (para el ingreso en cuestión)
                            // pues pudiera darse el caso q se elimine un adjunto pero no se borre el fichero asociado, en ese caso sí se tiene q permitir crear el nuevo
                            // adjunto sobreescribiendose el fichero q existe en el repositorio
                            var adjuntosEnBd = adjunto.BandejaId == 7 
                                ? _repoAdjunto.GetAdjuntosOficio(adjunto.Id, true) 
                                : _repoAdjunto.GetAdjuntosIngreso(adjunto.RequerimientoId.GetValueOrDefault(0), true);
                            if (adjuntosEnBd.Data.Any(a =>
                                a.NombreArchivo.ToLower() == (datosArchivoAdj.File?.FileName ?? datosArchivoAdj.FileName ?? "").ToLower()))
                            {
                                resultado.Mensaje = "El título indicado para la carga de un nuevo adjunto ya existe. Por favor, utilizar otro nombre.";
                                return resultado;
                            }
                        }

                        // Delegate para subir el archivo adjunto
                        ProcesaArchivo procesaArch = (DatosArchivo datosArchivo, bool subirARepoGedoc, bool eliminar) =>
                        {
                            // Se guarda en repositorio el archivo
                            datosArchivo.File = file;
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

                        // Se graba en BD el nuevo adjunto
                        adjunto.FechaCarga = DateTime.Now;
                        adjunto.FileSize = file?.ContentLength;
                        adjunto.OrigenAdjunto = adjunto.BandejaId == 7 ? "Despacho" : "Requerimiento";
                        resultado = _repoAdjunto.New(adjunto, procesaArch);

                        //// Se sube el archivo al repositorio
                        //var filePath = fileHandler.SubirArchivoRepositorio(datosArchivo);

                        //if (string.IsNullOrWhiteSpace(filePath))
                        //{ // ocurrió error al subir el archivo al repositorio
                        //    resultado.Codigo = -2;
                        //    // Se informa q hubo error subiendo el archivo y se conserva el adjunto creado
                        //    resultado.Mensaje = "¡Atención! Se ha grabado la información del adjunto pero ocurrió un error al subir el archivo adjunto.";

                        //    // TODO: ahora si hay error al subir el archivo del despacho se deja el despacho y se informa q no se subió el archivo. Precisar si así o si eliminar el despacho.
                        //    //resultado.Mensaje = "Error al crear el despacho. Ocurrió un error al subir el archivo del despacho.";
                        //    // _repoDespacho.Eliminar<Despacho>(d => d.Id == despacho.Id);
                        //}
                        //else
                        //{
                        //    _repoAdjunto.UpdateDatosArchivo(adjunto.Id, file.FileName, filePath);
                        //}
                    }
                    else
                    {
                        resultado.Mensaje = "Tiene que especificar un archivo adjunto.";
                    }
                    resultado.Mensaje = resultado.Codigo < 0
                        ? resultado.Mensaje
                        : "Se ha creado satisfactoriamente el adjunto.";
                    resultado.Codigo = resultado.Codigo == -2 ? 1 : resultado.Codigo; // Si error cargando archivo entonces se ignora el error

                    logSistema.Accion = "CREACION";
                }
                else
                {
                    // No hay edición de adjunto por ahora
                }

                if (resultado.Codigo > 0)
                {
                    logSistema.OrigenId = adjunto.Id;
                    logSistema.EstadoId = null;
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

        public ResultadoOperacion MarcaAdjuntosEliminado(int[] adjIds, int usuarioId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                _repoAdjunto.MarcaAdjuntosEliminado(adjIds, usuarioId);
                resultado.Codigo = 1;
                resultado.Mensaje = "Adjuntos borrados con éxito.";
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
            }
            return resultado;
        }

        public DatosAjax<List<AdjuntoDto>> GetAdjuntosUsuario(DateTime fechaD, DateTime fechaH, int usuarioId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new DatosAjax<List<AdjuntoDto>>(new List<AdjuntoDto>(), 
                new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null));
            try
            {
                var datos = _repoAdjunto.GetAdjuntosUsuario(fechaD, fechaH, usuarioId);
                resultado.Data = datos;
                resultado.Total = datos.Count;
                resultado.Resultado.Codigo = 1;
                resultado.Resultado.Mensaje = "Adjuntos borrados con éxito.";
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Resultado.Mensaje);
            }
            return resultado;
        }

        #region Adjuntos de Oficio
        public DatosAjax<List<AdjuntoDto>> GetAdjuntosOficio(int idOficio)
        {
            var resultado = new DatosAjax<List<AdjuntoDto>>(new List<AdjuntoDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos.", null));
            try
            {
                resultado = _repoAdjunto.GetAdjuntosOficio(idOficio);
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            return resultado;
        }

        public AdjuntoDto GetAdjuntoOficioById(int id)
        {
            var datos = _repoAdjunto.GetAdjuntoOficioById(id);
            return datos;
        }

        public DatosArchivo GetArchivoOficio(int adjuntoId)
        {
            var result = new DatosArchivo
            {
                FileStream = null,
                Mensaje = "Error"
            };
            try
            {
                // Datos del adjunto
                var adjunto = _repoAdjunto.GetAdjuntoOficioById(adjuntoId);
                if (adjunto == null)
                {
                    result.Mensaje = "No se encontró el Adjunto especificado.";
                    return result;
                }

                result.OrigenId = adjuntoId;
                result.OrigenCodigo = adjunto.OficioId.ToString();
                result.FileName = adjunto.NombreArchivo;
                result.FilePath = adjunto.UrlArchivo;
                result.TipoArchivo = TiposArchivo.Adjunto;
                result.Mensaje = "OK";
                var fileHandler = new FileHandler();
                fileHandler.GetFileStream(result);
            }
            catch (Exception ex)
            {
                LogError(ex);
                result.OrigenId = -1;
                result.Mensaje = "Ha ocurrido un error al descargar el archivo, por favor chequee el log de errores de la aplicación.";
            }
            return result;
        }

        public ResultadoOperacion SaveAdjuntoOficio(AdjuntoDto adjunto, IEnumerable<HttpPostedFileBase> files)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var logSistema = new LogSistemaDto()
                {
                    Flujo = "ADJUNTO_OFICIO",
                    Fecha = DateTime.Now,
                    Origen = "ADJUNTO",
                    OrigenId = adjunto.OficioId.GetValueOrDefault(0),
                    Usuario = adjunto.UsuarioActual ?? "<desconocido>",
                    UsuarioId = adjunto.UsuarioCreacionId
                };

                if (adjunto.Id == 0)
                {  // Nuevo Adjunto
                    var file = files != null && files.Count() > 0 ? files.First() : null;
                    if (file != null)
                    {
                        var fileHandler = new FileHandler();
                        var datosArchivo = new DatosArchivo
                        {
                            TipoArchivo = TiposArchivo.AdjuntoOficio,
                            File = file,
                            OrigenCodigo = adjunto.OficioId.ToString(), 
                            OrigenId = adjunto.OficioId.GetValueOrDefault(0) // adjunto.Id
                        };
                        adjunto.DatosArchivo = datosArchivo;

                        // Se chequea q el documento adjunto no exista para el oficio
                        if (fileHandler.ExisteArchivoEnRepo(datosArchivo))
                        {
                            // Si existe en el repositorio se chequea además si existe o no un adjunto en la base de datos con ese fichero asociado (para el oficio en cuestión)
                            // pues pudiera darse el caso q se elimine un adjunto pero no se borre el fichero asociado, en ese caso sí se tiene q permitir crear el nuevo
                            // adjunto sobreescribiendose el fichero q existe en el repositorio
                            var adjuntosEnBd = _repoAdjunto.GetAdjuntosOficio(adjunto.OficioId.GetValueOrDefault(0), true);
                            if (adjuntosEnBd.Data.Any(a =>
                                a.NombreArchivo.ToLower() == (datosArchivo.File?.FileName ?? datosArchivo.FileName ?? "").ToLower()))
                            {
                                resultado.Mensaje = "El título indicado para la carga de un nuevo adjunto ya existe. Por favor, utilizar otro nombre.";
                                return resultado;
                            }
                        }

                        // Delegate para subir el archivo adjunto
                        ProcesaArchivo procesaAdjOfic = (DatosArchivo datosArchivoOfic, bool subirARepoGedoc, bool eliminar) =>
                        {
                            var resultadoArch = new ResultadoOperacion(1, "OK", null);
                            var filePath = fileHandler.SubirArchivoRepositorio(datosArchivoOfic);

                            if (string.IsNullOrWhiteSpace(filePath))
                            { // ocurrió error al subir el archivo al repositorio
                                resultadoArch.Codigo = -1;
                                resultadoArch.Mensaje = "Error subiendo el archivo.";
                            }
                            else
                            {
                                resultadoArch.Extra = new[] { filePath, datosArchivoOfic.FileName ?? datosArchivoOfic.File.FileName };
                            }

                            return resultadoArch;
                        };

                        // Se graba en BD el nuevo adjunto
                        adjunto.FechaCarga = DateTime.Now;
                        adjunto.FileSize = file?.ContentLength;
                        adjunto.OrigenAdjunto = "Oficio";
                        resultado = _repoAdjunto.NewAdjuntoOficio(adjunto, procesaAdjOfic);

                        //// Se sube el archivo al repositorio
                        //var filePath = fileHandler.SubirArchivoRepositorio(datosArchivo);

                        //if (string.IsNullOrWhiteSpace(filePath))
                        //{ // ocurrió error al subir el archivo al repositorio
                        //    resultado.Codigo = -2;
                        //    // Se informa q hubo error subiendo el archivo y se conserva el adjunto creado
                        //    resultado.Mensaje = "¡Atención! Se ha grabado la información del adjunto pero ocurrió un error al subir el archivo adjunto.";

                        //    // TODO: ahora si hay error al subir el archivo del despacho se deja el despacho y se informa q no se subió el archivo. Precisar si así o si eliminar el despacho.
                        //    //resultado.Mensaje = "Error al crear el adjunto. Ocurrió un error al subir el adjunto.";
                        //    // _repoDespacho.Eliminar<Despacho>(d => d.Id == despacho.Id);
                        //}
                        //else
                        //{
                        //    _repoAdjunto.UpdateDatosArchivoAdjuntoOficio(adjunto.Id, file.FileName, filePath);
                        //}
                    }
                    else
                    {
                        resultado.Mensaje = "Tiene que especificar un archivo adjunto.";
                    }
                    resultado.Mensaje = resultado.Codigo < 0
                        ? resultado.Mensaje
                        : "Se ha creado satisfactoriamente el adjunto.";
                    resultado.Codigo = resultado.Codigo == -2 ? 1 : resultado.Codigo; // Si error cargando archivo entonces se ignora el error

                    logSistema.Accion = "CREACION";
                }
                else
                {
                    // No hay edición de adjunto por ahora
                }

                logSistema.OrigenId = adjunto.Id;
                logSistema.EstadoId = null;
                _repoMant.SaveLogSistema(logSistema);
                // TODO: enviar notificación según corresponda
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

        public ResultadoOperacion MarcaAdjuntosOficioEliminado(int[] adjIds, int usuarioId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                _repoAdjunto.MarcaAdjuntosOficioEliminado(adjIds, usuarioId);
                resultado.Codigo = 1;
                resultado.Mensaje = "Adjuntos borrados con éxito.";
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
            }
            return resultado;
        }

        public DatosAjax<List<AdjuntoDto>> GetAdjuntosOficioUsuario(DateTime fechaD, DateTime fechaH, int usuarioId)
        {
            var errorId = Guid.NewGuid();
            var resultado = new DatosAjax<List<AdjuntoDto>>(new List<AdjuntoDto>(),
                new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null));
            try
            {
                var datos = _repoAdjunto.GetAdjuntosOficioUsuario(fechaD, fechaH, usuarioId);
                resultado.Data = datos;
                resultado.Resultado.Codigo = 1;
                resultado.Resultado.Mensaje = "Adjuntos borrados con éxito.";
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Resultado.Mensaje);
            }
            return resultado;
        }
        #endregion

    }
}
