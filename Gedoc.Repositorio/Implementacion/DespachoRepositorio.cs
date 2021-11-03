using System;
using System.Collections.Generic;
using System.Linq;
using Gedoc.Helpers.Dto;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Maps.Interfaces;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Gedoc.Repositorio.Model;
using Gedoc.Helpers;
using Gedoc.Helpers.Enum;
using EstadoOficio = Gedoc.Helpers.Enum.EstadoOficio;
using EtapaOficio = Gedoc.Helpers.Enum.EtapaOficio;
using Rol = Gedoc.Helpers.Enum.Rol;

namespace Gedoc.Repositorio.Implementacion
{

    public class DespachoRepositorio : RepositorioBase, IDespachoRepositorio
    {
        private readonly IGenericMap _mapper;

        public DespachoRepositorio(IGenericMap mapper)
        {
            this._mapper = mapper;
        }

        #region Despacho
        public DespachoDto GetById(int id)
        {
            var datos = db.Despacho
                .Include(d => d.EstadoDespacho)
                .Include(d => d.Remitente)
                .Include(d => d.DestinatarioCopia)
                //.Include(d => d.Requerimiento)
                .Include(d => d.TipoAdjunto)
                .Include(d => d.Soporte)
                .FirstOrDefault(r => r.Id == id);
            return _mapper.MapFromModelToDto<Despacho, DespachoDto>(datos);
        }

        public ResultadoOperacion GetDespachoDetalle(DespachoDto despachoDto)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var despacho = db.Despacho
                .Include(d => d.EstadoDespacho)
                .Include(d => d.Remitente)
                .Include(d => d.DestinatarioCopia)
                //.Include(d => d.Requerimiento)
                .Include(d => d.RequerimientoPrincipal)
                .Include(d => d.RequerimientoPrincipal.UnidadTecnicaAsign)
                .Include(d => d.RequerimientoPrincipal.ProfesionalUt)
                .Include(d => d.TipoAdjunto)
                .Include(d => d.Soporte)
                .FirstOrDefault(r => r.Id == despachoDto.Id);
            if (despacho == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el despacho especificado";
            }
            else
            {
                // Se actualiza el despacho especifcado de parametro con todos los datos obtenidos desde Bd
                _mapper.MapFromOrigenToDestino<Despacho, DespachoDto>(despacho, despachoDto);
                // TODO: configurar AutoMapper en vez de realizar lo siguiente, y después de configurado comprobar q no se afecte el uso de RequerimientoMain en otros lugares donde se ocupa
                var reqDto =
                    _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(despacho.RequerimientoPrincipal);
                despachoDto.RequerimientoMain = reqDto;
            }

            return resultado;
        }

        public DatosAjax<List<DespachoDto>> GetDatosUltimos(int diasAtras)
        {
            var datos = db.sp_DespachosUltimos(diasAtras)
                .Select(_mapper.MapFromModelToDto<sp_DespachosUltimos_Result, DespachoDto>).ToList();
            var resultado = new DatosAjax<List<DespachoDto>>(datos, new ResultadoOperacion(1, "OK", null));
            return resultado;
        }

        public DatosAjax<List<DespachoDto>> GetDespachosIngreso(int idIngreso, int skip, int take, SortParam sort)
        {
            var query = db.Despacho
                .Include(d => d.Requerimiento)
                .Include(d => d.Remitente)
                .Include(d => d.EstadoDespacho)
                .Include(d => d.ProveedorDespacho)
                .Where(d => d.Requerimiento.Any(r => r.Id == idIngreso)
                 && d.Eliminado == false);

            var datos = query
                .OrderBy($"{sort.Field} {sort.Dir}")
                .Skip(skip)
                .Take(take)
                .AsEnumerable()
                .Select(d => _mapper.MapFromModelToDto<Despacho, DespachoDto>(d))
                .ToList();

            var resultado = new DatosAjax<List<DespachoDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;
        }

        public ResultadoOperacion NewDespacho(DespachoDto datos, ProcesaArchivo procesaArchivo)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var despacho = _mapper.MapFromDtoToModel<DespachoDto, Despacho>(datos);

            DbContextTransaction transaction = datos.DesdeOficio ? null : db.Database.BeginTransaction(); //db.Database.CurrentTransaction ?? db.Database.BeginTransaction();

            // using (DbContextTransaction transaction = db.Database.BeginTransaction())
            try
            {
                // Si no tiene origen en un Oficio entonces se genera el valor de Número de Oficio en base al correlativo si es q el usuario no lo especifica
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
                    despacho.NumeroDespacho = datos.NumeroDespacho;
                }

                if (datos.Requerimiento != null && datos.Requerimiento.Count > 0)
                {
                    var reqIds = datos.Requerimiento.Select(r => r.IdInt).ToList();
                    var reqs = db.Requerimiento.Where(r => reqIds.Contains(r.Id)).ToList();
                    despacho.Requerimiento = reqs;
                }

                EvitaInsertHijo(despacho);

                db.Despacho.Add(despacho);
                db.SaveChanges();
                datos.Id = despacho.Id;
                datos.FolioDespacho = despacho.FolioDespacho ?? datos.FolioDespacho;
                datos.NumeroDespacho = despacho.NumeroDespacho ?? datos.NumeroDespacho;
                if (despacho.Requerimiento?.Count > 0)
                {
                    //TODO: obtener el requerimiento principal del despacho q sería el más reciente
                    datos.RequerimientoMain =
                        _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(
                            despacho.Requerimiento.FirstOrDefault());
                }

                // Se sube al repositorio de archivos el archivo adjunto al Despacho
                datos.DatosArchivo.OrigenId = despacho.Id;
                datos.DatosArchivo.OrigenCodigo = datos.RequerimientoMain?.DocumentoIngreso ?? despacho.NumeroDespacho;
                var resultArch = procesaArchivo(datos.DatosArchivo, true, false);

                if (resultArch.Codigo <= 0)
                {
                    Gedoc.Helpers.Logging.Logger.LogError(
                        $"Error creando nuevo Despacho, falló la carga del archivo adjunto. Id interno generado: {despacho.Id}");
                    resultado.Codigo = -1; // -2 para ignorarlo en el formulario
                    resultado.Mensaje =
                        "¡Atención! No se ha podido crear el Despacho, ha ocurrió un error al subir el archivo adjunto.";
                    transaction?.Rollback();  // Cuando el Despacho se origina de un Oficio el Rollback se hace en el método de firmar el oficio
                }
                else
                {
                    var archUrl = (string[]) resultArch.Extra ?? new[] {"", ""};
                    despacho.UrlArchivo = archUrl[0];
                    despacho.NombreArchivo = archUrl[1];
                    resultado.Mensaje = $"Número de Oficio: {despacho.NumeroDespacho}.";
                    db.SaveChanges();
                    transaction?.Commit();  // Cuando el Despacho se origina de un Oficio el Commit se hace en el método de firmar el oficio
                }

            } finally
            {
                transaction?.Dispose();
            }

            return resultado;
        }

        public ResultadoOperacion UpdateDatosArchivo(int id, string nombreArchivo, string urlArchivo)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var despacho = db.Despacho.FirstOrDefault(d => d.Id == id);

            if (despacho != null)
            {
                despacho.UrlArchivo = urlArchivo;
                despacho.NombreArchivo = nombreArchivo;

                db.SaveChanges();
            }

            return resultado;
        }

        public ResultadoOperacion Update(DespachoDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var despacho = db.Despacho
                .Include(r => r.Soporte)
                .Include(r => r.TipoAdjunto)
                .Include(r => r.Requerimiento)
                .Include(r => r.DestinatarioCopia)
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
            despacho.Materia = datos.Materia;
            #endregion
            #region Panel Adjuntos
            despacho.AdjuntaDocumentacion = datos.AdjuntaDocumentacion;
            despacho.CantidadAdjuntos = datos.CantidadAdjuntos;
            despacho.ObservacionesAdjuntos = datos.ObservacionesAdjuntos;
            #endregion
            #region Panel General
            despacho.ProyectoActividad = datos.ProyectoActividad;
            despacho.AdjuntaDocumentacion = datos.AdjuntaDocumentacion;
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
            // Update Destinatario en Copia
            var ids2 = datos.DestinatarioCopia.Select(d => d.IdInt).ToList();
            despacho.DestinatarioCopia = db.Remitente.Where(x => ids2.Any(d => d == x.Id)).ToList();
            // Update Requerimientos
            ids2 = datos.Requerimiento.Select(d => d.IdInt).ToList();
            if (ids2.Count > 0)
            {  // Es posible dejar el despacho sin requerimiento asociado (el campo Requerimiento no es obligatorio), en ese caso no se actualiza la relación y se conserva el requerimiento asociado, así se comporta en Gedoc antiguo
                despacho.Requerimiento = db.Requerimiento.Where(x => ids2.Any(d => d == x.Id)).ToList();
            }

            EvitaInsertHijo(despacho);

            #endregion

            db.SaveChanges();

            return resultado;
        }

        public ResultadoOperacion UpdateCierre(DespachoDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var despacho = db.Despacho
                .FirstOrDefault(d => d.Id == datos.Id);

            if (despacho == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el despacho a actualizar.";
                return resultado;
            }

            despacho.EstadoId = datos.EstadoId;
            despacho.MedioDespachoCod = datos.MedioDespachoCod;
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

        private void EvitaInsertHijo(Despacho despacho)
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
            foreach (var det in despacho.Requerimiento ?? Enumerable.Empty<Requerimiento>())
            {
                db.Entry(det).State = EntityState.Unchanged;
            }

        }
        #endregion

        #region Vistas
        public DatosAjax<List<DespachoDto>> GetDatosVistaDestinatario(int skip, int take,
            SortParam sort, string filterText)
        {
            var query = db.vw_DestinatarioDesp;

            var datos = query
                .OrderBy("RequerimientoMainUnidadTecnicaAsignTitulo ASC, FechaEmisionOficio DESC")
                .Skip(skip)
                .Take(take)
                .AsEnumerable()
                .Select(d => _mapper.MapFromModelToDto<vw_DestinatarioDesp, DespachoDto>(d))
                .ToList();
            var resultado = new DatosAjax<List<DespachoDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;
        }

        public ResultadoOperacion MarcaEliminado(int despachoId, int usuarioId)
        {
            var resultado = new ResultadoOperacion(1, "Datos eliminados con éxito", null);

            var despacho = db.Despacho.FirstOrDefault(a => a.Id == despachoId);
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

        public ResultadoOperacion EliminarDesp(int despachoId)
        {
            var resultado = new ResultadoOperacion(1, "Datos eliminados con éxito", null);

            var despacho = db.Despacho.FirstOrDefault(a => a.Id == despachoId);
            if (despacho == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el despacho a eliminar.";
                return resultado;
            }

            db.Despacho.Remove(despacho);
            db.SaveChanges();

            return resultado;
        }
        #endregion



        #region Oficios

        public DatosAjax<List<OficioDto>> GetDatosBandejaEntradaOficio(int idBandeja, int skip, int take, SortParam sort, string filterText,
            DateTime fechaDesde, int idUsuario, ConfigBandejaDto configBandeja)
        {
            var hayFiltroExtra = !string.IsNullOrEmpty(filterText) ? 1 : 0;

            var query = db.Oficio
                .Include(o => o.Requerimiento)
                .Include(o => o.UnidadTecnica)
                .Include(o => o.EstadoOficio)
                .Include(o => o.EtapaOficio)
                .Include(o => o.UsuarioCreacion)
                //.Include(o => o.UsuarioCreacion.UnidadTecnicaIntegrante)
             //   .Take(configBandeja.CantRegistros)
                .Where(d => !d.Eliminado);
            if (hayFiltroExtra == 1)
                query = query
                    .Where(d => SqlFunctions.StringConvert((double)d.Id).Contains(filterText) ||
                                 d.NumeroOficio.Contains(filterText) ||
                                 (d.FechaCreacion.Day + "/" + d.FechaCreacion.Month + "/" + d.FechaCreacion.Year).Contains(filterText) ||
                                 (d.FechaUltEstado.Day + "/" + d.FechaUltEstado.Month + "/" + d.FechaUltEstado.Year).Contains(filterText) ||
                                 d.EtapaOficio.Titulo.Contains(filterText) ||
                                 d.EstadoOficio.Titulo.Contains(filterText) ||
                                 d.Observaciones.Contains(filterText) ||
                                 d.Requerimiento.Any(r => r.DocumentoIngreso.Contains(filterText)));

            // Filtros segun estado y etapa
            var filtro = "";
            //foreach (var estado in estadosEtapa.Keys)
            //{
            //    filtro += (filtro == "" ? "(" : " OR ") + $" (EstadoId = {estado} AND EtapaId = {estadosEtapa[estado]}) ";
            //}

            foreach (var accion in configBandeja.Acciones)
            {
                filtro += (filtro == "" ? "(" : " OR ") + $" (EstadoId = {accion.EstadoId} AND EtapaId = {accion.EtapaId}) ";
            }

            filtro += filtro == "" ? "" : ")";
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                filtro = "( " + filtro + $" OR (UltimoUsuarioFlujoId = {idUsuario}) )";
                query = query.Where(filtro, new object[0]);
            }

            var datosUsuario = (db.Usuario
                                   .Include(u => u.Rol)
                              .FirstOrDefault(u => u.Id == idUsuario) ?? new Usuario());
            var esAdmin = datosUsuario.Rol?.Any(r => r.Id == (int)Rol.Administrador) ?? false;

            // Filtros de acuerdo a la bandeja desde la cual se está accediendo y de acuerdo a la UT del usuario.
            // Si es bandeja de transparencia entonces se toman los oficios q tengan requerimientos de transparencia
            // Si es bandeja de Visador General (antigua bandeja de Administración) el usuario tiene q pertenecer al perfil de Visador General
            // Si es bandeja de Jefatura CMN se chequea q el usuario sea el responsable de la UT de Jefatura CMN, si no lo es entonces no se le muestran los oficios
            // Si es las otras bandejas (de Profesional UT o de Encargado UT):
            //  Si es administrador entoncen se muestran todos los oficios
            //  Si es encargado (o subrogante) sólo deben aparecer oficios de Despacho q tengan requerimientos de (u oficios de Despachos Iniciativas, q tengan Unidad Técnica igual a) las UTs de las cuales es responsable
            //  Si es profesional sólo deben aparecer oficios de Despacho q tengan requerimientos de las UTs de las cuales es profesional asignado. U oficios de Despachos Iniciativas, q tengan Unidad Técnica igual a las UTs a las q pertenece el profesional
            //  Para el resto de los usuarios sólo deben aparecer oficios de Despacho q tengan requerimientos de (u oficios de Despachos Iniciativas, q tengan Unidad Técnica igual a) las UTs a las q el usuario pertenece
            if (esAdmin)
            {
                // No se hace nada
            }
            else if(idBandeja == (int) Bandeja.Transparencia)
            {
                query = query.Where(d => d.Requerimiento.Any(r => r.TipoIngreso == "SIAC/TRANSPARENCIA"));
            }
            else if (idBandeja == (int)Bandeja.Administracion)
            {
                // Es bandeja de Visador General, se chequea q el usuario pertenezca al perfil de Visador General
                var esVisador = datosUsuario.Rol?.Any(r => r.Id == (int)Rol.VisadorGeneral) ?? false;
                // No es Visador General, se aplica un filtro q no devolverá datos.
                if (!esVisador)
                    query = query.Where(d => d.Id < 0);
            } else if (idBandeja == (int) Bandeja.JefaturaUt)
            {
                // Es bandeja de Jefatura CMN, se comprueba q el usuario es el Responsable de Jefatura CMN
                var utJefaturaId = WebConfigValues.Ut_JefaturaCmn;
                utJefaturaId = utJefaturaId == 0 ? 76 : utJefaturaId;
                var esSecTecn = db.UnidadTecnica.Any(b => b.ResponsableId == idUsuario && b.Id == utJefaturaId);
                // No es el responsable de Jefatura CMN, se aplica un filtro q no devolverá datos.
                if (!esSecTecn)
                    query = query.Where(d => d.Id < 0);
            }
            else
            {
                var esEncargado = datosUsuario.Rol?.Any(r => r.Id == (int)Rol.EncargadoUt) ?? false;
                var esSubrogante = db.UnidadTecnica.Any(u => u.SubroganteId == idUsuario);
                var esProf = datosUsuario.Rol?.Any(r => r.Id == (int)Rol.ProfesionalUt) ?? false;
                if (esProf && (idBandeja == (int)Bandeja.ProfesionalUt || !esEncargado))
                {   // Si el usuario es Profesional UT se muestran los oficios de Despacho con requerimientos q el usuario es Profesional Asociado
                    // o q el usuario sea el q creó el oficio (para el caso de los oficios de despacho iniciativas)
                    query = query.Where(d => d.Requerimiento.Any(r => r.ProfesionalId == idUsuario) || (d.UsuarioCreacionId == idUsuario));
                }
                else
                {   // Si el usuario es Encargado UT se muestran sólo oficios con requerimientos de las UT q él sea encargado UT, o q sea el encargado de la UT del oficio  (para Despachos Iniciativas.
                    // Si no es encargado, ni tampoco profesional ut, se muestran sólo los oficios con requerimientos de las UT a las q pertenece el usuario, o q el usuario pertenezca a la ut del oficio  (para Despachos Iniciativas).
                    var uts = new List<int?>();
                    if (esEncargado || esSubrogante)
                    { // Si es encargado (o subrogante) se obtienen las UTs de las q el usuario es responsable ut (o subrogante)
                        var datoUt = db.UnidadTecnica.Where(b => b.ResponsableId == idUsuario || b.SubroganteId == idUsuario);
                        uts = datoUt.Select(d => (int?)d.Id).ToList();
                    }
                    else
                    {
                        // Si no es Encargado Ut se obtienen las UTs a las q pertenezca el usuario
                        var dato = db.UnidadTecnica.Where(b => b.UsuariosUt.Any(u => u.Id == idUsuario));
                        uts = dato.Select(d => (int?)d.Id).ToList();
                    }
                    if ((uts?.Count ?? 0) == 0)
                    { // No hay UT para el usuario, se aplica un filtro q no devolverá datos.
                        query = query.Where(d => d.Id < 0);
                    }
                    else
                    {
                        query = query.Where(d => d.Requerimiento.Any(r => uts.Any(utId => utId == r.UtAsignadaId)) 
                                                 || uts.Any(idUt => idUt == d.UnidadTecnicaId ) );
                    }
                }
            }

            //// Si el usuario es el último q cambió el oficio de etapa se le va a mostrar en su bandeja, pero sin acciones, solo ver
            //query = query.Where(d => d.UltimoUsuarioFlujoId == idUsuario);

            //****var test = query.ToList();

            var datos = query
                .OrderBy($"{sort.Field} {sort.Dir}")
                .Skip(skip)
                .Take(take)
                .AsEnumerable()
                .Select(d => _mapper.MapFromModelToDto<Oficio, OficioDto>(d))
                .ToList();

            var resultado = new DatosAjax<List<OficioDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;
        }

        public List<OficioDto> GetOficoAll()
        {
            var datos = db.Oficio
                .Include(d => d.EstadoOficio)
                .Include(d => d.EtapaOficio)
                .Where(r => !r.Eliminado);
            return datos.Select(_mapper.MapFromModelToDto<Oficio, OficioDto>).ToList();
        }

        public OficioDto GetOficoById(int id)
        {
            var datos = db.Oficio
                .Include(d => d.Requerimiento)
                .Include(d => d.EstadoOficio)
                .Include(d => d.EtapaOficio)
                .Include(d => d.PlantillaOficio)
                .FirstOrDefault(r => r.Id == id);
            return _mapper.MapFromModelToDto<Oficio, OficioDto>(datos);
        }

        public List<OficioDto> GetOficiosPendienteFirma()
        {
            var datos = db.Oficio
                .Where(r => !r.Eliminado && r.EstadoId == (int)EstadoOficio.EnviadoFirma && !string.IsNullOrEmpty(r.CodigoDocFirmado));
            return datos.Select(_mapper.MapFromModelToDto<Oficio, OficioDto>).ToList();
        }

        public List<OficioObservacionDto> GetObservacionesOficio(int oficioId)
        {
            var datos = db.OficioObservacion
                .Include(d => d.EstadoOficio)
                .Include(d => d.EtapaOficio)
                .Include(d => d.Usuario)
                .Where(r => r.OficioId == oficioId)
                .OrderByDescending(r => r.Fecha);
            return datos.Select(_mapper.MapFromModelToDto<OficioObservacion, OficioObservacionDto>).ToList();
        }

        //public ResultadoOperacion SaveOficio(OficioDto datos, bool updateSoloActivo)
        //{
        //    var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);
        //    if (datos.Id == 0)
        //    { // Nuevo
        //        var oficio = _mapper.MapFromDtoToModel<OficioDto, Oficio>(datos);
        //        oficio.EstadoId = (int)EstadoOficio.Borrador;
        //        oficio.FechaUltEstado = DateTime.Now;
        //        oficio.EtapaId = (int)EtapaOficio.EnProceso;
        //        oficio.Eliminado = false;
        //        oficio.FechaCreacion = DateTime.Now;
        //        oficio.FechaModificacion = DateTime.Now;
        //        oficio.UsuarioCreacionId = datos.UsuarioActualId;
        //        oficio.UsuarioModificacionId = datos.UsuarioActualId;
        //        db.Oficio.Add(oficio);
        //        db.SaveChanges();
        //        datos.Id = oficio.Id;
        //    }
        //    else
        //    { // Update
        //        var oficio = db.Oficio.FirstOrDefault(n => n.Id == datos.Id);
        //        if (oficio == null)
        //        {
        //            return new ResultadoOperacion(-1, "No se encontró el Id de oficio a actualizar", null);
        //        }

        //        oficio.Contenido = datos.Contenido;
        //        oficio.FechaModificacion = DateTime.Now;
        //        oficio.UsuarioModificacionId = datos.UsuarioActualId;
        //        db.SaveChanges();
        //    }

        //    return resultado;
        //}

        public ResultadoOperacion NewOficio(OficioDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var oficio = _mapper.MapFromDtoToModel<OficioDto, Oficio>(datos);

            if (datos.Requerimiento != null && datos.Requerimiento.Count > 0)
            {
                var reqIds = datos.Requerimiento.Select(r => r.IdInt).ToList();
                var reqs = db.Requerimiento.Where(r => reqIds.Contains(r.Id)).ToList();
                oficio.Requerimiento = reqs;
                oficio.UnidadTecnicaId = reqs.FirstOrDefault()?.UtAsignadaId;
            }

            // Evitar insert en tablas hijas
            foreach (var det in oficio.Requerimiento ?? Enumerable.Empty<Requerimiento>())
            {
                db.Entry(det).State = EntityState.Unchanged;
            }

            oficio.UltimoUsuarioFlujoId = datos.UsuarioCreacionId;
            db.Oficio.Add(oficio);
            db.SaveChanges();
            datos.Id = oficio.Id;
            datos.NumeroOficio = oficio.NumeroOficio;
            if (oficio.Requerimiento?.Count > 0)
            {
                //datos.Requerimientos = oficio.Requerimiento.Select(r => _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(r)).ToList();
                datos.RequerimientoMain = _mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>(oficio.Requerimiento.FirstOrDefault());
            }
            return resultado;
        }

        public ResultadoOperacion UpdateOficio(OficioDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var oficio = db.Oficio
                .Include(r => r.Requerimiento)
                .FirstOrDefault(d => d.Id == datos.Id);

            if (oficio == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el oficio a actualizar.";
                return resultado;
            }

            oficio.NumeroOficio = datos.NumeroOficio ?? oficio.NumeroOficio;
            oficio.FechaEmisionOficio = datos.FechaEmisionOficio ?? oficio.FechaEmisionOficio;
            oficio.Contenido = datos.Contenido;
            oficio.FechaModificacion = datos.FechaModificacion;
            oficio.UsuarioModificacionId = datos.UsuarioModificacionId;

            #region Relaciones de muchos-muchos
            // Update Requerimientos
            var ids2 = datos.Requerimiento.Select(d => d.IdInt).ToList();
            oficio.Requerimiento = db.Requerimiento.Where(x => ids2.Any(d => d == x.Id)).ToList();
            oficio.UnidadTecnicaId = oficio.Requerimiento.FirstOrDefault()?.UtAsignadaId;

            // Evita Insert Hijo
            foreach (var det in oficio.Requerimiento ?? Enumerable.Empty<Requerimiento>())
            {
                db.Entry(det).State = EntityState.Unchanged;
            }

            #endregion

            db.SaveChanges();
            datos.FechaCreacion = oficio.FechaCreacion;

            return resultado;
        }

        public ResultadoOperacion UpdateEstadoOficio(OficioDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Operación realizada con éxito", null);

            var oficio = db.Oficio
                .Include(r => r.Requerimiento)
                .FirstOrDefault(d => d.Id == datos.Id);

            if (oficio == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el oficio a actualizar.";
                return resultado;
            }

            oficio.NumeroOficio = datos.NumeroOficio ?? (datos.Accion == AccionOficio.EDITFIRMADO.ToString() ? "" : oficio.NumeroOficio);
            oficio.RequerimientoPrincipalId = datos.RequerimientoPrincipalId ?? oficio.RequerimientoPrincipalId;
            oficio.FechaEmisionOficio = datos.Accion == AccionOficio.EDITFIRMADO.ToString() ? datos.FechaEmisionOficio : (datos.FechaEmisionOficio ?? oficio.FechaEmisionOficio);
            oficio.FechaModificacion = datos.FechaModificacion;
            oficio.UsuarioModificacionId = datos.UsuarioModificacionId;
            oficio.EstadoId = datos.EstadoId == -1 ? oficio.EstadoId : datos.EstadoId;
            oficio.EtapaId = datos.EtapaId == -1 ? oficio.EtapaId : datos.EtapaId;
            oficio.UrlArchivo = datos.UrlArchivo ?? oficio.UrlArchivo;
            oficio.NombreArchivo = datos.NombreArchivo ?? oficio.NombreArchivo;
            oficio.Urgente = datos.Urgente;
            if (datos.Accion != AccionOficio.EDITFIRMADO.ToString())
            {
                oficio.FechaUltEstado = datos.FechaUltEstado;
                oficio.FechaUltEtapa = datos.FechaUltEtapa;
            }
            oficio.Observaciones = datos.Observaciones ?? oficio.Observaciones;
            if (datos.Eliminado)
            {
                oficio.Eliminado = datos.Eliminado;
                oficio.EliminacionFecha = datos.EliminacionFecha;
                oficio.UsuarioEliminacionId = datos.UsuarioEliminacionId;
            }

            oficio.UltimoUsuarioFlujoId = datos.UsuarioModificacionId ?? oficio.UltimoUsuarioFlujoId;

            if (datos.NuevaObservacion)
            {
                db.OficioObservacion.Add(new OficioObservacion {
                    OficioId = datos.Id,
                    EstadoId = datos.EstadoId,
                    EtapaId = datos.EtapaId,
                    Fecha = datos.FechaUltEstado,
                    UsuarioId = datos.UsuarioModificacionId.GetValueOrDefault(0),
                    Observaciones = datos.Observaciones
                });
            }

            db.SaveChanges();
            datos.FechaCreacion = oficio.FechaCreacion;
            if (oficio.Requerimiento != null) // Para utilizarlo en el log de sistema luego:
                datos.Requerimiento = oficio.Requerimiento.Select(r => new GenericoDto { IdInt = r.Id}).ToList();

            return resultado;
        }

        /// <summary>
        /// Envía el oficio a firma digital. Involucra los siguientes pasos:
        /// <list type="number">
        /// <item>Obtener número de correlativo del oficio para generar el Número de Oficio</item>
        /// <item>Generar el pdf del oficio en base a la propuesta de oficio creada por el usuario</item>
        /// <item>Enviar al firmador digital el pdf del oficio para su firma digital. El resultado puede ser:</item>
        ///     <list type="bullet">
        ///     <item>El documento se crea en la plataforma de firma digital y también queda firmado. En este caso se recibe el pdf firmado y:</item>
        ///         <list type="bullet">
        ///         <item>Se sube el pdf firmado al repositorio de Gedoc</item>
        ///         <item>Se crea el despacho del oficio</item>
        ///         <item>El estado del oficio queda como Firmado por Jefatura CMN</item>
        ///         </list>
        ///     <item>El documento se crea en la plataforma pero no se puede firmar. En este caso NO se recibe el pdf firmado y:</item>
        ///         <list type="bullet">
        ///         <item>El estado del oficio queda como Enviado a Firma (y no se realiza otra operación)</item>
        ///         </list>
        ///     <item>No se crea el documento ni se firma, ocurre algún error:</item>
        ///         <list type="bullet">
        ///         <item>No se realiza ninguna operación y se revierte los cambios realizados.</item>
        ///         </list>
        ///     </list>
        /// </list>
        /// </summary>
        /// <param name="datos">Datos del oficio</param>
        /// <param name="procesaArchivo">Delegate que sube el archivo pdf al repositorio de Gedoc</param>
        /// <param name="creaNuevoDespacho">Delegate que crea el nuevo despacho</param>
        /// <param name="firmaDigitalOficio">Delegate que realiza la firma digital</param>
        /// <returns></returns>
        public ResultadoOperacion FirmarOficio(OficioDto datos, ProcesaArchivo procesaArchivo, CreaNuevoDespacho creaNuevoDespacho, FirmaDigitalOficio firmaDigitalOficio)
        {
            var resultado = new ResultadoOperacion(1, "Operación realizada con éxito", null);

            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {

                var oficio = db.Oficio
                    .Include(r => r.Requerimiento)
                    .Include(r => r.PlantillaOficio)
                    .FirstOrDefault(d => d.Id == datos.Id);

                if (oficio == null)
                {
                    resultado.Codigo = -1;
                    resultado.Mensaje = "No se encontró el oficio a actualizar.";
                    return resultado;
                }

                // Se genera el Número de Oficio de manera automática, el mismo q será usado en el Despacho q se genere
                // a partir del Oficio
                oficio.FechaEmisionOficio = DateTime.Now;
                var anno = oficio.FechaEmisionOficio.GetValueOrDefault(DateTime.Now).Year;
                var corr = db.Correlativo.FirstOrDefault(c => c.Anno == anno);
                if (corr == null)
                {
                    throw new Exception("No se encontró en base de datos el correlativo para el año " + anno);
                }
                var corrDespacho = corr.CorrelativoDespacho++;
                var corrDespachoStr = corrDespacho.ToString().PadLeft(5, '0');

                var folio = $"{corrDespachoStr}-{anno}";
                oficio.NumeroOficio = folio; /**** Se genera de manera automática el Número de Despacho *******/

                oficio.FechaModificacion = datos.FechaModificacion;
                oficio.UsuarioModificacionId = datos.UsuarioModificacionId;
                oficio.EstadoId = datos.EstadoId;
                oficio.EtapaId = datos.EtapaId;

                // Se sustituye en el contenido del oficio las variables 
                var secTecnico = "";
                if (oficio.Contenido.IndexOf("%SecretarioTecn%", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    // Se obtiene el Secretario Técnico q es el responsable de la UT Jefatura CMN
                    var utJefaturaId = WebConfigValues.Ut_JefaturaCmn;
                    utJefaturaId = utJefaturaId == 0 ? 76 : utJefaturaId;
                    var ut = db.UnidadTecnica
                        .Include(u => u.Responsable)
                        .FirstOrDefault(u => u.Id == utJefaturaId);
                    secTecnico = ut?.Responsable?.NombresApellidos ?? "";
                    // La variable %SecretarioTecn% si está dentro de una función entonces llega aquí encerrada en la función por lo q es necesario sustituir el 
                    //valor de esta variable pero además aplicar la función correspondiente:
                    oficio.Contenido = ReemplazaFuncion("MAYUSC", oficio.Contenido, "%SecretarioTecn%", secTecnico);
                    oficio.Contenido = ReemplazaFuncion("MINUSC", oficio.Contenido, "%SecretarioTecn%", secTecnico);
                    oficio.Contenido = ReemplazaFuncion("SIGLAS", oficio.Contenido, "%SecretarioTecn%", secTecnico);
                    // si la variable %SecretarioTecn% NO está dentro de una función entonces se sustituye la variable por su correspondiente valor
                    oficio.Contenido = Regex.Replace(oficio.Contenido, "%SecretarioTecn%", secTecnico, RegexOptions.IgnoreCase);
                }
                oficio.Contenido = Regex.Replace(oficio.Contenido, "%NumOficio%", oficio.NumeroOficio, RegexOptions.IgnoreCase);
                var fechaOficio = oficio.FechaEmisionOficio.Value.ToString(GeneralData.FORMATO_FECHA_TEXTUAL,
                    System.Globalization.CultureInfo.CreateSpecificCulture("es-ES"));
                oficio.Contenido = Regex.Replace(oficio.Contenido, "%FechaOficio%", fechaOficio, RegexOptions.IgnoreCase);

                // Se graban los datos del oficio
                db.SaveChanges();

                datos.datosArchivo.OrigenId = oficio.Id;
                datos.datosArchivo.OrigenCodigo = oficio.NumeroOficio;
                datos.datosArchivo.FileName = $"Oficio_{oficio.NumeroOficio}.pdf";
                datos.datosArchivo.FileTextContent = oficio.Contenido;

                var subirARepoGedoc = true; // *******************
                // (1) SE CREA Y FIRMA EL OFICIO EN EL FIRMADOR DIGITAL
                var resultFirmaDig = firmaDigitalOficio(datos.datosArchivo, false);
                if (resultFirmaDig.Codigo > 0 || resultFirmaDig.Codigo == -2)
                {
                    // Documento CREADO con éxito en el firmador. Puede q se haya FIRMADO con éxito o puede q no.

                    // Se actualiza en el oficio el código de documento devuelto por el firmador digital
                    var docResult = resultFirmaDig.Extra as Dictionary<string, object>
                                    ?? new Dictionary<string, object> { { "code", "" }, { "file", "" }, { "url_preview", "" } };
                    oficio.CodigoDocFirmado = docResult.ContainsKey("code") ? docResult["code"]?.ToString() : "";
                    if (!subirARepoGedoc) oficio.UrlArchivo = docResult.ContainsKey("url_preview") ? docResult["url_preview"]?.ToString() : "";

                    if (!docResult.ContainsKey("file") || string.IsNullOrEmpty(docResult["file"]?.ToString()))
                    {
                        oficio.EstadoId = (int)EstadoOficio.EnviadoFirma;
                        db.SaveChanges();
                        // El firmador no devolvió documento firmado, pero se creó el documento en el firmador. Indica
                        // q está aún pendiente de firma el documento.
                        // No se genera despacho ni se sube oficio al repositorio de Gedoc.
                        resultado.Codigo = -1;
                        resultado.Mensaje = $"Oficio enviado a firma. Se generó el número de oficio: {oficio.NumeroOficio}<br/>" +
                                            $"Sin embargo, no fue posible firmar en estos momento el oficio.<br/>" +
                                            $"Mensaje devuelto por la plataforma de firma digital:<br/>" + resultFirmaDig.Mensaje;
                        transaction.Commit();
                    }
                    else
                    {
                        // El firmador devolvió el documento firmado. Se sube el documento al repositorio Gedoc y se genera el despacho
                        oficio.EstadoId = (int)EstadoOficio.Firmado;
                        db.SaveChanges();
                        datos.datosArchivo.FileTextContent = docResult["file"].ToString();

                        // (2) SE SUBE EL DOCUMENTO AL REPOSITORIO DE ARCHIVOS EL PDF FIRMADO, generado a partir del contenido del oficio y firmado en el firmador digital
                        var resultArch = procesaArchivo(datos.datosArchivo, subirARepoGedoc, false);

                        if (resultArch.Codigo <= 0)
                        { // ERROR SUBIENDO EL PDF HACIA EL REPOSITORIO DE ARCHIVOS
                            // Se elimina del firmador digital el documento recien creado
                            datos.datosArchivo.OrigenCodigo = oficio.CodigoDocFirmado;
                            firmaDigitalOficio(datos.datosArchivo, true);

                            Gedoc.Helpers.Logging.Logger.LogError($"Error firmando el Oficio, falló la carga del archivo PDF. Id interno generado: {oficio.Id}");
                            resultado.Codigo = 1; // -2 para ignorarlo en el formulario
                            resultado.Mensaje = "¡Atención! No se ha podido firmar el Oficio, ha ocurrió un error al subir el PDF del oficio.";
                            transaction.Rollback();
                        }
                        else
                        {
                            var archUrl = (string[])resultArch.Extra ?? new[] { "", datos.datosArchivo.FileName };
                            if (subirARepoGedoc) oficio.UrlArchivo = archUrl[0];
                            oficio.NombreArchivo = archUrl[1];
                            // Se actualiza en el oficio los datos del archivo subido al repositorio
                            db.SaveChanges();

                            // (3) SE GENERA EL DESPACHO O DESPACHO INICIATIVAS EN BASE AL OFICIO FIRMADO:
                            var despacho = new DespachoDto();
                            var despachoInic = new DespachoIniciativaDto();
                            if (oficio.PlantillaOficio.TipoPlantillaId == (int)TipoPlantillaOficio.Despacho)
                            {
                                // Oficio de Despacho. Valores por defecto:
                                despacho.FechaEmisionOficio = oficio.FechaEmisionOficio.Value.Date; // Se quita la hora de la Fecha de Oficio para el despacho. Si se quiere dejar la hora entonces tener en cuenta q al editar el despacho, como el formulario tiene para especificar la Fecha de Emisión, entonces se pierde la hora al grabar
                                despacho.NumeroDespacho = oficio.NumeroOficio;
                                despacho.Requerimiento =
                                    oficio.Requerimiento.Select(_mapper.MapFromModelToDto<Requerimiento, GenericoDto>).ToList();
                                datos.datosArchivo.OrigenId = 0;
                                datos.datosArchivo.OrigenCodigo = "";
                                despacho.DatosArchivo = datos.datosArchivo; //new DatosArchivo { FileName = oficio.NombreArchivo };
                                if (!subirARepoGedoc) despacho.UrlArchivo = oficio.UrlArchivo;
                                despachoInic = null;
                            }
                            else
                            {
                                // Oficio de Despacho Iniciativas. Valores por defecto:
                                despachoInic.FechaEmisionOficio = oficio.FechaEmisionOficio.Value.Date; // Se quita la hora de la Fecha de Oficio para el despacho inic. Si se quiere dejar la hora entonces tener en cuenta q al editar el despacho inic, como el formulario tiene para especificar la Fecha de Emisión, entonces se pierde la hora al grabar
                                despachoInic.NumeroDespacho = oficio.NumeroOficio;
                                despachoInic.UtAsignadaId = oficio.UnidadTecnicaId;
                                despachoInic.ProfesionalId = oficio.UsuarioCreacionId; // Se asume q el profesional es el q creó el oficio
                                despachoInic.MonumentoNacional = new MonumentoNacionalDto();
                                datos.datosArchivo.OrigenId = 0;
                                datos.datosArchivo.OrigenCodigo = "";
                                despachoInic.DatosArchivo = datos.datosArchivo; //new DatosArchivo { FileName = oficio.NombreArchivo };
                                despacho = null;
                            }
                            var resultNuevoDesp = creaNuevoDespacho(despacho, despachoInic);

                            if (resultNuevoDesp.Codigo > 0)
                            {
                                var textoFirma = "El Oficio se ha firmado con éxito";
                                resultado.Mensaje = $"{textoFirma}.<br/>Se ha generado el " +
                                                    $"{(oficio.PlantillaOficio.TipoPlantillaId == (int)TipoPlantillaOficio.Despacho ? "Despacho" : "Despacho Iniciativas CMN")} " +
                                                    $"con Número de Oficio: {oficio.NumeroOficio}.";
                                transaction.Commit();
                            }
                            else
                            {
                                // ERROR CREANDO EL DESPACHO A PARTIR DEL OFICIO
                                // Se elimina del repositorio de archivos de Gedoc el pdf del oficio q fue subido al repositorio
                                var resultDelArch = procesaArchivo(datos.datosArchivo, subirARepoGedoc, true);

                                // Se elimina del firmador digital el documento recien creado
                                datos.datosArchivo.OrigenCodigo = oficio.CodigoDocFirmado;
                                firmaDigitalOficio(datos.datosArchivo, true);

                                resultado.Codigo = -1;
                                resultado.Mensaje = resultNuevoDesp.Mensaje; // "Ha ocurrido un error al realizar la operación. Por favor, chequee el log de error de la aplicación.";
                                transaction.Rollback();
                            }
                        }
                    }
                }
                else
                {
                    // ERROR EN FIRMA DIGITAL
                    // Hubo error con el firmador, no se generó documento en el firmador (por ej no se pudo contactar la api o algún error devuelto por la api)
                    resultado.Codigo = -1;
                    resultado.Mensaje = resultFirmaDig.Mensaje;
                    transaction.Rollback();
                    return resultado;
                }


                datos.NumeroOficio = oficio.NumeroOficio;
                datos.FechaCreacion = oficio.FechaCreacion;
                if (oficio.Requerimiento != null) // Para utilizarlo en el log de sistema luego:
                    datos.Requerimiento = oficio.Requerimiento.Select(r => new GenericoDto { IdInt = r.Id }).ToList();
            }

            return resultado;
        }

        /// <summary>
        /// Reemplaza, en el texto especificado, la función especificada como parámetro con su respectivo valor.
        /// Esta función dentro del texto puede tener dentro variables (están encerradas entre %%) las cuales son
        /// sustituidas por sus respectivos valores y luego se le aplica la función especificada.
        /// <para>Ejemplo de una función dentro del texto: MAYUSC(%RemitenteNombre%), lo cual debe sustituirse por el nombre
        /// del remitente del requerimiento, en mayúsculas</para>
        /// </summary>
        /// <param name="funcion">Función a buscar y reemplazar en el texto</param>
        /// <param name="contenido">Texto a buscar la función</param>
        /// dentro de la función.</param>
        /// <returns></returns>
        private string ReemplazaFuncion(string funcion, string contenido, string variable, string valorVariable)
        {
            foreach (Match match in Regex.Matches(
                contenido,
                $"{funcion}[(](.*?)[)]"))
            {
                var contenidoFunc = match.Groups[1].Value;
                var contenidoFuncConDatos = Regex.Replace(contenidoFunc, variable, valorVariable, RegexOptions.IgnoreCase);

                // Se aplica la función en cuestión
                // Se decodifica cualquier codificiacón Html, por ejemplo de las tildes.
                contenidoFuncConDatos = System.Net.WebUtility.HtmlDecode(contenidoFuncConDatos) ?? "";
                switch (funcion)
                {
                    case "MAYUSC":
                        contenidoFuncConDatos = contenidoFuncConDatos.ToUpper();
                        break;
                    case "MINUSC":
                        contenidoFuncConDatos = contenidoFuncConDatos.ToLower();
                        break;
                    case "SIGLAS":
                        if (!string.IsNullOrWhiteSpace(valorVariable) && contenidoFuncConDatos.Contains(valorVariable))
                        {
                            // Se obtienen las siglas
                            string[] strSplit = valorVariable.Split();
                            var siglas = "";
                            foreach (string res in strSplit)
                            {
                                siglas += res.Substring(0, 1);
                            }
                            // Se sustituye las siglas en el valor
                            contenidoFuncConDatos = contenidoFuncConDatos.Replace(valorVariable, siglas.ToUpper());
                        }
                        break;
                }
                // Se vuelve a codificar a Html
                //contenidoFuncConDatos = System.Net.WebUtility.HtmlEncode(contenidoFuncConDatos);

                // Se sustituye, en el contenido de la plantilla, la función por el valor obtenido al aplicar la función
                contenido = contenido.Replace($"{funcion}({contenidoFunc})", contenidoFuncConDatos);
            }

            return contenido;
        }

        /// <summary>
        /// Se actualiza el oficio luego de que ha sido firmado en la plataforma de firma digital.<br/>
        /// Esto sucede cuando al enviar el oficio al firmado digital este no puede firmarse en ese momento y, en algún momento posterior,
        /// se realiza la firma del documento por alguna persona utilizando las herramientas de la plataforma de firma digital.<br/>
        /// Se realizan los siguientes pasos:
        /// <list type="number">
        /// <item>Se sube el pdf firmado al repositorio de Gedoc</item>
        /// <item>Se crea el despacho del oficio</item>
        /// <item>El estado del oficio pasa a Firmado por Jefatura CMN</item>
        /// </list>
        /// </summary>
        /// <param name="datos">Datos del oficio</param>
        /// <param name="procesaArchivo">Delegate que sube el archivo pdf al repositorio de Gedoc</param>
        /// <param name="creaNuevoDespacho">Delegate que crea el nuevo despacho</param>
        /// <returns></returns>
        public ResultadoOperacion UpdateOficioFirmadoDigital(OficioDto datos, ProcesaArchivo procesaArchivo, CreaNuevoDespacho creaNuevoDespacho)
        {
            var resultado = new ResultadoOperacion(1, "Operación realizada con éxito", null);

            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {

                var oficio = db.Oficio
                    .Include(r => r.Requerimiento)
                    .Include(r => r.PlantillaOficio)
                    .FirstOrDefault(d => d.Id == datos.Id);

                if (oficio == null)
                {
                    resultado.Codigo = -1;
                    resultado.Mensaje = "No se encontró el oficio a actualizar.";
                    return resultado;
                }

                oficio.FechaModificacion = datos.FechaModificacion;
                oficio.UsuarioModificacionId = datos.UsuarioModificacionId;
                oficio.EstadoId = (int)EstadoOficio.Firmado;
                // Se graban los datos del oficio
                db.SaveChanges();

                datos.datosArchivo.OrigenId = oficio.Id;
                datos.datosArchivo.OrigenCodigo = oficio.NumeroOficio;
                datos.datosArchivo.FileName = $"Oficio_{oficio.NumeroOficio}.pdf";
                // datos.datosArchivo.FileTextContent = docResult["file"].ToString(); // Documento firmado en codificación Base64

                // (1) SE SUBE EL DOCUMENTO AL REPOSITORIO DE ARCHIVOS EL PDF FIRMADO, generado a partir del contenido del oficio y firmado en el firmador digital
                var resultArch = procesaArchivo(datos.datosArchivo, true, false);

                if (resultArch.Codigo <= 0)
                { // ERROR SUBIENDO EL PDF HACIA EL REPOSITORIO DE ARCHIVOS
                    Gedoc.Helpers.Logging.Logger.LogError($"Error al pasar al estado Firmado el Oficio {oficio.NumeroOficio}, falló la carga del archivo PDF. Id interno generado: {oficio.Id}");
                    resultado.Codigo = 1; // -2 para ignorarlo en el formulario
                    resultado.Mensaje = $"¡Atención! No se ha podido pasar al estadoo Firmado el Oficio {oficio.NumeroOficio}, ha ocurrió un error al subir el PDF del oficio.";
                    transaction.Rollback();
                }
                else
                {
                    var archUrl = (string[])resultArch.Extra ?? new[] { "", datos.datosArchivo.FileName };
                    oficio.UrlArchivo = archUrl[0];
                    oficio.NombreArchivo = archUrl[1];
                    // Se actualiza en el oficio los datos del archivo subido al repositorio
                    db.SaveChanges();

                    // (2) SE GENERA EL DESPACHO O DESPACHO INICIATIVAS EN BASE AL OFICIO FIRMADO:
                    var despacho = new DespachoDto();
                    var despachoInic = new DespachoIniciativaDto();
                    if (oficio.PlantillaOficio.TipoPlantillaId == (int)TipoPlantillaOficio.Despacho)
                    {
                        // Oficio de Despacho. Valores por defecto:
                        despacho.FechaEmisionOficio = oficio.FechaEmisionOficio.Value.Date; // Se quita la hora de la Fecha de Oficio para el despacho. Si se quiere dejar la hora entonces tener en cuenta q al editar el despacho, como el formulario tiene para especificar la Fecha de Emisión, entonces se pierde la hora al grabar
                        despacho.NumeroDespacho = oficio.NumeroOficio;
                        despacho.Requerimiento =
                            oficio.Requerimiento.Select(_mapper.MapFromModelToDto<Requerimiento, GenericoDto>).ToList();
                        datos.datosArchivo.OrigenId = 0;
                        datos.datosArchivo.OrigenCodigo = "";
                        despacho.DatosArchivo = datos.datosArchivo; //new DatosArchivo { FileName = oficio.NombreArchivo };
                        despacho.UrlArchivo = oficio.UrlArchivo;
                        despachoInic = null;
                    }
                    else
                    {
                        // Oficio de Despacho Iniciativas. Valores por defecto:
                        despachoInic.FechaEmisionOficio = oficio.FechaEmisionOficio.Value.Date; // Se quita la hora de la Fecha de Oficio para el despacho inic. Si se quiere dejar la hora entonces tener en cuenta q al editar el despacho inic, como el formulario tiene para especificar la Fecha de Emisión, entonces se pierde la hora al grabar
                        despachoInic.NumeroDespacho = oficio.NumeroOficio;
                        despachoInic.UtAsignadaId = oficio.UnidadTecnicaId;
                        despachoInic.ProfesionalId = oficio.UsuarioCreacionId; // Se asume q el profesional es el q creó el oficio
                        despachoInic.MonumentoNacional = new MonumentoNacionalDto();
                        datos.datosArchivo.OrigenId = 0;
                        datos.datosArchivo.OrigenCodigo = "";
                        despachoInic.DatosArchivo = datos.datosArchivo; //new DatosArchivo { FileName = oficio.NombreArchivo };
                        despacho = null;
                    }
                    var resultNuevoDesp = creaNuevoDespacho(despacho, despachoInic);

                    if (resultNuevoDesp.Codigo > 0)
                    {
                        var textoFirma = "El oficio ha pasado a estado Firmado con éxito";
                        resultado.Mensaje = $"{textoFirma}.<br/>Se ha generado el " +
                                            $"{(oficio.PlantillaOficio.TipoPlantillaId == (int)TipoPlantillaOficio.Despacho ? "Despacho" : "Despacho Iniciativas CMN")} " +
                                            $"con Número de Oficio: {oficio.NumeroOficio}.";
                        transaction.Commit();
                    }
                    else
                    {
                        // ERROR CREANDO EL DESPACHO A PARTIR DEL OFICIO
                        // Se elimina del repositorio de archivos de Gedoc el pdf del oficio q fue subido al repositorio
                        var resultDelArch = procesaArchivo(datos.datosArchivo, true, true);

                        resultado.Codigo = -1;
                        resultado.Mensaje = resultNuevoDesp.Mensaje; // "Ha ocurrido un error al realizar la operación. Por favor, chequee el log de error de la aplicación.";
                        transaction.Rollback();
                    }
                }

                datos.NumeroOficio = oficio.NumeroOficio;
                datos.FechaCreacion = oficio.FechaCreacion;
                if (oficio.Requerimiento != null) // Para utilizarlo en el log de sistema luego:
                    datos.Requerimiento = oficio.Requerimiento.Select(r => new GenericoDto { IdInt = r.Id }).ToList();
            }

            return resultado;
        }

        public ResultadoOperacion UpdateOficioFirmado(OficioDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Operación realizada con éxito", null);

            var oficio = db.Oficio
                .Include(r => r.Requerimiento)
                .FirstOrDefault(d => d.Id == datos.Id);

            if (oficio == null)
            {
                resultado.Codigo = -1;
                resultado.Mensaje = "No se encontró el oficio a actualizar.";
                return resultado;
            }

            oficio.NumeroOficio = datos.NumeroOficio ?? oficio.NumeroOficio;
            oficio.FechaEmisionOficio = datos.FechaEmisionOficio ?? oficio.FechaEmisionOficio;
            oficio.FechaEmisionOficio = datos.FechaEmisionOficio ?? oficio.FechaEmisionOficio;
            oficio.FechaModificacion = datos.FechaModificacion;
            oficio.UsuarioModificacionId = datos.UsuarioModificacionId;
            oficio.EstadoId = datos.EstadoId == -1 ? oficio.EstadoId : datos.EstadoId;
            oficio.EtapaId = datos.EtapaId == -1 ? oficio.EtapaId : datos.EtapaId;
            oficio.FechaUltEstado = datos.FechaUltEstado;
            oficio.FechaUltEtapa = datos.FechaUltEtapa;


            db.SaveChanges();
            datos.FechaCreacion = oficio.FechaCreacion;

            return resultado;
        }

        public bool ExistePlantillaEnOficio(int plantillaId)
        {
            var enUso = db.Oficio.Any(o => o.PlantillaId == plantillaId);
            return enUso;
        }

        public ResultadoOperacion ValidaNumeroOficio(string numeroOficio)
        {
            numeroOficio = numeroOficio ?? "";
            // Se valida si el número de oficio está reservado y además no está asignado en algún despacho o despacho iniciativas
            // El número de oficio debe venir en la forma 00000-YYYY, se elimina el -YYYY para tomar el valor correlativo.
            var corr = numeroOficio.Contains("-") ? numeroOficio.Substring(0, numeroOficio.IndexOf("-")) : "0";
            if (!Int32.TryParse(corr, out var corrInt))
            {
                return new ResultadoOperacion(-1, "El Número de Oficio no se encuentra reservado.", null);
            }

            var annoNumOf = numeroOficio.Contains("-") 
                ? numeroOficio.Substring(numeroOficio.IndexOf("-") + 1, numeroOficio.Length - numeroOficio.IndexOf("-") - 1) 
                : "0";
            Int32.TryParse(annoNumOf, out var annoNumOfInt);
            // Se chequea q el número de oficio esté reservado
            var existe = db.ReservaCorrelativo.Any(r => r.Correlativo == corrInt && r.FechaCreacion.Year == annoNumOfInt);
            if (!existe)
            {
                return new ResultadoOperacion(-1, "El Número de Oficio no se encuentra reservado.", null);
            }

            // Se chequea q el número de oficio no esté asignado a un despacho existente
            existe = db.Despacho.Any(d => d.NumeroDespacho == numeroOficio);
            if (existe)
            {
                return new ResultadoOperacion(-2, "El Número de Oficio ya se encuentra en uso en un Despacho.", null);
            }

            // Se chequea q el número de oficio no esté asignado a un despacho iniciativas existente
            existe = db.DespachoIniciativa.Any(d => d.NumeroDespacho == numeroOficio);
            if (existe)
            {
                return new ResultadoOperacion(-3, "El Número de Oficio ya se encuentra en uso en un Despacho Iniciativas CMN.", null);
            }

            return new ResultadoOperacion(1, "OK", null);
        }
        #endregion

        public List<RequerimientoDto> GetRequerimientosDespacho(int idDespacho)
        {
            var reqs = db.Requerimiento
                .Include(r => r.ProfesionalUt)
                //.Include(r => r.UnidadTecnicaAsign)
                //.Include(r => r.UnidadTecnicaAnterior)
                //.Include(r => r.UnidadTecnicaApoyo)
                //.Include(r => r.UnidadTecnicaTemp)
                //.Include(r => r.UnidadTecnicaTransp)
                //.Include(r => r.EstadoRequerimiento)
                //.Include(r => r.EtapaRequerimiento)
                .Include(r => r.Despacho)
                .Where(r => r.Despacho.Any(d => d.Id == idDespacho));
            var datos = reqs.AsEnumerable().Select(_mapper.MapFromModelToDto<Requerimiento, RequerimientoDto>).ToList();

            return datos;
        }


    }
}
