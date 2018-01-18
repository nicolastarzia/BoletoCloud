using System;

namespace BoletoCloud.Entities
{
    public class Boleto
    {
        public bool? Aceite { get; set; }
        public string Titulo { get; set; }
        public string Documento { get; set; }
        public string Numero { get; set; }
        public string Sequencial { get; set; }
        public string Instrucao { get; set; }
        public DateTime? Emissao { get; set; }
        public DateTime? Vencimento { get; set; }
        public decimal? Valor { get; set; }
        public decimal? Juros { get; set; }
        public decimal? Multa { get; set; }
        public Pessoa Beneficiario { get; set; }
        public Pessoa Pagador { get; set; }
        public dynamic Conta { get; set; }

    }
}
