-- ============================================================
-- SimasTurbo - Esquema completo (5 tabelas)
-- usuario | cliente | categoria | veiculo | locacao
-- ============================================================

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

-- NOVA TABELA: relação 1:N com veiculo (uma categoria tem vários veículos)
CREATE TABLE categoria (
    id UUID PRIMARY KEY,
    nome VARCHAR(50) NOT NULL UNIQUE,
    descricao VARCHAR(200),
    capacidade_passageiros SMALLINT NOT NULL CHECK (capacidade_passageiros > 0)
);

CREATE TABLE veiculo (
    id UUID PRIMARY KEY,
    placa VARCHAR(7) NOT NULL UNIQUE,
    marca VARCHAR(50) NOT NULL,
    modelo VARCHAR(50) NOT NULL,
    ano SMALLINT NOT NULL,
    valor_diaria NUMERIC(12, 2) NOT NULL,
    status status_disponibilidade NOT NULL DEFAULT 'DISPONIVEL',
    id_categoria UUID NOT NULL REFERENCES categoria(id) ON DELETE RESTRICT
);

CREATE INDEX idx_veiculo_categoria ON veiculo(id_categoria);

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

-- Dados iniciais (opcional)
INSERT INTO categoria (id, nome, descricao, capacidade_passageiros) VALUES
    (gen_random_uuid(), 'Econômico', 'Carros compactos, baixo consumo', 5),
    (gen_random_uuid(), 'SUV', 'Utilitários esportivos, mais espaço e conforto', 5),
    (gen_random_uuid(), 'Van', 'Transporte de grupos', 12);
