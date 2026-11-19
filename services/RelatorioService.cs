using Dapper;
using Npgsql;
using SimasTurbo.Models;
using SimasTurbo.Dto;

namespace SimasTurbo.Services
{
    public class RelatorioService : IRelatorioInterface
    {
        private readonly IConfiguration _configuration;

        private static readonly string[] StatusValidos = { "DISPONIVEL", "INDISPONIVEL", "ALUGADO", "INATIVO" };

        public RelatorioService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Relatório 1: WHERE + COUNT, MIN, MAX e AVG
        public async Task<ModeloResposta<RelatorioVeiculosResumoDto>> ResumoVeiculos(string status)
        {
            var resposta = new ModeloResposta<RelatorioVeiculosResumoDto>();

            try
            {
                var statusNormalizado = (status ?? string.Empty).Trim().ToUpper();

                if (!StatusValidos.Contains(statusNormalizado))
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Status inválido. Use: DISPONIVEL, INDISPONIVEL, ALUGADO ou INATIVO.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string query = """
                SELECT
                    COUNT(*)::int AS Quantidade,
                    COALESCE(MIN(valor_diaria), 0) AS MenorDiaria,
                    COALESCE(MAX(valor_diaria), 0) AS MaiorDiaria,
                    COALESCE(ROUND(AVG(valor_diaria), 2), 0) AS MediaDiaria
                FROM veiculo
                WHERE status = @Status::status_disponibilidade;
                """;

                var resumo = await conexao.QuerySingleAsync<RelatorioVeiculosResumoDto>(query, new { Status = statusNormalizado });

                resposta.Dados = resumo;
                resposta.Mensagem = $"Resumo dos veículos com status {statusNormalizado} gerado com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem = "Erro ao gerar o resumo dos veículos.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        // Relatório 2: busca por padrão com ILIKE
        public async Task<ModeloResposta<List<RelatorioClienteBuscaDto>>> BuscaClientes(string nome)
        {
            var resposta = new ModeloResposta<List<RelatorioClienteBuscaDto>>();

            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Informe o parâmetro 'nome' para a busca.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                // Escapa curingas digitados pelo usuário e acrescenta o % do "começa com".
                var padrao = nome.Trim()
                    .Replace("\\", "\\\\")
                    .Replace("%", "\\%")
                    .Replace("_", "\\_") + "%";

                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string query = """
                SELECT nome AS Nome, cpf AS Cpf, cidade AS Cidade, uf AS Uf, telefone AS Telefone
                FROM cliente
                WHERE nome ILIKE @Padrao
                ORDER BY nome;
                """;

                var clientes = await conexao.QueryAsync<RelatorioClienteBuscaDto>(query, new { Padrao = padrao });

                resposta.Dados = clientes.ToList();
                resposta.Mensagem = "Busca de clientes realizada com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem = "Erro ao buscar clientes.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        // Relatório 3: dois INNER JOIN (locacao -> veiculo -> categoria) + GROUP BY
        public async Task<ModeloResposta<List<RelatorioLocacaoCategoriaDto>>> LocacoesPorCategoria()
        {
            var resposta = new ModeloResposta<List<RelatorioLocacaoCategoriaDto>>();

            try
            {
                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string query = """
                SELECT
                    c.nome AS Categoria,
                    COUNT(l.id)::int AS TotalLocacoes,
                    COALESCE(SUM(l.valor_total), 0) AS Faturamento
                FROM locacao l
                INNER JOIN veiculo v ON v.id = l.id_veiculo
                INNER JOIN categoria c ON c.id = v.id_categoria
                WHERE l.status <> 'CANCELADO'
                GROUP BY c.nome
                ORDER BY Faturamento DESC;
                """;

                var relatorio = await conexao.QueryAsync<RelatorioLocacaoCategoriaDto>(query);

                resposta.Dados = relatorio.ToList();
                resposta.Mensagem = "Relatório de locações por categoria gerado com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem = "Erro ao gerar o relatório de locações por categoria.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }
    }
}
