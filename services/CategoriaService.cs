using Dapper;
using Npgsql;
using SimasTurbo.Models;
using SimasTurbo.Dto;

namespace SimasTurbo.Services
{
    public class CategoriaService : ICategoriaInterface
    {
        private readonly IConfiguration _configuration;
        public CategoriaService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ModeloResposta<List<Categoria>>> ListarCategorias()
        {
            var resposta = new ModeloResposta<List<Categoria>>();

            try
            {
                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string query = """
                SELECT * FROM categoria ORDER BY nome;
                """;

                var categorias = await conexao.QueryAsync<Categoria>(query);

                resposta.Dados = categorias.ToList();
                resposta.Mensagem = "Categorias listadas com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem = "Erro ao listar categorias.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        public async Task<ModeloResposta<List<CategoriaResumoDto>>> ResumoCategorias()
        {
            var resposta = new ModeloResposta<List<CategoriaResumoDto>>();

            try
            {
                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                // LEFT JOIN para que categorias sem veículos também apareçam (com total 0).
                // COUNT(...) FILTER conta só os veículos que atendem à condição.
                const string query = """
                SELECT
                    c.id AS Id,
                    c.nome AS Nome,
                    COUNT(v.id)::int AS TotalVeiculos,
                    COUNT(v.id) FILTER (WHERE v.status = 'DISPONIVEL')::int AS TotalDisponiveis,
                    COALESCE(ROUND(AVG(v.valor_diaria), 2), 0) AS MediaValorDiaria
                FROM categoria c
                LEFT JOIN veiculo v ON v.id_categoria = c.id
                GROUP BY c.id, c.nome
                ORDER BY c.nome;
                """;

                var resumo = await conexao.QueryAsync<CategoriaResumoDto>(query);

                resposta.Dados = resumo.ToList();
                resposta.Mensagem = "Resumo das categorias gerado com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem = "Erro ao gerar o resumo das categorias.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        public async Task<ModeloResposta<Categoria>> GetCategoriaNome(string nome)
        {
            var resposta = new ModeloResposta<Categoria>();

            try
            {
                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string query = """
                SELECT * FROM categoria WHERE nome = @Nome LIMIT 1;
                """;

                var categoria = await conexao.QuerySingleOrDefaultAsync<Categoria>(query, new { Nome = nome });

                if (categoria == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Categoria não encontrada.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                }
                else
                {
                    resposta.Dados = categoria;
                    resposta.Mensagem = "Categoria encontrada com sucesso.";
                    resposta.IsSucesso = true;
                    resposta.StatusCode = 200;
                }
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem = "Erro ao buscar categoria.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        public async Task<ModeloResposta<Categoria>> CadastrarCategoria(CategoriaCadastrarDto categoriaCadastrarDto)
        {
            var resposta = new ModeloResposta<Categoria>();

            try
            {
                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string sqlVerificaDuplicado = """
                SELECT 1 FROM categoria WHERE nome = @Nome LIMIT 1;
                """;

                var nomeDuplicado = await conexao.ExecuteScalarAsync<int?>(sqlVerificaDuplicado, new { categoriaCadastrarDto.Nome });

                if (nomeDuplicado != null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Já existe uma categoria com esse nome.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                var novaCategoria = new Categoria
                {
                    Id = Guid.NewGuid(),
                    Nome = categoriaCadastrarDto.Nome,
                    Descricao = categoriaCadastrarDto.Descricao,
                    CapacidadePassageiros = categoriaCadastrarDto.CapacidadePassageiros
                };

                const string query = """
                INSERT INTO categoria (id, nome, descricao, capacidade_passageiros)
                VALUES (@Id, @Nome, @Descricao, @CapacidadePassageiros);
                """;

                await conexao.ExecuteAsync(query, novaCategoria);

                resposta.Dados = novaCategoria;
                resposta.Mensagem = "Categoria cadastrada com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 201;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem = "Erro ao cadastrar categoria.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        public async Task<ModeloResposta<Categoria>> AtualizarCategoria(Guid id, CategoriaAtualizarDto categoriaAtualizarDto)
        {
            var resposta = new ModeloResposta<Categoria>();

            try
            {
                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string queryBusca = """
                SELECT * FROM categoria WHERE id = @Id LIMIT 1;
                """;

                var categoriaExistente = await conexao.QuerySingleOrDefaultAsync<Categoria>(queryBusca, new { Id = id });

                if (categoriaExistente == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Categoria não encontrada.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }

                if (!string.IsNullOrWhiteSpace(categoriaAtualizarDto.Nome) && categoriaAtualizarDto.Nome != categoriaExistente.Nome)
                {
                    const string queryVerificaNome = """
                    SELECT 1 FROM categoria WHERE nome = @Nome AND id != @Id LIMIT 1;
                    """;

                    var nomeDuplicado = await conexao.ExecuteScalarAsync<int?>(queryVerificaNome, new { Nome = categoriaAtualizarDto.Nome, Id = id });

                    if (nomeDuplicado != null)
                    {
                        resposta.Dados = null;
                        resposta.Mensagem = "Já existe outra categoria com esse nome.";
                        resposta.IsSucesso = false;
                        resposta.StatusCode = 400;
                        return resposta;
                    }

                    categoriaExistente.Nome = categoriaAtualizarDto.Nome;
                }

                categoriaExistente.Descricao = categoriaAtualizarDto.Descricao ?? categoriaExistente.Descricao;
                categoriaExistente.CapacidadePassageiros = categoriaAtualizarDto.CapacidadePassageiros ?? categoriaExistente.CapacidadePassageiros;

                const string queryAtualiza = """
                UPDATE categoria
                SET nome = @Nome,
                    descricao = @Descricao,
                    capacidade_passageiros = @CapacidadePassageiros
                WHERE id = @Id;
                """;

                await conexao.ExecuteAsync(queryAtualiza, new
                {
                    Id = categoriaExistente.Id,
                    Nome = categoriaExistente.Nome,
                    Descricao = categoriaExistente.Descricao,
                    CapacidadePassageiros = categoriaExistente.CapacidadePassageiros
                });

                resposta.Dados = categoriaExistente;
                resposta.Mensagem = "Categoria atualizada com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem = "Erro ao atualizar categoria.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        public async Task<ModeloResposta<Categoria>> DeletarCategoria(Guid id)
        {
            var resposta = new ModeloResposta<Categoria>();

            try
            {
                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string queryBusca = """
                SELECT * FROM categoria WHERE id = @Id LIMIT 1;
                """;

                var categoriaExistente = await conexao.QuerySingleOrDefaultAsync<Categoria>(queryBusca, new { Id = id });

                if (categoriaExistente == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Categoria não encontrada.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }

                const string queryVerificaVeiculos = """
                SELECT 1 FROM veiculo WHERE id_categoria = @Id LIMIT 1;
                """;

                var possuiVeiculos = await conexao.ExecuteScalarAsync<int?>(queryVerificaVeiculos, new { Id = id });

                if (possuiVeiculos != null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Não é possível deletar a categoria, pois ela possui veículos associados.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                const string queryDeleta = """
                DELETE FROM categoria WHERE id = @Id;
                """;

                await conexao.ExecuteAsync(queryDeleta, new { Id = id });

                resposta.Dados = categoriaExistente;
                resposta.Mensagem = "Categoria deletada com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem = "Erro ao deletar categoria.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }
    }
}
