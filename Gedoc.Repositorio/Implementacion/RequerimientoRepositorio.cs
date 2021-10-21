using System;
using System.Collections.Generic;
using System.Linq;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Maps.Interfaces;
using Gedoc.Repositorio.Model;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq.Dynamic;
using Gedoc.Helpers;
using Gedoc.Helpers.Enum;
using Gedoc.Helpers.Logging;
using Gedoc.Service.Maps;
using Rol = Gedoc.Helpers.Enum.Rol;

namespace Gedoc.Repositorio.Implementacion
{
    public class RequerimientoRepositorio : RepositorioBase, IRequerimientoRepositorio
    {
        private readonly IGenericMap _mapper;

        public RequerimientoRepositorio(IGenericMap mapper)
        {
            this._mapper = mapper;
        }

        public GedocEntities GetDbContext()
        {
            return this.db;
        }

        public ResultadoOperacion New(RequerimientoDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var ingreso = _mapper.MapFromDtoToModel<RequerimientoDto, Requerimiento>(datos);

            // Se genera el valor de Documento de Ingreso en base al correlativo (no se realiza para Requerimiento Histórico)
            if (ingreso.TipoIngreso != "Ingreso historico")
            {
                var annoIngreso = ingreso.FechaIngreso.Year;
                var corr = db.Correlativo.FirstOrDefault(c => c.Anno == annoIngreso);
                if (corr == null)
                {
                    throw new Exception("No se encontró en base de datos el correlativo para el año " + annoIngreso);
                }

                var corrIngreso = corr.CorrelativoIngreso++;

                var docIngreso = $"{corrIngreso}-{annoIngreso}";
                ingreso.NumeroIngreso = corrIngreso;
                ingreso.DocumentoIngreso = docIngreso;
            }

            EvitaInsertHijo(ingreso);

            db.Requerimiento.Add(ingreso);
            datos.ControlCambios = ControlCambios(ingreso);
            db.SaveChanges();

            datos.Id = ingreso.Id;
            datos.DocumentoIngreso = ingreso.DocumentoIngreso;
            datos.NumeroIngreso = ingreso.NumeroIngreso;

            return resultado;
        }

        public ResultadoOperacion UpdateIngresoCentral(RequerimientoDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var ingreso = db.Requerimiento
                .Include(r => r.Etiqueta)
                .Include(r => r.Soporte)
                .Include(r => r.TipoAdjunto)
                .Include(r => r.MonumentoNacional)
                .Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                .Include(r => r.MonumentoNacional.Region)
                .Include(r => r.MonumentoNacional.Provincia)
                .Include(r => r.MonumentoNacional.Comuna)
                .FirstOrDefault(r => r.Id == datos.Id);

            if (ingreso == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el ingreso a actualizar.";
                return resultado;
            }

            // Se especifican los datos a actualizar
            ingreso.EstadoId = datos.EstadoId;
            ingreso.EtapaId = datos.EtapaId;
            #region Panel Documento
            if (DateTime.Compare(ingreso.FechaIngreso.Date, datos.FechaIngreso.Date) != 0)
            {
                ingreso.FechaIngreso = datos.FechaIngreso.Date + new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            }
            ingreso.TipoTramiteId = datos.TipoTramiteId;
            ingreso.TipoDocumentoCod = datos.TipoDocumentoCod;
            ingreso.ObservacionesTipoDoc = datos.ObservacionesTipoDoc;
            ingreso.FechaDocumento = datos.FechaDocumento;
            ingreso.FechaDocumento = datos.FechaDocumento;
            ingreso.TipoIngreso = string.IsNullOrWhiteSpace(datos.TipoIngreso) ? ingreso.TipoIngreso  : datos.TipoIngreso;
            ingreso.CanalLlegadaTramiteCod = datos.CanalLlegadaTramiteCod;
            //ingreso.SiacTransparencia = datos.SiacTransparencia; // TODO
            #endregion
            #region Panel Adjuntos
            ingreso.AdjuntaDocumentacion = datos.AdjuntaDocumentacion;
            ingreso.CantidadAdjuntos = datos.CantidadAdjuntos;
            ingreso.ObservacionesAdjuntos = datos.ObservacionesAdjuntos;
            #endregion
            #region Panel Remitente
            ingreso.RemitenteId = datos.RemitenteId;
            #endregion
            #region Panel Proyecto
            ingreso.NombreProyectoPrograma = datos.NombreProyectoPrograma;
            ingreso.CasoId = datos.CasoId;
            ingreso.Materia = datos.Materia;
            #endregion
            #region Panel Monumento Nacional
            if (datos.MonumentoNacional != null)
            {
                ingreso.MonumentoNacional.CodigoMonumentoNac = datos.MonumentoNacional.CodigoMonumentoNac;
                ingreso.MonumentoNacional.DenominacionOficial = datos.MonumentoNacional.DenominacionOficial;
                ingreso.MonumentoNacional.OtrasDenominaciones = datos.MonumentoNacional.OtrasDenominaciones;
                ingreso.MonumentoNacional.NombreUsoActual = datos.MonumentoNacional.NombreUsoActual;
                ingreso.MonumentoNacional.DireccionMonumentoNac = datos.MonumentoNacional.DireccionMonumentoNac;
                ingreso.MonumentoNacional.ReferenciaLocalidad = datos.MonumentoNacional.ReferenciaLocalidad;
                ingreso.MonumentoNacional.RolSii = datos.MonumentoNacional.RolSii;

                // Update Categorías MN
                var cods2 = datos.MonumentoNacional.CategoriaMonumentoNac.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.CategoriaMonumentoNac = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.CategoriaMn)
                    .ToList();
                // Update Regiones
                cods2 = datos.MonumentoNacional.Region.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Region = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Region)
                    .ToList();
                // Update Provincias
                cods2 = datos.MonumentoNacional.Provincia.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Provincia = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Provincia)
                    .ToList();
                // Update Comunas
                cods2 = datos.MonumentoNacional.Comuna.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Comuna = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Comuna)
                    .ToList();
            }
            #endregion
            #region Panel General
            ingreso.FormaLlegadaCod = datos.FormaLlegadaCod;
            ingreso.ObservacionesFormaLlegada = datos.ObservacionesFormaLlegada;
            ingreso.CaracterId = datos.CaracterId;
            ingreso.ObservacionesCaracter = datos.ObservacionesCaracter;
            ingreso.Redireccionado = datos.Redireccionado;
            ingreso.NumeroTicket = datos.NumeroTicket;
            ingreso.RequerimientoAnteriorId = datos.RequerimientoAnteriorId;
            ingreso.RequerimientoNoRegistrado = datos.RequerimientoNoRegistrado;
            #endregion

            #region Relaciones de muchos-muchos
            // Update Tipos de Adjunto
            var cods = datos.TipoAdjunto.Select(d => d.Id).ToList();
            ingreso.TipoAdjunto = db.ListaValor
                .Where(x => cods.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.TipoDocumento)
                .ToList();
            // Update Soportes
            cods = datos.Soporte.Select(d => d.Id).ToList();
            ingreso.Soporte = db.ListaValor
                .Where(x => cods.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Soporte)
                .ToList();
            // Update Etiquetas
            cods = datos.Etiqueta.Select(d => d.Id).ToList();
            ingreso.Etiqueta = db.ListaValor
                .Where(x => cods.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Etiqueta)
                .ToList();

            EvitaInsertHijo(ingreso);

            #endregion

            //var changeInfo = db.ChangeTracker.Entries()
            //    .Where(t => t.State == EntityState.Modified)
            //    .Select(t => new EntityChanges
            //    {
            //        EntityName = t.Entity.ToString(),
            //        Original = t.OriginalValues.PropertyNames.ToDictionary(pn => pn, pn => t.OriginalValues[pn]),
            //        Current = t.CurrentValues.PropertyNames.ToDictionary(pn => pn, pn => t.CurrentValues[pn]),
            //    })
            //    .ToList();

            datos.ControlCambios = ControlCambios(ingreso);
            db.SaveChanges();

            return resultado;
        }

        public ResultadoOperacion UpdateAsignacionUt(RequerimientoDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var ingreso = db.Requerimiento
                .Include(r => r.Etiqueta)
                .Include(r => r.Soporte)
                .Include(r => r.TipoAdjunto)
                .Include(r => r.UnidadTecnicaCopia)
                .Include(r => r.MonumentoNacional)
                .Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                .Include(r => r.MonumentoNacional.Region)
                .Include(r => r.MonumentoNacional.Provincia)
                .Include(r => r.MonumentoNacional.Comuna)
                .FirstOrDefault(r => r.Id == datos.Id);

            if (ingreso == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el ingreso a actualizar.";
                return resultado;
            }

            // Se especifican los datos a actualizar
            ingreso.EstadoId = datos.EstadoId;
            ingreso.EtapaId = datos.EtapaId;
            ingreso.TipoIngreso = string.IsNullOrWhiteSpace(datos.TipoIngreso) ? ingreso.TipoIngreso : datos.TipoIngreso;
            #region Panel Documento
            ingreso.TipoTramiteId = datos.TipoTramiteId;
            ingreso.TipoDocumentoCod = datos.TipoDocumentoCod;
            ingreso.ObservacionesTipoDoc = datos.ObservacionesTipoDoc;
            ingreso.FechaDocumento = datos.FechaDocumento;
            ingreso.CanalLlegadaTramiteCod = datos.CanalLlegadaTramiteCod;
            //ingreso.SiacTransparencia = datos.SiacTransparencia; // TODO
            #endregion
            #region Panel Adjuntos
            ingreso.AdjuntaDocumentacion = datos.AdjuntaDocumentacion;
            ingreso.CantidadAdjuntos = datos.CantidadAdjuntos;
            ingreso.ObservacionesAdjuntos = datos.ObservacionesAdjuntos;
            #endregion
            #region Panel Remitente
            ingreso.RemitenteId = datos.RemitenteId;
            #endregion
            #region Panel Proyecto
            ingreso.NombreProyectoPrograma = datos.NombreProyectoPrograma;
            ingreso.CasoId = datos.CasoId;
            ingreso.Materia = datos.Materia;
            #endregion
            #region Panel Monumento Nacional
            if (datos.MonumentoNacional != null)
            {
                ingreso.MonumentoNacional.DenominacionOficial = datos.MonumentoNacional.DenominacionOficial;
                ingreso.MonumentoNacional.OtrasDenominaciones = datos.MonumentoNacional.OtrasDenominaciones;
                ingreso.MonumentoNacional.NombreUsoActual = datos.MonumentoNacional.NombreUsoActual;
                ingreso.MonumentoNacional.DireccionMonumentoNac = datos.MonumentoNacional.DireccionMonumentoNac;
                ingreso.MonumentoNacional.ReferenciaLocalidad = datos.MonumentoNacional.ReferenciaLocalidad;
                ingreso.MonumentoNacional.RolSii = datos.MonumentoNacional.RolSii;

                // Update Categorías MN
                var cods2 = datos.MonumentoNacional.CategoriaMonumentoNac.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.CategoriaMonumentoNac = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.CategoriaMn)
                    .ToList();
                // Update Regiones
                cods2 = datos.MonumentoNacional.Region.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Region = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Region)
                    .ToList();
                // Update Provincias
                cods2 = datos.MonumentoNacional.Provincia.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Provincia = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Provincia)
                    .ToList();
                // Update Comunas
                cods2 = datos.MonumentoNacional.Comuna.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Comuna = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Comuna)
                    .ToList();
            }
            #endregion
            #region Panel General
            ingreso.FormaLlegadaCod = datos.FormaLlegadaCod;
            ingreso.ObservacionesFormaLlegada = datos.ObservacionesFormaLlegada;
            ingreso.CaracterId = datos.CaracterId;
            ingreso.ObservacionesCaracter = datos.ObservacionesCaracter;
            ingreso.Redireccionado = datos.Redireccionado;
            ingreso.NumeroTicket = datos.NumeroTicket;
            ingreso.RequerimientoAnteriorId = datos.RequerimientoAnteriorId;
            ingreso.RequerimientoNoRegistrado = datos.RequerimientoNoRegistrado;
            #endregion
            #region Panel Asignación
            ingreso.AsignacionUt = datos.AsignacionUt ?? ingreso.AsignacionUt;
            ingreso.ResponsableUtId = datos.ResponsableUtId ?? ingreso.ResponsableUtId;
            ingreso.EnviadoUt = datos.EnviadoUt ?? ingreso.EnviadoUt;
            ingreso.UtAsignadaId = datos.UtAsignadaId;
            ingreso.UtConocimientoId = datos.UtConocimientoId;
            ingreso.RequiereRespuesta = datos.RequiereRespuesta;
            ingreso.ComentarioAsignacion = datos.ComentarioAsignacion;
            #endregion
            #region Panel Asignación Temporal
            ingreso.AsignacionUtTemp = datos.AsignacionUtTemp ?? ingreso.AsignacionUtTemp;
            ingreso.ResponsableUtTempId = datos.ResponsableUtTempId ?? ingreso.ResponsableUtTempId;
            ingreso.UtTemporalId = datos.UtTemporalId;
            #endregion
            #region Panel Priorización
            ingreso.PrioridadCod = datos.PrioridadCod ?? ingreso.PrioridadCod;
            ingreso.SolicitanteUrgenciaId = datos.SolicitanteUrgenciaId ?? ingreso.SolicitanteUrgenciaId;
            ingreso.Plazo = datos.Plazo ?? ingreso.Plazo;
            ingreso.Resolucion = datos.Resolucion ?? ingreso.Resolucion;
            #endregion

            #region Relaciones muchos a muchos 
            // Update Tipos de Adjunto
            var cods = datos.TipoAdjunto.Select(d => d.Id).ToList();
            ingreso.TipoAdjunto = db.ListaValor
                .Where(x => cods.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.TipoDocumento)
                .ToList();
            // Update Soportes
            cods = datos.Soporte.Select(d => d.Id).ToList();
            ingreso.Soporte = db.ListaValor
                .Where(x => cods.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Soporte)
                .ToList();
            // Update Etiquetas
            cods = datos.Etiqueta.Select(d => d.Id).ToList();
            ingreso.Etiqueta = db.ListaValor.Where(x => cods.Any(d => d == x.Codigo && x.IdLista == (int)Mantenedor.Etiqueta)).ToList();
            // Update UT en Copia 
            var ids2 = datos.UnidadTecnicaCopia.Select(d => d.IdInt).ToList();
            ingreso.UnidadTecnicaCopia = db.UnidadTecnica.Where(x => ids2.Any(d => d == x.Id)).ToList();

            EvitaInsertHijo(ingreso);
            #endregion

            datos.ControlCambios = ControlCambios(ingreso);
            db.SaveChanges();

            return resultado;
        }

        public ResultadoOperacion UpdateReasignacionUt(RequerimientoDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var ingreso = db.Requerimiento
                .Include(r => r.UnidadTecnicaCopia)
                .Include(r => r.MonumentoNacional)
                .Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                .Include(r => r.MonumentoNacional.Region)
                .Include(r => r.MonumentoNacional.Provincia)
                .Include(r => r.MonumentoNacional.Comuna)
                .FirstOrDefault(r => r.Id == datos.Id);

            if (ingreso == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el ingreso a actualizar.";
                return resultado;
            }

            // Se especifican los datos a actualizar
            ingreso.EstadoId = datos.EstadoId;
            ingreso.EtapaId = datos.EtapaId;
            ingreso.TipoIngreso = string.IsNullOrWhiteSpace(datos.TipoIngreso) ? ingreso.TipoIngreso : datos.TipoIngreso;
            #region Panel Documento
            ingreso.TipoTramiteId = datos.TipoTramiteId;
            ingreso.FechaDocumento = datos.FechaDocumento;
            //ingreso.SiacTransparencia = datos.SiacTransparencia; // TODO
            #endregion
            #region Panel Adjuntos
            // Nada
            #endregion
            #region Panel Remitente
            // Nada
            #endregion
            #region Panel Proyecto
            // Nada
            #endregion
            #region Panel Monumento Nacional
            if (datos.MonumentoNacional != null)
            {
                ingreso.MonumentoNacional.DenominacionOficial = datos.MonumentoNacional.DenominacionOficial;
                ingreso.MonumentoNacional.OtrasDenominaciones = datos.MonumentoNacional.OtrasDenominaciones;
                ingreso.MonumentoNacional.NombreUsoActual = datos.MonumentoNacional.NombreUsoActual;
                ingreso.MonumentoNacional.DireccionMonumentoNac = datos.MonumentoNacional.DireccionMonumentoNac;
                ingreso.MonumentoNacional.ReferenciaLocalidad = datos.MonumentoNacional.ReferenciaLocalidad;
                ingreso.MonumentoNacional.RolSii = datos.MonumentoNacional.RolSii;

                // Update Categorías MN
                var cods2 = datos.MonumentoNacional.CategoriaMonumentoNac.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.CategoriaMonumentoNac = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.CategoriaMn)
                    .ToList();
                // Update Regiones
                cods2 = datos.MonumentoNacional.Region.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Region = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Region)
                    .ToList();
                // Update Provincias
                cods2 = datos.MonumentoNacional.Provincia.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Provincia = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Provincia)
                    .ToList();
                // Update Comunas
                cods2 = datos.MonumentoNacional.Comuna.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Comuna = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Comuna)
                    .ToList();
            }
            #endregion
            #region Panel General
            // Nada
            #endregion
            #region Panel Asignación
            // Nada
            #endregion
            #region Panel Reasignación
            ingreso.AsignacionUt = datos.AsignacionUt ?? ingreso.AsignacionUt;
            ingreso.ResponsableUtId = datos.ResponsableUtId ?? ingreso.ResponsableUtId;
            ingreso.EnviadoUt = datos.EnviadoUt ?? ingreso.EnviadoUt;
            ingreso.UtAsignadaId = datos.UtAsignadaId;
            ingreso.ComentarioAsignacion = datos.ComentarioAsignacion;
            #endregion
            #region Panel Asignación Temporal
            // Nada
            #endregion
            #region Panel Priorización
            // Nada
            #endregion

            #region Relaciones muchos a muchos 
            // Update UT en Copia 
            var ids2 = datos.UnidadTecnicaCopia.Select(d => d.IdInt).ToList();
            ingreso.UnidadTecnicaCopia = db.UnidadTecnica.Where(x => ids2.Any(d => d == x.Id)).ToList();

            EvitaInsertHijo(ingreso);
            #endregion

            datos.ControlCambios = ControlCambios(ingreso);
            db.SaveChanges();

            return resultado;
        }

        public ResultadoOperacion UpdatePriorizacion(RequerimientoDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var ingreso = db.Requerimiento
                .Include(r => r.Etiqueta)
                .Include(r => r.Soporte)
                .Include(r => r.TipoAdjunto)
                .Include(r => r.MonumentoNacional)
                .Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                .Include(r => r.MonumentoNacional.Region)
                .Include(r => r.MonumentoNacional.Provincia)
                .Include(r => r.MonumentoNacional.Comuna)
                .FirstOrDefault(r => r.Id == datos.Id);

            if (ingreso == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el ingreso a actualizar.";
                return resultado;
            }

            // Se especifican los datos a actualizar
            #region Datos propios del flujo
            ingreso.EstadoId = datos.EstadoId;
            ingreso.EtapaId = datos.EtapaId;
            #endregion
            #region Panel Documento
            ingreso.TipoTramiteId = datos.TipoTramiteId;
            ingreso.TipoDocumentoCod = datos.TipoDocumentoCod;
            ingreso.ObservacionesTipoDoc = datos.ObservacionesTipoDoc;
            ingreso.FechaDocumento = datos.FechaDocumento;
            //ingreso.SiacTransparencia = datos.SiacTransparencia; // TODO
            #endregion
            #region Panel Adjuntos
            ingreso.AdjuntaDocumentacion = datos.AdjuntaDocumentacion;
            ingreso.CantidadAdjuntos = datos.CantidadAdjuntos;
            ingreso.ObservacionesAdjuntos = datos.ObservacionesAdjuntos;
            #endregion
            #region Panel Remitente
            ingreso.RemitenteId = datos.RemitenteId;
            #endregion
            #region Panel Proyecto
            ingreso.NombreProyectoPrograma = datos.NombreProyectoPrograma;
            ingreso.CasoId = datos.CasoId;
            ingreso.Materia = datos.Materia;
            #endregion
            #region Panel Monumento Nacional
            if (datos.MonumentoNacional != null)
            {
                ingreso.MonumentoNacional.DenominacionOficial = datos.MonumentoNacional.DenominacionOficial;
                ingreso.MonumentoNacional.OtrasDenominaciones = datos.MonumentoNacional.OtrasDenominaciones;
                ingreso.MonumentoNacional.NombreUsoActual = datos.MonumentoNacional.NombreUsoActual;
                ingreso.MonumentoNacional.DireccionMonumentoNac = datos.MonumentoNacional.DireccionMonumentoNac;
                ingreso.MonumentoNacional.ReferenciaLocalidad = datos.MonumentoNacional.ReferenciaLocalidad;
                ingreso.MonumentoNacional.RolSii = datos.MonumentoNacional.RolSii;

                // Update Categorías MN
                var cods2 = datos.MonumentoNacional.CategoriaMonumentoNac.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.CategoriaMonumentoNac = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.CategoriaMn)
                    .ToList();
                // Update Regiones
                cods2 = datos.MonumentoNacional.Region.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Region = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Region)
                    .ToList();
                // Update Provincias
                cods2 = datos.MonumentoNacional.Provincia.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Provincia = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Provincia)
                    .ToList();
                // Update Comunas
                cods2 = datos.MonumentoNacional.Comuna.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Comuna = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Comuna)
                    .ToList();
            }
            #endregion
            #region Panel General
            ingreso.FormaLlegadaCod = datos.FormaLlegadaCod;
            ingreso.ObservacionesFormaLlegada = datos.ObservacionesFormaLlegada;
            ingreso.CaracterId = datos.CaracterId;
            ingreso.ObservacionesCaracter = datos.ObservacionesCaracter;
            ingreso.Redireccionado = datos.Redireccionado;
            ingreso.NumeroTicket = datos.NumeroTicket;
            ingreso.RequerimientoAnteriorId = datos.RequerimientoAnteriorId;
            ingreso.RequerimientoNoRegistrado = datos.RequerimientoNoRegistrado;
            #endregion
            #region Panel Asignación
            ingreso.RequiereRespuesta = datos.RequiereRespuesta;
            ingreso.ComentarioAsignacion = datos.ComentarioAsignacion;
            #endregion
            #region Panel Priorización
            ingreso.PrioridadCod = datos.PrioridadCod;
            ingreso.SolicitanteUrgenciaId = datos.SolicitanteUrgenciaId;
            ingreso.Plazo = datos.Plazo;
            ingreso.Resolucion = datos.Resolucion;
            #endregion

            #region Relaciones muchos a mucho 
            // Update Tipos de Adjunto
            var cods = datos.TipoAdjunto.Select(d => d.Id).ToList();
            ingreso.TipoAdjunto = db.ListaValor
                .Where(x => cods.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.TipoDocumento)
                .ToList();
            // Update Soportes
            cods = datos.Soporte.Select(d => d.Id).ToList();
            ingreso.Soporte = db.ListaValor
                .Where(x => cods.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Soporte)
                .ToList();
            // Update Etiquetas
            cods = datos.Etiqueta.Select(d => d.Id).ToList();
            ingreso.Etiqueta = db.ListaValor.Where(x => cods.Any(d => d == x.Codigo && x.IdLista == (int)Mantenedor.Etiqueta)).ToList();

            EvitaInsertHijo(ingreso);
            #endregion

            datos.ControlCambios = ControlCambios(ingreso);
            db.SaveChanges();

            return resultado;
        }

        public ResultadoOperacion UpdateAsignarProfesional(RequerimientoDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var ingreso = db.Requerimiento
                .Include(r => r.Etiqueta)
                .Include(r => r.Soporte)
                .Include(r => r.TipoAdjunto)
                .Include(r => r.UnidadTecnicaAsign)
                .Include(r => r.MonumentoNacional)
                .Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                .Include(r => r.MonumentoNacional.Region)
                .Include(r => r.MonumentoNacional.Provincia)
                .Include(r => r.MonumentoNacional.Comuna)
                .FirstOrDefault(r => r.Id == datos.Id);

            if (ingreso == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el ingreso a actualizar.";
                return resultado;
            }

            // Se especifican los datos a actualizar
            ingreso.EstadoId = datos.EstadoId;
            ingreso.EtapaId = datos.EtapaId;
            ingreso.AsignacionProfesionalTemp = datos.AsignacionProfesionalTemp ?? ingreso.AsignacionProfesionalTemp;
            ingreso.RecepcionUtTemp = datos.RecepcionUtTemp ?? ingreso.RecepcionUtTemp;
            ingreso.RecepcionUt = datos.RecepcionUt;
            ingreso.AsignacionResponsable = datos.AsignacionResponsable;


            //Forzar Prioridad
            var fechaPrioOld = ingreso.ForzarPrioridadFecha;
            ingreso.ForzarPrioridad = datos.ForzarPrioridad;
            ingreso.ForzarPrioridadFecha = datos.ForzarPrioridadFecha;
            ingreso.ForzarPrioridadMotivo = datos.ForzarPrioridadMotivo;
            datos.ForzarPrioridadFecha = fechaPrioOld;

            #region Datos propios del flujo
            #endregion
            #region Panel Documento
            ingreso.TipoTramiteId = datos.TipoTramiteId;
            //ingreso.SiacTransparencia = datos.SiacTransparencia; // TODO
            #endregion
            #region Panel Adjuntos
            #endregion
            #region Panel Remitente
            #endregion
            #region Panel Proyecto
            ingreso.NombreProyectoPrograma = datos.NombreProyectoPrograma;
            ingreso.CasoId = datos.CasoId;
            //ingreso.Materia = datos.Materia;
            ingreso.ProyectoActividad = datos.ProyectoActividad;
            #endregion
            #region Panel Monumento Nacional
            if (datos.MonumentoNacional != null)
            {
                ingreso.MonumentoNacional.DenominacionOficial = datos.MonumentoNacional.DenominacionOficial;
                ingreso.MonumentoNacional.OtrasDenominaciones = datos.MonumentoNacional.OtrasDenominaciones;
                ingreso.MonumentoNacional.NombreUsoActual = datos.MonumentoNacional.NombreUsoActual;
                ingreso.MonumentoNacional.DireccionMonumentoNac = datos.MonumentoNacional.DireccionMonumentoNac;
                ingreso.MonumentoNacional.ReferenciaLocalidad = datos.MonumentoNacional.ReferenciaLocalidad;
                ingreso.MonumentoNacional.RolSii = datos.MonumentoNacional.RolSii;

                // Update Categorías MN
                var cods2 = datos.MonumentoNacional.CategoriaMonumentoNac.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.CategoriaMonumentoNac = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.CategoriaMn)
                    .ToList();
                // Update Regiones
                cods2 = datos.MonumentoNacional.Region.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Region = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Region)
                    .ToList();
                // Update Provincias
                cods2 = datos.MonumentoNacional.Provincia.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Provincia = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Provincia)
                    .ToList();
                // Update Comunas
                cods2 = datos.MonumentoNacional.Comuna.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Comuna = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Comuna)
                    .ToList();
            }
            #endregion
            #region Panel General
            ingreso.RequerimientoAnteriorId = datos.RequerimientoAnteriorId;
            ingreso.RequerimientoNoRegistrado = datos.RequerimientoNoRegistrado;
            #endregion
            #region Panel Asignación
            ingreso.RequiereRespuesta = datos.RequiereRespuesta;
            #endregion
            #region Panel Priorización
            #endregion
            #region Panel UT-Asignación Profesional UT
            ingreso.ProfesionalId = datos.ProfesionalId;
            ingreso.ComentarioEncargadoUt = datos.ComentarioEncargadoUt;
            ingreso.RequiereTimbrajePlano = datos.RequiereTimbrajePlano;
            ingreso.RequiereAcuerdo = datos.RequiereAcuerdo;
            if (datos.EnAsignacionTemp)
            {
                ingreso.ProfesionalTempId = datos.ProfesionalTempId;
            }
            #endregion
            #region Panel Solicitud Reasignación
            if (datos.DevolverAsignacion)
            {
                datos.UtAsignadaTitulo = ingreso.UnidadTecnicaAsign?.Titulo;
                ingreso.EstadoId = datos.EstadoId;
                ingreso.EtapaId = datos.EtapaId;
                ingreso.AsignacionUt = null;
                ingreso.ProfesionalId = datos.ProfesionalId;
                ingreso.ProfesionalTempId = datos.ProfesionalTempId;
                ingreso.ProfesionalTranspId = datos.ProfesionalTranspId;
                ingreso.ResponsableUtId = datos.ResponsableUtId;
                ingreso.UtAnteriorId = datos.UtAnteriorId;
                ingreso.UtAsignadaId = datos.UtAsignadaId;
                ingreso.UtTemporalId = datos.UtTemporalId;
                ingreso.UtApoyoId = datos.UtApoyoId;
                ingreso.UtConocimientoId = datos.UtConocimientoId;
                ingreso.UtTransparenciaId = datos.UtTransparenciaId;
                ingreso.Devolucion = datos.Devolucion;
                ingreso.AsignacionAnterior = datos.AsignacionAnterior;
                ingreso.LiberarAsignacionTemp = datos.LiberarAsignacionTemp;
                ingreso.EnviadoUt = datos.EnviadoUt;
                ingreso.ComentarioDevolucion = datos.ComentarioDevolucion;
                ingreso.UnidadTecnicaCopia = null;
            }
            #endregion

            #region Relaciones muchos a mucho 
            // Update Etiquetas
            var cods = datos.Etiqueta.Select(d => d.Id).ToList();
            ingreso.Etiqueta = db.ListaValor.Where(x => cods.Any(d => d == x.Codigo && x.IdLista == (int)Mantenedor.Etiqueta)).ToList();

            EvitaInsertHijo(ingreso);
            #endregion

            datos.ControlCambios = ControlCambios(ingreso);
            db.SaveChanges();

            return resultado;
        }

        public ResultadoOperacion UpdateReasignarProfesional(RequerimientoDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var ingreso = db.Requerimiento
                .Include(r => r.SolicitanteUrgencia)
                .FirstOrDefault(r => r.Id == datos.Id);

            if (ingreso == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el ingreso a actualizar.";
                return resultado;
            }

            // Se especifican los datos a actualizar
            ingreso.EstadoId = datos.EstadoId;
            ingreso.EtapaId = datos.EtapaId;
            #region Datos propios del flujo
            #endregion
            #region Panel UT- Reasignación Profesional UT
            // Datos del profesional anterior asignado
            var profAnt = db.Usuario.FirstOrDefault(u => u.Id == ingreso.ProfesionalId);
            datos.ProfesionalIdAnt = ingreso.ProfesionalId;
            datos.ProfesionalNombreAnt = profAnt?.NombresApellidos; 
            datos.AsignacionResponsableAnt = ingreso.AsignacionResponsable;
            ingreso.AsignacionResponsable = datos.AsignacionResponsable;
            ingreso.ProfesionalId = datos.ProfesionalId;
            #endregion

            datos.ControlCambios = ControlCambios(ingreso);
            db.SaveChanges();

            return resultado;
        }

        public ResultadoOperacion UpdateEditarRequerimiento(RequerimientoDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);
            var camposModificados = new Dictionary<string, object>();

            var ingreso = db.Requerimiento
                .Include(r => r.Etiqueta)
                .Include(r => r.Soporte)
                .Include(r => r.TipoAdjunto)
                .Include(r => r.UnidadTecnicaCopia)
                .Include(r => r.MonumentoNacional)
                .Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                .Include(r => r.MonumentoNacional.Region)
                .Include(r => r.MonumentoNacional.Provincia)
                .Include(r => r.MonumentoNacional.Comuna)
                .FirstOrDefault(r => r.Id == datos.Id);

            if (ingreso == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el ingreso a actualizar.";
                return resultado;
            }

            // Bakup de datos actuales para control de cambio
            datos.BackupData.UtAsignadaId = ingreso.UtAsignadaId;
            datos.BackupData.PrioridadCod = ingreso.PrioridadCod;
            datos.BackupData.UnidadTecnicaCopia = ingreso.UnidadTecnicaCopia != null 
                ? ingreso.UnidadTecnicaCopia.Select(u => new GenericoDto {IdInt= u.Id, Titulo = u.Titulo}).ToList()
                : new List<GenericoDto>();
            datos.BackupData.UtConocimientoId = ingreso.UtConocimientoId;
            datos.BackupData.UtTemporalId = ingreso.UtTemporalId;
            // Registro de campos modificados (otros q no se contemplan en ControlCambios
            if (ingreso.EstadoId != datos.EstadoId) camposModificados["EstadoId"] = $"Id Estado Anterior: {ingreso.EstadoId}; Id Estado Nuevo: {datos.EstadoId} ";
            if (ingreso.EtapaId != datos.EtapaId) camposModificados["EtapaId"] = $"Id Etapa Anterior: {ingreso.EtapaId}; Id Etapa Nueva: {datos.EtapaId} ";
            if (ingreso.UtAsignadaId != datos.UtAsignadaId) camposModificados["UtAsignadaId"] = $"Id UT Asignada Anterior: {ingreso.UtAsignadaId}; Id UT Asigna Nueva: {datos.UtAsignadaId} ";
            if (ingreso.ProfesionalId != datos.ProfesionalId) camposModificados["ProfesionalId"] = $"Id Profesional UT Anterior: {ingreso.ProfesionalId}; Id Profesional UT Nuevo: {datos.ProfesionalId} ";
            //if (ingreso.ResponsableUtId != datos.ResponsableUtId) camposModificados["ResponsableUtId"] = $"Id Responsable UT Anterior: {ingreso.ResponsableUtId}; Id Etapa Nuevo: {datos.ResponsableUtId} ";
            // Se especifican los datos a actualizar
            ingreso.TipoIngreso = string.IsNullOrWhiteSpace(datos.TipoIngreso) ? ingreso.TipoIngreso : datos.TipoIngreso;
            #region Panel Documento

            if (DateTime.Compare(ingreso.FechaIngreso.Date, datos.FechaIngreso.Date) != 0)
            {
                ingreso.FechaIngreso = datos.FechaIngreso.Date + new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            }
            ingreso.TipoTramiteId = datos.TipoTramiteId;
            ingreso.CanalLlegadaTramiteCod = datos.CanalLlegadaTramiteCod;
            ingreso.TipoDocumentoCod = datos.TipoDocumentoCod;
            ingreso.ObservacionesTipoDoc = datos.ObservacionesTipoDoc;
            ingreso.FechaDocumento = datos.FechaDocumento;
            ingreso.EstadoId = datos.EstadoId;
            ingreso.EtapaId = datos.EtapaId;
            //ingreso.SiacTransparencia = datos.SiacTransparencia; // TODO
            #endregion
            #region Panel Adjuntos
            ingreso.AdjuntaDocumentacion = datos.AdjuntaDocumentacion;
            ingreso.CantidadAdjuntos = datos.CantidadAdjuntos;
            ingreso.ObservacionesAdjuntos = datos.ObservacionesAdjuntos;
            #endregion
            #region Panel Remitente
            ingreso.RemitenteId = datos.RemitenteId;
            #endregion
            #region Panel Proyecto
            ingreso.NombreProyectoPrograma = datos.NombreProyectoPrograma;
            ingreso.CasoId = datos.CasoId;
            ingreso.Materia = datos.Materia;
            #endregion
            #region Panel Monumento Nacional
            if (datos.MonumentoNacional != null)
            {
                ingreso.MonumentoNacional.CodigoMonumentoNac = datos.MonumentoNacional.CodigoMonumentoNac;
                ingreso.MonumentoNacional.DenominacionOficial = datos.MonumentoNacional.DenominacionOficial;
                ingreso.MonumentoNacional.OtrasDenominaciones = datos.MonumentoNacional.OtrasDenominaciones;
                ingreso.MonumentoNacional.NombreUsoActual = datos.MonumentoNacional.NombreUsoActual;
                ingreso.MonumentoNacional.DireccionMonumentoNac = datos.MonumentoNacional.DireccionMonumentoNac;
                ingreso.MonumentoNacional.ReferenciaLocalidad = datos.MonumentoNacional.ReferenciaLocalidad;
                ingreso.MonumentoNacional.RolSii = datos.MonumentoNacional.RolSii;

                // Update Categorías MN
                var cods2 = datos.MonumentoNacional.CategoriaMonumentoNac.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.CategoriaMonumentoNac = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.CategoriaMn)
                    .ToList();
                // Update Regiones
                cods2 = datos.MonumentoNacional.Region.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Region = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Region)
                    .ToList();
                // Update Provincias
                cods2 = datos.MonumentoNacional.Provincia.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Provincia = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Provincia)
                    .ToList();
                // Update Comunas
                cods2 = datos.MonumentoNacional.Comuna.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Comuna = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Comuna)
                    .ToList();
            }
            #endregion
            #region Panel General
            ingreso.FormaLlegadaCod = datos.FormaLlegadaCod;
            ingreso.ObservacionesFormaLlegada = datos.ObservacionesFormaLlegada;
            ingreso.CaracterId = datos.CaracterId;
            ingreso.ObservacionesCaracter = datos.ObservacionesCaracter;
            ingreso.Redireccionado = datos.Redireccionado;
            ingreso.NumeroTicket = datos.NumeroTicket;
            ingreso.RequiereAcuerdo = datos.RequiereAcuerdo;
            ingreso.RequerimientoAnteriorId = datos.RequerimientoAnteriorId;
            ingreso.RequerimientoNoRegistrado = datos.RequerimientoNoRegistrado;
            #endregion
            #region Panel Asignación
            if (!ingreso.UtAsignadaId.HasValue && datos.UtAsignadaId.HasValue)
            { // Se asignó la UT en el formulario de Editar Requerimiento
                ingreso.ResponsableUtId = datos.ResponsableUtId;
                ingreso.AsignacionUt = DateTime.Now;
            }
            ingreso.UtAsignadaId = datos.UtAsignadaId;
            ingreso.UtConocimientoId = datos.UtConocimientoId;
            ingreso.UtTemporalId = datos.UtTemporalId;
            ingreso.RequiereRespuesta = datos.RequiereRespuesta;
            ingreso.ComentarioAsignacion = datos.ComentarioAsignacion;
            ingreso.LiberarAsignacionTemp = datos.LiberarAsignacionTemp;
            #endregion
            #region Panel Priorización
            ingreso.PrioridadCod = datos.PrioridadCod;
            ingreso.Plazo = datos.Plazo;
            ingreso.Resolucion = datos.Resolucion;
            ingreso.SolicitanteUrgenciaId = datos.SolicitanteUrgenciaId;
            #endregion

            #region Relaciones de muchos-muchos
            // Update Tipos de Adjunto
            var cods = datos.TipoAdjunto.Select(d => d.Id).ToList();
            ingreso.TipoAdjunto = db.ListaValor
                .Where(x => cods.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.TipoDocumento)
                .ToList();
            // Update Soportes
            cods = datos.Soporte.Select(d => d.Id).ToList();
            ingreso.Soporte = db.ListaValor
                .Where(x => cods.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Soporte)
                .ToList();
            // Update Etiquetas
            cods = datos.Etiqueta.Select(d => d.Id).ToList();
            ingreso.Etiqueta = db.ListaValor.Where(x => cods.Any(d => d == x.Codigo && x.IdLista == (int)Mantenedor.Etiqueta)).ToList();
            // Update UT en Copia
            var ids2 = datos.UnidadTecnicaCopia.Select(d => d.IdInt).ToList();
            ingreso.UnidadTecnicaCopia = db.UnidadTecnica.Where(x => ids2.Any(d => d == x.Id)).ToList();

            EvitaInsertHijo(ingreso);

            #endregion
            #region SIAC/Transparencia
            ingreso.UtTransparenciaId = datos.UtTransparenciaId;
            ingreso.ProfesionalTranspId = datos.ProfesionalTranspId;
            ingreso.ObservacionesTransparencia = datos.ObservacionesTransparencia;
            #endregion

            datos.ControlCambios = ControlCambios(ingreso);
            foreach (var pair in camposModificados)
            {
                if (!datos.ControlCambios.CamposModificados.ContainsKey(pair.Key))
                    datos.ControlCambios.CamposModificados.Add(pair.Key, pair.Value);
            }
            db.SaveChanges();

            return resultado;
        }

        public ResultadoOperacion UpdateEditarCamposUt(RequerimientoDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var ingreso = db.Requerimiento
                .Include(r => r.Etiqueta)
                .Include(r => r.Soporte)
                .Include(r => r.TipoAdjunto)
                .Include(r => r.MonumentoNacional)
                .Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                .Include(r => r.MonumentoNacional.Region)
                .Include(r => r.MonumentoNacional.Provincia)
                .Include(r => r.MonumentoNacional.Comuna)
                .FirstOrDefault(r => r.Id == datos.Id);

            if (ingreso == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el ingreso a actualizar.";
                return resultado;
            }

            // Bakup de datos actuales para control de cambio
            datos.BackupData.UtApoyoIdAnt = ingreso.UtApoyoId;
            // Se especifican los datos a actualizar
            if (datos.LiberarAsignacionTemp &&
                !datos.LiberarAsignacionTempAnt &&
                datos.UtTemporalId.GetValueOrDefault(0) > 0)
            { // Se libera la asignación temporal
                ingreso.LiberarAsignacionTemp = datos.LiberarAsignacionTemp;
                ingreso.EstadoId = datos.EstadoId;
                ingreso.EtapaId = datos.EtapaId;
                ingreso.ResponsableUtId = datos.ResponsableUtId;
                ingreso.EnviadoUt = datos.EnviadoUt;
                ingreso.Liberacion = datos.Liberacion;
            }
            #region Panel Documento
            ingreso.TipoTramiteId = datos.TipoTramiteId;
            //ingreso.SiacTransparencia = datos.SiacTransparencia; // TODO
            #endregion
            #region Panel Adjuntos
            #endregion
            #region Panel Remitente
            #endregion
            #region Panel Proyecto
            ingreso.NombreProyectoPrograma = datos.NombreProyectoPrograma;
            ingreso.CasoId = datos.CasoId;
            #endregion
            #region Panel Monumento Nacional
            if (datos.MonumentoNacional != null)
            {
                ingreso.MonumentoNacional.CodigoMonumentoNac = datos.MonumentoNacional.CodigoMonumentoNac;
                ingreso.MonumentoNacional.DenominacionOficial = datos.MonumentoNacional.DenominacionOficial;
                ingreso.MonumentoNacional.OtrasDenominaciones = datos.MonumentoNacional.OtrasDenominaciones;
                ingreso.MonumentoNacional.NombreUsoActual = datos.MonumentoNacional.NombreUsoActual;
                ingreso.MonumentoNacional.DireccionMonumentoNac = datos.MonumentoNacional.DireccionMonumentoNac;
                ingreso.MonumentoNacional.ReferenciaLocalidad = datos.MonumentoNacional.ReferenciaLocalidad;
                ingreso.MonumentoNacional.RolSii = datos.MonumentoNacional.RolSii;

                // Update Categorías MN
                var cods2 = datos.MonumentoNacional.CategoriaMonumentoNac.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.CategoriaMonumentoNac = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.CategoriaMn)
                    .ToList();
                // Update Regiones
                cods2 = datos.MonumentoNacional.Region.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Region = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Region)
                    .ToList();
                // Update Provincias
                cods2 = datos.MonumentoNacional.Provincia.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Provincia = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Provincia)
                    .ToList();
                // Update Comunas
                cods2 = datos.MonumentoNacional.Comuna.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Comuna = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Comuna)
                    .ToList();
            }
            #endregion
            #region Otros campos
            ingreso.RequerimientoAnteriorId = datos.RequerimientoAnteriorId;
            ingreso.RequerimientoNoRegistrado = datos.RequerimientoNoRegistrado;
            ingreso.ProyectoActividad = datos.ProyectoActividad;
            ingreso.UtApoyoId = datos.UtApoyoId;
            #endregion
            #region SIAC/Transparencia
            ingreso.UtTransparenciaId = datos.UtTransparenciaId;
            ingreso.ProfesionalTranspId = datos.ProfesionalTranspId;
            ingreso.ObservacionesTransparencia = datos.ObservacionesTransparencia;
            #endregion

            #region Relaciones de muchos-muchos
            // Update Etiquetas
            var cods = datos.Etiqueta.Select(d => d.Id).ToList();
            ingreso.Etiqueta = db.ListaValor.Where(x => cods.Any(d => d == x.Codigo && x.IdLista == (int)Mantenedor.Etiqueta)).ToList();

            EvitaInsertHijo(ingreso);

            #endregion

            datos.ControlCambios = ControlCambios(ingreso);
            db.SaveChanges();

            return resultado;
        }

        public ResultadoOperacion UpdateCierre(RequerimientoDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var ingreso = db.Requerimiento
                .Include(r => r.Etiqueta)
                .Include(r => r.Soporte)
                .Include(r => r.TipoAdjunto)
                .Include(r => r.MonumentoNacional)
                .Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                .Include(r => r.MonumentoNacional.Region)
                .Include(r => r.MonumentoNacional.Provincia)
                .Include(r => r.MonumentoNacional.Comuna)
                .FirstOrDefault(r => r.Id == datos.Id);

            if (ingreso == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el ingreso a actualizar.";
                return resultado;
            }

            // Se especifican los datos a actualizar
            #region Datos propios del flujo

            ingreso.Cierre = datos.Cierre;
            ingreso.CerradoPorId = datos.CerradoPorId;
            if (datos.EstadoId > 0) ingreso.EstadoId = datos.EstadoId;
            #endregion
            #region Panel Documento
            ingreso.TipoTramiteId = datos.TipoTramiteId;
            #endregion
            #region Panel Proyecto
            ingreso.NombreProyectoPrograma = datos.NombreProyectoPrograma;
            ingreso.CasoId = datos.CasoId;
            #endregion
            #region Panel Monumento Nacional
            if (datos.MonumentoNacional != null)
            {
                ingreso.MonumentoNacional.CodigoMonumentoNac = datos.MonumentoNacional.CodigoMonumentoNac;
                ingreso.MonumentoNacional.DenominacionOficial = datos.MonumentoNacional.DenominacionOficial;
                ingreso.MonumentoNacional.OtrasDenominaciones = datos.MonumentoNacional.OtrasDenominaciones;
                ingreso.MonumentoNacional.NombreUsoActual = datos.MonumentoNacional.NombreUsoActual;
                ingreso.MonumentoNacional.DireccionMonumentoNac = datos.MonumentoNacional.DireccionMonumentoNac;
                ingreso.MonumentoNacional.ReferenciaLocalidad = datos.MonumentoNacional.ReferenciaLocalidad;
                ingreso.MonumentoNacional.RolSii = datos.MonumentoNacional.RolSii;

                // Update Categorías MN
                var cods2 = datos.MonumentoNacional.CategoriaMonumentoNac.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.CategoriaMonumentoNac = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.CategoriaMn)
                    .ToList();
                // Update Regiones
                cods2 = datos.MonumentoNacional.Region.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Region = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Region)
                    .ToList();
                // Update Provincias
                cods2 = datos.MonumentoNacional.Provincia.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Provincia = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Provincia)
                    .ToList();
                // Update Comunas
                cods2 = datos.MonumentoNacional.Comuna.Select(d => d.Id).ToList();
                ingreso.MonumentoNacional.Comuna = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Comuna)
                    .ToList();
            }
            #endregion
            #region Panel Cierre
            ingreso.ComentarioCierre = datos.ComentarioCierre;
            ingreso.MotivoCierreId = datos.MotivoCierreId;
            #endregion

            #region Relaciones de muchos-muchos
            // Update Etiquetas
            var cods = datos.Etiqueta.Select(d => d.Id).ToList();
            ingreso.Etiqueta = db.ListaValor.Where(x => cods.Any(d => d == x.Codigo && x.IdLista == (int)Mantenedor.Etiqueta)).ToList();

            EvitaInsertHijo(ingreso);

            #endregion

            datos.ControlCambios = ControlCambios(ingreso);
            db.SaveChanges();

            return resultado;
        }

        public List<RequerimientoDto> GetDatosCierreMultiple(int idUT, int idProfesional, DateTime fechaDesde, DateTime fechaHasta)
        {
            var reqs = db.Requerimiento
                .Include(i => i.Remitente)
                .Include(i => i.EstadoRequerimiento)
                .Where(r => r.UnidadTecnicaAsign.Id == idUT 
                            && !r.Eliminado
                            && r.FechaIngreso >= fechaDesde && r.FechaIngreso < fechaHasta
                            && (idProfesional == 0 || r.ProfesionalId == idProfesional)
                            && (r.EstadoId == (int)EstadoIngreso.EnProcesoEnEstudio || r.EstadoId == (int)EstadoIngreso.EnProcesoAcuerdoComisión || r.EstadoId == (int)EstadoIngreso.EnProcesoAcuerdoSesión));
            
            var datos = reqs.Select(_mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>).ToList();
            return datos;
        }

        public ResultadoOperacion CierreMultiple(RequerimientoDto datos, UsuarioActualDto usuario)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            try
            {
                var ingreso = db.Requerimiento
                    .FirstOrDefault(r => r.Id == datos.Id && r.EstadoId != (int)EstadoIngreso.Cerrado);

                if (ingreso == null)
                {
                    return new ResultadoOperacion(-1, "No se encontró el requerimiento a cerra o ya se encuentra cerrado.", null);
                }

                // Se especifican los datos a actualizar
                ingreso.Cierre = datos.Cierre;
                ingreso.CerradoPorId = datos.CerradoPorId;
                ingreso.EstadoId = datos.EstadoId == (int)EstadoIngreso.Cerrado ? datos.EstadoId : ingreso.EstadoId;
                ingreso.ComentarioCierre = datos.ComentarioCierre;
                ingreso.MotivoCierreId = datos.MotivoCierreId;

                db.SaveChanges();

                resultado.Extra = _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(ingreso);
            } catch (Exception exc)
            {
                Logger.LogError(exc);
                resultado = new ResultadoOperacion(-1, "Error en cierre múltiple. Requerimiento ID: " + datos.Id, null);
            }
            return resultado;
        }

        public List<RequerimientoDto> UpdateForzarPrioridad(List<int> idReqs, DateTime? nuevaFecha)
        {
            var datos = db.Requerimiento.Where(r => idReqs.Contains(r.Id));
            foreach (var requerimiento in datos)
            {
                requerimiento.ForzarPrioridad = nuevaFecha.HasValue;
                requerimiento.ForzarPrioridadFecha = nuevaFecha;
                requerimiento.ForzarPrioridadMotivo = ""; // TODO: precisar si se especifica algún motivo, y si se conserva el anterior q tenga en caso de tener alguno (ahora se borra si tenía un motivo previo)
            }
            db.SaveChanges();

            return datos.AsEnumerable().Select(r => _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(r)).ToList();
        }

        private void EvitaInsertHijo(Requerimiento ingreso)
        {
            // Relaciones muchos a muchos. Se evita q se intente insertar en tablas hijas.
            foreach (var det in ingreso.Etiqueta ?? Enumerable.Empty<ListaValor>())
            {
                db.Entry(det).State = EntityState.Unchanged;
            }
            foreach (var det in ingreso.Soporte ?? Enumerable.Empty<ListaValor>())
            {
                db.Entry(det).State = EntityState.Unchanged;
            }
            foreach (var det in ingreso.TipoAdjunto ?? Enumerable.Empty<ListaValor>())
            {
                db.Entry(det).State = EntityState.Unchanged;
            }
            foreach (var det in ingreso.UnidadTecnicaCopia ?? Enumerable.Empty<UnidadTecnica>())
            {
                db.Entry(det).State = EntityState.Unchanged;
            }

            if (ingreso.MonumentoNacional != null)
            {
                foreach (var det in ingreso.MonumentoNacional.CategoriaMonumentoNac ?? Enumerable.Empty<ListaValor>())
                {
                    db.Entry(det).State = EntityState.Unchanged;
                }
                foreach (var det in ingreso.MonumentoNacional.Region ?? Enumerable.Empty<ListaValor>())
                {
                    db.Entry(det).State = EntityState.Unchanged;
                }
                foreach (var det in ingreso.MonumentoNacional.Provincia ?? Enumerable.Empty<ListaValor>())
                {
                    db.Entry(det).State = EntityState.Unchanged;
                }
                foreach (var det in ingreso.MonumentoNacional.Comuna ?? Enumerable.Empty<ListaValor>())
                {
                    db.Entry(det).State = EntityState.Unchanged;
                }
            }
        }

        public RequerimientoDto MarcaEliminado(int reqId, int usuarioId)
        {
            var req = db.Requerimiento.FirstOrDefault(a => a.Id == reqId);
            if (req != null)
            {
                req.EliminacionFecha = DateTime.Now;
                req.Eliminado = true;
                req.UsuarioEliminacionId = usuarioId;

                // Se envian a papelera las bitácoras del ingreso:
                var bitacoras = db.Bitacora.Where(b => b.RequerimientoId == req.Id);
                foreach (var bitacora in bitacoras)
                {
                    bitacora.EliminacionFecha = DateTime.Now;
                    bitacora.Eliminado = true;
                    bitacora.UsuarioEliminacionId = usuarioId;
                }

                db.SaveChanges();
            }

            return _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(req);
        }

        public RequerimientoDto UpdateByBitacora(string tipoBitCod, int reqId)
        {
            var req = db.Requerimiento.FirstOrDefault(r => r.Id == reqId);
            var tipoBit = db.ListaValor.FirstOrDefault(t => t.IdLista == (int)Mantenedor.TipoBitacora && t.Codigo == tipoBitCod);
            if (tipoBit != null)
            {
                if (req != null)
                {
                    //var estadosReq = db.EstadoRequerimiento;
                    //if (tipoBit.Titulo == "Acuerdo Comisión")
                    //    req.EstadoId = estadosReq.FirstOrDefault(e => e.Titulo == "En proceso - Acuerdo de comisión").Id;
                    //if (tipoBit.Titulo == "Acuerdo Sesión")
                    //    req.EstadoId = estadosReq.FirstOrDefault(e => e.Titulo == "En proceso - Acuerdo de sesión").Id;

                    //var etapaReq = db.EtapaRequerimiento.FirstOrDefault(e => e.Titulo == "Unidad Técnica");
                    //req.EtapaId = etapaReq.Id;

                    if (req.EstadoId != (int) EstadoIngreso.Ingresado &&
                        req.EstadoId != (int) EstadoIngreso.Archivado && 
                        req.EstadoId != (int) EstadoIngreso.Cerrado)
                    {
                        if (tipoBit.Codigo == TipoBitacora.AcuerdoComision.ToString("D"))
                            req.EstadoId = (int)EstadoIngreso.EnProcesoAcuerdoComisión;
                        if (tipoBit.Codigo == TipoBitacora.AcuerdoSesion.ToString("D"))
                            req.EstadoId = (int)EstadoIngreso.EnProcesoAcuerdoSesión;

                        req.EtapaId = (int)EtapaIngreso.UnidadTecnica;
                    }

                    db.SaveChanges();
                }
                else
                {
                    Helpers.Logging.Logger.LogInfo(
                        $"Atención. No se encontró el id {reqId} de requerimiento al actualizar el requerimiento.");
                }
            }
            else
            {
                Helpers.Logging.Logger.LogInfo(
                    $"Atención. No se encontró el id {tipoBitCod} de tipo de bitácora al actualizar el requerimiento.");
            }

            return _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(req);
        }

        public ResultadoOperacion GetRequirimientoDetalle(RequerimientoDto ingresoDto)
        {
            var resultado = new ResultadoOperacion(1, "OK", null);

            var ingreso = db.Requerimiento
                .Include(r => r.Etiqueta)
                .Include(r => r.Soporte)
                .Include(r => r.TipoAdjunto)
                .Include(r => r.UnidadTecnicaAsign)
                .Include(r => r.UnidadTecnicaCopia)
                .Include(r => r.UnidadTecnicaAnterior)
                .Include(r => r.UnidadTecnicaTemp)
                .Include(r => r.UnidadTecnicaApoyo)
                .Include(r => r.UnidadTecnicaConoc)
                .Include(r => r.Remitente)
                .Include(r => r.ProfesionalUt)
                .Include(r => r.MotivoCierre)
                .Include(r => r.MonumentoNacional)
                //.Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                //.Include(r => r.MonumentoNacional.Region)
                //.Include(r => r.MonumentoNacional.Provincia)
                //.Include(r => r.MonumentoNacional.Comuna)
                .FirstOrDefault(r => r.Id == ingresoDto.Id);
            if (ingreso == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el requerimiento especificado";
            }
            else
            {
                // Se actualiza el ingreso especifcado de parametro con todos los datos obtenidos desde Bd
                _mapper.MapFromOrigenToDestino<Requerimiento, RequerimientoDto>(ingreso, ingresoDto);
            }

            return resultado;

        }

        public DatosAjax<List<RequerimientoDto>> GetDatosUltimos(int diasAtras)
        {
            var datos = db.sp_IngresosUltimos(diasAtras)
                .Select(_mapper.MapFromModelToDto<sp_IngresosUltimos_Result, RequerimientoDto>).ToList();
            var resultado = new DatosAjax<List<RequerimientoDto>>(datos, new ResultadoOperacion(1, "OK", null));
            return resultado;
        }

        public RequerimientoDto GetById(int id)
        {
            var datos = db.Requerimiento
                .Include(r => r.EstadoRequerimiento)
                .Include(r => r.EtapaRequerimiento)
                .Include(r => r.TipoDocumento)
                .Include(r => r.TipoAdjunto)
                .Include(r => r.Remitente)
                .Include(r => r.MonumentoNacional)
                .Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                .Include(r => r.MonumentoNacional.Region)
                .Include(r => r.MonumentoNacional.Provincia)
                .Include(r => r.MonumentoNacional.Comuna)
                .Include(r => r.Prioridad)
                .Include(r => r.CanalLlegadaTramite)
                .Include(r => r.Etiqueta)
                .Include(r => r.Soporte)
                .Include(r => r.Caracter)
                .Include(r => r.Caso)
                .Include(r => r.UnidadTecnicaAsign)
                .Include(r => r.UnidadTecnicaApoyo)
                .Include(r => r.UnidadTecnicaConoc)
                .Include(r => r.UnidadTecnicaTemp)
                .Include(r => r.UnidadTecnicaTransp)
                .Include(r => r.UnidadTecnicaCopia)
                .Include(r => r.SolicitanteUrgencia)
                .FirstOrDefault(r => r.Id == id);
            return _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(datos);
        }

        public RequerimientoDto GetByDocumentoIngreso(string docingreso)
        {
            var datos = db.Requerimiento
                .Include(r => r.EstadoRequerimiento)
                .Include(r => r.EtapaRequerimiento)
                .Include(r => r.TipoDocumento)
                .Include(r => r.TipoAdjunto)
                .Include(r => r.Remitente)
                .Include(r => r.Prioridad)
                .Include(r => r.CanalLlegadaTramite)
                .Include(r => r.Etiqueta)
                .Include(r => r.Soporte)
                .Include(r => r.Caracter)
                .Include(r => r.ProfesionalUt)
                .Include(r => r.UnidadTecnicaAsign)
                .Include(r => r.UnidadTecnicaApoyo)
                .Include(r => r.UnidadTecnicaConoc)
                .Include(r => r.UnidadTecnicaTemp)
                .Include(r => r.UnidadTecnicaTransp)
                .Include(r => r.UnidadTecnicaCopia)
                .FirstOrDefault(r => r.DocumentoIngreso == docingreso);
            return _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(datos);
        }

        public RequerimientoDto GetResumenByDocumentoIngreso(string docingreso)
        {
            var datos = db.Requerimiento
                .FirstOrDefault(r => r.DocumentoIngreso == docingreso);
            return _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(datos);
        }

        public RequerimientoDto GetBySolicitudId(int idSolic)
        {
            var datos = db.Requerimiento
                .FirstOrDefault(r => r.SolicitudTramId == idSolic);
            return _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(datos);
        }

        public RequerimientoDto GetFichaById(int id, bool fullDatos = false)
        {
            var datos = db.vw_FichaIngreso
                .FirstOrDefault(r => r.Id == id);
            var datosDto = _mapper.MapFromModelToDto<vw_FichaIngreso, RequerimientoDto>(datos);

            if (fullDatos)
            {
                // Despachos del requerimiento
                var datosDesp = db.Despacho
                    .Include(d => d.Requerimiento)
                    .Include(d => d.Remitente)
                    .Include(d => d.EstadoDespacho)
                    .Include(d => d.ProveedorDespacho)
                    .Where(d => d.Requerimiento.Any(r => r.Id == id))
                    .OrderBy(d => d.FechaEmisionOficio)
                    .AsEnumerable()
                    .Select(d => _mapper.MapFromModelToDto<Despacho, DespachoDto>(d))
                    .ToList();
                datosDto.Despachos = datosDesp;
                // Adjuntos del requeirimiento
                var datosAdj = db.Adjunto
                    .Include(a => a.UsuarioCreacion)
                    .Where(d => d.RequerimientoId == id)
                    .OrderBy(a => a.FechaCarga)
                    .AsEnumerable()
                    .Select(d => _mapper.MapFromModelToDto<Adjunto, AdjuntoDto>(d))
                    .ToList();
                datosDto.Adjuntos = datosAdj;
                // Bitacoras del requeirimiento
                //var datosBit = db.Bitacora
                //    .Include(d => d.Requerimiento)
                //    .Include(d => d.TipoBitacora)
                //    .Include(d => d.UsuarioCreacion)
                //    .Where(d => d.RequerimientoId == id)
                //    .OrderBy(b => b.Fecha)
                //    .AsEnumerable()
                //    .Select(d => _mapper.MapFromModelToDto<Bitacora, BitacoraDto>(d))
                //    .ToList();
                //datosDto.Bitacoras = datosBit;

                // Nueva Bitácora (log de requerimeinto)
                var datosLogBita = GetLogBitacoraRequerimiento(id);
                datosDto.LogBitacoras = datosLogBita;

            }

            return datosDto;
        }

        public vw_FichaIngreso GetFichaFullById(int id, bool fullDatos = false)
        {
            var datos = db.vw_FichaIngreso
                .FirstOrDefault(r => r.Id == id);

            return datos;
        }

        public string GetFechasEmisionOficioReq(int idReq)
        {
            var fechas = db.Requerimiento
                .Include(r => r.Despacho)
                .Where(r => r.Id == idReq)
                .Select(r => r.Despacho.Select(d => d.FechaEmisionOficio))
                .FirstOrDefault();
            return string.Join("; ", (fechas ?? new List<DateTime?>()).Select(f => !f.HasValue ? "" : f.Value.ToString(GeneralData.FORMATO_FECHA_CORTO)));
        }

        public List<RequerimientoDto> GetResumenAll(bool soloCerrados)
        {
            var datos = db.Requerimiento
                .Include(r => r.EstadoRequerimiento)
                .Include(r => r.EtapaRequerimiento)
                .Include(r => r.UnidadTecnicaAsign)
                .Where(r => !soloCerrados || (soloCerrados && r.EstadoId == (int)EstadoIngreso.Cerrado) )
                .OrderBy(r => DbFunctions.Right("00000000000000000000000000000000000000000000000000" + r.DocumentoIngreso, 50))
                .Select(r => new RequerimientoDto()
                {
                    Id = r.Id,
                    EstadoId = r.EstadoId,
                    EtapaId = r.EtapaId,
                    //UnidadTecnicaAsignId = r.UtAsignadaId.Value,
                    DocumentoIngreso = r.DocumentoIngreso
                })
                .ToList();
            return datos;
        }

        public List<GenericoDto> GetRequerimientoResumenByIds(List<int> ids, bool soloCerrados)
        {
            var query = db.Requerimiento.Where(r => ids.Contains(r.Id));
            var datos = query
                .OrderBy(r => r.DocumentoIngreso)
                .AsEnumerable()
                .Where(r => !r.Eliminado && (!soloCerrados || r.EstadoId == (int)EstadoIngreso.Cerrado) )
                .Select(r => _mapper.MapFromModelToDto<Requerimiento, GenericoDto>(r)).ToList();
            return datos;
        }

        public List<RequerimientoDto> GetRequerimientoByIds(List<int> ids, bool soloCerrados)
        {
            var query = db.Requerimiento
                .Include(r => r.ProfesionalUt)
                .Where(r => ids.Contains(r.Id));
            var datos = query
                .OrderBy(r => r.DocumentoIngreso)
                .AsEnumerable()
                .Where(r => !soloCerrados || r.EstadoRequerimiento.Titulo == "Cerrado")
                .Select(r => _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(r)).ToList();
            return datos;
        }

        public DatosAjax<List<RequerimientoDto>> GetDatosBandejaEntrada(int idBandeja, int codigoBandeja, int skip, int take,
            SortParam sort, string filterText, string filtroSql, object[] filtroSqlParams, DateTime fechaDesde, int idUsuario, int? DocumentoIngreso, DateTime? FechaHasta, 
            int? UnidadTecnica, int? Estado)
        {
            var hayFiltroExtra = !string.IsNullOrEmpty(filterText) ? 1 : 0;
            var esAdmin = db.Usuario
                          .FirstOrDefault(u => u.Id == idUsuario && u.Rol.Any(r => r.Id == (int)Rol.Administrador)) != null;
            // UTs de las q el usuario es integrante. Si es mostrar la bandeja de Encargado UT se obtienen las UT de las q el usuario en Responsable o Subrogantee
            var utsUsuario = codigoBandeja == (int)Bandeja.EncargadoUt
                ? db.UnidadTecnica
                    .Where(u => u.ResponsableId == idUsuario || u.SubroganteId == idUsuario)
                    .Select(u => u.Id)
                    .ToList()
                : db.UnidadTecnica
                .Where(u => u.UsuariosUt.Any(user => user.Id == idUsuario))
                .Select(u => u.Id)
                .ToList();

            IQueryable<vw_BandejaEntrada> query = null;
            List<RequerimientoDto> datos = null;

            if (DocumentoIngreso.HasValue || FechaHasta.HasValue || UnidadTecnica.HasValue || Estado.HasValue)
            {
                if (DocumentoIngreso.HasValue)
                {
                    query = db.vw_BandejaEntrada.Where(d => d.BandejaId == idBandeja && d.Id == DocumentoIngreso.Value);
                }
                else
                {
                    DateTime fh = FechaHasta.Value.Date.AddDays(1);
                    query = db.vw_BandejaEntrada
                    .Where(d => d.FechaIngreso >= fechaDesde && d.FechaIngreso < fh && d.BandejaId == idBandeja);

                    if (UnidadTecnica.HasValue)
                        query = query.Where(d => d.UtAsignadaId == UnidadTecnica.Value);

                    if (Estado.HasValue)
                        query = query.Where(d => d.EstadoId == Estado.Value);
                }
            }
            else
            {
                query = codigoBandeja == (int)Bandeja.Historico // Bandeja de Históricos no tiene en cuenta el rango de fechas
                    ? db.vw_BandejaEntrada.Where(d => d.BandejaId == idBandeja)
                    : db.vw_BandejaEntrada.Where(d => d.FechaIngreso >= fechaDesde && d.BandejaId == idBandeja);
            }

            /* filtros aplicados en las columnas de la grilla*/
            if (!string.IsNullOrWhiteSpace(filtroSql))
            {
                query = query.Where(filtroSql, filtroSqlParams);
            }

            /* Lógica implementada en el actual Gedoc Sharepoint:
             * Los ingresos a mostrar en cada bandeja están condicionados por el rango de fechas y por el Estado y Etapa q se configure en la aplicación pero además se tienen en cuenta las siguientes condiciones:
             * Bandeja Ingreso Central: <no contempla filtro adicional, solo los configurados según rango de fechas y estado y etapa de los ingresos>
             * Bandeja Asignación: <no contempla filtro adicional, solo los configurados según rango de fechas y estado y etapa de los ingresos>
             * Bandeja Priorización: <no contempla filtro adicional, solo los configurados según rango de fechas y estado y etapa de los ingresos>
             * Bandeja Encargado UT: se muestran los ingresos de las UT q el usuario sea Responsable UT (se chequea tanto en UT asignada, temporal o transparencia).Si es administrador tiene acceso a todas las UT
             * Bandeja Profesional UT: se muestran los ingresos q han sido asignados al usuario (como Profesional UT) tanto par la UT asignada, o para la temporal si aún no ha sido liberada la asignación temporal.Si es administrador tiene acceso a todas las UT.
             * Bandeja Secreataria UT: se muestran los ingresos de las UT del usuario (tanto en UT asignada, temporal o transparencia).Si es administrador tiene acceso a todas las UT
             * Bandeja Jefatura CMN:   se muestran los ingresos de las UT del usuario (tanto en UT asignada, temporal o transparencia).Si es administrador tiene acceso a todas las UT
             * Bandeja Transparencia: se muestran todos los ingresos marcados con el check SIAC/TRANSPARENCIA, no se tiene en cuenta la UT del usuario.
             * Bandeja Históricos: se muestran los ingresos q TipoIngreso es "Ingreso historico", no estén cerrados, no tiene en cuenta el rango de fechas configurado para las bandejas, y además q sean de las UT del usuario (en UT asignada).Si es administrador tiene acceso a todas las UT
             * Bandeja Despachos: <no contempla filtro adicional, solo los configurados según rango de fechas y estado y etapa de los ingresos>
             
             * Grilla de Nueva Priorización de Encargado (se maneja como una Bandeja de "Priorización de Encargado UT" pero es una grilla dentro de la bandeja de encargo UT. Sigue las mismas reglas de esta bandeja.
             */
            //Se separan las query en vez de hacer una query q contemple todos los posibles filtros
            switch (codigoBandeja)
            {
                case (int)Bandeja.EncargadoUt:
                    query = query.Where(d =>
                            esAdmin ||
                            //( d.ResponsableUtId == idUsuario ||
                            //   (d.ResponsableUtTempId == idUsuario && !(d.LiberarAsignacionTemp == true))
                            // ) // <-- Si se habilitan estas condiciones entonces se obtienen los ingresos de las uts q el usuario era responsable al momento de la asignación de UT, aunque no sea el actual responsable de la UT
                            utsUsuario.Any(idUt => d.UtAsignadaId == idUt ||
                                                   (d.UtTemporalId == idUt && !(d.LiberarAsignacionTemp == true)) ||
                                                   d.UtTransparenciaId == idUt
                            ) // <-- Con estas condiciones se devuelven los ingresos de las UTs q el usuario es actualmente el Responsable);
                    );
                    break;
                case (int)Bandeja.ProfesionalUt:
                    query = query.Where(d =>
                        (esAdmin || d.ProfesionalId == idUsuario ||
                         (d.ProfesionalTempId == idUsuario && !(d.LiberarAsignacionTemp == true))) );
                    break;
                case (int)Bandeja.SecretariaUt:
                case (int)Bandeja.JefaturaUt:
                    query = query.Where(d =>
                        esAdmin || utsUsuario.Any(idUt => d.UtAsignadaId == idUt ||
                                                           (d.UtTemporalId == idUt && !(d.LiberarAsignacionTemp == true)) ||
                                                           d.UtTransparenciaId == idUt ) );
                    break;
                case (int)Bandeja.Transparencia:
                    query = query.Where(d => d.TipoIngreso == "SIAC/TRANSPARENCIA");
                    break;
                case (int)Bandeja.Historico:
                    query = query.Where(d => d.TipoIngreso == "Ingreso historico" && d.EstadoId != (int)EstadoIngreso.Cerrado &&
                                            (esAdmin || utsUsuario.Any(idUt => d.UtAsignadaId == idUt))   );
                    break;
                case (int)Bandeja.Despachos:
                case (int)Bandeja.IngresoCentral:
                case (int)Bandeja.Asignacion:
                case (int)Bandeja.Priorizacion:
                default:
                    // Nada
                    break;
            }
            if (hayFiltroExtra == 1)
                query = query.Where(d => d.DocumentoIngreso.Contains(filterText) || d.RemitenteNombre.Contains(filterText) ||
                                 d.RemitenteInstitucion.Contains(filterText) || d.Materia.Contains(filterText) || d.EstadoTitulo.Contains(filterText) ||
                                 d.UtAsignadaTitulo.Contains(filterText) || d.ProfesionalNombre.Contains(filterText));

            datos = query
                .OrderBy($"{sort.Field} {sort.Dir}")
                .Skip(skip)
                .Take(take)
                .AsEnumerable()
                .Select(d => _mapper.MapFromModelToDto<vw_BandejaEntrada, RequerimientoDto>(d))
                .ToList();

            var resultado = new DatosAjax<List<RequerimientoDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;
        }

        public DatosAjax<List<GroupResult>> GetDatosBandejaPriorizados(int idBandeja, int codigoBandeja, int skip, int take,
            SortParam sort, string filterText, string filtroSql, object[] filtroSqlParams, DateTime fechaDesde, int idUsuario, int? DocumentoIngreso, DateTime? FechaHasta,
            int? UnidadTecnica, int? Estado, string tipoOper)
        {
            /* De acuerdo al parámetro tipoOper, este método puede devolver:
             tipoOper = "FILTROS" - se devuelven los datos reducidos (datos de las columnas de filtros con un distinct) necesarios para los 
                                    filtros de las grillas. Se cargan todos los registros de acuerdo a los filtros (filtros de columnas, cajón 
                                    de Buscar, y Búsqueda en Bandeja), NO  se utiliza paginación
             tipoOper = "IDREQ" - se devuelven los datos reducidos (solo id de requerimientos) necesarios para actualizar la Nueva Fecha de 
                                    Resolución cuando se va a actualizar en todos los registros qde la grilla. Se cargan todos los registros 
                                    de acuerdo a los filtros, NO  se utiliza paginación
             tipoOper = "NOTIF" - se devuelven los datos reducidos (solo id de requerimientos) de los registros q tengan asignado la Nueva Fecha de 
                                    Resolución Estimada, esto se utiliza para tomar la cantidad de registros y mostralo en la cantidad de notificaciones
                                    en la bandeja. Se cargan todos los registros de acuerdo al filtro mencionado, NO  se utiliza paginación
             tipoOper diferente a los anteriores - se devuelven los datos necesarios para la grilla de priorizados. Sí utiliza la 
                                    paginación teniendo además en cuenta los filtros.
             Todos los datos anteriores se obtienen siguiendo los mismos filtros, solo cambia los campos a cargar y si tendrá paginación o se cargan todos los registros
             */

            // Si tipoOper == "NOTIF" entonces se ignoran los filtros q se hayan aplicado a la grilla, tanto por filtro de columnas,
            // Búsqueda en Bandeja, o cajón de Buscar, u otra condición. Pero se agrega la condición de q la Nueva Fecha de Asignación
            // Estimnada no sea null
            if (tipoOper == "NOTIF")
            {
                filtroSql = "ForzarPrioridad == true"; // TODO: ¿o debería ser  "ForzarPrioridadFecha != null"?
                filterText = "";
                DocumentoIngreso = null;
                FechaHasta = null;
                UnidadTecnica = null;
                Estado = null;
            }
            var hayFiltroExtra = !string.IsNullOrEmpty(filterText) ? 1 : 0;
            var esAdmin = db.Usuario
                          .FirstOrDefault(u => u.Id == idUsuario && u.Rol.Any(r => r.Id == (int)Rol.Administrador)) != null;
            // UTs de las q el usuario es integrante. Si es mostrar la bandeja de Encargado UT se obtienen las UT de las q el usuario es Encargado UT o Subrigante
            var utsUsuario = codigoBandeja == (int)Bandeja.PriorizacionEncargado
                ? db.UnidadTecnica
                    .Where(u => u.ResponsableId == idUsuario || u.SubroganteId == idUsuario)
                    .Select(u => u.Id)
                    .ToList()
                : db.UnidadTecnica
                .Where(u => u.UsuariosUt.Any(user => user.Id == idUsuario))
                .Select(u => u.Id)
                .ToList();

            IQueryable<vw_BandejaEntrada> query = null;

            if (DocumentoIngreso.HasValue || FechaHasta.HasValue || UnidadTecnica.HasValue || Estado.HasValue)
            {
                if (DocumentoIngreso.HasValue)
                {
                    query = db.vw_BandejaEntrada.Where(d => d.BandejaId == idBandeja && d.Id == DocumentoIngreso.Value);
                }
                else
                {
                    DateTime fh = FechaHasta.Value.Date.AddDays(1);
                    query = db.vw_BandejaEntrada
                    .Where(d => d.FechaIngreso >= fechaDesde && d.FechaIngreso < fh && d.BandejaId == idBandeja);

                    if (UnidadTecnica.HasValue)
                        query = query.Where(d => d.UtAsignadaId == UnidadTecnica.Value);

                    if (Estado.HasValue)
                        query = query.Where(d => d.EstadoId == Estado.Value);
                }
            }
            else
            {
                query = db.vw_BandejaEntrada.Where(d => d.FechaIngreso >= fechaDesde && d.BandejaId == idBandeja);
            }
            
            /* filtros aplicados en las columnas de la grilla*/
            if (!string.IsNullOrWhiteSpace(filtroSql))
            {
                query = query.Where(filtroSql, filtroSqlParams);
            }
            /* Filtros especificos en duro para la pestaña Priorizados:
               - no puede mostrar requerimientos no cerrados (esto se maneja actualmente por la configuración de bandeja-estado-etapa en bd
               - no puede mostrar requerimientos q no tengan fecha de resolución estimada (requerimientos no priorizados)
               - no puede mostrar requerimientos en asignación temporal
               - no puede mostrar requerimientos sin profesional UT
             */
            query = query.Where(d => d.Resolucion != null && d.TipoIngreso != "TEMPORAL" && d.ProfesionalId != null);

            /* Grilla de Nueva Priorización de Encargado o Secretaria UT (se maneja como una Bandeja de "Priorización de Encargado UT" pero es una grilla dentro de la bandeja de encargo UT o de secretaria UT, está configurada en bd con las mismas reglas de esas bandejas.
            */
            //Se separan las query en vez de hacer una query q contemple todos los posibles filtros
            switch (codigoBandeja)
            {
                case (int)Bandeja.PriorizacionEncargado:
                /*case (int)Bandeja.EncargadoUt:*/
                    query = query.Where(d =>
                            esAdmin ||
                            //( d.ResponsableUtId == idUsuario ||
                            //   (d.ResponsableUtTempId == idUsuario && !(d.LiberarAsignacionTemp == true))
                            // ) // <-- Si se habilitan estas condiciones entonces se obtienen los ingresos de las uts q el usuario era responsable al momento de la asignación de UT, aunque no sea el actual responsable de la UT
                            utsUsuario.Any(idUt => d.UtAsignadaId == idUt ||
                                                   (d.UtTemporalId == idUt && !(d.LiberarAsignacionTemp == true)) ||
                                                   d.UtTransparenciaId == idUt
                            ) // <-- Con estas condiciones se devuelven los ingresos de las UTs q el usuario es actualmente el Responsable);
                    );
                    break;
                case (int)Bandeja.PriorizacionProfesional:
                    query = query.Where(d =>
                        (esAdmin || d.ProfesionalId == idUsuario ||
                         (d.ProfesionalTempId == idUsuario && !(d.LiberarAsignacionTemp == true))));
                    break;
                case (int)Bandeja.PriorizacionSecretaria:
                /*case (int)Bandeja.SecretariaUt:*/
                    query = query.Where(d =>
                        esAdmin || utsUsuario.Any(idUt => d.UtAsignadaId == idUt ||
                                                           (d.UtTemporalId == idUt && !(d.LiberarAsignacionTemp == true)) ||
                                                           d.UtTransparenciaId == idUt));
                    break;
                default:
                    // Nada
                    break;
            }
            query = query.Where(d => hayFiltroExtra == 0 || d.DocumentoIngreso.Contains(filterText) || d.RemitenteNombre.Contains(filterText) ||
                             d.RemitenteInstitucion.Contains(filterText) || d.Materia.Contains(filterText) || d.EstadoTitulo.Contains(filterText) ||
                             d.UtAsignadaTitulo.Contains(filterText) || d.ProfesionalNombre.Contains(filterText));

            var datosResult = new List<GroupResult>();

            if (tipoOper == "FILTROS")
            {
                var datosFull = query
                    .Select(d => new {d.Resolucion, d.ForzarPrioridadFecha, d.DiasResolucion})
                    .Distinct()
                    .AsEnumerable();
                datosResult = new List<GroupResult>()
                {
                    new GroupResult()
                    {
                        Key = "",
                        Member = "",
                        Items = datosFull.Select(d => new RequerimientoDto
                        {
                            Resolucion = d.Resolucion.GetValueOrDefault(),
                            ForzarPrioridadFecha = d.ForzarPrioridadFecha.GetValueOrDefault(),
                            DiasResolucion = d.DiasResolucion.GetValueOrDefault()
                        }).ToList()
                    }
                };
            }
            else if (tipoOper == "IDREQ")
            {
                var datosFull = query
                    .Select(d => d.Id)
                    .Distinct()
                    .AsEnumerable();
                datosResult = new List<GroupResult>()
                {
                    new GroupResult()
                    {
                        Key = "",
                        Member = "",
                        Items = datosFull.ToList()
                    }
                };
            }
            else if (tipoOper == "NOTIF")
            {
                var datosFull = query
                    .Select(d => d.Id)
                    .AsEnumerable();
                datosResult = new List<GroupResult>()
                {
                    new GroupResult()
                    {
                        Key = "",
                        Member = "",
                        Items = datosFull.ToList()
                    }
                };
            }
            else
            {
                var datos = query
                    .OrderBy( sort.Field == "Resolucion" ? $"{sort.Field} {sort.Dir}" : $"Resolucion ASC, {sort.Field} {sort.Dir}")
                    //.OrderBy(string.Format("{0} {1}", sort.Field, sort.Dir))
                    .Skip(skip)
                    .Take(take)
                    .GroupBy(g => DbFunctions.TruncateTime(g.Resolucion.Value))
                    .AsEnumerable();
                datosResult = datos
                    .Select(d => new GroupResult()
                    {
                        Key = d.Key,
                        Member = "Resolucion",
                        Items = d.Select(s => _mapper.MapFromModelToDto<vw_BandejaEntrada, RequerimientoDto>(s)),
                        Aggregates = new 
                        {
                            Resolucion = new
                            {
                                Count = d.Count()
                            }
                        }
                    })
                    .ToList();
            }

            var resultado = new DatosAjax<List<GroupResult>>(datosResult, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;
        }

        //public DatosAjax<List<RequerimientoDto>> GetByUnidadTecnica(int idUT, int? idProfesional)
        //{
        //    var reqs = db.Requerimiento
        //                .Include(i => i.Remitente)
        //                .Include(i => i.EstadoRequerimiento)
        //                .Where(x => x.UnidadTecnicaAsign.Id == idUT && x.EstadoId != 14 && (x.EstadoId == 11 || x.EstadoId == 12 || x.EstadoId == 13));

        //    if (idProfesional.HasValue && idProfesional.Value > 0)
        //        reqs = reqs.Where(x => x.ProfesionalId == idProfesional);

        //    var datos = reqs.Select(_mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>).ToList();
        //    var resultado = new DatosAjax<List<RequerimientoDto>>(datos, new ResultadoOperacion(1, "OK", null));
        //    return resultado;
        //}

        public DatosAjax<List<UsuarioDto>> GetProfesionales(int idUT, int? idProfesional)
        {
            var usus = db.Usuario.Join(db.Requerimiento, usu => usu.Id, req => req.ProfesionalId, (usu, req) => new { Usu = usu, Req = req })
                        .Where(x => x.Req.UnidadTecnicaAsign.Id == idUT && (idProfesional == null || x.Usu.Id == idProfesional))
                        .Select(s => new UsuarioDto
                        {
                            Id = s.Usu.Id,
                            NombresApellidos = s.Usu.NombresApellidos
                        })
                        .Distinct();

            var datos = usus.Select(_mapper.MapFromModelToDto<UsuarioDto, UsuarioDto>).ToList();
            var resultado = new DatosAjax<List<UsuarioDto>>(datos, new ResultadoOperacion(1, "OK", null));
            return resultado;
        }


        #region Vistas
        public DatosAjax<List<RequerimientoDto>> GetDatosVistaEtiqueta(int skip, int take,
            SortParam sort, string filterText)
        {
            var query = db.vw_EtiquetasReq;

            var datos = query
                .OrderBy($"{sort.Field} {sort.Dir}")
                .Skip(skip)
                .Take(take)
                .AsEnumerable()
                .Select(d => _mapper.MapFromModelToDto<vw_EtiquetasReq, RequerimientoDto>(d))
                .ToList();
            var resultado = new DatosAjax<List<RequerimientoDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;
        }

        public DatosAjax<List<RequerimientoDto>> GetDatosVistaRemitente(int skip, int take,
            SortParam sort, string filterText)
        {
            var query = db.vw_RemitentesReq;

            var datos = query
                .OrderBy($"{sort.Field} {sort.Dir}")
                .Skip(skip)
                .Take(take)
                .AsEnumerable()
                .Select(d => _mapper.MapFromModelToDto<vw_RemitentesReq, RequerimientoDto>(d))
                .ToList();
            var resultado = new DatosAjax<List<RequerimientoDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;
        }

        //public DatosAjax<List<RequerimientoDto>> GetDatosVistaGenero(int skip, int take,
        //    SortParam sort, string filterText)
        //{
        //    var query = db.vw_GeneroReq
        //        .OrderBy(" AnnoMes DESC, RemitenteGenero ASC");

        //    var consulta = "SELECT vw.*, vw.AnnoMes as AnnoMesInt, t1.TotalAnnoMes, t2.TotalAnnoMesGenero FROM vw_GeneroReq vw " +
        //                   "    JOIN(SELECT AnnoMes, COUNT(id) as TotalAnnoMes FROM vw_GeneroReq GROUP BY AnnoMes) as t1 " +
        //                   "        ON vw.AnnoMes = t1.AnnoMes " +
        //                   "    JOIN(SELECT AnnoMes, RemitenteGenero, COUNT(id) AS TotalAnnoMesGenero FROM vw_GeneroReq GROUP BY AnnoMes, RemitenteGenero) AS t2 " +
        //                   "        ON vw.AnnoMes = t2.AnnoMes AND vw.RemitenteGenero = t2.RemitenteGenero " +
        //                   "    ORDER BY vw.AnnoMes DESC, vw.RemitenteGenero " +
        //                   $"    OFFSET {skip} ROWS  " +
        //                   $"    FETCH NEXT {take} ROWS ONLY";
        //    var datos = db.Database.SqlQuery<RequerimientoDto>(consulta).ToList();

        //    var resultado = new DatosAjax<List<RequerimientoDto>>(datos, new ResultadoOperacion(1, "OK", null))
        //    {
        //        Total = query.Count()
        //    };

        //    return resultado;
        //}


        public DatosAjax<List<GroupResult>> GetDatosVistaGenero(int skip, int take,
            SortParam sort, string filterText, string filtroSql, object[] filtroSqlParams)
        {
            filtroSql = string.IsNullOrEmpty(filtroSql)
                ? "1=1"
                : filtroSql;
            var filtroSqlModif = filtroSql.Replace("EstadoTitulo", "EstadoRequerimiento.Titulo");
            // Datos de grupo nivel 1 (Año-Mes)
            var queryNivel1 = db.Requerimiento.Include(r => r.EstadoRequerimiento)
                .Where(r => !r.Eliminado)
                .Where(filtroSqlModif, filtroSqlParams)
                .GroupBy(p => new {p.FechaIngreso.Year, p.FechaIngreso.Month});
            var datos = queryNivel1
                .OrderByDescending(d => d.Key.Year).ThenByDescending(d => d.Key.Month)
                .Skip(skip)
                .Take(take)
                .Select(d => new VistaGeneroGrupoDto
                {
                    Anno = d.Key.Year,
                    Mes = d.Key.Month,
                    Total = d.Count()
                })
                .ToList();

            var result = new List<GroupResult>();
            foreach (var annomes in datos)
            {
                // Datos de grupo nivel 2 (Género)
                var queryNivel2 = db.Requerimiento.Include(r => r.EstadoRequerimiento).Include(r => r.Remitente)
                    .Where(r => !r.Eliminado)
                    .Where(filtroSqlModif, filtroSqlParams)
                    .GroupBy(p => new { p.FechaIngreso.Year, p.FechaIngreso.Month, p.Remitente.Genero })
                    .Where(d => d.Key.Year == annomes.Anno && d.Key.Month == annomes.Mes);
                var datosGen = queryNivel2
                    .OrderBy(d => d.Key.Genero)
                    .Select(d => new VistaGeneroDto
                    {
                        RemitenteGenero = d.Key.Genero,
                        Total = d.Count()
                    })
                    .AsEnumerable();
                // Todos los  datos de requerimientos para luego filtrarlos por año/mes/genero
                var datosReqAll = db.vw_GeneroReq
                    .Where(filtroSql, filtroSqlParams)
                    .Where(d => d.Anno == annomes.Anno && d.Mes == annomes.Mes)
                    .AsEnumerable()
                    .Select(d => _mapper.MapFromModelToDto<vw_GeneroReq, VistaGeneroDto>(d))
                    .ToList();

                var datosGenero = new List<GroupResult>();
                foreach (var genero in datosGen)
                {
                    var generoRemit = genero.RemitenteGenero ?? "";
                    // Datos de detalle de requerimiento
                    var datosReqDto = datosReqAll
                        .Where(d => d.FechaIngreso.Year == annomes.Anno && d.FechaIngreso.Month == annomes.Mes &&
                                    (d.RemitenteGenero == generoRemit))
                        .OrderByDescending(d => d.FechaIngreso);

                    var datoGenero = new GroupResult()
                    {
                        Key = genero.RemitenteGenero,
                        Member = "RemitenteGenero",
                        HasSubgroups = false,
                        ItemCount = datosReqDto.Count(),
                        Items = datosReqDto.Cast<object>().ToList(),
                        AggregateFunctionsProjection = new
                        {
                            Count_AnnoMes = genero.Total.GetValueOrDefault(0),
                            Count_RemitenteGenero = genero.Total.GetValueOrDefault(0)
                        },
                        Aggregates = new
                        {
                            AnnoMes = new { Count = genero.Total.GetValueOrDefault(0) },
                            RemitenteGenero = new { Count = genero.Total.GetValueOrDefault(0) }
                        }
                    };
                    datosGenero.Add(datoGenero);
                }

                var annomesdto = new GroupResult()
                {
                    Key = annomes.AnnoMes, // $"{annomes.Anno}-{annomes.Mes}",
                    Member = "AnnoMes",
                    HasSubgroups = true,
                    ItemCount = datosGenero.Count(),
                    Items = datosGenero.Cast<object>().ToList(),
                    AggregateFunctionsProjection = new
                    {
                        Count_AnnoMes = annomes.Total,
                        Count_RemitenteGenero = annomes.Total
                    },
                    Aggregates = new
                    {
                        AnnoMes = new { Count = annomes.Total },
                        RemitenteGenero = new { Count = annomes.Total }
                    }
                };
                result.Add(annomesdto);
            }

            var resultado = new DatosAjax<List<GroupResult>>(result, new ResultadoOperacion(1, "OK", null))
            {
                Total = queryNivel1.Count()
            };
            return resultado;
        }

        public List<VistaGeneroDto> GetDatosVistaGeneroSinGrupos(string filterText, string filtroSql, object[] filtroSqlParams)
        {
            filtroSql = string.IsNullOrEmpty(filtroSql) || filtroSql == "~"
                ? "1=1"
                : filtroSql;
            var data = db.vw_GeneroReq
                .Where(filtroSql, filtroSqlParams)
                .AsEnumerable()
                .Select(d =>
                {
                    return new VistaGeneroDto()
                    {
                        FechaIngreso = d.FechaIngreso,
                        DocumentoIngreso = d.DocumentoIngreso,
                        NumeroIngreso = d.NumeroIngreso,
                        CanalLlegadaTramiteTitulo = d.CanalLlegadaTramiteTitulo,
                        CategoriaMonumentoNacTitulo = d.CategoriaMonumentoNacTitulo,
                        CerradoPor = d.CerradoPor,
                        AnnoMes = d.AnnoMes,
                        ComentarioCierre = d.ComentarioCierre,
                        MonumentoNacionalComunaTitulo = d.MonumentoNacionalComunaTitulo,
                        MonumentoNacionalDenominacionOficial = d.MonumentoNacionalDenominacionOficial,
                        EstadoTitulo = d.EstadoTitulo,
                        EtiquetaTitulos = d.EtiquetaTitulos,
                        Cierre = d.Cierre,
                        Materia = d.Materia,
                        MotivoCierreTitulo = d.MotivoCierreTitulo,
                        MonumentoNacionalNombreUsoActual = d.MonumentoNacionalNombreUsoActual,
                        CasoId = d.CasoId,
                        MonumentoNacionalOtrasDenominaciones = d.MonumentoNacionalOtrasDenominaciones,
                        ProfesionalNombre = d.ProfesionalNombre,
                        MonumentoNacionalReferenciaLocalidad = d.MonumentoNacionalReferenciaLocalidad,
                        MonumentoNacionalRegionTitulo = d.MonumentoNacionalRegionTitulo,
                        RemitenteNombre = d.RemitenteNombre,
                        RemitenteInstitucion = d.RemitenteInstitucion,
                        RemitenteGenero = d.RemitenteGenero,
                        TipoIngreso = d.TipoIngreso,
                        TipoTramiteTitulo = d.TipoTramiteTitulo,
                        UtAsignadaTitulo = d.UtAsignadaTitulo
                    };
                })
                .OrderByDescending(o => o.FechaIngreso.Year).ThenBy(o => o.FechaIngreso.Month).ThenBy(o => o.RemitenteGenero).ThenByDescending(o => o.FechaIngreso)
                .ToList();
            return data;
        }

        #endregion


        #region Archivar y restaurar requerimientos
        public DatosAjax<List<RequerimientoDto>> GetDatosBusquedaArchivar(int skip, int take, SortParam sort, string filterText,
            DateTime fechaDesde, DateTime fechaHasta, int unidadTecnicaId, int tipoBusqueda)
        {

            var query = db.Requerimiento
                .Include(r => r.UnidadTecnicaAsign)
                .Include(r => r.Bitacora)
                .Include(r => r.EstadoRequerimiento)
                .AsNoTracking()
                .Where(a => a.Eliminado == false
                                                        && a.FechaIngreso >= fechaDesde
                                                        && a.FechaIngreso <= fechaHasta);

            if (unidadTecnicaId > 0)
                query = query.Where(a => a.UtAsignadaId == unidadTecnicaId);

            if (tipoBusqueda == 1) // Archivar
                query = query.Where(a => a.EstadoId == (int)EstadoIngreso.Cerrado);
            else if (tipoBusqueda == 2) // Restaurar
                query = query.Where(a => a.EstadoId == (int)EstadoIngreso.Archivado);

            var datos = query
                .OrderBy($"{sort.Field} {sort.Dir}")
                .Skip(skip)
                .Take(take)
                .AsEnumerable()
                .Select(a => new RequerimientoDto()
                {
                    Id = a.Id,
                    DocumentoIngreso = a.DocumentoIngreso,
                    FechaIngreso = a.FechaIngreso,
                    UtAsignadaTitulo = a.UnidadTecnicaAsign.Titulo,
                    RequiereAcuerdo = a.RequiereAcuerdo,
                    RequiereRespuesta = a.RequiereRespuesta,
                    //FechaUltAcuerdoComision = a.FechaUltAcuerdoComision,
                    //FechaUltAcuerdoSesion = a.FechaUltAcuerdoSesion,
                    FechaUltAcuerdoComision = a.Bitacora.Where(b => b.TipoBitacoraCod == TipoBitacora.AcuerdoComision.ToString("D")).OrderByDescending(c => c.Fecha).FirstOrDefault()?.Fecha,
                    FechaUltAcuerdoSesion = a.Bitacora.Where(b => b.TipoBitacoraCod == TipoBitacora.AcuerdoSesion.ToString("D")).OrderByDescending(c => c.Fecha).FirstOrDefault()?.Fecha,
                    EstadoTitulo = a.EstadoRequerimiento.Titulo,
                })
                .ToList();

            var resultado = new DatosAjax<List<RequerimientoDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;

        }
        #endregion

        #region Log de requermiento

        public List<LogSistemaDto> GetLogRequerimiento(int idReq)
        {
            var datos = db.LogSistema
                .Where(l => l.RequerimientoId == idReq)
                .OrderByDescending(l => l.Fecha)
                .Select(_mapper.MapFromModelToDto<LogSistema, LogSistemaDto>)
                .ToList();
            return datos;
        }

        public List<LogBitacoraDto> GetLogBitacoraRequerimiento(int idReq)
        {
            try
            {
                var datos = from ls in db.LogSistema
                    from lst in db.LogSistemaTexto.Where(l =>
                        (l.Flujo == "*" || l.Flujo == ls.Flujo) && ls.Accion == l.Accion).DefaultIfEmpty()
                    from est in db.EstadoRequerimiento.Where(e => ls.EstadoId == e.Id).DefaultIfEmpty()
                    from eta in db.EtapaRequerimiento.Where(e => ls.EtapaId == e.Id).DefaultIfEmpty()
                    from ut in db.UnidadTecnica.Where(u => ls.UnidadTecnicaId == u.Id).DefaultIfEmpty()
                    from usr in db.Usuario.Where(u => ls.UsuarioId == u.Id).DefaultIfEmpty()
                    where ls.RequerimientoId == idReq && (lst.TipoLog == "B" || lst.TipoLog == "C")
                    select new LogBitacoraDto()
                    {
                        Id = ls.Id,
                        FechaLog = ls.Fecha,
                        FechaFormulario = ls.OrigenFecha,
                        EstadoId = ls.EstadoId,
                        EstadoTitulo = est.Titulo,
                        EtapaId = ls.EtapaId,
                        EtapaTitulo = eta.Titulo,
                        UnidadTecnicaId = ls.UnidadTecnicaId,
                        UnidadTecnicaTitulo = ut.Titulo,
                        UsuarioId = ls.UsuarioId,
                        UsuarioNombre = usr.NombresApellidos ?? ls.Usuario,
                        Actividad = lst.Actividad.Replace("%origen%", ls.Origen ?? ""),
                        TextoDefecto = lst.TextoBitacora.Replace("%extradata%", ls.ExtraData ?? ""),
                        OrigenId = ls.OrigenId,
                        Origen = ls.Origen,
                        Accion = ls.Accion
                    };
                return datos.OrderBy(d => d.FechaLog).ToList();
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                return new List<LogBitacoraDto>();
            }
        }

        private ControlCambiosEntidad ControlCambios(Requerimiento model)
        {
            // TODO: poner de manera parametrizada los campos a chequear si se modificaron, ahora están en duro en GeneralData.CamposControlCambio

            // var camposModificados = new Dictionary<string, object>();
            var cambios = new ControlCambiosEntidad 
            {
                EntityName = "Requerimiento",
                CamposModificados = new Dictionary<string, object>()
            };

            // return cambios; // TODO: <--- eliminar. Se desactiva este método pues no está terminado aún el log de bitácora y se necesita pasar a QA el resto de las implementaciones y correcciones

            var modifiedEntities = db.ChangeTracker.Entries()
                .Where(p => p.State == EntityState.Modified).ToList();
            foreach (var change in modifiedEntities)
            {
                // var entityName = change.Entity.GetType().Name;
                foreach (var prop in change.OriginalValues.PropertyNames)
                {
                    var originalValue = change.OriginalValues[prop];
                    var currentValue = change.CurrentValues[prop];
                    if (!object.Equals(originalValue, currentValue) && GeneralData.CamposControlCambio.Keys.Contains(prop))
                    {
                        if (currentValue is bool) currentValue = ((bool)currentValue ? "Sí" : "No");
                        if (currentValue is DateTime) currentValue = ((DateTime)currentValue).ToString(GeneralData.FORMATO_FECHA_CORTO);
                        cambios.CamposModificados[prop] = (currentValue ?? "").ToString();
                    }
                }
            }

            // Update en relaciones
            ((IObjectContextAdapter)db).ObjectContext.DetectChanges();
            var addedRelations = ((IObjectContextAdapter)db).ObjectContext
                .ObjectStateManager.GetObjectStateEntries(EntityState.Added)
                .Where(e => e.IsRelationship)
                .Select(r => string.Format("[{0}][{1}]",
                    ((System.Data.Entity.Core.EntityKey)r.CurrentValues[0]).EntitySetName,
                    ((System.Data.Entity.Core.EntityKey)r.CurrentValues[1]).EntitySetName) )
                .ToList();
            var deletedRelations = ((IObjectContextAdapter)db).ObjectContext
                .ObjectStateManager.GetObjectStateEntries(EntityState.Deleted)
                .Where(e => e.IsRelationship)
                .Select(r => string.Format("[{0}][{1}]",
                    ((System.Data.Entity.Core.EntityKey)r.OriginalValues[0]).EntitySetName,
                    ((System.Data.Entity.Core.EntityKey)r.OriginalValues[1]).EntitySetName) )
                .ToList();
            // Para valores de Categoría Monumento
            if (GeneralData.CamposControlCambio.Keys.Contains("CategoriaMonumentoNac") &&
                (addedRelations.Any(r => r.Contains("CategoriaMonumentoNac")) || deletedRelations.Any(r => r.Contains("CategoriaMonumentoNac"))) ) {
                var titulos = string.Join("; ", model.MonumentoNacional.CategoriaMonumentoNac.Select(c => c.Titulo).ToList());
                cambios.CamposModificados["CategoriaMonumentoNac"] = titulos;
            }
            // Para valores de Región
            if (GeneralData.CamposControlCambio.Keys.Contains("Region") &&
                (addedRelations.Any(r => r.Contains("Region")) || deletedRelations.Any(r => r.Contains("Region"))) ) {
                var titulos = string.Join("; ", model.MonumentoNacional.Region.Select(c => c.Titulo).ToList());
                cambios.CamposModificados["Region"] = titulos;
            }

            // Para valor de UT Apoyo si se modificó
            if (cambios.CamposModificados.Keys.Contains("UtApoyoId"))
            {
                int.TryParse((cambios.CamposModificados["UtApoyoId"] ?? "").ToString(), out var id);
                var ut = db.UnidadTecnica.FirstOrDefault(u => u.Id == id);
                cambios.CamposModificados["UtApoyoId"] = ut?.Titulo;
            }
            // Para valor de UT Conocimiento si se modificó
            if (cambios.CamposModificados.Keys.Contains("UtConocimientoId"))
            {
                int.TryParse((cambios.CamposModificados["UtConocimientoId"] ?? "").ToString(), out var id);
                var ut = db.UnidadTecnica.FirstOrDefault(u => u.Id == id);
                cambios.CamposModificados["UtConocimientoId"] = ut?.Titulo;
            }
            // Para valor de Nombre de Caso si se modificó
            if (cambios.CamposModificados.Keys.Contains("CasoId"))
            {
                int.TryParse((cambios.CamposModificados["CasoId"] ?? "").ToString(), out var id);
                var ut = db.Caso.FirstOrDefault(u => u.Id == id);
                cambios.CamposModificados["CasoId"] = ut?.Titulo;
            }
            // Para valor de Tipo Trámite si se modificó
            if (cambios.CamposModificados.Keys.Contains("TipoTramiteId"))
            {
                int.TryParse((cambios.CamposModificados["TipoTramiteId"] ?? "").ToString(), out var id);
                var ut = db.TipoTramite.FirstOrDefault(u => u.Id == id);
                cambios.CamposModificados["TipoTramiteId"] = ut?.Titulo;
            }

            return cambios;
        }
        #endregion

        #region Actualizar datos de MN en requermientos

        public ResultadoOperacion ActualizarMnReq(List<int> reqIds, List<int> casoIds, MonumentoNacionalDto datosMn)
        {
            reqIds = reqIds ?? new List<int>();
            casoIds = casoIds ?? new List<int>();
            var requerimientos = db.Requerimiento
                .Include(r => r.MonumentoNacional)
                .Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                .Include(r => r.MonumentoNacional.Region)
                .Include(r => r.MonumentoNacional.Provincia)
                .Include(r => r.MonumentoNacional.Comuna)
                .Where(r => reqIds.Any(i => i == r.Id) || casoIds.Any(i => i == r.CasoId));

            // Dato Categorías MN
            var categMn = db.ListaValor
                .Where(c => c.Codigo == datosMn.CategoriaMonumentoNacCod && c.IdLista == (int)Mantenedor.CategoriaMn)
                .ToList();
            // Dato Regiones
            var region = db.ListaValor.Where(x => datosMn.RegionCod.Any(d => d == x.Codigo)).ToList();
            // Dato Provincias
            var provincia = db.ListaValor.Where(x => datosMn.ProvinciaCod.Any(d => d == x.Codigo)).ToList();
            // Dato Comunas
            var comuna = db.ListaValor.Where(x => datosMn.ComunaCod.Any(d => d == x.Codigo)).ToList();
            foreach (var ingreso in requerimientos)
            {
                if (ingreso.MonumentoNacional == null)
                { // Monumento nacional no debe ser null, es obligatorio en los formularios de requermientos, pero por si acaso
                    ingreso.MonumentoNacional = new MonumentoNacional();
                }
                ingreso.MonumentoNacional.CodigoMonumentoNac = datosMn.CodigoMonumentoNac;
                ingreso.MonumentoNacional.DenominacionOficial = datosMn.DenominacionOficial;
                ingreso.MonumentoNacional.OtrasDenominaciones = datosMn.OtrasDenominaciones;
                ingreso.MonumentoNacional.NombreUsoActual = datosMn.NombreUsoActual;
                ingreso.MonumentoNacional.DireccionMonumentoNac = datosMn.DireccionMonumentoNac;
                ingreso.MonumentoNacional.ReferenciaLocalidad = datosMn.ReferenciaLocalidad;
                ingreso.MonumentoNacional.RolSii = datosMn.RolSii;

                // Update Categorías MN
                ingreso.MonumentoNacional.CategoriaMonumentoNac = categMn;
                // Update Regiones
                ingreso.MonumentoNacional.Region = region;
                // Update Provincias
                ingreso.MonumentoNacional.Provincia = provincia;
                // Update Comunas
                ingreso.MonumentoNacional.Comuna = comuna;
            }

            db.SaveChanges();

            return new ResultadoOperacion(1, "Datos actualizados con éxito.", null);
        }
        #endregion

        #region Requerimientos asociados a un despacho

        //public List<RequerimientoDto> GetRequerimientosDespacho(int idDespacho)
        //{
        //    var reqs = db.Requerimiento
        //        .Include(r => r.ProfesionalUt)
        //        //.Include(r => r.UnidadTecnicaAsign)
        //        //.Include(r => r.UnidadTecnicaAnterior)
        //        //.Include(r => r.UnidadTecnicaApoyo)
        //        //.Include(r => r.UnidadTecnicaTemp)
        //        //.Include(r => r.UnidadTecnicaTransp)
        //        //.Include(r => r.EstadoRequerimiento)
        //        //.Include(r => r.EtapaRequerimiento)
        //        .Include(r => r.Despacho)
        //        .Where(r => r.Despacho.Any(d => d.Id == idDespacho));
        //    var datos = reqs.AsEnumerable().Select(_mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>).ToList();

        //    return datos;
        //}
        #endregion

    }
}
