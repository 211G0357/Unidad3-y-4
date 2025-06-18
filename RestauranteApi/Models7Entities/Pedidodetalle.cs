using System;
using System.Collections.Generic;

namespace RestauranteApi.Models7Entities;

public partial class Pedidodetalle
{
    public int IdDetalle { get; set; }

    public int? IdPedido { get; set; }

    public string? TipoProducto { get; set; }

    public int? IdProducto { get; set; }

    public int? Cantidad { get; set; }

    public decimal? PrecioUnitario { get; set; }

    public virtual Pedido? IdPedidoNavigation { get; set; }

    public virtual ICollection<Pedidococina> Pedidococina { get; set; } = new List<Pedidococina>();
}
