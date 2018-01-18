using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace BoletoCloud
{
    public class Boleto
    {
        private const string MODULO_BOLETOCLOUD = "/boletos";

        


        private void ApiKeyFoiPreenchida()
        {
            if (string.IsNullOrEmpty(Config.APIKey))
                throw new System.ArgumentNullException("APIKey", "É obrigatório preencher o parametro APIKey");
        }

        public async Task<Resultado> Criar(Entities.Boleto boleto)
        {
            var TARGETURL = string.Concat(ConfigURIS.URI(), MODULO_BOLETOCLOUD);

            Func<HttpClient, Task<HttpResponseMessage>> acao = async (client) => {
                var boletoKeys = boleto.ToKeyValue();

                var response = await client.PostAsync(TARGETURL, new FormUrlEncodedContent(boletoKeys));
                return response;
            };

            return await EnviarRequisicao(TARGETURL, acao);
        }

        public async Task<Resultado> Buscar(string token)
        {
            var TARGETURL = string.Concat(ConfigURIS.URI(), MODULO_BOLETOCLOUD,"/", token);

            Func<HttpClient, Task<HttpResponseMessage>> acao = async (client) => {
                var response = await client.GetAsync(TARGETURL);
                return response;
            };

            return await EnviarRequisicao(TARGETURL, acao);
        }

        private async Task<Resultado> EnviarRequisicao(string TARGETURL, Func<HttpClient, Task<HttpResponseMessage>> acao)
        {
            Resultado result = null;
            HttpClient client = null;
            // .. DECLARE

            // .. VALIDATION
            ApiKeyFoiPreenchida();
            // .. VALIDATION

            // .. PROXY
            if (!string.IsNullOrWhiteSpace(Config.UrlProxy))
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    Proxy = new WebProxy(Config.UrlProxy),
                    UseProxy = true,
                };
                client = new HttpClient(handler);
            }
            else
            {
                client = new HttpClient();
            }
            // .. PROXY

            // ... LOGIN
            var byteArray = Encoding.ASCII.GetBytes($"{Config.APIKey}:token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            // ... LOGIN

            // .. REQUEST
            
            HttpResponseMessage response = await acao(client);
            // .. REQUEST


            // .. RETURN
            if (!response.IsSuccessStatusCode)
                result = await FillErrorResponse(response);
            else
                result = await FillSuccessResponse(response);

            return result;
            // .. RETURN
        }

        private async Task<Resultado> FillSuccessResponse(HttpResponseMessage response)
        {
            var Resultado = new Resultado();
            var pdfFile = await response.Content.ReadAsStreamAsync();
            Resultado.PDF = pdfFile;
            PreencherCabecalho(response, Resultado);
            return Resultado;
        }

        private async Task<Resultado> FillErrorResponse(HttpResponseMessage response)
        {
            var strCorpo = await response.Content.ReadAsStringAsync();
            Resultado Resultado = JsonConvert.DeserializeObject<Resultado>(strCorpo);
            PreencherCabecalho(response, Resultado);
            return Resultado;
        }

        private void PreencherCabecalho(HttpResponseMessage response, Resultado Resultado)
        {
            IEnumerable<string> valores;
            Resultado.StatusCode = response.StatusCode.ToString();
            response.Headers.TryGetValues("X-BoletoCloud-Token", out valores);
            if (valores != null)
                Resultado.Token = valores.FirstOrDefault();
            response.Headers.TryGetValues("Content-Type", out valores);
            if (valores != null)
                Resultado.ContentType = valores.FirstOrDefault();
            response.Headers.TryGetValues("Location", out valores);
            if (valores != null)
                Resultado.Location = valores.FirstOrDefault();
            response.Headers.TryGetValues("X-BoletoCloud-Version", out valores);
            if (valores != null)
                Resultado.Version = valores.FirstOrDefault();

        }
    }
}
