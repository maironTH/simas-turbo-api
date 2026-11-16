using SimasTurbo.Dto;
using SimasTurbo.Models;

namespace SimasTurbo.Services
{
    public interface ILocacaoInterface
    {
        Task<ModeloResposta<List<LocacaoListarDto>>> ListarLocacao();

        Task<ModeloResposta<List<LocacaoListarDto>>> GetLocacaoCliente(string cpfCliente);

        Task<ModeloResposta<List<LocacaoListarDto>>> GetLocacaoVeiculo(string placaVeiculo);

        Task<ModeloResposta<Locacao>> CadastrarLocacao(LocacaoCadastrarDto locacaoCadastrarDto);

        Task<ModeloResposta<Locacao>> DevolverLocacao(Guid id);

        Task<ModeloResposta<Locacao>> DeletarLocacao(Guid id);
    }
}