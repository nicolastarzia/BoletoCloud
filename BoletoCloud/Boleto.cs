using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

namespace BoletoCloud
{
    public class Boleto
    {
        private const string MODULO_BOLETOCLOUD = "/boletos/";

        private void ApiKeyFoiPreenchida()
        {
            if (string.IsNullOrEmpty(Config.APIKey))
                throw new System.ArgumentNullException("APIKey", "É obrigatório preencher o parametro APIKey");
        }

        public async Task<Resultado> Criar(Entities.Boleto boleto)
        {
            var TARGETURL = string.Concat(ConfigURIS.URI(), MODULO_BOLETOCLOUD);

            Func<HttpClient, Task<HttpResponseMessage>> acao = async (client) => {
                string boletoJSON = JsonConvert.SerializeObject(boleto);
                var response = await client.PostAsync(TARGETURL, new StringContent(boletoJSON));
                return response;
            };

            return await EnviarRequisicao(TARGETURL, acao);
        }

        public async Task<Resultado> Buscar(string token)
        {
            var TARGETURL = string.Concat(ConfigURIS.URI(), MODULO_BOLETOCLOUD, token);

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
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
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
            Resultado.Token = response.Headers.GetValues("X-BoletoCloud-Token").FirstOrDefault();
            Resultado.ContentType = response.Headers.GetValues("Content-Type").FirstOrDefault();
            Resultado.Location = response.Headers.GetValues("Location").FirstOrDefault();
            Resultado.Version = response.Headers.GetValues("X-BoletoCloud-Version").FirstOrDefault();

            return Resultado;
        }

        private async Task<Resultado> FillErrorResponse(HttpResponseMessage response)
        {
            var Resultado = new Resultado();
            var strCorpo = await response.Content.ReadAsStringAsync();
            Entities.Error err = JsonConvert.DeserializeObject<Entities.Error>(strCorpo);
            Resultado.Error = err;
            Resultado.StatusCode = response.StatusCode.ToString();
            Resultado.ContentType = response.Headers.GetValues("Content-Type").FirstOrDefault();
            return Resultado;
        }
    }
}
