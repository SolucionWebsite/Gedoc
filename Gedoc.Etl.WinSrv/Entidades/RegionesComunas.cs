using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Entidades
{
    public class RegionesComunas : BaseEntity
    {
        public int ID { get; set; }
        public string C_x00f3_digo_x0020_comuna { get; set; }
        public string C_x00f3_digo_x0020_provincia { get; set; }
        public string C_x00f3_digo_x0020_regi_x00f3_n { get; set; }
        public DateTime? Created { get; set; }
        public string Created_x0020_Date { get; set; }
        public DateTime? Modified { get; set; }
        public string Last_x0020_Modified { get; set; }
        public string Title { get; set; }
        public string Nombre_x0020_provincia { get; set; }
        public string Nombre_x0020_regi_x00f3_n { get; set; }
        public string Orden_x0020_regi_x00f3_n { get; set; }
        public string UniqueId { get; set; }
        //public int IdCarga { get; set; }
    }
}
