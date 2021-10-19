using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Entidades
{
    public class UnidadTecnica: BaseEntity
    {
        public int ID { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime? Created { get; set; }
        public string GUID { get; set; }
        public string Last_x0020_Modified { get; set; }
        public string Created_x0020_Date { get; set; }
        public string Title { get; set; }
        public string Correo_x0020_Responsable_x0020_U { get; set; }
        public string Correo_x0020_Secretaria_x0020_UT { get; set; }
        public string Nombre_x0020_Grupo { get; set; }
        public string Responsable_x0020_UT { get; set; }
        public string Subrogante { get; set; }
        public string UniqueId { get; set; }
        //public int IdCarga { get; set; }
    }
}
