using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using HiQPdf;

namespace Gedoc.Service.Pdf
{
    public interface IPdfService
    {

        byte[] HtmlToPdf(PdfData pdfData);

    }
}
