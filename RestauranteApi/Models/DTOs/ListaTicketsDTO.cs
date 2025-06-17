namespace RestauranteApi.Models.DTOs
{
    public class ListaTicketsDTO
    {
        public int? Id { get; set; }
        public string Nombre { get; set; } = "";
        public int NumMesa { get; set; }
        public List<DetallesTicketDTO> Detalles { get; set; } = new();

    }
}
