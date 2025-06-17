using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public CocinaController(
            Repository<Pedido> pedidoRepository,
            Repository<Pedidodetalle> detalleRepository,
            Repository<Pedidococina> cocinaRepository)
        {
            this.pedidoRepository = pedidoRepository;
            this.detalleRepository = detalleRepository;
            this.cocinaRepository = cocinaRepository;
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

        // 🔹 Cambiar estado de un producto
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

            // Verificar si todos los detalles del pedido están "Terminado"
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
    }
}