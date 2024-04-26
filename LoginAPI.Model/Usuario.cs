using System;
using System.Collections.Generic;

namespace LoginAPI.Model;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombres { get; set; }

    public string Apellidos { get; set; }

    public string Foto { get; set; }

    public string Pais { get; set; }

    public bool? Reestablecer { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public int? RolId { get; set; }

    public string Email { get; set; }

    public string Clave { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<HistorialRefreshToken> HistorialRefreshTokens { get; set; } = new List<HistorialRefreshToken>();

    public virtual Rol Rol { get; set; }
}
