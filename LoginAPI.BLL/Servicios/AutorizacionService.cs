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
            var refreshTokenEncontrado = _context.HistorialRefreshTokens.FirstOrDefault(x => x.Token == refreshTokenRequest.TokenExpirado &&
            x.RefreshToken == refreshTokenRequest.RefreshToken &&
            x.IdUsuario == IdUsuario);

            if (refreshTokenEncontrado == null) return new AutorizacionResponse { Resultado = false, Msg = "No existe RefreshToken" };

            var refreshTokenCreado = GenerarRefreshToken();
            var tokenCreado = GenerarToken(IdUsuario.ToString());

            return await GuardarHistorialRefreshToken(IdUsuario, tokenCreado, refreshTokenCreado);
        }
    }
}
