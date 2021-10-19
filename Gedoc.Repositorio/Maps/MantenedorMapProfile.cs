using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Repositorio.Model;

namespace Gedoc.Service.Maps
{
    public class MantenedorMapProfile : AutoMapper.Profile
    {
        public MantenedorMapProfile()
        {
            #region Configuración maps de mantenedores
            CreateMap<TipoTramite, GenericoDto>()
                .ReverseMap();
            CreateMap<TipoTramite, TipoTramiteDto>()
                .ReverseMap()
                .AfterMap((o, d) =>
                {
                    // Se asignan ID de Lista (de mantenedores)
                    d.PrioridadListaId = (int)Mantenedor.Prioridad;
                });
            //CreateMap<TipoAdjunto, GenericoDto>()
            //    .ReverseMap();
            //CreateMap<TipoDocumento, GenericoDto>()
            //    .ReverseMap();
            //CreateMap<Soporte, GenericoDto>()
            //    .ReverseMap();
            //CreateMap<Etiqueta, GenericoDto>()
            //    .ReverseMap();
            CreateMap<UnidadTecnica, GenericoDto>()
                .ReverseMap();
            //CreateMap<GenericoDto, Region>()
            //    .ForMember(dst => dst.Codigo,
            //        opt => opt.MapFrom(src => src.ExtraData))
            //    .ReverseMap();
            //CreateMap<Provincia, GenericoDto>()
            //    .ReverseMap();
            //CreateMap<Comuna, GenericoDto>()
            //    .ReverseMap();
            //CreateMap<GenericoDto, CategoriaMonumentoNac>()
            //    .ForMember(dst => dst.Codigo,
            //        opt => opt.MapFrom(src => src.ExtraData))
            //    .ReverseMap()
            //    .ForMember(dst => dst.ExtraData,
            //        opt => opt.MapFrom(src => src.Codigo));
            ////CreateMap<CategoriaMonumentoNac, GenericoDto>()
            ////    .ForMember(dst => dst.ExtraData,
            ////        opt => opt.MapFrom(src => src.Codigo))
            ////    .ReverseMap();
            CreateMap<SesionTabla, GenericoDto>()
                .ForMember(dst => dst.Titulo,
                    opt => opt.MapFrom(src => src.Nombre))
                .ReverseMap();
            CreateMap<EstadoRequerimiento, GenericoDto>()
                .ReverseMap();
            CreateMap<EtapaRequerimiento, GenericoDto>()
                .ReverseMap();

            CreateMap<Remitente, GenericoDto>()
                .ForMember(dst => dst.Titulo,
                    opt => opt.MapFrom(src => src.Nombre))
                .ReverseMap();
            CreateMap<Remitente, RemitenteDto>()
                .ReverseMap();
            CreateMap<BandejaEntrada, ConfigBandejaDto>()
                .ReverseMap();
            CreateMap<AccionBandeja, AccionBandejaDto>()
                .ReverseMap();
            CreateMap<AccionesPermitidasBandejas, AccionPermitidaBandejaDto>()
                .ReverseMap();
            CreateMap<ListaValor, PrioridadDto>()
                .ForMember(dst => dst.Plazo,
                    opt => opt.ConvertUsing(new StringIntConverter(), "ValorExtra1"))
                .ReverseMap();
            CreateMap<MonumentoNacional, MonumentoNacionalDto>()
                .ReverseMap()
                .AfterMap((o, d) =>
                {
                    // Se asignan ID de Lista (de mantenedores)
                    foreach (var etiq in (d.CategoriaMonumentoNac ?? Enumerable.Empty<ListaValor>()))
                    {
                        etiq.IdLista = (int)Mantenedor.CategoriaMn;
                    }
                    foreach (var reg in (d.Region ?? Enumerable.Empty<ListaValor>()))
                    {
                        reg.IdLista = (int)Mantenedor.Region;
                    }
                    foreach (var prov in (d.Provincia ?? Enumerable.Empty<ListaValor>()))
                    {
                        prov.IdLista = (int)Mantenedor.Provincia;
                    }
                    foreach (var com in (d.Comuna ?? Enumerable.Empty<ListaValor>()))
                    {
                        com.IdLista = (int)Mantenedor.Comuna;
                    }
                });

            CreateMap<Bitacora, BitacoraDto>()
                .ReverseMap();
            CreateMap<BitacoraDto, Bitacora>()
                .AfterMap((o, d) =>
                {
                    // Se asignan ID de Lista (de mantenedores)
                    d.TipoBitacoraListaId = (int)Mantenedor.TipoBitacora;
                })
                .ForAllMembers(option => {
                    option.Condition((source, destination, sourceMember, destMember) => sourceMember != null);
                });
            CreateMap<CalendarioBitacora, CalendarioBitacoraDto>()
                .ReverseMap()
                .AfterMap((o, d) =>
                {
                    // Se asignan ID de Lista (de mantenedores)
                    d.TipoBitacoraListaId = (int)Mantenedor.TipoBitacora;
                });
            //CreateMap<TipoBitacora, GenericoDto>()
            //    .ReverseMap();

            CreateMap<Adjunto, AdjuntoDto>()
                .ReverseMap();
            CreateMap<AdjuntoDto, Adjunto>()
                .AfterMap((o, d) =>
                {
                    // Se asignan ID de Lista (de mantenedores)
                    foreach (var tadj in (d.TipoAdjunto ?? Enumerable.Empty<ListaValor>()))
                    {
                        tadj.IdLista = (int)Mantenedor.TipoDocumento;
                    }
                })
                .ForAllMembers(option => {
                    option.Condition((source, destination, sourceMember, destMember) => sourceMember != null);
                });

            CreateMap<AdjuntoOficio, AdjuntoDto>()
                .ReverseMap();
            CreateMap<AdjuntoDto, AdjuntoOficio>()
                .AfterMap((o, d) =>
                {
                    // Se asignan ID de Lista (de mantenedores)
                    foreach (var tadj in (d.TipoAdjunto ?? Enumerable.Empty<ListaValor>()))
                    {
                        tadj.IdLista = (int)Mantenedor.TipoDocumento;
                    }
                })
                .ForAllMembers(option => {
                    option.Condition((source, destination, sourceMember, destMember) => sourceMember != null);
                });
            CreateMap<LogSistemaDto, LogSistema>()
                .ReverseMap();
            CreateMap<NotificacionEmailDto, NotificacionEmail>()
                .ReverseMap();
            CreateMap<LogWssIntegracionDto, LogWssIntegracion>()
                .ReverseMap();

            CreateMap<PlantillaOficio, PlantillaOficioDto>()
                .ReverseMap();
            CreateMap<CampoPlantilla, CampoSeleccionableDto>()
                .ReverseMap();
            CreateMap<ReservaCorrelativo, ReservaCorrelativoDto>()
                .ReverseMap();

            CreateMap<Lista, ListaDto>()
                .ReverseMap();
            CreateMap<ListaValor, ListaValorDto>()
                .ReverseMap();
            CreateMap<ListaValor, GenericoDto>()
                .ForMember(dst => dst.Id,
                    opt => opt.MapFrom(src => src.Codigo))
                .ReverseMap()
                .ForMember(dst => dst.Codigo,
                    opt => opt.MapFrom(src => src.Id));

            CreateMap<Caso, CasoDto>()
                .ReverseMap();
            #endregion

            #region Configuración maps de Reporte
            CreateMap<Reporte, ReporteDto>()
                .ReverseMap();
            #endregion

            #region Configuración maps de Vistas
            CreateMap<vw_GeneroReqNivel1, VistaGeneroGrupoDto>()
                .ReverseMap();
            CreateMap<vw_GeneroReqNivel2, VistaGeneroGrupoDto>()
                .ReverseMap();
            #endregion
        }
    }

    public class StringIntConverter : IValueConverter<string, int>
    {
        public int Convert(string source, ResolutionContext context)
        {
            int.TryParse(source, out var intValor);
            return intValor;
        }
    }
}
