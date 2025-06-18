namespace RestauranteApi.Models.DTOs
{
    public class ProductoCocinaDTO
    {
        public int IdDetalle { get; set; }
        public string Tipo { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public string Estado { get; set; }
    }
}
