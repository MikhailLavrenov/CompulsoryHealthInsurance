using CHI.Modules.MedicalExaminations.Models;
using PatientsFomsRepository.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Script.Serialization;

namespace CHI.Modules.MedicalExaminations.Services
{
    public class WebSiteApi
    {
        #region Поля        
        private HttpClient client;
        #endregion

        #region Свойства
        public bool Authorized { get; protected set; }
        #endregion

        #region Конструкторы
        protected WebSiteApi(string URL)
            : this(URL, null, 0)
        { }
        protected WebSiteApi(string URL, string proxyAddress, int proxyPort)
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
        public bool TryAuthorize(string login, string password)
        {
            var requestValues = new Dictionary<string, string> {
                { "Login",      login    },
                { "Password",   password }
            };

            var isRequestSuccessful = TrySendRequest(HttpMethod.Post, @"account/login", requestValues, out var responseText);

            if (isRequestSuccessful && !string.IsNullOrEmpty(responseText) && !responseText.Contains(@"<li>Пользователь не найден</li>"))
                return Authorized = true;
            else
                return Authorized = false;
        }
        public bool TryLogout()
        {
            return TrySendRequest(HttpMethod.Get, @"account/logout", null, out _);

        }
        //поиск пациента в плане
        protected bool TryGetPatientDataFromPlan(string insuranceNumber, ExaminationType examinationType, int year, out WebPatientData foundPatientData)
        {
            foundPatientData = null;

            //if (!Authorized)
            //    throw new UnauthorizedAccessException("Сначала необходимо авторизоваться.");

            var uriParameters = new Dictionary<string, string> {
                {"Filter.Year", GetYearId(year) },
                {"Filter.PolisNum", insuranceNumber },
                {"Filter.DispType", ((int)examinationType).ToString() },
            };

            var uriParamentersString = new FormUrlEncodedContent(uriParameters).ReadAsStringAsync().Result;
            var urn = $@"disp/GetDispData?{uriParamentersString}";

            var contentParameters = new Dictionary<string, string> {
                {"columns[0][data]", "PersonId"},
                {"order[0][column]", "0"},
                {"length", "25"},
            };

            var isRequestSuccessful = TrySendRequest(HttpMethod.Post, urn, contentParameters, out var responseText);

            if (isRequestSuccessful)
            {
                var planResponse = new JavaScriptSerializer().Deserialize<PlanResponse>(responseText);

                if (planResponse?.Data?.Count == 1)
                    foundPatientData = planResponse.Data.FirstOrDefault();
                else
                    isRequestSuccessful = false;
            }

            return isRequestSuccessful;
        }
        protected bool TryDeletePatientFromPlan(int patientId)
        {
            var contentParameters = new Dictionary<string, string>
            {
                { "Id", patientId.ToString() }
            };

            var isRequestSuccessful = TrySendRequest(HttpMethod.Post, @"disp/removeDisp", contentParameters, out var responseText);

            if (isRequestSuccessful)
            {
                var response = new JavaScriptSerializer().Deserialize<WebResult>(responseText);

                if (response.IsError)
                    isRequestSuccessful = false;
            }

            return isRequestSuccessful;
        }
        protected bool TryAddPatientToPlan(int srzPatientId, ExaminationType examinationType, int year)
        {
            var contentParameters = new Dictionary<string, string>
            {
                { "personId", srzPatientId.ToString() },
                { "yearId", GetYearId(year) },
                { "dispType", ((int)examinationType).ToString() },
            };

            var isRequestSuccessful = TrySendRequest(HttpMethod.Post, @"disp/AddToDisp", contentParameters, out var responseText);

            if (isRequestSuccessful)
            {
                var response = new JavaScriptSerializer().Deserialize<WebResult>(responseText);

                if (response.IsError)
                    isRequestSuccessful = false;
            }

            return isRequestSuccessful;
        }
        protected bool TryGetPatientFromSRZ(string insuranceNumber, int year, out int srzPatientId)
        {
            srzPatientId = 0;

            if (!Authorized)
                throw new UnauthorizedAccessException("Сначала необходимо авторизоваться.");

            var contentParameters = new Dictionary<string, string>
            {
                {"SearchData.DispYearId", GetYearId(year) },
                {"SearchData.SelectSearchValues", "polis"},
                {"SearchData.PolisNum", insuranceNumber }
            };

            var isRequestSuccessful = TrySendRequest(HttpMethod.Post, @"disp/SrzSearch", contentParameters, out var responseText);

            if (isRequestSuccessful)
            {
                var startIndex = responseText.IndexOf("personId") + 1;
                var idBegin = responseText.IndexOf('\"', startIndex) + 1;
                var idLength = responseText.IndexOf('\"', idBegin) - idBegin;
                var idString = responseText.Substring(idBegin, idLength);

                int.TryParse(idString, out srzPatientId);
            }

            return srzPatientId != 0;
        }
        protected bool TryAddStep(ExaminationStep step, DateTime date, HealthGroup healthGroup, Referral referralTo, int patientId)
        {
            var contentParameters = new Dictionary<string, string>
            {
                { "stageId", ((int)step).ToString() },
                { "stageDate", date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) },
                { "resultId", ((int)healthGroup).ToString()  },
                { "destId", ((int)referralTo).ToString() },
                { "dispId", patientId.ToString() },
            };

            var isRequestSuccessful = TrySendRequest(HttpMethod.Post, @"disp/editDispStage", contentParameters, out var responseText);

            if (isRequestSuccessful)
            {
                var response = new JavaScriptSerializer().Deserialize<WebResult>(responseText);

                if (response.IsError)
                    isRequestSuccessful = false;
            }

            return isRequestSuccessful;
        }
        protected bool TryDeleteLastStep(int patientId)
        {
            var contentParameters = new Dictionary<string, string> {
                { "dispId", patientId.ToString() },
            };

            var isRequestSuccessful = TrySendRequest(HttpMethod.Post, @"disp/deleteDispStage", contentParameters, out var responseText);

            if (isRequestSuccessful)
            {
                var response = new JavaScriptSerializer().Deserialize<WebResult>(responseText);

                if (response.IsError)
                    isRequestSuccessful = false;
            }

            return isRequestSuccessful;
        }
        protected static string GetYearId(int year)
        {
            return (year - 2017).ToString();
        }
        private bool TrySendRequest(HttpMethod httpMethod, string urn, Dictionary<string, string> contentParameters, out string responseText)
        {
            try
            {
                var requestMessage = new HttpRequestMessage(httpMethod, urn);

                if (httpMethod == HttpMethod.Post && contentParameters?.Count > 0)
                    requestMessage.Content = new FormUrlEncodedContent(contentParameters);

                var response = client.SendAsync(requestMessage).GetAwaiter().GetResult();

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

        #region Классы для десериализации
        protected class WebPatientData
        {
            public int Id { get; set; }
            public int PersonId { get; set; }
            public int YearId { get; set; }
            public int Month { get; set; }
            public DateTime? Disp1BeginDate { get; set; }
            public DateTime? Disp1Date { get; set; }
            public DateTime? Disp2DirectDate { get; set; }
            public DateTime? Disp2BeginDate { get; set; }
            public DateTime? Disp2Date { get; set; }
            public DateTime? DispCancelDate { get; set; }
            public DateTime? DispSuccessDate { get; set; }
            public object Stage1ResultId { get; set; }
            public object Stage1ResultName { get; set; }
            public object Stage1DestId { get; set; }
            public object Stage1DestName { get; set; }
            public object Stage2ResultId { get; set; }
            public object Stage2ResultName { get; set; }
            public object Stage2DestId { get; set; }
            public object Stage2DestName { get; set; }
            public string Polis_Num { get; set; }
            public string Person_ENP { get; set; }
            public int DispType { get; set; }
        }
        protected class PlanResponse
        {
            public List<WebPatientData> Data { get; set; }
            //public int recordsFiltered { get; set; }
        }
        protected class WebResult
        {
            public bool IsError { get; set; }
        }
        #endregion
    }
}
