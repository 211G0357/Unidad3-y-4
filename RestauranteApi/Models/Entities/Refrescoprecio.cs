using System;
using System.Collections.Generic;

namespace RestauranteApi.Models.Entities;

public partial class Refrescoprecio
{
    public int Id { get; set; }

    public int? IdSaboresRefresco { get; set; }

    public string Tamaño { get; set; } = null!;

    public decimal? Precio { get; set; }
}
