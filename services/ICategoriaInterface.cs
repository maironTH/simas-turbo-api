using SimasTurbo.Models;
using SimasTurbo.Dto;

namespace SimasTurbo.Services
{
    public interface ICategoriaInterface
    {
        Task<ModeloResposta<List<Categoria>>> ListarCategorias();

        Task<ModeloResposta<List<CategoriaResumoDto>>> ResumoCategorias();

        Task<ModeloResposta<Categoria>> GetCategoriaNome(string nome);

        Task<ModeloResposta<Categoria>> CadastrarCategoria(CategoriaCadastrarDto categoriaCadastrarDto);

        Task<ModeloResposta<Categoria>> AtualizarCategoria(Guid id, CategoriaAtualizarDto categoriaAtualizarDto);

        Task<ModeloResposta<Categoria>> DeletarCategoria(Guid id);
    }
}
