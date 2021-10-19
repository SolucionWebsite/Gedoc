using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class CustomHttpPostedFile : System.Web.HttpPostedFileBase
    {
        readonly Stream _stream;
        readonly string _contentType;
        readonly string _fileName;
        private readonly byte[] _fileBytes;

        public CustomHttpPostedFile(Stream stream, string contentType, string fileName)
        {
            this._contentType = contentType;
            this._stream = stream;
            this._fileName = fileName;
        }

        public CustomHttpPostedFile(byte[] fileBytes, string fileName = null)
        {
            this._fileBytes = fileBytes;
            this._stream = new MemoryStream(fileBytes);
            this._fileName = fileName;
        }

        public CustomHttpPostedFile(System.Web.HttpPostedFile hostedFile)
        {
            this._contentType = hostedFile.ContentType;
            this._stream = hostedFile.InputStream;
            this._fileName = hostedFile.FileName;
        }

        public override Stream InputStream
        {
            get { return _stream; }
        }

        public override string ContentType
        {
            get { return _contentType; }
        }

        public override int ContentLength
        {
            get { return (int)_stream.Length; }
        }

        public override string FileName
        {
            get { return _fileName; }
        }

        public override void SaveAs(string filename)
        {
            using (var file = File.Open(filename, FileMode.CreateNew))
                _stream.CopyTo(file);
        }
    }
}
