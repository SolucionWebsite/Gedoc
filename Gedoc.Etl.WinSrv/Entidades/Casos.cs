using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Entidades
{
    public class Casos : BaseEntity
    {
        public int ID { get; set; }
        public DateTime? Created { get; set; }
        public string Created_x0020_Date { get; set; }
        public DateTime? Modified { get; set; }
        public string Last_x0020_Modified { get; set; }
        public string Title { get; set; }
        public string UniqueId { get; set; }
        //public int IdCarga { get; set; }
        public string GUID { get; set; }
        public int Cantidad_x0020_Casos { get; set; }
        public DateTime? Fecha_x0020_Referencia_x0020_del { get; set; }

    }
}
