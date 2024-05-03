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
            try
            {
                rsp.status = true;
                rsp.value = await _usuarioService.lista();
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
            string token = Request.Headers.Authorization;
            if (_autorizacionService.ValidarSesion(token) == false)
            {
                rsp.status = false;
                rsp.msg = "La sesion no esta activa";
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
            rsp.value= resultado_autorizacion;
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
        public async Task<IActionResult> CerrarSesion([FromBody] RefreshTokenRequest request, int id)
        {
            var rsp = new Response<AutorizacionResponse>();
            string token = Request.Headers.Authorization;
            if (_autorizacionService.ValidarSesion(token) == false)
            {
                rsp.status = false;
                rsp.msg = "La sesion no esta activa";
                return BadRequest(rsp);
            }

            var auth = await  _autorizacionService.CerrarSesion(request, id);
            if (auth.Resultado == false)
            {
                rsp.status = auth.Resultado;
                rsp.msg = auth.Msg;
                return BadRequest(rsp);
            }
            rsp.status = auth.Resultado;
            rsp.msg = auth.Msg;
            return Ok(rsp);
        }
    }
}
