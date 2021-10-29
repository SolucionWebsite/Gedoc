using System.Collections.Generic;
using System.Linq;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Maps.Interfaces;
using System.Linq.Dynamic;
using Gedoc.Repositorio.Model;
using Gedoc.Helpers;
using System.Data.Entity;
using System;

namespace Gedoc.Repositorio.Implementacion
{
    public class AdjuntoRepositorio : RepositorioBase, IAdjuntoRepositorio
    {
        private readonly IGenericMap _mapper;

        public AdjuntoRepositorio(IGenericMap mapper)
        {
            this._mapper = mapper;
        }

        public AdjuntoDto GetById(int id)
        {
            var datos = db.Adjunto
                .FirstOrDefault(r => r.Id == id);
            return _mapper.MapFromModelToDto<Adjunto, AdjuntoDto>(datos);
        }
        
        public AdjuntoDto GetByUrl(string url)
        {
            var datos = db.Adjunto.FirstOrDefault(r => r.UrlArchivo == url);
            return _mapper.MapFromModelToDto<Adjunto, AdjuntoDto>(datos);
        }

        public DatosAjax<List<AdjuntoDto>> GetAdjuntosIngreso(int idIngreso, bool incluyeEliminados = false)
        {
            var query = db.Adjunto
                .Include(a => a.UsuarioCreacion)
                .Where(d => d.RequerimientoId == idIngreso 
                    && (incluyeEliminados || d.Eliminado==false));

            var datos = query
                .OrderBy(a => a.FechaCarga)
                .AsEnumerable()
                .Select(d => _mapper.MapFromModelToDto<Adjunto, AdjuntoDto>(d))
                .ToList();

            var resultado = new DatosAjax<List<AdjuntoDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;
        }

        public ResultadoOperacion Save(AdjuntoDto datos)
        {
            // TODO
            return new ResultadoOperacion(1, "Datos salvados con éxito.", null);
        }

        public ResultadoOperacion New(AdjuntoDto datos, ProcesaArchivo procesaArchivo)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            DbContextTransaction transaction = db.Database.BeginTransaction(); 
            try
            {

                var adjunto = _mapper.MapFromDtoToModel<AdjuntoDto, Adjunto>(datos);

                // Relaciones muchos a muchos. Se evita q se intente insertar en tablas hijas.
                foreach (var det in adjunto.TipoAdjunto ?? Enumerable.Empty<ListaValor>())
                {
                    db.Entry(det).State = EntityState.Unchanged;
                }

                db.Adjunto.Add(adjunto);
                db.SaveChanges();
                datos.Id = adjunto.Id;

                // Se sube al repositorio de archivos el archivo adjunto
                datos.DatosArchivo.TipoArchivo = TiposArchivo.Adjunto;
                datos.DatosArchivo.OrigenId = adjunto.Id;
                datos.DatosArchivo.OrigenCodigo = datos.DocIngreso;
                var resultArch = procesaArchivo(datos.DatosArchivo, true, false);

                if (resultArch.Codigo <= 0)
                {
                    Gedoc.Helpers.Logging.Logger.LogError(
                        $"Error creando nuevo Adjunto, falló la carga del archivo adjunto al repositorio de Gedoc.");
                    resultado.Codigo = -1; // -2 para ignorarlo en el formulario
                    resultado.Mensaje =
                        "¡Atención! No se ha podido crear el Adjunto, ha ocurrió un error al subir el archivo al repositorio de Gedoc.";
                    transaction?.Rollback(); 
                }
                else
                {
                    var archUrl = (string[])resultArch.Extra ?? new[] { "", "" };
                    adjunto.UrlArchivo = archUrl[0];
                    adjunto.NombreArchivo = archUrl[1];
                    resultado.Mensaje = $"Adjunto creado con éxito.";
                    db.SaveChanges();
                    transaction?.Commit();
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

            var adjunto = db.Adjunto.FirstOrDefault(d => d.Id == id);

            if (adjunto != null)
            {
                adjunto.UrlArchivo = urlArchivo;
                adjunto.NombreArchivo = nombreArchivo;

                db.SaveChanges();
            }

            return resultado;
        }

        public void MarcaAdjuntosEliminado(int[] adjIds, int usuarioId)
        {
            var adjuntos_borrar = db.Adjunto.Where(x => adjIds.Any(a => a == x.Id));

            foreach (var ab in adjuntos_borrar)
            {
                ab.Eliminado = true;
                ab.EliminacionFecha = DateTime.Now;
                ab.UsuarioEliminacionId = usuarioId;
            }
            db.SaveChanges();
        }

        public List<AdjuntoDto> GetAdjuntosUsuario(DateTime fechaD, DateTime fechaH, int usuarioId)
        {
            fechaD = fechaD.Date;
            fechaH = fechaH.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            var esAdmin = db.Usuario
                              .FirstOrDefault(u => u.Id == usuarioId && u.Rol.Any(r => r.Id == (int)Gedoc.Helpers.Enum.Rol.Administrador)) != null;
            var datos = db.Adjunto
                .Include(a => a.Requerimiento)
                .Where(a => a.FechaCarga >= fechaD && a.FechaCarga <= fechaH && !a.Eliminado &&
                            (esAdmin || a.UsuarioCreacionId == usuarioId));
            return datos.AsEnumerable().Select(_mapper.MapFromModelToDto<Adjunto, AdjuntoDto>).ToList();


        }

        #region Adjuntos Oficios

        public AdjuntoDto GetAdjuntoOficioById(int id)
        {
            var datos = db.AdjuntoOficio
                .FirstOrDefault(r => r.Id == id);
            return _mapper.MapFromModelToDto<AdjuntoOficio, AdjuntoDto>(datos);
        }

        public DatosAjax<List<AdjuntoDto>> GetAdjuntosOficio(int idOficio, bool incluyeEliminados = false)
        {
            var query = db.AdjuntoOficio
                .Include(a => a.UsuarioCreacion)
                .Where(d => d.OficioId == idOficio
                            && (incluyeEliminados || d.Eliminado == false));

            var datos = query
                .OrderBy(a => a.FechaCarga)
                .AsEnumerable()
                .Select(d => _mapper.MapFromModelToDto<AdjuntoOficio, AdjuntoDto>(d))
                .ToList();

            var resultado = new DatosAjax<List<AdjuntoDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;
        }

        public ResultadoOperacion SaveAdjuntoOficio(AdjuntoDto datos)
        {
            // TODO
            return new ResultadoOperacion(1, "Datos salvados con éxito.", null);
        }

        public ResultadoOperacion NewAdjuntoOficio(AdjuntoDto datos, ProcesaArchivo procesaArchivo)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);


            DbContextTransaction transaction = db.Database.BeginTransaction();
            try
            {
                var adjunto = _mapper.MapFromDtoToModel<AdjuntoDto, AdjuntoOficio>(datos);

                // Relaciones muchos a muchos. Se evita q se intente insertar en tablas hijas.
                foreach (var det in adjunto.TipoAdjunto ?? Enumerable.Empty<ListaValor>())
                {
                    db.Entry(det).State = EntityState.Unchanged;
                }

                db.AdjuntoOficio.Add(adjunto);
                db.SaveChanges();
                datos.Id = adjunto.Id;

                // Se sube al repositorio de archivos el archivo adjunto al Despacho
                datos.DatosArchivo.OrigenId = adjunto.Id;
                var resultArch = procesaArchivo(datos.DatosArchivo, true, false);

                if (resultArch.Codigo <= 0)
                {
                    Gedoc.Helpers.Logging.Logger.LogError(
                        $"Error creando nuevo adjunto de oficio, falló la carga del archivo.");
                    resultado.Codigo = -1; // -2 para ignorarlo en el formulario
                    resultado.Mensaje =
                        "¡Atención! No se ha podido crear el adjunto, ha ocurrió un error al subir el archivo adjunto.";
                    transaction?.Rollback(); 
                }
                else
                {
                    var archUrl = (string[])resultArch.Extra ?? new[] { "", "" };
                    adjunto.UrlArchivo = archUrl[0];
                    adjunto.NombreArchivo = archUrl[1];
                    resultado.Mensaje = "Se ha creado satisfactoriamente el adjunto.";
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

        public ResultadoOperacion UpdateDatosArchivoAdjuntoOficio(int id, string nombreArchivo, string urlArchivo)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var adjunto = db.AdjuntoOficio.FirstOrDefault(d => d.Id == id);

            if (adjunto != null)
            {
                adjunto.UrlArchivo = urlArchivo;
                adjunto.NombreArchivo = nombreArchivo;

                db.SaveChanges();
            }

            return resultado;
        }

        public void MarcaAdjuntosOficioEliminado(int[] adjIds, int usuarioId)
        {
            var adjuntos_borrar = db.AdjuntoOficio.Where(x => adjIds.Any(a => a == x.Id));

            foreach (var ab in adjuntos_borrar)
            {
                ab.Eliminado = true;
                ab.EliminacionFecha = DateTime.Now;
                ab.UsuarioEliminacionId = usuarioId;
            }
            db.SaveChanges();
        }

        public List<AdjuntoDto> GetAdjuntosOficioUsuario(DateTime fechaD, DateTime fechaH, int usuarioId)
        {
            var esAdmin = db.Usuario
                              .FirstOrDefault(u => u.Id == usuarioId && u.Rol.Any(r => r.Id == (int)Gedoc.Helpers.Enum.Rol.Administrador)) != null;
            var datos = db.AdjuntoOficio
                .Include(a => a.Oficio)
                .Where(a => a.FechaCarga >= fechaD && a.FechaCarga <= fechaH && !a.Eliminado &&
                            (esAdmin || a.UsuarioCreacionId == usuarioId));
            return datos.AsEnumerable().Select(_mapper.MapFromModelToDto<AdjuntoOficio, AdjuntoDto>).ToList();

        }
        #endregion
    }
}
