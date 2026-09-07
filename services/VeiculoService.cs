using Dapper;
using Npgsql;
using SimasTurbo.Models;
using SimasTurbo.Dto;

namespace SimasTurbo.Services
{
    public class VeiculoService : IVeiculoInterface
    {
        private readonly IConfiguration _configuration;
        public VeiculoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ModeloResposta<List<Veiculo>>> ListarVeiculos()
        {
            var resposta = new ModeloResposta<List<Veiculo>>();

            try
            {
                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string query = """
                Select * from veiculo
                """;

                var veiculos = await conexao.QueryAsync<Veiculo>(query);
                
                if (veiculos == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Nenhum veículo encontrado.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }
               
                resposta.Dados = veiculos.ToList();
                resposta.Mensagem = "Veículos listados com sucesso.";
            }
            catch (Exception ex)
            {
                resposta.Dados = null;
                resposta.Mensagem = $"Erro ao listar veículos: {ex.Message}";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        public async Task<ModeloResposta<Veiculo>> GetVeiculoPlaca(string placa)
        {
            var resposta = new ModeloResposta<Veiculo>();

            try
            {
                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string query = """
                Select * from veiculo where placa = @Placa LIMIT 1;
                """;

                var veiculo = await conexao.QuerySingleOrDefaultAsync<Veiculo>(query, new { Placa = placa });

                if (veiculo == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Veículo não encontrado.";
                    resposta.StatusCode = 404;
                    resposta.IsSucesso = false;
                }
                else
                {
                    resposta.Dados = veiculo;
                    resposta.Mensagem = "Veículo encontrado com sucesso.";
                    resposta.IsSucesso = true;
                }
            }
            catch (Exception ex)
            {
                resposta.Dados = null;
                resposta.Mensagem = $"Erro ao buscar veículo: {ex.Message}";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        public async Task<ModeloResposta<Veiculo>> CadastrarVeiculo(VeiculoCadastrarDto veiculoCadastrarDto)
        {
            var resposta = new ModeloResposta<Veiculo>();

            try
            {
                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string sqlVerificaDuplicado = """
                SELECT 1 FROM veiculo WHERE placa = @Placa LIMIT 1;
                """;

                var placaDuplicado = await conexao.QuerySingleOrDefaultAsync<Veiculo>(sqlVerificaDuplicado, new { veiculoCadastrarDto.Placa });

                if (placaDuplicado != null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Placa já cadastrada.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                var anoAtual = DateTime.Now.Year;
                
                if (veiculoCadastrarDto.Ano < anoAtual - 20 || veiculoCadastrarDto.Ano > anoAtual + 1) 
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "O veículo deve ter no máximo 20 anos de uso e não pode ter um ano de fabricação inválido.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                var novoVeiculo = new Veiculo
                {
                    Id = Guid.NewGuid(),
                    Placa = veiculoCadastrarDto.Placa,
                    Marca = veiculoCadastrarDto.Marca,
                    Modelo = veiculoCadastrarDto.Modelo,
                    Ano = veiculoCadastrarDto.Ano,
                    ValorDiaria = veiculoCadastrarDto.ValorDiaria,
                };

                const string query = """
                INSERT INTO veiculo (id, marca, modelo, ano, placa, valor_diaria)
                VALUES (@Id, @Marca, @Modelo, @Ano, @Placa, @ValorDiaria);
                """; 

                await conexao.ExecuteAsync(query, novoVeiculo);
                resposta.Dados = novoVeiculo;
                resposta.Mensagem = "Veículo cadastrado com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 201;
            }
            catch (Exception ex)
            {
                resposta.Dados = null;
                resposta.Mensagem = $"Erro ao cadastrar veículo: {ex.Message}";
                resposta.StatusCode = 500;
                resposta.IsSucesso = false;
            }

            return resposta;
        }

        public async Task<ModeloResposta<Veiculo>> AtualizarVeiculo(Guid id, VeiculoAtualizarDto veiculoAtualizarDto)
        {
            var resposta = new ModeloResposta<Veiculo>();

            try
            {
                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string queryBusca = """
                SELECT * FROM veiculo WHERE id = @Id LIMIT 1;
                """;

                var veiculoExistente = await conexao.QuerySingleOrDefaultAsync<Veiculo>(queryBusca, new { Id = id });

                if (veiculoExistente == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Veículo não encontrado.";
                    resposta.IsSucesso = false;
                    return resposta;
                }

                if (!string.IsNullOrWhiteSpace(veiculoAtualizarDto.Placa) && veiculoAtualizarDto.Placa != veiculoExistente.Placa)
                {
                    const string queryVerificaPlaca = """
                    SELECT 1 FROM veiculo WHERE placa = @Placa AND id != @Id LIMIT 1;
                    """;

                    var placaDuplicada = await conexao.ExecuteScalarAsync<int?>(queryVerificaPlaca, new { Placa = veiculoAtualizarDto.Placa, Id = id });

                    if (placaDuplicada != null)
                    {
                        resposta.Dados = null;
                        resposta.Mensagem = "Placa já cadastrada para outro veículo.";
                        resposta.IsSucesso = false;
                        resposta.StatusCode = 400;
                        return resposta;
                    }

                    veiculoExistente.Placa = veiculoAtualizarDto.Placa;
                }
                veiculoExistente.Marca = !string.IsNullOrWhiteSpace(veiculoAtualizarDto.Marca) ? veiculoAtualizarDto.Marca : veiculoExistente.Marca;
                veiculoExistente.Modelo = !string.IsNullOrWhiteSpace(veiculoAtualizarDto.Modelo) ? veiculoAtualizarDto.Modelo : veiculoExistente.Modelo;
                veiculoExistente.Ano = veiculoAtualizarDto.Ano.HasValue ? veiculoAtualizarDto.Ano.Value : veiculoExistente.Ano;
                veiculoExistente.ValorDiaria = veiculoAtualizarDto.ValorDiaria.HasValue ? veiculoAtualizarDto.ValorDiaria.Value : veiculoExistente.ValorDiaria;
                veiculoExistente.Status = veiculoAtualizarDto.Status.HasValue ? veiculoAtualizarDto.Status.Value : veiculoExistente.Status;

                const string queryAtualiza = """
                UPDATE veiculo
                SET marca = @Marca,
                    modelo = @Modelo,
                    ano = @Ano,
                    valor_diaria = @ValorDiaria,
                    status = @Status::status_disponibilidade
                WHERE id = @Id;
                """;

                await conexao.ExecuteAsync(queryAtualiza, new
                {
                    Id = veiculoExistente.Id,
                    Marca = veiculoExistente.Marca,
                    Modelo = veiculoExistente.Modelo,
                    Ano = veiculoExistente.Ano,
                    ValorDiaria = veiculoExistente.ValorDiaria,
                    Status = veiculoExistente.Status.ToString().ToUpper()
                });
                
                resposta.Dados = veiculoExistente;
                resposta.Mensagem = "Veículo atualizado com sucesso.";
            }
            catch (Exception ex)
            {
                resposta.Dados = null;
                resposta.Mensagem = $"Erro ao atualizar veículo: {ex.Message}";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        public async Task<ModeloResposta<Veiculo>> DeletarVeiculo(Guid id)
        {
            var resposta = new ModeloResposta<Veiculo>();

            try
            {
                string stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string queryBusca = """
                SELECT * FROM veiculo WHERE id = @Id LIMIT 1;
                """;

                var veiculoExistente = await conexao.QuerySingleOrDefaultAsync<Veiculo>(queryBusca, new { Id = id });

                if (veiculoExistente == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Veículo não encontrado.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }

                const string queryVerificaLocacoes = """
                SELECT 1 FROM locacao WHERE id_veiculo = @Id LIMIT 1;
                """;      

                var locacaoAtiva = await conexao.ExecuteScalarAsync<int?>(queryVerificaLocacoes, new { Id = id });

                if (locacaoAtiva != null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Não é possível deletar o veículo, pois ele possui locações associadas.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                const string queryDeleta = """
                DELETE FROM veiculo WHERE id = @Id;
                """;

                await conexao.ExecuteAsync(queryDeleta, new { Id = id });

                resposta.Dados = veiculoExistente;
                resposta.Mensagem = "Veículo deletado com sucesso.";
            }
            catch (Exception ex)
            {
                resposta.Dados = null;
                resposta.Mensagem = $"Erro ao deletar veículo: {ex.Message}";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }
    }
}