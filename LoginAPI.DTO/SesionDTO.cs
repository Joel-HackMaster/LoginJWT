using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginAPI.DTO
{
    public class SesionDTO
    {
        public int IdUsuario {  get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public string NombreRol { get; set; }
    }
}
