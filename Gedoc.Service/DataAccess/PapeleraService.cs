using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;
using Gedoc.Service.DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel.Channels;
using Gedoc.Helpers.Enum;
using Gedoc.Helpers.Logging;

namespace Gedoc.Service.DataAccess
{
    public class PapeleraService : BaseService, IPapeleraService
    {
        public DatosAjax<List<PapeleraItemDto>> GetDataPapelera(ParametrosGrillaDto<int> param, int userId)
        {
            var resultado = new DatosAjax<List<PapeleraItemDto>>(new List<PapeleraItemDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos de la papelera.", null));
            try
            {
                var idBandeja = param.Dato;
                using (var db = new GedocEntities())
                {
                    #region Requerimiento
                    var requerimientosEliminados = GetRequerimientosPapelera(db, userId)
                                   .Select(a => new PapeleraItemDto()
                                   {
                                       Id = a.Id,
                                       CreadoPor = a.UsuarioCreacion?.NombresApellidos,
                                       EliminadoPor = a.UsuarioEliminacion?.NombresApellidos,
                                       FechaEliminacion = a.EliminacionFecha ?? DateTime.MinValue,
                                       Nombre = a.DocumentoIngreso,
                                       OrigenId = (int)PapeleraOrigenEnum.Requerimiento,
                                       Tamaño = "N/A",
                                       TipoObjetoId = (int)PapeleraTypeIcon.Registro,
                                       UbicacionOriginal = "/Requerimiento", //TODO: quieren saber de que bandeja era...
                                   });
                    resultado.Data.AddRange(requerimientosEliminados);
                    #endregion

                    #region Despacho
                    var despachosEliminados = GetDespachosPapelera(db, userId)
                                    .Select(a => new PapeleraItemDto()
                                    {
                                        Id = a.Id,
                                        CreadoPor = a.Usuario2?.NombresApellidos,
                                        EliminadoPor = a.Usuario1?.NombresApellidos,
                                        FechaEliminacion = a.EliminacionFecha ?? DateTime.MinValue,
                                        Nombre = a.NumeroDespacho, 
                                        OrigenId = (int)PapeleraOrigenEnum.Despacho,
                                        Tamaño = "N/A",
                                        TipoObjetoId = (int)PapeleraTypeIcon.Registro,
                                        UbicacionOriginal = "/Despacho",
                                    });
                    resultado.Data.AddRange(despachosEliminados);
                    #endregion

                    #region Adjunto
                    var adjuntoEliminado = GetAdjuntosPapelera(db, userId)
                                   .Select(a => new PapeleraItemDto()
                                   {
                                       Id = a.Id,
                                       CreadoPor = a.UsuarioCreacion?.NombresApellidos,
                                       EliminadoPor = a.Usuario?.NombresApellidos,
                                       FechaEliminacion = a.EliminacionFecha ?? DateTime.MinValue,
                                       Nombre = string.IsNullOrEmpty(a.NombreArchivo) ? "N/A" : a.NombreArchivo,
                                       OrigenId = (int)PapeleraOrigenEnum.Adjunto,
                                       Tamaño = a.FileSize < 1000 ? $"{a.FileSize} B" : $"{Convert.ToInt32(a.FileSize / 1000)} kB",
                                       TipoObjetoId = (int)PapeleraTypeIcon.ArchivoDocumento,
                                       UbicacionOriginal = string.IsNullOrEmpty(a.UrlArchivo) ? "Adjunto Requerimiento" : a.UrlArchivo,                                       
                                   });
                    resultado.Data.AddRange(adjuntoEliminado);
                    #endregion

                    #region Adjunto de Oficio
                    var adjuntoOficEliminado = GetAdjuntosOficioPapelera(db, userId)
                        .Select(a => new PapeleraItemDto()
                        {
                            Id = a.Id,
                            CreadoPor = a.UsuarioCreacion?.NombresApellidos,
                            EliminadoPor = a.UsuarioEliminacion?.NombresApellidos,
                            FechaEliminacion = a.EliminacionFecha ?? DateTime.MinValue,
                            Nombre = string.IsNullOrEmpty(a.NombreArchivo) ? "N/A" : a.NombreArchivo,
                            OrigenId = (int)PapeleraOrigenEnum.AdjuntoOficio,
                            Tamaño = a.FileSize < 1000 ? $"{a.FileSize} B" : $"{Convert.ToInt32(a.FileSize / 1000)} kB",
                            TipoObjetoId = (int)PapeleraTypeIcon.ArchivoDocumento,
                            UbicacionOriginal = string.IsNullOrEmpty(a.UrlArchivo) ? "Adjunto Oficio" : a.UrlArchivo,
                        });
                    resultado.Data.AddRange(adjuntoOficEliminado);
                    #endregion

                    #region Bitacora
                    var bitacoraEliminado = GetBitacorasPapelera(db, userId)
                                   .Select(a => new PapeleraItemDto()
                                   {
                                       Id = a.Id,
                                       CreadoPor = a.UsuarioCreacion?.NombresApellidos,
                                       EliminadoPor = a.Usuario1?.NombresApellidos,
                                       FechaEliminacion = a.EliminacionFecha ?? DateTime.MinValue,
                                       Nombre = a.TipoBitacora?.Titulo,
                                       OrigenId = (int)PapeleraOrigenEnum.Bitacora,
                                       Tamaño = "N/A",
                                       TipoObjetoId = (int)PapeleraTypeIcon.Registro,
                                       UbicacionOriginal = string.Format("/Requerimiento {0}/Bitacora", a.Requerimiento?.DocumentoIngreso),
                                   });
                    resultado.Data.AddRange(bitacoraEliminado);
                    #endregion

                    #region DespachoIniciativa
                    var despachoIniciativaEliminado = GetDespachoIniciativasPapelera(db, userId)
                                  .Select(a => new PapeleraItemDto()
                                  {
                                      Id = a.Id,
                                      CreadoPor = a.Usuario?.NombresApellidos,
                                      EliminadoPor = a.Usuario1?.NombresApellidos,
                                      FechaEliminacion = a.EliminacionFecha ?? DateTime.MinValue,
                                      Nombre = a.NumeroDespacho,
                                      OrigenId = (int)PapeleraOrigenEnum.DespachoIniciativa,
                                      Tamaño = "N/A",
                                      TipoObjetoId = (int)PapeleraTypeIcon.Registro,  
                                      UbicacionOriginal = "/Despacho Iniciativa",
                                  });
                    resultado.Data.AddRange(despachoIniciativaEliminado);
                    #endregion

                    #region Plantilla de Oficio
                    var plantillaOficioEliminado = GetPlantillaOficioPapelera(db, userId)
                                  .Select(a => new PapeleraItemDto()
                                  {
                                      Id = a.Id,
                                      CreadoPor = a.UsuarioCreacion?.NombresApellidos,
                                      EliminadoPor = a.UsuarioEliminacion?.NombresApellidos,
                                      FechaEliminacion = a.EliminacionFecha ?? DateTime.MinValue,
                                      Nombre = a.Nombre,
                                      OrigenId = (int)PapeleraOrigenEnum.PlantillaOficio,
                                      Tamaño = "N/A",
                                      TipoObjetoId = (int)PapeleraTypeIcon.Registro,  
                                      UbicacionOriginal = "/Plantilla Oficio",
                                  });
                    resultado.Data.AddRange(plantillaOficioEliminado);
                    #endregion

                    #region Plantilla de Oficio
                    var oficioEliminado = GetOficioPapelera(db, userId)
                                  .Select(a => new PapeleraItemDto()
                                  {
                                      Id = a.Id,
                                      CreadoPor = a.UsuarioCreacion?.NombresApellidos,
                                      EliminadoPor = a.UsuarioEliminacion?.NombresApellidos,
                                      FechaEliminacion = a.EliminacionFecha ?? DateTime.MinValue,
                                      Nombre = a.Id.ToString(),
                                      OrigenId = (int)PapeleraOrigenEnum.Oficio,
                                      Tamaño = "N/A",
                                      TipoObjetoId = (int)PapeleraTypeIcon.Registro,  
                                      UbicacionOriginal = "/Oficio",
                                  });
                    resultado.Data.AddRange(oficioEliminado);
                    #endregion

                    #region Remitente/Destinatario
                    var remiEliminado = GetRemitentePapelera(db, userId)
                        .Select(a => new PapeleraItemDto()
                        {
                            Id = a.Id,
                            CreadoPor = a.UsuarioCreacion?.NombresApellidos,
                            EliminadoPor = a.UsuarioEliminacion?.NombresApellidos,
                            FechaEliminacion = a.FechaEliminacion ?? DateTime.MinValue,
                            Nombre = a.Nombre,
                            OrigenId = (int)PapeleraOrigenEnum.Remitente,
                            Tamaño = "N/A",
                            TipoObjetoId = (int)PapeleraTypeIcon.Registro,
                            UbicacionOriginal = "/Remitente",
                        });
                    resultado.Data.AddRange(remiEliminado);
                    #endregion

                    resultado.Resultado.Mensaje = "OK";
                    resultado.Resultado.Codigo = 1;
                    resultado.Resultado.Extra = null;
                    resultado.Total = resultado.Data.Count();
                }
                return resultado;
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de la Papelera.");
            }
            return resultado;
        }

        public ResultadoOperacion DeleteTrashItems(List<PapeleraItemDto> items)
        {
            var resultado = new ResultadoOperacion()
            {
                Codigo = -1,
                Extra = null,
                Mensaje = "Ha ocurrido un error al eliminar el registro."
            };

            try
            {
                foreach (var item in items)
                {
                    switch (item.OrigenId)
                    {
                        case (int)PapeleraOrigenEnum.Oficio:
                            #region Oficio
                            using (var db = new GedocEntities())
                            {
                                var itemForDelete = db.Oficio.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForDelete != null)
                                {
                                    itemForDelete.Requerimiento.Clear();
                                    itemForDelete.OficioObservacion.Clear();
                                    db.Oficio.Remove(itemForDelete);
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue eliminado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a eliminar
                                }
                            }
                            break;
                        #endregion
                        case (int)PapeleraOrigenEnum.PlantillaOficio:
                            #region Plantilla de Oficio
                            using (var db = new GedocEntities())
                            {
                                var itemForDelete = db.PlantillaOficio.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForDelete != null)
                                {
                                    db.PlantillaOficio.Remove(itemForDelete);
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue eliminado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a eliminar
                                }
                            }
                            break;
                        #endregion
                        case (int)PapeleraOrigenEnum.Despacho:
                            #region Despacho
                            using (var db = new GedocEntities())
                            {
                                var itemForDelete = db.Despacho.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForDelete != null)
                                {
                                    itemForDelete.Requerimiento.Clear();
                                    itemForDelete.DestinatarioCopia.Clear();
                                    itemForDelete.TipoAdjunto.Clear();
                                    itemForDelete.Soporte.Clear();
                                    db.Despacho.Remove(itemForDelete);

                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue eliminado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a eliminar
                                }
                            }
                            break;
                        #endregion
                        case (int)PapeleraOrigenEnum.DespachoIniciativa:
                            #region DespachoIniciativa
                            using (var db = new GedocEntities())
                            {
                                var itemForDelete = db.DespachoIniciativa.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForDelete != null)
                                {
                                    itemForDelete.Etiqueta.Clear();
                                    itemForDelete.DestinatarioCopia.Clear();
                                    itemForDelete.TipoAdjunto.Clear();
                                    itemForDelete.Soporte.Clear();
                                    db.DespachoIniciativa.Remove(itemForDelete);
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue eliminado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a eliminar
                                }
                            }
                            break;
                        #endregion
                        case (int)PapeleraOrigenEnum.Adjunto:
                            #region Adjunto
                            using (var db = new GedocEntities())
                            {
                                var itemForDelete = db.Adjunto.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForDelete != null)
                                {
                                    itemForDelete.TipoAdjunto.Clear();
                                    db.SaveChanges();
                                    db.Adjunto.Remove(itemForDelete);
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue eliminado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a eliminar
                                }
                            }
                            break;
                        #endregion
                        case (int)PapeleraOrigenEnum.AdjuntoOficio:
                            #region Adjunto Oficio
                            using (var db = new GedocEntities())
                            {
                                var itemForDelete = db.AdjuntoOficio.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForDelete != null)
                                {
                                    itemForDelete.TipoAdjunto.Clear();
                                    db.SaveChanges();
                                    db.AdjuntoOficio.Remove(itemForDelete);
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue eliminado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a eliminar
                                }
                            }
                            break;
                        #endregion
                        case (int)PapeleraOrigenEnum.Bitacora:
                            #region Bitacora
                            using (var db = new GedocEntities())
                            {
                                var itemForDelete = db.Bitacora.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForDelete != null)
                                {
                                    db.Bitacora.Remove(itemForDelete);
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue eliminado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe la bitacora a eliminar
                                }
                            }
                            break;
                        #endregion
                        case (int)PapeleraOrigenEnum.Requerimiento:
                            #region Requerimiento
                            using (var db = new GedocEntities())
                            {
                                var itemForDelete = db.Requerimiento.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForDelete != null)
                                {
                                    // Se eliminan las bitácoras del Requerimiento, estén o no en la papelera
                                    var bitacoras = db.Bitacora.Where(a => a.RequerimientoId == itemForDelete.Id);
                                    foreach (var bitacora in bitacoras)
                                    {
                                        db.Bitacora.Remove(bitacora);
                                    }
                                    // Se eliminan los adjuntos del Requerimiento, estén o no en la papelera
                                    var adjuntos = db.Adjunto.Where(a => a.RequerimientoId == itemForDelete.Id);
                                    foreach (var adjunto in adjuntos)
                                    {
                                        adjunto.TipoAdjunto.Clear();
                                        db.Adjunto.Remove(adjunto);
                                    }
                                    // Se elimina el requerimiento de las Tablas de sesión donde esté
                                    var tablaDetalle = db.SesionTablaDetalle.Where(a => a.RequerimentoId == itemForDelete.Id);
                                    foreach (var tabla in tablaDetalle)
                                    {
                                        db.SesionTablaDetalle.Remove(tabla);
                                    }
                                    // TODO: ¿eliminar el requerimiento en otras tablas q tengan FK con Requerimiento?
                                    // Se elimina el Requerimiento
                                    itemForDelete.Caso1.Clear();
                                    itemForDelete.Etiqueta.Clear();
                                    itemForDelete.Soporte.Clear();
                                    itemForDelete.TipoAdjunto.Clear();
                                    itemForDelete.UnidadTecnicaCopia.Clear();
                                    db.Requerimiento.Remove(itemForDelete);
                                    //TODO: quizás hay q eliminar en cascada
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue eliminado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a eliminar
                                }
                            }
                            break;
                        #endregion
                        case (int)PapeleraOrigenEnum.Remitente:
                            #region Remitente
                            using (var db = new GedocEntities())
                            {
                                db.sp_CambiaEstadoRemitente("E", item.Id, null, "");
                            }
                            break;
                        #endregion
                    }
                }

                if (1 == 1) //TODO: validar si no hay errores
                {
                    resultado.Codigo = 1;
                    resultado.Mensaje = "Se eliminaron todos los items seleccionados.";
                }
                //else if (1 == 2) //TODO: validar si hay algunos errores
                //{
                //    resultado.Codigo = 1;
                //    resultado.Mensaje = "Se eliminaron Algunos items";

                //}
                //else //TODO: validar si hay errores
                //{
                //    resultado.Codigo = 1;
                //    resultado.Mensaje = "Lo sentimos, ha ocurrido un error al eliminar los datos de la Papelera";
                //}
            }
            catch (DbUpdateException ex)
            {
                SqlException innerException = ex.InnerException.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 547))
                {
                    resultado = new ResultadoOperacion(-1,
                        "Ocurrió un error al eliminar el registro, se encuentra referenciado en <br/>registros de otras tablas de la base de datos.", null);
                }
                else
                {
                    resultado = new ResultadoOperacion(-1,
                        "Lo sentimos, ha ocurrido un error al eliminar los datos de la Papelera.< br/>Por favor, chequee el log de error de la aplicación.", null);
                }
                LogError(null, ex, resultado.Mensaje);
            }
            catch (Exception ex)
            {
                resultado.Extra = ex.Message;
                LogError(resultado, ex, "Lo sentimos, ha ocurrido un error al eliminar los datos de la Papelera.");
            }
            return resultado;
        }

        public ResultadoOperacion EmptyTrash(int userId)
        {
            var mensajes = new List<PapeleraMensajes>();
            var resultado = new ResultadoOperacion();            
            try
            {
                using (var db = new GedocEntities())
                {
                    //TODO: evaluar si esta acción debería hacerse con una transacción.

                    #region Oficio
                    try
                    {
                        var itemsForDelete = GetOficioPapelera(db, userId);
                        foreach (var item in itemsForDelete)
                        {
                            item.Requerimiento.Clear();
                            item.OficioObservacion.Clear();
                            db.Oficio.Remove(item);
                        }

                        if (itemsForDelete.Count > 0)
                        {
                            db.SaveChanges();
                            mensajes.Add(new PapeleraMensajes()
                            {
                                Codigo = 1,
                                Mensaje = "Oficio Eliminadas"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(null, ex, "Error eliminando Oficios de la papelera.");
                        mensajes.Add(new PapeleraMensajes()
                        {
                            Codigo = -1,
                            Mensaje = "Error al eliminar Oficios"
                        });
                    }
                    #endregion

                    #region Plantilla de Oficio
                    try
                    {
                        var itemsForDelete = GetPlantillaOficioPapelera(db, userId);
                        foreach (var item in itemsForDelete)
                        {
                            db.PlantillaOficio.Remove(item);
                        }
                        if (itemsForDelete.Count > 0)
                        {
                            db.SaveChanges();
                            mensajes.Add(new PapeleraMensajes()
                            {
                                Codigo = 1,
                                Mensaje = "Plantillas de Oficio Eliminadas"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(null, ex, "Error eliminando Plantillas de Oficio de la papelera.");
                        mensajes.Add(new PapeleraMensajes()
                        {
                            Codigo = -1,
                            Mensaje = "Error al eliminar Plantillas de Oficios"
                        });
                    }
                    #endregion

                    #region Despacho
                    {
                        try
                        {
                            var itemsForDelete = GetDespachosPapelera(db, userId);
                            foreach (var item in itemsForDelete)
                            {
                                item.Requerimiento.Clear();
                                item.DestinatarioCopia.Clear();
                                item.TipoAdjunto.Clear();
                                item.Soporte.Clear();
                                db.Despacho.Remove(item);
                            }
                            if (itemsForDelete.Count > 0)
                            {
                                db.SaveChanges();
                                mensajes.Add(new PapeleraMensajes()
                                {
                                    Codigo = 1,
                                    Mensaje = "Despachos Eliminados"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError(null, ex, "Error eliminando despacho de la papelera.");
                            mensajes.Add(new PapeleraMensajes()
                            {
                                Codigo = -1,
                                Mensaje = "Error al eliminar despachos"
                            });                          
                        }
                    }
                    #endregion

                    #region DespachoIniciativa
                    {
                        try
                        {
                            var itemsForDelete = GetDespachoIniciativasPapelera(db, userId);
                            foreach (var item in itemsForDelete)
                            {
                                item.Etiqueta.Clear();
                                item.DestinatarioCopia.Clear();
                                item.TipoAdjunto.Clear();
                                item.Soporte.Clear();
                                db.DespachoIniciativa.Remove(item);
                            }
                            if (itemsForDelete.Count > 0)
                            {
                                db.SaveChanges();
                                mensajes.Add(new PapeleraMensajes()
                                {
                                    Codigo = 1,
                                    Mensaje = "Despachos Iniciativa Eliminados"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError(null, ex, "Error eliminando despacho Iniciativa de la papelera.");
                            mensajes.Add(new PapeleraMensajes()
                            {
                                Codigo = -1,
                                Mensaje = "Error al eliminar despachos iniciativa"
                            });
                        }
                    }
                    #endregion

                    #region Adjunto
                    {
                        try
                        {
                            var itemsForDelete = GetAdjuntosPapelera(db, userId);
                            foreach (var item in itemsForDelete)
                            {
                                item.TipoAdjunto.Clear();
                                db.Adjunto.Remove(item);
                            }
                            if (itemsForDelete.Count > 0)
                            {
                                db.SaveChanges();
                                mensajes.Add(new PapeleraMensajes()
                                {
                                    Codigo = 1,
                                    Mensaje = "Adjuntos Eliminados"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError(null, ex, "Error eliminando Adjuntos de la papelera.");
                            mensajes.Add(new PapeleraMensajes()
                            {
                                Codigo = -1,
                                Mensaje = "Error al eliminar Adjuntos"
                            });
                        }
                    }
                    #endregion

                    #region Adjunto Oficio
                    {
                        try
                        {
                            var itemsForDelete = GetAdjuntosOficioPapelera(db, userId);
                            foreach (var item in itemsForDelete)
                            {
                                item.TipoAdjunto.Clear();
                                db.AdjuntoOficio.Remove(item);
                            }
                            if (itemsForDelete.Count > 0)
                            {
                                db.SaveChanges();
                                mensajes.Add(new PapeleraMensajes()
                                {
                                    Codigo = 1,
                                    Mensaje = "Adjuntos de Oficio Eliminados"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError(null, ex, "Error eliminando Adjuntos de Oficio de la papelera.");
                            mensajes.Add(new PapeleraMensajes()
                            {
                                Codigo = -1,
                                Mensaje = "Error al eliminar Adjuntos de Oficio"
                            });
                        }
                    }
                    #endregion

                    #region Bitacora
                    {
                        try
                        {
                            var itemsForDelete = GetBitacorasPapelera(db, userId);
                            foreach (var items in itemsForDelete)
                            {
                                db.Bitacora.Remove(items);
                            }
                            if (itemsForDelete.Count > 0)
                            {
                                db.SaveChanges();
                                mensajes.Add(new PapeleraMensajes()
                                {
                                    Codigo = 1,
                                    Mensaje = "Bitácoras Eliminadas"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError(null, ex, "Error eliminando Bitácoras de la papelera.");
                            mensajes.Add(new PapeleraMensajes()
                            {
                                Codigo = -1,
                                Mensaje = "Error al eliminar Bitacoras"
                            });
                        }
                    }
                    #endregion                   

                    #region Requerimiento
                    {
                        try
                        {
                            var itemsForDelete = GetRequerimientosPapelera(db, userId);
                            foreach (var item in itemsForDelete)
                            {
                                item.Etiqueta.Clear();
                                item.Soporte.Clear();
                                item.TipoAdjunto.Clear();
                                item.UnidadTecnicaCopia.Clear();
                                db.Requerimiento.Remove(item);
                            }
                            if (itemsForDelete.Count > 0)
                            {
                                db.SaveChanges();
                                mensajes.Add(new PapeleraMensajes()
                                {
                                    Codigo = 1,
                                    Mensaje = "Requerimientos Eliminados"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError(null, ex, "Error eliminando Requerimientos de la papelera.");
                            mensajes.Add(new PapeleraMensajes()
                            {
                                Codigo = -1,
                                Mensaje = "Error al eliminar Requerimiento"
                            });
                        }
                    }
                    #endregion

                    #region Remitente
                    {
                        try
                        {
                            var itemsForDelete = GetRemitentePapelera(db, userId);
                            foreach (var item in itemsForDelete)
                            {
                                db.sp_CambiaEstadoRemitente("E", item.Id, userId, "");
                            }
                            if (itemsForDelete.Count > 0)
                            {
                                mensajes.Add(new PapeleraMensajes()
                                {
                                    Codigo = 1,
                                    Mensaje = "Remitentes Eliminados"
                                });
                            }

                        }
                        catch (Exception ex)
                        {
                            LogError(null, ex, "Error eliminando Remitentes de la papelera.");
                            mensajes.Add(new PapeleraMensajes()
                            {
                                Codigo = -1,
                                Mensaje = "Error al eliminar Remitentes"
                            });
                        }
                    }
                    #endregion
                }


                if (mensajes.Any(a=> a.Codigo == -1) && mensajes.Any(a => a.Codigo == 1)) 
                {
                    resultado.Codigo = -1;
                    resultado.Mensaje = "No se pudo eliminar algunos elementos.<br/>Es posible que algún dato se encuentre referenciado en otro<br/>registro de la base de datos:<br/>";
                    resultado.Extra = string.Join("<br/>", mensajes.Where(a => a.Codigo == -1).Select(b => b.Mensaje).ToArray()); 
                }
                else if (mensajes.All(a => a.Codigo == -1))
                {
                    resultado.Codigo = -1;
                    resultado.Mensaje = "No se pudo eliminar ningún elementos.<br/>Es posible que los datos se encuentren referenciados en otros<br/>registros de la base de datos:<br/>";
                    resultado.Extra = string.Join("<br/>", mensajes.Where(a => a.Codigo == -1).Select(b => b.Mensaje).ToArray());
                }
                else 
                {
                    resultado.Codigo = 1;
                    resultado.Mensaje = "Se eliminaron con éxito todos los elementos";
                    //resultado.Extra = string.Join(", ", mensajes.Where(a => a.Codigo == -1).Select(b => b.Mensaje).ToArray());
                }
            }
            catch (DbUpdateException ex)
            {
                SqlException innerException = ex.InnerException.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 547))
                {
                    resultado = new ResultadoOperacion(-1,
                        "Ocurrió un error al eliminar el registro, se encuentra referenciado en <br/>registros de otras tablas de la base de datos.", null);
                }
                else
                {
                    resultado = new ResultadoOperacion(-1,
                        "Lo sentimos, ha ocurrido un error al eliminar los datos de la Papelera.< br/>Por favor, chequee el log de error de la aplicación.", null);
                }
                LogError(null, ex, resultado.Mensaje);
            }
            catch (Exception ex)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "Lo sentimos, ha ocurrido un error al eliminar los datos de la Papelera.";
                resultado.Extra = ex.Message;
                LogError(resultado, ex, "Lo sentimos, ha ocurrido un error al eliminar los datos de la Papelera.");
            }
            return resultado;
        }

        public ResultadoOperacion RestoreTrashItems(List<PapeleraItemDto> items)
        {
            var resultado = new ResultadoOperacion()
            {
                Codigo = -1,
                Extra = null,
                Mensaje = "Ha ocurrido un error al restaurar los datos de la Papelera."
            };
            try
            {
                foreach (var item in items)
                {
                    switch ((PapeleraOrigenEnum)item.OrigenId)
                    {
                        case PapeleraOrigenEnum.Requerimiento:
                            #region Requerimiento
                            using (var db = new GedocEntities())
                            {
                                var itemForRestore = db.Requerimiento.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForRestore != null)
                                {
                                    itemForRestore.UsuarioEliminacionId = null;
                                    itemForRestore.EliminacionFecha = null;
                                    itemForRestore.Eliminado = false;
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue restaurado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a restaurar.
                                }
                            }
                            break;
                        #endregion
                        case PapeleraOrigenEnum.Despacho:
                            #region Despacho
                            using (var db = new GedocEntities())
                            {
                                var itemForRestore = db.Despacho.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForRestore != null)
                                {
                                    itemForRestore.UsuarioEliminacionId = null;
                                    itemForRestore.EliminacionFecha = null;
                                    itemForRestore.Eliminado = false;
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue restaurado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a restaurar.
                                }
                            }
                            break;
                        #endregion
                        case PapeleraOrigenEnum.Adjunto:
                            #region Adjunto
                            using (var db = new GedocEntities())
                            {
                                var itemForRestore = db.Adjunto.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForRestore != null)
                                {
                                    itemForRestore.UsuarioEliminacionId = null;
                                    itemForRestore.EliminacionFecha = null;
                                    itemForRestore.Eliminado = false;
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue restaurado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a restaurar.
                                }
                            }
                            break;
                        #endregion
                        case PapeleraOrigenEnum.AdjuntoOficio:
                            #region Adjunto de Oficio
                            using (var db = new GedocEntities())
                            {
                                var itemForRestore = db.AdjuntoOficio.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForRestore != null)
                                {
                                    itemForRestore.UsuarioEliminacionId = null;
                                    itemForRestore.EliminacionFecha = null;
                                    itemForRestore.Eliminado = false;
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue restaurado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a restaurar.
                                }
                            }
                            break;
                        #endregion
                        case PapeleraOrigenEnum.Bitacora:
                            #region Bitacora
                            using (var db = new GedocEntities())
                            {
                                var itemForRestore = db.Bitacora.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForRestore != null)
                                {
                                    itemForRestore.UsuarioEliminacionId = null;
                                    itemForRestore.EliminacionFecha = null;
                                    itemForRestore.Eliminado = false;
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue restaurado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a restaurar.
                                }
                            }
                            break;
                        #endregion
                        case PapeleraOrigenEnum.DespachoIniciativa:
                            #region DespachoIniciativa
                            using (var db = new GedocEntities())
                            {
                                var itemForRestore = db.DespachoIniciativa.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForRestore != null)
                                {
                                    itemForRestore.UsuarioEliminacionId = null;
                                    itemForRestore.EliminacionFecha = null;
                                    itemForRestore.Eliminado = false;
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue restaurado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a restaurar.
                                }
                            }
                            break;
                        #endregion
                        case PapeleraOrigenEnum.PlantillaOficio:
                            #region Plantilla de Oficio
                            using (var db = new GedocEntities())
                            {
                                var itemForRestore = db.PlantillaOficio.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForRestore != null)
                                {
                                    itemForRestore.UsuarioEliminacionId = null;
                                    itemForRestore.EliminacionFecha = null;
                                    itemForRestore.Eliminado = false;
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue restaurado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a restaurar.
                                }
                            }
                            break;
                        #endregion
                        case PapeleraOrigenEnum.Oficio:
                            #region Oficio
                            using (var db = new GedocEntities())
                            {
                                var itemForRestore = db.Oficio.FirstOrDefault(a => a.Id == item.Id);
                                if (itemForRestore != null)
                                {
                                    itemForRestore.UsuarioEliminacionId = null;
                                    itemForRestore.EliminacionFecha = null;
                                    itemForRestore.Eliminado = false;
                                    db.SaveChanges();
                                    //TODO: guardar mensaje: requerimiento id: item.Id fue restaurado con exito.
                                }
                                else
                                {
                                    //TODO: guardar el error no existe el requerimiento a restaurar.
                                }
                            }
                            break;
                        #endregion
                        case PapeleraOrigenEnum.Remitente:
                            #region Remitente
                            using (var db = new GedocEntities())
                            {
                                db.sp_CambiaEstadoRemitente("R", item.Id, null, "");
                            }
                            break;
                        #endregion
                    }
                }

                if (1 == 1) //TODO: validar si no hay errores
                {
                    resultado.Codigo = 1;
                    resultado.Mensaje = "Se restauraron los elementos seleccionados de la Papelera";
                }
                //else if (1 == 2) //TODO: validar si hay algunos errores
                //{
                //    resultado.Codigo = 1;
                //    resultado.Mensaje = "Se restauraron Algunos items";

                //}
                //else //TODO: validar si hay errores
                //{
                //    resultado.Codigo = -1;
                //    resultado.Mensaje = "Lo sentimos, ha ocurrido un error al restauraron los datos de la Papelera";
                //}
            }
            catch (Exception ex)
            {
                resultado.Extra = ex.Message;
                LogError(resultado, ex, "Lo sentimos, ha ocurrido un error al restaurar los datos de la Papelera.");
            }
            return resultado;
        }

        private List<Requerimiento> GetRequerimientosPapelera(GedocEntities db, int userId)
        {
            var itemsEliminados = db.Requerimiento.Where(a => a.Eliminado == true);
            if (!IsAdmin(db, userId))
            {
                itemsEliminados = itemsEliminados.Where(a => a.UsuarioCreacionId == userId);
            }
            return itemsEliminados.ToList();
        }

        private List<Despacho> GetDespachosPapelera(GedocEntities db, int userId)
        {
            var itemsEliminados = db.Despacho.Where(a => a.Eliminado == true);
            if (!IsAdmin(db, userId))
            {
                itemsEliminados = itemsEliminados.Where(a => a.UsuarioCreacionId == userId);
            }
            return itemsEliminados.ToList();
        }

        private List<Adjunto> GetAdjuntosPapelera(GedocEntities db, int userId)
        {
            var itemsEliminados = db.Adjunto.Where(a => a.Eliminado == true);
            if (!IsAdmin(db, userId))
            {
                //var user = db.Usuario.FirstOrDefault(a => a.Id == userId);
                itemsEliminados = itemsEliminados.Where(a => a.UsuarioCreacionId == userId);
            }
            return itemsEliminados.ToList();
        }

        private List<AdjuntoOficio> GetAdjuntosOficioPapelera(GedocEntities db, int userId)
        {
            var itemsEliminados = db.AdjuntoOficio.Where(a => a.Eliminado == true);
            if (!IsAdmin(db, userId))
            {
                //var user = db.Usuario.FirstOrDefault(a => a.Id == userId);
                itemsEliminados = itemsEliminados.Where(a => a.UsuarioCreacionId == userId);
            }
            return itemsEliminados.ToList();
        }

        private List<Bitacora> GetBitacorasPapelera(GedocEntities db, int userId)
        {
            var itemsEliminados = db.Bitacora.Where(a => a.Eliminado == true);
            if (!IsAdmin(db, userId))
            {
                itemsEliminados = itemsEliminados.Where(a => a.UsuarioCreacionId == userId);
            }
            return itemsEliminados.ToList();
        }

        private List<DespachoIniciativa> GetDespachoIniciativasPapelera(GedocEntities db, int userId)
        {
            var itemsEliminados = db.DespachoIniciativa.Where(a => a.Eliminado == true);
            if (!IsAdmin(db, userId))
            {
                itemsEliminados = itemsEliminados.Where(a => a.UsuarioCreacionId == userId);
            }
            return itemsEliminados.ToList();
        }

        private List<PlantillaOficio> GetPlantillaOficioPapelera(GedocEntities db, int userId)
        {
            var itemsEliminados = db.PlantillaOficio.Where(a => a.Eliminado);
            if (!IsAdmin(db, userId))
            {
                itemsEliminados = itemsEliminados.Where(a => a.UsuarioCreacionId == userId);
            }
            return itemsEliminados.ToList();
        }

        private List<Oficio> GetOficioPapelera(GedocEntities db, int userId)
        {
            var itemsEliminados = db.Oficio.Where(a => a.Eliminado);
            if (!IsAdmin(db, userId))
            {
                itemsEliminados = itemsEliminados.Where(a => a.UsuarioCreacionId == userId);
            }
            return itemsEliminados.ToList();
        }

        private List<Remitente> GetRemitentePapelera(GedocEntities db, int userId)
        {
            var itemsEliminados = db.Remitente.Where(a => !a.Activo);
            if (!IsAdmin(db, userId))
            {
                itemsEliminados = itemsEliminados.Where(a => a.UsuarioEliminacionId == userId);
            }
            return itemsEliminados.ToList();
        }

        private bool IsAdmin(GedocEntities db, int userid)
        {
            try
            {
                //rol 21	Propietarios gestor documental CMN               
                var isAdmin = db.Usuario.Any(a => a.Id == userid && a.Rol.Any(b => b.Id == 21));
                return isAdmin;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }
        }

        
    }

    public class PapeleraMensajes
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; }
    }
}
