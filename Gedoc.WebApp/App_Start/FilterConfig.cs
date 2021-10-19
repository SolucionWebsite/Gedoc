using System.Web;
using System.Web.Mvc;

namespace Gedoc.WebApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            var handleErrAtt = new HandleErrorAttribute
            {
                // View = "/Error"
            };
            filters.Add(handleErrAtt);
        }
    }
}
