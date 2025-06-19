using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace RestauranteApi.Services
{
    public class PedidosHub:Hub
    {
        public async Task NotificarPedidoListo(int idPedido, string mesa)
        {
            await Clients.All.SendAsync("PedidoListo", idPedido, mesa);
        }

        public async Task NotificarEstadoCocina(int idDetalle, string estado)
        {
            await Clients.All.SendAsync("EstadoCocinaActualizado", idDetalle, estado);
        }
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
