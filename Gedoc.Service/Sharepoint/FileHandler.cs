using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Logging;
using Microsoft.SharePoint.Client;
using File = System.IO.File;

namespace Gedoc.Service.Sharepoint
{
    public class FileHandler
    {

        /// <summary>
        /// Sube el archivo especificado al repositorio configurado en la aplicación.
        /// </summary>
        /// <param name="datosArchivo">Datos del archivo a subir, tiene la url donde subir, el nombre de archivo y además el
        /// Stream del archivo a subir</param>
        /// <returns>Ruta completa donde se subió el archivo, o vacío si hubo error en la operación.</returns>
        public string SubirArchivoRepositorio(DatosArchivo datosArchivo)
        {
            if (datosArchivo == null || /*.OrigenId == 0 ||*/ string.IsNullOrWhiteSpace(datosArchivo.OrigenCodigo) )
            {
                return "";
            }

            var filePath = new PathArchivo();
            try
            {
                filePath = GetPathArchivo(datosArchivo); // Se obtiene el path del archivo en el repositorio
                if (datosArchivo.renombraSiExiste)
                {
                    var resultRen = RenombraSiExiste(filePath.PathFull);
                }
                if (WebConfigValues.EsRepositorioLocal)
                {  // Subir a repositorio local
                    datosArchivo.File.SaveAs(filePath.PathFull);
                }
                else
                {  // Subir a repositorio Sharepoint
                    var sp = new SharePointClient();
                    var folderSp = string.IsNullOrWhiteSpace(filePath.PathRelative) 
                        ? ReturnSharePoint.CarpetaCreada 
                        : sp.CreateFolder3(filePath.BibliotecaSp, filePath.PathRelative);
                    if (folderSp == ReturnSharePoint.CarpetaCreada)
                    {
                        var fileName = string.IsNullOrWhiteSpace(datosArchivo.FileName) ? datosArchivo.File.FileName : datosArchivo.FileName;
                        filePath.PathFull = "/" + filePath.PathFull + "/" + fileName;
                        var resultado = sp.UploadFile(filePath.PathFull, datosArchivo.File.InputStream);
                        //var resultado = datosArchivo.TipoArchivo != TiposArchivo.Bitacora
                        //        ? sp.UploadFile(filePath.PathFull, datosArchivo.File.InputStream)
                        //        : sp.UploadFileToList(filePath.PathFull, datosArchivo.FileName, datosArchivo.File.InputStream, "Bitácora");
                        filePath.PathRelative = (resultado == ReturnSharePoint.ArchivoCreado) ? 
                            filePath.PathFull
                            : "";
                    }
                    else
                    {
                        filePath.PathRelative = "";
                        // eliminarFileTmp = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                filePath.PathRelative = "";
            }

            return filePath.PathRelative;
        }

        /// <summary>
        /// Devuelve el nombre del primer archivo en la carpeta especificada en Sharepoint
        /// </summary>
        /// <param name="folderUrl">Url de la carpeta</param>
        /// <returns>Nombre del primer archivo en la carpeta</returns>
        public string GetNombreArchivoEnRepoSp(string folderUrl)
        {
            var sp = new SharePointClient();
            sp.GetFileStreamLista("aaaa", folderUrl);
            return sp.FileNameInFolder(folderUrl);

        }

        /// <summary>
        /// Inidica si existe o no el archivo en el repositorio
        /// </summary>
        /// <param name="datosArchivo">Datos del archivo, por ej. el nombre de archivo, la url de descarga, el tipo de archivo (bitácora, adjunto o despacho)</param>
        /// <returns>true si existe; false si no existe</returns>
        public bool ExisteArchivoEnRepo(DatosArchivo datosArchivo)
        {
            var filePath = datosArchivo.FilePath;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                var datosArch = new DatosArchivo
                {
                    FileName = string.IsNullOrWhiteSpace(datosArchivo.FileName) ? (datosArchivo.File?.FileName ?? "") : datosArchivo.FileName,
                    ContentType = datosArchivo.ContentType,
                    FileTextContent = datosArchivo.FileTextContent,
                    Mensaje = datosArchivo.Mensaje,
                    OrigenCodigo = datosArchivo.OrigenCodigo,
                    OrigenId = datosArchivo.OrigenId,
                    TipoArchivo = datosArchivo.TipoArchivo
                };
                var path = GetPathArchivo(datosArch);
                filePath = path?.PathFull;
            }
            if (WebConfigValues.EsRepositorioLocal)
            {  // Es repositorio local
                return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath);
            }
            else
            {  // Es repositorio Sharepoint

                // TODO: implementar
            }

            return false;
        }

        /// <summary>
        /// Renombra un archivo si ya existe en el repositorio, para q se pueda subir un nuevo archivo con el mismo nombre
        /// </summary>
        /// <param name="filePath">Ubicación del archivo</param>
        /// <returns>true si se renombra con éxito, false en caso contrario</returns>
        public bool RenombraSiExiste(string filePath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    if (WebConfigValues.EsRepositorioLocal)
                    {  // Es repositorio local
                        var basePath = WebConfigValues.PathRepositorioLocal;
                        if (!filePath.StartsWith(basePath))
                        {
                            filePath = Path.Combine(basePath, filePath);
                        }
                        var fi = new FileInfo(filePath);
                        if (fi.Exists)
                        {
                            fi.MoveTo($"{filePath}_{DateTime.Now.ToString("yyyyMMdd_HHmm")}.bak");
                        }
                    }
                    else
                    {  // Es repositorio Sharepoint

                        // TODO: implementar
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            return false;
        }

        /// <summary>
        /// Devuelve el stream del archivo a descargar del repositorio.
        /// </summary>
        /// <param name="datosArchivo">Datos del archivo, por ej. el nombre de archivo, la url de descarga, el tipo de archivo (bitácora, adjunto o despacho)</param>
        public void GetFileStream(DatosArchivo datosArchivo)
        {
            if (WebConfigValues.EsRepositorioLocal)
            {  // Es repositorio local
                if (string.IsNullOrEmpty(datosArchivo.FilePath)) return;
                var filePath = datosArchivo.FilePath.Replace('/', '\\');
                filePath = filePath.StartsWith("\\") ? filePath.Remove(0, 1) : filePath;
                filePath = Path.Combine(WebConfigValues.PathRepositorioLocal, filePath);
                datosArchivo.FileStream = File.OpenRead(filePath);
            }
            else
            {  // Es repositorio Sharepoint
                var sp = new SharePointClient();
                var fs = (datosArchivo.TipoArchivo == TiposArchivo.Bitacora && string.IsNullOrWhiteSpace(datosArchivo.FileName))
                    ? sp.GetFileStreamLista("Bitácora", datosArchivo.FilePath)
                    : sp.GetFileStream(datosArchivo.FilePath);
                datosArchivo.FileStream = fs; // .FileStream;
                datosArchivo.Mensaje = fs == null ? "Ha ocurrido un error al descargar el archivo." : datosArchivo.Mensaje;
            }
        }

        /// <summary>
        /// Devuelve el camino, relativo y completo, del archivo cuando se va a subir al repositorio, en dependencia del tipo
        /// de archivo (bitáocra, adjunto o despahco) y del tipo de repositorio (local o Sharepoint)
        /// </summary>
        /// <param name="datosArchivo">Datos del archivo, por ej. el nombre de archivo, la url de descarga, el tipo de archivo (bitácora, adjunto o despacho)</param>
        /// <returns>El camino a donde subir el archivo</returns>
        private PathArchivo GetPathArchivo(DatosArchivo datosArchivo)
        {
            var pathArchivo = new PathArchivo();
            if (WebConfigValues.EsRepositorioLocal)
            {  // Es repositorio local
                var basePath = WebConfigValues.PathRepositorioLocal;

                switch (datosArchivo.TipoArchivo)
                {
                    case TiposArchivo.Adjunto:
                        pathArchivo.BibliotecaSp = "Adjuntos";
                        pathArchivo.PathRelative = $"{pathArchivo.BibliotecaSp}\\Adjuntos de {datosArchivo.OrigenCodigo}";
                        break;
                    case TiposArchivo.AdjuntoOficio:
                        pathArchivo.BibliotecaSp = "Adjuntos";
                        pathArchivo.PathRelative = $"{pathArchivo.BibliotecaSp}\\Oficios\\Adjuntos de {datosArchivo.OrigenCodigo}";
                        break;
                    case TiposArchivo.Bitacora:
                        pathArchivo.BibliotecaSp = "Lists\\Bitacora";
                        pathArchivo.PathRelative = $"{pathArchivo.BibliotecaSp}\\{datosArchivo.OrigenCodigo}";
                        break;
                    case TiposArchivo.Despacho:
                        pathArchivo.BibliotecaSp = "Despachos";
                        pathArchivo.PathRelative = $"{pathArchivo.BibliotecaSp}\\Despachos de {datosArchivo.OrigenCodigo}";
                        break;
                    case TiposArchivo.DespachoIniciativa:
                        pathArchivo.BibliotecaSp = "Despachos Iniciativas CMN";
                        pathArchivo.PathRelative = $"{pathArchivo.BibliotecaSp}\\{DateTime.Now.Year}";
                        //pathArchivo.BibliotecaSp = "";
                        //pathArchivo.PathRelative = "Despachos Iniciativas CMN";
                        break;
                    case TiposArchivo.Oficio:
                        pathArchivo.BibliotecaSp = "Despachos";
                        pathArchivo.PathRelative = $"{pathArchivo.BibliotecaSp}\\Oficios\\{DateTime.Now.Year}";
                        break;
                }

                pathArchivo.PathFull = Path.Combine(
                    basePath,
                    pathArchivo.PathRelative);
                if (!Directory.Exists(pathArchivo.PathFull)) Directory.CreateDirectory(pathArchivo.PathFull);

                var fileName = string.IsNullOrWhiteSpace(datosArchivo.FileName) ? datosArchivo.File.FileName : datosArchivo.FileName;
                fileName = LimpiaCaracteresUnicode(fileName);
                datosArchivo.FileName = fileName;
                pathArchivo.PathRelative = Path.Combine(pathArchivo.PathRelative, fileName);
                pathArchivo.PathFull = Path.Combine(pathArchivo.PathFull, fileName);
            }
            else
            {  // Es repositorio Sharepoint
                switch (datosArchivo.TipoArchivo)
                {
                    case TiposArchivo.Adjunto:
                        pathArchivo.BibliotecaSp = "Adjuntos";
                        pathArchivo.PathFull = $"{pathArchivo.BibliotecaSp}/Adjuntos de {datosArchivo.OrigenCodigo}";
                        pathArchivo.PathRelative = $"Adjuntos de {datosArchivo.OrigenCodigo}";
                        break;
                    case TiposArchivo.AdjuntoOficio:
                        pathArchivo.BibliotecaSp = "Adjuntos";
                        pathArchivo.PathFull = $"{pathArchivo.BibliotecaSp}/Oficios/Adjuntos de {datosArchivo.OrigenCodigo}";
                        pathArchivo.PathRelative = $"Oficios/Adjuntos de {datosArchivo.OrigenCodigo}";
                        break;
                    case TiposArchivo.Bitacora:
                        pathArchivo.BibliotecaSp = "Lists/Bitacora";
                        pathArchivo.PathFull = $"{pathArchivo.BibliotecaSp}/{datosArchivo.OrigenCodigo}";
                        pathArchivo.PathRelative = $"{datosArchivo.OrigenCodigo}";
                        break;
                    case TiposArchivo.Despacho:
                        pathArchivo.BibliotecaSp = "Despachos";
                        pathArchivo.PathFull = $"{pathArchivo.BibliotecaSp}/Despachos de {datosArchivo.OrigenCodigo}";
                        pathArchivo.PathRelative = $"Despachos de {datosArchivo.OrigenCodigo}";
                        break;
                    case TiposArchivo.DespachoIniciativa:
                        pathArchivo.BibliotecaSp = "Despachos Iniciativas CMN";
                        pathArchivo.PathFull = $"{pathArchivo.BibliotecaSp}/{DateTime.Now.Year}";
                        //pathArchivo.BibliotecaSp = "Despachos Iniciativas CMN";
                        //pathArchivo.PathFull = pathArchivo.BibliotecaSp;
                        pathArchivo.PathRelative = $"{DateTime.Now.Year}";
                        break;
                    case TiposArchivo.Oficio:
                        pathArchivo.BibliotecaSp = "Despachos";
                        pathArchivo.PathFull = $"{pathArchivo.BibliotecaSp}/Oficios/{DateTime.Now.Year}";
                        pathArchivo.PathRelative = $"Oficios/{DateTime.Now.Year}";
                        break;
                }
            }

            return pathArchivo;
        }

        /// <summary>
        /// Método para eliminar de una cadena los caracteres unicode de acento. Por ejemplo, en el siguiente texto
        /// "Propuesta Técnica.pdf" la e con tilde no corresponde al caracter é (unicode U+00E9) sino q es el caracter e
        /// seguido de ´ (unicode %u0301) sin embargo se visualiza de la misma manera q é. (para entender, posicionar el
        /// cursor después de é en la cadena "Propuesta Técnica.pdf" y presionar Backspace, se verá q desaparece la tilde de la e pero se conserva la
        /// letra e). Este comportamiento ocasiona al guardar un nombre de archivo en un campo de la base de datos entonces se separa
        /// la letra del acento (por ejemplo "Propuesta Técnica.pdf" se almacena como "Propuesta Te´cnica.pdf") y entonces al intentar luego acceder
        /// al archivo este no va a existir en el repositorio. 
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        private string LimpiaCaracteresUnicode(string valor)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(valor)) 
                    return Regex.Replace(valor, @"([^n\u0300-\u036f]|n(?!\u0303(?![\u0300-\u036f])))[\u0300-\u036f]+", String.Empty);
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
            }

            return valor;
        }

        public bool EliminarArchivo(string filePath)
        {
            var resultado = true;
            try
            {
                if (WebConfigValues.EsRepositorioLocal)
                {
                    // Es repositorio local
                    var basePath = WebConfigValues.PathRepositorioLocal;
                    if (!filePath.StartsWith(basePath))
                    {
                        filePath = Path.Combine(basePath, filePath);
                    }
                    System.IO.File.Delete(filePath);
                }
                else
                {
                    // Es repositorio Sharepoint
                    // TODO: implementar
                }
            }
            catch (DirectoryNotFoundException ex1)
            {
                // No se encuentra el archivo, no se hace nada
            }
            catch (Exception ex)
            {
                Logger.LogInfo("ADVERTENCIA. " + "No se pudo eliminar el archivo " + filePath + ", debe eliminarse manualmente para liberar espacio en disco.");
                Logger.LogError(ex);
                resultado = false;
            }

            return resultado;
        }

        private class PathArchivo
        {
            public string BibliotecaSp { get; set; }
            public string PathFull { get; set; }
            public string PathRelative { get; set; }
        }


    }
}
