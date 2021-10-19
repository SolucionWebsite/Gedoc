using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class ControlCambiosEntidad
    {
        public string EntityName { get; set; }
        public Dictionary<string, object> CamposModificados { get; set; }
        //public Dictionary<string, object> Original { get; set; }
        //public Dictionary<string, object> Current { get; set; }
    }
}
