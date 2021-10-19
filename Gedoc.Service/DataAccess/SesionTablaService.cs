using AutoMapper.QueryableExtensions;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Enum;

namespace Gedoc.Service.DataAccess
{
    public class SesionTablaService : BaseService
    {
        public List<SelectListItemDto> GetUnidadTecnicaList()
        {
            var resultado = new List<SelectListItemDto>();
            try
            {
                using (var db = new GedocEntities())
                {
                    resultado = db.UnidadTecnica.Where(a => a.Activo == true)
                        .Select(b => new SelectListItemDto()
                        {
                            Text = b.Titulo,
                            Value = b.Id.ToString(),
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                resultado = new List<SelectListItemDto>();
                LogError(null, ex, "error al obtner el listado.");
            }
            return resultado;
        }

        public List<SelectListItemDto> GetEstadosList()
        {
            var resultado = new List<SelectListItemDto>();
            try
            {
                using (var db = new GedocEntities())
                {
                    resultado = db.EstadoRequerimiento.Where(a => a.Activo == true)
                        .Select(b => new SelectListItemDto()
                        {
                            Text = b.Titulo,
                            Value = b.Id.ToString(),
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                resultado = new List<SelectListItemDto>();
                LogError(null, ex, "error al obtner el listado.");
            }
            return resultado;
        }

        public List<SelectListItemDto> GetEtiquetasList()
        {
            var resultado = new List<SelectListItemDto>();
            try
            {
                using (var db = new GedocEntities())
                {
                    resultado = db.ListaValor.Where(a => a.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo && a.IdLista == 10)
                        .Select(b => new SelectListItemDto()
                        {
                            Text = b.Titulo,
                            Value = b.Codigo,
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                resultado = new List<SelectListItemDto>();
                LogError(null, ex, "error al obtner el listado.");
            }
            return resultado;
        }

        public List<SelectListItemDto> GetEtapasList()
        {
            var resultado = new List<SelectListItemDto>();
            try
            {
                using (var db = new GedocEntities())
                {
                    resultado = db.EtapaRequerimiento.Where(a => a.Activo == true)
                        .Select(b => new SelectListItemDto()
                        {
                            Text = b.Titulo,
                            Value = b.Id.ToString(),
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                resultado = new List<SelectListItemDto>();
                LogError(null, ex, "error al obtner el listado.");
            }
            return resultado;
        }

        public List<SelectListItemDto> GetSesionTabla(int unidadTecnicaId)
        {
            var resultado = new List<SelectListItemDto>();
            try
            {
                using (var db = new GedocEntities())
                {
                    resultado = db.SesionTabla.Where(a => a.UnidadTecnicaId == unidadTecnicaId)
                        .Select(b => new SelectListItemDto()
                        {
                            Text = b.Nombre,
                            Value = b.Id.ToString(),
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                resultado = new List<SelectListItemDto>();
                LogError(null, ex, "error al obtner el listado.");
            }
            return resultado;
        }

        public List<SelectListItemDto> GetDocumentosIngresos(int unidadTecnicaId, DateTime fechaDesde, DateTime fechaHasta)
        {
            var resultado = new List<SelectListItemDto>();
            try
            {
                if (unidadTecnicaId != 0)
                {
                    using (var db = new GedocEntities())
                    {
                        resultado = db.Requerimiento.Where(a => a.UtAsignadaId == unidadTecnicaId
                        && a.FechaIngreso >= fechaDesde && a.FechaIngreso <= fechaHasta
                        ).Select(b => new SelectListItemDto()
                        {
                            Text = b.DocumentoIngreso,
                            Value = b.DocumentoIngreso,
                        }).Distinct().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                resultado = new List<SelectListItemDto>();
                LogError(null, ex, "error al obtner el listado.");
            }
            return resultado;
        }

        public int CreateSesionTabla(int userId, string nombreTabla, int unidadTecnicaId)
        {
            try
            {
                using (var db = new GedocEntities())
                {
                    var sesiontabla = new SesionTabla()
                    {
                        Nombre = nombreTabla,
                        CreadoPorId = userId,
                        FechaCreacion = DateTime.Now,
                        UnidadTecnicaId = unidadTecnicaId
                    };
                    sesiontabla = db.SesionTabla.Add(sesiontabla);
                    var svc = db.SaveChanges();
                    return sesiontabla.Id;
                }
            }
            catch (Exception ex)
            {
                LogError(null, ex, "error al guardar SesionTabla.");
            }
            return 0;
        }

        public List<RequerimientoDto> GetRequerimientos(int unidadTecnicaId, DateTime desde, DateTime hasta, int[] estados, int[] etapas, int[] etiquetas, string[] documentoIngresos, string materia)
        {
            var resultado = new List<RequerimientoDto>();
            try
            {
                using (var db = new GedocEntities())
                {
                    var preResult = db.Requerimiento
                        .Where(a =>
                            a.UtAsignadaId == unidadTecnicaId
                            && a.FechaIngreso >= desde && a.FechaIngreso <= hasta
                        );

                    if (etapas?.Length > 0)
                        preResult = preResult.Where(a => etapas.Contains(a.EstadoId));

                    if (estados?.Length > 0)
                        preResult = preResult.Where(a => estados.Contains(a.EstadoId));

                    if (etiquetas?.Length > 0)
                        preResult = preResult.Where(a => etiquetas.Contains(a.EtapaId));

                    if (documentoIngresos?.Length > 0)
                        preResult = preResult.Where(a => documentoIngresos.Contains(a.DocumentoIngreso));

                    if (!string.IsNullOrEmpty(materia))
                        preResult = preResult.Where(a => a.Materia.Contains(materia)); //TODO: aqui es un like o busqueda exacta?

                    //preResult = preResult.OrderBy(a => a.Id);

                    resultado = preResult.Select(a => new RequerimientoDto()
                    {
                        Id = a.Id,
                        DocumentoIngreso = a.DocumentoIngreso,
                        FechaIngreso = a.FechaIngreso,
                        RemitenteNombre = a.Remitente.Nombre,
                        Materia = a.Materia,
                        FechaUltAcuerdoComision = a.FechaUltAcuerdoComision,
                        FechaUltAcuerdoSesion = a.FechaUltAcuerdoSesion,
                    }).ToList();
                }

            }
            catch (Exception ex)
            {
                resultado = new List<RequerimientoDto>();
                LogError(null, ex, "error al obtner el listado.");
            }
            return resultado;
        }

        public List<SelectListItemDto> GetUsuariosTablas()
        {
            var resultado = new List<SelectListItemDto>();
            try
            {
                using (var db = new GedocEntities())
                {
                    resultado = db.Usuario.Where(a => a.SesionTabla.Any() )
                        .Select(b => new SelectListItemDto()
                        {
                            Text = b.NombresApellidos,
                            Value = b.Id.ToString(),
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                resultado = new List<SelectListItemDto>();
                LogError(null, ex, "error al obtner el listado.");
            }
            return resultado;
        }

    }
}
