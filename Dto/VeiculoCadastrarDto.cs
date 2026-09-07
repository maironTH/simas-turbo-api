using System.ComponentModel.DataAnnotations;

namespace SimasTurbo.Dto
{
    public class VeiculoCadastrarDto
    {
        [Required(ErrorMessage = "O campo Placa é obrigatório.")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "O campo Placa deve ter 7 caracteres.")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Marca é obrigatório.")]
        [StringLength(50, ErrorMessage = "O campo Marca deve ter no máximo 50 caracteres.")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Modelo é obrigatório.")]
        [StringLength(50, ErrorMessage = "O campo Modelo deve ter no máximo 50 caracteres.")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Ano é obrigatório.")]
        [Range(1886, 2100, ErrorMessage = "O campo Ano deve estar entre 1886 e 2100.")]
        public short Ano { get; set; }

        [Required(ErrorMessage = "O campo Valor da Diária é obrigatório.")]
        [Range(typeof(decimal), "0", "99999999999", ErrorMessage = "O campo Valor da Diária deve ser maior ou igual a zero.")]
        public decimal ValorDiaria { get; set; }
    }
}