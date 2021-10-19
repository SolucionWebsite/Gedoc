using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.ReportData.Wss.Data
{

      [Serializable]
    public class SessionBL
    {
          public int idSession { get; set; }
          public string Nombre { get; set; }
          public string IdUT { get; set; }
          public string UT { get; set; }

    }
}