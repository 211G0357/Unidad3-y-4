using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestauranteApi.Models.DTOs;
using RestauranteApi.Models.Entities;
using RestauranteApi.Models.Validators;
using RestauranteApi.Repositories;
using RestauranteApi.Services;

namespace RestauranteApi.Controllers
{
    [Authorize(Roles = "Cocinero")]
    [Route("api/[controller]")]
    [ApiController]
    public class CocinaController : ControllerBase
    {
        private readonly Repository<Pedido> _pedidoRepository;
        private readonly Repository<Pedidodetalle> _detalleRepository;
        private readonly Repository<Pedidococina> _cocinaRepository;
        private readonly HamburguesaContext _context;
        private readonly IHubContext<PedidosHub> _hubContext;
        private readonly TicketDetalleValidator _validator;

        public CocinaController(
            Repository<Pedido> pedidoRepository,
            Repository<Pedidodetalle> detalleRepository,
            Repository<Pedidococina> cocinaRepository,
            HamburguesaContext context,
            IHubContext<PedidosHub> hubContext,
            TicketDetalleValidator validator)
        {
            _pedidoRepository = pedidoRepository;
            _detalleRepository = detalleRepository;
            _cocinaRepository = cocinaRepository;
            _context = context;
            _hubContext = hubContext;
            _validator = validator;
        }

        [HttpGet("PedidosActivos")]
        public async Task<IActionResult> GetPedidosActivos()
        {
            var pedidos = await _context.Pedido
                .Where(p => p.Estado != "Terminado")
                .Include(p => p.Pedidodetalle)
                .OrderBy(p => p.Fecha)
                .Select(p => new
                {
                    p.IdPedido,
                    p.NumMesa,
                    p.Fecha,
                    p.Estado,
                    Detalles = p.Pedidodetalle.Select(d => new
                    {
                        d.IdDetalle,
                        d.TipoProducto,
                        d.IdProducto,
                        d.Cantidad,
                        EstadoCocina = _context.Pedidococina
                            .Where(pc => pc.IdDetalle == d.IdDetalle)
                            .Select(pc => pc.Estado)
                            .FirstOrDefault() ?? "No aplica",
                        Producto = d.TipoProducto == "Hamburguesa"
                            ? _context.Hamburguesa
                                .Where(h => h.IdHamburguesa == d.IdProducto)
                                .Select(h => h.Categoria)
                                .FirstOrDefault() ?? "Hamburguesa"
                            : d.TipoProducto == "Papas"
                                ? _context.Papas
                                    .Where(p => p.IdPapas == d.IdProducto)
                                    .Select(p => p.Categoria)
                                    .FirstOrDefault() ?? "Papas"
                                : "Refresco",
                        PrecioUnitario = d.TipoProducto == "Hamburguesa"
                            ? _context.Hamburguesa
                                .Where(h => h.IdHamburguesa == d.IdProducto)
                                .Select(h => h.Precio)
                                .FirstOrDefault() ?? 0
                            : d.TipoProducto == "Papas"
                                ? _context.Papas
                                    .Where(p => p.IdPapas == d.IdProducto)
                                    .Select(p => p.Precio)
                                    .FirstOrDefault() ?? 0
                                : _context.Refrescoprecio
                                    .Where(r => r.Id == d.IdProducto)
                                    .Select(r => r.Precio)
                                    .FirstOrDefault() ?? 0
                    })
                })
                .ToListAsync();

            return Ok(pedidos);
        }

        [HttpPut("ActualizarEstado/{idDetalle}")]
        public async Task<IActionResult> ActualizarEstado(int idDetalle, [FromQuery] string nuevoEstado)
        {
            if (nuevoEstado != "Pendiente" && nuevoEstado != "En preparación" && nuevoEstado != "Terminado")
                return BadRequest("Estado inválido.");

            var detalle = _detalleRepository.Get(idDetalle);
            if (detalle == null)
                return NotFound("Detalle no encontrado");

            if (detalle.TipoProducto != "Hamburguesa" && detalle.TipoProducto != "Papas")
                return BadRequest("Solo hamburguesas y papas pueden actualizar estado");

            var estadoCocina = _cocinaRepository.GetAll().FirstOrDefault(c => c.IdDetalle == idDetalle);

            if (estadoCocina == null)
            {
                estadoCocina = new Pedidococina
                {
                    IdDetalle = idDetalle,
                    Estado = nuevoEstado
                };
                _cocinaRepository.Insert(estadoCocina);
            }
            else
            {
                if (estadoCocina.Estado == "Terminado" && nuevoEstado != "Terminado")
                    return BadRequest("No se puede modificar un producto terminado");

                estadoCocina.Estado = nuevoEstado;
                _cocinaRepository.Update(estadoCocina);
            }

            if (_hubContext != null)
            {
                await _hubContext.Clients.Group("Cocina").SendAsync("EstadoActualizado", new
                {
                    idDetalle,
                    nuevoEstado,
                    detalle.TipoProducto,
                    Mesa = detalle.IdPedidoNavigation?.NumMesa
                });
            }

            var pedido = _pedidoRepository.Get(detalle.IdPedido ?? 0);
            if (pedido != null)
            {
                var detallesPedido = _detalleRepository.GetAll().Where(d => d.IdPedido == pedido.IdPedido).ToList();
                var todosTerminados = detallesPedido.All(d =>
                    d.TipoProducto != "Hamburguesa" && d.TipoProducto != "Papas" ||
                    _cocinaRepository.GetAll().FirstOrDefault(c => c.IdDetalle == d.IdDetalle)?.Estado == "Terminado");

                if (todosTerminados)
                {
                    pedido.Estado = "Terminado";
                    _pedidoRepository.Update(pedido);

                    if (_hubContext != null)
                    {
                        await _hubContext.Clients.Group("Meseros").SendAsync("PedidoListo", new
                        {
                            pedido.IdPedido,
                            pedido.NumMesa,
                            HoraTerminado = DateTime.Now
                        });
                    }
                }
            }

            return Ok(new
            {
                Mensaje = "Estado actualizado correctamente",
                IdDetalle = idDetalle,
                NuevoEstado = nuevoEstado
            });
        }

        [HttpPost("AgregarProducto")]
        public async Task<IActionResult> AgregarProducto([FromBody] TicketDetalleCrearDTO dto)
        {
            if (!_validator.Validate(dto, out var errores))
                return BadRequest(errores);

            var pedido = await _context.Pedido
                .FirstOrDefaultAsync(p => p.IdPedido == dto.IdPedido);

            if (pedido == null)
                return NotFound("Pedido no encontrado");

            var (precio, nombreProducto) = await ObtenerInfoProducto(dto);

            var detalle = new Pedidodetalle
            {
                IdPedido = dto.IdPedido,
                TipoProducto = dto.TipoProducto,
                IdProducto = dto.IdProducto,
                Cantidad = dto.Cantidad,
                PrecioUnitario = precio
            };

            _context.Pedidodetalle.Add(detalle);
            await _context.SaveChangesAsync();

            if (dto.TipoProducto == "Hamburguesa" || dto.TipoProducto == "Papas")
            {
                _context.Pedidococina.Add(new Pedidococina
                {
                    IdDetalle = detalle.IdDetalle,
                    Estado = "Pendiente",

                });

                await _context.SaveChangesAsync();

                await _hubContext.Clients.Group("Cocina").SendAsync("NuevoProducto", new
                {
                    detalle.IdDetalle,
                    Tipo = dto.TipoProducto,
                    Producto = nombreProducto,
                    Mesa = pedido.NumMesa,
                    Cantidad = dto.Cantidad,
                    Hora = DateTime.Now.ToString("HH:mm")
                });
            }

            if (pedido.Estado == "Pendiente")
            {
                pedido.Estado = "Preparación";
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Success = true,
                IdDetalle = detalle.IdDetalle,
                Producto = nombreProducto,
                Cantidad = dto.Cantidad,
                PrecioTotal = precio * dto.Cantidad,
                EnCocina = (dto.TipoProducto == "Hamburguesa" || dto.TipoProducto == "Papas")
            });
        }

        private async Task<(decimal precio, string nombre)> ObtenerInfoProducto(TicketDetalleCrearDTO dto)
        {
            switch (dto.TipoProducto)
            {
                case "Hamburguesa":
                    var hamburguesa = await _context.Hamburguesa
                        .FirstOrDefaultAsync(h => h.IdHamburguesa == dto.IdProducto);
                    return (hamburguesa?.Precio ?? 0, hamburguesa?.Categoria ?? "Hamburguesa");

                case "Papas":
                    var papas = await _context.Papas
                        .FirstOrDefaultAsync(p => p.IdPapas == dto.IdProducto);
                    return (papas?.Precio ?? 0, papas?.Categoria ?? "Papas");

                case "Refresco":
                    var refresco = await _context.Refrescoprecio
                        .FirstOrDefaultAsync(r => r.Id == dto.IdProducto);

                    if (refresco == null)
                        return (0, "Refresco");

                    var sabor = await _context.Saboresrefresco
                        .FirstOrDefaultAsync(s => s.Id == refresco.IdSaboresRefresco);

                    return (refresco.Precio ?? 0, $"{sabor?.Sabor ?? "Refresco"} {refresco.Tamaño}");

                default:
                    return (0, "Producto");
            }
        }
    }

}