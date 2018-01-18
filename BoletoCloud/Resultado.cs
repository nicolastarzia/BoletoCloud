using System.Collections.Generic;
using System.IO;

namespace BoletoCloud
{
    public class Resultado
    {
        public string StatusCode { get; set; }
        public string ContentType { get; set; }
        public string Location { get; set; }
        public string Token { get; set; }
        public string Version { get; set; }
        public Stream PDF { get; set; }
        public Entities.Error Erro { get; set; }
    }
}