using Microsoft.EntityFrameworkCore;

namespace API_Trabalho_Pratico
{
    public class LocadoraDB : DbContext
    {
        public LocadoraDB(DbContextOptions<LocadoraDB> options) : base(options) { }

        public DbSet<Fabricante> Fabricantes { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Aluguel> Alugueis { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    @"Server=ELIANA\SQLEXPRESS;Database=LocadoraDB;Trusted_Connection=True;TrustServerCertificate=True");
            }
        }

        //garante que o funcionário seja único
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Funcionario>()
                .HasIndex(f => f.CPF)
                .IsUnique();
        }
    }
}
