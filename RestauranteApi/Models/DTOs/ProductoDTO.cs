namespace RestauranteApi.Models.DTOs
{
    public class ProductoDTO
    {
        public string TipoProducto { get; set; } 
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
    }
}
