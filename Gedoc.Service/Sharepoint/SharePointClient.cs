using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.UI.WebControls;
using System.Xml;
using Gedoc.Helpers;
using Gedoc.Helpers.Logging;
using Microsoft.SharePoint.Client;
using ListItemCollection = Microsoft.SharePoint.Client.ListItemCollection;

namespace Gedoc.Service.Sharepoint
{
    public class SharePointClient : ISharePointClient
    {

        public string SP_URL { get; set; }
        public string SP_User { get; set; }
        public string SP_Pass { get; set; }
        public string SP_Domain { get; set; }
        public string SP_Biblioteca { get; set; }
        const int SpTimeOut = 10 * 60 * 1000;

        public SharePointClient()
        {
            SP_URL = WebConfigValues.SP_URL;
            SP_User = WebConfigValues.SP_User;
            SP_Pass = WebConfigValues.SP_Pass;
            SP_Domain = WebConfigValues.SP_Domain;
            SP_Biblioteca = WebConfigValues.SP_Biblioteca;
        }

        public NetworkCredential GetNetworkCredential()
        {
            try
            {
                //CredentialCache.DefaultNetworkCredentials;
                NetworkCredential networkCredential = new NetworkCredential(SP_Domain + "\\" + SP_User, SP_Pass);
                return networkCredential;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        public ReturnSharePoint DeleteFile()
        {
            try
            {
                return ReturnSharePoint.ArchivoEliminado;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return ReturnSharePoint.Error;
            }
        }

        public ReturnSharePoint UploadFile(string fullFilePathSp, Stream fileStream)
        {
            var resultado = ReturnSharePoint.ArchivoCreado;
            try
            {
                
                using (var context = new ClientContext(SP_URL))
                {
                    ConfigContext(context);
                    context.Credentials = GetNetworkCredential();

                    // Se hacen 3 intentos de subida de archivo por si hay error con una pausa de 3 seg entre intentos
                    for (var intento = 1; intento <= 3; intento++)
                    {
                        try
                        {
                            Microsoft.SharePoint.Client.File.SaveBinaryDirect(context, fullFilePathSp, fileStream, true);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogInfo("ERROR subiendo archivo a repositorio Sharepoint. Intento " + intento + ".");
                            Logger.LogError(ex);
                            if (intento < 3)
                            {
                                Thread.Sleep(3000);
                            }
                            else
                            {
                                resultado = ReturnSharePoint.Error;
                            }
                            if (context.HasPendingRequest)
                                context.ExecuteQuery();
                        }
                    }

                    //var web = context.Web;
                    //var f = web.GetFileByServerRelativeUrl(fullFilePathSp);
                    //var item = f.ListItemAllFields;

                    //context.Load(item, i => i.Id);
                    //context.ExecuteQuery();

                }
            }
            catch (Exception ex)
            {
                resultado = ReturnSharePoint.Error;
                Logger.LogError(ex);
            }

            return resultado;
        }

        public ReturnSharePoint UploadFileToList(string fullFilePathSp, string fileName, Stream fileStream, string lista)
        {
            try
            {
                using (var context = new ClientContext(SP_URL))
                {
                    ConfigContext(context);
                    context.Credentials = GetNetworkCredential();

                    var web = context.Web;

                    byte[] fileArr;
                    using (var memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        fileArr = memoryStream.ToArray();
                    }

                    var esOk = false;
                    // Se hacen 3 intentos de subida de archivo por si hay error con una pausa de 3 seg entre intentos
                    for (var intento = 1; intento <= 3; intento++)
                    {
                        try
                        {
                            FileCreationInformation newFile = new FileCreationInformation();
                            newFile.Content = fileArr; // System.IO.File.ReadAllBytes("document.pdf");

                            //file url is name
                            newFile.Url = fileName;
                            List docs = web.Lists.GetByTitle(lista);

                            //get folder and add to that
                            Folder folder = docs.RootFolder; // .Folders.GetByUrl("demo");
                            Microsoft.SharePoint.Client.File uploadFile = folder.Files.Add(newFile);

                            context.Load(docs);
                            context.Load(uploadFile);
                            context.ExecuteQuery();

                            esOk = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogInfo("ERROR subiendo archivo a repositorio Sharepoint. Intento " + intento + ".");
                            Logger.LogError(ex);
                            if (intento < 3)
                            {
                                Thread.Sleep(3000);
                            }
                            if (context.HasPendingRequest)
                                context.ExecuteQuery();
                        }
                    }

                    return esOk ? ReturnSharePoint.ArchivoCreado : ReturnSharePoint.Error;

                };

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return ReturnSharePoint.Error;
            }
        }

        public bool CheckFolderExists(string biblioteca, string folder)
        {
            try
            {

                using (var clientContext = new ClientContext(SP_URL))
                {
                    clientContext.Credentials = GetNetworkCredential();


                    Folder f = clientContext.Web.GetFolderByServerRelativeUrl(biblioteca + "/" + folder);
                    clientContext.Load(f);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return true;
            }
        }

        public ReturnSharePoint AddItemLibrary(string localFilePath, string folderNameSp, string fileRelativeUrl)
        {
            string newLocation = string.Empty;
            var resultado = ReturnSharePoint.ArchivoCreado; ;
            try
            {
                using (var clientContext = new ClientContext(SP_URL))
                {
                    clientContext.Credentials = GetNetworkCredential();

                    var folders = clientContext.Web.Folders;
                    clientContext.Load(folders);
                    var result = clientContext.LoadQuery(folders.Where(f => f.Name == folderNameSp));
                    clientContext.ExecuteQuery();
                    var folder = result.FirstOrDefault();

                    var fileInfo = new FileCreationInformation();
                    var localFileBin = System.IO.File.ReadAllBytes(localFilePath);
                    fileInfo.Content = localFileBin;
                    fileInfo.Url = fileRelativeUrl;

                    var newFile = folder.Files.Add(fileInfo);
                    var newListItem = newFile.ListItemAllFields;
                    clientContext.Load(newFile);
                    clientContext.Load(newListItem);
                    clientContext.ExecuteQuery();

                    newListItem.Update();
                    clientContext.Load(newListItem);
                    clientContext.ExecuteQuery();
                    var itemId = newListItem.Id.ToString();
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                fileRelativeUrl = fileRelativeUrl.StartsWith("/") ? fileRelativeUrl.Remove(0, 1) : fileRelativeUrl;
                if (exc is Microsoft.SharePoint.Client.ServerException &&
                   (((Microsoft.SharePoint.Client.ServerException)(exc)).ServerErrorTypeName == "Microsoft.SharePoint.SPDuplicateValuesFoundException"
                    //  || exc is Microsoft.SharePoint.SPDuplicateValuesFoundException
                   || exc.Message.Contains("El archivo " + fileRelativeUrl + " ya existe")))
                {
                    resultado = ReturnSharePoint.ArchivoYaExiste;
                }
                else
                {
                    resultado = ReturnSharePoint.Error;
                }
            }
            return resultado;
        }

        #region Descarga de archivo

        public Stream GetFileStream_OLD(string fileRef)
        {
            if (string.IsNullOrWhiteSpace(fileRef))
                return null;

            using (ClientContext clientContext = new ClientContext(SP_URL))
            {
                clientContext.Credentials = GetNetworkCredential();

                var fileInformation =
                    Microsoft.SharePoint.Client.File.OpenBinaryDirect(clientContext, fileRef);
                var memoryStream = new MemoryStream();
                {
                    byte[] buffer = new byte[32768];
                    int bytesRead;
                    do
                    {
                        bytesRead = fileInformation.Stream.Read(buffer, 0, buffer.Length);
                        memoryStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                    // return buffer;
                    return memoryStream;
                }
            }
        }

        public Stream GetFileStream(string fileRelativePath)
        {
            try
            {
                ClientContext context = new ClientContext(SP_URL);
                context.Credentials = GetNetworkCredential();

                FileInformation f = Microsoft.SharePoint.Client.File.OpenBinaryDirect(context, fileRelativePath);
                
                return f.Stream;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }


        }

        public Stream GetFileStreamLista(string lista, string fileUrl)
        {
            var attachmentName = "";
            if (string.IsNullOrWhiteSpace(lista) || string.IsNullOrWhiteSpace(fileUrl))
                return null;

            try
            {
                using (ClientContext clientContext = new ClientContext(SP_URL))
                {
                    clientContext.Credentials = GetNetworkCredential();
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();

                    Folder attachmentsFolder = TryGetFolderByServerRelativeUrl(fileUrl); // web.GetFolderByServerRelativeUrl(src);
                    if (attachmentsFolder == null)
                    {
                        return null;
                    }
                    //clientContext.Load(attachmentsFolder);  //************************
                    FileCollection attachments = attachmentsFolder.Files;
                    clientContext.Load(attachments);
                    clientContext.Load(attachments, fs => fs.Include(f => f.ServerRelativeUrl));
                    clientContext.ExecuteQuery();

                    if (attachments.Count > 0)
                    {
                        var attachment = attachments[0];
                        attachmentName = attachment.Name;
                        FileInformation fileInformation =
                            Microsoft.SharePoint.Client.File.OpenBinaryDirect(clientContext, attachment.ServerRelativeUrl);
                        var memoryStream = new MemoryStream();
                        byte[] buffer = new byte[32768];
                        int bytesRead;
                        do
                        {
                            bytesRead = fileInformation.Stream.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead != 0);
                        return memoryStream;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (ServerException ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// Devuelve el nombre del primer archivo en la carpeta especificada en Sharepoint
        /// </summary>
        /// <param name="folderUrl">Url de la carpeta</param>
        /// <returns>Nombre del primer archivo en la carpeta</returns>
        public string FileNameInFolder(string folderUrl)
        {
            var fileName = "";
            if (string.IsNullOrWhiteSpace(folderUrl))
                return "";

            try
            {
                using (ClientContext clientContext = new ClientContext(SP_URL))
                {
                    clientContext.Credentials = GetNetworkCredential();
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();

                    Folder attachmentsFolder = TryGetFolderByServerRelativeUrl(folderUrl); // web.GetFolderByServerRelativeUrl(folderUrl);  // 
                    if (attachmentsFolder == null)
                    {
                        return "";
                    }
                    //clientContext.Load(attachmentsFolder);
                    FileCollection attachments = attachmentsFolder.Files;
                    clientContext.Load(attachmentsFolder);
                    clientContext.Load(attachments);
                    clientContext.Load(attachments, fs => fs.Include(f => f.ServerRelativeUrl));
                    clientContext.ExecuteQuery();

                    if (attachments.Count > 0)
                    {
                        var attachment = attachments[0];
                        fileName = attachment.Name;
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            catch (ServerException ex)
            {
                Logger.LogError(ex);
                return "";
            }

            return fileName;
        }

        public Stream GetFileStreamLista(string lista, string itemId, string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(lista) || string.IsNullOrWhiteSpace(fileUrl))
                return null;

            try
            {
                using (ClientContext clientContext = new ClientContext(SP_URL))
                {
                    clientContext.Credentials = GetNetworkCredential();
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();

                    var list = clientContext.Web.Lists.GetByTitle(lista);
                    var listItem = list.GetItemById(itemId);
                    clientContext.Load(list);
                    clientContext.Load(listItem, i => i.File);
                    clientContext.ExecuteQuery();

                    var fileRef = listItem.File.ServerRelativeUrl;
                    var fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(clientContext, fileRef);
                    var memoryStream = new MemoryStream();
                    fileInfo.Stream.CopyTo(memoryStream);
                    return memoryStream;
                }
            }
            catch (ServerException ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public Folder TryGetFolderByServerRelativeUrl(string serverRelativeUrl)
        {
            using (ClientContext clientContext = new ClientContext(SP_URL))
            {
                clientContext.Credentials = GetNetworkCredential();
                try
                {
                    var folder = clientContext.Web.GetFolderByServerRelativeUrl(serverRelativeUrl);
                    clientContext.Load(folder);
                    clientContext.Load(folder.Files);
                    clientContext.ExecuteQuery();
                    foreach (var item in folder.Files)
                    {
                        clientContext.Load(item);
                        clientContext.Load(item, i => i.ListItemAllFields);
                        clientContext.ExecuteQuery();
                    }
                    return folder;
                }
                catch (ServerException ex)
                {
                    Logger.LogError(ex);
                    return null;
                }
            }
        }

        #endregion


        #region Private Error Read or Dws result

        private bool IsDwsErrorResult(string ResultFragment)
        {
            var srResult = new System.IO.StringReader(ResultFragment);
            var xtr = new System.Xml.XmlTextReader(srResult);

            xtr.Read();

            return xtr.Name == "Error";
        }

        private void ParseDwsErrorResult(string ErrorFragment, out int ErrorID, out string ErrorMsg)
        {
            var srError = new System.IO.StringReader(ErrorFragment);
            var xtr = new System.Xml.XmlTextReader(srError);
            xtr.Read();

            xtr.MoveToAttribute("ID");
            xtr.ReadAttributeValue();

            ErrorID = System.Convert.ToInt32(xtr.Value);
            ErrorMsg = xtr.ReadString();
        }

        private string ParseDwsSingleResult(string ResultFragment)
        {
            var srResult = new System.IO.StringReader(ResultFragment);
            var xtr = new System.Xml.XmlTextReader(srResult);
            xtr.Read();

            return xtr.ReadString();
        }

        #endregion

        
        public static XmlElement GetXmlElement(System.Xml.Linq.XElement element)
        {
            using (var xmlReader = element.CreateReader())
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc.FirstChild as XmlElement;
            }
        }

        public ReturnSharePoint CreateFolder(string biblioteca, string nombreFolder)
        {
            using (var clientContext = new ClientContext(SP_URL))
            {
                clientContext.Credentials = GetNetworkCredential();
                try
                {
                    var folderPadre = clientContext.Web.GetFolderByServerRelativeUrl(biblioteca);
                    var folderNew = folderPadre.Folders.Add(nombreFolder);
                    folderPadre.Context.ExecuteQuery();
                    return ReturnSharePoint.CarpetaCreada;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return ReturnSharePoint.Error;
                }
            }
        }

        public ReturnSharePoint CreateFolder2(string biblioteca, string nombreFolder)
        {
            using (var clientContext = new ClientContext(SP_URL))
            {
                clientContext.Credentials = GetNetworkCredential();
                try
                {
                    var listaLib = clientContext.Web.Lists.GetByTitle(biblioteca);
                    var folders = listaLib.RootFolder.Folders;
                    clientContext.Load(folders);
                    clientContext.ExecuteQuery();

                    var nombreFolders = folders.Select(f => f.Name).ToList();
                    if (nombreFolders.Contains(nombreFolder))
                    {
                        return ReturnSharePoint.CarpetaYaExiste;
                    }
                    else
                    {
                        var folderNew = listaLib.RootFolder.Folders.Add(nombreFolder);
                        clientContext.ExecuteQuery();
                    }
                    return ReturnSharePoint.CarpetaCreada;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return ReturnSharePoint.Error;
                }
            }
        }

        public ReturnSharePoint CreateFolder3(string biblioteca, string nombreFolder)
        {
            using (var clientContext = new ClientContext(SP_URL))
            {
                clientContext.Credentials = GetNetworkCredential();
                try
                {
                    var folderPadre = clientContext.Web.GetFolderByServerRelativeUrl(biblioteca);
                    //var folderPadre = clientContext.Web.Lists.GetByTitle(biblioteca).RootFolder;
                    var curFolder = CreateFolderInternal(clientContext.Web, folderPadre, nombreFolder);
                    return ReturnSharePoint.CarpetaCreada;
                }
                catch (Exception ex) // ServerException ex)
                {
                    Logger.LogError(ex);
                    return ReturnSharePoint.Error;
                }
            }
        }

        private Folder CreateFolderInternal(Web web, Folder parentFolder, string fullFolderUrl)
        {
            var folderUrls = fullFolderUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string folderUrl = folderUrls[0];
            var curFolder = parentFolder.Folders.Add(folderUrl);
            web.Context.Load(curFolder);
            web.Context.ExecuteQuery();

            if (folderUrls.Length > 1)
            {
                var subFolderUrl = string.Join("/", folderUrls, 1, folderUrls.Length - 1);
                return CreateFolderInternal(web, curFolder, subFolderUrl);
            }
            return curFolder;
        }

        private void ConfigContext(ClientContext context)
        {
            var timeOut = WebConfigValues.SP_TimeOut;
            if (timeOut != 0)
            {
                context.RequestTimeout = timeOut * 1000;
            }
        }

        public ListItemCollection GetItemsListaSP(string lista, string query)
        {
            ListItemCollection collListItem = null;
            try
            {
                using (ClientContext clientContext = new ClientContext(SP_URL))
                {
                    clientContext.Credentials = GetNetworkCredential();
                    clientContext.RequestTimeout = SpTimeOut;
                    var oList = clientContext.Web.Lists.GetByTitle(lista);

                    var camlQuery = string.IsNullOrEmpty(query) ? CamlQuery.CreateAllItemsQuery() : new CamlQuery();
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        camlQuery.ViewXml = query;
                    }

                    collListItem = oList.GetItems(camlQuery);
                    clientContext.Load(oList);
                    clientContext.Load(collListItem);
                    clientContext.ExecuteQuery();

                    if (lista.ToLower() != "requerimientos") 
                    {
                        var fieldcol = oList.Fields;
                        clientContext.Load(fieldcol);
                        clientContext.ExecuteQuery();

                        foreach (var item in collListItem.ToList())
                        {
                            clientContext.Load(item);
                            clientContext.Load(item, i => i.ContentType);
                            clientContext.Load(item, i => i.File, i => i.File.ListItemAllFields);
                        }
                        clientContext.ExecuteQuery();
                    }

                }

                return collListItem;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                collListItem = null;
            }
            return collListItem;
        }

    }

}
