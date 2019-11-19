using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Script.Serialization;

namespace CHI.Services.MedicalExaminations
{
    public class ExaminationServiceApi : WebServiceBase
    {
        #region Поля        
        private static readonly string ParseResponseErrorMessage = "Ошибка разбора ответа от web-сервера";
        #endregion

        #region Конструкторы
        protected ExaminationServiceApi(string URL)
            : base(URL)
        { }
        protected ExaminationServiceApi(string URL, string proxyAddress, int proxyPort)
            : base(URL, proxyAddress, proxyPort)
        { }
        #endregion

        #region Методы
        //авторизация на сайте
        public bool Authorize(string login, string password)
        {
            var requestValues = new Dictionary<string, string> {
                { "Login",      login    },
                { "Password",   password }
            };

            var responseText = SendRequest(HttpMethod.Post, @"account/login", requestValues);

            if (!string.IsNullOrEmpty(responseText) && !responseText.Contains(@"<li>Пользователь не найден</li>"))
                return Authorized = true;
            else
                return Authorized = false;
        }
        public void Logout()
        {
            SendRequest(HttpMethod.Get, @"account/logout", null);

        }
        //поиск пациента в плане
        protected WebPatientData GetPatientDataFromPlan(string insuranceNumber, ExaminationKind examinationType, int year)
        {
            CheckAuthorization();

            var uriParameters = new Dictionary<string, string> {
                {"Filter.Year", ConvertToYearId(year) },
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

            var responseText = SendRequest(HttpMethod.Post, urn, contentParameters);

            var planResponse = new JavaScriptSerializer().Deserialize<PlanResponse>(responseText);

            if (planResponse != null)
                return planResponse.Data?.FirstOrDefault();
            else
                throw new InvalidOperationException(ParseResponseErrorMessage);
        }
        protected void AddPatientToPlan(int srzPatientId, ExaminationKind examinationType, int year)
        {
            CheckAuthorization();

            var contentParameters = new Dictionary<string, string>
            {
                { "personId", srzPatientId.ToString() },
                { "yearId", ConvertToYearId(year) },
                { "dispType", ((int)examinationType).ToString() },
            };

            var responseText = SendRequest(HttpMethod.Post, @"disp/AddToDisp", contentParameters);

            var response = new JavaScriptSerializer().Deserialize<WebResponse>(responseText);

            if (response.IsError)
                throw new WebServerOperationException();
        }
        protected void DeletePatientFromPlan(int patientId)
        {
            CheckAuthorization();

            var contentParameters = new Dictionary<string, string>
            {
                { "Id", patientId.ToString() }
            };

            var responseText = SendRequest(HttpMethod.Post, @"disp/removeDisp", contentParameters);

            var response = new JavaScriptSerializer().Deserialize<WebResponse>(responseText);

            if (response.IsError)
                throw new WebServerOperationException();
        }        
        protected int GetPatientFromSRZ(string insuranceNumber, int year)
        {
            CheckAuthorization();

            var contentParameters = new Dictionary<string, string>
            {
                {"SearchData.DispYearId", ConvertToYearId(year) },
                {"SearchData.SelectSearchValues", "polis"},
                {"SearchData.PolisNum", insuranceNumber }
            };

            var responseText = SendRequest(HttpMethod.Post, @"disp/SrzSearch", contentParameters);

            var startIndex = responseText.IndexOf("personId") + 1;
            var idBegin = responseText.IndexOf('\"', startIndex) + 1;
            var idLength = responseText.IndexOf('\"', idBegin) - idBegin;
            var idString = responseText.Substring(idBegin, idLength);

            if (int.TryParse(idString, out var srzPatientId))
                return srzPatientId;
            else
                throw new InvalidOperationException(ParseResponseErrorMessage);
        }
        protected List<AvailableStage> GetAvailableSteps(int patientId)
        {
            var contentParameters = new Dictionary<string, string>
            {
                { "dispId", patientId.ToString() },
            };

            var responseText = SendRequest(HttpMethod.Post, @"/disp/getAvailableStages", contentParameters);

            var availableStagesResponse = new JavaScriptSerializer().Deserialize<AvailableStagesResponse>(responseText);

            return availableStagesResponse?.AvailableStages?? new List<AvailableStage>();
        }
        protected void AddStep(int patientId, ExaminationStepKind step, DateTime date, ExaminationHealthGroup healthGroup, ExaminationReferral referralTo)
        {
            CheckAuthorization();

            var contentParameters = new Dictionary<string, string>
            {
                { "stageId", ((int)step).ToString() },
                { "stageDate", date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) },
                { "resultId", ((int)healthGroup).ToString()  },
                { "destId", referralTo==ExaminationReferral.None? string.Empty:((int)referralTo).ToString() },
                { "dispId", patientId.ToString() },
            };

            var responseText = SendRequest(HttpMethod.Post, @"disp/editDispStage", contentParameters);

            var response = new JavaScriptSerializer().Deserialize<WebResponse>(responseText);

            if (response.IsError)
                throw new WebServerOperationException();
        }        
        protected ExaminationStepKind DeleteLastStep(int patientId)
        {
            CheckAuthorization();

            var contentParameters = new Dictionary<string, string> {
                { "dispId", patientId.ToString() },
            };

            var responseText = SendRequest(HttpMethod.Post, @"disp/deleteDispStage", contentParameters);

            var response = new JavaScriptSerializer().Deserialize<DeleteLastStepResponse>(responseText);

            if (response.IsError)
                throw new WebServerOperationException();

            return response.Data?.FirstOrDefault()?.DispStage?.DispStageId ?? 0;
        }        
        protected static string ConvertToYearId(int year)
        {
            return (year - 2017).ToString();
        }
        #endregion

        #region Классы для десериализации
        public class WebPatientData
        {
            public int Id { get; set; }
            public int PersonId { get; set; }
            public int YearId { get; set; }
            public int Month { get; set; }
            public DateTime? Disp1BeginDate { get; set; }
            public DateTime? Disp1Date { get; set; }
            public int? Stage1ResultId { get; set; }
            public int? Stage1DestId { get; set; }
            public DateTime? Disp2DirectDate { get; set; }
            public DateTime? Disp2BeginDate { get; set; }
            public DateTime? Disp2Date { get; set; }
            public int? Stage2ResultId { get; set; }
            public int? Stage2DestId { get; set; }
            public DateTime? DispSuccessDate { get; set; }
            public DateTime? DispCancelDate { get; set; }
            public string Polis_Num { get; set; }
            public string Person_ENP { get; set; }
            public int DispType { get; set; }
        }
        protected class PlanResponse
        {
            public List<WebPatientData> Data { get; set; }
        }
        protected class WebResponse
        {
            public bool IsError { get; set; }
        }
        public class AvailableStagesResponse
        {
            public List<AvailableStage> AvailableStages { get; set; }
        }
        public class AvailableStage
        {
            public ExaminationStepKind StageId { get; set; }
            public int NextStageId { get; set; }
            //public string StageName { get; set; }
            public object PreviousStageId { get; set; }
        }

        public class DeletedStep
        {
            //public int Id { get; set; }
            //public int DispPersonId { get; set; }
            public ExaminationStepKind DispStageId { get; set; }
            //public string DispStageName { get; set; }
            //public string DispStageOrgCode { get; set; }
            //public string DispStageOrgName { get; set; }
            //public DateTime DispStageDate { get; set; }
            //public object DispStageResult { get; set; }
            //public DateTime UpdateDate { get; set; }
            //public int YearId { get; set; }
        }
        public class DeletedData
        {
            //public int DispStageType { get; set; }
            public DeletedStep DispStage { get; set; }
            //public object DispResult { get; set; }
        }
        public class DeleteLastStepResponse
        {
            public bool IsError { get; set; }
            public List<DeletedData> Data { get; set; }
        }
        #endregion
    }
}
