using System.ComponentModel.DataAnnotations;

namespace RestauranteApi.Models.DTOs
{
    public class ActualizarEstadoDTO
    {
        [Required]
        public string Estado { get; set; }
    }
}
