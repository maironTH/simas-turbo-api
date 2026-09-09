using SimasTurbo.Models;
using SimasTurbo.Dto;
using Dapper;
using Npgsql;

namespace SimasTurbo.Services
{
    public class ClienteService : IClienteInterface
    {
        private readonly IConfiguration _configuration;

        public ClienteService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ModeloResposta<List<ClienteListarDto>>> ListarClientes()
        {
            var resposta = new ModeloResposta<List<ClienteListarDto>>();

            try
            {
                var stringconexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringconexao);

                const string query = """
                SELECT id, nome, telefone, email FROM cliente;
                """;

                var clientes = await conexao.QueryAsync<ClienteListarDto>(query);

                resposta.Dados = clientes.ToList();
                resposta.Mensagem = "Clientes Listados com Sucesso";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            } 
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem= "Erro ao listar os clientes.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;  
            }
            return resposta;
        }

        public async Task<ModeloResposta<ClienteListarDto>> GetClienteCpf(string cpf)
        {
            var resposta = new ModeloResposta<ClienteListarDto>();

            try
            {
                var stringconexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringconexao);

                const string query = """
                SELECT id, nome, telefone, email FROM cliente where cpf = @Cpf LIMIT 1;
                """;

                var cliente = await conexao.QuerySingleOrDefaultAsync<ClienteListarDto>(query, new { Cpf = cpf });

                if (cliente == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Cliente não encontrado.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }

                resposta.Dados = cliente;
                resposta.Mensagem = "Cliente encontrado com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem= "Erro ao buscar o cliente.";
                resposta.StatusCode = 500;
                resposta.IsSucesso = false;
            }
            return resposta;
        }

        public async Task<ModeloResposta<Cliente>> CadastrarCliente(ClienteCadastrarDto clienteCadastrarDto)
        {
            var resposta = new ModeloResposta<Cliente>();

            try
            {
                var stringconexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringconexao);

                const string sqlVerificaDuplicado = """
                SELECT 
                    case 
                        when cpf = @Cpf then 'CPF'
                        when cnh = @Cnh then 'CNH'
                        when email = @Email then 'Email'
                        when telefone = @Telefone then 'Telefone'
                    end
                from cliente
                where cpf = @Cpf or cnh = @Cnh or email = @Email or telefone = @Telefone
                LIMIT 1;
                """;

                string? campoDuplicado = await conexao.ExecuteScalarAsync<string?>(sqlVerificaDuplicado, new {
                    clienteCadastrarDto.Cpf,
                    clienteCadastrarDto.Cnh,
                    clienteCadastrarDto.Email,
                    clienteCadastrarDto.Telefone
                });
               
                if (!string.IsNullOrEmpty(campoDuplicado))
                {
                    resposta.Dados = null;
                    resposta.Mensagem = $"Erro ao Cadastrar o Cliente: {campoDuplicado} já cadastrado.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                var dataMinimaMaioridade = DateOnly.FromDateTime(DateTime.Now.AddYears(-18));
                if (clienteCadastrarDto.DataNascimento > dataMinimaMaioridade)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Erro ao Cadastrar o Cliente: O cliente deve ser maior de idade.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                var novoCliente = new Cliente
                {
                    Id = Guid.NewGuid(),
                    Nome = clienteCadastrarDto.Nome,
                    DataNascimento = clienteCadastrarDto.DataNascimento,
                    Telefone = clienteCadastrarDto.Telefone,
                    Email = clienteCadastrarDto.Email,
                    Cpf = clienteCadastrarDto.Cpf,
                    Cnh = clienteCadastrarDto.Cnh,
                    Cep = clienteCadastrarDto.Cep,
                    UF = clienteCadastrarDto.UF,
                    Cidade = clienteCadastrarDto.Cidade,
                    Bairro = clienteCadastrarDto.Bairro,
                    Logradouro = clienteCadastrarDto.Logradouro,
                    Numero = clienteCadastrarDto.Numero,
                    Complemento = clienteCadastrarDto.Complemento
                };

                const string query = """
                INSERT INTO cliente (id, nome, data_nascimento, telefone, email, cpf, cnh, cep, uf, cidade, bairro, logradouro, numero, complemento)
                VALUES (@Id, @Nome, @DataNascimento, @Telefone, @Email, @Cpf, @Cnh, @Cep, @UF, @Cidade, @Bairro, @Logradouro, @Numero, @Complemento)
                RETURNING *;
                """;

                await conexao.ExecuteAsync(query, novoCliente);

                resposta.Dados = novoCliente;
                resposta.Mensagem = "Cliente Cadastrado com Sucesso";
                resposta.StatusCode = 201;
                resposta.IsSucesso = true;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem= "Erro ao cadastrar o cliente.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }
            return resposta;
        }

        public async Task<ModeloResposta<Cliente>> AtualizarCliente(Guid Id, ClienteAtualizarDto clienteAtualizarDto)
        {
            var resposta = new ModeloResposta<Cliente>();

            try
            {
                var stringconexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringconexao);

                const string sqlBuscaCliente = """
                SELECT * FROM cliente WHERE id = @Id LIMIT 1;
                """;
                var clienteExistente = await conexao.QuerySingleOrDefaultAsync<Cliente>(sqlBuscaCliente, new { Id = Id });
                
                if (clienteExistente == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Cliente não encontrado.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }

                const string sqlVerificaDuplicado = """
                    SELECT 1 FROM cliente
                    WHERE (cpf = @Cpf OR cnh = @Cnh OR email = @Email OR telefone = @Telefone) AND id != @Id
                    LIMIT 1;    
                """;

                var clienteDuplicadoId = await conexao.ExecuteScalarAsync<int?>(sqlVerificaDuplicado, new {
                    Cpf = clienteAtualizarDto.Cpf ?? clienteExistente.Cpf,
                    Cnh = clienteAtualizarDto.Cnh ?? clienteExistente.Cnh,
                    Email = clienteAtualizarDto.Email ?? clienteExistente.Email,
                    Telefone = clienteAtualizarDto.Telefone ?? clienteExistente.Telefone,
                    Id = Id
                });

                if (clienteDuplicadoId != null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Erro ao atualizar o Cliente: CPF, CNH, Email ou Telefone já cadastrado para outro cliente.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                var dataMinimaMaioridade = DateOnly.FromDateTime(DateTime.Now.AddYears(-18));
                
                if (clienteAtualizarDto.DataNascimento.HasValue && clienteAtualizarDto.DataNascimento.Value > dataMinimaMaioridade)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Erro ao atualizar o Cliente: O cliente deve ser maior de idade.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }
                

                clienteExistente.Nome = !string.IsNullOrWhiteSpace(clienteAtualizarDto.Nome) ? clienteAtualizarDto.Nome : clienteExistente.Nome;
                clienteExistente.DataNascimento = clienteAtualizarDto.DataNascimento.HasValue ? clienteAtualizarDto.DataNascimento.Value : clienteExistente.DataNascimento;
                clienteExistente.Telefone = !string.IsNullOrWhiteSpace(clienteAtualizarDto.Telefone) ? clienteAtualizarDto.Telefone : clienteExistente.Telefone;
                clienteExistente.Email = !string.IsNullOrWhiteSpace(clienteAtualizarDto.Email) ? clienteAtualizarDto.Email : clienteExistente.Email;
                clienteExistente.Cpf = !string.IsNullOrWhiteSpace(clienteAtualizarDto.Cpf) ? clienteAtualizarDto.Cpf : clienteExistente.Cpf;
                clienteExistente.Cnh = !string.IsNullOrWhiteSpace(clienteAtualizarDto.Cnh) ? clienteAtualizarDto.Cnh : clienteExistente.Cnh;
                clienteExistente.Cep = !string.IsNullOrWhiteSpace(clienteAtualizarDto.Cep) ? clienteAtualizarDto.Cep : clienteExistente.Cep;
                clienteExistente.UF = !string.IsNullOrWhiteSpace(clienteAtualizarDto.UF) ? clienteAtualizarDto.UF : clienteExistente.UF;
                clienteExistente.Cidade = !string.IsNullOrWhiteSpace(clienteAtualizarDto.Cidade) ? clienteAtualizarDto.Cidade : clienteExistente.Cidade;
                clienteExistente.Bairro = !string.IsNullOrWhiteSpace(clienteAtualizarDto.Bairro) ? clienteAtualizarDto.Bairro : clienteExistente.Bairro;
                clienteExistente.Logradouro = !string.IsNullOrWhiteSpace(clienteAtualizarDto.Logradouro) ? clienteAtualizarDto.Logradouro : clienteExistente.Logradouro;
                clienteExistente.Numero = !string.IsNullOrWhiteSpace(clienteAtualizarDto.Numero) ? clienteAtualizarDto.Numero : clienteExistente.Numero;
                clienteExistente.Complemento = clienteAtualizarDto.Complemento ?? clienteExistente.Complemento;

                const string query = """
                UPDATE cliente
                SET nome = @Nome,
                    data_nascimento = @DataNascimento,
                    telefone = @Telefone,
                    email = @Email,
                    cpf = @Cpf,
                    cnh = @Cnh,
                    cep = @Cep,
                    uf = @UF,
                    cidade = @Cidade,
                    bairro = @Bairro,
                    logradouro = @Logradouro,
                    numero = @Numero,
                    complemento = @Complemento
                WHERE id = @Id;
                """;
                await conexao.ExecuteAsync(query, clienteExistente);
                
                resposta.Dados = clienteExistente;
                resposta.Mensagem = "Cliente atualizado com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem= "Erro ao atualizar o cliente.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }
            return resposta;
        }

        public async Task<ModeloResposta<Cliente>> DeletarCliente(Guid Id)
        {
            var resposta = new ModeloResposta<Cliente>();

            try
            {
                var stringconexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringconexao);

                const string sqlBuscaCliente = """
                SELECT * FROM cliente WHERE id = @Id LIMIT 1;
                """;
                var cliente = await conexao.QuerySingleOrDefaultAsync<Cliente>(sqlBuscaCliente, new { Id = Id });
                
                if (cliente == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Cliente não encontrado.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }

                const string sqlVerificaLocacoes = """
                SELECT 1 FROM locacao WHERE id_cliente = @Id LIMIT 1;
                """;
                var locacoesAtivas = await conexao.ExecuteScalarAsync<int?>(sqlVerificaLocacoes, new { Id });

                if (locacoesAtivas != null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Não é possível excluir o cliente pois possui locações associadas.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                const string query = """
                DELETE FROM cliente WHERE id = @Id;
                """;
                await conexao.ExecuteAsync(query, new { Id });

                resposta.Dados = cliente;
                resposta.Mensagem = "Cliente deletado com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem= "Erro ao deletar o cliente.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }
            return resposta;
        }
    }

}