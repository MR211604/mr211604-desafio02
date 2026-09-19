using Microsoft.EntityFrameworkCore;

namespace APIPedidos.Models
{
    public class PedidosDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Pedido> Pedidos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Pedido>().HasData(
                 new Pedido
                 {
                     Id = 1,
                     Nombre = "Juan",
                     Descripcion = "Esto es el 1er pedido",
                     ClienteId = 1
                 },
                 new Pedido
                 {
                     Id = 2,
                     Nombre = "Ana",
                     Descripcion = "Esto es el 2do pedido",
                     ClienteId = 2
                 },
                 new Pedido
                 {
                     Id = 3,
                     Nombre = "Maria",
                     Descripcion = "Esto es el 3er pedido",
                     ClienteId = 3
                 }
            );
        }
    }
}
