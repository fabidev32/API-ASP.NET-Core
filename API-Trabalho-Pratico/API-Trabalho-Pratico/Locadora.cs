using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API_Trabalho_Pratico
{
    public class Fabricante
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do fabricante � obrigat�rio.")]
        [StringLength(100, ErrorMessage = "O nome do fabricante n�o pode ter mais que 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Veiculo>? Veiculos { get; set; }
    }


    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O modelo do ve�culo � obrigat�rio.")]
        [StringLength(100, ErrorMessage = "O modelo do ve�culo n�o pode ter mais que 100 caracteres.")]
        public string Modelo { get; set; } = string.Empty;

        [Range(1886, 2100, ErrorMessage = "O ano deve estar entre 1886 e 2100.")]
        public int Ano { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "A quilometragem deve ser maior ou igual a zero.")]
        public double Quilometragem { get; set; }

        [Required(ErrorMessage = "A placa � obrigat�ria.")]
        [RegularExpression(
            @"^[A-Z]{3}-?\d{4}$|^[A-Z]{3}\d[A-Z]\d{2}$",
            ErrorMessage = "A placa deve estar no formato 'AAA-1234' ou 'AAA1A23'."
        )]
        [StringLength(8, ErrorMessage = "A placa n�o pode ter mais que 8 caracteres.")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "O fabricante � obrigat�rio.")]
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

        [Required(ErrorMessage = "O nome do cliente � obrigat�rio.")]
        [StringLength(100, ErrorMessage = "O nome do cliente n�o pode ter mais que 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF � obrigat�rio.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve conter exatamente 11 caracteres.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter apenas n�meros.")]
        public string CPF { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail � obrigat�rio.")]
        [EmailAddress(ErrorMessage = "O e-mail n�o � v�lido.")]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Aluguel>? Alugueis { get; set; }
    }


    public class Funcionario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do funcion�rio � obrigat�rio.")]
        [StringLength(100, ErrorMessage = "O nome do funcion�rio n�o pode ter mais que 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF � obrigat�rio.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve conter exatamente 11 caracteres.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter apenas n�meros.")]
        public string CPF { get; set; } = string.Empty;

        [Required(ErrorMessage = "O cargo do funcion�rio � obrigat�rio.")]
        [StringLength(50, ErrorMessage = "O cargo n�o pode ter mais que 50 caracteres.")]
        public string Cargo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail � obrigat�rio.")]
        [EmailAddress(ErrorMessage = "O e-mail n�o � v�lido.")]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Aluguel>? AlugueisRegistrados { get; set; }
    }


    public class Aluguel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O cliente � obrigat�rio.")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "O ve�culo � obrigat�rio.")]
        public int VeiculoId { get; set; }

        [Required(ErrorMessage = "O funcion�rio � obrigat�rio.")]
        public int FuncionarioId { get; set; }

        [ForeignKey(nameof(ClienteId))]
        public Cliente? Cliente { get; set; }

        [ForeignKey(nameof(VeiculoId))]
        public Veiculo? Veiculo { get; set; }

        [ForeignKey(nameof(FuncionarioId))]
        public Funcionario? Funcionario { get; set; }

        [Required(ErrorMessage = "A data de in�cio � obrigat�ria.")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "A data de fim � obrigat�ria.")]
        [DateGreaterThan("DataInicio", ErrorMessage = "A data de fim deve ser maior ou igual � data de in�cio.")]
        public DateTime DataFim { get; set; }

        public DateTime? DataDevolucao { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "A quilometragem inicial deve ser maior ou igual a zero.")]
        public double KmInicial { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "A quilometragem final deve ser maior ou igual a zero.")]
        [GreaterThanOrEqualTo("KmInicial", ErrorMessage = "A quilometragem final deve ser maior ou igual � inicial.")]
        public double KmFinal { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor da di�ria deve ser maior que zero.")]
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
