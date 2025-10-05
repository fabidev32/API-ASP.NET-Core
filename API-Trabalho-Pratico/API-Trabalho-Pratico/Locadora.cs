using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_Trabalho_Pratico
{
    public class Fabricante
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do fabricante é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome do fabricante não pode ter mais que 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Veiculo>? Veiculos { get; set; }
    }


    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O modelo do veículo é obrigatório.")]
        [StringLength(100, ErrorMessage = "O modelo do veículo não pode ter mais que 100 caracteres.")]
        public string Modelo { get; set; } = string.Empty;

        [Range(1886, 2100, ErrorMessage = "O ano deve estar entre 1886 e 2100.")]
        public int Ano { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "A quilometragem deve ser maior ou igual a zero.")]
        public double Quilometragem { get; set; }

        [Required(ErrorMessage = "A placa é obrigatória.")]
        [RegularExpression(
            @"^[A-Z]{3}-?\d{4}$|^[A-Z]{3}\d[A-Z]\d{2}$",
            ErrorMessage = "A placa deve estar no formato 'AAA-1234' ou 'AAA1A23'."
        )]
        [StringLength(8, ErrorMessage = "A placa não pode ter mais que 8 caracteres.")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "O fabricante é obrigatório.")]
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

        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome do cliente não pode ter mais que 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve conter exatamente 11 caracteres.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter apenas números.")]
        public string CPF { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail não é válido.")]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Aluguel>? Alugueis { get; set; }
    }


    public class Funcionario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do funcionário é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome do funcionário não pode ter mais que 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve conter exatamente 11 caracteres.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter apenas números.")]
        public string CPF { get; set; } = string.Empty;

        [Required(ErrorMessage = "O cargo do funcionário é obrigatório.")]
        [StringLength(50, ErrorMessage = "O cargo não pode ter mais que 50 caracteres.")]
        public string Cargo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail não é válido.")]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Aluguel>? AlugueisRegistrados { get; set; }
    }


    public class Aluguel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O cliente é obrigatório.")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "O veículo é obrigatório.")]
        public int VeiculoId { get; set; }

        [Required(ErrorMessage = "O funcionário é obrigatório.")]
        public int FuncionarioId { get; set; }

        [ForeignKey(nameof(ClienteId))]
        public Cliente? Cliente { get; set; }

        [ForeignKey(nameof(VeiculoId))]
        public Veiculo? Veiculo { get; set; }

        [ForeignKey(nameof(FuncionarioId))]
        public Funcionario? Funcionario { get; set; }

        [Required(ErrorMessage = "A data de início é obrigatória.")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "A data de fim é obrigatória.")]
        [DateGreaterThan("DataInicio", ErrorMessage = "A data de fim deve ser maior ou igual à data de início.")]
        public DateTime DataFim { get; set; }

        public DateTime? DataDevolucao { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "A quilometragem inicial deve ser maior ou igual a zero.")]
        public double KmInicial { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "A quilometragem final deve ser maior ou igual a zero.")]
        [GreaterThanOrEqualTo("KmInicial", ErrorMessage = "A quilometragem final deve ser maior ou igual à inicial.")]
        public double KmFinal { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor da diária deve ser maior que zero.")]
        public decimal ValorDiaria { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor total deve ser maior que zero.")]
        public decimal ValorTotal { get; set; }
    }

    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var currentValue = (DateTime?)value;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null)
                return new ValidationResult($"Propriedade desconhecida: {_comparisonProperty}");

            var comparisonValue = (DateTime?)property.GetValue(validationContext.ObjectInstance);

            if (currentValue != null && comparisonValue != null && currentValue < comparisonValue)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }

    public class GreaterThanOrEqualToAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public GreaterThanOrEqualToAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var currentValue = Convert.ToDouble(value);

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null)
                return new ValidationResult($"Propriedade desconhecida: {_comparisonProperty}");

            var comparisonValue = Convert.ToDouble(property.GetValue(validationContext.ObjectInstance));

            if (currentValue < comparisonValue)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
