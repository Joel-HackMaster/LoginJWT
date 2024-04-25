using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LoginAPI.DTO
{
    public class UsuarioDTO
    {
        public int IdUsuario { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Foto { get; set;}
        public string Pais { get; set;}
        public string RolId { get; set;}
        public string NombreRol { get; set;}
        public string Email { get; set;}
        [JsonIgnore]
        public string Clave { get; set;}
        [JsonIgnore]
        public int Reestablecer { get; set; }
        [JsonIgnore]
        public int EsActivo { get; set; }
    }
}
