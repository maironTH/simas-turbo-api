using SimasTurbo.Models;
using SimasTurbo.Dto;

namespace SimasTurbo.Services
{
    public interface IClienteInterface
    {
        Task<ModeloResposta<List<ClienteListarDto>>> ListarClientes();

        Task<ModeloResposta<ClienteListarDto>> GetClienteCpf(string cpf);

        Task<ModeloResposta<Cliente>> CadastrarCliente(ClienteCadastrarDto clienteCadastrarDto);

        Task<ModeloResposta<Cliente>> AtualizarCliente(Guid Id, ClienteAtualizarDto clienteAtualizarDto);

        Task<ModeloResposta<Cliente>> DeletarCliente(Guid Id);
    }
}