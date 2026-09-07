using Microsoft.AspNetCore.Mvc;
using SimasTurbo.Services;
using SimasTurbo.Dto;


namespace SimasTurbo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VeiculoController : ControllerBase
    {
        private readonly IVeiculoInterface _veiculoInterface;

        public VeiculoController(IVeiculoInterface veiculoInterface)
        {
            _veiculoInterface = veiculoInterface;
        }
        
        [HttpGet]
        public async Task<IActionResult> ListarVeiculos()
        {
            var resposta = await _veiculoInterface.ListarVeiculos();
            
            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpGet("{placa}")]
        public async Task<IActionResult> GetVeiculoPlaca(string placa)
        {
            var resposta = await _veiculoInterface.GetVeiculoPlaca(placa);

            return StatusCode(resposta.StatusCode, resposta);
        }
        
        [HttpPost]
        public async Task<IActionResult> CadastrarVeiculo(VeiculoCadastrarDto veiculoCadastrar)
        {
            var resposta = await _veiculoInterface.CadastrarVeiculo(veiculoCadastrar);

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> AtualizarVeiculo(Guid id, VeiculoAtualizarDto veiculoAtualizarDto)
        {
            var resposta = await _veiculoInterface.AtualizarVeiculo(id, veiculoAtualizarDto);

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarVeiculo(Guid id)
        {
            var resposta = await _veiculoInterface.DeletarVeiculo(id);

            if (!resposta.IsSucesso)
            {
                return StatusCode(resposta.StatusCode, resposta);
            }
            return StatusCode(resposta.StatusCode, resposta);
        }
    }
}
