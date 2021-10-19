using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Gedoc.WebApp.Helpers;

namespace Gedoc.WebApp.Controllers
{
    public class ErrorController : BaseController
    {
        [IgnoreLoginFilter]
        [AllowAnonymous]
        public ViewResult Index(string id)
        {
            Response.StatusCode = (int)HttpStatusCode.InternalServerError; ;
            ViewBag.ErrorId = id;
            return View("Error");
        }

        //public ViewResult Index(int statusCode, Exception exception)
        //{
        //    //Response.StatusCode = statusCode;
        //    // var model = new ErrorModel() { Exception = exception, HttpStatusCode = statusCode };
        //    return View("Error", exception);
        //}

        [IgnoreLoginFilter]
        [AllowAnonymous]
        public ViewResult NotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View("NotFound");
        }

        [IgnoreLoginFilter]
        [AllowAnonymous]
        public ViewResult Unauthorized()
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return View("Unauthorized");
        }

        [IgnoreLoginFilter]
        [AllowAnonymous]
        public ViewResult SessionExpired()
        {
            Response.StatusCode = (int) HttpStatusCode.OK; // (int)HttpStatusCode.Unauthorized;
            return View("SessionExpired");
        }

    }
}