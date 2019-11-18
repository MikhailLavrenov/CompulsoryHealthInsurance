using CHI.Modules.MedicalExaminations.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Modules.MedicalExaminations.Services
{
    public class WebSite : WebSiteApi
    {
        #region Поля
        private static readonly ExaminationStepKind[] examinationSteps;
        #endregion

        #region Конструкторы
        static WebSite()
        {
            examinationSteps = Enum.GetValues(typeof(ExaminationStepKind))
                .Cast<ExaminationStepKind>()
                .Where(x => x != ExaminationStepKind.Refuse && x != ExaminationStepKind.None)
                .OrderBy(x => (int)x)
                .ToArray();
        }
        public WebSite(string URL)
            : this(URL, null, 0)
        { }
        public WebSite(string URL, string proxyAddress, int proxyPort) : base(URL, proxyAddress, proxyPort)
        { }
        #endregion

        #region Методы
        public void AddPatientExaminations(Models.Patient patient, IEnumerable<Examination> examinations)
        {
            var webPatientData = GetOrAddPatientToPlan(patient, examinations.First().Kind, examinations.First().Year);

            var transfer2StageDate = examinations.FirstOrDefault(x => x.Stage == 1)?.EndDate ?? webPatientData.Disp1Date;
            var userSteps = ConvertToExaminationSteps(examinations, transfer2StageDate);
            var webSteps = ConvertToExaminationSteps(webPatientData);

            AddPatientExaminations(webPatientData.Id, userSteps, webSteps);

        }
        private WebPatientData GetOrAddPatientToPlan(Models.Patient patient, ExaminationKind examinationKind, int examinationYear)
        {
            //пациент не найден в нужном плане
            if (!TryGetPatientDataFromPlan(patient.InsuranceNumber, examinationKind, examinationYear, out webPatientData))
            {
                var otherExaminationTypes = Enum.GetValues(typeof(ExaminationKind)).Cast<ExaminationKind>().Where(x => x != ExaminationKind.None && x != examinationKind).ToList();

                //ищем в др. планах
                foreach (var examinationType in otherExaminationTypes)
                {
                    //пациент добавлен в другой план
                    if (TryGetPatientDataFromPlan(patient.InsuranceNumber, examinationType, examinationYear, out webPatientData))
                    {
                        //заполнены шаги осмотра
                        if (webPatientData.Disp1BeginDate != default || webPatientData.Disp2BeginDate != default || webPatientData.DispCancelDate != default)
                            while (TryDeleteLastStep(webPatientData.Id)) ;

                        DeletePatientFromPlan(webPatientData.Id);

                        break;
                    }
                }

                var srzPatientId = webPatientData?.PersonId ?? 0;

                if (srzPatientId == 0)
                    GetPatientFromSRZ(patient.InsuranceNumber, examinationYear, out srzPatientId);

                AddPatientToPlan(srzPatientId, examinationKind, examinationYear);

                if (!TryGetPatientDataFromPlan(patient.InsuranceNumber, examinationKind, examinationYear, out webPatientData))
                    return false;
            }

        }
        private void AddPatientExaminations(int patientId, List<ExaminationStep> userSteps, List<ExaminationStep> webSteps)
        {
            try
            {
                int deletedStepsTotal = 0;

                for (int i = 0; i < examinationSteps.Length; i++)
                {
                    var step = examinationSteps[i];
                    var userStep = userSteps.Where(x => x.ExaminationStepKind == step).FirstOrDefault();
                    var webStep = webSteps.Where(x => x.ExaminationStepKind == step).FirstOrDefault();

                    if (userStep != default)
                    {
                        if (userStep != webStep)
                        {
                            if (webStep == default)
                                AddStep(patientId, userStep);
                            else
                            {
                                var webStepIndex = webSteps.IndexOf(webStep);
                                var deleteSteps = webSteps.Count - webStepIndex - deletedStepsTotal;
                                deleteSteps = deleteSteps > 0 ? deleteSteps : 0;
                                DeleteLastSteps(patientId, deleteSteps);
                                deletedStepsTotal += deleteSteps;
                                AddStep(patientId, userStep);
                            }
                        }
                        else if (deletedStepsTotal != 0)
                            AddStep(patientId, userStep);
                    }
                    else
                    {
                        if (webStep == default)
                        {
                            if (step == ExaminationStepKind.FirstResult)
                                continue;
                            else
                                break;
                        }
                        if (deletedStepsTotal != 0)
                            AddStep(patientId, webStep);
                    }
                }

            }
            catch (WebServerOperationException)
            {
                while (TryDeleteLastStep(patientId));

                foreach (var webStep in webSteps)
                    AddStep(patientId, webStep);
            }
        }

        private static List<ExaminationStep> ConvertToExaminationSteps(IEnumerable<Examination> patientExaminations, DateTime? transfer2StageDate = null)
        {
            var examinationSteps = new List<ExaminationStep>();

            var stage1 = patientExaminations.FirstOrDefault(x => x.Stage == 1);
            var stage2 = patientExaminations.FirstOrDefault(x => x.Stage == 2);

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
        private static List<ExaminationStep> ConvertToExaminationSteps(WebPatientData webPatientData)
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

            if (webPatientData.Disp1Date != default && webPatientData.Stage1ResultId != default && webPatientData.Stage1DestId != default)
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstResult,
                    Date = webPatientData.Disp1Date.Value,
                    HealthGroup = (HealthGroup)webPatientData.Stage1ResultId.Value,
                    Referral = (Referral)webPatientData.Stage1DestId.Value
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
                    Date = webPatientData.Disp1Date.Value,
                    HealthGroup = (HealthGroup)webPatientData.Stage1ResultId.Value,
                    Referral = (Referral)webPatientData.Stage1DestId.Value
                });

            return examinationSteps;
        }
        #endregion
    }
}