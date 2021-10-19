using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Logging;
using HiQPdf;

namespace Gedoc.Service.Pdf
{
    public class PdfService : IPdfService
    {
        private PdfData _pdfData;
        private readonly int _browserWidth = 800;

        public byte[] HtmlToPdf(PdfData pdfData)
        {
            // Se obtiene la etiqueta <style> a incrustar en el html de la plantilla. Este style tiene el estilo de los margenes
            // del body del Html. Los margenes de la página pdf no se especifican, se dejan en 0, los margenes serán los
            // definidos en el body del html. Para el encabezado se toman todos los margenes excepto el inferior. Para
            // el cuerpo se toman los margenes izquierdo y derecho. Y para el pie se toman todos los margenes excepto el superior.
            _pdfData = pdfData;
            var pageSize = GetPageSize();
            _pdfData.Html =
                "<html> <head> " +
                "<link rel=\"stylesheet\" href=\"/Content/hiQPdfStyles.css\" type=\"text/css\"  />" +
                "<link rel=\"stylesheet\" href=\"/Content/pdfCommonStyles.css\" type=\"text/css\"  />" +
                $" {GetStyleTag(' ', _browserWidth)}</head> <body class=\"bodyPdf\">" +
                _pdfData.Html +
                "</body> </html>";
            _pdfData.HeaderHtml =
                "<html> <head> " +
                "<link rel=\"stylesheet\" href=\"/Content/hiQPdfStyles.css\" type=\"text/css\"  /> " +
                //"<link rel=\"stylesheet\" href=\"/Content/pdfCommonStyles.css\" type=\"text/css\"  />" +
                $" {GetStyleTag('H', _browserWidth)}</head> <body id=\"body-encabezado-impresion\">" +
                _pdfData.HeaderHtml +
                "</body> </html>";
            _pdfData.FooterHtml =
                "<html> <head> " +
                "<link rel=\"stylesheet\" href=\"/Content/hiQPdfStyles.css\" type=\"text/css\"  /> " +
                //"<link rel=\"stylesheet\" href=\"/Content/pdfCommonStyles.css\" type=\"text/css\"  />" +
                $" {GetStyleTag('F', _browserWidth)}</head> <body id=\"body-pie-impresion\">" +
                _pdfData.FooterHtml +
                "</body> </html>";
            HtmlToPdf htmlToPdfConverter = new HtmlToPdf();
            ConfiguraPdf(htmlToPdfConverter);
            SetFooter(htmlToPdfConverter.Document);
            SetHeader(htmlToPdfConverter.Document);

            // Intentando corregir los parrafos q solo tienen el caracter uFEFF y q cuando se exporta a pdf en ocasiones no se muestra ese parrafo vacío y se pierde la separación entre parrafos.
            //_pdfData.Html = System.Text.RegularExpressions.Regex.Replace(_pdfData.Html, @"[^\u0000-\u007F]", "------");
            //_pdfData.Html = _pdfData.Html.Replace("\uFEFF", @"------");
            //_pdfData.Html = _pdfData.Html.Replace("\xFEFF", @"------");

            //Logger.LogInfo("HTML de Pdf de plantilla de oficio********************");
            //Logger.LogInfo(_pdfData.Html);
            //Logger.LogInfo("FIN -- HTML de Pdf de plantilla de oficio********************");

            var result = htmlToPdfConverter.ConvertHtmlToMemory(_pdfData.Html, _pdfData.BaseUrl);

            return result;
        }

        private void ConfiguraPdf(HtmlToPdf htmlToPdfConverter)
        {
            htmlToPdfConverter.SerialNumber = WebConfigValues.HiQPdfSerialNumber;
            var document = htmlToPdfConverter.Document;

            // Información del PDF
            document.Properties.Author = "Gestor Documental CMN";
            document.Properties.CreationDate = DateTime.Now;
            document.Properties.Title = "";
            document.Properties.Subject = "";
            document.Properties.Keywords = "";

            // Tamaño de página
            document.PageSize = GetPageSize();

            // Márgenes de la página
            document.Margins = new PdfMargins(
                0, // _pdfData.MargenIzqF,
                0, //_pdfData.MargenDerF, 
                0, /*_pdfData.MargenSupF,*/ // El Header no queda dentro del margen sino q después. Aparece primero el margen superior y luego el header, por eso se pone solo 0, luego el header tendrá su ancho propio y luego vendrá el texto de la página. Idem para el footer.
                0 /*_pdfData.MargenInfF*/);

            // Escala y otras opciones
            document.ResizePageWidth = false;
            document.FitPageHeight = false;
            document.FitPageWidth = false; // true; // 
            htmlToPdfConverter.BrowserWidth = _browserWidth;


            htmlToPdfConverter.PageCreatingEvent +=
                        new PdfPageCreatingDelegate(htmlToPdfConverter_PageCreatingEvent);

        }

        private void SetHeader(PdfDocumentControl htmlToPdfDocument)
        {
            var documentHeader = htmlToPdfDocument.Header;
            // se habilita el header
            documentHeader.Enabled = true;

            // header height
            documentHeader.Height = GetHeaderFooterHeight(true);

            // header background color
            documentHeader.BackgroundColor = Color.White;

            //float pdfPageWidth = htmlToPdfDocument.PageOrientation == PdfPageOrientation.Portrait ?
            //                            htmlToPdfDocument.PageSize.Width : htmlToPdfDocument.PageSize.Height;
            //float headerWidth = pdfPageWidth - htmlToPdfDocument.Margins.Left - htmlToPdfDocument.Margins.Right;
            //float headerHeight = documentHeader.Height;

            #region Layout en el Header
            // layout HTML en header
            PdfHtml headerHtml = new PdfHtml(0, 0, _pdfData.HeaderHtml, _pdfData.BaseUrl);
            headerHtml.BrowserWidth = _browserWidth;
            headerHtml.FitDestHeight = false;
            headerHtml.FitDestWidth = true;
            headerHtml.FontEmbedding = true;
            documentHeader.Layout(headerHtml);
            #endregion

            #region Imagen en Header
            //string headerImageFile = Application.StartupPath + @"\DemoFiles\Images\HiQPdfLogo.png";
            //PdfImage logoHeaderImage = new PdfImage(headerWidth - 40 - 5, 5, 40, Image.FromFile(headerImageFile));
            //documentHeader.Layout(logoHeaderImage);
            #endregion

            #region Borde en el Header
            // create a border for header
            //PdfRectangle borderRectangle = new PdfRectangle(1, 1, headerWidth - 2, headerHeight - 2);
            //borderRectangle.LineStyle.LineWidth = 0.5f;
            //borderRectangle.ForeColor = Color.DarkGreen;
            //documentHeader.Layout(borderRectangle);
            #endregion
        }

        private void SetFooter(PdfDocumentControl htmlToPdfDocument)
        {
            var documentFooter = htmlToPdfDocument.Footer;
            // se habilita el footer
            documentFooter.Enabled = true;

            // footer height
            documentFooter.Height = GetHeaderFooterHeight(false);

            // footer background color
            documentFooter.BackgroundColor = Color.White;

            //float pdfPageWidth = htmlToPdfDocument.PageOrientation == PdfPageOrientation.Portrait ?
            //                            htmlToPdfDocument.PageSize.Width : htmlToPdfDocument.PageSize.Height;
            //float footerWidth = pdfPageWidth - htmlToPdfDocument.Margins.Left - htmlToPdfDocument.Margins.Right;
            //float footerHeight = documentFooter.Height;

            #region Layout en el Footer
            //layout HTML in footer
            PdfHtml footerHtml = new PdfHtml(0, 0, _pdfData.FooterHtml, _pdfData.BaseUrl);
            footerHtml.BrowserWidth = _browserWidth;
            footerHtml.FitDestHeight = false;
            footerHtml.FitDestWidth = true;
            footerHtml.FontEmbedding = true;
            documentFooter.Layout(footerHtml);
            #endregion

            #region Número de página en el Footer
            //// add page numbering
            //Font pageNumberingFont = new Font(new FontFamily("Arial"), 8, GraphicsUnit.Point);
            ////pageNumberingFont.Mea
            //PdfText pageNumberingText = new PdfText(5, footerHeight - 12, "Página {CrtPage} de {PageCount}", pageNumberingFont);
            //pageNumberingText.HorizontalAlign = PdfTextHAlign.Center;
            //pageNumberingText.EmbedSystemFont = true;
            //pageNumberingText.ForeColor = Color.DarkGray;
            //documentFooter.Layout(pageNumberingText);
            #endregion

            #region Imagen en el Footer
            //string footerImageFile = Application.StartupPath + @"\DemoFiles\Images\HiQPdfLogo.png";
            //PdfImage logoFooterImage = new PdfImage(footerWidth - 40 - 5, 5, 40, Image.FromFile(footerImageFile));
            //documentFooter.Layout(logoFooterImage);
            #endregion

            #region Borde en el Footer
            // create a border for footer
            //PdfRectangle borderRectangle = new PdfRectangle(1, 1, footerWidth - 2, footerHeight - 2);
            //borderRectangle.LineStyle.LineWidth = 0.5f;
            //borderRectangle.ForeColor = Color.DarkGreen;
            //documentFooter.Layout(borderRectangle);
            #endregion
        }

        void htmlToPdfConverter_PageCreatingEvent(PdfPageCreatingParams eventParams)
        {
            PdfPage pdfPage = eventParams.PdfPage;
            int pdfPageNumber = eventParams.PdfPageNumber;
            //...
        }

        private PdfPageSize GetPageSize()
        {
            var paperSize = PdfPageSize.A4;
            switch (_pdfData.PageSize)
            {
                case "A0":
                    paperSize = PdfPageSize.A0;
                    break;
                case "A1":
                    paperSize = PdfPageSize.A1;
                    break;
                case "A2":
                    paperSize = PdfPageSize.A2;
                    break;
                case "A3":
                    paperSize = PdfPageSize.A3;
                    break;
                case "A4":
                    paperSize = PdfPageSize.A4;
                    break;
                case "A5":
                    paperSize = PdfPageSize.A5;
                    break;
                case "A6":
                    paperSize = PdfPageSize.A6;
                    break;
                case "A7":
                    paperSize = PdfPageSize.A7;
                    break;
                case "A8":
                    paperSize = PdfPageSize.A8;
                    break;
                case "A9":
                    paperSize = PdfPageSize.A9;
                    break;
                case "A10":
                    paperSize = PdfPageSize.A10;
                    break;
                case "B0":
                    paperSize = PdfPageSize.B0;
                    break;
                case "B1":
                    paperSize = PdfPageSize.B1;
                    break;
                case "B2":
                    paperSize = PdfPageSize.B2;
                    break;
                case "B3":
                    paperSize = PdfPageSize.B3;
                    break;
                case "B4":
                    paperSize = PdfPageSize.B4;
                    break;
                case "B5":
                    paperSize = PdfPageSize.B5;
                    break;
                case "ArchE":
                    paperSize = PdfPageSize.ArchE;
                    break;
                case "ArchD":
                    paperSize = PdfPageSize.ArchD;
                    break;
                case "ArchC":
                    paperSize = PdfPageSize.ArchC;
                    break;
                case "ArchB":
                    paperSize = PdfPageSize.ArchB;
                    break;
                case "ArchA":
                    paperSize = PdfPageSize.ArchA;
                    break;
                case "Flsa":
                    paperSize = PdfPageSize.Flsa;
                    break;
                case "Letter":
                    paperSize = PdfPageSize.Letter;
                    break;
                case "Note":
                    paperSize = PdfPageSize.Note;
                    break;
                case "Legal":
                    paperSize = PdfPageSize.Legal;
                    break;
                case "HalfLetter":
                    paperSize = PdfPageSize.HalfLetter;
                    break;
                case "11x17":
                    paperSize = PdfPageSize.Letter11x17;
                    break;
                case "Ledger":
                    paperSize = PdfPageSize.Ledger;
                    break;
            }

            return paperSize;
        }

        private float GetHeaderFooterHeight(bool esHeader)
        {
            // TODO: analizar si una variante más optima de obtener el alto del footer y header q hay en la plantilla
            // de oficio html. Este footer y header lo define el usuario al diseñar la plantilla por lo q puede tener cualquier alto.

            // Aquí lo q se hace es generar un pdf solamente con el footer, o con el header, y q la página
            // se ajuste al tamaño del contenido. Luego se toma el alto de la página.

            try
            {
                HtmlToPdf htmlToPdfConverter2 = new HtmlToPdf();
                htmlToPdfConverter2.BrowserWidth = _browserWidth;
                var document = htmlToPdfConverter2.Document;
                document.PageOrientation = PdfPageOrientation.Portrait;
                document.PostCardMode = true;
                document.PageSize = new PdfPageSize(_browserWidth, 20);
                document.FitPageWidth = false;
                document.FitPageHeight = false;
                document.ResizePageWidth = true;
                var pdfDoc = htmlToPdfConverter2.ConvertHtmlToPdfDocument(esHeader ? _pdfData.HeaderHtml : _pdfData.FooterHtml, _pdfData.BaseUrl);

                return pdfDoc.Pages[0].Size.Height + 10; // se da una holgura de 10pt
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return esHeader ? _pdfData.MargenSupF : _pdfData.MargenInfF;
            }
        }

        private string GetStyleTag(char tipo, int width = 800)
        {
            var boxSizingCss = /*tipo != 'H' && tipo != 'F' ?*/ "box-sizing: border-box;" /*: ""*/;
            var widthCss = $" width: {width}px !important; max-width: {width}px !important; {boxSizingCss} ";
            var style = $"<style> body {{padding: 0 {_pdfData.MargenDer} 0 {_pdfData.MargenIzq}; {widthCss} }} </style>";
            switch (tipo)
            {
                case 'H': // Header
                    style = $"<style> body {{padding: {_pdfData.MargenSup} {_pdfData.MargenDer} 0 {_pdfData.MargenIzq}; {widthCss} }} </style>";
                    break;
                case 'F': // Footer
                    style = $"<style> body {{padding: 0 {_pdfData.MargenDer} {_pdfData.MargenInf} {_pdfData.MargenIzq}; {widthCss} }} </style>";
                    break;
                default:
                    break;
            }

            return style;
        }

    }
}
