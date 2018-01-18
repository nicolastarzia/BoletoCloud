namespace BoletoCloud.Entities
{
    public class Pessoa
    {
        public string Nome { get; set; }
        public string CPRF { get; set; }
        public Endereco Endereco { get; set; }
    }
}