using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class TipoTramiteModel
    {

        public int Id { get; set; }
        public string Titulo { get; set; }
        public bool Activo { get; set; }
        public string Codigo { get; set; }
        public string PrioridadCod { get; set; }
        public string PrioridadTitulo { get; set; }
        public int? UnidadTecnicaId { get; set; }
        public int? EstadoId { get; set; }
        public string EstadoRequerimientoTitulo { get; set; }
        public int? EtapaId { get; set; }
        public string EtapaRequerimientoTitulo { get; set; }
        public string UnidadTecnicaTitulo { get; set; }
        public string UsuarioActual { get; set; }
        public int? UsuarioCreacionId { get; set; }
    }
}