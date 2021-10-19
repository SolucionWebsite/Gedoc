using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Service.Pdf
{
    public class PdfData
    {
        private float _margenSupF = 0;
        private float _margenDerF = 0;
        private float _margenInfF = 0;
        private float _margenIzqF = 0;

        public string Html { get; set; }
        public string BaseUrl { get; set; }
        public string HeaderHtml { get; set; }
        public string FooterHtml { get; set; }
        public string PageSize { get; set; }
        public string MargenSup { get; set; }
        public string MargenDer { get; set; }
        public string MargenInf { get; set; }
        public string MargenIzq { get; set; }

        public float MargenSupF
        {
            get
            {
                if (_margenSupF > 0) return _margenSupF;
                if (float.TryParse(MargenSup, out var margen))
                    _margenSupF = margen;
                else
                {
                    return 0;
                }

                return _margenSupF;
            }
            set { _margenSupF = value; }
        }

        public float MargenDerF
        {
            get
            {
                if (_margenDerF > 0) return _margenDerF;
                if (float.TryParse(MargenDer, out var margen))
                    _margenDerF = margen;
                else
                {
                    return 0;
                }

                return _margenDerF;
            }
            set { _margenDerF = value; }
        }
        public float MargenInfF
        {
            get
            {
                if (_margenInfF > 0) return _margenInfF;
                if (float.TryParse(MargenInf, out var margen))
                    _margenInfF = margen;
                else
                {
                    return 0;
                }

                return _margenInfF;
            }
            set { _margenInfF = value; }
        }
        public float MargenIzqF
        {
            get
            {
                if (_margenIzqF > 0) return _margenIzqF;
                if (float.TryParse(MargenIzq, out var margen))
                    _margenIzqF = margen;
                else
                {
                    return 0;
                }

                return _margenIzqF;

            }
            set { _margenIzqF = value; }
        }

    }
}
