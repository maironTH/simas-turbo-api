using SimasTurbo.Dto;
using SimasTurbo.Models;

namespace SimasTurbo.Services
{
    public interface IAuthInterface
    {
        Task<ModeloResposta<List<UsuarioMostrarDto>>> ListarUsuarios();
        Task<ModeloResposta<UsuarioMostrarDto>> GetUsuarioEmail(string email);
        Task<ModeloResposta<UsuarioMostrarDto>> LoginUsuario(UsuarioLoginDto usuarioLoginDto);
        Task<ModeloResposta<UsuarioMostrarDto>> CadastrarUsuario(UsuarioCadastrarDto usuarioCadastrarDto);
        Task<ModeloResposta<UsuarioMostrarDto>> AtualizarUsuario(Guid id, UsuarioAtualizaDto usuarioAtualizaDto);
        Task<ModeloResposta<UsuarioMostrarDto>> DeletarUsuario(Guid id);
    }
}