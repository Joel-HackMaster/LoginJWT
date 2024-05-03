using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using LoginAPI.Model.Custom;
using Microsoft.Extensions.Configuration;
using LoginAPI.BLL.Servicios.Contrato;
using LoginAPI.DAL.DBContext;
using LoginAPI.Utility.Tools;
using System.Security.Cryptography;
using LoginAPI.Model;
using Azure.Core;

namespace LoginAPI.BLL.Servicios
{
    public class AutorizacionService : IAutorizacionService
    {
        private readonly DbLoginJwtContext _context;
        private readonly IConfiguration _configuration;

        public AutorizacionService(DbLoginJwtContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private string GenerarToken(string idUsuario)
        {
            var key = _configuration.GetValue<string>("JwtSettings:key"); //Obtemos la llave de appsettings
            var keyBytes = Encoding.ASCII.GetBytes(key);//Convertimos key en un array

            var claims = new ClaimsIdentity(); //creamos un claim
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, idUsuario));//Agregamos al claim la informacion del usuario en este caso el idusuario

            var credencialesToken = new SigningCredentials( //Generamos las credenciales para nuestro token
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature
                );
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(10), //Tiempo universal
                SigningCredentials = credencialesToken
            };

            var tokenHandler = new JwtSecurityTokenHandler(); //Controladores del token
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);

            string tokenCreado = tokenHandler.WriteToken(tokenConfig);

            return tokenCreado;

        }

        public async Task<AutorizacionResponse> DevolverToken(AutorizacionRequest autorizacion)
        {
            var usuario_encontrado = _context.Usuarios.FirstOrDefault(x =>
                x.Email == autorizacion.Correo && x.Clave == Herramientas.ConvertSha256(autorizacion.Clave)
                );
            if(usuario_encontrado == null)
            {
                return await Task.FromResult<AutorizacionResponse>(null);
            }

            string tokenCreado = GenerarToken(usuario_encontrado.IdUsuario.ToString());

            string refreshTokenCreado = GenerarRefreshToken();

            //return new AutorizacionResponse() { Token = tokenCreado, Resultado= true, Msg="Ok" };
            return await GuardarHistorialRefreshToken(usuario_encontrado.IdUsuario, tokenCreado, refreshTokenCreado);
        }

        private string GenerarRefreshToken()
        {
            var byteArray = new byte[64];
            var refreshToken = "";

            using (var mg = RandomNumberGenerator.Create())
            {
                mg.GetBytes(byteArray);
                refreshToken = Convert.ToBase64String(byteArray);
            }
            return refreshToken;
        }

        private async Task<AutorizacionResponse> GuardarHistorialRefreshToken(int IdUsuario, string token, string refreshToken)
        {
            var historialRefresh = new HistorialRefreshToken
            {
                IdUsuario = IdUsuario,
                Token = token,
                RefreshToken = refreshToken,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddDays(1),
            };

            await _context.HistorialRefreshTokens.AddAsync(historialRefresh);
            await _context.SaveChangesAsync();

            return new AutorizacionResponse { Token= token, RefreshToken= refreshToken , Resultado=true, Msg="Ok"};
        }

        public async Task<AutorizacionResponse> DevolverRefreshToken(RefreshTokenRequest refreshTokenRequest, int IdUsuario)
        {
            var refreshTokenEncontrado = _context.HistorialRefreshTokens.FirstOrDefault(x => x.RefreshToken == refreshTokenRequest.RefreshToken &&
            x.IdUsuario == IdUsuario);

            if (refreshTokenEncontrado == null) return new AutorizacionResponse { Resultado = false, Msg = "No existe RefreshToken" };

            if (refreshTokenEncontrado.EsActivo == true)
            {
                return await RestaurarTokens(refreshTokenEncontrado, refreshTokenRequest, IdUsuario);
            }

            return new AutorizacionResponse { Resultado = false, Msg = "La sesion caduco" };
        }

        public async Task<AutorizacionResponse> RestaurarTokens(HistorialRefreshToken refreshTokenEncontrado, RefreshTokenRequest refreshTokenRequest, int IdUsuario)
        {
            if ((refreshTokenEncontrado.FechaExpiracion - DateTime.UtcNow).TotalHours <= 1)
            {
                refreshTokenEncontrado.FechaExpiracion = DateTime.UtcNow.AddHours(2);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenExpiradoSupuesto = tokenHandler.ReadJwtToken(refreshTokenRequest.TokenExpirado);

            if (tokenExpiradoSupuesto.ValidTo > DateTime.UtcNow)
            {
                return new AutorizacionResponse { Resultado = true, Msg = "El token aun esta Activo" };
            }

            refreshTokenEncontrado.Token = GenerarToken(IdUsuario.ToString());

            _context.HistorialRefreshTokens.Update(refreshTokenEncontrado);
            await _context.SaveChangesAsync();
            return new AutorizacionResponse { Resultado = true, Token = refreshTokenEncontrado.Token, Msg = "El token se restauro" };
        }



        public async Task<AutorizacionResponse> CerrarSesion(RefreshTokenRequest refreshTokenRequest, int IdUsuario)
        {
            var refreshTokenEncontrado = _context.HistorialRefreshTokens.FirstOrDefault(x => x.IdUsuario == IdUsuario &&
            x.RefreshToken == refreshTokenRequest.RefreshToken);

            if (refreshTokenEncontrado != null)
            {
                refreshTokenEncontrado.FechaExpiracion = DateTime.UtcNow;
                refreshTokenEncontrado.EsActivo = false;
                _context.HistorialRefreshTokens.Update(refreshTokenEncontrado);

                var tokenInvalido = new TokenListaNegra
                {
                    IdUsuario = IdUsuario,
                    TokenInvalido = refreshTokenEncontrado.Token,
                };
                await _context.TokenListaNegra.AddAsync(tokenInvalido);
                await _context.SaveChangesAsync();
                return new AutorizacionResponse { Resultado = true, Msg = "Se cerro la sesion." };
            }

            return new AutorizacionResponse { Resultado = false, Msg = "Hubo un error y no se pudo cerrar sesion." };
        }

        public bool ValidarSesion(String token)
        {
            var tokenInvalido = _context.TokenListaNegra.FirstOrDefault(x => x.TokenInvalido == token);
            if (tokenInvalido != null)
            {
                return false;
            }
            return true;
        }
    }
}
