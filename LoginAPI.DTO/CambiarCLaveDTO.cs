using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginAPI.DTO
{
    public class CambiarCLaveDTO
    {
        public string Email { get; set; }
        public string ClaveAntigua { get; set; }
        public string ClaveNueva { get; set; }
    }
}
