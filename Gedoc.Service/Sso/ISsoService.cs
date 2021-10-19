using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;

namespace Gedoc.Service.Sso
{
    public interface ISsoService
    {
        IdentidadSsoDto ValidateSso(string token, int sistemaId);
    }
}
