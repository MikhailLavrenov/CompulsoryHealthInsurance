using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CHI.Services.Common
{
    /// <summary>
    /// Абстрактный базовый класс для реализации веб-сервисов 
    /// </summary>
    public abstract class WebServiceBase : IDisposable
    {
        HttpClient client;


        public bool IsAuthorized { get; protected set; }


        public WebServiceBase(string URL, bool useProxy, string proxyAddress = null, int? proxyPort = null)
        {
            IsAuthorized = false;

            var clientHandler = new HttpClientHandler();
            clientHandler.CookieContainer = new CookieContainer();
            clientHandler.UseProxy = useProxy;
            clientHandler.Proxy = useProxy ? new WebProxy($"{proxyAddress}:{proxyPort}") : null;

            client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(URL);
            client.Timeout = new TimeSpan(0, 2, 0);
        }


        protected async Task<string> SendGetTextAsync(string urn)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, urn);

            var response = await client.SendAsync(requestMessage);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        protected async Task<string> SendPostAsync(string urn, IDictionary<string, string> contentParameters)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, urn);

            if ((contentParameters?.Count ?? 0) > 0)
                requestMessage.Content = new FormUrlEncodedContent(contentParameters);

            var response = await client.SendAsync(requestMessage);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        protected async Task<Stream> SendGetStreamAsync(string urn)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, urn);

            var response = await client.SendAsync(requestMessage);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }

        protected void ThrowExceptionIfNotAuthorized ()
        {
            if (!IsAuthorized)
                throw new UnauthorizedAccessException("Сначала необходимо авторизоваться.");
        }

        public virtual void Dispose()
        {
            client?.Dispose();
        }
    }
}
