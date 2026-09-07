using Microsoft.AspNetCore.Mvc;
using SimasTurbo.Services;
using SimasTurbo.Dto;

namespace SimasTurbo.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteInterface _clienteInterface;

        public ClienteController(IClienteInterface clienteInterface)
        {
            _clienteInterface = clienteInterface;
        }
        [HttpGet]
        public async Task<IActionResult> ListarClientes()
        {
            var resposta = await _clienteInterface.ListarClientes();

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpGet("{cpf}")]
        public async Task<IActionResult> GetClienteCpf(string cpf)
        {
            var resposta = await _clienteInterface.GetClienteCpf(cpf);

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarCliente(ClienteCadastrarDto clienteCadastrarDto)
        {
            var resposta = await _clienteInterface.CadastrarCliente(clienteCadastrarDto);

            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> AtualizarCliente(Guid id, ClienteAtualizarDto clienteAtualizarDto)
        {
            var resposta = await _clienteInterface.AtualizarCliente(id, clienteAtualizarDto);
            
            return StatusCode(resposta.StatusCode, resposta);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarCliente(Guid id)
        {
            var resposta = await _clienteInterface.DeletarCliente(id);  

            return StatusCode(resposta.StatusCode, resposta);   
        }
    }
}
