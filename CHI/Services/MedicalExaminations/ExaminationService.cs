using CHI.Models;
using CHI.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет операции с web-порталом диспансеризации
    /// </summary>
    public class ExaminationService : ExaminationServiceApi
    {
        static readonly StepKind[] examinationSteps;
        static readonly ExaminationKind[] examinationKinds;


        static ExaminationService()
        {
            examinationSteps = Enum.GetValues(typeof(StepKind))
                .Cast<StepKind>()
                .Where(x => x != StepKind.Refuse && x != StepKind.None)
                .OrderBy(x => (int)x)
                .ToArray();

            examinationKinds = Enum.GetValues(typeof(ExaminationKind))
                .Cast<ExaminationKind>()
                .Where(x => x != ExaminationKind.None)
                .ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="URL">URL</param>
        /// <param name="useProxy">Использовать прокси-сервер</param>
        /// <param name="proxyAddress">Адрес прокси-сервера</param>
        /// <param name="proxyPort">Порт прокси-сервера</param>
        public ExaminationService(string URL, bool useProxy, string proxyAddress = null, int? proxyPort = null)
            : base(URL, useProxy, proxyAddress, proxyPort)
        { }


        /// <summary>
        /// Добавление пациента в нужный план, добавление информации о прохождении этапов профилактических осмотров.
        /// При необходимости удаление пациента из др. планов и информации о прохождении профилактических осмотров.
        /// </summary>
        /// <param name="patientExaminations">Экземпляр PatientExaminations</param>
        /// <exception cref="InvalidOperationException">
        /// Вызвыет исключение при невозможности добавить пациента в нужный план.
        /// </exception>
        public async Task AddPatientExaminationsAsync(PatientExaminations patientExaminations)
        {
            if (AnyFutureDateFound(patientExaminations))
                throw new InvalidOperationException("Осмотр будущей датой не может быть загружен.");

            var webPatientData = await ChangeOrAddPatientToPlanAsync(patientExaminations);

            var transfer2StageDate = patientExaminations.Stage1?.EndDate ?? webPatientData.Disp1Date;
            var userSteps = ConvertToExaminationsSteps(patientExaminations, transfer2StageDate);
            var webSteps = ConvertToExaminationsSteps(webPatientData);

            await AddExaminationStepsAsync(webPatientData.Id, userSteps, webSteps);
        }

        static bool AnyFutureDateFound(PatientExaminations patientExaminations)
            => ((patientExaminations.Stage1?.BeginDate ?? default) > DateTime.Today)
                || ((patientExaminations.Stage1?.EndDate ?? default) > DateTime.Today)
                || ((patientExaminations.Stage2?.BeginDate ?? default) > DateTime.Today)
                || ((patientExaminations.Stage2?.EndDate ?? default) > DateTime.Today);


        /// <summary>
        /// Добавление пациента в нужный план.
        /// При необходимости удаление пациента из др. планов и информации о прохождении профилактических осмотров.
        /// </summary>
        /// <param name="patientExaminations">Экземпляр PatientExaminations</param>
        /// <param name="srzPatientId">Id пациента в СРЗ</param>
        /// <returns>Возвращает информацию о пациенте </returns>
        async Task<WebPatientData> ChangeOrAddPatientToPlanAsync(PatientExaminations patientExaminations)
        {
            var webPatientData = await GetPatientDataFromPlanAsync(patientExaminations.InsuranceNumber, patientExaminations.Kind, patientExaminations.Year);

            if (webPatientData != null)
                return webPatientData;

            var otherExaminationKinds = examinationKinds.Where(x => x != patientExaminations.Kind).ToList();

            //если пациент находится в не правильном плане, его нужно удалить из этого плана
            foreach (var otherEexaminationKind in otherExaminationKinds)
            {
                webPatientData = await GetPatientDataFromPlanAsync(patientExaminations.InsuranceNumber, otherEexaminationKind, patientExaminations.Year);
                //пациент найден в другом плане
                if (webPatientData != null)
                {
                    await DeepDeleteFromPlanAsync(webPatientData);
                    break;
                }
            }

            var srzPatientId = webPatientData?.PersonId
                ?? await GetPatientIdFromSRZAsync(patientExaminations.InsuranceNumber, patientExaminations.Year)
                ?? await GetPatientIdFromSRZAsync(patientExaminations, patientExaminations.Year)
                ?? throw new InvalidOperationException("Не удалось найти пациента в СРЗ");

            if (await TryAddPatientToPlanAsync(srzPatientId, patientExaminations.Kind, patientExaminations.Year))
                return await GetPatientDataFromPlanAsync(srzPatientId, patientExaminations.Kind, patientExaminations.Year);

            //если дошли сюда, значит пациент состоит в плане но поиск по полису не дал результат, пробуем искать по SrzPatientId
            webPatientData = await GetPatientDataFromPlanAsync(srzPatientId, patientExaminations.Kind, patientExaminations.Year);

            if (webPatientData != null)
                return webPatientData;

            //если пациент не найден в нужном плане - ищем в др. планах
            foreach (var otherEexaminationKind in otherExaminationKinds)
            {
                webPatientData = await GetPatientDataFromPlanAsync(srzPatientId, otherEexaminationKind, patientExaminations.Year);
                //пациент найден в другом плане
                if (webPatientData != null)
                {
                    await DeepDeleteFromPlanAsync(webPatientData);
                    break;
                }
            }

            if (webPatientData == null)
                throw new InvalidOperationException("Ошибка добавления пациента в план");

            await AddPatientToPlanAsync(srzPatientId, patientExaminations.Kind, patientExaminations.Year);

            return await GetPatientDataFromPlanAsync(srzPatientId, patientExaminations.Kind, patientExaminations.Year);
        }

        async Task DeepDeleteFromPlanAsync(WebPatientData webPatientData)
        {
            //заполнены шаги осмотра
            if (webPatientData.Disp1BeginDate != default || webPatientData.DispCancelDate != default)
                await DeleteAllStepsAsync(webPatientData.Id);

            await DeletePatientFromPlanAsync(webPatientData.Id);
        }

        /// <summary>
        /// Сравнение каждого шага, пропуск, добавление или замена информации о прохождении шагов профилактических осмотров. 
        /// В случае возникновения ошибки на стороне сервера - возврат изменений назад.
        /// </summary>
        /// <param name="patientId">Id пациента</param>
        /// <param name="userSteps">Список добавляемых шагов</param>
        /// <param name="webSteps">Список шагов уже содержащихся в веб-портале</param>
        async Task AddExaminationStepsAsync(int patientId, List<ExaminationStep> userSteps, List<ExaminationStep> webSteps)
        {
            try
            {
                var realWebStep = webSteps.LastOrDefault()?.StepKind ?? StepKind.None;

                for (int i = 0; i < examinationSteps.Length; i++)
                {
                    var step = examinationSteps[i];
                    var userStep = userSteps.Where(x => x.StepKind == step).FirstOrDefault();
                    var webStep = webSteps.Where(x => x.StepKind == step).FirstOrDefault();

                    if (userStep != null)
                    {
                        if (!userStep.Equals(userStep, webStep))
                        {
                            if (webStep == null && realWebStep < step)
                                realWebStep = await AddStepAsync(patientId, userStep);
                            else
                            {
                                while (realWebStep >= userStep.StepKind)
                                    realWebStep = await DeleteLastStepAsync(patientId);

                                realWebStep = await AddStepAsync(patientId, userStep);
                            }
                        }
                        else if (realWebStep < userStep.StepKind)
                            realWebStep = await AddStepAsync(patientId, userStep);
                    }
                    else
                    {
                        if (webStep == null)
                        {
                            if (step == StepKind.FirstResult)
                                continue;
                            else
                                break;
                        }
                        else if (realWebStep < webStep.StepKind)
                            realWebStep = await AddStepAsync(patientId, webStep);
                    }
                }

            }
            catch (WebServiceOperationException)
            {
                await DeleteAllStepsAsync(patientId);

                foreach (var webStep in webSteps)
                    await AddStepAsync(patientId, webStep);

                throw;
            }
        }

        /// <summary>
        /// Добавление шага профилактического осмотра на веб-портал
        /// </summary>
        /// <param name="patientId">Id пациента</param>
        /// <param name="examinationStep">Информация о шаге профилактического осмотра</param>
        /// <returns>Текущий шаг прохождения профилактического осмотра</returns>
        async Task<StepKind> AddStepAsync(int patientId, ExaminationStep examinationStep)
        {
            await AddStepAsync(patientId, examinationStep.StepKind, examinationStep.Date, examinationStep.HealthGroup, examinationStep.Referral);

            return examinationStep.StepKind;
        }

        /// <summary>
        /// Удаление всех шагов
        /// </summary>
        /// <param name="patientId">Id пациента</param>
        async Task DeleteAllStepsAsync(int patientId)
        {
            while (await DeleteLastStepAsync(patientId) != 0) ;
        }

        /// <summary>
        /// Конвертация PatientExaminations в список ExaminationStep
        /// </summary>
        /// <param name="patientExaminations">Экземпляр PatientExaminations</param>
        /// <param name="transfer2StageDate">Дата перевода на 2ой этап. 
        /// Необходимо задать, если информация о прохождении пациентом профилактических осмотров содержит только 2ой этап.
        /// </param>
        /// <returns>Список ExaminationStep</returns>
        static List<ExaminationStep> ConvertToExaminationsSteps(PatientExaminations patientExaminations, DateTime? transfer2StageDate = null)
        {
            var examinationSteps = new List<ExaminationStep>();

            var stage1 = patientExaminations.Stage1;
            var stage2 = patientExaminations.Stage2;

            if (stage1 != default)
            {
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.FirstBegin,
                    Date = stage1.BeginDate
                });
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.FirstEnd,
                    Date = stage1.EndDate
                });
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.FirstResult,
                    Date = stage1.EndDate,
                    HealthGroup = stage1.HealthGroup,
                    Referral = stage1.Referral
                });
            }


            if (stage2 != default)
            {
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.TransferSecond,
                    Date = stage1?.EndDate ?? transfer2StageDate.Value
                });

                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.SecondBegin,
                    Date = stage2.BeginDate
                });
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.SecondEnd,
                    Date = stage2.EndDate
                });
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.SecondResult,
                    Date = stage2.EndDate,
                    HealthGroup = stage2.HealthGroup,
                    Referral = stage2.Referral
                });
            }

            return examinationSteps;
        }

        /// <summary>
        /// Конвертация WebPatientData в список ExaminationStep
        /// </summary>
        /// <param name="webPatientData">Экземпляр WebPatientData</param>
        /// <returns>Список ExaminationStep</returns>
        static List<ExaminationStep> ConvertToExaminationsSteps(WebPatientData webPatientData)
        {
            var examinationSteps = new List<ExaminationStep>();

            if (webPatientData.Disp1BeginDate != default)
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.FirstBegin,
                    Date = webPatientData.Disp1BeginDate.Value
                });

            if (webPatientData.Disp1Date != default)
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.FirstEnd,
                    Date = webPatientData.Disp1Date.Value
                });

            if (webPatientData.Disp1Date != default && webPatientData.Stage1ResultId != default)
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.FirstResult,
                    Date = webPatientData.Disp1Date.Value,
                    HealthGroup = webPatientData.Stage1ResultId.Value,
                    Referral = webPatientData.Stage1DestId ?? 0
                });

            if (webPatientData.Disp2DirectDate != default)
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.TransferSecond,
                    Date = webPatientData.Disp2DirectDate.Value
                });

            if (webPatientData.Disp2BeginDate != default)
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.SecondBegin,
                    Date = webPatientData.Disp2BeginDate.Value
                });

            if (webPatientData.Disp2Date != default)
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.SecondEnd,
                    Date = webPatientData.Disp2Date.Value
                });

            if (webPatientData.Disp2Date != default && webPatientData.Stage2ResultId != default && webPatientData.Stage2DestId != default)
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.SecondResult,
                    Date = webPatientData.Disp2Date.Value,
                    HealthGroup = webPatientData.Stage2ResultId.Value,
                    Referral = webPatientData.Stage2DestId.Value
                });

            if (webPatientData.DispCancelDate != default)
                examinationSteps.Add(new ExaminationStep
                {
                    StepKind = StepKind.Refuse,
                    Date = webPatientData.DispCancelDate.Value
                });

            return examinationSteps;
        }
    }
}