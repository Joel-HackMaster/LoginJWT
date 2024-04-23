using AutoMapper;
using LoginAPI.BLL.Servicios.Contrato;
using LoginAPI.DAL.Repositorios.Contrato;
using LoginAPI.DTO;
using LoginAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginAPI.BLL.Servicios
{
    public class ReestablecerService : IReestablecerService
    {
        private readonly IReestablecerRepository _reestablecerRepositorio;
        private readonly IGenericRepository<Usuario> _usuarioRepositorio;
        private readonly IMapper _mapper;

        public ReestablecerService(IReestablecerRepository reestablecerRepositorio, IGenericRepository<Usuario> usuarioRepositorio, IMapper mapper)
        {
            _reestablecerRepositorio = reestablecerRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _mapper = mapper;
        }

        public async Task<SesionDTO> ValidacionReestablecer(SesionDTO sesion, string nuevaClave)
        {
            try
            {
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(s => s.Email == sesion.Email);
                if (usuarioEncontrado == null) throw new TaskCanceledException("Ocurrio un error");
                usuarioEncontrado.Clave = nuevaClave;
                usuarioEncontrado.Reestablecer = true;
                Usuario usuarioDevuelto = await _reestablecerRepositorio.ValidacionReestablecer(usuarioEncontrado);
                if (usuarioDevuelto == null) throw new TaskCanceledException("Ocurrio un error al devolver el usuario");

                return _mapper.Map<SesionDTO>(usuarioDevuelto);
            }
            catch
            {
                throw;
            }
        }
    }
}
