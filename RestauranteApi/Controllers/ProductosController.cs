using Microsoft.AspNetCore.Mvc;
using RestauranteApi.Models.DTOs;
using RestauranteApi.Models.Entities;

namespace RestauranteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController:ControllerBase
    {
        private readonly HamburguesaContext _context;

        public ProductosController(HamburguesaContext context)
        {
            _context = context;
        }

        [HttpGet("Todos")]
        public IActionResult ObtenerTodosLosProductos()
        {
            var hamburguesas = _context.Hamburguesa
                .Select(h => new ProductoDTO
                {
                    TipoProducto = "Hamburguesa",
                    IdProducto = h.IdHamburguesa,
                    Nombre = h.Categoria,
                    Precio = h.Precio ?? 0
                }).ToList();

            var papas = _context.Papas
                .Select(p => new ProductoDTO
                {
                    TipoProducto = "Papas",
                    IdProducto = p.IdPapas,
                    Nombre = p.Categoria,
                    Precio = p.Precio ?? 0
                }).ToList();

            var refrescos = _context.Refrescoprecio
                .Join(_context.Saboresrefresco,
                    rp => rp.IdSaboresRefresco,
                    sr => sr.Id,
                    (rp, sr) => new ProductoDTO
                    {
                        TipoProducto = "Refresco",
                        IdProducto = rp.Id,
                        Nombre = $"{sr.Sabor} ({rp.Tamaño})",
                        Precio = rp.Precio ?? 0
                    }).ToList();

            var productos = hamburguesas.Concat(papas).Concat(refrescos).ToList();

            return Ok(productos);
        }
    }
}
