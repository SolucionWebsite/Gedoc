using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Web;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Helpers.Logging;
using Gedoc.Interop.Wss.Data;
using Gedoc.Interop.Wss.Helpers;
using Gedoc.Service.DataAccess;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.Service.Sharepoint;
using DateTime = System.DateTime;

namespace Gedoc.Interop.Wss.Services
{
    public class IngresoSrv: IIngresoSrv
    {
        private readonly string _dateFormat = ConfigurationManager.AppSettings["DateFormat"] ?? "dd-MM-yyyy HH:mm";
        private readonly IRequerimientoService _reqSrv;
        private readonly IDespachoService _despSrv;
        private readonly IMantenedorService _mantSrv;

        public IngresoSrv(IRequerimientoService reqSrv, IDespachoService despSrv, IMantenedorService mantSrv)
        {
            _reqSrv = reqSrv;
            _despSrv = despSrv;
            _mantSrv = mantSrv;
        }

        #region Ingresos
        public DatosIngresoNuevo GetDatosIngreso(string numeroIng)
        {
            var datos = new DatosIngresoNuevo();
            try
            {
                var ingreso = _reqSrv.GetByDocumentoIngreso(numeroIng);

                if (ingreso == null)
                { // No encontrado
                    return datos;
                }
                else if (ingreso.Id == 0)
                { // Error
                    return null;
                }

                datos.NumeroIngreso = ingreso.DocumentoIngreso;
                datos.FechaIngreso = ingreso.FechaIngreso;
                datos.FechaIngresoStr = ingreso.FechaIngreso.ToString(_dateFormat);
                datos.Estado = ingreso.EstadoTitulo;
                datos.Etapa = ingreso.EtapaTitulo;
                datos.ProfesionalAsignado = ingreso.ProfesionalNombre;
                datos.UnidadTecnica = ingreso.UnidadTecnicaAsign?.Titulo;
                datos.UnidadTecnicaId = ingreso.UnidadTecnicaAsign?.Id ?? 0;

                // Se obtienen los despachos del ingreso
                var oficios = _despSrv.GetDespachosIngreso(new ParametrosGrillaDto<int> { Dato = ingreso.Id, Take = 100, Skip = 0, Sort = null }).Data;
                if (oficios?.Count > 0)
                {
                    var baseAddress = OperationContext.Current.Host.BaseAddresses[0].AbsoluteUri;
                    datos.Despachos = new List<DatosDespacho>();
                    var segSrv = new SeguridadSrv();
                    foreach (var oficio in oficios)
                    {
                        var fechaOficio = oficio.FechaEmisionOficio.GetValueOrDefault();
                        var numOficio = oficio.Id.ToString(); // oficio.NumeroDespacho;
                        var hash = segSrv.GetHash((SeguridadSrv.ClaveSeguridad ?? "") + numOficio);
                        var urlArchivo = baseAddress + "/json/despachoadj/" +
                                          numOficio +
                                          "?clave=" + hash;
                        datos.Despachos.Add(new DatosDespacho()
                        {
                            Numero = numOficio,
                            Materia = oficio.Materia,
                            FechaEmision = fechaOficio,
                            FechaEmisionStr = fechaOficio.ToString(_dateFormat),
                            UrlArchivo = urlArchivo
                        });
                    }
                }
                else
                {
                    // TODO: devolver datos del ingreso pero Error en los datos del despacho
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                return null;
            }


            return datos;
        }

        public ResultadoNuevoIngreso CreaIngreso(DatosNuevoIngreso datos)
        {
            var valida = ValidaDatosNuevoIng(datos);
            if (valida.Resultado != "OK")
            {
                return valida;
            }

            // Se verifica si existe el remitente
            var remitente = _mantSrv.GetRemitenteByRut(datos.RemitenteRut);
            if (remitente == null)
            {
                // Ocurrió un error con la consulta
                return new ResultadoNuevoIngreso()
                {
                    Resultado = "REINTENTAR",
                    Observaciones = "Ocurrió un error al comprobar existencia del remitente, por favor, reintentar la operación."
                };
            }
            if (remitente.Id == 0)
            {
                // No existe el remitente según el rut, se busca según el nombre pues la lista de remitentes no admite duplicados en el nombre
                // TODO: precisar qué hacer si hay dos remitentes diferentes pero q tengan el mismo nombre (ahora se toma como si fuera el mismo remitente)
                remitente = _mantSrv.GetRemitenteByNombre(datos.RemitenteNombre);
                if (remitente == null)
                {
                    // Ocurrió un error con la consulta
                    return new ResultadoNuevoIngreso()
                    {
                        Resultado = "REINTENTAR",
                        Observaciones = "Ocurrió un error al comprobar existencia del remitente, por favor, reintentar la operación."
                    };
                }
                if (remitente.Id == 0)
                {
                    // No existe el remitente según el nombre, ni tampoco según rut, se crea nuevo
                    var nuevoRemit = new RemitenteDto
                    {
                        Nombre = datos.RemitenteNombre,
                        Rut = datos.RemitenteRut,
                        Email = datos.RemitenteEmail,
                        Genero = datos.RemitenteGenero,
                        //Cargo = datos.RemitenteCargo,
                        Direccion = datos.RemitenteDireccion,
                        Telefono = datos.RemitenteTelefono,
                        Institucion = datos.RemitenteInstitucion
                    };
                    var resultadoNew = _mantSrv.SaveRemitente(nuevoRemit);
                    if (resultadoNew.Codigo < 1)
                    {
                        // Ocurrió un error al crear el remitente
                        return new ResultadoNuevoIngreso()
                        {
                            Resultado = "REINTENTAR",
                            Observaciones = "Ocurrió un error al crear el remitente, por favor, reintentar la operación."
                        };
                    }

                    remitente = nuevoRemit;
                }
            }

            datos.IdRemitente = remitente.Id;
            var nuevoReq = NuevoRequerimientoDatos(datos);
            var resultado = _reqSrv.Save(nuevoReq);
            datos.IdRequerimiento = nuevoReq.Id;
            datos.DocumentoIngreso = nuevoReq.DocumentoIngreso;
            if (resultado.Codigo < 0)
            {
                return new ResultadoNuevoIngreso()
                {
                    Observaciones = "Ha ocurrido un error al grabar los datos del nuevo requerimiento.",
                    Resultado = "REINTENTAR"
                };
            }
            return new ResultadoNuevoIngreso()
            {
                Resultado = "OK",
                Datos = new DatosIngresoCreado()
                {
                    FechaIngreso = nuevoReq.FechaIngreso,
                    FechaIngresoStr = nuevoReq.FechaIngreso.ToString(_dateFormat),
                    NumeroIngreso = nuevoReq.DocumentoIngreso
                }
            };

        }

        private RequerimientoDto NuevoRequerimientoDatos(DatosNuevoIngreso datoInyectado)
        {
            var nuevoReq = new RequerimientoDto()
            {
                Flujo = FlujoIngreso.NuevoRequerimientoWss,
                SolicitudTramId = datoInyectado.IdSolicitudInt
            };
            #region Estado, Etapa, Priorización según el Tipo de trámite especificado
            nuevoReq.EstadoId = datoInyectado.Estado;
            nuevoReq.EtapaId = datoInyectado.Etapa;
            // El cálculo de la prioridad se realizar en el RequerimientoService al grabar
            nuevoReq.PrioridadCod = datoInyectado.IdPrioridad;
            if (datoInyectado.UnidadTecnica > 0) // Siempre debe ser true, pero por si acaso
            {
                nuevoReq.UtAsignadaId = datoInyectado.UnidadTecnica;
                nuevoReq.AsignacionUt = DateTime.Now;
                #region Asignación de Responsable de UT
                var respUt = _reqSrv.GetResponsableUt(nuevoReq.UtAsignadaId.GetValueOrDefault(0));
                nuevoReq.ResponsableUtId = respUt.Id;
                #endregion
            }
            #endregion

            #region Valores por defecto
            nuevoReq.FechaIngreso = DateTime.Now;
            nuevoReq.TipoIngreso = "Nuevo ingreso";
            nuevoReq.CanalLlegadaTramiteCod = CanalLlegada.Web.ToString("D");
            nuevoReq.LiberarAsignacionTemp = false;
           // nuevoReq. ["Documento_x0020_de_x0020_ingreso"] = ""; *******************
            nuevoReq.SoloMesAnno = false;
            nuevoReq.RequiereRespuesta = true;
            #endregion

            #region Valores obligatorios de entrada enviados al servicio
            nuevoReq.FechaDocumento = datoInyectado.FechaDocumento;
            nuevoReq.RemitenteId = datoInyectado.IdRemitente;
            nuevoReq.TipoDocumentoCod = datoInyectado.IdTipoDoc;
            nuevoReq.TipoTramiteId = datoInyectado.IdTipoTramite;
            #endregion

            #region Valores opcionales de entrada enviados al servicio

            nuevoReq.ObservacionesTipoDoc = datoInyectado.ObservacionTipoDoc;
            nuevoReq.AdjuntaDocumentacion = datoInyectado.AdjuntaDoc;
            nuevoReq.CantidadAdjuntos = datoInyectado.CantAdjuntos;
            nuevoReq.TipoAdjunto = (datoInyectado.IdTipoAdj ?? new List<string>()).Select(id => new GenericoDto() {Id = id}).ToList();
            nuevoReq.Soporte = (datoInyectado.IdSoporte ?? new List<string>()).Select(id => new GenericoDto() { Id = id }).ToList();
            nuevoReq.ObservacionesAdjuntos = datoInyectado.ObservacionAdjuntos;
            nuevoReq.NombreProyectoPrograma = datoInyectado.NombreProyectoPrograma;
            nuevoReq.CasoId = datoInyectado.IdCaso;
            nuevoReq.Etiqueta = (datoInyectado.IdEtiqueta ?? new List<string>()).Select(id => new GenericoDto() { Id = id }).ToList();
            #region Datos Monumento Nacional
            var mn = new MonumentoNacionalDto();
            mn.CategoriaMonumentoNac = (datoInyectado.IdCategoriaMn ?? new List<string>()).Select(id => new GenericoDto() { Id = id }).ToList();
            mn.CodigoMonumentoNac = datoInyectado.CodigoMonNac;
            mn.DenominacionOficial = datoInyectado.DenominacionOficMonNac;
            mn.OtrasDenominaciones = datoInyectado.OtrasDenominacionesMonNac;
            mn.NombreUsoActual = datoInyectado.NombreUsoActualMonNac;
            mn.DireccionMonumentoNac = datoInyectado.DireccionMonNac;
            mn.ReferenciaLocalidad = datoInyectado.ReferenciaLocalidadMonNac;
            mn.Region = (datoInyectado.IdRegion ?? new List<string>()).Select(id => new GenericoDto() { Id = id }).ToList();
            mn.Provincia = (datoInyectado.IdProvincia ?? new List<string>()).Select(id => new GenericoDto() { Id = id }).ToList();
            mn.Comuna = (datoInyectado.IdComuna ?? new List<string>()).Select(id => new GenericoDto() { Id = id }).ToList();
            mn.RolSii = datoInyectado.Rol;
            nuevoReq.MonumentoNacional = mn;
            #endregion
            nuevoReq.FormaLlegadaCod = datoInyectado.IdFormaLlegada;
            nuevoReq.ObservacionesFormaLlegada = datoInyectado.ObservacionesFormaLlegada;
            nuevoReq.CaracterId = datoInyectado.CaracterId;
            nuevoReq.ObservacionesCaracter = datoInyectado.ObservacionesCaracter;
            nuevoReq.Redireccionado = datoInyectado.Redireccionado;
            nuevoReq.NumeroTicket = datoInyectado.NumeroTicket;
            nuevoReq.RequerimientoAnteriorId = datoInyectado.IdReqAnterior;
            nuevoReq.RequerimientoNoRegistrado = datoInyectado.RequermientoNoRegistrado;
            nuevoReq.Materia = datoInyectado.Materia;
            #endregion

            return nuevoReq;
        }

        private ResultadoNuevoIngreso ValidaDatosNuevoIng(DatosNuevoIngreso datos)
        {
            #region Validación id solicitud
            // Se admiten sólo numero en el id de solicitud
            if (!int.TryParse(datos.IdSolicitud, out datos.IdSolicitudInt))
            {
                return new ResultadoNuevoIngreso()
                {
                    Resultado = "ERROR",
                    Observaciones = "01,El Id de Solicitud tiene que ser un número entero.",
                    Datos = null
                };
            }
            // Si ya fue procesada una solicitud y se creó con éxito un ingreso entonces simplemente se devuelven los datos del ingreso
            var reqSolic = _reqSrv.GetBySolicitudId(datos.IdSolicitudInt);
            if (reqSolic?.Id > 0)
            {  // Existe un requerimiento asociado al id de trámites especificado
               // Se obtiene el log de la solicitud 
               var logWss = _mantSrv.GetLastLogWssBySolicitudId(datos.IdSolicitudInt);
               var obs = "";
               if (logWss != null && logWss.RequerimientoId.HasValue)
               { // Se encontró el log de creación del requerimiento asociado a la solicitud
                   obs = "La solicitud de id " + datos.IdSolicitud + " fue procesada en la fecha " +
                         logWss.Fecha.GetValueOrDefault().ToString("dd-MM-yyyy HH:mm");
               }
               else
               { // No se encontró el log de creación del requerimiento asociado a la solicitud
                    obs = "La solicitud de id " + datos.IdSolicitud + " fue procesada en la fecha " +
                          reqSolic.FechaIngreso.ToString("dd-MM-yyyy HH:mm");
               }
               return new ResultadoNuevoIngreso()
               {
                   Resultado = "EXISTE",
                   Observaciones = obs,
                   Datos = new DatosIngresoCreado()
                   {
                       FechaIngreso = reqSolic.FechaIngreso,
                       FechaIngresoStr = reqSolic.FechaIngreso.ToString(_dateFormat),
                       NumeroIngreso = reqSolic.DocumentoIngreso,
                       IdSolicitud = datos.IdSolicitud
                   }
               };
            }
            #endregion

            #region Validación de datos completos
            var campos = new List<string>();
            if (string.IsNullOrWhiteSpace(datos.RemitenteGenero)) campos.Add("RemitenteGenero");
            if (string.IsNullOrWhiteSpace(datos.RemitenteNombre)) campos.Add("RemitenteNombre");
            if (string.IsNullOrWhiteSpace(datos.RemitenteRut)) campos.Add("RemitenteRut");
            if (string.IsNullOrWhiteSpace(datos.TipoDocumento)) campos.Add("TipoDocumento");
            if (string.IsNullOrWhiteSpace(datos.TipoTramite)) campos.Add("TipoTramite");
            if (string.IsNullOrWhiteSpace(datos.Materia)) campos.Add("Materia");

            if (campos.Count > 0)
            {
                return new ResultadoNuevoIngreso()
                {
                    Resultado = "ERROR",
                    Observaciones = "01,Faltan datos obligatorios por especificar: " + string.Join(", ", campos.ToArray()),
                    Datos = null
                };
            }
            #endregion

            return ValidaDatosMantenedoresNuevoIngreso(datos);
        }

        private ResultadoNuevoIngreso ValidaDatosMantenedoresNuevoIngreso(DatosNuevoIngreso datos)
        {

            #region Validación Tipo de Trámite
            var tipoTramite = _mantSrv.GetTipoTramiteByCodigo(datos.TipoTramite);
            if (tipoTramite == null)
            {
                return new ResultadoNuevoIngreso()
                {
                    Resultado = "REINTENTAR",
                    Observaciones = "Ha ocurrido un error al validar el Tipo de Trámite especificado, por favor, reintentar la operacion.",
                    Datos = null
                };
            }
            else if (tipoTramite.Id == 0)
            {
                return new ResultadoNuevoIngreso()
                {
                    Resultado = "ERROR",
                    Observaciones = "01,No se encontró en GEDOC el valor de Tipo de Trámite especificado.",
                    Datos = null
                };
            }
            #endregion

            #region Unidad Técnica
            // [26/05/2020] - Se agrega como parametro obligatorio a la inyección la UT (CodigoUt). Al tener una UT asignada el requermiento
            // se ignora la UT configurada en el Tipo de Trámite.
            // [20/07/2020] - Se quita la obligatoriedad del parametro CodigoUt
            datos.UnidadTecnica = null;
            datos.UnidadTecnicaTitulo = null;
            if (!string.IsNullOrWhiteSpace(datos.CodigoUt))
            {
                Int32.TryParse(datos.CodigoUt, out var idUtInt);
                var ut = _mantSrv.GetUnidadTecnicaByUtTramiteId(idUtInt);
                if (ut == null || ut.Id == 0)
                {
                    return new ResultadoNuevoIngreso()
                    {
                        Resultado = "ERROR",
                        Observaciones = "01,No se encontró en GEDOC el valor de CodigoUt especificado.",
                        Datos = null
                    };
                }
                datos.UnidadTecnica = ut.Id;
                datos.UnidadTecnicaTitulo = ut.Titulo;
            }
            if (ConfigurationManager.AppSettings["IgnoraConfigTipoTramite"] == "1")
            {
                // Se ignora la configuración existente en Tipo de Trámite en relación a Etapa, Estado, Ut y Priorización
                // y se asigna por defecto Estado = Asignado y Etapa = Unidad Técnica
                datos.Estado = datos.UnidadTecnica.HasValue ? (int)EstadoIngreso.Asignado : (int)EstadoIngreso.Ingresado;
                datos.Etapa = datos.UnidadTecnica.HasValue ? (int)EtapaIngreso.UnidadTecnica : (int)EtapaIngreso.Asignacion;
                datos.IdPrioridad = null;
            }
            else
            {
                datos.UnidadTecnica = datos.UnidadTecnica ?? tipoTramite.UnidadTecnicaId;
                datos.UnidadTecnicaTitulo = datos.UnidadTecnicaTitulo ?? tipoTramite.UnidadTecnicaTitulo;
                datos.Estado = tipoTramite.EstadoId ?? (datos.UnidadTecnica.HasValue ? (int)EstadoIngreso.Asignado : (int)EstadoIngreso.Ingresado);
                datos.Etapa =  tipoTramite.EtapaId ?? (datos.UnidadTecnica.HasValue ? (int)EtapaIngreso.UnidadTecnica : (int)EtapaIngreso.Asignacion);
                datos.IdPrioridad = tipoTramite.PrioridadCod;
            }
            datos.IdTipoTramite = tipoTramite.Id;
            #endregion

            #region Validación Tipo de Documento. Se inyecta el título, se busca el id.
            var datosGen = _mantSrv.GetGenericoMatenedor(Mantenedor.TipoDocumento).Data ?? new List<GenericoDto>();
            datos.IdTipoDoc = datosGen.FirstOrDefault(d => d.Titulo.ToLower().Trim() == datos.TipoDocumento.ToLower().Trim())?.Id ?? "";
            if (string.IsNullOrWhiteSpace(datos.IdTipoDoc))
            {
                return new ResultadoNuevoIngreso()
                {
                    Resultado = "ERROR",
                    Observaciones = "02,No se encontró en GEDOC el valor de Tipo de Documento especificado.",
                    Datos = null
                }; ;
            }
            #endregion

            #region Validación Tipo de Adjunto, es multiseleccion. Se inyectan los títulos, se buscan los ids.
            var datoArr = datos.TipoAdjuntos ?? new string[] { };
            //datoArr = datoArr.Length == 1 ? string.IsNullOrWhiteSpace(datoArr[0]) ? new string[] { } : datoArr;
            if (datoArr.Length > 0)
            {
                datos.IdTipoAdj = new List<string>();
                datosGen = _mantSrv.GetGenericoMatenedor(Mantenedor.TipoDocumento).Data ?? new List<GenericoDto>();
                foreach (var valor in datoArr)
                {
                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        var d = datosGen.FirstOrDefault(s => s.Titulo.ToLower().Trim() == valor.ToLower().Trim());
                        if (d == null)
                        {
                            return new ResultadoNuevoIngreso()
                            {
                                Resultado = "ERROR",
                                Observaciones = $"03,No se encontró en GEDOC el valor {valor} especificado en Tipo de Adjunto.",
                                Datos = null
                            };
                        }
                        datos.IdTipoAdj.Add(d.Id);
                    }
                }
            }
            #endregion

            #region Validación Soporte, es multiseleccion. Se inyectan los títulos, se buscan los ids.
            datoArr = datos.Soporte ?? new string[]{};
            if (datoArr.Length > 0)
            {
                datos.IdSoporte = new List<string>();
                datosGen = _mantSrv.GetGenericoMatenedor(Mantenedor.Soporte).Data ?? new List<GenericoDto>();
                foreach (var valor in datoArr)
                {
                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        var sop = datosGen.FirstOrDefault(s => s.Titulo.ToLower().Trim() == valor.ToLower().Trim());
                        if (sop == null)
                        {
                            return new ResultadoNuevoIngreso()
                            {
                                Resultado = "ERROR",
                                Observaciones = $"04,No se encontró en GEDOC el valor {valor} especificado en Soporte.",
                                Datos = null
                            };
                        }

                        datos.IdSoporte.Add(sop.Id);
                    }
                }
            }
            #endregion

            #region Validación genero de remitente
            if (datos.RemitenteGenero != "Femenino" && datos.RemitenteGenero != "Masculino" && datos.RemitenteGenero != "Neutro")
            {
                return new ResultadoNuevoIngreso()
                {
                    Resultado = "ERROR",
                    Observaciones = "05,Valor de Género incorrecto. Posibles valores: Femenino, Masculino, Neutro.",
                    Datos = null
                };
            }
            #endregion

            #region Validación caracter. Se inyecta el título, se busca el id.
            switch (datos.Caracter)
            {
                case "Público":
                    datos.CaracterId = (int) Caracter.Publico;
                    break;
                case "Reservado":
                    datos.CaracterId = (int)Caracter.Reservado;
                    break;
                case "Secreto":
                    datos.CaracterId = (int)Caracter.Secreto;
                    break;
                default:
                    return new ResultadoNuevoIngreso()
                    {
                        Resultado = "ERROR",
                        Observaciones = "20,Valor de Caracter incorrecto. Posibles valores: Público, Reservado, Secreto.",
                        Datos = null
                    };
            }
            #endregion

            #region Validación Rut
            datos.RemitenteRut = RutHelper.FormateaRut(datos.RemitenteRut);
            if (!RutHelper.ValidaRut(datos.RemitenteRut))
            {
                return new ResultadoNuevoIngreso()
                {
                    Resultado = "ERROR",
                    Observaciones = "06,El RUT especificado no es válido.",
                    Datos = null
                };
            }
            #endregion

            #region Validación Nombre de Caso. Se inyecta el nombre, se busca el id.
            if (!string.IsNullOrWhiteSpace(datos.NombreCaso))
            {
                datosGen = _mantSrv.GetGenericoMatenedor(Mantenedor.Caso).Data ?? new List<GenericoDto>();
                datos.IdCaso = datosGen.FirstOrDefault(d => d.Titulo.ToLower().Trim() == datos.NombreCaso.ToLower().Trim())?.IdInt ?? 0;
                if (datos.IdCaso == 0)
                {
                    return new ResultadoNuevoIngreso()
                    {
                        Resultado = "ERROR",
                        Observaciones = "07,No se encontró en GEDOC el valor de Nombre del Caso especificado.",
                        Datos = null
                    }; ;
                }
            }
            #endregion

            #region Validación Etiqueta,  es multiseleccion. Se inyectan los título, se buscan los ids.
            datoArr = datos.Etiqueta ?? new string[] { };
            if (datoArr.Length > 0)
            {
                datos.IdEtiqueta = new List<string>();
                datosGen = _mantSrv.GetGenericoMatenedor(Mantenedor.Etiqueta).Data ?? new List<GenericoDto>();
                foreach (var valor in datoArr)
                {
                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        var etiq = datosGen.FirstOrDefault(s => s.Titulo.ToLower().Trim() == valor.ToLower().Trim());
                        if (etiq == null)
                        {
                            return new ResultadoNuevoIngreso()
                            {
                                Resultado = "ERROR",
                                Observaciones =
                                    $"08,No se encontró en GEDOC el valor {valor} especificado en Etiqueta.",
                                Datos = null
                            };
                        }

                        datos.IdEtiqueta.Add(etiq.Id);
                    }
                }
            }
            #endregion

            #region Validación Categoría Monumento Nacional,  es multiseleccion. Se inyectan los código (MH, SN, ZT, etc), se buscan los ids.
            datoArr = datos.CategoriaMonNac ?? new string[] { };
            if (datoArr.Length > 0)
            {
                datos.IdCategoriaMn = new List<string>();
                datosGen = _mantSrv.GetGenericoMatenedor(Mantenedor.CategoriaMn).Data ?? new List<GenericoDto>();
                foreach (var valor in datoArr)
                {
                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        var d = datosGen.FirstOrDefault(s => s.ExtraData.ToLower().Trim() == valor.ToLower().Trim()); // en ExtraData está el cñodifo de la categoría
                        if (d == null)
                        {
                            return new ResultadoNuevoIngreso()
                            {
                                Resultado = "ERROR",
                                Observaciones =
                                    $"09,No se encontró en GEDOC el valor {valor} especificado en Categoría Monumento Nacional.",
                                Datos = null
                            };
                        }

                        datos.IdCategoriaMn.Add(d.Id);
                    }
                }
            }
            #endregion

            #region Validación Forma de llegada. Se inyecta el nombre, se busca el id.
            if (!string.IsNullOrWhiteSpace(datos.FormaLlegada))
            {
                var datosForm = _mantSrv.GetGenericoMatenedor(Mantenedor.FormaLlegada).Data ?? new List<GenericoDto>();
                datos.IdFormaLlegada = datosForm.FirstOrDefault(d => d.Titulo.ToLower().Trim() == datos.FormaLlegada.ToLower().Trim())?.Id ?? null;
                if (string.IsNullOrWhiteSpace(datos.IdFormaLlegada))
                {
                    return new ResultadoNuevoIngreso()
                    {
                        Resultado = "ERROR",
                        Observaciones = "10,No se encontró en GEDOC el valor de Forma de Llegada especificado.",
                        Datos = null
                    }; ;
                }
            }
            #endregion

            #region Validación Región,  es multiseleccion. Se inyectan los códigos, se buscan los ids.
            datoArr = datos.CodigoRegion ?? new string[] { };
            if (datoArr.Length > 0)
            {
                var regiones = datoArr.ToList().Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
                if (regiones?.Count > 0)
                {
                    datos.IdRegion = new List<string>();
                    datosGen = _mantSrv.GetRegionesByCodigos(regiones) ?? new List<GenericoDto>();
                    if (datosGen.Count == 0)
                    {
                        return new ResultadoNuevoIngreso()
                        {
                            Resultado = "ERROR",
                            Observaciones = $"11,No se encontró en GEDOC el valor especificado en Región.",
                            Datos = null
                        };
                    }
                    datos.IdRegion = datosGen.Select(d => d.Id).ToList();
                }
            }
            #endregion

            #region Validación Provincia,  es multiseleccion. Se inyectan los códigos, se buscan los ids.
            datoArr = datos.CodigoProvincia ?? new string[] { };
            if (datoArr.Length > 0)
            {
                var provincias = datoArr.ToList().Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
                if (provincias?.Count > 0)
                {
                    datos.IdProvincia = new List<string>();
                    datosGen = _mantSrv.GetProvinciasByCodigos(provincias) ?? new List<GenericoDto>();
                    if (datosGen.Count == 0)
                    {
                        return new ResultadoNuevoIngreso()
                        {
                            Resultado = "ERROR",
                            Observaciones = $"12,No se encontró en GEDOC el valor especificado en Provincia.",
                            Datos = null
                        };
                    }
                    datos.IdProvincia = datosGen.Select(d => d.Id).ToList();
                }
            }
            #endregion

            #region Validación Comuna,  es multiseleccion. Se inyectan los códigos, se buscan los ids.
            datoArr = datos.CodigoComuna ?? new string[] { };
            if (datoArr.Length > 0)
            {
                var comunas = datoArr.ToList().Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
                if (comunas?.Count > 0)
                {
                    datos.IdComuna = new List<string>();
                    datosGen = _mantSrv.GetComunasByCodigos(comunas) ?? new List<GenericoDto>();
                    if (datosGen.Count == 0)
                    {
                        return new ResultadoNuevoIngreso()
                        {
                            Resultado = "ERROR",
                            Observaciones = $"13,No se encontró en GEDOC el valor especificado en Comuna.",
                            Datos = null
                        };
                    }
                    datos.IdComuna = datosGen.Select(d => d.Id).ToList();
                }
            }
            #endregion


            #region Validación Requerimiento Anterior. Se inyecta el número de documento de ingreso, se busca el id.
            if (!string.IsNullOrWhiteSpace(datos.RequermientoAnterior))
            {
                var datosReqAnt = _reqSrv.GetResumenByDocumentoIngreso(datos.RequermientoAnterior) ?? new RequerimientoDto();
                if (datosReqAnt.Id == 0)
                {
                    return new ResultadoNuevoIngreso()
                    {
                        Resultado = "ERROR",
                        Observaciones = "14,No se encontró en GEDOC el valor de Requerimiento Anterior especificado.",
                        Datos = null
                    }; ;
                }

                datos.IdReqAnterior = datosReqAnt.Id;
            }
            #endregion

            return new ResultadoNuevoIngreso()
            {
                Resultado = "OK"
            };
        }
        #endregion

        #region Despachos
        public DatosAdjunto GetAdjuntoDespacho(string numero)
        {

            var adjData = new DatosAdjunto();

            if (!int.TryParse(numero, out var idDespacho))
            {
                return null;
            }
            var datosAdj = _despSrv.GetArchivo(idDespacho);

            //var spClient = new SharePointClient();
            //var query = "<View Scope=\"RecursiveAll\"><Query>" +
            //            "<Where>" +
            //            "  <Eq>" +
            //            "    <FieldRef Name='ID' />" +
            //            "    <Value Type=\'Counter\'>" + numero + "</Value>" +
            //            "  </Eq>" +
            //            "</Where>" +
            //            "</Query></View>";
            //var oficios = spClient.GetItemsListaSP("Despachos", query);

            if (datosAdj.FileStream == null)
            {
                return null;
            }
            else
            {
                adjData.FileName = datosAdj.FileName;
                adjData.FileStream = datosAdj.FileStream;
            }

            return adjData;
        }
        #endregion

        #region Estado y datos de solicitud

        public ResultadoEstadoSolicitud GetEstadoSolicitud(int idSolicitud)
        {
            try
            {
                var logSol = _mantSrv.GetLastLogWssBySolicitudId(idSolicitud, "NUEVO INGRESO", "");
                GenericoDto req = null;
                if (logSol?.Id > 0)
                {
                    var reqs = _reqSrv.GetRequerimientoResumenByIds(new List<int>{ { logSol.RequerimientoId.GetValueOrDefault() } }, false)?.Data;
                    req = reqs?.Count > 0 ? reqs[0] : new GenericoDto();
                }

                return new ResultadoEstadoSolicitud()
                {
                    Resultado = logSol == null ? "ERROR" : (logSol.Id == 0 ? "NOTFOUND" : logSol.Resultado),
                    Observaciones = logSol?.Observaciones ?? "",
                    Datos = logSol == null
                        ? null
                        : new DatosEstadoSolicitud()
                        {
                            NumeroIngreso = req?.Titulo,
                            IdSolicitud = idSolicitud.ToString()
                        }
                };

            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                return new ResultadoEstadoSolicitud()
                {
                    Resultado = "ERRORINTERNO",
                    Observaciones = "Error interno al realizar la operación.",
                    Datos = null
                };
            }
        }
        #endregion

        public ResultadoOperacion InsertaLog(LogWssIntegracionDto log)
        {
            var resultado = new ResultadoOperacion(1, "OK");
            try
            {
                log.Operacion = AcortaTexto(log.Operacion, 20);
                log.Resultado = AcortaTexto(log.Resultado, 20);
                log.Observaciones = AcortaTexto(log.Observaciones, 1000);
                var result = _mantSrv.SaveLogWss(log);
                resultado.Codigo = result.Codigo;
                resultado.Texto = result.Mensaje;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                resultado.Codigo = -1;
                resultado.Texto = "Error realizando la operación.";
            }
            return resultado;
        }

        private string AcortaTexto(string texto, int largo)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }
            return texto.Substring(0, texto.Length > largo ? largo : texto.Length);
        }


    }
}