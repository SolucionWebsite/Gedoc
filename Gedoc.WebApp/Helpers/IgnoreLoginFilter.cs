using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gedoc.WebApp.Helpers
{
    public class IgnoreLoginFilter : ActionFilterAttribute
    {
        // Sólo para indicar actions en controllers q no chequearán por un login válido.
    }
}