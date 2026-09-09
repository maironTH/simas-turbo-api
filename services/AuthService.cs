using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dapper;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using SimasTurbo.Dto;
using SimasTurbo.Models;

namespace SimasTurbo.Services
{
    public class AuthService : IAuthInterface
    {
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _jwtSecurityKey;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IConfiguration configuration,
            SymmetricSecurityKey jwtSecurityKey,
            ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _jwtSecurityKey = jwtSecurityKey;
            _logger = logger;
        }

        public async Task<ModeloResposta<UsuarioMostrarDto>> LoginUsuario(UsuarioLoginDto usuarioLoginDto)
        {
            var resposta = new ModeloResposta<UsuarioMostrarDto>();

            try
            {
                var stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string sql = @"
                    SELECT id, email, senha, funcao
                    FROM usuario
                    WHERE email = @Email
                    LIMIT 1;";

                var usuario = await conexao.QuerySingleOrDefaultAsync<Usuario>(sql, new { Email = usuarioLoginDto.Email });

                if (usuario == null)
                {
                    resposta.Mensagem = "E-mail ou senha incorretos.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 401;
                    return resposta;
                }

                var senhaValida = BCrypt.Net.BCrypt.Verify(usuarioLoginDto.Senha, usuario.Senha);
                if (!senhaValida)
                {
                    resposta.Mensagem = "E-mail ou senha incorretos.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 401;
                    return resposta;
                }

                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, usuario.Email),
                        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                        new Claim(ClaimTypes.Role, usuario.Funcao.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddHours(2),
                    SigningCredentials = new SigningCredentials(
                        _jwtSecurityKey,
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                resposta.Dados = new UsuarioMostrarDto
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    Funcao = usuario.Funcao,
                    Token = jwt
                };
                resposta.Mensagem = "Login realizado com sucesso.";
                resposta.StatusCode = 200;
                resposta.IsSucesso = true;

                return resposta;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Erro ao autenticar usuário {Email}.", usuarioLoginDto.Email);
                resposta.Mensagem = "Erro ao autenticar o usuário.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
                return resposta;
            }
        }

        public async Task<ModeloResposta<UsuarioMostrarDto>> GetUsuarioEmail(string email)
        {
            var resposta = new ModeloResposta<UsuarioMostrarDto>();

            try
            {
                var stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string sql = @"
                    SELECT id, email, funcao
                    FROM usuario
                    WHERE email = @Email
                    LIMIT 1;";

                var usuario = await conexao.QuerySingleOrDefaultAsync<UsuarioMostrarDto>(sql, new { Email = email });

                if (usuario == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Usuário não encontrado.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }

                resposta.Dados = usuario;
                resposta.Mensagem = "Usuário encontrado com sucesso.";
                resposta.StatusCode = 200;
                resposta.IsSucesso = true;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem = "Erro ao buscar usuário por e-mail.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        public async Task<ModeloResposta<List<UsuarioMostrarDto>>> ListarUsuarios()
        {
            var resposta = new ModeloResposta<List<UsuarioMostrarDto>>();

            try
            {
                var stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string query = @"
                    SELECT id, email, funcao
                    FROM usuario;";

                var usuarios = await conexao.QueryAsync<UsuarioMostrarDto>(query);

                resposta.Dados = usuarios.ToList();
                resposta.Mensagem = "Usuários listados com sucesso.";
                resposta.StatusCode = 200;
                resposta.IsSucesso = true;
            }
            catch
            {
                resposta.Dados = null;
                resposta.Mensagem = "Erro ao listar usuários.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        public async Task<ModeloResposta<UsuarioMostrarDto>> CadastrarUsuario(UsuarioCadastrarDto usuarioCadastrarDto)
        {
            var resposta = new ModeloResposta<UsuarioMostrarDto>();

            try
            {
                var stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                var existeEmail = await conexao.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM usuario WHERE email = @Email;",
                    new { Email = usuarioCadastrarDto.Email });

                if (existeEmail > 0)
                {
                    resposta.Mensagem = "Este e-mail já está cadastrado.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                var funcao = FuncaoUsuario.FUNCIONARIO;

                var novoUsuario = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Email = usuarioCadastrarDto.Email,
                    Senha = BCrypt.Net.BCrypt.HashPassword(usuarioCadastrarDto.Senha),
                    Funcao = funcao
                };

                const string sqlInsert = @"
                    INSERT INTO usuario (id, email, senha, funcao)
                    VALUES (@Id, @Email, @Senha, @Funcao::funcao_usuario)
                    RETURNING *;";

                var usuarioCriado = await conexao.QuerySingleAsync<Usuario>(sqlInsert, new
                {
                    Id = novoUsuario.Id,
                    Email = novoUsuario.Email,
                    Senha = novoUsuario.Senha,
                    Funcao = novoUsuario.Funcao.ToString().ToUpperInvariant()
                });

                resposta.Dados = new UsuarioMostrarDto
                {
                    Id = usuarioCriado.Id,
                    Email = usuarioCriado.Email,
                    Funcao = usuarioCriado.Funcao
                };
                resposta.Mensagem = "Usuário cadastrado com sucesso.";
                resposta.StatusCode = 201;
                resposta.IsSucesso = true;
                return resposta;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Erro ao cadastrar usuário {Email}.", usuarioCadastrarDto.Email);
                resposta.Mensagem = "Erro ao cadastrar o usuário.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
                return resposta;
            }
        }

        public async Task<ModeloResposta<UsuarioMostrarDto>> AtualizarUsuario(Guid id, UsuarioAtualizaDto usuarioAtualizaDto)
        {
            var resposta = new ModeloResposta<UsuarioMostrarDto>();

            if (string.IsNullOrWhiteSpace(usuarioAtualizaDto.Email) &&
                string.IsNullOrWhiteSpace(usuarioAtualizaDto.Senha) &&
                string.IsNullOrWhiteSpace(usuarioAtualizaDto.Funcao))
            {
                resposta.Dados = null;
                resposta.Mensagem = "Informe pelo menos um campo para atualizar: Email, Senha ou Funcao.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 400;
                return resposta;
            }

            if (!string.IsNullOrWhiteSpace(usuarioAtualizaDto.Email) &&
                !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(usuarioAtualizaDto.Email))
            {
                resposta.Dados = null;
                resposta.Mensagem = "O formato do e-mail é inválido.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 400;
                return resposta;
            }

            if (!string.IsNullOrWhiteSpace(usuarioAtualizaDto.Senha) && usuarioAtualizaDto.Senha.Length < 6)
            {
                resposta.Dados = null;
                resposta.Mensagem = "A senha deve ter no mínimo 6 caracteres.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 400;
                return resposta;
            }

            try
            {
                var stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string sqlBuscaUsuario = @"
                    SELECT id, email, senha, funcao
                    FROM usuario
                    WHERE id = @Id
                    LIMIT 1;";

                var usuarioExistente = await conexao.QuerySingleOrDefaultAsync<Usuario>(sqlBuscaUsuario, new { Id = id });

                if (usuarioExistente == null)
                {
                    resposta.Mensagem = "Usuário não encontrado.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }

                var email = string.IsNullOrWhiteSpace(usuarioAtualizaDto.Email)
                    ? usuarioExistente.Email
                    : usuarioAtualizaDto.Email;
                var senha = string.IsNullOrWhiteSpace(usuarioAtualizaDto.Senha)
                    ? usuarioExistente.Senha
                    : BCrypt.Net.BCrypt.HashPassword(usuarioAtualizaDto.Senha);
                var funcao = usuarioExistente.Funcao;

                if (!string.IsNullOrWhiteSpace(usuarioAtualizaDto.Funcao) &&
                    !Enum.TryParse(usuarioAtualizaDto.Funcao, true, out funcao))
                {
                    resposta.Mensagem = "Função inválida. Use ADMIN ou FUNCIONARIO.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                const string sqlVerificaEmail = @"
                    SELECT COUNT(1)
                    FROM usuario
                    WHERE email = @Email AND id <> @Id;";

                var emailDuplicado = await conexao.ExecuteScalarAsync<int>(sqlVerificaEmail, new { Email = email, Id = id });

                if (emailDuplicado > 0)
                {
                    resposta.Mensagem = "Este e-mail já está cadastrado para outro usuário.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 400;
                    return resposta;
                }

                const string sqlAtualizaUsuario = @"
                    UPDATE usuario
                    SET email = @Email,
                        senha = @Senha,
                        funcao = @Funcao::funcao_usuario
                    WHERE id = @Id
                    RETURNING id, email, senha, funcao;";

                var usuarioAtualizado = await conexao.QuerySingleAsync<Usuario>(sqlAtualizaUsuario, new
                {
                    Id = id,
                    Email = email,
                    Senha = senha,
                    Funcao = funcao.ToString().ToUpperInvariant()
                });

                resposta.Dados = new UsuarioMostrarDto
                {
                    Id = usuarioAtualizado.Id,
                    Email = usuarioAtualizado.Email,
                    Funcao = usuarioAtualizado.Funcao
                };
                resposta.Mensagem = "Usuário atualizado com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            }
            catch
            {
                resposta.Mensagem = "Erro ao atualizar o usuário.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }

        public async Task<ModeloResposta<UsuarioMostrarDto>> DeletarUsuario(Guid id)
        {
            var resposta = new ModeloResposta<UsuarioMostrarDto>();

            try
            {
                var stringConexao = _configuration.GetConnectionString("neondb") ?? string.Empty;
                using var conexao = new NpgsqlConnection(stringConexao);

                const string sqlBuscaUsuario = @"
                    SELECT id, email, funcao
                    FROM usuario
                    WHERE id = @Id
                    LIMIT 1;";

                var usuario = await conexao.QuerySingleOrDefaultAsync<UsuarioMostrarDto>(sqlBuscaUsuario, new { Id = id });

                if (usuario == null)
                {
                    resposta.Dados = null;
                    resposta.Mensagem = "Usuário não encontrado.";
                    resposta.IsSucesso = false;
                    resposta.StatusCode = 404;
                    return resposta;
                }

                const string sqlDeletaUsuario = "DELETE FROM usuario WHERE id = @Id;";
                await conexao.ExecuteAsync(sqlDeletaUsuario, new { Id = id });

                resposta.Dados = usuario;
                resposta.Mensagem = "Usuário deletado com sucesso.";
                resposta.IsSucesso = true;
                resposta.StatusCode = 200;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Erro ao deletar usuário {Id}.", id);
                resposta.Dados = null;
                resposta.Mensagem = "Erro ao deletar o usuário.";
                resposta.IsSucesso = false;
                resposta.StatusCode = 500;
            }

            return resposta;
        }
    }
}