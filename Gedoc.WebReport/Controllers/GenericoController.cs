using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Gedoc.Helpers.Enum;
using Gedoc.Repositorio.Implementacion;
using Gedoc.Repositorio.Model;
using Gedoc.WebReport.Models;
using Gedoc.WebReport.Enums;
using Gedoc.WebReport.WSGedoc;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Maps;

namespace Gedoc.WebReport.Controllers
{
    [AllowAnonymous]
    public class GenericoController : ApiController
    {
        private GedocEntities db = new GedocEntities();

        // GET: api/Generico
        public IEnumerable<string> Get()
        {
            return null;
        }

        // GET: api/Generico/5
        public IHttpActionResult Get([FromUri]List<string> extraParam, int id, string idPadre="", int? todos=1, string filtro="")
        {
            var result = new List<Generico>();
            switch (id)
            {
                case (int)ListasGenericas.TipoMonumento:
                    result = db.ListaValor
                        .Where(w => w.IdLista == (int)Mantenedor.CategoriaMn && w.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo)
                        .OrderBy(d => d.Titulo)
                        .Select(d => new Generico { Id = d.Codigo, Nombre = d.Codigo })
                        .ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.UnidadTecnia:
                    result = db.UnidadTecnica.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.Profesionales:
                    var idUts = new List<int>();
                    int.TryParse(idPadre, out int idPadreInt);
                    if (idPadreInt > 0)
                    {
                        idUts.Add(idPadreInt);
                    }
                    else
                    {
                        extraParam = extraParam ?? new List<string>();
                        if (extraParam.Count > 0 && extraParam.All(e => e != "[TODOS]"))
                        {
                            idUts = db.UnidadTecnica
                                .Where(u => extraParam.Any(e =>
                                    e.Equals(u.Titulo, StringComparison.InvariantCultureIgnoreCase)))
                                .Select(u => u.Id)
                                .ToList();
                        }
                        else
                        {
                            idPadreInt = 0; // Todos
                        }
                    }
                    result = db.Usuario
                        .Where(q => q.Activo
                                    && q.Rol.Any(r => r.Id == 13) /*Profesional UT*/
                                    && (idPadreInt == 0 || q.UnidadTecnicaIntegrante.Any(ut => idUts.Any(u2 => u2 == ut.Id)))
                                    && (string.IsNullOrEmpty(filtro) || q.NombresApellidos.Contains(filtro)))
                        .OrderBy(q => q.NombresApellidos)
                        .Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.NombresApellidos })
                        .ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.Monumentos:
                    result = db.MonumentoNacional.Where(q => q.DenominacionOficial != null || q.OtrasDenominaciones != null).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.DenominacionOficial == null ? q.OtrasDenominaciones : q.DenominacionOficial }).ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.Estados:
                    result = db.EstadoRequerimiento.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.Etapas:
                    result = db.EtapaRequerimiento.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.UnidadTecnicaCopia:
                    result = db.UnidadTecnica.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.Etiqueta:
                    result = db.ListaValor
                        .Where(w => w.IdLista == (int) Mantenedor.Etiqueta &&
                                    w.IdEstadoRegistro == (int) EstadoRegistroEnum.Activo)
                        .OrderBy(d => d.Titulo)
                        .Select(d => new Generico() {Id = d.Codigo, Nombre = d.Titulo})
                        .ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.MotivoCierre:
                    result = db.MotivoCierre.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.Prioridad:
                    result = db.ListaValor
                        .Where(w => w.IdLista == (int)Mantenedor.Prioridad && w.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo)
                        .OrderBy(d => d.Titulo)
                        .Select(d => new Generico() { Id = d.Codigo, Nombre = d.Titulo })
                        .ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.Region:
                    result = db.ListaValor
                        .Where(lv => lv.IdLista == (int)Mantenedor.Region && lv.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo)
                        .Select(d => new Generico { Id = d.Codigo, Nombre = d.Titulo })
                        .OrderBy(d => d.Nombre)
                        .ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.Comuna:
                    result = db.ListaValor
                        .Where(d => d.IdLista == (int)Mantenedor.Comuna && d.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo)
                        .Select(d => new Generico { Id = d.Codigo, Nombre = d.Titulo })
                        .ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.TipoTramite:
                    result = db.TipoTramite.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.CanalLlegadaTramite:
                    result = db.ListaValor
                        .Where(w => w.IdLista == (int)Mantenedor.CanalLlegadaTramite && w.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo)
                        .OrderBy(d => d.Titulo)
                        .Select(d => new Generico { Id = d.Codigo, Nombre = d.Titulo })
                        .ToList();
                    return Json(result);

                case (int)ListasGenericas.Caso:
                    result = db.Caso.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
                    return Json(result);

                case (int)ListasGenericas.DocIngreso:
                    result = db.Requerimiento
                        .Where(q => q.FechaIngreso >= new DateTime(2018, 1, 1)) // TODO: precisar bien qué filtro aplicar para reporte Tramites
                        .Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.DocumentoIngreso }).ToList();
                    return Json(result);

                case (int)ListasGenericas.ComunaRegion:
                    result = (from lvC in db.ListaValor
                            join lisC in db.Lista on lvC.IdLista equals lisC.IdLista
                            join lvP in db.ListaValor on new { lisC.IdListaPadre, lvC.CodigoPadre } equals new { IdListaPadre = (int?)lvP.IdLista, CodigoPadre = lvP.Codigo }
                            join lisP in db.Lista on lvP.IdLista equals lisP.IdLista
                            join lvR in db.ListaValor on new { lisP.IdListaPadre, lvP.CodigoPadre } equals new { IdListaPadre = (int?)lvR.IdLista, CodigoPadre = lvR.Codigo }
                            where (lisC.IdLista == (int)Mantenedor.Comuna)
                            select new Generico { Id = lvC.Codigo, Nombre = lvC.Titulo + " - " + lvP.Titulo + " - " + lvR.Titulo })
                        .ToList();
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);

                case (int)ListasGenericas.TipoInstitucion:
                    result = new List<Generico>
                    {
                        new Generico { Id = "1", Nombre = "Pública" },
                        new Generico { Id = "2", Nombre = "Privada" },
                        new Generico { Id = "3", Nombre = "Interna" }
                    };
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(result);
                case (int)ListasGenericas.Semaforo:
                    var ressema = new List<Generico>
                    {
                        new Generico { Id = "1", Nombre = "Verde" },
                        new Generico { Id = "2", Nombre = "Amarillo" },
                        new Generico { Id = "3", Nombre = "Rojo" }
                    };
                    if (todos == 1)
                        result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
                    return Json(ressema);
                case (int)ListasGenericas.EstadoEtapa:
                    var datos = GetEstadosEtapa("[TODOS]");
                    return Json(datos);
                case (int)ListasGenericas.Remitente:
                    var rem = db.Remitente.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Nombre }).ToList();
                    return Json(rem);
                case (int)ListasGenericas.SesionTabla:
                    int.TryParse(idPadre, out int idPadreInt2);
                    var ses = db.SesionTabla.Where(q => idPadreInt2 == 0 || q.UnidadTecnicaId == idPadreInt2).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Nombre }).ToList();
                    return Json(ses);
                default:
                    return Json(new List<Generico>());
            }


            //return Json(new List<Generico>());

        }

        // POST: api/Generico
        public IHttpActionResult Post([FromBody]List<string> extraParam, int id, int? idPadre = 0, int? todos = 1)//([FromBody]string value)
        {
            var result = new List<Generico>();
            //switch (id)
            //{
            //    case (int)ListasGenericas.TipoMonumento:
            //        result = db.CategoriaMonumentoNac.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Codigo }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.UnidadTecnia:
            //        result = db.UnidadTecnica.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.Profesionales:
            //        var idUts = extraParam ?? new List<string>();
            //        //if (!string.IsNullOrWhiteSpace(extraParam))
            //        //{
            //        //    idUts = extraParam.Split(',');
            //        //}
            //        result = db.Usuario
            //            .Where(q => q.Activo
            //                        && q.Rol.Any(r => r.Id == 13) /*Profesional UT*/
            //                        && (idPadre <= 0 || q.UnidadTecnicaIntegrante.Any(ut => ut.Id == idPadre)))
            //            .Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.NombresApellidos })
            //            .ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.Monumentos:
            //        result = db.MonumentoNacional.Where(q => q.DenominacionOficial != null || q.OtrasDenominaciones != null).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.DenominacionOficial == null ? q.OtrasDenominaciones : q.DenominacionOficial }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.Estados:
            //        result = db.EstadoRequerimiento.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.Etapas:
            //        result = db.EtapaRequerimiento.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.UnidadTecnicaCopia:
            //        result = db.UnidadTecnica.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.Etiqueta:
            //        result = db.Etiqueta.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.MotivoCierre:
            //        result = db.MotivoCierre.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.Prioridad:
            //        result = db.Prioridad.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.Region:
            //        result = db.Region.Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.Comuna:
            //        idPadre = idPadre.HasValue && idPadre.Value > 0 ? idPadre : -1;
            //        result = db.Comuna.Include("Provincia")
            //            .Where(q => idPadre == -1 || q.Provincia.RegionId == idPadre)
            //            .Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.TipoTramite:
            //        result = db.TipoTramite.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.CanalLlegadaTramite:
            //        result = db.CanalLlegadaTramite.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
            //        return Json(result);

            //    case (int)ListasGenericas.Caso:
            //        result = db.Caso.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo }).ToList();
            //        return Json(result);

            //    case (int)ListasGenericas.DocIngreso:
            //        result = db.Requerimiento
            //            .Where(q => q.FechaIngreso >= new DateTime(2018, 1, 1)) // TODO: precisar bien qué filtro aplicar para reporte Tramites
            //            .Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.DocumentoIngreso }).ToList();
            //        return Json(result);

            //    case (int)ListasGenericas.ComunaRegion:
            //        result = db.Comuna
            //            .Include("Provincia")
            //            .Include("Region")
            //            .Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Titulo + " - " + q.Provincia.Titulo + " - " + q.Provincia.Region.Titulo }).ToList();
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);

            //    case (int)ListasGenericas.TipoInstitucion:
            //        result = new List<Generico>
            //        {
            //            new Generico { Id = "1", Nombre = "Pública" },
            //            new Generico { Id = "2", Nombre = "Privada" },
            //            new Generico { Id = "3", Nombre = "Interna" }
            //        };
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(result);
            //    case (int)ListasGenericas.Semaforo:
            //        var ressema = new List<Generico>
            //        {
            //            new Generico { Id = "1", Nombre = "Verde" },
            //            new Generico { Id = "2", Nombre = "Amarillo" },
            //            new Generico { Id = "3", Nombre = "Rojo" }
            //        };
            //        if (todos == 1)
            //            result.Insert(0, new Generico { Id = "0", Nombre = "[TODOS]" });
            //        return Json(ressema);
            //    case (int)ListasGenericas.EstadoEtapa:
            //        var datos = GetEstadosEtapa("[TODOS]");
            //        return Json(datos);
            //    case (int)ListasGenericas.Remitente:
            //        var rem = db.Remitente.Where(q => q.Activo == true).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Nombre }).ToList();
            //        return Json(rem);
            //    case (int)ListasGenericas.SesionTabla:
            //        idPadre = idPadre.GetValueOrDefault(0);
            //        var ses = db.SesionTabla.Where(q => idPadre == 0 || q.UnidadTecnicaId == idPadre).Select(q => new Generico { Id = q.Id.ToString(), Nombre = q.Nombre }).ToList();
            //        return Json(ses);
            //    default:
            //        return Json(new List<Generico>());
            //}

            return Json(result);

        }

        // PUT: api/Generico/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Generico/5
        public void Delete(int id)
        {
        }

        #region Búsqueda por Estado-Etapa
        public static List<Generico> GetEstadosEtapa(string primero)
        {
            var resultado = new List<Generico>();
            if (!string.IsNullOrEmpty(primero))
            {
                resultado.Add(new Generico() { Id = "-1", Nombre = primero });
            }

            resultado.Add(new Generico() { Id = "1", Nombre = "Ingresado -- Ingreso Central" });
            resultado.Add(new Generico() { Id = "2", Nombre = "Ingresado -- Asignación" });
            resultado.Add(new Generico() { Id = "3", Nombre = "Ingresado -- Reasignación" });
            resultado.Add(new Generico() { Id = "4", Nombre = "Asignado -- Unidad Técnica" });
            resultado.Add(new Generico() { Id = "5", Nombre = "Asignado -- Priorización" });
            resultado.Add(new Generico() { Id = "6", Nombre = "Asignado -- Asignación Profesional Temporal" });
            resultado.Add(new Generico() { Id = "7", Nombre = "Asignación Temporal -- Asignación" });
            resultado.Add(new Generico() { Id = "8", Nombre = "Asignación Temporal -- Unidad Técnica" });
            resultado.Add(new Generico() { Id = "9", Nombre = "Asignación Temporal -- Asignación Profesional Temporal" });
            resultado.Add(new Generico() { Id = "10", Nombre = "En proceso - Acuerdo de comisión -- Unidad Técnica" });
            resultado.Add(new Generico() { Id = "11", Nombre = "En proceso - Acuerdo de sesión -- Unidad Técnica" });
            resultado.Add(new Generico() { Id = "12", Nombre = "En proceso - En estudio -- Unidad Técnica" });
            resultado.Add(new Generico() { Id = "13", Nombre = "Cerrado -- Unidad Técnica" });

            return resultado;
        }

        public static List<string>[] SeparaEstadosEtapa(string estadosEtapas)
        {
            // estadosEtapas tiene los id de las combinaciones de estaod y etapas separados por coma
            var resultado = new List<string>[2] {
                new List<string>(),
                new List<string>()
            };
            if (string.IsNullOrEmpty(estadosEtapas))
            {
                return resultado;
            }
            if (("," + estadosEtapas + ",").Contains(",-1,"))
            {
                resultado[0].Add("[TODOS]");
                resultado[1].Add("[TODOS]");
                return resultado;
            }

            var estEtTodos = GetEstadosEtapa("");

            var estEtIdArr = estadosEtapas.Split(',');
            var separador = new string[] { "--" };
            foreach (var estEtId in estEtIdArr)
            {
                var estEt = estEtTodos.Where(ee => ee.Id == estEtId).FirstOrDefault();
                if (estEt != null && !string.IsNullOrEmpty(estEt.Nombre))
                {
                    var itemArr = estEt.Nombre.Split(separador, StringSplitOptions.RemoveEmptyEntries);
                    resultado[0].Add(itemArr[0].Trim());
                    if (itemArr.Length > 1)
                    {
                        resultado[1].Add(itemArr[1].Trim());
                    }
                }
            }
            return resultado;
        }

        #endregion




    }
}
