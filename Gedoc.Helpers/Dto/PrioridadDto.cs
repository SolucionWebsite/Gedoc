using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Helpers.Dto
{
    public class PrioridadDto
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
        public int Plazo { get; set; }
        public bool Activo { get; set; }
    }
}
