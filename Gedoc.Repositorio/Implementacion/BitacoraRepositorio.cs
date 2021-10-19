using System;
using System.Collections.Generic;
using System.Linq;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Maps.Interfaces;
using System.Data.Entity;
using System.Linq.Dynamic;
using Gedoc.Repositorio.Model;
using Gedoc.Helpers;

namespace Gedoc.Repositorio.Implementacion
{
    public class BitacoraRepositorio : RepositorioBase, IBitacoraRepositorio
    {
        private readonly IGenericMap _mapper;
        private readonly IRequerimientoRepositorio _reqRepo;

        public BitacoraRepositorio(IGenericMap mapper, IRequerimientoRepositorio reqRepo)
        {
            this._mapper = mapper;
            _reqRepo = reqRepo;
        }

        public BitacoraDto GetById(int id)
        {
            var datos = db.Bitacora
                .Include(d => d.TipoBitacora)
                .Include(d => d.UsuarioCreacion)
                .FirstOrDefault(r => r.Id == id);
            return _mapper.MapFromModelToDto<Bitacora, BitacoraDto>(datos);
        }

        public BitacoraDto GetUltimoComentarioByTipo(string tipoBitacoraId, int reqId)
        {
            var datos = db.Bitacora
                .OrderByDescending(b => b.Fecha)
                .FirstOrDefault(r => r.TipoBitacoraCod == tipoBitacoraId && r.RequerimientoId == reqId);
            return _mapper.MapFromModelToDto<Bitacora, BitacoraDto>(datos);
        }

        public DatosAjax<List<BitacoraDto>> GetBitacorasIngreso(int idIngreso)
        {
            var query = db.Bitacora
                .Include(d => d.Requerimiento)
                .Include(d => d.TipoBitacora)
                .Include(d => d.UsuarioCreacion)
                .Where(d => d.RequerimientoId == idIngreso
                && d.Eliminado == false);

            var datos = query
                .OrderBy(b => b.Fecha)
                .AsEnumerable()
                .Select(d => _mapper.MapFromModelToDto<Bitacora, BitacoraDto>(d))
                .ToList();

            var resultado = new DatosAjax<List<BitacoraDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = datos.Count()
            };
            return resultado;
        }

        public DatosAjax<List<BitacoraDto>> GetBitacorasDespachoInic(int idDesp)
        {
            var query = db.Bitacora
                .Include(d => d.TipoBitacora)
                .Include(d => d.UsuarioCreacion)
                .Where(d => d.DespachoInicId == idDesp && d.Eliminado == false);

            var datos = query
                .OrderBy(b => b.Fecha)
                .AsEnumerable()
                .Select(d => _mapper.MapFromModelToDto<Bitacora, BitacoraDto>(d))
                .ToList();

            var resultado = new DatosAjax<List<BitacoraDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = datos.Count()
            };
            return resultado;
        }

        public ResultadoOperacion Save(BitacoraDto datos)
        {
            // TODO
            return new ResultadoOperacion(1, "Datos salvados con éxito.", null);
        }

        public ResultadoOperacion New(BitacoraDto datos, ProcesaArchivo procesaArchivo)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);


            DbContextTransaction transaction = db.Database.BeginTransaction();
            try
            {
                var bitacora = _mapper.MapFromDtoToModel<BitacoraDto, Bitacora>(datos);

                db.Bitacora.Add(bitacora);
                db.SaveChanges();
                datos.Id = bitacora.Id;

                // Se sube al repositorio de archivos el archivo adjunto
                datos.DatosArchivo.OrigenId = datos.Id;
                var resultArch = procesaArchivo(datos.DatosArchivo, true, false);

                if (resultArch.Codigo <= 0)
                {
                    Gedoc.Helpers.Logging.Logger.LogError(
                        $"Error creando nueva Bitácora, falló la carga del archivo adjunto al repositorio de Gedoc.");
                    resultado.Codigo = -1; // -2 para ignorarlo en el formulario
                    resultado.Mensaje =
                        "¡Atención! No se ha podido crear la Bitácora, ha ocurrió un error al subir el archivo al repositorio de Gedoc.";
                    transaction?.Rollback();
                }
                else
                {
                    // Se actualiza el estado y etapa del requerimiento si corresponde
                    if (bitacora.RequerimientoId.HasValue)
                    {
                        var req = _reqRepo.UpdateByBitacora(bitacora.TipoBitacoraCod, bitacora.RequerimientoId.GetValueOrDefault(0));
                        datos.Requerimiento = req;
                    }

                    var archUrl = (string[])resultArch.Extra ?? new[] { "", "" };
                    bitacora.UrlArchivo = archUrl[0];
                    bitacora.NombreArchivo = archUrl[1];
                    resultado.Mensaje = $"Bitácora creada con éxito.";
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

            var bitacora = db.Bitacora.FirstOrDefault(d => d.Id == id);

            if (bitacora != null)
            {
                bitacora.UrlArchivo = urlArchivo;
                bitacora.NombreArchivo = nombreArchivo;

                db.SaveChanges();
            }

            return resultado;
        }


    }
}
