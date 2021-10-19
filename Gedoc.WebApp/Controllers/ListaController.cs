﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
 using Gedoc.Helpers;
 using Gedoc.Helpers.Dto;
 using Gedoc.Helpers.Enum;
 using Gedoc.Helpers.Logging;
 using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Gedoc.Repositorio.Model;
using Gedoc.WebApp.Models;
using Gedoc.Service.DataAccess.Interfaces;
 using Gedoc.WebApp.Helpers;
 using Gedoc.WebApp.Helpers.Maps.Interface;

 namespace Gedoc.WebApp.Controllers
{
    public class ListaController : BaseController
    {
        //private readonly IListaRepositorio servicioLista;
        //private readonly IParametroRepositorio servicioParametro;
        private readonly IMantenedorService _mantSrv;
        private readonly IGenericMap _mapper;

        #region Listas
        public ListaController(/*IListaRepositorio servicioLista, IParametroRepositorio servicioParametro,*/
            IMantenedorService mantSrv, IGenericMap mapper)
        {
            //this.servicioLista = servicioLista;
            //this.servicioParametro = servicioParametro;
            _mantSrv = mantSrv;
            _mapper = mapper;
        }

        #region Lista
        [Route("Lista")]
        public ActionResult IndexLista()
        {
            return View();
        }

        public ActionResult FormLista(int id)
        {
            ViewBag.AccesoForm = ValidaAccesoUtForm();
            var datos = new ListaDto
            {
                ListaValor = new List<ListaValorDto>(),
                IdEstadoRegistro = (int)EstadoRegistroEnum.Activo
            };

            if (id > 0)
            {
                datos = _mantSrv.GetListaById(id);
            }

            var model = _mapper.MapFromDtoToModel<ListaDto, ListaModel>(datos);

            ViewBag.ListaId = id;
            return View(model);
        }

        public ActionResult GetListaAll()
        {
            var result = _mantSrv.GetListaAll();

            return Json(result);
        }

        [HttpPost]
        public ActionResult SaveLista(ListaModel model)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }

            var datos = _mapper.MapFromModelToDto<ListaModel, ListaDto>(model);

            var resultadoOper = _mantSrv.SaveLista(datos);

            return Json(resultadoOper);
        }

        [HttpPost]
        public ActionResult EliminaLista(int listaId)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }
            var resultadoOper = _mantSrv.DeleteLista(listaId);

            return Json(resultadoOper);
        }
        #endregion

        #region ListaValor
        public ActionResult FormListaValor(int listaId, string codigo)
        {
            ViewBag.AccesoForm = ValidaAccesoUtForm();
            var datos = new ListaValorDto
            {
                IdEstadoRegistro = (int)EstadoRegistroEnum.Activo,
                IdLista = listaId
            };

            if (!string.IsNullOrWhiteSpace(codigo))
            {
                datos = _mantSrv.GetListaValorById(listaId, codigo);
            }

            var model = _mapper.MapFromDtoToModel<ListaValorDto, ListaValorModel>(datos);
            model.EsNuevo = string.IsNullOrWhiteSpace(model.Codigo);

            ViewBag.ListaId = listaId;
            return View(model);
        }

        public ActionResult GetListaValorAll(int listaId)
        {
            var result = _mantSrv.GetListaValorAllByListaId(listaId);

            return Json(result);
        }
        #endregion

        [HttpPost]
        public ActionResult SaveListaValor(ListaValorModel model)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }

            var datos = _mapper.MapFromModelToDto<ListaValorModel, ListaValorDto>(model);

            var resultadoOper = _mantSrv.SaveListaValor(datos);

            return Json(resultadoOper);
        }

        [HttpPost]
        public ActionResult EliminaListaValor(int listaId, string codigo)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }
            var resultadoOper = _mantSrv.DeleteListaValor(listaId, codigo);

            return Json(resultadoOper);
        }
        #endregion

        #region Mantenedor Unidad Técnica

        [Route("unidadtecnica")]
        public ActionResult IndexUnidadTecnica()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetUnidadTecnicaAll()
        {
            var datos = _mantSrv.GetUnidadTecnicaAll();
            return Json(datos);
        }

        public ActionResult FormUnidadTecnica(int utId)
        {
            ViewBag.AccesoForm = ValidaAccesoUtForm();
            var datos = new UnidadTecnicaDto
            {
                Activo = true
            };
            if (utId > 0)
            {
                datos = _mantSrv.GetUnidadTecnicaById(utId);
            }
            var model = _mapper.MapFromDtoToModel<UnidadTecnicaDto, UnidadTecnicaModel>(datos);
            return View(model);
        }

        public ActionResult FormIntegrantesUnidadTecnica(int utId)
        {
            ViewBag.AccesoForm = ValidaAccesoUtForm();
            ViewBag.UtId = utId;
            return View();
        }

        [HttpPost]
        public ActionResult GetIntegrantesUt(int utId)
        {
            var datos = _mantSrv.GetUsuariosUt(utId);
            return Json(datos);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveUnidadTecnica(UnidadTecnicaModel model)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }
            var datos = _mapper.MapFromModelToDto<UnidadTecnicaModel, UnidadTecnicaDto>(model);
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioCreacionId = CurrentUserId;
            var resultadoOper = _mantSrv.SaveUnidadTecnica(datos, false);

            return Json(resultadoOper);
        }

        [HttpPost]
        public ActionResult EliminaUnidadTecnica(int utId)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }
            var resultadoOper = _mantSrv.DeleteUnidadTecnica(utId);

            return Json(resultadoOper);
        }

        [HttpPost]
        public ActionResult SaveUsuarioUnidadTecnica(int utId, int userId)
        {
            var result = _mantSrv.SaveUsuarioUnidadTecnica(utId, userId);

            return Json(result);
        }

        [HttpPost]
        public ActionResult EliminaUsuarioUnidadTecnica(int utId, int userId)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }
            var resultadoOper = _mantSrv.DeleteUsuarioUnidadTecnica(utId, userId);

            return Json(resultadoOper);
        }
        #endregion


        #region Mantenedor Tipo de Trámite

        [Route("TipoTramite")]
        public ActionResult IndexTipoTramite()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetTipoTramiteAll()
        {
            var datos = _mantSrv.GetTipoTramiteAll(true);
            return Json(datos);
        }

        public ActionResult FormTipoTramite(int utId)
        {
            ViewBag.AccesoForm = ValidaAccesoUtForm();
            var datos = new TipoTramiteDto
            {
                Activo = true
            };
            if (utId > 0)
            {
                datos = _mantSrv.GetTipoTramiteById(utId);
            }
            var model = _mapper.MapFromDtoToModel<TipoTramiteDto, TipoTramiteModel>(datos);
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveTipoTramite(TipoTramiteModel model)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }
            var datos = _mapper.MapFromModelToDto<TipoTramiteModel, TipoTramiteDto>(model);
            datos.UsuarioActual = CurrentUserName;
            datos.UsuarioCreacionId = CurrentUserId;
            var resultadoOper = _mantSrv.SaveTipoTramite(datos, false);

            return Json(resultadoOper);
        }

        [HttpPost]
        public ActionResult EliminaTipoTramite(int utId)
        {
            if (!HaySesionActiva())
            {
                return Json(new ResultadoOperacion(-1, "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.", null));
            }
            var resultadoOper = _mantSrv.DeleteTipoTramite(utId);

            return Json(resultadoOper);
        }
        #endregion

        private ResultadoOperacion ValidaAccesoUtForm()
        {
            var result = new ResultadoOperacion(1, "OK", null);

            // TODO: chequear si el perfil del usuario tiene acceso al formulario (en la app no debe aparecerle la acción para acceder al form pero hacer el chequeo aquí para aumetar la seguridad)

            if (!HaySesionActiva())
            {
                result.Codigo = -1;
                result.Mensaje = "La sesión ha expirado, por favor, vuelva a <a href='/Home/Login'>iniciar sesión</a>.";
            }

            return result;
        }




    }
}
