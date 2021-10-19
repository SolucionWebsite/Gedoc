using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Entidades
{
    public class LogEtl
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        public int ParentLogId { get; set; }
    }
}
