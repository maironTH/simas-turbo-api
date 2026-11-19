using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimasTurbo.Models
{
    public class Usuario
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do e-mail é inválido.")]
        [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public required string Senha { get; set; }

        [Required(ErrorMessage = "A função é obrigatória.")]
        public FuncaoUsuario Funcao { get; set; }
    }
}