using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;

namespace Gedoc.Service.DataAccess.Interfaces
{
    public interface IUsuarioService
    {
        //UsuarioDto GetUsuarioByFuncionarioId(int funcionarioId);
        List<UsuarioDto> GetUsuarios();
        ResultadoOperacion SaveUsuario(UsuarioDto usuario, bool soloUpdateActivo = false);
        ResultadoOperacion UpdateSolicitanteUrgencia(int usuarioId, bool solicitanteUrg);
        ResultadoOperacion DeleteUsuario(int id);

        DatosAjax<List<UsuarioDto>> GetUsuariosTransparencia();
        DatosAjax<List<UsuarioDto>> GetUsuariosUrgencia();
        DatosAjax<List<UsuarioDto>> GetUsuariosJefaturaNoUrgencia();
        DatosAjax<List<UsuarioDto>> GetUsuariosByUt(int idUt);
        DatosAjax<List<UsuarioDto>> GetUsuariosByRol(int idRol);
        bool AccesoUrlUsuario(int usuarioId, string url);
        List<RolDto> GetRolAll();
        ResultadoOperacion SaveRol(RolDto rol);
        ResultadoOperacion DeleteRol(int id);
        ResultadoOperacion UpdateRolesUsuario(int usuarioId, List<RolDto> roles);
        List<RolDto> GetRolesUsuario(int usuarioId, bool incluyeNoTiene);
        List<RolDto> GetRolesMenu(int menuId);
        List<AccionBandejaDto> GetAccionesActivasrol(int rolId);
        ResultadoOperacion UpdateAccionesRol(int rolId, List<AccionBandejaDto> acciones);
        ResultadoOperacion UpdateRolesMenu(int menuId, List<RolDto> roles);

    }
}
