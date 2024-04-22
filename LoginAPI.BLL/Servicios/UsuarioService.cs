using AutoMapper;
using LoginAPI.BLL.Servicios.Contrato;
using LoginAPI.DAL.Repositorios.Contrato;
using LoginAPI.DTO;
using LoginAPI.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginAPI.BLL.Servicios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _usuarioRepositorio;
        private readonly IMapper _mapper;

        public UsuarioService(IGenericRepository<Usuario> usuarioRepositorio, IMapper mapper)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _mapper = mapper;
        }

        public async Task<UsuarioDTO> BuscarPorID(int id)
        {
            try
            {
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(p => p.IdUsuario == id);
                if (usuarioEncontrado == null) throw new TaskCanceledException("El usuario no existe");
                return _mapper.Map<UsuarioDTO>(usuarioEncontrado);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> CambiarClave(CambiarCLaveDTO modelo)
        {
            try
            {
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.Email == modelo.Email && u.Clave == modelo.ClaveAntigua);
                if (usuarioEncontrado == null) throw new TaskCanceledException("No se puedo cambiar la clave por que el usuario no existe");
                usuarioEncontrado.Clave = modelo.ClaveNueva;
                bool respuesta = await _usuarioRepositorio.Editar(usuarioEncontrado);
                if (!respuesta) throw new TaskCanceledException("No se pudo cambiar la contraseña");
                return respuesta;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> CambiarEmail(String correo, SesionDTO sesion)
        {
            try
            {
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.Email == correo);
                if (usuarioEncontrado != null) throw new TaskCanceledException("Existe un usuario relacionado a ese correo por favor ingrese otro");
                var usuarioSesion = await _usuarioRepositorio.Obtener(u => u.Email == sesion.Email);
                if (usuarioSesion == null) throw new TaskCanceledException("Ocurrio un error");
                usuarioSesion.Email = correo;
                bool respuesta = await _usuarioRepositorio.Editar(usuarioSesion);
                if (!respuesta) throw new TaskCanceledException("No se pudo cambiar el correo");
                return respuesta;
            }
            catch
            {
                throw;
            }
        }

        public async Task<UsuarioDTO> Crear(UsuarioDTO modelo)
        {
            try
            {
                var usuarioCreado = await _usuarioRepositorio.Crear(_mapper.Map<Usuario>(modelo));
                if (usuarioCreado.IdUsuario == 0) throw new TaskCanceledException("No se pudo crear el usuario");

                var query = await _usuarioRepositorio.Consultar(u => u.IdUsuario == usuarioCreado.IdUsuario);
                usuarioCreado = query.Include(rol => rol.Rol).First();

                return _mapper.Map<UsuarioDTO>(usuarioCreado);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Editar(UsuarioDTO modelo)
        {
            try
            {
                var usuarioModelo = _mapper.Map<Usuario>(modelo);
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == usuarioModelo.IdUsuario);
                if (usuarioEncontrado == null) throw new TaskCanceledException("El usuario no existe");
                usuarioEncontrado.Nombres = usuarioModelo.Nombres;
                usuarioEncontrado.Apellidos = usuarioModelo.Apellidos;
                usuarioEncontrado.Foto = usuarioModelo.Foto;
                usuarioEncontrado.Pais = usuarioModelo.Pais;
                usuarioEncontrado.RolId = usuarioModelo.RolId;

                bool respuesta = await _usuarioRepositorio.Editar(usuarioEncontrado);
                if (!respuesta) throw new TaskCanceledException("No se pudo editar el usuario");
                return respuesta;

            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == id);
                if (usuarioEncontrado == null) throw new TaskCanceledException("El usuario no existe");
                bool respuesta = await _usuarioRepositorio.Eliminar(usuarioEncontrado);
                if (!respuesta) throw new TaskCanceledException("No se pudo eliminar");

                return respuesta;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<UsuarioDTO>> lista()
        {
            try
            {
                var queryUsuarios = await _usuarioRepositorio.Consultar();
                var listaUsuarios = queryUsuarios.Include(rol => rol.Rol).ToList();
                return _mapper.Map<List<UsuarioDTO>>(listaUsuarios);
            }
            catch
            {
                throw;
            }
        }

        public Task<SesionDTO> ValidarCredenciales(string correo, string clave)
        {
            throw new NotImplementedException();
        }
    }
}
