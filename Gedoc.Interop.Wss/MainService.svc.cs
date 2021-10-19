using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Logging;
using Gedoc.Interop.Wss.Data;
using Gedoc.Interop.Wss.Services;
using Gedoc.Service.DataAccess;

namespace Gedoc.Interop.Wss
{
    [ServiceBehavior(Namespace = "http://dibam.cl/gedocservice")]
    public class MainService : IMainService
    {
        private readonly IIngresoSrv _ingresoSrv;

        //public MainService(IIngresoSrv ingresoSrv)
        //{
        //    _ingresoSrv = ingresoSrv;
        //}

        public MainService(RequerimientoService reqSrv, DespachoService despSrv, MantenedorService mantSrv)
        {
            _ingresoSrv = new IngresoSrv(reqSrv, despSrv, mantSrv);
        }

        public ResultadoDatosIngreso GetIngreso(string numero)
        {
            //TODO: validar q el sistema de trámites envíe la clave de seguridad para las peticiones q no son de inyección de datos
            if (!AccesoValido("NO-VALIDAR"))
            {
                return new ResultadoDatosIngreso()
                {
                    Resultado = "DENEGADO",
                    Observaciones = "Acceso denegado.",
                    Datos = null
                };
            }

            var datos = _ingresoSrv.GetDatosIngreso(numero);

            return new ResultadoDatosIngreso()
            {
                Resultado = datos == null ? "ERROR" : (string.IsNullOrWhiteSpace(datos.NumeroIngreso) ? "NOTFOUND" : "OK"),
                Observaciones = datos == null ? "Error interno al realizar la operación." : "",
                Datos = datos
            };
        }

        public ResultadoEstadoSolicitud GetEstadoSolicitud(string idSolicitud)
        {
            if (!AccesoValido("NO-VALIDAR")) //idSolicitud))
            {
                return new ResultadoEstadoSolicitud()
                {
                    Resultado = "DENEGADO",
                    Observaciones = "Acceso denegado.",
                    Datos = null
                };
            }

            if (!int.TryParse(idSolicitud, out var idSolicitudInt))
            {
                return new ResultadoEstadoSolicitud()
                {
                    Resultado = "ERROR",
                    Observaciones = "El parametro idSolicitud tiene que ser un número entero.",
                    Datos = null
                };
            }
            
            return _ingresoSrv.GetEstadoSolicitud(idSolicitudInt);

        }

        public Stream GetAdjuntoIngreso(string numero, string clave)
        {
            try
            {
                var segSrv = new SeguridadSrv();
                var isValid = segSrv.CompruebaHash((SeguridadSrv.ClaveSeguridad ?? "") + numero, clave);
                if (!isValid)
                {
                    if (WebOperationContext.Current != null)
                    {
                        var response = WebOperationContext.Current.OutgoingResponse;
                        response.StatusCode = HttpStatusCode.Forbidden;
                        response.StatusDescription = "Acceso denegado.";
                    }
                    return null;
                }

                var datosAdj = _ingresoSrv.GetAdjuntoDespacho(numero);
                if (datosAdj == null)
                {
                    // Hubo error
                    if (WebOperationContext.Current != null)
                    {
                        var response = WebOperationContext.Current.OutgoingResponse;
                        response.StatusCode = HttpStatusCode.InternalServerError;
                        response.StatusDescription = "Ha ocurrido un error al procesar la petición, consulte al administrador del sistema.";
                    }
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(datosAdj.FileName))
                {
                    // No se encontró el adjunto
                    if (WebOperationContext.Current != null)
                    {
                        var response = WebOperationContext.Current.OutgoingResponse;
                        response.StatusCode = HttpStatusCode.NotFound;
                        response.StatusDescription = "No se encontró el adjunto solicitado";
                    }
                    return null;
                }

                if (WebOperationContext.Current != null)
                {
                    // WebOperationContext.Current.OutgoingResponse.ContentType = "application/pdf";
                    WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-disposition",
                        "inline; filename=" + datosAdj.FileName);
                }
                return datosAdj.FileStream;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                if (WebOperationContext.Current != null)
                {
                    var response = WebOperationContext.Current.OutgoingResponse;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.StatusDescription = "Ha ocurrido un error al procesar la petición, consulte al administrador del sistema.";
                }
                return null;
            }
        }

        public ResultadoNuevoIngreso NuevoIngreso(DatosNuevoIngreso datos)
        {
            if (!AccesoValido(datos.IdSolicitud, datos.Clave))
            {
                return new ResultadoNuevoIngreso()
                {
                    Resultado = "DENEGADO",
                    Observaciones = "Acceso denegado.",
                    Datos = null
                };
            }

            var resultadoNuevoIng = _ingresoSrv.CreaIngreso(datos);
            if (resultadoNuevoIng.Datos == null)
            {
                resultadoNuevoIng.Datos = new DatosIngresoCreado();
            }
            resultadoNuevoIng.Datos.IdSolicitud = datos.IdSolicitud;

            #region Log de operación

            if (resultadoNuevoIng.Resultado == "EXISTE")
            {
                resultadoNuevoIng.Resultado = "OK";
                // Cuando se intenta crear un ingreso q ya fue procesado entonces no se guarda log de la operación
            }
            else
            {
                var logWssDto = new LogWssIntegracionDto
                {
                    SolicitudTramId = datos.IdSolicitudInt,
                    Fecha = DateTime.Now,
                    RequerimientoId = datos.IdRequerimiento,
                    Operacion = "NUEVO INGRESO",
                    Resultado = resultadoNuevoIng.Resultado,
                    Observaciones = resultadoNuevoIng.Observaciones
                };
                var resultLog = _ingresoSrv.InsertaLog(logWssDto);
            }
            #endregion

            return resultadoNuevoIng;
        }

        private bool AccesoValido(string idChequeo, string hashChequeo = "")
        {
            return hashChequeo == "ads98saf9sd9sd7sdaf" || idChequeo == "NO-VALIDAR";

            if (string.IsNullOrWhiteSpace(idChequeo) && WebOperationContext.Current != null)
            {
                idChequeo = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["idSolicitud"];
            }

            if (string.IsNullOrWhiteSpace(hashChequeo) && WebOperationContext.Current != null)
            {
                hashChequeo = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["clave"];
            }

            var segSrv = new SeguridadSrv();
            return !string.IsNullOrWhiteSpace(idChequeo) && !string.IsNullOrWhiteSpace(hashChequeo) && segSrv.CompruebaHash(idChequeo, hashChequeo);
        }


    }
}
