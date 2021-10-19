using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Entidades
{
    public class SesionTablaDet : BaseEntity
    {
        public int SesionDetalle_Id { get; set; }
        public int Sesion_Id { get; set; }
        public int Requerimento_Id { get; set; }
        public string DocumentoIngreso { get; set; }
        //public int IdCarga { get; set; }

    }
}
