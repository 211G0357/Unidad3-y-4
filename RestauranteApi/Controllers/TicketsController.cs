using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestauranteApi.Models.DTOs;
using RestauranteApi.Models.Entities;
using RestauranteApi.Repositories;
using System.Net.Sockets;

[Authorize(Roles = "Mesero")]
[Route("api/[controller]")]
[ApiController]
public class TicketsController : ControllerBase
{
    private readonly Repository<Pedido> ticketRepository;
    private readonly HamburguesaContext _context;

    public TicketsController(Repository<Pedido> ticketRepository, HamburguesaContext context )
    {
        this.ticketRepository = ticketRepository;
        _context = context;
    }

    [HttpPost]
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
}
