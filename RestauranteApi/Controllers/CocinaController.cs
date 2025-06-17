using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestauranteApi.Models.DTOs;
using RestauranteApi.Models.Entities;
using RestauranteApi.Repositories;
using System.Linq;

namespace RestauranteApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CocinaController : ControllerBase
    {
        private readonly Repository<Pedido> pedidoRepository;
        private readonly Repository<Pedidococina> pedidococinaRepository;
        private readonly Repository<Pedidodetalle> pedidodetalleRepository;

        public CocinaController(
            Repository<Pedido> pedidoRepository,
            Repository<Pedidococina> pedidococinaRepository,
            Repository<Pedidodetalle> pedidodetalleRepository)
        {
            this.pedidoRepository = pedidoRepository;
            this.pedidococinaRepository = pedidococinaRepository;
            this.pedidodetalleRepository = pedidodetalleRepository;
        }

        // Obtener todos los pedidos con sus detalles y estados de cocina
        [HttpGet]
        public IActionResult GetPedidosCocina()
        {
            var pedidos = pedidoRepository.GetAll()
                .Where(p => p.Estado != "Terminado") // Opcional, solo pedidos no terminados
                .Select(p => new ListaTicketsDTO
                {
                    Id = p.IdPedido,
                    NumMesa = (int)p.NumMesa,
                    Detalles = pedidodetalleRepository.GetAll()
                        .Where(d => d.IdPedido == p.IdPedido)
                        .Select(d => new DetallesTicketDTO
                        {
                            IdDetalle = d.IdDetalle,
                            TipoProducto = d.TipoProducto,
                            IdProducto = (int)d.IdProducto,
                            Cantidad = (int)d.Cantidad,
                            PrecioUnitario = (decimal)d.PrecioUnitario,
                            Estado = pedidococinaRepository.GetAll()
                                .FirstOrDefault(pc => pc.IdDetalle == d.IdDetalle)?.Estado ?? "Pendiente"
                        }).ToList()
                }).ToList();

            return Ok(pedidos);
        }

        
        [HttpPut("ActualizarEstado/{idDetalle}")]
        public IActionResult ActualizarEstado(int idDetalle, [FromBody] string nuevoEstado)
        {
            var pedidoCocina = pedidococinaRepository.GetAll().FirstOrDefault(pc => pc.IdDetalle == idDetalle);

            if (pedidoCocina == null)
            {
                // Si no existe, se crea uno nuevo
                pedidoCocina = new Pedidococina
                {
                    IdDetalle = idDetalle,
                    Estado = nuevoEstado
                };
                pedidococinaRepository.Insert(pedidoCocina);
            }
            else
            {
                pedidoCocina.Estado = nuevoEstado;
                pedidococinaRepository.Update(pedidoCocina);
            }

            
            var idPedido = pedidodetalleRepository.GetAll()
                .Where(d => d.IdDetalle == idDetalle)
                .Select(d => d.IdPedido)
                .FirstOrDefault();

            if (idPedido != 0)
            {
                var todosDetalles = pedidococinaRepository.GetAll()
                    .Where(pc => pedidodetalleRepository.GetAll().Any(d => d.IdDetalle == pc.IdDetalle && d.IdPedido == idPedido))
                    .ToList();

                bool todosTerminado = todosDetalles.All(d => d.Estado == "Terminado");
                if (todosTerminado)
                {
                    var pedido = pedidoRepository.Get(idPedido);
                    if (pedido != null)
                    {
                        pedido.Estado = "Terminado";
                        pedidoRepository.Update(pedido);
                    }
                }
            }

            return Ok();
        }
    }
}