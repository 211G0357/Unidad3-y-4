using System;
using System.Collections.Generic;

namespace RestauranteApi.Models7Entities;

public partial class Pedidococina
{
    public int IdEstado { get; set; }

    public int? IdDetalle { get; set; }

    public string? Estado { get; set; }

    public virtual Pedidodetalle? IdDetalleNavigation { get; set; }
}
