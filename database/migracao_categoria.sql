-- ============================================================
-- Migração para quem JÁ tem o banco com 4 tabelas
-- (usuario, cliente, veiculo, locacao) e quer adicionar categoria.
-- Execute o bloco inteiro de uma vez, no SQL Editor do Neon.
-- ============================================================

BEGIN;

CREATE TABLE categoria (
    id UUID PRIMARY KEY,
    nome VARCHAR(50) NOT NULL UNIQUE,
    descricao VARCHAR(200),
    capacidade_passageiros SMALLINT NOT NULL CHECK (capacidade_passageiros > 0)
);

INSERT INTO categoria (id, nome, descricao, capacidade_passageiros) VALUES
    (gen_random_uuid(), 'Econômico', 'Carros compactos, baixo consumo', 5),
    (gen_random_uuid(), 'SUV', 'Utilitários esportivos, mais espaço e conforto', 5),
    (gen_random_uuid(), 'Van', 'Transporte de grupos', 12);

-- 1) cria a coluna aceitando NULL (a tabela já tem linhas)
ALTER TABLE veiculo
    ADD COLUMN id_categoria UUID REFERENCES categoria(id) ON DELETE RESTRICT;

-- 2) preenche os veículos existentes com uma categoria padrão
UPDATE veiculo
SET id_categoria = (SELECT id FROM categoria WHERE nome = 'Econômico')
WHERE id_categoria IS NULL;

-- 3) agora que ninguém é NULL, torna obrigatória
ALTER TABLE veiculo ALTER COLUMN id_categoria SET NOT NULL;

CREATE INDEX idx_veiculo_categoria ON veiculo(id_categoria);

COMMIT;
