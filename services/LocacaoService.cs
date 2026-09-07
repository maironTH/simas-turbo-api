using Npgsql;
using Dapper;
using SimasTurbo.Models;
using SimasTurbo.Dto;

namespace SimasTurbo.Services
{
    public class LocacaoService : ILocacaoInterface
    {
        private readonly IConfiguration _configuration;
        public LocacaoService(IConfiguration configuration)
        {
            _configuration = configuration; 
        }

        public async Task<ModeloResposta<List<LocacaoListarDto>>> ListarLocacao()
        {
            var resposta = new ModeloResposta<List<LocacaoListarDto>>();

            try
            {
                var stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);


                
                var query = """
                Select * from locacao
                """;

                var locacao = await conexao.QueryAsync<LocacaoListarDto>(query);
                
                if (locacao == null)
                {
                 
                    resposta.Dados = null;
                    resposta.Mensagem = "Nenhuma locação encontrada.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }
                
                resposta.Dados = locacao.ToList();
                resposta.Mensagem = "Locações Listadas com Sucesso";
            }catch (Exception e)
            {
                resposta.Dados = null;
                resposta.Mensagem = $"Erro ao Listar as Locações: {e.Message}";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }
            return resposta;
        }

        public async Task<ModeloResposta<List<LocacaoListarDto>>> GetLocacaoCliente(string cpfCliente)
        {
            var resposta = new ModeloResposta<List<LocacaoListarDto>>();

            try
            {
                var stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string query = """
                SELECT
                    l.id AS Id,
                    c.nome AS NomeCliente,
                    c.cpf AS CpfCliente,
                    v.placa AS PlacaVeiculo,
                    l.data_retirada AS DataRetirada,
                    l.prazo_devolucao AS PrazoDevolucao,
                    l.data_devolucao AS DataDevolucao,
                    l.valor_total AS ValorTotal
                FROM locacao l
                INNER JOIN cliente c ON l.id_cliente = c.id
                INNER JOIN veiculo v ON l.id_veiculo = v.id
                WHERE c.cpf = @CpfCliente;
                """;

                var locacoes = await conexao.QueryAsync<LocacaoListarDto>(query, new { CpfCliente = cpfCliente });
                resposta.Dados = locacoes.ToList();
                resposta.Mensagem = "Locações do cliente listadas com sucesso.";
                resposta.StatusCode = 200;
            }
            catch (Exception e)
            {
                resposta.Dados = null;
                resposta.Mensagem = $"Erro ao listar as locações do cliente: {e.Message}";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }
            return resposta;
        }

        public async Task<ModeloResposta<List<LocacaoListarDto>>> GetLocacaoVeiculo(string placaVeiculo)
        {
            var resposta = new ModeloResposta<List<LocacaoListarDto>>();

            try
            {
                var stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string query = """
                SELECT 
                l.id AS Id,
                c.nome AS NomeCliente,
                c.cpf AS CpfCliente,
                v.placa AS PlacaVeiculo,
                l.data_retirada AS DataRetirada,
                l.prazo_devolucao AS PrazoDevolucao,
                l.data_devolucao AS DataDevolucao,
                l.valor_total AS ValorTotal
                FROM locacao l
                INNER JOIN veiculo v ON l.id_veiculo = v.id
                INNER JOIN cliente c ON  l.id_cliente = c.id
                WHERE v.placa = @PlacaVeiculo;
                """;

                var locacoes = await conexao.QueryAsync<LocacaoListarDto>(query, new { PlacaVeiculo = placaVeiculo});
                resposta.Dados = locacoes.ToList();
                resposta.Mensagem = "Locações do veículo listadas com sucesso.";
                resposta.StatusCode = 200;
            }
            catch (Exception e)
            {
                resposta.Dados = null;
                resposta.Mensagem = $"Erro ao listar as locações do veículo: {e.Message}";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }
            return resposta;
        }


        public async Task<ModeloResposta<Locacao>> CadastrarLocacao(LocacaoCadastrarDto locacaoCadastrarDto)
        {
            var resposta = new ModeloResposta<Locacao>();

            try
            {
                var stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                await conexao.OpenAsync();

                const string SqlClienteId = """
                SELECT id FROM cliente WHERE cpf = @CpfCliente;
                """;

                var clienteId = await conexao.ExecuteScalarAsync<Guid?>(SqlClienteId, new { locacaoCadastrarDto.CpfCliente });

                if (clienteId == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Cliente não encontrado.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }
                
                const string SqlVeiculoId = """
                SELECT id, status, valor_diaria as valorDiaria FROM veiculo WHERE placa = @PlacaVeiculo LIMIT 1;
                """;

                var veiculo = await conexao.QueryFirstOrDefaultAsync<Veiculo>(SqlVeiculoId, new { locacaoCadastrarDto.PlacaVeiculo });
                
                if (veiculo == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Veículo não encontrado.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }

                if (veiculo.Status != StatusVeiculo.Disponivel)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Veículo não está disponível para locação.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }



                var dataRetirada = DateTime.Now; 
                var dataRetiradaDateOnly = DateOnly.FromDateTime(dataRetirada);
                   

                if (locacaoCadastrarDto.PrazoDevolucao <= dataRetiradaDateOnly)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Prazo de devolução inválido.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                int diasLocacao = locacaoCadastrarDto.PrazoDevolucao.DayNumber - dataRetiradaDateOnly.DayNumber;
                Decimal valorTotal = diasLocacao * veiculo.ValorDiaria;

                var novaLocacao = new Locacao
                {
                    Id = Guid.NewGuid(),
                    IdCliente = clienteId.Value,
                    IdVeiculo = veiculo.Id,
                    DataRetirada = dataRetirada,
                    PrazoDevolucao = locacaoCadastrarDto.PrazoDevolucao,
                    ValorTotal = valorTotal,
                };

                using var transacao = await conexao.BeginTransactionAsync();

                try
                {
                    
                const string query = """
                INSERT INTO locacao (id, id_cliente, id_veiculo, data_retirada, prazo_devolucao, valor_total)
                VALUES (@Id, @IdCliente, @IdVeiculo, @DataRetirada, @PrazoDevolucao, @ValorTotal)
                RETURNING *;
                """;

                await conexao.ExecuteAsync(query, novaLocacao, transacao);
                
                    const string sqlAtualizaVeiculoStatus = """
                    UPDATE veiculo SET status = @NovoStatus::status_disponibilidade WHERE id = @IdVeiculo;
                    """;
                    
                    await conexao.ExecuteAsync(sqlAtualizaVeiculoStatus, new { 
                        NovoStatus = StatusVeiculo.Alugado.ToString().ToUpper(),
                        IdVeiculo = veiculo.Id 
                    }, transacao);

                    await transacao.CommitAsync();

                }
                catch
                {
                    await transacao.RollbackAsync();
                    throw;
                }
                resposta.Dados = novaLocacao;
                resposta.Mensagem = "Locação cadastrada com sucesso.";  
            }
            catch (Exception e)
            {
                resposta.Dados = null;
                resposta.Mensagem = $"Erro ao cadastrar a locação: {e.Message}";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }
            return resposta;
        }

        public async Task<ModeloResposta<Locacao>> DevolverLocacao(Guid id)
        {
            var resposta = new ModeloResposta<Locacao>();


            try
            {
                var stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);
            
                await conexao.OpenAsync();
                
                

                const string sqlLocacao = """
                SELECT * FROM locacao WHERE id = @Id;
                """;
                
                var locacaoExistente = await conexao.QueryFirstOrDefaultAsync<Locacao>(sqlLocacao, new { Id = id });

                if (locacaoExistente == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Locação não encontrada.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }

                if (locacaoExistente.DataDevolucao != null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Locação já foi devolvida.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }
                

                var dataAtual = DateTime.Now;
                var dataAtualDateOnly = DateOnly.FromDateTime(dataAtual);
                bool isAtrasado = dataAtualDateOnly > locacaoExistente.PrazoDevolucao;

                string statusLocacao = isAtrasado ? StatusLocacao.Atrasado.ToString().ToUpper() : StatusLocacao.Devolvido.ToString().ToUpper();

                using var transacao = await conexao.BeginTransactionAsync();

                try
                {
                    const string sqlAtualizaLocacao = """
                    UPDATE locacao
                    SET data_devolucao = @DataDevolucao,
                        status = @StatusLocacao::status_locacao
                    WHERE id = @Id;
                    """;

                    await conexao.ExecuteAsync(sqlAtualizaLocacao, new { 
                        DataDevolucao = dataAtual,
                        StatusLocacao = statusLocacao,
                        Id = id }, transacao);


                    const string sqlAtualizaVeiculoStatus = """
                    UPDATE veiculo
                    SET status = 'DISPONIVEL'::status_disponibilidade
                    WHERE id = @IdVeiculo;
                    """;


                    await conexao.ExecuteAsync(sqlAtualizaVeiculoStatus, new { 
                        IdVeiculo = locacaoExistente.IdVeiculo 
                    }, transacao);

                    await transacao.CommitAsync();
                    
                    locacaoExistente.DataDevolucao = dataAtual;
                    resposta.Dados = locacaoExistente;
                    resposta.Mensagem = "Locação devolvida com sucesso.";
                } catch
                {
                    await transacao.RollbackAsync();
                    throw;
                }
            }
            catch (Exception e)
            {
                resposta.Dados = null;
                resposta.Mensagem = $"Erro ao devolver a locação: {e.Message}";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }
            return resposta;
        }

        public async Task<ModeloResposta<Locacao>> DeletarLocacao(Guid id)
        {
            var resposta = new ModeloResposta<Locacao>();

            try
            {

                var stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string sqlBuscaLocacao = """
                SELECT * FROM locacao WHERE id = @Id LIMIT 1;
                """;

                var locacaoExistente = await conexao.QuerySingleOrDefaultAsync<Locacao>(sqlBuscaLocacao, new { Id = id });

                if (locacaoExistente == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Locação não encontrada.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }

                if (locacaoExistente.Status == StatusLocacao.Ativo)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Não é possível deletar uma locação ativa.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                const string sqlDeletaLocacao = """
                DELETE FROM locacao WHERE id = @Id;
                """;

                await conexao.ExecuteAsync(sqlDeletaLocacao, new { Id = id });
                resposta.Dados = locacaoExistente;
                resposta.Mensagem = "Locação deletada com sucesso.";
            }
            catch (Exception e)
            {
                resposta.Dados = null;
                resposta.Mensagem = $"Erro ao deletar a locação: {e.Message}";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }
            return resposta;
        }
    }
}