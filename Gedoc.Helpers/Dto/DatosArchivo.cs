using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Gedoc.Helpers.Dto
{
    public class DatosArchivo
    {
        public int AdjuntoId { get; set; }
        public int OrigenId { get; set; }
        public string OrigenCodigo { get; set; }
        public HttpPostedFileBase File { get; set; }
        public TiposArchivo TipoArchivo { get; set; }

        public string FileName;
        public string FilePath;
        public string FileTextContent;
        public string ContentType;
        public Stream FileStream;
        public string Mensaje;
        public bool renombraSiExiste;
    }

    public enum TiposArchivo
    {
        Despacho = 1,
        DespachoIniciativa = 2,
        Adjunto = 3,
        Bitacora = 4,
        Oficio = 5,
        AdjuntoOficio = 6

    }
}