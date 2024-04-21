using LoginAPI.DAL.DBContext;
using LoginAPI.DAL.Repositorios.Contrato;
using LoginAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginAPI.DAL.Repositorios
{
    public class ReestablecerRepository : GenericRepository<Usuario>, IReestablecerRepository
    {
        private readonly DbLoginJwtContext _dbContext;

        public ReestablecerRepository(DbLoginJwtContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Usuario> ValidacionReestablecer(Usuario modelo)
        {
            Usuario usuarioGenerado = new Usuario();

            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    Usuario usuario_encontrado = _dbContext.Usuarios.Where(p => p.IdUsuario == modelo.IdUsuario).First();
                    usuario_encontrado.Clave = modelo.Clave;
                    usuario_encontrado.Reestablecer = modelo.Reestablecer;
                    _dbContext.Usuarios.Update(usuario_encontrado);
                    await _dbContext.SaveChangesAsync();
                    usuarioGenerado = modelo;
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
                return usuarioGenerado;
            }
        }
    }
}
