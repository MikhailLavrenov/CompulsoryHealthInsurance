using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace CHI.Services
{
    public abstract class WebServiceBase : IDisposable
    {
        #region Поля   
        private HttpClient client;
        private static readonly string UnauthorizedAccessErrorMessage = "Сначала необходимо авторизоваться.";
        #endregion

        #region Свойства
        public bool Authorized { get; protected set; }
        #endregion

        #region Конструкторы
        public WebServiceBase(string URL)
            : this(URL, null, 0)
        { }
        public WebServiceBase(string URL, string proxyAddress, int proxyPort)
        {
            Authorized = false;

            var clientHandler = new HttpClientHandler();
            clientHandler.CookieContainer = new CookieContainer();

            if (proxyAddress != null && proxyPort != 0)
            {
                clientHandler.UseProxy = true;
                clientHandler.Proxy = new WebProxy($"{proxyAddress}:{proxyPort}");
            }

            client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(URL);
        }
        #endregion

        #region Методы
        protected string SendRequest(HttpMethod httpMethod, string urn, IDictionary<string, string> contentParameters)
        {
            var requestMessage = new HttpRequestMessage(httpMethod, urn);

            if (httpMethod == HttpMethod.Post && contentParameters?.Count > 0)
                requestMessage.Content = new FormUrlEncodedContent(contentParameters);

            var response = client.SendAsync(requestMessage).ConfigureAwait(false).GetAwaiter().GetResult();

            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        protected void CheckAuthorization()
        {
            if (!Authorized)
                throw new UnauthorizedAccessException(UnauthorizedAccessErrorMessage);
        }
        public void Dispose()
        {
            client?.Dispose();
        }
        #endregion
    }
}
