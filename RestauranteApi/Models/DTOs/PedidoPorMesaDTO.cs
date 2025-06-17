namespace RestauranteApi.Models.DTOs
{
    public class PedidoPorMesaDTO
    {
        public int NumMesa { get; set; }
        public List<TicketRespuestaDTO> Tickets { get; set; } = new();
    }
}
