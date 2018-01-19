using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace BoletoCloud
{
    /// <summary>
    /// Classe responsavel por conectar no servico BoletoCloud e efetuar todas as operações
    /// </summary>
    public class Boleto
    {
        #region Private
        private const string MODULO_BOLETOCLOUD = "/boletos";

        private void LancarErroSeApiKeyNaoFoiPreenchida()
        {
            if (string.IsNullOrEmpty(Config.APIKey))
                throw new ArgumentNullException("APIKey", "É obrigatório preencher o parametro APIKey");
        }

        private async Task<Resultado> EnviarRequisicao(Func<HttpClient, Task<HttpResponseMessage>> acao)
        {
            LancarErroSeApiKeyNaoFoiPreenchida();
            HttpClient clienteHttp = CriarObjetoHttpClient();

            HttpResponseMessage mensagemResposta = await acao(clienteHttp);
            return await PreencherObjetoSucessoOuErro(mensagemResposta);
        }

        private async Task<Resultado> PreencherObjetoSucessoOuErro(HttpResponseMessage mensagemRetorno)
        {
            if (!mensagemRetorno.IsSuccessStatusCode)
                return await PreencherErro(mensagemRetorno);
            else
                return await PreencherSucesso(mensagemRetorno);
        }

        private static HttpClient CriarObjetoHttpClient()
        {
            HttpClient clienteHttp;
            if (!string.IsNullOrWhiteSpace(Config.UrlProxy))
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    Proxy = new WebProxy(Config.UrlProxy),
                    UseProxy = true,
                };
                clienteHttp = new HttpClient(handler);
            }
            else
            {
                clienteHttp = new HttpClient();
            }

            var senhaBase64 = Encoding.ASCII.GetBytes($"{Config.APIKey}:token");
            clienteHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(senhaBase64));

            return clienteHttp;
        }

        private async Task<Resultado> PreencherSucesso(HttpResponseMessage mensagemResposta)
        {
            var Resultado = new Resultado();
            var arquivoPDF = await mensagemResposta.Content.ReadAsStreamAsync();
            Resultado.PDF = arquivoPDF;
            PreencherCabecalho(mensagemResposta, Resultado);
            return Resultado;
        }

        private async Task<Resultado> PreencherErro(HttpResponseMessage mensagemResposta)
        {
            var strCorpo = await mensagemResposta.Content.ReadAsStringAsync();
            Resultado Resultado = JsonConvert.DeserializeObject<Resultado>(strCorpo);
            PreencherCabecalho(mensagemResposta, Resultado);
            return Resultado;
        }

        private void PreencherCabecalho(HttpResponseMessage mensagemResposta, Resultado Resultado)
        {
            Resultado.StatusCode = mensagemResposta.StatusCode.ToString();
            Resultado.Token = PreencherValorCabecalho(mensagemResposta, "X-BoletoCloud-Token");
            Resultado.ContentType = PreencherValorCabecalho(mensagemResposta, "Content-Type");
            Resultado.Location = PreencherValorCabecalho(mensagemResposta, "Location");
            Resultado.Version = PreencherValorCabecalho(mensagemResposta, "X-BoletoCloud-Version");
        }

        private string PreencherValorCabecalho(HttpResponseMessage mensagemResposta, string chave)
        {
            IEnumerable<string> valores;
            mensagemResposta.Headers.TryGetValues(chave, out valores);
            if (valores != null)
                return valores.FirstOrDefault();
            return "";
        }
        #endregion



        #region Public
        /// <summary>
        /// Criar um novo boleto, através dos parametros enviados
        /// </summary>
        /// <param name="boleto">Classe boleto, necessaria para cadastrar o novo Boleto</param>
        /// <returns>Em caso de sucesso, retorna o PDF preenchido, caso de erro a variavel Erro é preenchida</returns>
        public async Task<Resultado> Criar(Entities.Boleto boleto)
        {
            Func<HttpClient, Task<HttpResponseMessage>> criarBoleto = async (clienteHttp) => {
                var boletoFormatoQueryString = boleto.ToKeyValue();
                var URLDEFAULT = string.Concat(ConfigURIS.URI(), MODULO_BOLETOCLOUD);
                var mensagemResposta = await clienteHttp.PostAsync(URLDEFAULT, new FormUrlEncodedContent(boletoFormatoQueryString));
                return mensagemResposta;
            };

            return await EnviarRequisicao(criarBoleto);
        }

        /// <summary>
        /// Buscar o boleto através de um token existente
        /// </summary>
        /// <param name="token">Token do boleto gerado</param>
        /// <returns>Em caso de sucesso, retorna o PDF preenchido, caso de erro a variavel Erro é preenchida</returns>
        public async Task<Resultado> Buscar(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException("Token", "Preencher o parametro Token");

            

            Func<HttpClient, Task<HttpResponseMessage>> consultarBoleto = async (clienteHttp) => {
                var URLDEFAULT = string.Concat(ConfigURIS.URI(), MODULO_BOLETOCLOUD, "/", token);
                var mensagemResposta = await clienteHttp.GetAsync(URLDEFAULT);
                return mensagemResposta;
            };

            return await EnviarRequisicao(consultarBoleto);
        }
        #endregion



    }
}
