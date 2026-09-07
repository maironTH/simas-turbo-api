using SimasTurbo.Models;

namespace SimasTurbo.Dto
{
    public class VeiculoAtualizarDto
    {
        public string? Placa { get; set; } 

        public string? Marca { get; set; }

        public string? Modelo { get; set; }

        public short? Ano { get; set; }

        public decimal? ValorDiaria { get; set; }

        public StatusVeiculo? Status { get; set; } = StatusVeiculo.Disponivel;
    }
}