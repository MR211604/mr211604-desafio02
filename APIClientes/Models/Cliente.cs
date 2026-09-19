using System.ComponentModel.DataAnnotations;

namespace APIClientes.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; }
    }
}
