using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Gedoc.Helpers;
using Gedoc.Helpers.Logging;
using Gedoc.WebApp.App_Start;
using Gedoc.WebApp.Controllers;
using Newtonsoft.Json;

namespace Gedoc.WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            AppInitializer.Initialize();
        }

        //protected void Application_Error(object sender, EventArgs e)
        //{
        //    System.Diagnostics.Trace.WriteLine("Enter - Application_Error");

        //    var httpContext = ((MvcApplication)sender).Context;

        //    var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
        //    var currentController = " ";
        //    var currentAction = " ";

        //    if (currentRouteData != null)
        //    {
        //        if (currentRouteData.Values["controller"] != null &&
        //            !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
        //        {
        //            currentController = currentRouteData.Values["controller"].ToString();
        //        }

        //        if (currentRouteData.Values["action"] != null &&
        //            !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
        //        {
        //            currentAction = currentRouteData.Values["action"].ToString();
        //        }
        //    }

        //    var ex = Server.GetLastError();

        //    if (ex != null)
        //    {
        //        System.Diagnostics.Trace.WriteLine(ex.Message);

        //        if (ex.InnerException != null)
        //        {
        //            System.Diagnostics.Trace.WriteLine(ex.InnerException);
        //            System.Diagnostics.Trace.WriteLine(ex.InnerException.Message);
        //        }
        //    }

        //    var controller = new ErrorController();
        //    var routeData = new RouteData();
        //    var action = "CustomError";
        //    var statusCode = 500;

        //    if (ex is HttpException)
        //    {
        //        var httpEx = ex as HttpException;
        //        statusCode = httpEx.GetHttpCode();

        //        switch (httpEx.GetHttpCode())
        //        {
        //            case 400:
        //                action = "BadRequest";
        //                break;

        //            case 401:
        //                action = "Unauthorized";
        //                break;

        //            case 403:
        //                action = "Forbidden";
        //                break;

        //            case 404:
        //                action = "PageNotFound";
        //                break;

        //            case 500:
        //                action = "CustomError";
        //                break;

        //            default:
        //                action = "CustomError";
        //                break;
        //        }
        //    }
        //    else if (ex is AuthenticationException)
        //    {
        //        action = "Forbidden";
        //        statusCode = 403;
        //    }

        //    httpContext.ClearError();
        //    httpContext.Response.Clear();
        //    httpContext.Response.StatusCode = statusCode;
        //    httpContext.Response.TrySkipIisCustomErrors = true;
        //    routeData.Values["controller"] = "Error";
        //    routeData.Values["action"] = action;

        //    controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
        //    ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        //}

        protected void Application_Error(object sender, EventArgs e)
        {
            // Response.TrySkipIisCustomErrors = true; 
            var httpContext = ((HttpApplication)sender).Context;
            var ex = Server.GetLastError();

            Server.ClearError();
            Response.Clear();

            var errorId = Guid.NewGuid();
            if (ex != null)
            {
                Logger.LogError("Error inesperado al procesar la solicitud. Código de error: " + errorId, ex);
            }

            if (httpContext.Request["X-Requested-With"] == "XMLHttpRequest" ||
                (httpContext.Request.Headers != null && httpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"))
            {
                // es error en una llamada Ajax
                var msg = "Lo sentimos, ha ocurrido un error al procesar la solicitud";
                if (!User.Identity.IsAuthenticated || Session["IdUsuario"] == null)
                {  // Error de sesión caducada
                    msg = "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.";
                } else if (ex is System.Web.Mvc.HttpAntiForgeryException)
                {  // Error de validación de formulario
                    msg =
                        "No ha sido posible validar el token de seguridad del formulario, por favor, intente <a href='/Home/Logout'>iniciando sesión</a> nuevamente.";
                }
                // Error de tamaño de solicitud demasiado grande (cuando se intenta subir archivos muy pesados)
                var httpException = (ex as HttpException) ?? (ex.InnerException as HttpException);
                if (httpException != null && httpException.WebEventCode == System.Web.Management.WebEventCodes.RuntimeErrorPostTooLarge)
                {
                    msg = "El tamaño de archivo sobrepasa el máximo permitido.";
                }

                Response.Write(JsonConvert.SerializeObject(new
                {
                    Datos = "",
                    Resultado = new { Codigo = -1, Mensaje = msg },
                    // Para q la respuesta sea compatible tanto para aquellas llamadas q deben recibir solamente un tipo ResultadoOperacion
                    // como las q reciben Resultado de tipo ResultadoOperacion:
                    Codigo = -1, 
                    Mensaje = msg
                })
                );
                Response.ContentType = "application/json";
            }
            else
            {
                string relativePath = "~/Error{0}";
                if (ex is HttpException)
                {
                    var httpEx = ex as HttpException;
                    switch (httpEx.GetHttpCode())
                    {
                        //case (int)HttpStatusCode.BadRequest:
                        //    Server.TransferRequest(string.Format(relativePath, "BadRequest"));
                        //    break;
                        case (int)HttpStatusCode.Unauthorized:
                            Server.TransferRequest(string.Format(relativePath, "/Unauthorized"));
                            break;
                        case (int)HttpStatusCode.Forbidden:
                            Server.TransferRequest(string.Format(relativePath, "/Unauthorized"));
                            break;
                        case (int)HttpStatusCode.NotFound:
                            Server.TransferRequest(string.Format(relativePath, "/NotFound"));
                            break;
                        case (int)HttpStatusCode.InternalServerError:
                            Server.TransferRequest(string.Format(relativePath, "?id=" + errorId.ToString()));
                            break;
                        default:
                            Server.TransferRequest(string.Format(relativePath, "?id=" + errorId.ToString()));
                            break;
                    }
                }
                else
                {
                    Server.TransferRequest(string.Format(relativePath, "?id=" + errorId.ToString()));
                }
            }


        }


    }
}
