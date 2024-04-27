using LoginAPI.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginAPI.BLL.Servicios.Contrato
{
    public interface IUsuarioService
    {
        Task<List<UsuarioDTO>> lista();
        Task<UsuarioDTO> Crear(UsuarioDTO modelo);
        Task<bool> Editar(UsuarioDTO modelo);
        Task<bool> Eliminar(int id);
        Task<UsuarioDTO> BuscarPorID(int id);
        Task<bool> CambiarEmail(string correo, SesionDTO sesion);
        Task<bool> CambiarClave(CambiarCLaveDTO modelo);
        Task<bool> ValidarCorreo(string correo);
    }
}
