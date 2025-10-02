using API;
using API_Trabalho_Pratico; 
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
    }
}
