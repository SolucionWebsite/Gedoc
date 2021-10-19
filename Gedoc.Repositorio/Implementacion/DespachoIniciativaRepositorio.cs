using System;
using System.Collections.Generic;
using System.Linq;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Maps.Interfaces;
using Gedoc.Repositorio.Model;
using System.Linq.Dynamic;
using System.Data.Entity;
using Gedoc.Helpers.Enum;

namespace Gedoc.Repositorio.Implementacion
{
    public class DespachoIniciativaRepositorio : RepositorioBase, IDespachoIniciativaRepositorio
    {
        private readonly IGenericMap _mapper;

        public DespachoIniciativaRepositorio(IGenericMap mapper)
        {
            this._mapper = mapper;
        }

        public DatosAjax<List<DespachoIniciativaDto>> GetDatosBandejaEntrada(int idBandeja, int skip, int take, SortParam sort, string filterText,
            DateTime fechaDesde)
        {
            var hayFiltroExtra = !string.IsNullOrEmpty(filterText) ? 1 : 0;

            var query = db.vw_BandejaEntradaDespachoInic
                .Where(d => hayFiltroExtra == 0 || d.NumeroDespacho.Contains(filterText) || d.DestinatarioNombre.Contains(filterText) ||
                             d.EstadoTitulo.Contains(filterText) || d.Materia.Contains(filterText));
            //Filtro de eliminado quedo en la vista.

            var datos = query
                .OrderBy($"{sort.Field} {sort.Dir}")
                .Skip(skip)
                .Take(take)
                .AsEnumerable()
                .Select(d => _mapper.MapFromModelToDto<vw_BandejaEntradaDespachoInic, DespachoIniciativaDto>(d))
                .ToList();

            var resultado = new DatosAjax<List<DespachoIniciativaDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;
        }

        public DespachoIniciativaDto GetById(int id)
        {
            var datos = db.DespachoIniciativa
                .Include(d => d.EstadoDespacho)
                .Include(d => d.Remitente)
                .Include(d => d.DestinatarioCopia)
                .Include(d => d.TipoAdjunto)
                .Include(d => d.Soporte)
                .Include(d => d.Etiqueta)
                .Include(d => d.MonumentoNacional)
                .Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                .Include(r => r.MonumentoNacional.Region)
                .Include(r => r.MonumentoNacional.Provincia)
                .Include(r => r.MonumentoNacional.Comuna)
                .FirstOrDefault(r => r.Id == id);
            return _mapper.MapFromModelToDto<DespachoIniciativa, DespachoIniciativaDto>(datos);
        }

        public ResultadoOperacion GetDespachoDetalle(DespachoIniciativaDto despachoDto)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var despacho = db.DespachoIniciativa
                .Include(d => d.EstadoDespacho)
                .Include(d => d.Remitente)
                .Include(d => d.DestinatarioCopia)
                .Include(d => d.TipoAdjunto)
                .Include(d => d.Soporte)
                .Include(d => d.Etiqueta)
                .Include(d => d.ProfesionalUt)
                .Include(d => d.UnidadTecnica)
                .FirstOrDefault(r => r.Id == despachoDto.Id);
            if (despacho == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el despacho especificado";
            }
            else
            {
                // Se actualiza el despacho especifcado de parametro con todos los datos obtenidos desde Bd
                _mapper.MapFromOrigenToDestino<DespachoIniciativa, DespachoIniciativaDto>(despacho, despachoDto);
            }

            return resultado;
        }

        public ResultadoOperacion NewDespachoInic(DespachoIniciativaDto datos, ProcesaArchivo procesaArchivo)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var despacho = _mapper.MapFromDtoToModel<DespachoIniciativaDto, DespachoIniciativa>(datos);
            despacho.CanalLlegadaTramiteCod = datos.CanalLlegadaTramiteCod ?? CanalLlegada.Presencial.ToString("D");

            DbContextTransaction transaction = datos.DesdeOficio ? null : db.Database.BeginTransaction(); //db.Database.CurrentTransaction ?? db.Database.BeginTransaction();

            // using (DbContextTransaction transaction = db.Database.BeginTransaction())
            try
            {
                // Si no tiene origen en un Oficio entonces se genera el valor de Número de Oficio en base al correlativo
                if (!datos.DesdeOficio && string.IsNullOrWhiteSpace(datos.NumeroDespacho))
                {
                    // if (string.IsNullOrWhiteSpace(datos.NumeroDespacho)) // No se especificó en el formulario el número de oficio, se genera automáticamente
                    {
                        var anno = despacho.FechaEmisionOficio.GetValueOrDefault(DateTime.Now).Year;
                        var corr = db.Correlativo.FirstOrDefault(c => c.Anno == anno);
                        if (corr == null)
                        {
                            throw new Exception("No se encontró en base de datos el correlativo para el año " + anno);
                        }

                        var corrDespacho = corr.CorrelativoDespacho++;
                        var corrDespachoStr = corrDespacho.ToString().PadLeft(5, '0');

                        var folio = $"{corrDespachoStr}-{anno}";
                        despacho.FolioDespacho = folio;
                        despacho.NumeroDespacho = folio; /**** Se genera de manera automática el Número de Despacho *******/
                    }
                }
                else
                {
                    despacho.FolioDespacho = datos.FolioDespacho ?? datos.NumeroDespacho;
                    despacho.NumeroDespacho = datos.NumeroDespacho; /**** Se genera de manera automática el Número de Despacho *******/
                }

                EvitaInsertHijo(despacho);

                db.DespachoIniciativa.Add(despacho);
                db.SaveChanges();
                datos.Id = despacho.Id;
                datos.FolioDespacho = despacho.FolioDespacho ?? datos.FolioDespacho;
                datos.NumeroDespacho = despacho.NumeroDespacho ?? datos.NumeroDespacho;

                // Se sube al repositorio de archivos el archivo adjunto al Despacho
                datos.DatosArchivo.OrigenId = despacho.Id;
                datos.DatosArchivo.OrigenCodigo = despacho.NumeroDespacho;
                var resultArch = procesaArchivo(datos.DatosArchivo, true, false);

                if (resultArch.Codigo <= 0)
                {
                    Gedoc.Helpers.Logging.Logger.LogError(
                        $"Error creando nuevo Despacho Iniciativas CMN, falló la carga del archivo adjunto. Id interno generado: {despacho.Id}");
                    resultado.Codigo = -1; // -2 para ignorarlo en el formulario
                    resultado.Mensaje =
                        "¡Atención! No se ha podido crear el Despacho Iniciativas CMN, ha ocurrió un error al subir el archivo adjunto.";
                    transaction?.Rollback();  // Cuando el Despacho se origina de un Oficio el Rollback se hace en el método de firmar el oficio
                }
                else
                {
                    var archUrl = (string[])resultArch.Extra ?? new[] { "", "" };
                    despacho.UrlArchivo = archUrl[0];
                    despacho.NombreArchivo = archUrl[1];
                    resultado.Mensaje = $"Número de Oficio: {despacho.NumeroDespacho}.";
                    db.SaveChanges();
                    transaction?.Commit();  // Cuando el Despacho se origina de un Oficio el Commit se hace en el método de firmar el oficio
                }
            }
            finally
            {
                transaction?.Dispose();
            }

            return resultado;

        }
        
        public ResultadoOperacion UpdateDatosArchivo(int id, string nombreArchivo, string urlArchivo)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var despacho = db.DespachoIniciativa.FirstOrDefault(d => d.Id == id);

            if (despacho != null)
            {
                despacho.UrlArchivo = urlArchivo;
                despacho.NombreArchivo = nombreArchivo;

                db.SaveChanges();
            }

            return resultado;
        }

        public ResultadoOperacion Update(DespachoIniciativaDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var despacho = db.DespachoIniciativa
                .Include(r => r.Etiqueta)
                .Include(r => r.Soporte)
                .Include(r => r.TipoAdjunto)
                .Include(r => r.DestinatarioCopia)
                .Include(r => r.MonumentoNacional)
                .Include(r => r.MonumentoNacional.CategoriaMonumentoNac)
                .Include(r => r.MonumentoNacional.Region)
                .Include(r => r.MonumentoNacional.Provincia)
                .Include(r => r.MonumentoNacional.Comuna)
                .FirstOrDefault(d => d.Id == datos.Id);

            if (despacho == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el despacho a actualizar.";
                return resultado;
            }

            #region Panel Documento
            despacho.NumeroDespacho = datos.NumeroDespacho ?? despacho.NumeroDespacho;
            despacho.FechaEmisionOficio = datos.FechaEmisionOficio;
            #endregion
            #region Panel Destinatario
            despacho.DestinatarioId = datos.DestinatarioId;
            #endregion
            #region Panel Despacho
            despacho.AntecedenteAcuerdo = datos.AntecedenteAcuerdo;
            despacho.Materia = datos.Materia;
            #endregion
            #region Proyecto
            despacho.NombreProyectoPrograma = datos.NombreProyectoPrograma;
            despacho.CasoId = datos.CasoId;
            #endregion
            #region Panel Monumento Nacional
            if (datos.MonumentoNacional != null)
            {
                despacho.MonumentoNacional.DenominacionOficial = datos.MonumentoNacional.DenominacionOficial;
                despacho.MonumentoNacional.OtrasDenominaciones = datos.MonumentoNacional.OtrasDenominaciones;
                despacho.MonumentoNacional.NombreUsoActual = datos.MonumentoNacional.NombreUsoActual;
                despacho.MonumentoNacional.DireccionMonumentoNac = datos.MonumentoNacional.DireccionMonumentoNac;
                despacho.MonumentoNacional.ReferenciaLocalidad = datos.MonumentoNacional.ReferenciaLocalidad;
                despacho.MonumentoNacional.RolSii = datos.MonumentoNacional.RolSii;

                // Update Categorías MN
                var cods2 = datos.MonumentoNacional.CategoriaMonumentoNac.Select(d => d.Id).ToList();
                despacho.MonumentoNacional.CategoriaMonumentoNac = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo && x.IdLista == (int)Mantenedor.CategoriaMn))
                    .ToList();
                // Update Regiones
                cods2 = datos.MonumentoNacional.Region.Select(d => d.Id).ToList();
                despacho.MonumentoNacional.Region = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo && x.IdLista == (int)Mantenedor.Region))
                    .ToList();
                // Update Provincias
                cods2 = datos.MonumentoNacional.Provincia.Select(d => d.Id).ToList();
                despacho.MonumentoNacional.Provincia = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo && x.IdLista == (int)Mantenedor.Provincia))
                    .ToList();
                // Update Comunas
                cods2 = datos.MonumentoNacional.Comuna.Select(d => d.Id).ToList();
                despacho.MonumentoNacional.Comuna = db.ListaValor
                    .Where(x => cods2.Any(d => d == x.Codigo && x.IdLista == (int)Mantenedor.Comuna))
                    .ToList();
            }
            #endregion
            #region Panel Adjuntos
            despacho.AdjuntaDocumentacion = datos.AdjuntaDocumentacion;
            despacho.CantidadAdjuntos = datos.CantidadAdjuntos;
            despacho.ObservacionesAdjuntos = datos.ObservacionesAdjuntos;
            #endregion
            #region Panel General
            despacho.ProyectoActividad = datos.ProyectoActividad;
            despacho.UtAsignadaId = datos.UtAsignadaId;
            despacho.ProfesionalId = datos.ProfesionalId;
            #endregion

            #region Relaciones de muchos-muchos
            // Update Tipos de Adjunto
            var cods = datos.TipoAdjunto.Select(d => d.Id).ToList();
            despacho.TipoAdjunto = db.ListaValor
                .Where(x => cods.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.TipoDocumento)
                .ToList();
            // Update Soportes
            cods = datos.Soporte.Select(d => d.Id).ToList();
            despacho.Soporte = db.ListaValor
                .Where(x => cods.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Soporte)
                .ToList();
            // Update Etiquetas
            cods = datos.Etiqueta.Select(d => d.Id).ToList();
            despacho.Etiqueta = db.ListaValor
                .Where(x => cods.Any(d => d == x.Codigo) && x.IdLista == (int)Mantenedor.Etiqueta)
                .ToList();
            // Update Destinatario en Copia
            var ids2 = datos.DestinatarioCopia.Select(d => d.IdInt).ToList();
            despacho.DestinatarioCopia = db.Remitente.Where(x => ids2.Any(d => d == x.Id)).ToList();

            EvitaInsertHijo(despacho);

            #endregion

            db.SaveChanges();

            return resultado;
        }

        public ResultadoOperacion MarcaEliminado(int despachoId, int usuarioId)
        {
            var resultado = new ResultadoOperacion(1, "Datos eliminados con éxito", null);

            var despacho = db.DespachoIniciativa.FirstOrDefault(a => a.Id == despachoId);
            if (despacho == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el despacho a eliminar.";
                return resultado;
            }
            despacho.EliminacionFecha = DateTime.Now;
            despacho.Eliminado = true;
            despacho.UsuarioEliminacionId = usuarioId;
            db.SaveChanges();

            return resultado;
        }

        public ResultadoOperacion EliminarDespInic(int despachoId)
        {
            var resultado = new ResultadoOperacion(1, "Datos eliminados con éxito", null);

            var despacho = db.DespachoIniciativa.FirstOrDefault(a => a.Id == despachoId);
            if (despacho == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el despacho a eliminar.";
                return resultado;
            }

            db.DespachoIniciativa.Remove(despacho);
            db.SaveChanges();

            return resultado;
        }

        public ResultadoOperacion UpdateCierre(DespachoIniciativaDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var despacho = db.DespachoIniciativa
                .FirstOrDefault(d => d.Id == datos.Id);

            if (despacho == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el despacho a actualizar.";
                return resultado;
            }

            despacho.EstadoId = datos.EstadoId;
            despacho.ProveedorDespachoCod = datos.ProveedorDespachoCod;
            despacho.NumeroGuia = datos.NumeroGuia;
            despacho.FechaRecepcion = datos.FechaRecepcion;
            despacho.MedioVerificacionCod = datos.MedioVerificacionCod;
            despacho.ObservacionesMedioVerif = datos.ObservacionesMedioVerif;
            despacho.ObservacionesDespacho = datos.ObservacionesDespacho;

            // EvitaInsertHijo(despacho);

            db.SaveChanges();

            return resultado;
        }

        private void EvitaInsertHijo(DespachoIniciativa despacho)
        {
            // Relaciones muchos a muchos. Se evita q se intente insertar en tablas hijas.
            foreach (var det in despacho.DestinatarioCopia ?? Enumerable.Empty<Remitente>())
            {
                db.Entry(det).State = EntityState.Unchanged;
            }
            foreach (var det in despacho.TipoAdjunto ?? Enumerable.Empty<ListaValor>())
            {
                db.Entry(det).State = EntityState.Unchanged;
            }
            foreach (var det in despacho.Soporte ?? Enumerable.Empty<ListaValor>())
            {
                db.Entry(det).State = EntityState.Unchanged;
            }
            foreach (var det in despacho.Etiqueta ?? Enumerable.Empty<ListaValor>())
            {
                db.Entry(det).State = EntityState.Unchanged;
            }

            if (despacho.MonumentoNacional != null)
            {
                foreach (var det in despacho.MonumentoNacional.CategoriaMonumentoNac ?? Enumerable.Empty<ListaValor>())
                {
                    db.Entry(det).State = EntityState.Unchanged;
                }
                foreach (var det in despacho.MonumentoNacional.Region ?? Enumerable.Empty<ListaValor>())
                {
                    db.Entry(det).State = EntityState.Unchanged;
                }
                foreach (var det in despacho.MonumentoNacional.Provincia ?? Enumerable.Empty<ListaValor>())
                {
                    db.Entry(det).State = EntityState.Unchanged;
                }
                foreach (var det in despacho.MonumentoNacional.Comuna ?? Enumerable.Empty<ListaValor>())
                {
                    db.Entry(det).State = EntityState.Unchanged;
                }
            }

        }

    }
}
