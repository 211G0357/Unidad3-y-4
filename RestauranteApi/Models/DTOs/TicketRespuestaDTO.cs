namespace RestauranteApi.Models.DTOs
{
    public class TicketRespuestaDTO
    {
        public int IdPedido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string NombreMesero { get; set; }

        public List<DetalleRespuestaDTO> Detalles { get; set; } = new();

    }
}
