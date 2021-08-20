using CHI.Infrastructure;
using CHI.Models;
using CHI.Services.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет базовые операции с веб-порталом диспансеризации
    /// </summary>

    public class ExaminationServiceApi : WebServiceBase
    {
        /// <summary>
        /// ФОМС код медицинской организации
        /// </summary>
        public string FomsCodeMO { get; private set; }


        protected ExaminationServiceApi(string URL, bool useProxy, string proxyAddress = null, int? proxyPort = null)
            : base(URL, useProxy, proxyAddress, proxyPort)
        { }


        /// <summary>
        /// Авторизация на сайте. При успешной авторизации инициализирует FomsCodeMO.
        /// </summary>
        /// <param name="credential">Учетные данные.</param>
        /// <returns>true-в случае успешной авторизации, false-иначе.</returns>
        public async Task<bool> AuthorizeAsync(ICredential credential)
        {
            var requestValues = new Dictionary<string, string> {
                { "Login",      credential.Login    },
                { "Password",   credential.Password }
            };

            var responseText = await SendPostAsync(@"account/login", requestValues);

            if (!string.IsNullOrEmpty(responseText) && !responseText.Contains(@"<li>Пользователь не найден</li>"))
            {
                FomsCodeMO = responseText.SubstringBetween("<div>", "<div>", "</div>");
                return IsAuthorized = true;
            }
            else
            {
                FomsCodeMO = null;
                return IsAuthorized = false;
            }
        }

        /// <summary>
        /// Выход
        /// </summary>
        public async Task LogoutAsync()
        {
            await SendGetTextAsync(@"account/logout");
            IsAuthorized = false;
            FomsCodeMO = null;
        }

        /// <summary>
        /// Получает информацию о прохождении пациентом профилактического осмотра из плана.
        /// </summary>
        /// <param name="srzPatientId">Id пациента в СРЗ</param>
        /// <param name="insuranceNumber">Серия и/или номер полиса ОМС</param>
        /// <param name="examinationType">Вид осмотра</param>
        /// <param name="year">Год осмотра</param>
        /// <returns>Экземпляр WebPatientData если пациент найден в плане, иначе null.</returns>
        /// <exception cref="WebServiceOperationException">Возникает если веб-сервер вернул пустой ответ.</exception>
        protected async Task<WebPatientData> GetPatientDataFromPlanAsync(int? srzPatientId, string insuranceNumber, ExaminationKind examinationType, int year)
        {
            ThrowExceptionIfNotAuthorized();

            var uriParameters = new Dictionary<string, string> {
                {"Filter.Year", GetYearId(year).ToString() },
                {"Filter.PolisNum", insuranceNumber??string.Empty },
                {"Filter.PersonId", srzPatientId?.ToString()??string.Empty },
                {"Filter.DispType", ((int)examinationType).ToString() },
            };

            var uriParamentersString = new FormUrlEncodedContent(uriParameters).ReadAsStringAsync().Result;
            var urn = $@"disp/GetDispData?{uriParamentersString}";

            var contentParameters = new Dictionary<string, string> {
                {"columns[0][data]", "PersonId"},
                {"order[0][column]", "0"},
                {"length", "25"},
            };

            var responseText = await SendPostAsync(urn, contentParameters);

            var planResponse = JsonConvert.DeserializeObject<PlanResponse>(responseText);

            if (planResponse == null)
                throw new WebServiceOperationException("Ошибка получения информации из плана.");

            return planResponse.Data?.FirstOrDefault();
        }

        /// <summary>
        /// Получает информацию о прохождении пациентом профилактического осмотра из плана.
        /// </summary>
        /// <param name="srzPatientId">Id пациента в СРЗ</param>
        /// <param name="examinationType">Вид осмотра</param>
        /// <param name="year">Год осмотра</param>
        /// <returns>Экземпляр WebPatientData если пациент найден в плане, иначе null.</returns>
        protected async Task<WebPatientData> GetPatientDataFromPlanAsync(int srzPatientId, ExaminationKind examinationType, int year)
            => await GetPatientDataFromPlanAsync(srzPatientId, null, examinationType, year);

        /// <summary>
        /// Получает информацию о прохождении пациентом профилактического осмотра из плана.
        /// </summary>
        /// <param name="insuranceNumber">Серия и/или номер полиса ОМС</param>
        /// <param name="examinationType">Вид осмотра</param>
        /// <param name="year">Год осмотра</param>
        /// <returns>Экземпляр WebPatientData если пациент найден в плане, иначе null.</returns>
        protected async Task<WebPatientData> GetPatientDataFromPlanAsync(string insuranceNumber, ExaminationKind examinationType, int year)
            => await GetPatientDataFromPlanAsync(null, insuranceNumber, examinationType, year);

        /// <summary>
        /// Добавляет пациента в план.
        /// </summary>
        /// <param name="srzPatientId">Id пациента в СРЗ</param>
        /// <param name="examinationType">Вид осмотра</param>
        /// <param name="year">Год осмотра</param>
        /// <exception cref="WebServiceOperationException">Возникает если не удалось добавить пациента в план.</exception>
        protected async Task AddPatientToPlanAsync(int srzPatientId, ExaminationKind examinationType, int year)
        {
            ThrowExceptionIfNotAuthorized();

            var contentParameters = new Dictionary<string, string>
            {
                { "personId", srzPatientId.ToString() },
                { "yearId", GetYearId(year).ToString() },
                { "dispType", ((int)examinationType).ToString() },
            };

            var responseText = await SendPostAsync(@"disp/AddToDisp", contentParameters);

            var response = JsonConvert.DeserializeObject<WebResponse>(responseText);

            if (response.IsError)
                throw new WebServiceOperationException("Ошибка при добавлении пациента в план.");
        }

        /// <summary>
        /// Удаляет пациент из плана.
        /// </summary>
        /// <param name="patientId">Id пациента</param>
        /// <exception cref="WebServiceOperationException">Возникает если не удалось добавить пациента в план.</exception>
        protected async Task DeletePatientFromPlanAsync(int patientId)
        {
            ThrowExceptionIfNotAuthorized();

            var contentParameters = new Dictionary<string, string>
            {
                { "Id", patientId.ToString() }
            };

            var responseText = await SendPostAsync(@"disp/removeDisp", contentParameters);

            var response = JsonConvert.DeserializeObject<WebResponse>(responseText);

            if (response.IsError)
                throw new WebServiceOperationException("Ошибка при удалении пациента из плана.");
        }

        /// <summary>
        /// Получает Id пациента в СРЗ. Работает медленнее чем поиск по ФИО и ДР.
        /// </summary>
        /// <param name="insuranceNumber">Серия и/или номер полиса ОМС.</param>
        /// <param name="examinationYear">Год осмотра.</param>
        /// <returns>Экземпляр SrzInfo если пациент найден, иначе null.</returns>
        /// <exception cref="WebServiceOperationException">Возникает если пациент находится в плане другой ЛПУ.</exception>
        protected async Task<SrzInfo> GetInfoFromSRZAsync(string insuranceNumber, int examinationYear)
        {
            var contentParameters = new Dictionary<string, string>
            {
                {"SearchData.DispYearId", GetYearId(examinationYear).ToString() },
                {"SearchData.Surname", string.Empty },
                {"SearchData.Firstname", string.Empty },
                {"SearchData.Secname", string.Empty },
                {"SearchData.Birthday", string.Empty },
                {"SearchData.PolisNum", insuranceNumber },
                {"SearchData.SelectSearchValues", "polis"}
            }; ;
            return await GetInfoFromSRZAsyncInternalAsync(contentParameters);
        }

        /// <summary>
        /// Получает Id пациента в СРЗ. Работает быстрее чем поиск по полису.
        /// </summary>
        /// <param name="insuranceNumber">Серия и/или номер полиса ОМС.</param>
        /// <param name="examinationYear">Год осмотра.</param>
        /// <returns>Экземпляр SrzInfo если пациент найден, иначе null.</returns>
        /// <exception cref="WebServiceOperationException">Возникает если пациент находится в плане другой ЛПУ.</exception>
        protected async Task<SrzInfo> GetInfoFromSRZAsync(IPatient patient, int examinationYear)
        {
            var contentParameters = new Dictionary<string, string>
            {
                { "SearchData.DispYearId", GetYearId(examinationYear).ToString() },
                { "SearchData.Surname", patient.Surname },
                { "SearchData.Firstname", patient.Name },
                { "SearchData.Secname", patient.Patronymic ?? string.Empty },
                { "SearchData.Birthday", patient.Birthdate.ToShortDateString() },
                { "SearchData.PolisNum", string.Empty },
                { "SearchData.SelectSearchValues", "fio" }
            };
            return await GetInfoFromSRZAsyncInternalAsync(contentParameters);
        }

        async Task<SrzInfo> GetInfoFromSRZAsyncInternalAsync(Dictionary<string, string> contentParameters)
        {
            ThrowExceptionIfNotAuthorized();

            var responseText = await SendPostAsync(@"disp/SrzSearch", contentParameters);

            var idString = responseText.SubstringBetween("personId", "\"", "\"");

            if (!int.TryParse(idString, out int srzPatientId))
                return null;

            return new SrzInfo
            {
                SrzPatientId = srzPatientId,
                FilledByAnotherClinic = responseText.Contains("Начата диспансеризация другой МО"),
                ExistInPlan = responseText.Contains("Застрахованное лицо уже находится в плане диспансеризации вашей МО")
            };
        }

        /// <summary>
        /// Получает список доступных шагов.
        /// </summary>
        /// <param name="patientId">Id пациента</param>
        /// <returns>Список доступных шагов</returns>
        protected async Task<List<AvailableStage>> GetAvailableStepsAsync(int patientId)
        {
            var contentParameters = new Dictionary<string, string>
            {
                { "dispId", patientId.ToString() },
            };

            var responseText = await SendPostAsync(@"/disp/getAvailableStages", contentParameters);

            var availableStagesResponse = JsonConvert.DeserializeObject<AvailableStagesResponse>(responseText);

            return availableStagesResponse?.AvailableStages ?? new List<AvailableStage>();
        }

        /// <summary>
        /// Добавляет шаг-осмотра.
        /// </summary>
        /// <param name="patientId">Id пациента</param>
        /// <param name="step">Шаг осмотра</param>
        /// <param name="date">Дата осмотра</param>
        /// <param name="healthGroup">Группа здоровья, опционально</param>
        /// <param name="referral">Направление, опционально</param>
        /// <exception cref="WebServiceOperationException">Возникает если не возможно добавить шаг осмотра.</exception>
        protected async Task AddStepAsync(int patientId, StepKind step, DateTime date, HealthGroup healthGroup = 0, Referral referral = 0)
        {
            ThrowExceptionIfNotAuthorized();

            var contentParameters = new Dictionary<string, string>
            {
                { "stageId", ((int)step).ToString() },
                { "stageDate", date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) },
                { "resultId", ((int)healthGroup).ToString()  },
                { "destId", referral==0 ? string.Empty : ((int)referral).ToString() },
                { "dispId", patientId.ToString() },
            };

            var responseText = await SendPostAsync(@"disp/editDispStage", contentParameters);

            var response = JsonConvert.DeserializeObject<WebResponse>(responseText);

            if (response.IsError)
                throw new WebServiceOperationException("Ошибка добавления шага выполнения осмотра.");
        }

        /// <summary>
        /// Удаляет последний шаг осмотра.
        /// </summary>
        /// <param name="patientId">Id пациента</param>
        /// <returns>Текущий шаг.</returns>
        /// <exception cref="WebServiceOperationException">Возникает если не возможно удалить шаг осмотра.</exception>
        protected async Task<StepKind> DeleteLastStepAsync(int patientId)
        {
            ThrowExceptionIfNotAuthorized();

            var contentParameters = new Dictionary<string, string> {
                { "dispId", patientId.ToString() },
            };

            var responseText = await SendPostAsync(@"disp/deleteDispStage", contentParameters);

            var response = JsonConvert.DeserializeObject<DeleteLastStepResponse>(responseText);

            if (response == null || response.IsError)
                throw new WebServiceOperationException("Ошибка удаления шага осмотра.");

            return response.Data?.LastOrDefault()?.DispStage?.DispStageId ?? 0;
        }

        static int GetYearId(int year)
           => year - 2017;
    }
}
