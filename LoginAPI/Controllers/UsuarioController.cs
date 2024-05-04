using LoginAPI.BLL.Servicios.Contrato;
using LoginAPI.Utility.Tools;
using LoginAPI.DTO;
using Microsoft.AspNetCore.Mvc;
using LoginAPI.Utilidad;
using LoginAPI.Model.Custom;
using LoginAPI.BLL.Servicios;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using LoginAPI.DTO.Validacion;

namespace LoginAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IAutorizacionService _autorizacionService;

        public UsuarioController(IUsuarioService usuarioService, IAutorizacionService autorizacionService)
        {
            _autorizacionService = autorizacionService;
            _usuarioService = usuarioService;
        }
        [Authorize]
        [HttpGet]
        [Route("ListaUsuarios")]
        public async Task<IActionResult> Lista()
        {
            var rsp = new Response<List<UsuarioDTO>>();
            var refresh = new RefreshTokenRequest();
            refresh.TokenExpirado = Request.Headers["Authorization"];
            if (refresh.TokenExpirado.StartsWith("Bearer "))
            {
                refresh.TokenExpirado = refresh.TokenExpirado.Substring(7); // Elimina los 7 caracteres de "Bearer "
            }
            refresh.RefreshToken = Request.Headers["Refresh-Token"];
            string userId = Request.Headers["User-Id"];
            var auth = await _autorizacionService.DevolverRefreshToken(refresh, int.Parse(userId));
            if (auth == null || auth.Resultado == false)
            {
                rsp.status = false;
                rsp.msg = "La sesion se cerro - "+auth.Msg;
                return BadRequest(rsp);
            }

            try
            {
                rsp.msg = "Autorizado-"+auth.Msg;
                rsp.status = true;
                rsp.value = await _usuarioService.lista();
                rsp.token = auth.Token;
                rsp.refreshtoken = auth.RefreshToken;
            }catch (Exception ex)
            {
                rsp.status=false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);
        }

        [Authorize]
        [HttpPost]
        [Route("CrearUsuario")]
        public async Task<IActionResult> CrearUsuario([FromBody] UsuarioDTO usuario)
        {
            var rsp = new Response<UsuarioDTO>();
            var refresh = new RefreshTokenRequest();
            refresh.TokenExpirado = Request.Headers["Authorization"];
            refresh.RefreshToken = Request.Headers["Refresh-Token"];
            string userId = Request.Headers["User-Id"];
            var auth = await _autorizacionService.DevolverRefreshToken(refresh, int.Parse(userId));
            if (auth.Resultado == false)
            {
                rsp.status = false;
                rsp.msg = "La sesion se cerro";
                return BadRequest(rsp);
            }

            string claveGenerada = Herramientas.GenerarClave();
            string asunto = "Creacion de Cuenta";
            string mensaje = "<h3>Su cuenta se creo correctamente</h3><br><p>Su contraseña para acceder es: !clave!</p>";
            mensaje = mensaje.Replace("!clave!", claveGenerada);
            bool respuesta = Herramientas.EnviarCorreo(usuario.Email, asunto, mensaje);

            if (respuesta)
            {
                usuario.Clave = Herramientas.ConvertSha256(claveGenerada);
                try
                {
                    rsp.status = true;
                    rsp.msg = "Autorizado";
                    rsp.token = auth.Token;
                    rsp.refreshtoken = auth.RefreshToken;
                    rsp.value = await _usuarioService.Crear(usuario);
                }
                catch (Exception ex)
                {
                    rsp.status = false;
                    rsp.msg = ex.Message;
                }
            }
            else
            {
                rsp.status = false;
                rsp.msg = "¡Error! No se pudo enviar el mensaje";
            }

            return Ok(rsp);
        }

        [HttpPost]
        [Route("Autenticar")]
        public async Task<IActionResult> Autenticar([FromBody] AutorizacionRequest autorizacion)
        {
            var rsp = new Response<AutorizacionResponse>();
            var resultado_autorizacion = await _autorizacionService.DevolverToken(autorizacion);
            if(resultado_autorizacion == null)
            {
                rsp.status = false;
                return BadRequest(rsp);
            }
            rsp.token= resultado_autorizacion.Token;
            rsp.refreshtoken= resultado_autorizacion.RefreshToken;
            rsp.status=true;
            rsp.msg = "Autorizado"; 
            return Ok(rsp);
        }
        /*
        [Authorize]
        [HttpPost]
        [Route("ObtenerRefreshToken")]
        public async Task<IActionResult> ObtenerRefreshToken([FromBody] RefreshTokenRequest request)
        {
            var rsp = new Response<AutorizacionResponse>();
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenExpiradoSupuesto = tokenHandler.ReadJwtToken(request.TokenExpirado);

            if (tokenExpiradoSupuesto.ValidTo > DateTime.UtcNow) 
            {
                rsp.status=false;
                rsp.value = new AutorizacionResponse { Resultado = false, Msg = "Token no ha expirado" };
                rsp.msg = "El token aun esta activo";
                return BadRequest(rsp); 
            }

            string idUsuario = tokenExpiradoSupuesto.Claims.First( x => 
            x.Type == JwtRegisteredClaimNames.NameId).Value.ToString();

            var autorizacionResponse = await _autorizacionService.DevolverRefreshToken(request, int.Parse(idUsuario));
            rsp.status=autorizacionResponse.Resultado;
            rsp.value = autorizacionResponse;
            rsp.msg = autorizacionResponse.Msg;

            if (rsp.status) return Ok(rsp);
            else return BadRequest(rsp);
        }
        */

        [HttpPost]
        [Route("ValidarCorreo")]
        public async Task<IActionResult> ValidarCorreo(string correo)
        {
            var rsp = new Response<ValidarEmail>();
            ValidarEmail validarEmail = new ValidarEmail();
            bool verificacion = await _usuarioService.ValidarCorreo(correo);
            rsp.status = verificacion;

            if (verificacion)
            {
                rsp.msg = "Verificado";
                return Ok(rsp);
            }

            rsp.msg = "El correo esta en uso";

            return Ok(rsp);
        }

        [Authorize]
        [HttpPost]
        [Route("CerrarSesion")]
        public async Task<IActionResult> CerrarSesion()
        {
            var rsp = new Response<AutorizacionResponse>();
            var refresh = new RefreshTokenRequest();
            refresh.TokenExpirado = Request.Headers["Authorization"];
            refresh.RefreshToken = Request.Headers["Refresh-Token"];
            string userId = Request.Headers["User-Id"];
            var auth = await _autorizacionService.DevolverRefreshToken(refresh, int.Parse(userId));
            if (auth.Resultado == false)
            {
                rsp.status = false;
                rsp.msg = "La sesion se cerro";
                return BadRequest(rsp);
            }

            var sesion = await  _autorizacionService.CerrarSesion(refresh, int.Parse(userId));
            if (sesion.Resultado == false)
            {
                rsp.status = sesion.Resultado;
                rsp.msg = sesion.Msg;
                return BadRequest(rsp);
            }
            rsp.status = auth.Resultado;
            rsp.msg = auth.Msg;
            return Ok(rsp);
        }
    }
}
