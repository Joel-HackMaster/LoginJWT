using LoginAPI.BLL.Servicios.Contrato;
using LoginAPI.Utility.Tools;
using LoginAPI.DTO;
using Microsoft.AspNetCore.Mvc;
using LoginAPI.Utilidad;
using LoginAPI.Model.Custom;
using LoginAPI.BLL.Servicios;
using Microsoft.AspNetCore.Authorization;

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

        [HttpPost]
        [Route("CrearUsuario")]
        public async Task<IActionResult> CrearUsuario([FromBody] UsuarioDTO usuario)
        {
            var rsp = new Response<UsuarioDTO>();
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
            var resultado_autorizacion = await _autorizacionService.DevolverToken(autorizacion);
            if(resultado_autorizacion == null)
            {
                return Unauthorized();
            }

            return Ok(resultado_autorizacion);
        }


    }
}
