using Microsoft.AspNetCore.Mvc;
using SimasTurbo.Services;
using Microsoft.AspNetCore.Authorization;

namespace SimasTurbo.Controllers
{
    [Authorize(Roles = "ADMIN,FUNCIONARIO")]
    [ApiController]
    [Route("api/[controller]")]
    public class RelatorioController : ControllerBase
    {
        private readonly IRelatorioInterface _relatorioInterface;

        public RelatorioController(IRelatorioInterface relatorioInterface)
        {
            _relatorioInterface = relatorioInterface;
        }

        // Relatório 1: resumo da frota por status (padrão: DISPONIVEL)
        [HttpGet("veiculos/resumo")]
        public async Task<IActionResult> ResumoVeiculos([FromQuery] string status = "DISPONIVEL")
        {
            var resposta = await _relatorioInterface.ResumoVeiculos(status);

            return StatusCode(resposta.StatusCode, resposta);
        }

        // Relatório 2: busca de clientes pelo início do nome
        [HttpGet("clientes/busca")]
        public async Task<IActionResult> BuscaClientes([FromQuery] string nome)
        {
            var resposta = await _relatorioInterface.BuscaClientes(nome);

            return StatusCode(resposta.StatusCode, resposta);
        }

        // Relatório 3: locações e faturamento por categoria (JOIN)
        [HttpGet("locacoes/por-categoria")]
        public async Task<IActionResult> LocacoesPorCategoria()
        {
            var resposta = await _relatorioInterface.LocacoesPorCategoria();

            return StatusCode(resposta.StatusCode, resposta);
        }
    }
}
