using PatientsFomsRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CHI.Modules.MedicalExaminations.Services
{
    public class WebServer
    {
        #region Поля
        private HttpClient client;
        #endregion

        #region Свойства
        public Credential Credential { get; private set; }
        public bool Authorized { get; private set; }
        #endregion

        #region Конструкторы
        public WebServer(string URL)
            : this(URL, null, 0)
        { }
        public WebServer(string URL, string proxyAddress, int proxyPort)
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
        //авторизация на сайте
        public bool TryAuthorize(Credential credential)
        {
            Credential = credential;
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Login", credential.Login),
                new KeyValuePair<string, string>("Password", credential.Password),
            });

            try
            {
                var response = client.PostAsync(@"account/login", content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (!string.IsNullOrEmpty(responseText) && !responseText.Contains("<li>Пользователь не найден</li>"))
                    return Authorized = true;
                else
                    return Authorized = false;
            }
            catch (Exception)
            {
                return Authorized = false;
            }
        }

        #endregion
    }
}
