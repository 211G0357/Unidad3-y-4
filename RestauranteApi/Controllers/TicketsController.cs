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
    [HttpPost("CrearTicketConDetalles")]
    public IActionResult CrearTicketConDetalles([FromBody] CrearTicketConDetallesDTO dto)
    {
        if (dto.Detalles == null || !dto.Detalles.Any())
            return BadRequest("Debe incluir al menos un producto.");

        var idMesero = int.Parse(User.FindFirst("IdUsuario")?.Value ?? "0");

        // Validar si ya existe un ticket activo en esa mesa
        var yaExiste = ticketRepository.GetAll()
            .Any(t => t.NumMesa == dto.NumMesa && t.Estado == "Pendiente");

        if (yaExiste)
            return BadRequest($"Ya existe un ticket activo para la mesa {dto.NumMesa}");

        // Crear el ticket (pedido)
        var nuevo = new Pedido
        {
            NumMesa = dto.NumMesa,
            Fecha = DateTime.Now,
            IdUsuario = idMesero,
            Estado = "Pendiente"
        };

        ticketRepository.Insert(nuevo);

        // Guardar los detalles
        foreach (var detalle in dto.Detalles)
        {
            var nuevoDetalle = new Pedidodetalle
            {
                IdPedido = nuevo.IdPedido,
                TipoProducto = detalle.TipoProducto,
                IdProducto = detalle.IdProducto,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario
            };

            _context.Pedidodetalle.Add(nuevoDetalle);

            // Si es hamburguesa o papas, agregamos registro a cocina
            if (detalle.TipoProducto.Contains("Hamburguesa") || detalle.TipoProducto.Contains("Papas"))
            {
                _context.Pedidococina.Add(new Pedidococina
                {
                    IdDetalleNavigation = nuevoDetalle,
                    Estado = "Pendiente"
                });
            }
        }

        _context.SaveChanges();

        return Ok(new { mensaje = "Ticket creado correctamente", nuevo.IdPedido });
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
        // Aquí podrías insertar los detalles del pedido si vienen incluidos en el DTO

        return Ok();
    }
}
