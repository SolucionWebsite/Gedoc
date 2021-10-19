using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class VistaGeneroGrupoDto
    {
        public int Total { get; set; }
        public int Mes { get; set; }
        public int Anno { get; set; }
        public string Genero { get; set; }
        public string AnnoMes
        {
            get
            {
                var nombreMes = CultureInfo.CreateSpecificCulture("es").DateTimeFormat.GetMonthName(Mes);
                return Anno.ToString() + "-(" + Mes.ToString().PadLeft(2, '0') + " " + nombreMes + ")";
            }
            set { }
        }
        //public AggregateFunctionsProjection AggregateFunctionsProjection { get; set; }
        //public Aggregates Aggregates { get; set; }
        //public bool HasSubgroups { get; set; }
        //public string Member { get; set; }
        //public int ItemCount { get; set; }
        //public string Key { get; set; }
        //public List<Object> Items { get; set; }
    }


}
