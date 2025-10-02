using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_Trabalho_Pratico
{
    public class Fabricante
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Veiculo>? Veiculos { get; set; }
    }

    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Modelo { get; set; } = string.Empty;

        public int Ano { get; set; }

        public double Quilometragem { get; set; }

        public int FabricanteId { get; set; }

        [ForeignKey(nameof(FabricanteId))]
        public Fabricante? Fabricante { get; set; }

        [JsonIgnore]
        public ICollection<Aluguel>? Alugueis { get; set; }
    }

    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(11)]
        public string CPF { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Aluguel>? Alugueis { get; set; }
    }

    public class Funcionario
    {
        [Key]
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Aluguel>? AlugueisRegistrados { get; set; }
    }

    public class Aluguel
    {
        [Key]
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public int VeiculoId { get; set; }
        public int FuncionarioId { get; set; }

        [ForeignKey(nameof(ClienteId))]
        public Cliente? Cliente { get; set; }

        [ForeignKey(nameof(VeiculoId))]
        public Veiculo? Veiculo { get; set; }

        [ForeignKey(nameof(FuncionarioId))]
        public Funcionario? Funcionario { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public DateTime? DataDevolucao { get; set; }

        public double KmInicial { get; set; }
        public double KmFinal { get; set; }

        public decimal ValorDiaria { get; set; }
        public decimal ValorTotal { get; set; }
    }
}


