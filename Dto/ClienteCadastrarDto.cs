using System.ComponentModel.DataAnnotations;
namespace SimasTurbo.Dto
{
    public class ClienteCadastrarDto
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(150, ErrorMessage = "O campo Nome deve ter no máximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Data de Nascimento é obrigatório.")]
        public DateOnly DataNascimento { get; set; }
        
        [Required(ErrorMessage = "O campo Telefone é obrigatório.")]
        [StringLength(15, ErrorMessage = "O campo Telefone deve ter no máximo 15 caracteres.")]
        public string Telefone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O campo Email é obrigatório.")]    
        [StringLength(100, ErrorMessage = "O campo Email deve ter no máximo 100 caracteres.")]   
        [EmailAddress(ErrorMessage = "O campo Email deve ser um endereço de email válido.")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O campo CPF é obrigatório.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O campo CPF deve ter no máximo 11 caracteres.")]
        public string Cpf { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O campo CNH é obrigatório.")]
        [StringLength(15, ErrorMessage = "O campo CNH deve ter no máximo 15 caracteres.")]
        public string Cnh { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O campo CEP é obrigatório.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "O campo CEP deve ter apenas os 8 números.")]
        public string Cep { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O campo UF é obrigatório.")]
        [StringLength(2, ErrorMessage = "O campo UF deve ter no máximo 2 caracteres.")]
        public string UF { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Cidade é obrigatório.")]
        [StringLength(100, ErrorMessage = "O campo Cidade deve ter no máximo 100 caracteres.")]       
        public string Cidade { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O campo Bairro é obrigatório.")]
        [StringLength(100, ErrorMessage = "O campo Bairro deve ter no máximo 100 caracteres.")] 
        public string Bairro { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O campo Logradouro é obrigatório.")]
        [StringLength(100, ErrorMessage = "O campo Logradouro deve ter no máximo 100 caracteres.")]
        public string Logradouro { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O campo Número é obrigatório.")]
        [StringLength(10, ErrorMessage = "O campo Número deve ter no máximo 10 caracteres.")]
        public string Numero { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "O campo Complemento deve ter no máximo 100 caracteres.")]
        public string? Complemento { get; set; }
    }
}