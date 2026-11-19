namespace SimasTurbo.Dto
{
    public class RelatorioLocacaoCategoriaDto
    {
        public string Categoria { get; set; } = string.Empty;
        public int TotalLocacoes { get; set; }
        public decimal Faturamento { get; set; }
    }
}
