using System.IO;
using System.Net;

namespace Gedoc.Service.Sharepoint
{
    public interface ISharePointClient
    {
        NetworkCredential GetNetworkCredential();

        //SPList.ListsSoapClient GetListService();

        //SPDWS.DwsSoapClient GetDwsService();

        ReturnSharePoint CreateFolder(string biblioteca, string nombreFolder);

        ReturnSharePoint CreateFolder2(string biblioteca, string nombreFolder);

        ReturnSharePoint CreateFolder3(string biblioteca, string nombreFolder);

        ReturnSharePoint UploadFile(string fullFilePathSp, Stream fileStream);

        ReturnSharePoint AddItemLibrary(string localFilePath, string folderNameSp, string fileRelativeUrl);

        Stream GetFileStream(string fileRelativePath);

        //Enums.ReturnSharePoint DeleteFile(string DocumentID, string PathFile);
    }
}
