using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace RestauranteApi.Services
{
    public class PedidosHub:Hub
    {
        // Notificar cuando un pedido cambia de estado
        public async Task NotificarPedidoListo(int idPedido, string mesa)
        {
            await Clients.All.SendAsync("PedidoListo", idPedido, mesa);
        }

        // Notificar cuando un producto cambia de estado en cocina
        public async Task NotificarEstadoCocina(int idDetalle, string estado)
        {
            await Clients.All.SendAsync("EstadoCocinaActualizado", idDetalle, estado);
        }
    }
}
