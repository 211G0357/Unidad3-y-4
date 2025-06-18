using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteApi.Models.DTOs;
using RestauranteApi.Models.Entities;
using RestauranteApi.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

[Authorize(Roles = "Mesero")]
[Route("api/[controller]")]
[ApiController]
public class TicketsController : ControllerBase
{
    private readonly Repository<Pedido> ticketRepository;
    private readonly HamburguesaContext _context;
    private readonly Repository<Pedido> repository;
    private readonly Repository<Pedidodetalle> pedidodetallRepository;
    private readonly Repository<Pedidococina> pedidococinaRepository;
    private readonly IValidator<ListaTicketsDTO> validator;

    public TicketsController(Repository<Pedido> ticketRepository, 
        HamburguesaContext context, 
        Repository<Pedido> repository, 
        Repository<Pedidodetalle> pedidodetallRepository, 
        Repository<Pedidococina> pedidococinaRepository,
        IValidator<ListaTicketsDTO> validator)
    {
        this.ticketRepository = ticketRepository;
        _context = context;
        this.repository = repository;
        this.pedidodetallRepository = pedidodetallRepository;
        this.pedidococinaRepository = pedidococinaRepository;
        this.validator = validator;
    }
    
    [HttpPost("CrearTicket")]
    public IActionResult CrearTicket([FromBody] CrearTicketDTO dto)
    {
        
        var existe = ticketRepository.GetAll()
            .Any(t => t.NumMesa == dto.NumMesa && t.Estado == "Pendiente");

        if (existe)
            return BadRequest($"Ya hay un ticket activo en la mesa {dto.NumMesa}");

        
        var idMesero = int.Parse(User.FindFirst("IdUsuario")?.Value ?? "0");

        var nuevo = new Pedido
        {
            NumMesa = dto.NumMesa,
            IdUsuario = idMesero,
            Fecha = DateTime.Now,
            Estado = "Pendiente"
        };

        ticketRepository.Insert(nuevo);

        return Ok(new { mensaje = "Ticket creado", nuevo.IdPedido });
    }
    [HttpGet("PedidosPorMesa")]
    public async Task<IActionResult> GetPedidosPorMesa()
    {
        var pedidos = await _context.Pedido
            .Where(p => p.Estado == "Pendiente" || p.Estado == "Activo")
            .Include(p => p.Pedidodetalle)
            .Include(p => p.IdUsuarioNavigation)
            .ToListAsync();

        var pedidosPorMesa = pedidos
         .GroupBy(p => p.NumMesa ?? 0)
         .Select(g => new PedidoPorMesaDTO
         {
             NumMesa = g.Key,
             Tickets = g.Select(p => new TicketRespuestaDTO
             {
                 IdPedido = p.IdPedido,
                 Estado = p.Estado ?? "",
                 NombreMesero = p.IdUsuarioNavigation?.Nombre ?? "Desconocido",
                 Detalles = p.Pedidodetalle.Select(d => new DetalleRespuestaDTO
                 {
                     IdDetalle = d.IdDetalle,
                     IdPedido = d.IdPedido ?? 0,
                     TipoProducto = d.TipoProducto ?? "",
                     IdProducto = d.IdProducto ?? 0,
                     Cantidad = d.Cantidad ?? 0,
                     PrecioUnitario = d.PrecioUnitario ?? 0m
                 }).ToList()
             }).ToList()
         })
         .OrderBy(m => m.NumMesa)
         .ToList();

        return Ok(pedidosPorMesa);
    }
    [Authorize(Roles = "Mesero")]
    [HttpPost("CrearTicketConDetalles")]
    public async Task<IActionResult> CrearTicketConDetalles([FromBody] CrearTicketConDetallesDTO dto)
    {
        // 1. Validaciones básicas
        if (dto == null) return BadRequest("Datos no proporcionados");
        if (dto.NumMesa <= 0) return BadRequest("Mesa inválida");
        if (dto.Detalles == null || !dto.Detalles.Any())
            return BadRequest("Debe incluir al menos un producto");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 2. Crear el pedido principal
            var pedido = new Pedido
            {
                NumMesa = dto.NumMesa,
                Fecha = DateTime.Now,
                IdUsuario = int.Parse(User.FindFirst("IdUsuario")?.Value ?? "0"),
                Estado = "Pendiente"
            };

            _context.Pedido.Add(pedido);
            await _context.SaveChangesAsync(); // Guardar para obtener el ID

            // 3. Procesar detalles
            foreach (var detalleDto in dto.Detalles)
            {
                var detalle = new Pedidodetalle
                {
                    IdPedido = pedido.IdPedido,
                    TipoProducto = detalleDto.TipoProducto,
                    IdProducto = detalleDto.IdProducto,
                    Cantidad = detalleDto.Cantidad,
                    PrecioUnitario = detalleDto.PrecioUnitario
                };

                _context.Pedidodetalle.Add(detalle);
                await _context.SaveChangesAsync(); // Guardar para obtener IdDetalle

                // 4. Solo para productos de cocina
                if (detalleDto.TipoProducto == "Hamburguesa" || detalleDto.TipoProducto == "Papas")
                {
                    var pedidoCocina = new Pedidococina
                    {
                        IdDetalle = detalle.IdDetalle, // Usa el ID ya generado
                        Estado = "Pendiente"
                    };
                    _context.Pedidococina.Add(pedidoCocina);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

           
            return Ok(new
            {
                mensaje = "Ticket creado",
                idPedido = pedido.IdPedido,
                detalles = dto.Detalles.Count
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Error al crear ticket: {ex.Message}");
        }
    }

    [Authorize(Roles = "Mesero,Cocinero")]
    [HttpGet("get")]
    public IActionResult Get()
    {
        int id = int.Parse(User.FindFirst("IdUsuario")?.Value ?? "0");

        var pedidos = repository.GetAll()
            .Where(p => p.IdUsuario == id)
            .ToList(); 

        var detalles = pedidodetallRepository.GetAll().ToList();
        var estados = pedidococinaRepository.GetAll().ToList(); 

        var resultado = pedidos.Select(p => new
        {
            p.IdPedido,
            p.NumMesa,
            p.Fecha,
            p.Estado,
            Detalles = detalles
                .Where(d => d.IdPedido == p.IdPedido)
                .Select(d => new
                {
                    d.IdDetalle,
                    d.TipoProducto,
                    d.IdProducto,
                    d.Cantidad,
                    d.PrecioUnitario,
                    Estado = estados.FirstOrDefault(pc => pc.IdDetalle == d.IdDetalle)?.Estado ?? "Pendiente"
                }).ToList()
        }).ToList();

        return Ok(resultado);
    }

    [Authorize(Roles = "Mesero,Cocinero")]
    [HttpPost("Post")]
    public IActionResult Post(ListaTicketsDTO dto)
    {
        var validatorResult = validator.Validate(dto);
        if (!validatorResult.IsValid)
        {
            return BadRequest(validatorResult.Errors.Select(x => x.ErrorMessage));
        }

        var idUsuario = int.Parse(User.FindFirst("Id")?.Value ?? "0");

        var pedido = new Pedido
        {
            IdUsuario = idUsuario,
            NumMesa = dto.NumMesa,
            Fecha = DateTime.Now,
            Estado = "Pendiente"
        };

        repository.Insert(pedido);

        return Ok();
    }
    [Authorize(Roles = "Cocinero")]
    [HttpPut("ActualizarEstadoCocina/{idDetalle}")]
    public IActionResult ActualizarEstadoCocina(int idDetalle, [FromBody] ActualizarEstadoDTO dto)
    {
        // 1. Buscar el detalle con su relación a cocina
        var detalle = _context.Pedidodetalle
            .Include(d => d.Pedidococina) // Asegúrate de incluir la relación
            .FirstOrDefault(d => d.IdDetalle == idDetalle);

        if (detalle == null)
            return NotFound("Detalle de pedido no encontrado");

        // 2. Verificar que existe registro en cocina
        var pedidoCocina = detalle.Pedidococina?.FirstOrDefault(); // Acceder al primer elemento

        if (pedidoCocina == null)
            return BadRequest("Este producto no requiere preparación en cocina");

        // 3. Validar el estado
        if (!new[] { "Pendiente", "En preparación", "Terminado" }.Contains(dto.Estado))
            return BadRequest("Estado no válido");

        // 4. Actualizar el estado
        pedidoCocina.Estado = dto.Estado; // Acceder al elemento individual

        try
        {
            _context.SaveChanges();
            return Ok(new
            {
                mensaje = $"Estado actualizado a {dto.Estado}",
                idDetalle,
                producto = detalle.TipoProducto
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al guardar: {ex.Message}");
        }
    }
    [Authorize(Roles = "Cocinero")]
    [HttpGet("PedidosCocina")]
    public IActionResult GetPedidosCocina()
    {
        var pedidos = _context.Pedidococina
            .Include(pc => pc.IdDetalleNavigation)
            .ThenInclude(d => d.IdPedidoNavigation)
            .Where(pc => pc.Estado != "Terminado")
            .Select(pc => new {
                pc.IdEstado,
                pc.Estado,
                Detalle = new
                {
                    pc.IdDetalleNavigation.IdDetalle,
                    pc.IdDetalleNavigation.TipoProducto,
                    pc.IdDetalleNavigation.Cantidad,
                    Pedido = new
                    {
                        pc.IdDetalleNavigation.IdPedidoNavigation.IdPedido,
                        pc.IdDetalleNavigation.IdPedidoNavigation.NumMesa,
                        pc.IdDetalleNavigation.IdPedidoNavigation.Fecha
                    }
                }
            })
            .ToList();

        return Ok(pedidos);
    }

}
