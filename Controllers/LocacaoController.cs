using Microsoft.AspNetCore.Mvc;
using SimasTurbo.Dto;
using SimasTurbo.Services;
using Microsoft.AspNetCore.Authorization;

namespace SimasTurbo.Controllers
{
    [Authorize(Roles = "ADMIN,FUNCIONARIO")]
    [ApiController]
    [Route("api/[controller]")]
    public class LocacaoController : ControllerBase
    {
        private readonly ILocacaoInterface _locacaoInterface;
        public LocacaoController(ILocacaoInterface locacaoInterface)
        {
            _locacaoInterface = locacaoInterface;
        }

        [HttpGet]
        public async Task<IActionResult> ListarLocacao()
        {
            var resposta = await _locacaoInterface.ListarLocacao();

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpGet("cliente/{cpfCliente}")]
        public async Task<IActionResult> GetLocacaoCliente(string cpfCliente)
        {
            var resposta = await _locacaoInterface.GetLocacaoCliente(cpfCliente);   
            
            return StatusCode(resposta.StatusCode, resposta);

        }

        [HttpGet("veiculo/{placaVeiculo}")]
        public async Task<IActionResult> GetLocacaoVeiculo(string placaVeiculo)
        {
            var resposta = await _locacaoInterface.GetLocacaoVeiculo(placaVeiculo);

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarLocacao([FromBody] LocacaoCadastrarDto locacaocadastrarDto)
        {
            var resposta = await _locacaoInterface.CadastrarLocacao(locacaocadastrarDto);

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> DevolverLocacao(Guid id)
        {
            var resposta = await _locacaoInterface.DevolverLocacao(id);

            return StatusCode(resposta.StatusCode, resposta);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarLocacao(Guid id)
        {
            var resposta = await _locacaoInterface.DeletarLocacao(id);

            return StatusCode(resposta.StatusCode, resposta);
        }
    }
}