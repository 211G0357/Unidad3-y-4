namespace RestauranteApi.Models.DTOs
{
    public class TicketDetalleCrearDTO
    {
        public int IdPedido { get; set; }
        public string TipoProducto { get; set; } = string.Empty;
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }

    }
}
