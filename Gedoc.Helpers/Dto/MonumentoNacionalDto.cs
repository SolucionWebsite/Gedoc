using System.Collections.Generic;

namespace Gedoc.Helpers.Dto
{
    public class MonumentoNacionalDto
    {
        public int Id { get; set; }
        public string CodigoMonumentoNac { get; set; }
        public string DenominacionOficial { get; set; }
        public string OtrasDenominaciones { get; set; }
        public string NombreUsoActual { get; set; }
        public string DireccionMonumentoNac { get; set; }
        public string ReferenciaLocalidad { get; set; }
        public string RolSii { get; set; }
        public List<GenericoDto> CategoriaMonumentoNac { get; set; }
        public string CategoriaMonumentoNacCod { get; set; }
        //public List<string> RegionId { get; set; }
        public List<string> RegionCod { get; set; }
        public string RegionTitulo { get; set; }
        //public List<int> ProvinciaId { get; set; }
        public List<string> ProvinciaCod { get; set; }
        public string ProvinciaTitulo { get; set; }
        //public List<int> ComunaId { get; set; }
        public List<string> ComunaCod { get; set; }
        public string ComunaTitulo { get; set; }
        public List<GenericoDto> Region { get; set; }
        public List<GenericoDto> Provincia { get; set; }
        public List<GenericoDto> Comuna { get; set; }
    }
}
