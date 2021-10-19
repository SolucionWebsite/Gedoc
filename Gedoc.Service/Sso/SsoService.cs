using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Logging;

namespace Gedoc.Service.Sso
{
    public class SsoService : ISsoService
    {
        public IdentidadSsoDto ValidateSso(string token, int sistemaId)
        {
            try
            {
                var ws = new WsSso.WSSOSoapClient();
                var identidad = ws.SystemValidate(token, sistemaId);

                var resultadoSso = new IdentidadSsoDto
                {
                    AtributoAD = identidad.AtributoAD,
                    Logeado = identidad.Logeado
                };
                return resultadoSso;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            return new IdentidadSsoDto();
        }
    }
}
