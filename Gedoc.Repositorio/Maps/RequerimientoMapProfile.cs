using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Repositorio.Model;

namespace Gedoc.Service.Maps
{
    public class RequerimientoMapProfile: AutoMapper.Profile
    {
        public RequerimientoMapProfile()
        {
            #region Configuración maps de Requerimiento
            CreateMap<sp_IngresosUltimos_Result, RequerimientoDto>();

            CreateMap<sp_DatosBandejaEntrada_Result, RequerimientoDto>();

            CreateMap<vw_BandejaEntrada, RequerimientoDto>();

            CreateMap<vw_EtiquetasReq, RequerimientoDto>();

            CreateMap<vw_RemitentesReq, RequerimientoDto>();

            CreateMap<RequerimientoDto, vw_GeneroReq>()
                .ReverseMap();

            CreateMap<VistaGeneroDto, vw_GeneroReq>()
                .ReverseMap();

            CreateMap<RequerimientoDto, vw_FichaIngreso>()
                .ReverseMap();

            CreateMap<GenericoDto, Requerimiento>()
                .ForMember(dst => dst.DocumentoIngreso,
                    opt => opt.MapFrom(src => src.ExtraData))
                .ReverseMap();

            CreateMap<Requerimiento, GenericoDto>()
                .ForMember(dst => dst.Titulo,
                    opt => opt.MapFrom(src => src.DocumentoIngreso))
                .ReverseMap();

            CreateMap<Requerimiento, RequerimientoDto>()
                .ForMember(dst => dst.EstadoTitulo, 
                    opt => opt.MapFrom(src => src.EstadoRequerimiento.Titulo))
                .ForMember(dst => dst.EtapaTitulo, 
                    opt => opt.MapFrom(src => src.EtapaRequerimiento.Titulo))
                .ForMember(dst => dst.UtAnteriorTitulo,
                    opt => opt.MapFrom(src => src.UnidadTecnicaAnterior.Titulo))
                .ForMember(dst => dst.UtApoyoTitulo,
                    opt => opt.MapFrom(src => src.UnidadTecnicaApoyo.Titulo))
                .ForMember(dst => dst.UtAsignadaTitulo,
                    opt => opt.MapFrom(src => src.UnidadTecnicaAsign.Titulo))
                .ForMember(dst => dst.UtTemporalTitulo,
                    opt => opt.MapFrom(src => src.UnidadTecnicaTemp.Titulo))
                .ForMember(dst => dst.UtConocimientoTitulo,
                    opt => opt.MapFrom(src => src.UnidadTecnicaConoc.Titulo))
                .ForMember(dst => dst.SolicitanteUrgenciaNombre,
                    opt => opt.MapFrom(src => src.SolicitanteUrgencia.NombresApellidos))
                .ForMember(dst => dst.TipoAdjuntoTitulos,
                    opt => opt.MapFrom(src => string.Join("; ", src.TipoAdjunto.Select(ta => ta.Titulo))))
                .ForMember(dst => dst.SoporteTitulos,
                    opt => opt.MapFrom(src => string.Join("; ", src.Soporte.Select(ta => ta.Titulo))))
                .ForMember(dst => dst.UtCopiaTitulos,
                    opt => opt.MapFrom(src => string.Join("; ", src.UnidadTecnicaCopia.Select(ta => ta.Titulo))))
                .ForMember(dst => dst.EtiquetaTitulos,
                    opt => opt.MapFrom(src => string.Join("; ", src.Etiqueta.Select(ta => ta.Titulo))))
                .ForMember(dto => dto.Materia, opt => opt.NullSubstitute(""))
                .ForMember(dst => dst.FechaIngresoFull,
                    opt => opt.MapFrom(src => src.FechaIngreso))
                .ForMember(dst => dst.ProfesionalNombre,
                    opt => opt.MapFrom(src => src.ProfesionalUt.NombresApellidos))
                .ForAllMembers(option => {
                    //explicitly telling automapper to ignore null source members...
                    option.Condition((source, destination, sourceMember, destMember) => sourceMember != null);
                });

            CreateMap<RequerimientoDto, Requerimiento>()
                .AfterMap((o, d) =>
                {
                    // Se asignan ID de Lista (de mantenedores)
                    d.FormaLlegadaListaId = (int)Mantenedor.FormaLlegada;
                    d.PrioridadListaId = (int)Mantenedor.Prioridad;
                    d.TipoDocumentoListaId = (int)Mantenedor.TipoDocumento;
                    d.CanalLlegadaTramiteListaId = (int)Mantenedor.CanalLlegadaTramite;
                    foreach (var etiq in (d.Etiqueta ?? Enumerable.Empty<ListaValor>()))
                    {
                        etiq.IdLista = (int)Mantenedor.Etiqueta;
                    }
                    foreach (var sop in (d.Soporte ?? Enumerable.Empty<ListaValor>()))
                    {
                        sop.IdLista = (int)Mantenedor.Soporte;
                    }
                    foreach (var tadj in (d.TipoAdjunto ?? Enumerable.Empty<ListaValor>()))
                    {
                        tadj.IdLista = (int)Mantenedor.TipoDocumento;
                    }
                })
                .ForAllMembers(option => {
                    //explicitly telling automapper to ignore null source members...
                    option.Condition((source, destination, sourceMember, destMember) => sourceMember != null);
                });

            #endregion
        }
    }
}
