namespace SimasTurbo.Dto
{
    public class CategoriaResumoDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int TotalVeiculos { get; set; }
        public int TotalDisponiveis { get; set; }
        public decimal MediaValorDiaria { get; set; }
    }
}
