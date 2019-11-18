﻿using CHI.Modules.MedicalExaminations.Models;
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
        private static readonly string ParseResponseErrorMessage = "Ошибка разбора ответа от web-сервера";
        private static readonly string SRZNotFoundErrorMessage = "Пациент не найден в СРЗ";
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

            if (planResponse?.Data?.Count == 1)
                return planResponse.Data.First();
            else
                throw new InvalidOperationException(ParseResponseErrorMessage);
        }
        protected void DeletePatientFromPlan(int patientId)
        {
            CheckAuthorization();

            var contentParameters = new Dictionary<string, string>
            {
                { "Id", patientId.ToString() }
            };

            var responseText = SendRequest(HttpMethod.Post, @"disp/removeDisp", contentParameters);

            var response = new JavaScriptSerializer().Deserialize<WebResult>(responseText);

            if (response.IsError)
                throw new WebServerOperationException();
        }
        protected void TryDeletePatientFromPlan(int patientId)
        {
            try
            {
                DeletePatientFromPlan(patientId);
            }
            catch (WebServerOperationException)
            {
            }
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

            var response = new JavaScriptSerializer().Deserialize<WebResult>(responseText);

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
            {
                if (srzPatientId != 0)
                    return srzPatientId;
                else
                    throw new InvalidOperationException(SRZNotFoundErrorMessage);
            }
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

            return availableStagesResponse?.AvailableStages ?? new List<AvailableStage>();
        }
        protected void AddStep(int patientId, ExaminationStepKind step, DateTime date, HealthGroup healthGroup, Referral referralTo)
        {
            CheckAuthorization();

            var contentParameters = new Dictionary<string, string>
            {
                { "stageId", ((int)step).ToString() },
                { "stageDate", date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) },
                { "resultId", ((int)healthGroup).ToString()  },
                { "destId", referralTo==Referral.None? string.Empty:((int)referralTo).ToString() },
                { "dispId", patientId.ToString() },
            };

            var responseText = SendRequest(HttpMethod.Post, @"disp/editDispStage", contentParameters);

            var response = new JavaScriptSerializer().Deserialize<WebResult>(responseText);

            if (response.IsError)
                throw new WebServerOperationException();
        }
        protected void AddStep(int patientId, ExaminationStep examinationStep)
        {
            AddStep(patientId, examinationStep.ExaminationStepKind, examinationStep.Date, examinationStep.HealthGroup, examinationStep.Referral);
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

            var currentStep = response.Data?.FirstOrDefault()?.DispStage?.DispStageId;

            if (currentStep == null)
                throw new InvalidOperationException(ParseResponseErrorMessage);

            return currentStep.Value;
        }
        //protected bool TryDeleteLastStep(int patientId)
        //{
        //    try
        //    {
        //        DeleteLastStep(patientId);
        //        return true;
        //    }
        //    catch (WebServerOperationException)
        //    {
        //        return false;
        //    }
        //}
        protected void DeleteStepsTo(int patientId, ExaminationStepKind stepTo)
        {
            while (DeleteLastStep(patientId) != stepTo) ;
        }
        protected static string ConvertToYearId(int year)
        {
            return (year - 2017).ToString();
        }
        private string SendRequest(HttpMethod httpMethod, string urn, Dictionary<string, string> contentParameters)
        {
            var requestMessage = new HttpRequestMessage(httpMethod, urn);

            if (httpMethod == HttpMethod.Post && contentParameters?.Count > 0)
                requestMessage.Content = new FormUrlEncodedContent(contentParameters);

            var response = client.SendAsync(requestMessage).GetAwaiter().GetResult();

            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }
        private void CheckAuthorization()
        {
            if (!Authorized)
                throw new UnauthorizedAccessException("Сначала необходимо авторизоваться.");
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

            public byte GetStepsCount()
            {
                byte count = 0;

                if (Disp1BeginDate != default)
                    count++;

                if (Disp1Date != default)
                    count++;

                if (Disp1Date != default && Stage1ResultId != default && Stage1DestId != default)
                    count++;

                if (Disp2DirectDate != default)
                    count++;

                if (Disp2BeginDate != default)
                    count++;

                if (Disp2Date != default)
                    count++;

                if (Disp2Date != default && Stage2ResultId != default && Stage2DestId != default)
                    count++;

                if (DispCancelDate != default)
                    count++;

                return count;
            }
        }
        protected class PlanResponse
        {
            public List<WebPatientData> Data { get; set; }
        }
        protected class WebResult
        {
            public bool IsError { get; set; }
        }
        public class AvailableStagesResponse
        {
            public List<AvailableStage> AvailableStages { get; set; }
        }
        public class AvailableStage
        {
            public int StageId { get; set; }
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
