using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Services.MedicalExaminations
{
    public class ExaminationService : ExaminationServiceApi
    {
        #region Поля
        private static readonly ExaminationStepKind[] examinationSteps;
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
        public void AddPatientExaminations(Patient patient, IEnumerable<Examination> examinations)
        {
            var webPatientData = GetOrAddPatientToPlan(patient, examinations.First().Kind, examinations.First().Year);

            if (webPatientData == null)
                return;

            var transfer2StageDate = examinations.FirstOrDefault(x => x.Stage == 1)?.EndDate ?? webPatientData.Disp1Date;
            var userSteps = ConvertToExaminationSteps(examinations, transfer2StageDate);
            var webSteps = ConvertToExaminationSteps(webPatientData);

            AddPatientExaminations(webPatientData.Id, userSteps, webSteps);

        }
        private WebPatientData GetOrAddPatientToPlan(Patient patient, ExaminationKind examinationKind, int examinationYear)
        {
            var webPatientData = GetPatientDataFromPlan(patient.InsuranceNumber, examinationKind, examinationYear);

            //пациент найден в нужном плане
            if (webPatientData == null)
            {
                var otherExaminationKinds = Enum.GetValues(typeof(ExaminationKind))
                    .Cast<ExaminationKind>()
                    .Where(x => x != ExaminationKind.None && x != examinationKind)
                    .ToList();

                //ищем в др. планах
                foreach (var examinationType in otherExaminationKinds)
                {
                    webPatientData = GetPatientDataFromPlan(patient.InsuranceNumber, examinationType, examinationYear);
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

                var srzPatientId = webPatientData?.PersonId ?? GetPatientFromSRZ(patient.InsuranceNumber, examinationYear);

                AddPatientToPlan(srzPatientId, examinationKind, examinationYear);

                webPatientData = GetPatientDataFromPlan(patient.InsuranceNumber, examinationKind, examinationYear);
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

                    if (userStep != default)
                    {
                        if (userStep != webStep)
                        {
                            if (webStep == default)
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
                        if (webStep == default)
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
            catch (WebServerOperationException)
            {
                DeleteAllSteps(patientId);

                foreach (var webStep in webSteps)
                    AddStep(patientId, webStep);

                throw;
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
                    HealthGroup = (ExaminationHealthGroup)webPatientData.Stage1ResultId.Value,
                    Referral = (ExaminationReferral)webPatientData.Stage1DestId.Value
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
                    HealthGroup = (ExaminationHealthGroup)webPatientData.Stage1ResultId.Value,
                    Referral = (ExaminationReferral)webPatientData.Stage1DestId.Value
                });

            return examinationSteps;
        }
        protected ExaminationStepKind AddStep(int patientId, ExaminationStep examinationStep)
        {
            AddStep(patientId, examinationStep.ExaminationStepKind, examinationStep.Date, examinationStep.HealthGroup, examinationStep.Referral);

            return examinationStep.ExaminationStepKind;
        }
        protected void DeleteAllSteps(int patientId)
        {
            while (DeleteLastStep(patientId) != 0) ;
        }
        #endregion
    }
}