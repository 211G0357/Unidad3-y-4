using Microsoft.AspNetCore.SignalR;
using RestauranteApi.Models.Entities;
using System.Threading.Tasks;
namespace RestauranteApi.Services
{
    public class PedidosHub:Hub
    {
        
        public async Task NotificarPedidoListo(int idPedido, string mesa)
        {
            await Clients.All.SendAsync("PedidoListo", new
            {
                IdPedido = idPedido,
                NumMesa = mesa,
                HoraTerminado = DateTime.Now
            });
        }

        public async Task NotificarEstadoCocina(int idDetalle, string estado)
        {
            await Clients.All.SendAsync("EstadoCocinaActualizado", idDetalle, estado);
        }
       

    }
}
