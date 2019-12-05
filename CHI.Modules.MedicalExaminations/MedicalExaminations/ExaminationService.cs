using CHI.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services.MedicalExaminations
{
    public class ExaminationService : ExaminationServiceApi
    {
        #region Поля
        private static readonly ExaminationStepKind[] examinationSteps;
        private static readonly string AddPlanErrorMessage = "Не удалось добавить пациента в план";
        #endregion

        #region Конструкторы
        static ExaminationService()
        {
            examinationSteps = Enum.GetValues(typeof(ExaminationStepKind))
                .Cast<ExaminationStepKind>()
                .Where(x => x != ExaminationStepKind.Refuse && x != ExaminationStepKind.None)
                .OrderBy(x => (int)x)
                .ToArray();
        }
        public ExaminationService(string URL)
            : this(URL, null, 0)
        { }
        public ExaminationService(string URL, string proxyAddress, int proxyPort) : base(URL, proxyAddress, proxyPort)
        { }
        #endregion

        #region Методы
        public bool TryAddPatientExaminations(PatientExaminations patientExaminations)
        {
            try
            {
                AddPatientExaminations(patientExaminations);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void AddPatientExaminations(PatientExaminations patientExaminations)
        {
            var webPatientData = GetOrAddPatientToPlan(patientExaminations);

            if (webPatientData == null)
                throw new InvalidOperationException(AddPlanErrorMessage);

            var transfer2StageDate = patientExaminations.Stage1?.EndDate ?? webPatientData.Disp1Date;
            var userSteps = ConvertToExaminationsSteps(patientExaminations, transfer2StageDate);
            var webSteps = ConvertToExaminationsSteps(webPatientData);

            AddPatientExaminations(webPatientData.Id, userSteps, webSteps);
        }
        private WebPatientData GetOrAddPatientToPlan(PatientExaminations patientExaminations, int? srzPatientId = null)
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
                    srzPatientId = webPatientData?.PersonId ?? GetPatientIdFromSRZ(patientExaminations.InsuranceNumber, patientExaminations.Year);

                //если пациент не найден по полису - возможно неправильный полис, ищем по ФИО и ДР
                if (srzPatientId == null)
                {
                    srzPatientId = GetPatientIdFromSRZ(patientExaminations.Surname, patientExaminations.Name, patientExaminations.Patronymic, patientExaminations.Birthdate, patientExaminations.Year);

                    return srzPatientId == null ? null : GetOrAddPatientToPlan(patientExaminations, srzPatientId);
                }

                AddPatientToPlan(srzPatientId.Value, patientExaminations.Kind, patientExaminations.Year);

                webPatientData = GetPatientDataFromPlan(srzPatientId, null, patientExaminations.Kind, patientExaminations.Year);
            }

            return webPatientData;
        }
        private void AddPatientExaminations(int patientId, List<ExaminationStep> userSteps, List<ExaminationStep> webSteps)
        {
            try
            {
                var realWebStep = webSteps.LastOrDefault()?.ExaminationStepKind ?? ExaminationStepKind.None;

                for (int i = 0; i < examinationSteps.Length; i++)
                {
                    var step = examinationSteps[i];
                    var userStep = userSteps.Where(x => x.ExaminationStepKind == step).FirstOrDefault();
                    var webStep = webSteps.Where(x => x.ExaminationStepKind == step).FirstOrDefault();

                    if (userStep != null)
                    {
                        if (!userStep.Equals(userStep, webStep))
                        {
                            if (webStep == null && realWebStep < step)
                                realWebStep = AddStep(patientId, userStep);
                            else
                            {
                                while (realWebStep >= userStep.ExaminationStepKind)
                                    realWebStep = DeleteLastStep(patientId);

                                realWebStep = AddStep(patientId, userStep);
                            }
                        }
                        else if (realWebStep < userStep.ExaminationStepKind)
                            realWebStep = AddStep(patientId, userStep);
                    }
                    else
                    {
                        if (webStep == null)
                        {
                            if (step == ExaminationStepKind.FirstResult)
                                continue;
                            else
                                break;
                        }
                        else if (realWebStep < webStep.ExaminationStepKind)
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
        private static List<ExaminationStep> ConvertToExaminationsSteps(PatientExaminations patientExaminations, DateTime? transfer2StageDate = null)
        {
            var examinationSteps = new List<ExaminationStep>();

            var stage1 = patientExaminations.Stage1;
            var stage2 = patientExaminations.Stage2;

            if (stage1 != default)
            {
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstBegin,
                    Date = stage1.BeginDate
                });
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstEnd,
                    Date = stage1.EndDate
                });
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstResult,
                    Date = stage1.EndDate,
                    HealthGroup = stage1.HealthGroup,
                    Referral = stage1.Referral
                });
            }


            if (stage2 != default)
            {
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.TransferSecond,
                    Date = stage1?.EndDate ?? transfer2StageDate.Value
                });

                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.SecondBegin,
                    Date = stage2.BeginDate
                });
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.SecondEnd,
                    Date = stage2.EndDate
                });
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.SecondResult,
                    Date = stage2.EndDate,
                    HealthGroup = stage2.HealthGroup,
                    Referral = stage2.Referral
                });
            }

            return examinationSteps;
        }
        private static List<ExaminationStep> ConvertToExaminationsSteps(WebPatientData webPatientData)
        {
            var examinationSteps = new List<ExaminationStep>();

            if (webPatientData.Disp1BeginDate != default)
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstBegin,
                    Date = webPatientData.Disp1BeginDate.Value
                });

            if (webPatientData.Disp1Date != default)
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstEnd,
                    Date = webPatientData.Disp1Date.Value
                });

            if (webPatientData.Disp1Date != default && webPatientData.Stage1ResultId != default)
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstResult,
                    Date = webPatientData.Disp1Date.Value,
                    HealthGroup = webPatientData.Stage1ResultId.Value,
                    Referral = webPatientData.Stage1DestId ?? 0
                });

            if (webPatientData.Disp2DirectDate != default)
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.TransferSecond,
                    Date = webPatientData.Disp2DirectDate.Value
                });

            if (webPatientData.Disp2BeginDate != default)
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.SecondBegin,
                    Date = webPatientData.Disp2BeginDate.Value
                });

            if (webPatientData.Disp2Date != default)
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.SecondEnd,
                    Date = webPatientData.Disp2Date.Value
                });

            if (webPatientData.Disp2Date != default && webPatientData.Stage2ResultId != default && webPatientData.Stage2DestId != default)
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.SecondResult,
                    Date = webPatientData.Disp2Date.Value,
                    HealthGroup = webPatientData.Stage2ResultId.Value,
                    Referral = webPatientData.Stage2DestId.Value
                });

            if (webPatientData.DispCancelDate != default)
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.Refuse,
                    Date = webPatientData.DispCancelDate.Value
                });

            return examinationSteps;
        }
        private ExaminationStepKind AddStep(int patientId, ExaminationStep examinationStep)
        {
            AddStep(patientId, examinationStep.ExaminationStepKind, examinationStep.Date, examinationStep.HealthGroup, examinationStep.Referral);

            return examinationStep.ExaminationStepKind;
        }
        private void DeleteAllSteps(int patientId)
        {
            while (DeleteLastStep(patientId) != 0) ;
        }
        #endregion
    }
}