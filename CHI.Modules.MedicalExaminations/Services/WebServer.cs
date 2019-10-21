using CHI.Modules.MedicalExaminations.Models;
using PatientsFomsRepository.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;

namespace CHI.Modules.MedicalExaminations.Services
{
    public class WebServer
    {
        #region Поля        
        private HttpClient client;
        private int drawCounter;
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
            drawCounter = 0;

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

            var requestValues = new Dictionary<string, string> {
                { "Login",      credential.Login    },
                { "Password",   credential.Password }
            };

            var isRequestSuccessful = TryPostReqest(@"account/login", requestValues, out var responseText);

            if (isRequestSuccessful && !string.IsNullOrEmpty(responseText) && !responseText.Contains(@"<li>Пользователь не найден</li>"))
                return Authorized = true;
            else
                return Authorized = false;
        }
        public bool TryFindPatientInPlan(string insuranceNumber, ExaminationType examinationType, int year)
        {
            var builder = new UriBuilder(@"/disp/GetDispData");
            builder.Port = -1;

            var query = HttpUtility.ParseQueryString(builder.Query);

            query["Filter.Year"] = GetYearId(year);
            query["Filter.Smo"] = "";
            query["Filter.PolisNum"] = insuranceNumber;
            query["Filter.PersonId"] = "";
            query["Filter.Surname"] = "";
            query["Filter.Firstname"] = "";
            query["Filter.Secname"] = "";
            query["Filter.Region"] = "";
            query["Filter.AnotherAttachment"] = "false";
            query["Filter.Stage"] = "";
            query["Filter.StageEmpty"] = "false";
            query["Filter.StageFrom"] = "";
            query["Filter.StageTo"] = "";
            query["Filter.Result1Id"] = "";
            query["Filter.Dest1Id"] = "";
            query["Filter.Result2Id"] = "";
            query["Filter.Dest2Id"] = "";
            query["Filter.BillsDispStage"] = "";
            query["Filter.BillsDispStageEmpty"] = "false";
            query["Filter.BillsFrom"] = "";
            query["Filter.BillsTo"] = "";
            query["Filter.DispType"] = ((int)examinationType).ToString();

            builder.Query = query.ToString();
            string urn = builder.ToString();

            var requestBodyValues = new Dictionary<string, string>
            {
                { "draw",   drawCounter++.ToString()    },
                { "start",  "0"                         },
                { "length", "25"                        },
            };

            var isRequestSuccessful = TryPostReqest(urn, requestBodyValues, out var responseText);

            return true;

            //// Query string parameters
            //var queryString = new Dictionary<string, string>() { { "foo", "bar" } };

            //// Create json for body
            //var content = new JObject(json);

            //// Create HttpClient
            //var client = new HttpClient();
            //client.BaseAddress = new Uri("https://api.baseaddress.com/");

            //// This is the missing piece
            //var requestUri = QueryHelpers.AddQueryString("something", queryString);

            //var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            //// Setup header(s)
            //request.Headers.Add("Accept", "application/json");
            //// Add body content
            //request.Content = new StringContent(
            //    content.ToString(),
            //    Encoding.UTF8,
            //    "application/json"
            //);

            //// Send the request
            //client.SendAsync(request);
        }
        private static string GetYearId(int year) => (year - 2017).ToString();
        private bool TryPostReqest(string URN, Dictionary<string, string> requestBodyValues, out string responseText)
        {
            try
            {
                var content = new FormUrlEncodedContent(requestBodyValues);

                var response = client.PostAsync(URN, content).GetAwaiter().GetResult();

                response.EnsureSuccessStatusCode();

                responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                return true;
            }
            catch (Exception)
            {
                responseText = string.Empty;
                return false;
            }
        }

        #endregion
    }
}
