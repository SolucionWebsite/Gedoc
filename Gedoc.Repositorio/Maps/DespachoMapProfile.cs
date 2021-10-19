using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Repositorio.Model;

namespace Gedoc.Repositorio.Maps
{
    public class DespachoMapProfile : AutoMapper.Profile
    {
        public DespachoMapProfile()
        {
            CreateMap<sp_DespachosUltimos_Result, DespachoDto>();

            CreateMap<Despacho, DespachoDto>()
                .ForMember(dst => dst.TipoAdjuntoTitulos,
                    opt => opt.MapFrom(src => string.Join("; ", src.TipoAdjunto.Select(ta => ta.Titulo))))
                .ForMember(dst => dst.ProfesionalNombre,
                    opt => opt.MapFrom(src => src.RequerimientoPrincipal.ProfesionalUt.NombresApellidos))
                .ForMember(dst => dst.UtAsignadaTitulo,
                    opt => opt.MapFrom(src => src.RequerimientoPrincipal.UnidadTecnicaAsign.Titulo))
                //.ForMember(dst => dst.Requerimientos, act => act.Ignore())
                //.ForMember(dst => dst.Requerimientos,
                //    opt => opt.MapFrom(src => src.Requerimiento))
                //.ReverseMap()
                ;

            CreateMap<DespachoDto, Despacho>()
                .AfterMap((o, d) =>
                {
                    // Se asignan ID de Lista (de mantenedores)
                    d.MedioDespachoListaId = (int)Mantenedor.FormaLlegada;
                    d.ProveedorDespachoListaId = (int)Mantenedor.FormaLlegada;
                    d.MedioVerificacionListaId = (int)Mantenedor.MedioVerificacion;
                    foreach (var sop in (d.Soporte ?? Enumerable.Empty<ListaValor>()))
                    {
                        sop.IdLista = (int)Mantenedor.Soporte;
                    }
                    foreach (var tadj in (d.TipoAdjunto ?? Enumerable.Empty<ListaValor>()))
                    {
                        tadj.IdLista = (int)Mantenedor.TipoDocumento;
                    }
                })
                .ForAllMembers(option =>
                {
                    // ignorar source members en null para q queden en null en el destino, sino Automapper crea en el destino un objecto vacio si el origen es null
                    option.Condition((source, destination, sourceMember, destMember) => sourceMember != null);
                });

            CreateMap<DespachoIniciativa, DespachoIniciativaDto>()
                .ForMember(dst => dst.ProfesionalNombre,
                    opt => opt.MapFrom(src => src.ProfesionalUt.NombresApellidos))
                .ForMember(dst => dst.UtAsignadaTitulo,
                    opt => opt.MapFrom(src => src.UnidadTecnica.Titulo))
                .ReverseMap();

            CreateMap<DespachoIniciativaDto, DespachoIniciativa>()
                .AfterMap((o, d) =>
                {
                    // Se asignan ID de Lista (de mantenedores)
                    d.MedioDespachoListaId = (int)Mantenedor.FormaLlegada;
                    d.ProveedorDespachoListaId = (int)Mantenedor.FormaLlegada;
                    d.CanalLlegadaTramiteListaId = (int)Mantenedor.CanalLlegadaTramite;
                    d.MedioVerificacionListaId = (int)Mantenedor.MedioVerificacion;
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

            CreateMap<vw_BandejaEntradaDespachoInic, DespachoIniciativaDto>()
                .ForMember(dst => dst.EstadoDespachoTitulo,
                    opt => opt.MapFrom(src => src.EstadoTitulo))
                .ForMember(dst => dst.RemitenteNombre,
                    opt => opt.MapFrom(src => src.DestinatarioNombre))
                .ReverseMap();
            CreateMap<DespachoDto, vw_DestinatarioDesp>()
                .ReverseMap();

            CreateMap<Oficio, OficioDto>()
                .ForMember(dst => dst.TipoPlantillaId,
                    opt => opt.MapFrom(src => src.PlantillaOficio.TipoPlantillaId))
                .ReverseMap();

            CreateMap<OficioDto, Oficio>()
                .ForAllMembers(option => {
                    // ignorar source members en null para q queden en null en el destino, sino Automapper crea en el destino un objecto vacio si el origen es null
                    option.Condition((source, destination, sourceMember, destMember) => sourceMember != null);
                });

            CreateMap<OficioObservacion, OficioObservacionDto>()
                .ReverseMap();
        }
    }
}
