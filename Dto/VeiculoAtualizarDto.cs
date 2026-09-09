using System.ComponentModel.DataAnnotations;
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

        [EnumDataType(typeof(StatusVeiculo), ErrorMessage = "O status do veículo é inválido.")]
        public StatusVeiculo? Status { get; set; }
    }
}