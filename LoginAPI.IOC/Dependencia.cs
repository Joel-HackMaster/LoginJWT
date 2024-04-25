using LoginAPI.BLL.Servicios;
using LoginAPI.BLL.Servicios.Contrato;
using LoginAPI.DAL.DBContext;
using LoginAPI.DAL.Repositorios;
using LoginAPI.DAL.Repositorios.Contrato;
using LoginAPI.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginAPI.IOC
{
    public static class Dependencia
    {
        public static void InyectarDependencias(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddDbContext<DbLoginJwtContext>(option => {
                option.UseSqlServer(configuration.GetConnectionString("cadenaSQL"));
            });
            service.AddTransient(typeof(IGenericRepository<>),typeof(GenericRepository<>));
            service.AddScoped<IReestablecerRepository, ReestablecerRepository>();

            service.AddAutoMapper(typeof(AutoMapperProfile));
            service.AddScoped<IReestablecerService, ReestablecerService>();
            service.AddScoped<IUsuarioService, UsuarioService>();
            service.AddScoped<IAutorizacionService, AutorizacionService>();
        }
    }
}
