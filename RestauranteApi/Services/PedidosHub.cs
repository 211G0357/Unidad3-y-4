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

        
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
        public async Task ActualizarEstadoCocina(int idDetalle, string nuevoEstado)
        {
            await Clients.Group("Cocina").SendAsync("EstadoActualizado", idDetalle, nuevoEstado);
        }
        public async Task NotificarNuevoPedido(int mesaId)
        {
            await Clients.Group("Meseros").SendAsync("NuevoPedido", new
            {
                mesa = mesaId,
                mensaje = $"Nuevo pedido para mesa {mesaId}"
            });
        }

        public async Task NotificarEstadoActualizado(int pedidoId)
        {
            await Clients.Group("Meseros").SendAsync("EstadoActualizado", pedidoId);
        }
    }
}
