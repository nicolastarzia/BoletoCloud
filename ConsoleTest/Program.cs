using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoletoCloud;
using BoletoCloud.Entities;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Config.APIKey = "api-key_UUjiuV9mXi0u4LdFhZPuOq6ADHYwd62msVFOXwrGd9k=";
            Config.isSandBox = true;

            BoletoCloud.Entities.Boleto bol = new BoletoCloud.Entities.Boleto()
            {
                Conta = new { banco = "237",
                agencia = "1234-5",
                numero = "12345-6",
                carteira = "12"
                            },
                Beneficiario = new Pessoa()
                {
                    Nome = "Nicolas Tarzia",
                    CPRF = "15.719.277/0001-46",
                    Endereco = new Endereco()
                    {
                        CEP = "59020-000",
                        UF = "RN",
                        Localidade = "Natal",
                        Bairro = "Petropolis",
                        Logradouro = "Avenida Hermes da Fonseca",
                        Numero = "384",
                        Complemento = "Sala 2A, segundo andar",
                    }
                },

                Emissao = new DateTime(2014,07,11),
                Vencimento = new DateTime(2020,05,30),
                Documento = "EX1",
                Numero= "12345678907-P",
                Titulo = "DM",
                Valor = (decimal) 1000000.99,
                Instrucao = "Atencao, não receber este boleto.",

                Pagador = new Pessoa()
                {
                    Nome = "Lucas Magalhaes",
                    CPRF = "111.111.111-11",
                    Endereco = new Endereco()
                    {
                        CEP = "36240-000",
                        UF = "MG",
                        Localidade = "Santos Dummont",
                        Bairro = "Casa Natal",
                        Logradouro = "BR-499",
                        Numero = "s/n",
                        Complemento = "Sitio - Subindo a serra da Mantiqueira",
                    }
                },
            };


            BoletoCloud.Boleto boleto = new BoletoCloud.Boleto();
            var retAsync = boleto.Criar(bol);
            retAsync.Wait();

            var result = retAsync.Result;



            using (var fileStream = File.Create("D:\\Boleto.PDF"))
            {
                result.PDF.Seek(0, SeekOrigin.Begin);
                result.PDF.CopyTo(fileStream);
            }

        }
    }
}
