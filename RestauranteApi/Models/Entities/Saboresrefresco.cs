﻿using System;
using System.Collections.Generic;

namespace RestauranteApi.Models.Entities;

public partial class Saboresrefresco
{
    public int Id { get; set; }

    public string Sabor { get; set; } = null!;
}
