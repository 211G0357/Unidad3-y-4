namespace RestauranteApi.Models.DTOs
{
    public class PedidoCocinaDTO
    {
        public int IdPedido { get; set; }
        public int? NumMesa { get; set; }
        public DateTime? Fecha { get; set; }
        public string Estado { get; set; }
        public List<ProductoCocinaDTO> Productos { get; set; }
    }
}
