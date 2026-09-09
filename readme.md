# SimasTurbo API

API RESTful desenvolvida para o processo seletivo da Include, focada em gestão de frota e sistema de aluguel de veículos. O projeto foi construído priorizando performance, manutenibilidade e segurança nas regras de negócio com o banco de dados.

---

## 🚀 Tecnologias
* **Linguagem:** C# (.NET 10)
* **Banco de Dados:** PostgreSQL (Hospedado na nuvem via Neon.tech)
* **Autenticação:** JWT Bearer e BCrypt
* **Acesso a Dados:** Dapper (Micro-ORM)
* **Documentação:** Swagger / OpenAPI

---

## 🏛️ Arquitetura e Decisões Técnicas

* **Thin Controller & Fat Service:** Os *Controllers* funcionam apenas como roteadores HTTP leves, enquanto toda a lógica de negócio e manipulação de dados fica isolada nos *Services*.
* **Padronização de Respostas (`ModeloResposta<T>`):** Uso de um *wrapper* genérico para unificar o formato dos retornos JSON, controlando mensagens, status de sucesso e códigos HTTP de forma limpa.
* **Integridade Transacional (ACID):** Operações que envolvem múltiplas tabelas (como registrar uma locação e alterar o status do veículo) utilizam transações explícitas (`BeginTransactionAsync`) com tratamento de *rollback*.
* **Autenticação e autorização:** A API utiliza JWT Bearer com claims de identificação e função (`ADMIN` ou `FUNCIONARIO`). As senhas são protegidas com BCrypt, e a chave de assinatura é obrigatória e carregada por User Secrets ou variável de ambiente.
* **Ajustes de Performance com Dapper:** 
  * Mapeamento global de propriedades com `MatchNamesWithUnderscores` para converter automaticamente o *snake_case* do banco para o *PascalCase* do C#.
  * Implementação de um `DateOnlyTypeHandler` customizado para lidar nativamente com datas.
    * Validações de duplicidade feitas diretamente em SQL com checagens rápidas (`LIMIT 1`). No cadastro de clientes, CPF, CNH, telefone e e-mail devem ser únicos.

---

## 📋 Endpoints da API

### Autenticação e usuários
* `POST /api/Auth/login` — Autentica um usuário e retorna um token JWT.
* `POST /api/Auth/cadastrar` — Cadastra um novo usuário público como `FUNCIONARIO`.
* `GET /api/Auth` — Lista usuários autenticados.
* `GET /api/Auth/{email}` — Busca um usuário autenticado pelo e-mail.
* `PATCH /api/Auth/{id}` — `FUNCIONARIO` atualiza apenas o próprio e-mail ou senha; `ADMIN` pode atualizar qualquer usuário e alterar sua função.
* `DELETE /api/Auth/{id}` — Remove um usuário; disponível somente para `ADMIN`.

### Clientes
* `GET /api/Cliente` — Lista todos os clientes.
* `GET /api/Cliente/{cpf}` — Busca cliente por CPF.
* `POST /api/Cliente` — Cadastra novo cliente (com validação de maioridade de 18 anos e checagem de duplicidade).
* `PATCH /api/Cliente/{id}` — Atualiza parcialmente os dados do cliente.
* `DELETE /api/Cliente/{id}` — Remove o cliente, desde que não possua locações associadas.

### Veículos
* `GET /api/Veiculo` — Lista a frota completa.
* `GET /api/Veiculo/{placa}` — Busca veículo pela placa.
* `POST /api/Veiculo` — Cadastra novo veículo.
* `PATCH /api/Veiculo/{id}` — Atualiza dados do veículo.
* `DELETE /api/Veiculo/{id}` — Remove o veículo, desde que não possua locações associadas.

### Locações
* `GET /api/Locacao` — Lista todas as locações.
* `GET /api/Locacao/cliente/{cpfCliente}` — Histórico de locações por CPF.
* `GET /api/Locacao/veiculo/{placaVeiculo}` — Histórico de locações por placa.
* `POST /api/Locacao` — Registra a locação (mudando o status do veículo para `ALUGADO`).
* `PATCH /api/Locacao/{id}` — Realiza a devolução (calcula atrasos e retorna o veículo para `DISPONIVEL` via transação).
* `DELETE /api/Locacao/{id}` — Remove o registro de locação.

---

## ⚙️ Como Executar Localmente

1. Clone o repositório:
   ```bash
   git clone https://github.com/maironTH/simas-turbo-api.git
    cd simas-turbo-api
    ```

2. Instale o .NET 10 SDK e confirme a instalação:
    ```bash
    dotnet --version
    ```

3. Configure um banco PostgreSQL no [Neon](https://neon.tech/). No painel do projeto, copie a connection string e configure-a como User Secret:
    ```bash
    dotnet user-secrets init
    dotnet user-secrets set "ConnectionStrings:neondb" "Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
    ```

4. Crie as tabelas, enums e relacionamentos esperados pela API no SQL Editor do Neon. O banco deve conter as tabelas `usuario`, `cliente`, `veiculo` e `locacao`, além dos tipos de status usados nas consultas:
    ```sql
    CREATE TYPE funcao_usuario AS ENUM ('ADMIN', 'FUNCIONARIO');
    CREATE TYPE status_disponibilidade AS ENUM ('DISPONIVEL', 'INDISPONIVEL', 'ALUGADO', 'INATIVO');
    CREATE TYPE status_locacao AS ENUM ('ATIVO', 'DEVOLVIDO', 'CANCELADO', 'ATRASADO');

    CREATE TABLE usuario (
       id UUID PRIMARY KEY,
       email VARCHAR(150) NOT NULL UNIQUE,
       senha TEXT NOT NULL,
       funcao funcao_usuario NOT NULL DEFAULT 'FUNCIONARIO'
    );

    CREATE TABLE cliente (
       id UUID PRIMARY KEY,
       nome VARCHAR(150) NOT NULL,
       data_nascimento DATE NOT NULL,
       telefone VARCHAR(15) NOT NULL UNIQUE,
       email VARCHAR(100) NOT NULL UNIQUE,
       cpf VARCHAR(11) NOT NULL UNIQUE,
       cnh VARCHAR(15) NOT NULL UNIQUE,
       cep VARCHAR(8) NOT NULL,
       uf VARCHAR(2) NOT NULL,
       cidade VARCHAR(100) NOT NULL,
       bairro VARCHAR(100) NOT NULL,
       logradouro VARCHAR(100) NOT NULL,
       numero VARCHAR(10) NOT NULL,
       complemento VARCHAR(100)
    );

    CREATE TABLE veiculo (
       id UUID PRIMARY KEY,
       placa VARCHAR(7) NOT NULL UNIQUE,
       marca VARCHAR(50) NOT NULL,
       modelo VARCHAR(50) NOT NULL,
       ano SMALLINT NOT NULL,
       valor_diaria NUMERIC(12, 2) NOT NULL,
       status status_disponibilidade NOT NULL DEFAULT 'DISPONIVEL'
    );

    CREATE TABLE locacao (
       id UUID PRIMARY KEY,
       id_veiculo UUID NOT NULL REFERENCES veiculo(id),
       id_cliente UUID NOT NULL REFERENCES cliente(id),
       data_retirada TIMESTAMP NOT NULL,
       prazo_devolucao DATE NOT NULL,
       data_devolucao TIMESTAMP NULL,
       valor_total NUMERIC(12, 2) NOT NULL,
       status status_locacao NOT NULL DEFAULT 'ATIVO'
    );
    ```

  Se os tipos ou tabelas já existirem, não execute o bloco novamente sem antes revisar o schema atual do banco.

5. Configure a chave usada para assinar os tokens JWT. Ela é obrigatória e deve ser mantida fora do código:
    ```bash
    dotnet user-secrets set "JwtConfig:Secret" "uma-chave-com-pelo-menos-32-caracteres"
    ```

    Em ambientes como Render, use as variáveis de ambiente equivalentes:
    ```text
    ConnectionStrings__neondb=<connection-string-do-neon>
    JwtConfig__Secret=<chave-secreta-do-jwt>
    ```

6. Restaure as dependências e execute a API:
    ```bash
    dotnet restore
    dotnet run
    ```

7. Abra o Swagger local em [http://localhost:5093/swagger](http://localhost:5093/swagger). Primeiro cadastre um funcionário, faça login e use o token retornado no botão **Authorize**. Usuários públicos são criados somente como `FUNCIONARIO`; a função `ADMIN` deve ser atribuída por um administrador autenticado.

### Criando o primeiro administrador

O endpoint público `/api/Auth/cadastrar` cria somente usuários `FUNCIONARIO`. Para criar o primeiro administrador, cadastre o usuário normalmente e depois promova-o no SQL Editor do Neon:

```sql
UPDATE usuario
SET funcao = 'ADMIN'::funcao_usuario
WHERE email = 'seu-email@example.com';
```

Depois, faça login novamente para receber um token com a role `ADMIN`. A partir daí, esse administrador pode alterar a função de outros usuários pelo endpoint `PATCH /api/Auth/{id}`.

### Solução de problemas

Se a aplicação informar que `JwtConfig:Secret` não foi encontrado, execute os comandos a partir da pasta que contém o arquivo `SimasTurbo.csproj`:

```bash
cd SimasTurbo
dotnet user-secrets set "JwtConfig:Secret" "uma-chave-secreta-com-pelo-menos-32-caracteres"
dotnet user-secrets list
dotnet run
```

A chave JWT não deve ser adicionada ao `appsettings.json` nem enviada ao Git. Se estiver executando a partir de outra pasta, use `dotnet run --project /caminho/para/SimasTurbo.csproj` ou configure a variável de ambiente `JwtConfig__Secret`.

## 🌐 API publicada

* **Swagger no Render:** [https://simas-turbo-api.onrender.com/swagger](https://simas-turbo-api.onrender.com/swagger)
* **Repositório:** [https://github.com/maironTH/simas-turbo-api](https://github.com/maironTH/simas-turbo-api)