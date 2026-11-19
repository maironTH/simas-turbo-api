using SimasTurbo.Models;
using SimasTurbo.Dto;

namespace SimasTurbo.Services
{
    public interface IRelatorioInterface
    {
        Task<ModeloResposta<RelatorioVeiculosResumoDto>> ResumoVeiculos(string status);

        Task<ModeloResposta<List<RelatorioClienteBuscaDto>>> BuscaClientes(string nome);

        Task<ModeloResposta<List<RelatorioLocacaoCategoriaDto>>> LocacoesPorCategoria();
    }
}
