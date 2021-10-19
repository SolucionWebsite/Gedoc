using Gedoc.Repositorio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Maps.Interfaces;
using System.Linq.Dynamic;

namespace Gedoc.Interop.Wss.Services
{
    public class DataAccessService
    {
        private GedocEntities db = new GedocEntities();
        private readonly IGenericMap _mapper;

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
    }
}