using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Gedoc.WebApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            config.Routes.MapHttpRoute(
               name: "ListadoApi",
               routeTemplate: "api/{controller}/{idListado}/{idItem}",
               defaults: new { idListado = RouteParameter.Optional, idItem = RouteParameter.Optional }
           );
            
        }
    }
}
