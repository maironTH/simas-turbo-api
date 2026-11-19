using System.ComponentModel.DataAnnotations;

namespace SimasTurbo.Dto
{
    public class CategoriaCadastrarDto
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(50, ErrorMessage = "O campo Nome deve ter no máximo 50 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "O campo Descrição deve ter no máximo 200 caracteres.")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "O campo Capacidade de Passageiros é obrigatório.")]
        [Range(1, 60, ErrorMessage = "A capacidade de passageiros deve estar entre 1 e 60.")]
        public short CapacidadePassageiros { get; set; }
    }
}
