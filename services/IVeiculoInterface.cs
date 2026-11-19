using SimasTurbo.Models;
using SimasTurbo.Dto;

namespace SimasTurbo.Services
{
    public interface IVeiculoInterface
    {
        Task<ModeloResposta<List<Veiculo>>> ListarVeiculos();

        Task<ModeloResposta<Veiculo>> GetVeiculoPlaca(string placa);

        Task<ModeloResposta<Veiculo>> CadastrarVeiculo(VeiculoCadastrarDto Dto);

        Task<ModeloResposta<Veiculo>> AtualizarVeiculo(Guid id, VeiculoAtualizarDto veiculoAtualizarDto);

        Task<ModeloResposta<Veiculo>> DeletarVeiculo(Guid id);
    }
}