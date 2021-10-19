using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;

namespace Gedoc.Helpers.Dto
{
    public class RequerimientoDespachoDto
    {
        public int Id { get; set; }
        public int? CantidadAdjuntos { get; set; }
        public int? CaracterId { get; set; }
        public string DocumentoIngreso { get; set; }
        public int EstadoId { get; set; }
        public string EstadoTitulo { get; set; }
        public int EtapaId { get; set; }
        public string EtapaTitulo { get; set; }
        public System.DateTime FechaIngreso { get; set; }
        public int? ProfesionalId { get; set; }
        public string ProfesionalNombre { get; set; }
        public string RolSii { get; set; }
        public UnidadTecnicaDto UnidadTecnicaAsign { get; set; }
        public int? UtAsignadaId { get; set; }
        public string UtAsignadaTitulo { get; set; }

        #region Campos Despacho
        public int DespachoId { get; set; }
        public string NumeroDespacho { get; set; }
        public DateTime? FechaEmisionOficio { get; set; }
        //public int? RegionId { get; set; }
        //public int? ComunaId { get; set; }
        public string RegionCod { get; set; }
        public string ComunaId { get; set; }
        public string RegionTitulo { get; set; }
        public string ComunaTitulo { get; set; }
        public string MedioDespachoTitulo { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public string MateriaOficioCMN { get; set; }
        public string FechasEmisionOficio { get; set; }
        public string DestinatarioNombre { get; set; }
        public string DestinatarioInstitucion { get; set; }
        public string Etiqueta { get; set; }
        public IEnumerable<string> TiposAdjuntos { get; set; }
        public string TipoAdjuntoTitulos { get; set; }

        #endregion
    }
}
