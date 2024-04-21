using LoginAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginAPI.DAL.Repositorios.Contrato
{
    public interface IReestablecerRepository : IGenericRepository<Usuario>
    {
        Task<Usuario> ValidacionReestablecer(Usuario usuario);
    }
}
