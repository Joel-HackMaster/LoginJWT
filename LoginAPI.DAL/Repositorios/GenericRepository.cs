using LoginAPI.DAL.DBContext;
using LoginAPI.DAL.Repositorios.Contrato;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LoginAPI.DAL.Repositorios
{
    public class GenericRepository<Tmodelo> : IGenericRepository<Tmodelo> where Tmodelo : class
    {
        private readonly DbLoginJwtContext _dbContext;

        public GenericRepository(DbLoginJwtContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IQueryable<Tmodelo>> Consultar(Expression<Func<Tmodelo, bool>> filtro = null)
        {
            try
            {
                IQueryable<Tmodelo> queryModelo = filtro == null ? _dbContext.Set<Tmodelo>() : _dbContext.Set<Tmodelo>().Where(filtro);
                return queryModelo;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Tmodelo> Crear(Tmodelo modelo)
        {
            try
            {
                _dbContext.Set<Tmodelo>().Add(modelo);
                await _dbContext.SaveChangesAsync();
                return modelo;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Editar(Tmodelo modelo)
        {
            try
            {
                _dbContext.Set<Tmodelo>().Update(modelo);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(Tmodelo modelo)
        {
            try
            {
                _dbContext.Set<Tmodelo>().Remove(modelo);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Tmodelo> Obtener(Expression<Func<Tmodelo, bool>> filtro)
        {
            try
            {
                Tmodelo modelo = await _dbContext.Set<Tmodelo>().FirstOrDefaultAsync(filtro);
                return modelo;
            }
            catch
            {
                throw;
            }
        }
    }
}
