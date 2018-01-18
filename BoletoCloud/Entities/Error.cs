using System;
using System.Collections.Generic;
using System.Text;

namespace BoletoCloud.Entities
{
    public class Error
    {
        public int Status { get; set; }
        public string Tipo { get; set; }
        public List<Causa> Causas { get; set; }
    }
}
