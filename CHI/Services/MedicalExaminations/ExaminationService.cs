using CHI.Models;
using CHI.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services.MedicalExaminations
{
    /// <summary>
    /// Представляет операции с web-порталом диспансеризации
    /// </summary>
    public class ExaminationService : ExaminationServiceApi
    {
        static readonly StepKind[] examinationSteps;


        /// <summary>
        /// Статический конструктор по-умолчанию
        /// </summary>
        static ExaminationService()
        {
            examinationSteps = Enum.GetValues(typeof(StepKind))
                .Cast<StepKind>()
                .Where(x => x != StepKind.Refuse && x != StepKind.None)
                .OrderBy(x => (int)x)
                .ToArray();
        }

        /// <summary>
        /// Конструктор
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
        public void AddPatientExaminations(PatientExaminations patientExaminations)
        {
            if ((patientExaminations.Stage1?.BeginDate != null && patientExaminations.Stage1.BeginDate > DateTime.Today)
                || (patientExaminations.Stage1?.EndDate != null && patientExaminations.Stage1.EndDate > DateTime.Today)
                || (patientExaminations.Stage2?.BeginDate != null && patientExaminations.Stage2.BeginDate > DateTime.Today)
                || (patientExaminations.Stage2?.EndDate != null && patientExaminations.Stage2.EndDate > DateTime.Today))
                throw new InvalidOperationException("Осмотры будущей датой не могут быть загружены.");

            var webPatientData = GetOrAddPatientToPlan(patientExaminations);

            if (webPatientData == null)
                throw new InvalidOperationException("Ошибка добавления пациента в план");

            var transfer2StageDate = patientExaminations.Stage1?.EndDate ?? webPatientData.Disp1Date;
            var userSteps = ConvertToExaminationsSteps(patientExaminations, transfer2StageDate);
            var webSteps = ConvertToExaminationsSteps(webPatientData);

            AddExaminationSteps(webPatientData.Id, userSteps, webSteps);
        }

        /// <summary>
        /// Добавление пациента в нужный план.
        /// При необходимости удаление пациента из др. планов и информации о прохождении профилактических осмотров.
        /// </summary>
        /// <param name="patientExaminations">Экземпляр PatientExaminations</param>
        /// <param name="srzPatientId">Id пациента в СРЗ</param>
        /// <returns>Возвращает информацию о пациенте </returns>
        WebPatientData GetOrAddPatientToPlan(PatientExaminations patientExaminations, int? srzPatientId = null)
        {
            var webPatientData = GetPatientDataFromPlan(srzPatientId, patientExaminations.InsuranceNumber, patientExaminations.Kind, patientExaminations.Year);

            //пациент найден в нужном плане
            if (webPatientData == null)
            {
                var otherExaminationKinds = Enum.GetValues(typeof(ExaminationKind))
                    .Cast<ExaminationKind>()
                    .Where(x => x != ExaminationKind.None && x != patientExaminations.Kind)
                    .ToList();

                //ищем в др. планах
                foreach (var examinationType in otherExaminationKinds)
                {
                    webPatientData = GetPatientDataFromPlan(srzPatientId, patientExaminations.InsuranceNumber, examinationType, patientExaminations.Year);
                    //пациент найден в другом план
                    if (webPatientData != null)
                    {
                        //заполнены шаги осмотра
                        if (webPatientData.Disp1BeginDate != default || webPatientData.Disp2BeginDate != default || webPatientData.DispCancelDate != default)
                            DeleteAllSteps(webPatientData.Id);

                        DeletePatientFromPlan(webPatientData.Id);

                        break;
                    }
                }

                if (srzPatientId == null)
                    srzPatientId = webPatientData?.PersonId ?? GetPatientIdFromSRZ(patientExaminations.InsuranceNumber, null, patientExaminations.Year);

                //если пациент не найден по полису - возможно неправильный полис, ищем по ФИО и ДР
                if (srzPatientId == null)
                {
                    srzPatientId = GetPatientIdFromSRZ(null, patientExaminations, patientExaminations.Year);

                    return srzPatientId == null ? null : GetOrAddPatientToPlan(patientExaminations, srzPatientId);
                }

                AddPatientToPlan(srzPatientId.Value, patientExaminations.Kind, patientExaminations.Year);

                webPatientData = GetPatientDataFromPlan(srzPatientId, null, patientExaminations.Kind, patientExaminations.Year);
            }

            return webPatientData;
        }

        /// <summary>
        /// Сравнение каждого шага, пропуск, добавление или замена информации о прохождении шагов профилактических осмотров. 
        /// В случае возникновения ошибки на стороне сервера - возврат изменений назад.
        /// </summary>
        /// <param name="patientId">Id пациента</param>
        /// <param name="userSteps">Список добавляемых шагов</param>
        /// <param name="webSteps">Список шагов уже содержащихся в веб-портале</param>
        void AddExaminationSteps(int patientId, List<ExaminationStep> userSteps, List<ExaminationStep> webSteps)
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
                                realWebStep = AddStep(patientId, userStep);
                            else
                            {
                                while (realWebStep >= userStep.StepKind)
                                    realWebStep = DeleteLastStep(patientId);

                                realWebStep = AddStep(patientId, userStep);
                            }
                        }
                        else if (realWebStep < userStep.StepKind)
                            realWebStep = AddStep(patientId, userStep);
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
                            realWebStep = AddStep(patientId, webStep);
                    }
                }

            }
            catch (WebServiceOperationException)
            {
                DeleteAllSteps(patientId);

                foreach (var webStep in webSteps)
                    AddStep(patientId, webStep);

                throw;
            }
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

        /// <summary>
        /// Добавление шага профилактического осмотра на веб-портал
        /// </summary>
        /// <param name="patientId">Id пациента</param>
        /// <param name="examinationStep">Информация о шаге профилактического осмотра</param>
        /// <returns>Текущий шаг прохождения профилактического осмотра</returns>
        StepKind AddStep(int patientId, ExaminationStep examinationStep)
        {
            AddStep(patientId, examinationStep.StepKind, examinationStep.Date, examinationStep.HealthGroup, examinationStep.Referral);

            return examinationStep.StepKind;
        }

        /// <summary>
        /// Удаление всех шагов
        /// </summary>
        /// <param name="patientId">Id пациента</param>
        void DeleteAllSteps(int patientId)
        {
            while (DeleteLastStep(patientId) != 0) ;
        }
    }
}