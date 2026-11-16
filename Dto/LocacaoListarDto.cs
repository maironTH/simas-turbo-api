using SimasTurbo.Models;

namespace SimasTurbo.Dto
{
    public class LocacaoListarDto
    {
        public Guid Id { get; set; }
        public string NomeCliente { get; set; } = string.Empty;
        public string CpfCliente { get; set; } = string.Empty;
        public string PlacaVeiculo { get; set; } = string.Empty;
        public DateOnly DataRetirada { get; set; }
        public DateOnly PrazoDevolucao { get; set; }
        public DateOnly? DataDevolucao { get; set; }
        public decimal ValorTotal { get; set; }
        public StatusLocacao Status { get; set; } = StatusLocacao.Ativo;
    }
}