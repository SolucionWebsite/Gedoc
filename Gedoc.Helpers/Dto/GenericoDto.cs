using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class GenericoDto
    {
        private int _id;

        public string Id { get; set; }

        public int IdInt
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Id) && int.TryParse(Id, out var idInt))
                {
                    return idInt;
                }
                return _id;
            }
            set
            {
                _id = value;
                Id = value.ToString();
            }
        }
        public string Titulo { get; set; }
        public string Title { get; set; }
        public bool Activo { get; set; }
        public string ExtraData { get; set; }
        public object ExtraDataObj { get; set; }
        public int? IdLista { get; set; }
    }
}
