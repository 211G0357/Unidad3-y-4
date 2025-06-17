using System;
using System.Collections.Generic;

namespace RestauranteApi.Models.Entities;

public partial class Hamburguesa
{
    public int IdHamburguesa { get; set; }

    public string? Categoria { get; set; }

    public decimal? Precio { get; set; }
}
