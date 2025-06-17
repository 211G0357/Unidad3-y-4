namespace RestauranteApi.Models.DTOs
{
    public class DetalleRespuestaDTO
    {
        public int IdDetalle { get; set; }
        public int IdPedido { get; set; }
        public string TipoProducto { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
