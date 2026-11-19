using Microsoft.AspNetCore.Mvc;
using SimasTurbo.Services;
using SimasTurbo.Dto;
using Microsoft.AspNetCore.Authorization;

namespace SimasTurbo.Controllers
{
    [Authorize(Roles = "ADMIN,FUNCIONARIO")]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaInterface _categoriaInterface;

        public CategoriaController(ICategoriaInterface categoriaInterface)
        {
            _categoriaInterface = categoriaInterface;
        }

        [HttpGet]
        public async Task<IActionResult> ListarCategorias()
        {
            var resposta = await _categoriaInterface.ListarCategorias();

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpGet("resumo")]
        public async Task<IActionResult> ResumoCategorias()
        {
            var resposta = await _categoriaInterface.ResumoCategorias();

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpGet("{nome}")]
        public async Task<IActionResult> GetCategoriaNome(string nome)
        {
            var resposta = await _categoriaInterface.GetCategoriaNome(nome);

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarCategoria(CategoriaCadastrarDto categoriaCadastrarDto)
        {
            var resposta = await _categoriaInterface.CadastrarCategoria(categoriaCadastrarDto);

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> AtualizarCategoria(Guid id, CategoriaAtualizarDto categoriaAtualizarDto)
        {
            var resposta = await _categoriaInterface.AtualizarCategoria(id, categoriaAtualizarDto);

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarCategoria(Guid id)
        {
            var resposta = await _categoriaInterface.DeletarCategoria(id);

            return StatusCode(resposta.StatusCode, resposta);
        }
    }
}
