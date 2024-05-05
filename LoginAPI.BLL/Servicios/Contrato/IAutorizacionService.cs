using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoginAPI.DTO;
using LoginAPI.Model.Custom;

namespace LoginAPI.BLL.Servicios.Contrato
{
    public interface IAutorizacionService
    {
        Task<AutorizacionResponse> DevolverToken(AutorizacionRequest autorizacion);
        Task<AutorizacionResponse> DevolverRefreshToken(RefreshTokenRequest refreshTokenRequest, int IdUsuario);
        Task<SesionDTO> DevolverSesion(AutorizacionRequest autorizacion);
        Task<AutorizacionResponse> CerrarSesion(RefreshTokenRequest refreshTokenRequest, int IdUsuario);
        bool ValidarSesion(String token);
    }
}
