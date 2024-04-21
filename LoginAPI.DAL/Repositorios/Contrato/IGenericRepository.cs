using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LoginAPI.DAL.Repositorios.Contrato
{
    public interface IGenericRepository<Tmodel> where Tmodel : class
    {
        Task<Tmodel> Obtener(Expression<Func<Tmodel, bool>> filtro); //A la funcion le estamos enviando un Tmodel y nos devolvera un valor bool con el nombre de filtro
        Task<Tmodel> Crear(Tmodel modelo);
        Task<bool> Editar(Tmodel modelo);
        Task<bool> Eliminar(Tmodel modelo);
        Task<IQueryable<Tmodel>> Consultar(Expression<Func<Tmodel, bool>> filtro = null);
    }
}
