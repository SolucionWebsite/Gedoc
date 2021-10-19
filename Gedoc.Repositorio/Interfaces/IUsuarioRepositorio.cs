using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;

namespace Gedoc.Repositorio.Interfaces
{
    public interface IUsuarioRepositorio
    {
        //UsuarioDto GetUsuarioByFuncionarioId(int funcionarioId);
        List<UsuarioDto> GetUsuarios();
        ResultadoOperacion UpdateUsuario(UsuarioDto usuario, bool soloUpdateActivo);
        ResultadoOperacion CreateUsuario(UsuarioDto usuario);
        ResultadoOperacion UpdateSolicitanteUrgencia(int usuarioId, bool solicitanteUrg);
        void DeleteUsuario(int id);
        List<UsuarioDto> GetUsuariosTransparencia();
        List<UsuarioDto> GetUsuariosUrgencia();
        List<UsuarioDto> GetUsuariosJefaturaNoUrgencia();
        List<UsuarioDto> GetUsuariosByUt(int idUt);
        List<UsuarioDto> GetUsuariosByRol(int idRol);
        bool AccesoUrlUsuario(int usuarioId, string url);
        List<RolDto> GetRolAll();
        List<RolDto> GetRolesUsuario(int usuarioId, bool incluyeNoTiene);
        List<RolDto> GetRolesMenu(int menuId);
        ResultadoOperacion UpdateRol(RolDto rol);
        ResultadoOperacion CreateRol(RolDto rol);
        void DeleteRol(int id);
        void UpdateRolesUsuario(int usuarioId, List<RolDto> rol);
        List<AccionBandejaDto> GetAccionesActivasrol(int rolId);
        ResultadoOperacion UpdateAccionesRol(int rolId, List<int> accionesId);
        ResultadoOperacion UpdateRolesMenu(int menuId, List<int> roles);
    }
}
