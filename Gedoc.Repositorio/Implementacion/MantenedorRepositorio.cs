using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Helpers.Logging;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Maps.Interfaces;
using Gedoc.Repositorio.Model;
using Rol = Gedoc.Helpers.Enum.Rol;

namespace Gedoc.Repositorio.Implementacion
{
    public class MantenedorRepositorio : RepositorioBase, IMantenedorRepositorio
    {
        private readonly IMantenedorMap _mapper;

        public MantenedorRepositorio(IMantenedorMap mapper)
        {
            this._mapper = mapper;
        }

        #region GetGenericoMatenedor
        public List<GenericoDto> GetGenericoMantenedor(Mantenedor tipoMantenedor, string extra, string extra2)
        {
            var datos = new List<GenericoDto>();
            var utId = (int)0;
            switch (tipoMantenedor)
            {
                case Mantenedor.CanalLlegadaTramite:
                    datos = GetValoresLista(tipoMantenedor); //(w => w.IdLista == (int)Lista.CanalLlegadaTramite);
                    break;
                case Mantenedor.Caracter:
                    datos = db.Caracter
                        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.Titulo }).OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.Caso:
                    if (extra == "ACTIVOS")
                        datos = db.Caso.Where(a => a.Activo == true).Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.Titulo }).OrderBy(d => d.Titulo).ToList();
                    else
                        datos = db.Caso.Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.Titulo }).OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.CategoriaMn:
                    datos = db.ListaValor
                        .Where(w => w.IdLista == (int)Mantenedor.CategoriaMn && w.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo)
                        .OrderBy(d => d.Titulo)
                        .Select(d => new GenericoDto() { Id = d.Codigo, Titulo = d.Titulo, ExtraData = d.Codigo })
                        .ToList();
                    break;
                case Mantenedor.Comuna:
                    var regArrC = (extra ?? "").Split(',');
                    var regListC = (regArrC ?? new string[0]).ToList().Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
                    var regTodasC = regListC.Count == 0;

                    var provArrC = (extra2.StartsWith("FullTitle") ? "" : (extra2 ?? "")).Split(',');
                    var provListC = (provArrC ?? new string[0]).ToList().Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
                    var provTodasC = provListC.Count == 0;
                    regTodasC = regTodasC || !provTodasC;

                    if (!extra2.StartsWith("FullTitle") && regTodasC)
                    {
                        datos = db.ListaValor
                            .Where(lv => lv.IdLista == (int) Mantenedor.Comuna
                                         && (provTodasC || provListC.Contains(lv.CodigoPadre)))
                            .Select(lv => new GenericoDto()
                            {
                                Id = lv.Codigo,
                                Titulo = lv.Titulo,
                                ExtraData = lv.CodigoPadre
                            })
                            .ToList();
                    }
                    else
                    {
                        datos = (from lvC in db.ListaValor
                                join lisC in db.Lista on lvC.IdLista equals lisC.IdLista
                                join lvP in db.ListaValor on new { lisC.IdListaPadre, lvC.CodigoPadre } equals new { IdListaPadre = (int?)lvP.IdLista, CodigoPadre = lvP.Codigo }
                                join lisP in db.Lista on lvP.IdLista equals lisP.IdLista
                                join lvR in db.ListaValor on new { lisP.IdListaPadre, lvP.CodigoPadre } equals new { IdListaPadre = (int?)lvR.IdLista, CodigoPadre = lvR.Codigo }
                                where (lisC.IdLista == (int)Mantenedor.Comuna &&
                                       (regTodasC || regListC.Contains(lvR.Codigo))
                                       && (provTodasC || provListC.Contains(lvP.Codigo)))
                                select new GenericoDto()
                                {
                                    Id = lvC.Codigo,
                                    Titulo = extra2 == "FullTitle"
                                        ? lvC.Titulo + " - " + lvP.Titulo + " - " + lvR.Titulo
                                        : (extra2 == "FullTitle2"
                                            ? lvR.Titulo + " - " + lvC.Titulo
                                            : lvC.Titulo),
                                    ExtraData = lvP.Codigo
                                })
                            .ToList();
                    }
                    break;
                case Mantenedor.Provincia:
                    var regArr = (extra ?? "").Split(',');
                    var regList = (regArr ?? new string[0]).ToList().Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
                    var regTodas = regList.Count == 0;
                    datos = db.ListaValor
                        .Where(p => p.IdLista == (int)Mantenedor.Provincia &&
                                    (regTodas || regList.Contains(p.CodigoPadre)) )
                        .Select(d => new GenericoDto() { Id = d.Codigo, Titulo = d.Titulo, ExtraData = d.CodigoPadre })
                        .OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.Region:
                    datos = db.ListaValor
                        .Where(lv => lv.IdLista == (int)Mantenedor.Region)
                        .Select(d => new GenericoDto() { Id = d.Codigo, Titulo = d.Titulo })
                        .OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.EstadoDespacho:
                    datos = db.EstadoDespacho
                        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.Titulo }).OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.EstadoRequerimiento:
                    datos = db.EstadoRequerimiento
                        .Include(e => e.EtapaRequerimiento)
                        .AsEnumerable()
                        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.Titulo, ExtraDataObj = d.EtapaRequerimiento.Select(_mapper.MapFromOrigenToDestino<EtapaRequerimiento, GenericoDto>).ToList()  })
                        .OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.EtapaRequerimiento:
                    datos = db.EtapaRequerimiento
                        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.Titulo }).OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.Etiqueta:
                    datos = GetValoresLista(Mantenedor.Etiqueta);
                    break;
                case Mantenedor.FormaLlegada:
                    datos = GetValoresLista(Mantenedor.FormaLlegada);
                    break;
                case Mantenedor.MedioVerificacion:
                    datos = GetValoresLista(tipoMantenedor);
                    break;
                case Mantenedor.MotivoCierre:
                    var incluirCerrados = extra == "1";
                    datos = db.MotivoCierre
                        .Where(d => incluirCerrados || d.Activo)
                        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.Titulo }).OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.Prioridad:
                    datos = GetValoresLista(tipoMantenedor);
                    break;
                case Mantenedor.Soporte:
                    datos = GetValoresLista(tipoMantenedor);
                    break;
                case Mantenedor.TipoDocumento:
                    datos = GetValoresLista(tipoMantenedor);
                    break;
                case Mantenedor.TipoTramite:
                    datos = db.TipoTramite
                        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.Titulo }).OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.UnidadTecnica:
                    if (!string.IsNullOrEmpty(extra))
                    {
                        int uid = Convert.ToInt32(extra);
                        datos = db.UnidadTecnica.Where(x => x.UsuariosUt.Any(a => a.Id == uid))
                            .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.Titulo }).OrderBy(d => d.Titulo).ToList();
                    }
                    else
                        datos = db.UnidadTecnica
                            .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.Titulo }).OrderBy(d => d.Titulo).ToList();

                    break;
                case Mantenedor.RequerimientoAnterior:
                    datos = db.Requerimiento
                        .Where(r => r.EstadoId == (int)EstadoIngreso.Cerrado)
                        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.DocumentoIngreso })
                        .OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.Requerimiento:
                    datos = db.Requerimiento
                        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.DocumentoIngreso })
                        .OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.SolicitanteUrg:
                    datos = db.Usuario
                        .Where(u => u.SolicitanteUrgencia) 
                        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.NombresApellidos })
                        .OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.UsuarioActivo:
                    datos = db.Usuario
                        .Where(u => u.Activo) // TODO
                        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.NombresApellidos })
                        .OrderBy(d => d.Titulo).ToList();
                    break;
                case Mantenedor.Profesional:
                    utId = (int)0;
                    Int32.TryParse(extra ?? "0", out utId);
                    // Se selecionan los usuarios integrantes de la UT especificada en el parametro "extra"
                    // ¿y q sean del perfil Profesionales UT? -> deberían ser los del perfil Profesional UT pero en Gedoc Sharepoint no filtran por el perfil
                    datos = db.Usuario
                        //.Include(u => u.UnidadTecnicaIntegrante)
                        .Where(u => u.Activo && 
                                    (utId == 0 || u.UnidadTecnicaIntegrante.Any(ut => ut.Id == utId)) /*&&
                                    u.Rol.Any(r => r.Id == (int)Rol.ProfesionalUt)*/)
                        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.NombresApellidos })
                        .OrderBy(d => d.Titulo)
                        .ToList();
                    break;
                //case Mantenedor.Profesional2:
                //    utId = (int)0;
                //    Int32.TryParse(extra ?? "0", out utId);
                //    // TODO: seleccionar los profesionales de la UT especificada en el parametro "extra"
                //    datos = db.Usuario
                //        .Include(u => u.UnidadTecnica2)
                //        .Where(u => u.Activo && (utId == 0 || u.UnidadTecnica2.Any(ut => ut.Id == utId)))
                //        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.NombresApellidos })
                //        .OrderBy(d => d.Titulo).ToList();
                //    break;
                case Mantenedor.TipoBitacora:
                    extra = extra ?? "";
                    datos = db.ListaValor
                         .Where(b => (extra == "" || b.Titulo.StartsWith(extra)) 
                                     && b.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo 
                                     && b.IdLista == (int)Mantenedor.TipoBitacora)
                         .OrderBy(d => d.Orden)
                         .Select(d => new GenericoDto() { Id = d.Codigo, Titulo = d.Titulo })
                         .ToList();
                    break;
                case Mantenedor.PlantillaOficio:
                    Int32.TryParse(extra ?? "0", out var idTipoTr);
                    datos = db.PlantillaOficio
                        .Where(b => (idTipoTr == 0 || b.TipoTramiteId == idTipoTr) && b.Activo && !b.Eliminado)
                        .OrderBy(d => d.Nombre)
                        .Select(d => new GenericoDto() { IdInt = d.Id, Titulo = d.Nombre, ExtraData = d.TipoPlantillaId.ToString() })
                        .ToList();
                    break;
                case Mantenedor.TipoPlantillaOficio:
                    datos = new List<GenericoDto> {
                            new GenericoDto() {IdInt = (int)TipoPlantillaOficio.Despacho, Titulo = "Plantilla de Despacho"},
                            new GenericoDto() {IdInt = (int)TipoPlantillaOficio.DespachoIniciativa, Titulo = "Plantilla de Despacho Iniciativa"}
                    };
                    break;
                case Mantenedor.Lista:
                    Int32.TryParse(extra ?? "0", out var listaId);
                    datos = db.Lista
                        .Where(b => b.IdLista != listaId && b.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo)
                        .OrderBy(d => d.Nombre)
                        .Select(d => new GenericoDto() { IdInt = d.IdLista, Titulo = d.Nombre })
                        .ToList();
                    break;
                case Mantenedor.EstadoRegistro:
                    datos = db.EstadoRegistro
                        .OrderBy(d => d.Nombre)
                        .Select(d => new GenericoDto() { IdInt = d.IdEstadoRegistro, Titulo = d.Nombre })
                        .ToList();
                    break;
            }

            return datos;
        }

        public List<GenericoDto> GetValoresLista(Mantenedor lista) // (Func<ListaValor, bool> condicion)
        {
            var datos = db.ListaValor
                .Where(w => w.IdLista == (int)lista && w.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo)
                .OrderBy(d => d.Titulo)
                .Select(d => new GenericoDto() { Id = d.Codigo, Titulo = d.Titulo })
                .ToList();

            return datos;
        }
        #endregion

        public List<GenericoDto> GetTipoDocumentoAll()
        {
            var datos = db.ListaValor
                .Where(l => l.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo && l.IdLista == (int)Mantenedor.TipoDocumento)
                .OrderBy(tp => tp.Titulo).ToList();
            return datos.Select(_mapper.MapFromOrigenToDestino<ListaValor, GenericoDto>).ToList();
        }

        #region Unidad Técnica

        public UsuarioDto GetResponsableUt(int utId)
        {
            var ut = db.UnidadTecnica
                .Include(u => u.Responsable)
                .FirstOrDefault(u => u.Id == utId);
            return _mapper.MapFromOrigenToDestino<Usuario, UsuarioDto>(ut?.Responsable);
        }

        public DatosAjax<List<UsuarioDto>> GetUsuariosUt(int utId)
        {
            var usuarios = new List<UsuarioDto>();
            var ut = db.UnidadTecnica
                .Include(d => d.UsuariosUt)
                .FirstOrDefault(u => u.Id == utId);

            if (ut != null)
            {
                usuarios = ut.UsuariosUt
                    .OrderBy(u => u.NombresApellidos)
                    .Select(d => _mapper.MapFromOrigenToDestino<Usuario, UsuarioDto>(d))
                    .ToList();
            }

            var resultado = new DatosAjax<List<UsuarioDto>>(usuarios, new ResultadoOperacion(1, "OK", null))
            {
                Total = usuarios.Count()
            };
            return resultado;
        }

        public List<UnidadTecnicaDto> GetUnidadTecnicaAll()
        {
            var datos = db.UnidadTecnica
                .Include(u => u.Responsable)
                .Include(u => u.Subrogante)
                .OrderBy(tp => tp.Titulo)
                .ToList();
            return datos.Select(d => _mapper.MapFromOrigenToDestino<UnidadTecnica, UnidadTecnicaDto>(d)).ToList();
        }

        public UnidadTecnicaDto GetUnidadTecnicaById(int id)
        {
            var dato = db.UnidadTecnica.FirstOrDefault(b => b.Id == id);
            return _mapper.MapFromOrigenToDestino<UnidadTecnica, UnidadTecnicaDto>(dato);
        }

        public UnidadTecnicaDto GetUnidadTecnicaByUtTramiteId(int id)
        {
            var dato = db.UnidadTecnica.FirstOrDefault(b => b.IdUtTramites == id);
            return _mapper.MapFromOrigenToDestino<UnidadTecnica, UnidadTecnicaDto>(dato);
        }

        public List<UnidadTecnicaDto> GetUnidadTecnicaByUsuario(int usuarioId, bool esEncargado)
        {
            if (esEncargado)
            { // Si es encargado se devuelven las UTs de las q el usuario es respnsable ut
                var datoUt = db.UnidadTecnica.Where(b => b.ResponsableId == usuarioId);
                return datoUt.Select(_mapper.MapFromOrigenToDestino<UnidadTecnica, UnidadTecnicaDto>).ToList();
            }
            // Si no es Encargado Ut se devuelven las UTs a las q pertenezca el usuario
            var dato = db.UnidadTecnica.Where(b => b.UsuariosUt.Any(u => u.Id == usuarioId));
            return dato.Select(_mapper.MapFromOrigenToDestino<UnidadTecnica, UnidadTecnicaDto>).ToList();
        }

        public ResultadoOperacion SaveUnidadTecnica(UnidadTecnicaDto ut, bool updateSoloActivo)
        {
            if (ut.Id == 0)
            {
                var model = _mapper.MapFromOrigenToDestino<UnidadTecnicaDto, UnidadTecnica>(ut);
                model.Responsable = null;
                model.Subrogante = null;
                db.UnidadTecnica.Add(model);
                db.SaveChanges();
            }
            else
            {
                var model = db.UnidadTecnica.FirstOrDefault(c => c.Id == ut.Id);
                if (model != null)
                {
                    if (!updateSoloActivo)
                    {
                        model.Titulo = ut.Titulo;
                        model.ResponsableId = ut.ResponsableId;
                        model.SubroganteId = ut.SubroganteId;
                        model.EmailResponsable = ut.EmailResponsable;
                        model.EmailSecretaria = ut.EmailSecretaria;
                        model.OtrosDestinatariosEmail = ut.OtrosDestinatariosEmail;
                    }
                    model.Activo = ut.Activo;
                    db.SaveChanges();
                }
            }

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);
        }

        public ResultadoOperacion SaveUsuarioUnidadTecnica(int utId, int usuarioId)
        {
            var ut = db.UnidadTecnica
                .Include(u => u.UsuariosUt)
                .FirstOrDefault(c => c.Id == utId);
            if ((ut.UsuariosUt ?? new List<Usuario>()).Any(u => u.Id == usuarioId))
            {
                return new ResultadoOperacion(1, "El usuario ya es integrante de la UT", null);
            } 
            var usuario = db.Usuario.FirstOrDefault(c => c.Id == usuarioId);
            if (ut != null && usuario != null)
            {
                var utUsr = ut.UsuariosUt ?? new List<Usuario>();
                utUsr.Add(usuario);
                ut.UsuariosUt = utUsr;

                db.SaveChanges();
            }

            return new ResultadoOperacion(1, "Se ha incorporado con éxito el usuario a la UT", null);
        }

        public void DeleteUnidadTecnica(int utId)
        {
            var model = db.UnidadTecnica.FirstOrDefault(c => c.Id == utId);
            if (model != null)
            {
                db.UnidadTecnica.Remove(model);
                db.SaveChanges();
            }
        }

        public void DeleteUsuarioUnidadTecnica(int utId, int usuarioId)
        {
            var ut = db.UnidadTecnica
                .Include(u => u.UsuariosUt)
                .FirstOrDefault(c => c.Id == utId);

            if (ut != null)
            {
                var utUsr = ut.UsuariosUt ?? new List<Usuario>();
                var usr = utUsr.FirstOrDefault(u => u.Id == usuarioId);
                if (usr != null)
                {
                    ut.UsuariosUt.Remove(usr);
                    db.SaveChanges();
                }

            }
        }
        #endregion

        public GenericoDto GetSesionTablaById(int id)
        {
            var dato = db.SesionTabla.FirstOrDefault(b => b.Id == id);
            return _mapper.MapFromOrigenToDestino<SesionTabla, GenericoDto>(dato);
        }

        public List<PrioridadDto> GetPrioridadAll()
        {
            var datos = db.ListaValor
                .Where(l => l.IdEstadoRegistro == (int)EstadoRegistroEnum.Activo && l.IdLista == (int)Mantenedor.Prioridad)
                .OrderBy(tp => tp.Titulo).ToList();
            return datos.Select(d => _mapper.MapFromOrigenToDestino<ListaValor, PrioridadDto>(d)).ToList();
        }

        public PrioridadDto GetPrioridadById(string cod)
        {
            var dato = db.ListaValor
                .FirstOrDefault(l => l.Codigo == cod && l.IdLista == (int) Mantenedor.Prioridad);
            return _mapper.MapFromOrigenToDestino<ListaValor, PrioridadDto>(dato);
        }

        public UsuarioDto GetUsuarioById(int id)
        {
            var usuario = db.Usuario.FirstOrDefault(u => u.Id == id);
            return _mapper.MapFromOrigenToDestino<Usuario, UsuarioDto>(usuario);
        }

        public OficioDto GetOficioById(int id)
        {
            var oficio = db.Oficio.Where(x => x.Id == id).FirstOrDefault();
            return _mapper.MapFromOrigenToDestino<Oficio, OficioDto>(oficio);
        }

        public bool IsAdminUser(int id)
        {
            var user = (db.Usuario.Include(u => u.Rol).FirstOrDefault(u => u.Id == id) ?? new Usuario());
            return user.Rol?.Any(r => r.Id == (int)Rol.Administrador) ?? false;
        }

        public UsuarioDto GetUsuarioByUserNamePassword(string userName, string passw)
        {
            var usuario = db.Usuario
                .Include(u => u.UnidadTecnicaIntegrante)
                .Include(u => u.Rol)
                .FirstOrDefault(u =>
                u.Username == userName && u.Password == passw &&
                u.Activo == true);
            return _mapper.MapFromOrigenToDestino<Usuario, UsuarioDto>(usuario);
        }

        public UsuarioDto GetUsuarioByUserName(string userName)
        {
            var usuario = db.Usuario
                .Include(u => u.UnidadTecnicaIntegrante)
                .Include(u => u.Rol)
                .FirstOrDefault(u =>
                    u.Username == userName &&
                    u.Activo == true);
            return _mapper.MapFromOrigenToDestino<Usuario, UsuarioDto>(usuario);
        }

        #region Calendario Bitácoras
        public List<CalendarioBitacoraDto> GetCalendarioBitacoraByTipo(string tipoBitacoraId, int anno)
        {
            var datos = db.CalendarioBitacora
                .Include(d => d.TipoBitacora)
                .Where(p => p.TipoBitacoraCod == tipoBitacoraId && p.Fecha.Year == anno)
                .OrderBy(tp => tp.Fecha)
                .ToList();
            return datos.Select(d => _mapper.MapFromOrigenToDestino<CalendarioBitacora, CalendarioBitacoraDto>(d)).ToList();
        }

        public CalendarioBitacoraDto GetCalendarioBitacoraById(int calendarioId)
        {
            var datos = db.CalendarioBitacora
                .Include(d => d.TipoBitacora)
                .FirstOrDefault(p => p.Id == calendarioId);
            return _mapper.MapFromOrigenToDestino<CalendarioBitacora, CalendarioBitacoraDto>(datos);
        }

        public List<int> GetCalendarioBitacoraAnnos(string tipoBitacoraId)
        {
            var datos = db.CalendarioBitacora
                .Where(p => string.IsNullOrEmpty(tipoBitacoraId) || p.TipoBitacoraCod == tipoBitacoraId)
                .OrderBy(tp => tp.Fecha)
                .Select(c => c.Fecha.Year)
                .Distinct()
                .ToList();
            return datos;
        }

        public ResultadoOperacion SaveCalendarioBitacora(CalendarioBitacoraDto calendario)
        {
            if (calendario.Id == 0)
            {
                var model = _mapper.MapFromOrigenToDestino<CalendarioBitacoraDto, CalendarioBitacora>(calendario);
                model.TipoBitacora = null;
                db.CalendarioBitacora.Add(model);
                db.SaveChanges();
            }
            else
            {
                var model = db.CalendarioBitacora.FirstOrDefault(c => c.Id == calendario.Id);
                if (model != null)
                {
                    model.Fecha = calendario.Fecha.GetValueOrDefault();
                    db.SaveChanges();
                }
            }

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);
        }

        public void DeleteCalendarioBitacora(int calendarioId)
        {
            var model = db.CalendarioBitacora.FirstOrDefault(c => c.Id == calendarioId);
            if (model != null)
            {
                db.CalendarioBitacora.Remove(model);
                db.SaveChanges();
            }
        }
        #endregion

        #region Bitácora

        public GenericoDto GetTipoBitacoraById(string cod)
        {
            var tipoBit = db.ListaValor.FirstOrDefault(t => t.Codigo == cod && t.IdLista == (int)Mantenedor.TipoBitacora);
            return _mapper.MapFromOrigenToDestino<ListaValor, GenericoDto>(tipoBit);
        }
        #endregion

        #region Remitente

        public List<GenericoDto> GetRemitenteResumenAll()
        {
            var datos = db.Remitente.OrderBy(r => r.Nombre);
            return datos.Select(_mapper.MapRemitenteFromModelToGenericoDto).ToList();
        }

        public List<GenericoDto> GetRemitenteResumeByIds(List<int> ids)
        {
            var datos = db.Remitente.Where(r => ids.Contains(r.Id)).OrderBy(r => r.Nombre);
            return datos.Select(_mapper.MapRemitenteFromModelToGenericoDto).ToList();
        }

        public List<RemitenteDto> GetRemitentesByIds(List<int> ids)
        {
            var datos = db.Remitente.Where(r => ids.Contains(r.Id)).OrderBy(r => r.Nombre);
            return datos.Select(_mapper.MapRemitenteFromModelToDto).ToList();
        }

        public DatosAjax<List<GenericoDto>>  GetRemitenteResumenPaging(int skip, int take, string filtro)
        {
            //var datos = db.Remitente.OrderBy(r => r.Nombre);
            //return datos.Select(_mapper.MapRemitenteFromModelToGenericoDto).ToList();

            var query = db.Remitente
                .Where(d => filtro == "" || d.Nombre.Contains(filtro) );

            var datos = db.Remitente
                .OrderBy(r => r.Nombre)
                .Skip(skip)
                .Take(take)
                .AsEnumerable()
                .Select(d => _mapper.MapRemitenteFromModelToGenericoDto(d))
                .ToList();
            var resultado = new DatosAjax<List<GenericoDto>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;

        }

        public RemitenteDto GetRemitenteById(int id)
        {
            var datos = db.Remitente.FirstOrDefault(r => r.Id == id);
            var result = _mapper.MapRemitenteFromModelToDto(datos);
            return result;
        }

        public RemitenteDto GetRemitenteByRut(string rut)
        {
            var datos = db.Remitente.FirstOrDefault(r => r.Rut == rut);
            var result = _mapper.MapRemitenteFromModelToDto(datos);
            return result;
        }

        public RemitenteDto GetRemitenteByNombre(string nombre)
        {
            var datos = db.Remitente.FirstOrDefault(r => r.Nombre == nombre);
            var result = _mapper.MapRemitenteFromModelToDto(datos);
            return result;
        }

        public List<RemitenteDto> GetRemitenteByFilter(string filter)
        {
            filter = filter ?? "";
            var datos = db.Remitente
                .Where(x => x.Activo && (filter == "" || x.Nombre.Contains(filter)) )
                .Select(r => new RemitenteDto()
                {
                    Id = r.Id,
                    Cargo = r.Cargo,
                    Direccion = r.Direccion,
                    Email = r.Email,
                    Rut = r.Rut,
                    Genero = r.Genero,
                    Institucion = r.Institucion,
                    Nombre = r.Nombre,
                    Telefono = r.Telefono,
                    TipoInstitucion = r.TipoInstitucion,
                    UsuarioCreacionNombre = r.UsuarioCreacionId.HasValue ? r.UsuarioCreacion.NombresApellidos : "",
                    FechaCreacion = r.FechaCreacion,
                    UsuarioModificacionNombre = r.UsuarioModificacionId.HasValue ? r.UsuarioModificacion.NombresApellidos : "",
                    FechaModificacion = r.FechaModificacion
                })
                .ToList();

            return datos;
        }

        public ResultadoOperacion SaveRemitente(RemitenteDto datos)
        {
            var model = _mapper.MapRemitenteFromDtoToModel(datos);
            if (datos.Id == 0)
            {
                // Nuevo remitente
                model.Activo = true;
                db.Remitente.Add(model);
                db.SaveChanges();
                datos.Id = model.Id;
            }
            else
            {
                // Modificar remitente
                var r = db.Remitente.Find(model.Id);
                //r.Activo = model.Activo;
                r.Cargo = model.Cargo;
                r.Direccion = model.Direccion;
                r.Email = model.Email;
                r.Genero = model.Genero;
                r.Institucion = model.Institucion;
                r.Nombre = model.Nombre;
                r.Rut = model.Rut;
                r.Telefono = model.Telefono;
                r.TipoInstitucion = model.TipoInstitucion;
                r.UsuarioCreacionId = model.UsuarioCreacionId;
                r.FechaCreacion = model.FechaCreacion;
                r.UsuarioModificacionId = model.UsuarioModificacionId;
                r.FechaModificacion = model.FechaModificacion;
                r.Activo = model.Activo;

                db.SaveChanges();
            }

            return new ResultadoOperacion(1, "Operación realizada con éxito", datos);
        }

        public ResultadoOperacion DesactivaRemitente(int id, UsuarioActualDto usuario)
        {
            var result = db.sp_CambiaEstadoRemitente("D", id, usuario.UsuarioId, usuario.LoginName);

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);
        }

        #endregion

        #region Número Despacho

        public List<string> GetNumeroDespachoResumenAll()
        {
            var datos = db.Despacho.OrderBy(r => r.NumeroDespacho).Select(s=>s.NumeroDespacho).Distinct();
            return datos.ToList();
        }

        public List<string> GetNumeroDespachoResumeByIds(List<string> ids)
        {
            var datos = db.Despacho.Where(r => ids.Contains(r.NumeroDespacho)).OrderBy(r => r.NumeroDespacho).Select(s => s.NumeroDespacho).Distinct();
            return datos.ToList();
        }

        public DatosAjax<List<string>> GetNumeroDespachoResumenPaging(int skip, int take, string filtro)
        {
            //var datos = db.NumeroDespacho.OrderBy(r => r.Nombre);
            //return datos.Select(_mapper.MapNumeroDespachoFromModelToGenericoDto).ToList();

            var query = db.Despacho.OrderBy(r => r.NumeroDespacho).Select(s => s.NumeroDespacho).Distinct()
                .Where(d => filtro == "" || d.Contains(filtro));

            var datos = db.Despacho.OrderBy(r => r.NumeroDespacho).Select(s => s.NumeroDespacho).Distinct()
                .Skip(skip)
                .Take(take)
                .AsEnumerable()
                //.Select(d => _mapper.MapNumeroDespachoFromModelToGenericoDto(d))
                .ToList();
            var resultado = new DatosAjax<List<string>>(datos, new ResultadoOperacion(1, "OK", null))
            {
                Total = query.Count()
            };
            return resultado;

        }
        #endregion

        #region Bandejas de entrada

        public List<AccionBandejaDto> GetDatosAccionesBandeja()
        {
            var datos = db.AccionBandeja.OrderBy(a => a.Orden);
            return datos.Select(_mapper.MapAccionBandejaFromModelToDto).ToList();
        }

        public ConfigBandejaDto GetConfigBandeja(int id, bool resumido, bool byIdBandeja = false, int[] roles = null, int? bandejaMainId = null)
        {
            if (id == (int)Bandeja.Oficio)
            { // Es Bandeja de Oficio
                var configBandeja = db.BandejaEntrada
                    .FirstOrDefault(b => b.IdBandeja == id);
                if (configBandeja == null) return null;

                var accionesBandeja = new List<AccionPermitidaBandejaDto>();
                accionesBandeja = db.AccionesPermitidasBandejaOficio
                    .Include(acc => acc.BandejaEntrada)
                    .Include(acc => acc.AccionBandeja)
                    .GroupBy(acc => new { acc.BandejaId, acc.EstadoId, acc.EtapaId })
                    .Where(acc => acc.Key.BandejaId == bandejaMainId)
                    .AsEnumerable()
                    .Select(acc => new AccionPermitidaBandejaDto()
                    { // TODO: hacer esto por Automapper
                        BandejaId = acc.Key.BandejaId,
                        EstadoId = acc.Key.EstadoId,
                        EtapaId = acc.Key.EtapaId,
                        Acciones = acc.Select(a => a.AccionId).ToList(),
                        AccionesDetalle = acc.Select(a => _mapper.MapFromOrigenToDestino<AccionBandeja, AccionBandejaDto>(a.AccionBandeja)).ToList()
                    })
                    .ToList();

                var configBandejaDto = _mapper.MapConfiBandejaFromModelToDto(configBandeja);
                configBandejaDto.Acciones = accionesBandeja;
                return configBandejaDto;
            }
            // Bandeja de Entrada
            if (resumido)
            {
                var esAdmin = roles != null && roles.Contains((int) Rol.Administrador);
                var accEditarReq = new AccionBandejaDto { Id= 0 };
                if (esAdmin)
                {
                    accEditarReq = _mapper.MapFromOrigenToDestino<AccionBandeja, AccionBandejaDto>(db.AccionBandeja.FirstOrDefault(a => a.IdAccion == "ED") ) ?? accEditarReq;
                }
                var accEliminarReqId = db.AccionBandeja.FirstOrDefault(a => a.IdAccion == "BORRARREQ")?.Id ?? 0;

                var configBandeja = db.BandejaEntrada
                    .FirstOrDefault(b => (byIdBandeja && b.IdBandeja == id) || (!byIdBandeja && b.Id == id));
                if (configBandeja == null) return null;

                var accionesBandeja = new List<AccionPermitidaBandejaDto>();
                // Se dejan solo las acciones permitidas en los roles del usuario
                if (roles != null)
                {
                    accionesBandeja = db.AccionesPermitidasBandejas
                        .Include(acc => acc.BandejaEntrada)
                        .Include(acc => acc.AccionBandeja)
                        //.OrderBy(acc => acc.BandejaId)
                        //.ThenBy(acc => acc.EstadoId)
                        //.ThenBy(acc => acc.EtapaId)
                        //.ThenBy(acc => acc.AccionBandeja.Orden)
                        .GroupBy(acc => new { acc.BandejaId, acc.EstadoId, acc.EtapaId })
                        .Where(acc => acc.Key.BandejaId == configBandeja.Id)
                        .AsEnumerable()
                        .Select(acc => new AccionPermitidaBandejaDto()
                        { // TODO: hacer esto por Automapper
                            BandejaId = acc.Key.BandejaId,
                            EstadoId = acc.Key.EstadoId,
                            EtapaId = acc.Key.EtapaId,
                            Acciones = acc.Select(a => a.AccionId).ToList(),
                            AccionesDetalle = acc.Select(a => _mapper.MapFromOrigenToDestino<AccionBandeja, AccionBandejaDto>(a.AccionBandeja)).ToList()
                        })
                    .ToList();
                    // Se ordenan las acciones
                    //accionesBandeja.ForEach(acc => acc.Acciones = acc.AccionesDetalle.OrderBy(ac => ac.Orden).Select(ac => ac.Id).ToList());

                    var accionesRol = db.AccionBandeja
                        .Include(acc => acc.Rol)
                        .Where(acc => acc.Rol.Any(r => roles.Contains(r.Id)))
                        .Select(acc => acc.Id)
                        .ToList();
                    // El usuario administrador (Propietarios Gestor Documental) siempre debe tener, en todas las bandejas (excepto de Despachos Iniciativas) y
                    // para todos los ingresos de la bandeja, las acciones de Editar Requerimiento. Si la bandeja no tiene estas acciones se agregan y 
                    // si las tiene pero no las tiene el perfil entonces no se eliminan.
                    if (configBandeja.IdBandeja != (int)Bandeja.DespachosIniciativas && esAdmin )
                    { 
                        // Si es administrador siempre tendrá el editar requerimiento aunque en las acciones del perfil no tenga esa acción especifcada
                        if (accionesRol.All(a => a != accEditarReq.Id))
                        {
                            accionesRol.Add(accEditarReq.Id);
                        }
                    }

                    foreach (var accBand in accionesBandeja)
                    {  // Se recorren las acciones de cada Estado-Etapa configuradas para la bandeja y se elinina las acciones a las q el perfil no tiene acceso
                        if (configBandeja.IdBandeja != (int)Bandeja.DespachosIniciativas && esAdmin )
                        { 
                            // Si es administrador siempre tendrá el editar requerimiento aunque la bandeja no tenga esa acción especifcada
                            if (accBand.Acciones.All(a => a != accEditarReq.Id))
                            {
                                accBand.AccionesDetalle.Add(accEditarReq);
                            }
                        }
                        // Se ordenan las acciones
                        accBand.Acciones = accBand.AccionesDetalle.OrderBy(ac => ac.Orden).Select(ac => ac.Id).ToList();

                        // Si el perfil tiene la acción de Eliminar Requerimiento entonces se habilita para todas las bandejas y todos los estados-etapas
                        if (configBandeja.IdBandeja != (int)Bandeja.DespachosIniciativas && accionesRol.Contains(accEliminarReqId) && accBand.Acciones.All(a => a != accEliminarReqId))
                        {
                            accBand.Acciones.Add(accEliminarReqId);
                        }
                        // Se eliminan las acciones a las q el perfil no tiene acceso
                        accBand.Acciones.RemoveAll(accId => !accionesRol.Contains(accId));
                    }
                }

                var configBandejaDto = _mapper.MapConfiBandejaFromModelToDto(configBandeja);
                configBandejaDto.Acciones = accionesBandeja;
                return configBandejaDto;
            }
            else
            {
                var configBandeja = db.BandejaEntrada
                    .FirstOrDefault(b => (byIdBandeja && b.IdBandeja == id) || (!byIdBandeja && b.Id == id));
                return _mapper.MapConfiBandejaFromModelToDto(configBandeja);
            }
        }

        public List<GenericoDto> GetDatosBandejaResumen(int idBandeja)
        {
            var datos = db.vw_BandejaEntrada
                .Where(d => d.BandejaId == idBandeja)
                .Select(s => new GenericoDto
                {
                    IdInt = s.Id,
                    Title = s.DocumentoIngreso
                }).Distinct().OrderBy(o => o.Title).ToList(); 
            return datos;
        }

        public List<GenericoDto> GetDatosBandejaResumenUt(int idBandeja)
        {
            var datos = db.vw_BandejaEntrada
                .Where(d => d.BandejaId == idBandeja && d.UtAsignadaId.HasValue)
                .Select(s => new GenericoDto
                {
                    IdInt = s.UtAsignadaId.Value,
                    Title = s.UtAsignadaTitulo
                }).Distinct().OrderBy(o => o.Title).ToList();
            return datos;
        }

        public List<GenericoDto> GetDatosBandejaResumenEstado(int idBandeja)
        {
            var datos = db.vw_BandejaEntrada
                .Where(d => d.BandejaId == idBandeja)
                .Select(s => new GenericoDto
                {
                    IdInt = s.EstadoId,
                    Title = s.EstadoTitulo
                }).Distinct().OrderBy(o => o.IdInt).ToList();
            return datos;
        }

        #endregion

        #region Log Sistema
        public ResultadoOperacion SaveLogSistema(LogSistemaDto log)
        {
            try
            {
                var model = _mapper.MapFromOrigenToDestino<LogSistemaDto, LogSistema>(log);
                db.LogSistema.Add(model);
                db.SaveChanges();
                log.Id = model.Id;

                return new ResultadoOperacion(1, "Operación realizada con éxito", null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new ResultadoOperacion(-1, "Error", null);
            }
        }

        public ResultadoOperacion SaveLogSistemaMulti(List<LogSistemaDto> logList)
        {
            foreach (var log in logList)
            {
                var model = _mapper.MapFromOrigenToDestino<LogSistemaDto, LogSistema>(log);
                db.LogSistema.Add(model);
                log.Id = model.Id;
            }
            db.SaveChanges();

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);
        }

        public LogWssIntegracionDto GetLastLogWssBySolicitudId(int idSolicitud, string operacion = "", string resultado = "OK")
        {
            resultado = resultado ?? "";
            var log = db.LogWssIntegracion
                .OrderByDescending(l => l.Id)
                .FirstOrDefault(l => l.SolicitudTramId == idSolicitud
                                     && l.Operacion == operacion
                                     && (resultado == "" || l.Resultado == resultado));
            return _mapper.MapFromOrigenToDestino<LogWssIntegracion, LogWssIntegracionDto>(log);
        }

        public ResultadoOperacion SaveLogWss(LogWssIntegracionDto log)
        {
            try
            {
                var model = _mapper.MapFromOrigenToDestino<LogWssIntegracionDto, LogWssIntegracion>(log);
                db.LogWssIntegracion.Add(model);
                db.SaveChanges();
                log.Id = model.Id;

                return new ResultadoOperacion(1, "Operación realizada con éxito", null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return new ResultadoOperacion(-1, "Error", null);
            }
        }
        #endregion

        #region Región, Provincia, Comuna
        public List<GenericoDto> GetRegionesByCodigos(List<string> codigos)
        {
            var datos = db.ListaValor
                .Where(r => r.IdLista == (int)Mantenedor.Region &&
                            (codigos.Contains(r.Codigo) || codigos.Contains("0" + r.Codigo)) )
                .ToList();
            return _mapper.MapFromOrigenToDestino<List<ListaValor>, List<GenericoDto>>(datos);
        }

        public List<GenericoDto> GetProvinciasByCodigos(List<string> codigos)
        {
            var datos = db.ListaValor
                .Where(r => r.IdLista == (int)Mantenedor.Provincia &&
                            (codigos.Contains(r.Codigo) || codigos.Contains("0" + r.Codigo)) )
                .ToList();
            return _mapper.MapFromOrigenToDestino<List<ListaValor>, List<GenericoDto>>(datos);
        }
        public List<GenericoDto> GetComunasByCodigos(List<string> codigos)
        {
            var datos = db.ListaValor
                .Where(r => r.IdLista == (int)Mantenedor.Comuna &&
                            (codigos.Contains(r.Codigo) || codigos.Contains("0" + r.Codigo)) )
                .ToList();
            return _mapper.MapFromOrigenToDestino<List<ListaValor>, List<GenericoDto>>(datos);
        }
        #endregion

        #region Cálculo de días hábiles

        public DateTime GetFechaDiasHabiles(DateTime fecha, int plazo)
        {
            fecha = fecha.Date;
            return GetFechaByDiasHabilesBd(fecha, plazo);
        }

        private DateTime GetFechaByDiasHabilesBd(DateTime fechaDesde, int cantDiasHab)
        {
            DateTime fechaHabil;
            try
            {

                fechaHabil = db.sp_FechaDiasHabiles(fechaDesde, cantDiasHab).FirstOrDefault() ?? fechaDesde;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                throw new Exception("No fue posible obtener los días hábiles, por favor, vuelva a intentar la operación.");
                //fechaHabil = GetFechaByDiasHabilesWss(fechaDesde, cantDiasHab);
                //fechaHabil = fechaDesde.Date.AddDays(cantDiasHab);
            }

            return fechaHabil;
        }
        #endregion

        #region Notificaciones Email

        public NotificacionEmailDto GetNotificacionByCodigo(string codigo)
        {
            var notif = db.NotificacionEmail.FirstOrDefault(n => n.Codigo == codigo);
            return _mapper.MapFromOrigenToDestino<NotificacionEmail, NotificacionEmailDto>(notif);
        }

        public NotificacionEmailDto GetNotificacionById(int id)
        {
            var notif = db.NotificacionEmail.FirstOrDefault(n => n.Id == id);
            return _mapper.MapFromOrigenToDestino<NotificacionEmail, NotificacionEmailDto>(notif);
        }

        public List<NotificacionEmailDto> GetNotificacionAll()
        {
            var datos = db.NotificacionEmail.OrderBy(tp => tp.Codigo).ToList();
            return datos.Select(d => _mapper.MapFromOrigenToDestino<NotificacionEmail, NotificacionEmailDto>(d)).ToList();
        }

        public ResultadoOperacion SaveNotificacion(NotificacionEmailDto notifDto, bool updateSoloActivo)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);
            if (notifDto.Id == 0)
            { // Nuevo

                var notificacion = _mapper.MapFromOrigenToDestino<NotificacionEmailDto, NotificacionEmail>(notifDto);
                db.NotificacionEmail.Add(notificacion);
                db.SaveChanges();
                notifDto.Id = notificacion.Id;
            }
            else
            { // Update
                var notificacion = db.NotificacionEmail.FirstOrDefault(n => n.Id == notifDto.Id);
                if (notificacion == null)
                {
                    return new ResultadoOperacion(-1, "No se encontró el Id de notificación a actualizar", null);
                }

                if (!updateSoloActivo)
                {
                    notificacion.Mensaje = notifDto.Mensaje ?? notificacion.Mensaje;
                    notificacion.Asunto = notifDto.Asunto ?? notificacion.Asunto;
                    notificacion.Descripcion = notifDto.Descripcion ?? notificacion.Descripcion;
                    notificacion.Periodicidad = notifDto.Periodicidad ?? notificacion.Periodicidad;
                    notificacion.Codigo = notifDto.Codigo ?? notificacion.Codigo;
                }
                notificacion.Activo = notifDto.Activo;
                db.SaveChanges();
            }

            return resultado;
        }
        #endregion

        #region Plantilla Oficios
        public List<PlantillaOficioDto> GetPlantillaOficioAll(bool resumen = true)
        {
            if (resumen)
            {
                var datosR = db.PlantillaOficio
                    .Where(d => !d.Eliminado)
                    .Select(d => new PlantillaOficioDto
                    {
                        Id = d.Id,
                        Nombre = d.Nombre,
                        Activo = d.Activo,
                        Descripcion = d.Descripcion,
                        TipoTramiteId = d.TipoTramiteId
                    })
                    .OrderBy(tp => tp.Nombre)
                    .ToList();
                return datosR;
            } 
            var datos = db.PlantillaOficio
                    .Where(d => !d.Eliminado)
                    .OrderBy(tp => tp.Nombre)
                    .ToList();
            return datos.Select(d => _mapper.MapFromOrigenToDestino<PlantillaOficio, PlantillaOficioDto>(d)).ToList();
        }

        public PlantillaOficioDto GetPlantillaOficioById(int id)
        {
            var dato = db.PlantillaOficio.FirstOrDefault(n => n.Id == id);
            return _mapper.MapFromOrigenToDestino<PlantillaOficio, PlantillaOficioDto>(dato);
        }

        public ResultadoOperacion NewPlantillaOficio(PlantillaOficioDto datos)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);

            var plantilla = _mapper.MapFromOrigenToDestino<PlantillaOficioDto, PlantillaOficio>(datos);
            plantilla.TipoTramite = null;
            plantilla.FechaCreacion = datos.FechaCreacion;
            plantilla.FechaModificacion = datos.FechaModificacion;
            plantilla.UsuarioCreacionId = datos.UsuarioCreacionId.GetValueOrDefault(0);
            plantilla.UsuarioModificacionId = datos.UsuarioModificacionId.GetValueOrDefault(0);
            db.PlantillaOficio.Add(plantilla);
            db.SaveChanges();
            datos.Id = plantilla.Id;

            return resultado;
        }

        public ResultadoOperacion UpdatePlantillaOficio(PlantillaOficioDto datos, bool updateSoloActivo)
        {
            var resultado = new ResultadoOperacion(1, "Datos grabados con éxito", null);
            var plantilla = db.PlantillaOficio.FirstOrDefault(n => n.Id == datos.Id);
            if (plantilla == null)
            {
                return new ResultadoOperacion(-1, "No se encontró el Id de plantilla a actualizar", null);
            }

            plantilla.TipoTramite = null;
            plantilla.FechaModificacion = datos.FechaModificacion;
            plantilla.UsuarioModificacionId = datos.UsuarioModificacionId.GetValueOrDefault(0);
            if (!updateSoloActivo)
            {
                plantilla.Nombre = datos.Nombre;
                plantilla.Activo = datos.Activo;
                plantilla.Descripcion = datos.Descripcion;
                plantilla.Contenido = datos.Contenido;
                plantilla.TipoTramiteId = datos.TipoTramiteId;
            }
            plantilla.Activo = datos.Activo;
            db.SaveChanges();

            return resultado;
        }

        public List<CampoSeleccionableDto> GetCamposSeleccionablePorGrupos(List<string> tiposCampo)
        {
            var filtroTipoCampo = (tiposCampo?.Count ?? 0) > 0;
            var datos = db.CampoPlantilla.Where(d => d.Activo && (!filtroTipoCampo || tiposCampo.Any(tc => tc == d.Origen)) );
            var datosDto = datos.AsEnumerable().Select(_mapper.MapFromOrigenToDestino<CampoPlantilla, CampoSeleccionableDto>).ToList();
            var grupos = datosDto.Where(d => d.PadreId == null || d.PadreId == 0)
                .OrderBy(g => g.Orden)
                .ToList();
            grupos.ForEach(g =>
            {
                g.Variable = "";
                g.Hijos = datosDto.Where(d => d.PadreId == g.Id).OrderBy(o => o.Orden).ToList();
            });
            return grupos;
        }

        public List<CampoSeleccionableDto> GetCamposSeleccionable()
        {
            var datos = db.CampoPlantilla.Where(d => d.Activo);
            var datosDto = datos.AsEnumerable()
                .Select(_mapper.MapFromOrigenToDestino<CampoPlantilla, CampoSeleccionableDto>)
                .ToList();
            return datosDto;
        }

        public List<CampoSeleccionableDto> GetCamposSeleccionableByPadre(int? padreId, List<string> tiposCampo)
        {
            var filtroTipoCampo = (tiposCampo?.Count ?? 0) > 0;
            var datos = db.CampoPlantilla.Where(d => d.Activo && d.PadreId == padreId
                                                     && (!filtroTipoCampo || tiposCampo.Any(tc => tc == d.Origen)))
                .OrderBy(c => c.Orden);
            var datosDto = datos.AsEnumerable()
                .Select(_mapper.MapFromOrigenToDestino<CampoPlantilla, CampoSeleccionableDto>)
                .ToList();
            return datosDto;
        }

        public void MarcaDeletePlantilla(int plantillaId, int usuarioId)
        {
            var model = db.PlantillaOficio.FirstOrDefault(c => c.Id == plantillaId);
            if (model != null)
            {
                model.Eliminado = true;
                model.EliminacionFecha = DateTime.Now;
                model.UsuarioEliminacionId = usuarioId;
                db.SaveChanges();
            }
        }

        public void DeletePlantilla(int id)
        {
            var model = db.PlantillaOficio.FirstOrDefault(c => c.Id == id);
            if (model != null)
            {
                db.PlantillaOficio.Remove(model);
                db.SaveChanges();
            }
        }
        #endregion

        #region Reserva de correlativo

        public List<ReservaCorrelativoDto> GetReservaCorrelativoAll()
        {
            var reservas = db.ReservaCorrelativo
                .Include(r => r.UsuarioCreacion)
                .OrderByDescending(r => r.FechaCreacion);
            return reservas.Select(_mapper.MapFromOrigenToDestino<ReservaCorrelativo, ReservaCorrelativoDto>).ToList();

        }

        public void ReservarCorrelativoOficio(ReservaCorrelativoDto datos)
        {
            // Se reserva un correlativo para el año actual
            var anno = DateTime.Today.Year;

            var corr = db.Correlativo.FirstOrDefault(c => c.Anno == anno);
            if (corr == null)
            {
                throw new Exception("No se encontró en base de datos el correlativo para el año " + anno);
            }

            var corrNuevo = corr.CorrelativoDespacho++;

            // Se graba nuevo registro de la reserva del correlativo
            var reserva = _mapper.MapFromOrigenToDestino<ReservaCorrelativoDto, ReservaCorrelativo>(datos);
            reserva.Correlativo = corrNuevo;
            reserva.UsuarioCreacion = null;
            db.ReservaCorrelativo.Add(reserva);
            db.SaveChanges();

            datos.Id = reserva.Id;
            datos.Correlativo = reserva.Correlativo;
        }
        #endregion

        #region Tipo de Trámite
        public List<GenericoDto> GetTipoTramiteGenericoAll()
        {
            var datos = db.TipoTramite.Where(t => t.Activo).OrderBy(tp => tp.Titulo).ToList();
            return datos.Select(_mapper.MapTipoTramiteFromModelToDto).ToList();
        }

        public List<TipoTramiteDto> GetTipoTramiteAll(bool incluirInactivos)
        {
            var datos = db.TipoTramite
                .Include(t => t.EstadoRequerimiento)
                .Include(t => t.EtapaRequerimiento)
                .Include(t => t.UnidadTecnica)
                .Include(t => t.Prioridad)
                .Where(t => incluirInactivos || t.Activo)
                .OrderBy(tp => tp.Titulo).ToList();
            return datos.Select(_mapper.MapFromOrigenToDestino<TipoTramite, TipoTramiteDto>).ToList();
        }

        public TipoTramiteDto GetTipoTramiteByCodigo(string codigo)
        {
            var datos = db.TipoTramite.Include(t => t.UnidadTecnica).FirstOrDefault(t => t.Codigo == codigo);
            return _mapper.MapFromOrigenToDestino<TipoTramite, TipoTramiteDto>(datos);
        }

        public TipoTramiteDto GetTipoTramiteById(int id)
        {
            var dato = db.TipoTramite.FirstOrDefault(b => b.Id == id);
            return _mapper.MapFromOrigenToDestino<TipoTramite, TipoTramiteDto>(dato);
        }

        public ResultadoOperacion SaveTipoTramite(TipoTramiteDto tipoTr, bool updateSoloActivo)
        {
            if (tipoTr.Id == 0)
            {
                var model = _mapper.MapFromOrigenToDestino<TipoTramiteDto, TipoTramite>(tipoTr);
                model.EstadoRequerimiento = null;
                model.EtapaRequerimiento = null;
                model.UnidadTecnica = null;
                model.Prioridad = null;
                db.TipoTramite.Add(model);
                db.SaveChanges();
            }
            else
            {
                var model = db.TipoTramite.FirstOrDefault(c => c.Id == tipoTr.Id);
                if (model != null)
                {
                    if (!updateSoloActivo)
                    {
                        model.Titulo = tipoTr.Titulo;
                        model.Codigo = tipoTr.Codigo;
                        model.EstadoId = tipoTr.EstadoId;
                        model.EtapaId = tipoTr.EtapaId;
                        model.UnidadTecnicaId = tipoTr.UnidadTecnicaId;
                        model.PrioridadCod = tipoTr.PrioridadCod;
                    }
                    model.Activo = tipoTr.Activo;
                    db.SaveChanges();
                }
            }

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);
        }

        public void DeleteTipoTramite(int id)
        {
            var model = db.TipoTramite.FirstOrDefault(c => c.Id == id);
            if (model != null)
            {
                db.TipoTramite.Remove(model);
                db.SaveChanges();
            }
        }
        #endregion

        #region Listas

        public List<ListaDto> GetListaAll()
        {
            var datos = db.Lista
                .Include(t => t.ListaPadre)
                .Include(t => t.ListasHijas)
                .Include(d => d.EstadoRegistro)
                .OrderBy(tp => tp.Nombre).ToList();
            return datos.Select(_mapper.MapFromOrigenToDestino<Lista, ListaDto>).ToList();
        }

        public ListaDto GetListaById(int id)
        {
            var dato = db.Lista.FirstOrDefault(b => b.IdLista == id);
            return _mapper.MapFromOrigenToDestino<Lista, ListaDto>(dato);
        }

        public ResultadoOperacion SaveLista(ListaDto lista)
        {
            var resultado = new ResultadoOperacion(1, "Operación realizada con éxito", null);
            if (lista.IdLista == 0)
            {  // Nueva lista
                // Se obtiene el nuevo IdLista
                var maxLista = db.Lista
                    .OrderByDescending(l => l.IdLista)
                    .Select(l => l.IdLista)
                    .First();
                var model = _mapper.MapFromOrigenToDestino<ListaDto, Lista>(lista);
                model.IdLista = ++maxLista;
                model.ListasHijas = null;
                model.ListaValor = null;
                model.ListaPadre = null;
                model.EstadoRegistro = null;
                db.Lista.Add(model);
                db.SaveChanges();
                lista.IdLista = model.IdLista;
            }
            else
            {
                var model = db.Lista.FirstOrDefault(c => c.IdLista == lista.IdLista);
                if (model != null)
                {
                    model.IdEstadoRegistro = lista.IdEstadoRegistro;
                    model.IdListaPadre = lista.IdListaPadre;
                    model.Nombre = lista.Nombre;
                    model.Descripcion = lista.Descripcion;
                    db.SaveChanges();
                    resultado.Codigo = 2;
                } else
                {
                    resultado.Codigo = -1;
                    resultado.Mensaje = "El registro a actualizar no se encuentra en la base de datos";
                }
            }

            return resultado;
        }

        public void DeleteLista(int id)
        {
            var model = db.Lista.FirstOrDefault(c => c.IdLista == id);
            if (model != null)
            {
                var listaValores = db.ListaValor.Where(lv => lv.IdLista == id);
                db.ListaValor.RemoveRange(listaValores);
                db.Lista.Remove(model);
                db.SaveChanges();
            }
        }
        #endregion

        #region Lista Valor

        public List<ListaValorDto> GetListaValorAllByListaId(int listaId)
        {
            var datos = db.ListaValor
                .Include(t => t.Lista)
                .Include(d => d.EstadoRegistro)
                .Where(l => l.IdLista == listaId)
                .OrderBy(tp => tp.Titulo).ToList();
            return datos.Select(_mapper.MapFromOrigenToDestino<ListaValor, ListaValorDto>).ToList();
        }

        public ListaValorDto GetListaValorById(int listaId, string codigo)
        {
            var dato = db.ListaValor
                .Include(d => d.EstadoRegistro)
                .FirstOrDefault(b => b.IdLista == listaId && b.Codigo == codigo);
            return _mapper.MapFromOrigenToDestino<ListaValor, ListaValorDto>(dato);
        }

        public ResultadoOperacion SaveListaValor(ListaValorDto listaValor)
        {
            ListaValor model;
            if (listaValor.EsNuevo)
            {
                model = _mapper.MapFromOrigenToDestino<ListaValorDto, ListaValor>(listaValor);
                model.EstadoRegistro = null;
                db.ListaValor.Add(model);
            }
            else
            {
                model = db.ListaValor.FirstOrDefault(b =>
                    b.IdLista == listaValor.IdLista && b.Codigo == listaValor.Codigo);
                if (model == null)
                {
                    return new ResultadoOperacion(-1, "El valor de la lista a actualizar no se encuentra.", null);
                }
                model.IdEstadoRegistro = listaValor.IdEstadoRegistro;
                model.Titulo = listaValor.Titulo;
                model.CodigoPadre = listaValor.CodigoPadre;
                model.IdEstadoRegistro = listaValor.IdEstadoRegistro;
                model.ValorExtra1 = listaValor.ValorExtra1;
                model.ValorExtra2 = listaValor.ValorExtra2;
                model.Orden = listaValor.Orden;
                db.SaveChanges();
            }
            db.SaveChanges();

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);
        }

        public void DeleteListaValor(int listaId, string codigo)
        {
            var model = db.ListaValor.FirstOrDefault(b => b.IdLista == listaId && b.Codigo == codigo);
            if (model != null)
            {
                db.ListaValor.Remove(model);
                db.SaveChanges();
            }
        }

        #endregion

        #region Caso
        public List<CasoDto> GetCasoAll()
        {
            var datos = db.Caso
                .Include(c => c.Requerimiento)
                .Include(c => c.UsuarioCreacion)
                .Include(c => c.UsuarioModificacion)
                .ToList();
            return datos.Select(c =>
            {
                var dto = _mapper.MapFromOrigenToDestino<Caso, CasoDto>(c);
                dto.CantidadReq = c.Requerimiento.Count;
                return dto;
            }).ToList();
        }

        public CasoDto GetCasoById(int id)
        {
            var datos = db.Caso
                .Include(c => c.Requerimiento)
                .Include(c => c.UsuarioCreacion)
                .Include(c => c.UsuarioModificacion)
                .FirstOrDefault(c => c.Id == id);
            var datosDto = _mapper.MapFromOrigenToDestino<Caso, CasoDto>(datos);
            datosDto.CantidadReq = datos.Requerimiento.Count;
            return datosDto;
        }

        public ResultadoOperacion NewCaso(CasoDto caso)
        {
            var model = _mapper.MapFromOrigenToDestino<CasoDto, Caso>(caso);
            model.UsuarioCreacion = null;
            model.UsuarioModificacion = null;
            db.Caso.Add(model);
            db.SaveChanges();
            var usuario = GetUsuarioById(caso.CreadoPor.GetValueOrDefault());
            caso.UsuarioCreacionNombresApellidos = usuario?.NombresApellidos;
            caso.UsuarioModificacionNombresApellidos = usuario?.NombresApellidos;
            caso.Id = model.Id;
            caso.ModificadoPor = caso.CreadoPor;

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);
        }


        public ResultadoOperacion UpdateCaso(CasoDto caso)
        {
            var model = db.Caso.FirstOrDefault(c => c.Id == caso.Id);
            if (model != null)
            {
                model.Titulo = caso.Titulo;
                model.FechaModificacion = caso.FechaModificacion;
                model.ModificadoPor = caso.ModificadoPor;
                model.IdCaso = caso.IdCaso;
                model.Activo = caso.Activo;
                model.UsuarioCreacion = null;
                model.UsuarioModificacion = null;
                db.SaveChanges();
                var usuario = GetUsuarioById(caso.ModificadoPor.GetValueOrDefault());
                caso.UsuarioModificacionNombresApellidos = usuario?.NombresApellidos;
            }
            else
            {
                return new ResultadoOperacion(-1, "No se encontró el caso a actualizarñ", null);
            }

            return new ResultadoOperacion(1, "Operación realizada con éxito", null);
        }

        public List<RequerimientoDto> GetRequerimientosNoAsignadoCaso(CasoFilterDto filtros)
        {
            var requerimientos = FiltrosRequerimiento(db.Requerimiento, filtros);

            var datos = requerimientos
                .Include(r => r.Remitente)
                .Include(r => r.UnidadTecnicaAsign)
                .Select(req => new RequerimientoDto
                {
                    Id = req.Id,
                    DocumentoIngreso = req.DocumentoIngreso,
                    FechaIngreso = req.FechaIngreso,
                    RemitenteNombre = req.Remitente.Nombre,
                    RemitenteInstitucion = req.Remitente.Institucion,
                    Materia = req.Materia,
                    UtAsignadaTitulo = req.UnidadTecnicaAsign.Titulo,
                    EstadoTitulo = req.EstadoRequerimiento.Titulo,
                    CantOficiosCmn = req.Despacho.Count()
                })
                .ToList();
            return datos;
        }

        public List<RequerimientoDto> GetRequerimientosByCasoId(int casoId, CasoFilterDto filtros)
        {
            var requerimientos = FiltrosRequerimiento(db.Requerimiento.Include(r => r.MonumentoNacional).Where(r => r.CasoId == casoId), filtros);

            var datos = requerimientos
                .Include(r => r.Remitente)
                .Include(r => r.UnidadTecnicaAsign)
                .Include(r => r.Despacho)
                .Select(req => new RequerimientoDto
                {
                    Id = req.Id,
                    DocumentoIngreso = req.DocumentoIngreso,
                    FechaIngreso = req.FechaIngreso,
                    RemitenteNombre = req.Remitente.Nombre,
                    RemitenteInstitucion = req.Remitente.Institucion,
                    Materia = req.Materia,
                    UtAsignadaTitulo = req.UnidadTecnicaAsign.Titulo,
                    EstadoTitulo = req.EstadoRequerimiento.Titulo,
                    CantOficiosCmn = req.Despacho.Count()
                })
                .ToList();
            return datos;
        }

        private IQueryable<Requerimiento> FiltrosRequerimiento(IQueryable<Requerimiento> result, CasoFilterDto filtros)
        {
            if (filtros.FechaDesde != null)
                result = result.Where(r => r.FechaIngreso >= filtros.FechaDesde);

            if (filtros.FechaHasta != null)
                result = result.Where(r => r.FechaIngreso <= filtros.FechaHasta);

            if (filtros.Estado != 0)
                result = result.Where(r => r.EstadoRequerimiento.Id == filtros.Estado);

            if (filtros.Etapa != 0)
                result = result.Where(r => r.EtapaRequerimiento.Id == filtros.Etapa);

            if (filtros.Remitente != null && filtros.Remitente.Length > 0)
                result = result.Where(r => filtros.Remitente.Contains(r.RemitenteId));

            if (!string.IsNullOrEmpty(filtros.Institucion))
                result = result.Where(r => r.Remitente.Institucion.Contains(filtros.Institucion));

            if (filtros.TipoTramite != 0)
                result = result.Where(r => r.TipoTramite.Id == filtros.TipoTramite);

            if (filtros.CategoriaMonumentoNacional?.Count > 0)
            {
                result = result.Where(r => r.MonumentoNacional.CategoriaMonumentoNac.Any(c => filtros.CategoriaMonumentoNacional.Contains(c.Codigo)));
            }

            if (!string.IsNullOrEmpty(filtros.MonumentoNacional))
                result = result.Where(r => r.MonumentoNacional.NombreUsoActual.Contains(filtros.MonumentoNacional));

            if (filtros.UnidadTecnica != null && filtros.UnidadTecnica.Length > 0)
                result = result.Where(r => filtros.UnidadTecnica.Contains(r.UtAsignadaId));

            if (!string.IsNullOrEmpty(filtros.Comuna))
                result = result.Where(r => r.MonumentoNacional.Comuna.Any(c => c.Codigo == filtros.Comuna));

            if (filtros.Etiqueta != 0)
            {
                var etiquetaStr = filtros.Etiqueta.ToString();
                result = result.Where(r => r.Etiqueta.Any(et => et.Codigo == etiquetaStr));
            }

            if (filtros.DocumentoIngreso != null && filtros.DocumentoIngreso.Length > 0)
                result = result.Where(r => filtros.DocumentoIngreso.Contains(r.Id));

            if (!string.IsNullOrEmpty(filtros.Materia))
                result = result.Where(r => r.Materia.Contains(filtros.Materia) || r.NombreProyectoPrograma.Contains(filtros.Materia) || r.ProyectoActividad.Contains(filtros.Materia));

            //return result.Take(1000); //De todas formas agrego un limite de 1000 registros
            return result;
        }

        public void AgregaRequeriminetosCaso(int casoId, List<int> reqs)
        {
            var requerimientos = db.Requerimiento.Where(r => reqs.Contains(r.Id));
            foreach (var requerimiento in requerimientos)
            {
                requerimiento.CasoId = casoId;
            }

            db.SaveChanges();
        }

        public void EliminaRequerimientosCaso(List<int> reqs)
        {
            var requerimientos = db.Requerimiento.Where(r => reqs.Contains(r.Id));
            foreach (var requerimiento in requerimientos)
            {
                requerimiento.CasoId = null;
            }

            db.SaveChanges();
        }

        public ResultadoOperacion EliminaCaso(int casoId)
        {
            var result = new ResultadoOperacion();
            try
            {
                var fordelete = db.Caso.FirstOrDefault(a => a.Id == casoId);
                if (fordelete != null)
                {
                    fordelete.Requerimiento.Clear();
                    fordelete.Requerimiento1.Clear();
                    fordelete.DespachoIniciativa.Clear();
                    db.Caso.Remove(fordelete);
                    db.SaveChanges();

                    result.Codigo = 1;
                    result.Mensaje = "Se eliminó el caso: " + casoId;
                } else
                {
                    result.Codigo = -1;
                    result.Mensaje = "No se encuentró el caso que intenta eliminar.";
                }
            }
            catch (Exception ex)
            {
                result.Codigo = -1;
                result.Mensaje = "Error al intentar eliminar el caso: " + casoId;
                result.Extra = ex.Message;
            }
            return result;
        }
        #endregion

        #region Reportes

        public List<ReporteDto> GetReporteAll()
        {
            var rep = db.Reporte;
            var datos = rep.Select(_mapper.MapFromOrigenToDestino<Reporte, ReporteDto>).ToList();
            return datos;
        }

        public ReporteDto GetReporteById(int id)
        {
            var req = db.Reporte.FirstOrDefault(r => r.Id == id);
            return _mapper.MapFromOrigenToDestino<Reporte, ReporteDto>(req);
        }

        #endregion
    }
}
