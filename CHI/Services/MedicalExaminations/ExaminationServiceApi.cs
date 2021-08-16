using CHI.Services.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;

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


        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="URL">URL</param>
        /// <param name="useProxy">Использовать прокси-сервер</param>
        /// <param name="proxyAddress">Адрес прокси-сервера</param>
        /// <param name="proxyPort">Порт прокси-сервера</param>
        protected ExaminationServiceApi(string URL, bool useProxy, string proxyAddress = null, int? proxyPort = null)
            : base(URL, useProxy, proxyAddress, proxyPort)
        { }


        /// <summary>
        /// Авторизация на сайте. При успешной авторизации инициализирует FomsCodeMO.
        /// </summary>
        /// <param name="credential">Учетные данные.</param>
        /// <returns>true-в случае успешной авторизации, false-иначе.</returns>
        public bool Authorize(ICredential credential)
        {
            var requestValues = new Dictionary<string, string> {
                { "Login",      credential.Login    },
                { "Password",   credential.Password }
            };

            var responseText = SendRequest(HttpMethod.Post, @"account/login", requestValues);

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
        public void Logout()
        {
            SendRequest(HttpMethod.Get, @"account/logout", null);
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
        /// <exception cref="InvalidOperationException">Возникает если веб-сервер вернул пустой ответ.</exception>
        protected WebPatientData GetPatientDataFromPlan(int? srzPatientId, string insuranceNumber, ExaminationKind examinationType, int year)
        {
            CheckAuthorization();

            if (srzPatientId != null)
                insuranceNumber = null;

            var uriParameters = new Dictionary<string, string> {
                {"Filter.Year", ConvertToYearId(year).ToString() },
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

            var responseText = SendRequest(HttpMethod.Post, urn, contentParameters);

            var planResponse = JsonConvert.DeserializeObject<PlanResponse>(responseText);

            if (planResponse != null)
                return planResponse.Data?.FirstOrDefault();
            else
                throw new InvalidOperationException("Ошибка получения информации из плана");
        }
        /// <summary>
        /// Добавляет пациента в план.
        /// </summary>
        /// <param name="srzPatientId">Id пациента в СРЗ</param>
        /// <param name="examinationType">Вид осмотра</param>
        /// <param name="year">Год осмотра</param>
        /// <exception cref="WebServiceOperationException">Возникает если не удалось добавить пациента в план.</exception>
        protected void AddPatientToPlan(int srzPatientId, ExaminationKind examinationType, int year)
        {
            CheckAuthorization();

            var contentParameters = new Dictionary<string, string>
            {
                { "personId", srzPatientId.ToString() },
                { "yearId", ConvertToYearId(year).ToString() },
                { "dispType", ((int)examinationType).ToString() },
            };

            var responseText = SendRequest(HttpMethod.Post, @"disp/AddToDisp", contentParameters);

            var response = JsonConvert.DeserializeObject<WebResponse>(responseText);

            if (response.IsError)
                throw new WebServiceOperationException("Ошибка при добавлении пациента в план");
        }
        /// <summary>
        /// Удаляет пациент из плана.
        /// </summary>
        /// <param name="patientId">Id пациента</param>
        /// <exception cref="WebServiceOperationException">Возникает если не удалось добавить пациента в план.</exception>
        protected void DeletePatientFromPlan(int patientId)
        {
            CheckAuthorization();

            var contentParameters = new Dictionary<string, string>
            {
                { "Id", patientId.ToString() }
            };

            var responseText = SendRequest(HttpMethod.Post, @"disp/removeDisp", contentParameters);

            var response = JsonConvert.DeserializeObject<WebResponse>(responseText);

            if (response.IsError)
                throw new WebServiceOperationException("Ошибка при удалении пациента из плана");
        }
        /// <summary>
        /// Получает Id пациента в СРЗ. Осуществляет поиск по полису или данным пациента. Если заданы оба - полис приоритетнее.
        /// </summary>
        /// <param name="insuranceNumber">Серия и/или номер полиса ОМС.</param>
        /// <param name="patient">Информация о пациенте.</param>
        /// <param name="year">Год осмотра.</param>
        /// <returns>Id пациента в СРЗ если пациент найден, null-иначе.</returns>
        /// <exception cref="WebServiceOperationException">Возникает если пациент находится в плане другой ЛПУ.</exception>
        protected int? GetPatientIdFromSRZ(string insuranceNumber, IPatient patient, int year)
        {
            CheckAuthorization();

            var selector = string.IsNullOrEmpty(insuranceNumber) ? "fio" : "polis";

            var contentParameters = new Dictionary<string, string>
            {
                {"SearchData.DispYearId", ConvertToYearId(year).ToString() },
                {"SearchData.Surname", patient?.Surname??string.Empty },
                {"SearchData.Firstname", patient?.Name??string.Empty },
                {"SearchData.Secname", patient?.Patronymic??string.Empty },
                {"SearchData.Birthday", patient?.Birthdate.ToShortDateString() ?? string.Empty },
                {"SearchData.PolisNum", insuranceNumber??string.Empty },
                {"SearchData.SelectSearchValues", selector}
            };

            var responseText = SendRequest(HttpMethod.Post, @"disp/SrzSearch", contentParameters);

            var idString = SubstringBetween(responseText, "personId", "\"", "\"");

            //if (responseText.IndexOf(">Застрахованное лицо находится в плане диспансеризации другой МО<") != -1)
            //    throw new WebServiceOperationException("Пациент находится в плане др. ЛПУ");

            if (responseText.IndexOf(">Начата диспансеризация другой МО<") != -1)
                throw new WebServiceOperationException("Ошибка добавления в план: При поиске в СРЗ установлено - периодический осмотр был выполнен в др. ЛПУ.");

            int.TryParse(idString, out var srzPatientId);

            return srzPatientId == 0 ? (int?)null : srzPatientId;
        }
        /// <summary>
        /// Получает список доступных шагов.
        /// </summary>
        /// <param name="patientId">Id пациента</param>
        /// <returns>Список доступных шагов</returns>
        protected List<AvailableStage> GetAvailableSteps(int patientId)
        {
            var contentParameters = new Dictionary<string, string>
            {
                { "dispId", patientId.ToString() },
            };

            var responseText = SendRequest(HttpMethod.Post, @"/disp/getAvailableStages", contentParameters);

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
        protected void AddStep(int patientId, StepKind step, DateTime date, HealthGroup healthGroup = 0, Referral referral = 0)
        {
            CheckAuthorization();

            var contentParameters = new Dictionary<string, string>
            {
                { "stageId", ((int)step).ToString() },
                { "stageDate", date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) },
                { "resultId", ((int)healthGroup).ToString()  },
                { "destId", referral==0 ? string.Empty : ((int)referral).ToString() },
                { "dispId", patientId.ToString() },
            };

            var responseText = SendRequest(HttpMethod.Post, @"disp/editDispStage", contentParameters);

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
        protected StepKind DeleteLastStep(int patientId)
        {
            CheckAuthorization();

            var contentParameters = new Dictionary<string, string> {
                { "dispId", patientId.ToString() },
            };

            var responseText = SendRequest(HttpMethod.Post, @"disp/deleteDispStage", contentParameters);

            var response = JsonConvert.DeserializeObject<DeleteLastStepResponse>(responseText);

            if (response == null || response.IsError)
                throw new WebServiceOperationException("Ошибка удаления шага осмотра.");

            return response.Data?.LastOrDefault()?.DispStage?.DispStageId ?? 0;
        }
        /// <summary>
        /// Преобразует год в Id года
        /// </summary>
        /// <param name="year">Год</param>
        /// <returns>Id года</returns>
        protected static int ConvertToYearId(int year)
        {
            return (year - 2017);
        }
        /// <summary>
        /// Преобразует Id года в год
        /// </summary>
        /// <param name="yearId">Id года</param>
        /// <returns>Год</returns>
        protected static int ConvertToYear(int yearId)
        {
            return (yearId + 2017);
        }
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

        #region Классы для десериализации

        #endregion
    }
}
