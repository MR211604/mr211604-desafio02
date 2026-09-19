using Microsoft.EntityFrameworkCore;

namespace APIClientes.Models
{
    public class ClientesDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Cliente> Clientes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Cliente>().HasData(
                 new Cliente
                 {
                     Id = 1,
                     Nombre = "Juan",
                     Apellido = "Perez"
                 },
                 new Cliente
                 {
                     Id = 2,
                     Nombre = "Ana",
                     Apellido = "Perez"
                 },
                 new Cliente
                 {
                     Id = 3,
                     Nombre = "Maria",
                     Apellido = "Perez"
                 }
            );
        }
    }
}
