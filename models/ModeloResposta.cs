namespace SimasTurbo.Models
{
    public class ModeloResposta <T>
    {
        public T? Dados { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public bool IsSucesso { get; set; } = true;

        public int StatusCode { get; set; } = 200;
    }
}