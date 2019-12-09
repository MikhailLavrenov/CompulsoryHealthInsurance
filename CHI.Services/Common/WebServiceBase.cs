using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;

namespace CHI.Services.Common
{
    /// <summary>
    /// Абстрактный базовый класс для реализации веб-сервисов 
    /// </summary>
    public abstract class WebServiceBase : IDisposable
    {
        #region Поля   
        private HttpClient client;
        #endregion

        #region Свойства
        /// <summary>
        /// Авторизация пройдена успешно
        /// </summary>
        public bool IsAuthorized { get; protected set; }
        #endregion

        #region Конструкторы
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="URL">URL</param>
        /// <param name="useProxy">Использовать прокси-сервер</param>
        /// <param name="proxyAddress">Адрес прокси-сервера</param>
        /// <param name="proxyPort">Порт прокси-сервера</param>
        public WebServiceBase(string URL, bool useProxy, string proxyAddress = null, int? proxyPort = null)
        {
            IsAuthorized = false;

            var clientHandler = new HttpClientHandler();
            clientHandler.CookieContainer = new CookieContainer();

            if (useProxy)
            {
                clientHandler.UseProxy = true;
                clientHandler.Proxy = new WebProxy($"{proxyAddress}:{proxyPort}");
            }

            client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(URL);
            client.Timeout = new TimeSpan(0, 2, 0);
        }
        #endregion

        #region Методы
        /// <summary>
        /// Отправка HTTP-запроса
        /// </summary>
        /// <param name="httpMethod">Метод HTTP запроса</param>
        /// <param name="urn">Относительный адрес</param>
        /// <param name="contentParameters">Параметры тела запроса</param>
        /// <returns>HTTP ответ в виде строки текста</returns>
        protected string SendRequest(HttpMethod httpMethod, string urn, IDictionary<string, string> contentParameters)
        {
            var requestMessage = new HttpRequestMessage(httpMethod, urn);

            if (httpMethod == HttpMethod.Post && contentParameters?.Count > 0)
                requestMessage.Content = new FormUrlEncodedContent(contentParameters);

            var response = client.SendAsync(requestMessage).ConfigureAwait(false).GetAwaiter().GetResult();

            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Отправка HTTP GET-запроса
        /// </summary>
        /// <param name="urn">Относительный адрес</param>
        /// <returns>HTTP ответ в виде потока</returns>
        protected Stream SendGetRequest(string urn)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, urn);

            var response = client.SendAsync(requestMessage).ConfigureAwait(false).GetAwaiter().GetResult();

            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Проверяет авторизацию, вызывает исключение, если авторизация не пройдена
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">
        ///  Возникает если авторизация не была пройдена успешно.
        /// </exception>
        protected void CheckAuthorization()
        {
            if (!IsAuthorized)
                throw new UnauthorizedAccessException("Сначала необходимо авторизоваться.");
        }
        /// <summary>
        /// Освобождает неуправляемые ресурсы.
        /// </summary>
        public virtual void Dispose()
        {
            client?.Dispose();
        }
        #endregion
    }
}
