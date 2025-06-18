namespace RestauranteApi.Models.DTOs
{
    public class CrearTicketConDetallesDTO
    {
        public int NumMesa { get; set; }
        public List<DetallesTicketDTO> Detalles { get; set; } = new();

    }
}
