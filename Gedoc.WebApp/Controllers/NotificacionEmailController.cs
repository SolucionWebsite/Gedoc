using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Controllers;
using Gedoc.WebApp.Helpers.Maps.Interface;
using Gedoc.WebApp.Models;

namespace Gedoc.WebApp.Controllers
{
    public class NotificacionEmailController : BaseController
    {
        private readonly IMantenedorService _mantenedorSrv;
        private readonly IGenericMap _mapper;

        public NotificacionEmailController(IMantenedorService mantenedorSrv, IGenericMap mapper)
        {
            _mantenedorSrv = mantenedorSrv;
            _mapper = mapper;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Edicion(int id)
        {
            var datos = _mantenedorSrv.GetNotificacionById(id);
            var model = _mapper.MapFromModelToDto<NotificacionEmailDto, NotificacionEmailModel>(datos);
            ViewBag.AccesoForm = ValidaAccesoForm();
            return View("FormNotificacion", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Save(NotificacionEmailModel model)
        {
            var datos = _mapper.MapFromModelToDto<NotificacionEmailModel, NotificacionEmailDto>(model);
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioCreacionId = CurrentUserId;
            datos.Mensaje = WebUtility.HtmlDecode(datos.Mensaje);
            var resultadoOper = _mantenedorSrv.SaveNotificacion(datos, false);

            return Json(resultadoOper);
        }

        [HttpPost]
        public ActionResult SaveActivo(NotificacionEmailModel model)
        {
            var datos = _mapper.MapFromModelToDto<NotificacionEmailModel, NotificacionEmailDto>(model);
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioCreacionId = CurrentUserId;
            datos.Mensaje = WebUtility.HtmlDecode(datos.Mensaje);
            var resultadoOper = _mantenedorSrv.SaveNotificacion(datos, true);

            return Json(resultadoOper);
        }

        public ActionResult GetNotificacionAll()
        {
            var datos = _mantenedorSrv.GetNotificacionAll();
            return Json(datos);
        }

        public ActionResult GetNotificacionById(int id)
        {
            var datos = _mantenedorSrv.GetNotificacionById(id);
            return Json(datos);
        }

        private ResultadoOperacion ValidaAccesoForm()
        {
            var result = new ResultadoOperacion(1, "OK", null);

            // TODO: chequear si el perfil del usuario tiene acceso al formulario (en la app no debe aparecerle la acción para acceder al form pero hacer el chequeo aquí para aumetar la seguridad)

            if (!HaySesionActiva())
            {
                result.Codigo = -1;
                result.Mensaje = "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.";
                return result;
            }

            return result;
        }



    }
}