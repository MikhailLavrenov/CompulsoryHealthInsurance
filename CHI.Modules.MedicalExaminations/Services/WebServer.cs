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
            drawCounter = 1;

            var clientHandler = new HttpClientHandler();
            clientHandler.CookieContainer = new CookieContainer();

            if (proxyAddress != null && proxyPort != 0)
            {
                clientHandler.UseProxy = true;
                clientHandler.Proxy = new WebProxy($"{proxyAddress}:{proxyPort}");
            }

            client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri(URL);
            client.Timeout = TimeSpan.FromMinutes(2);
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

            var isRequestSuccessful = TryPostRequest(@"account/login", requestValues, out var responseText);

            if (isRequestSuccessful && !string.IsNullOrEmpty(responseText) && !responseText.Contains(@"<li>Пользователь не найден</li>"))
                return Authorized = true;
            else
                return Authorized = false;
        }
        //поиск пациента в плане
        public bool TryFindPatientInPlan(string insuranceNumber, ExaminationType examinationType, int year, out WebPlanPatientData foundPatientData)
        {
            foundPatientData = null;

            if (!Authorized)
                throw new UnauthorizedAccessException("Сначала необходимо авторизоваться.");

            var uriParameters = new Dictionary<string, string>
            {
                {"Filter.Year", GetYearId(year) },
                {"Filter.PolisNum", insuranceNumber },
                {"Filter.DispType", ((int)examinationType).ToString() },
            };

            var uriParamentersString = new FormUrlEncodedContent(uriParameters).ReadAsStringAsync().Result;
            var urn = $@"disp/GetDispData?{uriParamentersString}";

            var contentParameters = new Dictionary<string, string>
            {
                {"columns[0][data]", "PersonId"},
                {"order[0][column]", "0"},
                {"length", "25"},
            };

            var isRequestSuccessful = TryPostRequest(urn, contentParameters, out var responseText);

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
        public bool TryDeletePatientFromPlan(int Id)
        {
            var contentParameters = new Dictionary<string, string>
            {
                { "Id", Id.ToString() }
            };

            var isRequestSuccessful = TryPostRequest(@"disp/removeDisp", contentParameters, out var responseText);

            if (isRequestSuccessful)
            {
                var response = new JavaScriptSerializer().Deserialize<WebResult>(responseText);

                if (response.IsError)
                    isRequestSuccessful = false;
            }

            return isRequestSuccessful;
        }
        public bool TryAddPatientToPlan(int PatientId, ExaminationType examinationType, int year)
        {
            var contentParameters = new Dictionary<string, string>
            {
                { "personId", PatientId.ToString() },
                { "yearId", GetYearId(year) },
                { "dispType", ((int)examinationType).ToString() },
            };

            var isRequestSuccessful = TryPostRequest(@"disp/AddToDisp", contentParameters, out var responseText);

            if (isRequestSuccessful)
            {
                var response = new JavaScriptSerializer().Deserialize<WebResult>(responseText);

                if (response.IsError)
                    isRequestSuccessful = false;
            }

            return isRequestSuccessful;
        }
        public bool TryFindPatientInSRZ(string insuranceNumber, int year, out int patientId)
        {
            patientId = 0;

            if (!Authorized)
                throw new UnauthorizedAccessException("Сначала необходимо авторизоваться.");

            var contentParameters = new Dictionary<string, string>
            {
                {"SearchData.DispYearId", GetYearId(year) },
                {"SearchData.SelectSearchValues", "polis"},
                {"SearchData.PolisNum", insuranceNumber }
            };

            var isRequestSuccessful = TryPostRequest(@"disp/SrzSearch", contentParameters, out var responseText);

            if (isRequestSuccessful)
            {
                var startIndex = responseText.IndexOf("personId") + 1;
                var idBegin = responseText.IndexOf('\"', startIndex) + 1;
                var idLength = responseText.IndexOf('\"', idBegin) - idBegin;
                var idString = responseText.Substring(idBegin, idLength);

                int.TryParse(idString, out patientId);
            }

            return patientId!=0;
        }
        public bool TryAddStep(ExaminationStage step, DateTime date,  HealthGroup healthGroup,Referral referralTo, int PatientId    )
        {
            var contentParameters = new Dictionary<string, string>
            {
                { "stageId", ((int)step).ToString() },
                { "stageDate", date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) },
                { "resultId", ((int)healthGroup).ToString()  },                               
                { "destId", ((int)referralTo).ToString() },
                { "dispId", PatientId.ToString() },
            };

            var isRequestSuccessful = TryPostRequest(@"disp/editDispStage", contentParameters, out var responseText);

            if (isRequestSuccessful)
            {
                var response = new JavaScriptSerializer().Deserialize<WebResult>(responseText);

                if (response.IsError)
                    isRequestSuccessful = false;
            }

            return isRequestSuccessful;
        }
        public bool TryDeleteLastStep(int PatientId)
        {
            var contentParameters = new Dictionary<string, string>
            {
                { "dispId", PatientId.ToString() },
            };

            var isRequestSuccessful = TryPostRequest(@"disp/deleteDispStage", contentParameters, out var responseText);

            if (isRequestSuccessful)
            {
                var response = new JavaScriptSerializer().Deserialize<WebResult>(responseText);

                if (response.IsError)
                    isRequestSuccessful = false;
            }

            return isRequestSuccessful;
        }

        private bool TryPostRequest(string urn, Dictionary<string, string> contentParameters, out string responseText)
        {
            try
            {
                var content = new FormUrlEncodedContent(contentParameters);

                var response = client.PostAsync(urn, content).GetAwaiter().GetResult();

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
        private static string GetYearId(int year)
        {
            return (year - 2017).ToString();
        }
        #endregion

        #region Классы для десериализации
        public class WebPlanPatientData
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
        public class PlanResponse
        {
            public List<WebPlanPatientData> Data { get; set; }
            //public int recordsFiltered { get; set; }
        }
        public class WebResult
        {
            public bool IsError { get; set; }
        }
        #endregion
    }
}
