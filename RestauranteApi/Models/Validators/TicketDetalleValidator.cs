using RestauranteApi.Models.DTOs;
using RestauranteApi.Models.Entities;
using RestauranteApi.Repositories;
using System.Text.RegularExpressions;

namespace RestauranteApi.Models.Validators
{
    public class TicketDetalleValidator
    {
        private readonly Repository<Pedido> _pedidoRepository;
        private readonly Repository<Hamburguesa> _hamburguesaRepository;
        private readonly Repository<Papas> _papasRepository;
        private readonly Repository<Refrescoprecio> _refrescoRepository;

        public TicketDetalleValidator(
            Repository<Pedido> pedidoRepository,
            Repository<Hamburguesa> hamburguesaRepository,
            Repository<Papas> papasRepository,
            Repository<Refrescoprecio> refrescoRepository)
        {
            _pedidoRepository = pedidoRepository;
            _hamburguesaRepository = hamburguesaRepository;
            _papasRepository = papasRepository;
            _refrescoRepository = refrescoRepository;
        }

        public bool Validate(TicketDetalleCrearDTO dto, out List<string> errores)
        {
            errores = new List<string>();

            
            if (dto.IdPedido <= 0)
            {
                errores.Add("El ID del pedido debe ser mayor que cero");
            }
            else
            {
                var pedido = _pedidoRepository.Get(dto.IdPedido);
                if (pedido == null)
                {
                    errores.Add($"No existe un pedido con ID {dto.IdPedido}");
                }
                else if (pedido.Estado == "Terminado")
                {
                    errores.Add("No se pueden agregar productos a un pedido terminado");
                }
            }

            if (string.IsNullOrWhiteSpace(dto.TipoProducto))
            {
                errores.Add("El tipo de producto es requerido");
            }
            else if (!Regex.IsMatch(dto.TipoProducto, "^(Hamburguesa|Papas|Refresco)$", RegexOptions.IgnoreCase))
            {
                errores.Add("Tipo de producto no válido. Debe ser: Hamburguesa, Papas o Refresco");
            }

            if (dto.IdProducto <= 0)
            {
                errores.Add("El ID del producto debe ser mayor que cero");
            }
            else
            {
                switch (dto.TipoProducto?.ToLower())
                {
                    case "hamburguesa":
                        if (_hamburguesaRepository.Get(dto.IdProducto) == null)
                            errores.Add($"No existe hamburguesa con ID {dto.IdProducto}");
                        break;

                    case "papas":
                        if (_papasRepository.Get(dto.IdProducto) == null)
                            errores.Add($"No existen papas con ID {dto.IdProducto}");
                        break;

                    case "refresco":
                        if (_refrescoRepository.Get(dto.IdProducto) == null)
                            errores.Add($"No existe refresco con ID {dto.IdProducto}");
                        break;
                }
            }

            if (dto.Cantidad <= 0)
            {
                errores.Add("La cantidad debe ser mayor que cero");
            }
            else if (dto.Cantidad > 10)
            {
                errores.Add("La cantidad máxima por producto es 10");
            }

            return errores.Count == 0;
        }
    }
}
