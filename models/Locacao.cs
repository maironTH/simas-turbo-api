using System.ComponentModel.DataAnnotations;
namespace SimasTurbo.Models
{
   public class Locacao
   {
      public Guid Id { get; set; }

      [Required(ErrorMessage = "O campo Veículo é obrigatório.")]
      public Guid IdVeiculo { get; set; }

      [Required(ErrorMessage = "O campo Cliente é obrigatório.")]
      public Guid IdCliente { get; set; }

      [Required(ErrorMessage = "O campo Data de Retirada é obrigatório.")]
      public DateTime DataRetirada { get; set; }

      [Required(ErrorMessage = "O campo Prazo de Devolução é obrigatório.")]
      public DateOnly PrazoDevolucao { get; set; }
      public DateTime? DataDevolucao { get; set; }

      [Required(ErrorMessage = "O campo Valor Total é obrigatório.")]
      [Range(typeof(decimal), "0", "99999999999", ErrorMessage = "O campo Valor Total deve ser maior ou igual a zero.")]
      public decimal ValorTotal { get; set; }

      [Required(ErrorMessage = "O campo Status é obrigatório.")]
      public StatusLocacao Status { get; set; } = StatusLocacao.Ativo;
   }
}