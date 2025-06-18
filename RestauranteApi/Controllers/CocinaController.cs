using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteApi.Models.DTOs;
using RestauranteApi.Models.Entities;
using RestauranteApi.Repositories;

namespace RestauranteApi.Controllers
{
    [Authorize(Roles = "Cocinero")]
    [Route("api/[controller]")]
    [ApiController]
    public class CocinaController : ControllerBase
    {
        private readonly Repository<Pedido> pedidoRepository;
        private readonly Repository<Pedidodetalle> detalleRepository;
        private readonly Repository<Pedidococina> cocinaRepository;
        private readonly HamburguesaContext _context;

        public CocinaController(
            Repository<Pedido> pedidoRepository,
            Repository<Pedidodetalle> detalleRepository,
            Repository<Pedidococina> cocinaRepository,
            HamburguesaContext context)
        {
            this.pedidoRepository = pedidoRepository;
            this.detalleRepository = detalleRepository;
            this.cocinaRepository = cocinaRepository;
            _context = context;
        }

        // 🔹 Obtener todos los pedidos activos con sus productos y estado
        [HttpGet]
        public IActionResult GetPedidos()
        {
            var pedidos = pedidoRepository.GetAll()
            .Where(p => p.Estado != "Terminado")
            .ToList(); 

            var detalles = detalleRepository.GetAll()
                .ToList(); 

            var cocinas = cocinaRepository.GetAll()
                .ToList(); 

            var resultado = pedidos.Select(p => new
            {
                p.IdPedido,
                p.NumMesa,
                p.Fecha,
                Detalles = detalles
                    .Where(d => d.IdPedido == p.IdPedido)
                    .Select(d => new
                    {
                        d.IdDetalle,
                        d.TipoProducto,
                        d.IdProducto,
                        d.Cantidad,
                        Estado = cocinas.FirstOrDefault(c => c.IdDetalle == d.IdDetalle)?.Estado ?? "Pendiente"
                    }).ToList()
            }).ToList();

            return Ok(resultado);
        }


        [HttpPut("ActualizarEstado/{idDetalle}")]
        public IActionResult ActualizarEstado(int idDetalle, [FromQuery] string nuevoEstado)
        {
            if (nuevoEstado != "Pendiente" && nuevoEstado != "En preparación" && nuevoEstado != "Terminado")
                return BadRequest("Estado inválido.");

            var cocina = cocinaRepository.GetAll().FirstOrDefault(c => c.IdDetalle == idDetalle);
            if (cocina == null)
            {
                cocina = new Pedidococina
                {
                    IdDetalle = idDetalle,
                    Estado = nuevoEstado
                };
                cocinaRepository.Insert(cocina);
            }
            else
            {
                cocina.Estado = nuevoEstado;
                cocinaRepository.Update(cocina);
            }


            var detalle = detalleRepository.Get(idDetalle);
            if (detalle != null)
            {
                var idPedido = detalle.IdPedido;
                var detalles = detalleRepository.GetAll().Where(d => d.IdPedido == idPedido).ToList();
                var todosTerminados = detalles.All(d =>
                    cocinaRepository.GetAll().FirstOrDefault(c => c.IdDetalle == d.IdDetalle)?.Estado == "Terminado"
                );

                if (todosTerminados)
                {
                    var pedido = pedidoRepository.Get(idPedido);
                    if (pedido != null)
                    {
                        pedido.Estado = "Terminado";
                        pedidoRepository.Update(pedido);
                    }
                }
            }

            return Ok("Estado actualizado correctamente.");
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