namespace RestauranteApi.Models.DTOs
{
    public class DetallesTicketDTO
    {
        public int IdDetalle { get; set; }
        public string TipoProducto { get; set; } = string.Empty; 
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string Estado { get; set; } = "Pendiente"; 
    }
}
