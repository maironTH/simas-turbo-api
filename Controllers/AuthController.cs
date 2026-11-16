using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SimasTurbo.Dto;
using SimasTurbo.Services;

namespace SimasTurbo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthInterface _authInterface;

        public AuthController(IAuthInterface authInterface)
        {
            _authInterface = authInterface;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ListarUsuarios()
        {
            var resposta = await _authInterface.ListarUsuarios();
            return StatusCode(resposta.StatusCode, resposta);
        }

        [Authorize]
        [HttpGet("{email}")]
        public async Task<IActionResult> GetUsuarioEmail(string email)
        {
            var resposta = await _authInterface.GetUsuarioEmail(email);
            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDto usuarioLoginDto)
        {
            var resposta = await _authInterface.LoginUsuario(usuarioLoginDto);

            if (!resposta.IsSucesso)
            {
                return StatusCode(resposta.StatusCode, resposta);
            }

            return Ok(resposta);
        }

        [HttpPost("cadastrar")]
        public async Task<IActionResult> CadastrarUsuario([FromBody] UsuarioCadastrarDto usuarioCadastrarDto)
        {
            var resposta = await _authInterface.CadastrarUsuario(usuarioCadastrarDto);

            return StatusCode(resposta.StatusCode, resposta);
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> AtualizarUsuario(Guid id, [FromBody] UsuarioAtualizaDto usuarioAtualizaDto)
        {
            var usuarioAutenticadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ehAdministrador = User.IsInRole("ADMIN");

            if (!ehAdministrador &&
                (!Guid.TryParse(usuarioAutenticadoId, out var idAutenticado) || idAutenticado != id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    Mensagem = "Você só pode atualizar o próprio usuário.",
                    IsSucesso = false,
                    StatusCode = StatusCodes.Status403Forbidden
                });
            }

            if (!string.IsNullOrWhiteSpace(usuarioAtualizaDto.Funcao) && !ehAdministrador)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    Mensagem = "Apenas usuários ADMIN podem alterar a função de um usuário.",
                    IsSucesso = false,
                    StatusCode = StatusCodes.Status403Forbidden
                });
            }

            var resposta = await _authInterface.AtualizarUsuario(id, usuarioAtualizaDto);

            return StatusCode(resposta.StatusCode, resposta);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarUsuario(Guid id)
        {
            var resposta = await _authInterface.DeletarUsuario(id);

            return StatusCode(resposta.StatusCode, resposta);
        }
    }
}