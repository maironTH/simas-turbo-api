using SimasTurbo.Models;

namespace SimasTurbo.Dto
{
    public class UsuarioMostrarDto
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }

        public FuncaoUsuario Funcao { get; set; }

        public string? Token { get; set; }
    }
}