# SimasTurbo API

API RESTful desenvolvida para o processo seletivo da Include, focada em gestão de frota e sistema de aluguel de veículos. O projeto foi construído priorizando performance, manutenibilidade e segurança nas regras de negócio com o banco de dados.

---

## 🚀 Tecnologias
* **Linguagem:** C# (.NET 10)
* **Banco de Dados:** PostgreSQL (Hospedado na nuvem via Neon.tech)
* **Acesso a Dados:** Dapper (Micro-ORM)
* **Documentação:** Swagger / OpenAPI

---

## 🏛️ Arquitetura e Decisões Técnicas

* **Thin Controller & Fat Service:** Os *Controllers* funcionam apenas como roteadores HTTP leves, enquanto toda a lógica de negócio e manipulação de dados fica isolada nos *Services*.
* **Padronização de Respostas (`ModeloResposta<T>`):** Uso de um *wrapper* genérico para unificar o formato dos retornos JSON, controlando mensagens, status de sucesso e códigos HTTP de forma limpa.
* **Integridade Transacional (ACID):** Operações que envolvem múltiplas tabelas (como registrar uma locação e alterar o status do veículo) utilizam transações explícitas (`BeginTransactionAsync`) com tratamento de *rollback*.
* **Ajustes de Performance com Dapper:** 
  * Mapeamento global de propriedades com `MatchNamesWithUnderscores` para converter automaticamente o *snake_case* do banco para o *PascalCase* do C#.
  * Implementação de um `DateOnlyTypeHandler` customizado para lidar nativamente com datas.
  * Validações de duplicidade feitas diretamente no SQL com checagens rápidas (`LIMIT 1`).

---

## 📋 Endpoints da API

### Clientes
* `GET /api/Cliente` — Lista todos os clientes.
* `GET /api/Cliente/{cpf}` — Busca cliente por CPF.
* `POST /api/Cliente` — Cadastra novo cliente (com validação de maioridade de 18 anos e checagem de duplicidade).
* `PATCH /api/Cliente/{id}` — Atualiza parcialmente os dados do cliente.
* `DELETE /api/Cliente/{id}` — Remove o cliente (com trava de segurança caso possua locações ativas).

### Veículos
* `GET /api/Veiculo` — Lista a frota completa.
* `GET /api/Veiculo/{placa}` — Busca veículo pela placa.
* `POST /api/Veiculo` — Cadastra novo veículo.
* `PATCH /api/Veiculo/{id}` — Atualiza dados do veículo.
* `DELETE /api/Veiculo/{id}` — Remove o veículo.

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