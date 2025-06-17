using System;
using System.Collections.Generic;

namespace RestauranteApi.Models.Entities;

public partial class Pedido
{
    public int IdPedido { get; set; }

    public int? NumMesa { get; set; }

    public DateTime? Fecha { get; set; }

    public int? IdUsuario { get; set; }

    public string? Estado { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<Pedidodetalle> Pedidodetalle { get; set; } = new List<Pedidodetalle>();
}
