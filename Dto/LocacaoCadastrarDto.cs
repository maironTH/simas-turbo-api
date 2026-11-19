using System.ComponentModel.DataAnnotations;
using SimasTurbo.Models;


namespace SimasTurbo.Dto
{
    public class LocacaoCadastrarDto
    {
      [Required(ErrorMessage = "O campo CpfCliente é obrigatório.")]
      [StringLength(11, MinimumLength = 11, ErrorMessage = "O campo CpfCliente deve ter 11 caracteres.")]
      public string CpfCliente { get; set; } = string.Empty;

      [Required(ErrorMessage = "O campo PlacaVeiculo é obrigatório.")]
      [StringLength(7, MinimumLength = 7, ErrorMessage = "O campo PlacaVeiculo deve ter 7 caracteres.")]
      public string PlacaVeiculo { get; set; } = string.Empty;

      [Required(ErrorMessage = "O campo DataRetirada é obrigatório.")]
      public DateTime DataRetirada { get; set; }

      [Required(ErrorMessage = "O campo PrazoDevolucao é obrigatório.")]
      public DateOnly PrazoDevolucao { get; set; }
   }
}