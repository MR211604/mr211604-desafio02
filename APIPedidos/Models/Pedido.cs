using System.ComponentModel.DataAnnotations;

namespace APIPedidos.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "ClienteId debe ser un identificador válido.")]
        public int ClienteId { get; set; }
    }
}
