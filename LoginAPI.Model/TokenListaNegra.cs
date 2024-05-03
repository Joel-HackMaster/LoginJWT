using System;
using System.Collections.Generic;

namespace LoginAPI.Model;

public partial class TokenListaNegra
{
    public int IdTokenLista { get; set; }

    public int? IdUsuario { get; set; }

    public string TokenInvalido { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; }
}
