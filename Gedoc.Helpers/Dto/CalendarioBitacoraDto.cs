using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class CalendarioBitacoraDto
    {
        public int Id { get; set; }
        public string TipoBitacoraCod { get; set; }
        public string TipoBitacoraTitulo { get; set; }
        public string CreadorPor { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaUltimaModificacion { get; set; }
        public DateTime? Fecha { get; set; }

        public string FechaStr
        {
            get { return Fecha.HasValue ? Fecha.Value.ToString(GeneralData.FORMATO_FECHA_CORTO) : ""; }
        }

        public string UsuarioActual { get; set; }
        public int? UsuarioCreacionId { get; set; }

        public int? Anno
        {
            get { return Fecha.HasValue ? Fecha.GetValueOrDefault().Year : (int?)null; }
        }
    }
}
