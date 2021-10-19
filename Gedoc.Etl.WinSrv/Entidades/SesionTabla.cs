using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Entidades
{
    public class SesionTabla : BaseEntity
    {
        public int Sesion_Id { get; set; }
        public string Nombre { get; set; }
        public int UnidadTecnica_Id { get; set; }
        public string CreadoPor { get; set; }
        public DateTime? FechaCreacion { get; set; }
        //public int IdCarga { get; set; }

    }
}
