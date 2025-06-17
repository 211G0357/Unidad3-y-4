using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteApi.Models.DTOs;
using RestauranteApi.Models.Entities;
using System.Threading.Tasks;

namespace RestauranteApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly HamburguesaContext _context;

        // Constructor con inyección de dependencias
        public ProductosController(HamburguesaContext context)
        {
            _context = context;
        }

        [HttpPost("AgregarProducto")]
        public async Task<IActionResult> AgregarProducto([FromBody] TicketDetalleCrearDTO dto)
        {
            var pedido = await _context.Pedido.FindAsync(dto.IdPedido);
            if (pedido == null)
                return NotFound($"No existe pedido con Id {dto.IdPedido}");

            decimal precioUnitario = 0m;

            switch (dto.TipoProducto)
            {
                case "Hamburguesa":
                    var hamburguesa = await _context.Hamburguesa.FindAsync(dto.IdProducto);
                    if (hamburguesa == null)
                        return NotFound($"No existe hamburguesa con Id {dto.IdProducto}");
                    precioUnitario = hamburguesa.Precio ?? 0m;
                    break;

                case "Papas":
                    var papas = await _context.Papas.FindAsync(dto.IdProducto);
                    if (papas == null)
                        return NotFound($"No existe papas con Id {dto.IdProducto}");
                    precioUnitario = papas.Precio ?? 0m;
                    break;

                case "Refresco":
                    var refresco = await _context.Refrescoprecio.FindAsync(dto.IdProducto);
                    if (refresco == null)
                        return NotFound($"No existe refresco con Id {dto.IdProducto}");
                    precioUnitario = refresco.Precio ?? 0m;
                    break;

                default:
                    return BadRequest("TipoProducto inválido. Debe ser: Hamburguesa, Papas o Refresco.");
            }

            var detalle = new Pedidodetalle
            {
                IdPedido = dto.IdPedido,
                TipoProducto = dto.TipoProducto,
                IdProducto = dto.IdProducto,
                Cantidad = dto.Cantidad,
                PrecioUnitario = precioUnitario
            };

            _context.Pedidodetalle.Add(detalle);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensaje = "Producto agregado correctamente",
                Detalle = new DetalleRespuestaDTO
                {
                    IdDetalle = detalle.IdDetalle,
                    IdPedido = (int)detalle.IdPedido,
                    TipoProducto = detalle.TipoProducto,
                    IdProducto = (int)detalle.IdProducto,
                    Cantidad = (int)detalle.Cantidad,
                    PrecioUnitario = (decimal)detalle.PrecioUnitario
                }
            });
        }
    }
}