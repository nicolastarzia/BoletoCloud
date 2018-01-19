﻿using System;
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

        private async Task<Resultado> EnviarRequisicao(string TARGETURL, Func<HttpClient, Task<HttpResponseMessage>> acao)
        {
            LancarErroSeApiKeyNaoFoiPreenchida();
            HttpClient client = CriarObjetoHttpClient();

            HttpResponseMessage response = await acao(client);
            return await PreencherObjetoSucessoOuErro(response);
        }

        private async Task<Resultado> PreencherObjetoSucessoOuErro(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                return await FillErrorResponse(response);
            else
                return await FillSuccessResponse(response);
        }

        private static HttpClient CriarObjetoHttpClient()
        {
            HttpClient client;
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

            var byteArray = Encoding.ASCII.GetBytes($"{Config.APIKey}:token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            return client;
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
        #endregion



        #region Public
        /// <summary>
        /// Criar um novo boleto, através dos parametros enviados
        /// </summary>
        /// <param name="boleto">Classe boleto, necessaria para cadastrar o novo Boleto</param>
        /// <returns>Em caso de sucesso, retorna o PDF preenchido, caso de erro a variavel Erro é preenchida</returns>
        public async Task<Resultado> Criar(Entities.Boleto boleto)
        {
            var TARGETURL = string.Concat(ConfigURIS.URI(), MODULO_BOLETOCLOUD);

            Func<HttpClient, Task<HttpResponseMessage>> criarBoleto = async (client) => {
                var boletoKeys = boleto.ToKeyValue();

                var response = await client.PostAsync(TARGETURL, new FormUrlEncodedContent(boletoKeys));
                return response;
            };

            return await EnviarRequisicao(TARGETURL, criarBoleto);
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

            var TARGETURL = string.Concat(ConfigURIS.URI(), MODULO_BOLETOCLOUD,"/", token);

            Func<HttpClient, Task<HttpResponseMessage>> acao = async (client) => {
                var response = await client.GetAsync(TARGETURL);
                return response;
            };

            return await EnviarRequisicao(TARGETURL, acao);
        }
        #endregion



    }
}
