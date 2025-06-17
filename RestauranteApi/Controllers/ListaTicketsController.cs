using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestauranteApi.Models.DTOs;
using RestauranteApi.Models.Entities;
using RestauranteApi.Repositories;
using System.ComponentModel.DataAnnotations;

namespace RestauranteApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ListaTicketsController : ControllerBase
    {
        private readonly Repository<Pedido> repository;
        private readonly Repository<Pedidococina> pedidococinaRepository;
        private readonly Repository<Pedidodetalle> pedidodetallRepository;
        private readonly Repository<Pedido> pedidoRepository;
        private readonly IValidator<ListaTicketsDTO> validator;

        public ListaTicketsController(
            Repository<Pedido>repository,
            Repository<Pedidococina> pedidococinaRepository,
            Repository<Pedidodetalle>pedidodetallRepository,
            Repository<Pedido>pedidoRepository,
            IValidator<ListaTicketsDTO> validator
            ) 
        {
            this.repository = repository;
            this.pedidococinaRepository = pedidococinaRepository;
            this.pedidodetallRepository = pedidodetallRepository;
            this.pedidoRepository = pedidoRepository;
            this.validator = validator;
        }
        [HttpGet]
        public IActionResult Get()
        {
            var id = int.Parse(User.FindFirst("Id")?.Value ?? "0");

            var pedidos = repository.GetAll()
                .Where(p => p.IdUsuario == id)
                .Select(p => new
                {
                    p.IdPedido,
                    p.NumMesa,
                    p.Fecha,
                    p.Estado,
                    Detalles = pedidodetallRepository.GetAll()
                        .Where(d => d.IdPedido == p.IdPedido)
                        .Select(d => new
                        {
                            d.IdDetalle,
                            d.TipoProducto,
                            d.IdProducto,
                            d.Cantidad,
                            d.PrecioUnitario,
                            Estado = pedidococinaRepository.GetAll()
                                .FirstOrDefault(pc => pc.IdDetalle == d.IdDetalle)?.Estado ?? "Pendiente"
                        }).ToList()
                }).ToList();

            return Ok(pedidos);
        }

        [HttpPost]
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
}
