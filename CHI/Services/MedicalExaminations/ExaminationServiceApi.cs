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
                FomsCodeMO = SubstringBetween(responseText, "<div>", "<div>", "</div>");
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

            if (srzPatientId != null)
                insuranceNumber = null;

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
        /// Получает Id пациента в СРЗ. Осуществляет поиск по полису или данным пациента. Если заданы оба - полис приоритетнее.
        /// </summary>
        /// <param name="insuranceNumber">Серия и/или номер полиса ОМС.</param>
        /// <param name="patient">Информация о пациенте.</param>
        /// <param name="year">Год осмотра.</param>
        /// <returns>Id пациента в СРЗ если пациент найден, null-иначе.</returns>
        /// <exception cref="WebServiceOperationException">Возникает если пациент находится в плане другой ЛПУ.</exception>
        protected async Task<int?> GetPatientIdFromSRZAsync(string insuranceNumber, IPatient patient, int year)
        {
            ThrowExceptionIfNotAuthorized();

            var selector = string.IsNullOrEmpty(insuranceNumber) ? "fio" : "polis";

            var contentParameters = new Dictionary<string, string>
            {
                {"SearchData.DispYearId", GetYearId(year).ToString() },
                {"SearchData.Surname", patient?.Surname??string.Empty },
                {"SearchData.Firstname", patient?.Name??string.Empty },
                {"SearchData.Secname", patient?.Patronymic??string.Empty },
                {"SearchData.Birthday", patient?.Birthdate.ToShortDateString() ?? string.Empty },
                {"SearchData.PolisNum", insuranceNumber??string.Empty },
                {"SearchData.SelectSearchValues", selector}
            };

            var responseText = await SendPostAsync(@"disp/SrzSearch", contentParameters);

            var idString = SubstringBetween(responseText, "personId", "\"", "\"");

            if (responseText.IndexOf(">Начата диспансеризация другой МО<") != -1)
                throw new WebServiceOperationException("Ошибка добавления в план: При поиске в СРЗ установлено - осмотр уже подан др. ЛПУ.");

            int.TryParse(idString, out var srzPatientId);

            return srzPatientId == 0 ? null : srzPatientId;
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

        /// <summary>
        /// Возвращает подстроку между левой leftStr и правой rightStr строками, поиск начинается от начальной позиции offsetStr.
        /// Если одна из строк не найдена-возвращает пустую строку.
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="offsetStr">Начальная позиция поиска</param>
        /// <param name="leftStr">Левая подстрока</param>
        /// <param name="rightStr">Правая подстрока</param>
        /// <returns>Искомая или пустая строка</returns>
        protected static string SubstringBetween(string text, string offsetStr, string leftStr, string rightStr)
        {
            int offset;

            if (string.IsNullOrEmpty(offsetStr))
                offset = 0;
            else
            {
                offset = text.IndexOf(offsetStr);

                if (offset == -1)
                    return string.Empty;

                offset += offsetStr.Length;
            }

            var begin = text.IndexOf(leftStr, offset);

            if (begin == -1)
                return string.Empty;

            begin += leftStr.Length;

            var end = text.IndexOf(rightStr, begin);

            if (end == -1)
                return string.Empty;

            var length = end - begin;

            if (length > 0)
                return text.Substring(begin, length);

            return string.Empty;
        }
    }
}
