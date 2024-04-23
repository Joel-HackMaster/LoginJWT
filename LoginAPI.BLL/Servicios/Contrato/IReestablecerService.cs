using LoginAPI.DTO;
using LoginAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginAPI.BLL.Servicios.Contrato
{
    public interface IReestablecerService
    {
        Task<SesionDTO> ValidacionReestablecer(SesionDTO modelo, string nuevaClave);
    }
}
