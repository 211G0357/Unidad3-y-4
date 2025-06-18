using System;
using System.Collections.Generic;

namespace RestauranteApi.Models7Entities;

public partial class Papas
{
    public int IdPapas { get; set; }

    public string? Categoria { get; set; }

    public decimal? Precio { get; set; }
}
