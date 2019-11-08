using CHI.Modules.MedicalExaminations.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CHI.Modules.MedicalExaminations.Services
{
    public class WebSite : WebSiteApi
    {
        #region Конструкторы
        public WebSite(string URL)
            : this(URL, null, 0)
        { }
        public WebSite(string URL, string proxyAddress, int proxyPort) : base(URL, proxyAddress, proxyPort)
        { }
        #endregion

        #region Методы
        public bool TryAddExaminations(Models.Patient patient, IEnumerable<Examination> examinations)
        {

            if (TryGetOrAddPatientToPlan(patient, examinations.First().Kind, examinations.First().Year, out var webPatientData)
                && (TryAddExaminations(patient, examinations, webPatientData)))
                return true;
            else
                return false;
        }
        protected bool TryGetOrAddPatientToPlan(Models.Patient patient, ExaminationKind examinationKind, int examinationYear, out WebPatientData webPatientData)
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

                        TryDeletePatientFromPlan(webPatientData.Id);

                        break;
                    }
                }

                var srzPatientId = webPatientData?.PersonId ?? 0;

                if (srzPatientId == 0)
                    TryGetPatientFromSRZ(patient.InsuranceNumber, examinationYear, out srzPatientId);

                TryAddPatientToPlan(srzPatientId, examinationKind, examinationYear);

                if (!TryGetPatientDataFromPlan(patient.InsuranceNumber, examinationKind, examinationYear, out webPatientData))
                    return false;
            }

            return true;
        }
        protected bool TryAddExaminations(Models.Patient patient, IEnumerable<Examination> examinations, WebPatientData webPatientData)
        {
            var transfer2StageDate = examinations.FirstOrDefault(x => x.Stage == 1)?.EndDate ?? webPatientData.Disp1Date;
            var userSteps = ConvertToExaminationSteps(examinations, transfer2StageDate);
            var webSteps = ConvertToExaminationSteps(webPatientData);

            var steps = Enum.GetValues(typeof(ExaminationStepKind))
                .Cast<ExaminationStepKind>()
                .Where(x => x != ExaminationStepKind.Refuse && x != ExaminationStepKind.None)
                .OrderBy(x => (int)x)
                .ToList();

            var wasDeleteSteps = false;

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                var userStep = userSteps.Where(x => x.ExaminationStepKind == step).FirstOrDefault();
                var webStep = webSteps.Where(x => x.ExaminationStepKind == step).FirstOrDefault();

                if (userStep != default)
                {
                    if (userStep != webStep)
                    {
                        if (webStep == default)
                            TryAddStep(userStep, webPatientData.Id);
                        else
                        {
                            var webStepIndex = webSteps.IndexOf(webStep);
                            TryDeleteLastSteps(webPatientData.Id, webSteps.Count - webStepIndex);
                            wasDeleteSteps = true;
                            TryAddStep(userStep, webPatientData.Id);
                        }
                    }
                    else if (wasDeleteSteps)
                        TryAddStep(userStep, webPatientData.Id);
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

                    if (wasDeleteSteps)
                        TryAddStep(webStep, webPatientData.Id);
                }
            }

            return true;
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

        private struct WebStepsCounter
        {
            public int InitialCount { get; }
            public int CurrentCount { get; private set; }


            public WebStepsCounter(int initialCount)
            {
                InitialCount = initialCount;
                CurrentCount = InitialCount;
            }
            public int SubstractAllButSaveAmount(int saveAmount)
            {
                var countForSubstraction = CurrentCount > saveAmount ? CurrentCount - saveAmount : 0;

                CurrentCount -= countForSubstraction;

                return countForSubstraction;
            }
        }

    }
}


//if (stage1.BeginDate != webPatientData.Disp1BeginDate
//    || stage1.EndDate != webPatientData.Disp1Date
//    || (int) stage1.HealthGroup != webPatientData.Stage1ResultId
//    || (int) stage1.Referral != webPatientData.Stage1DestId)

//protected bool TryAddExamination2(Models.Patient patient, IEnumerable<Examination> examinations, WebPatientData webPatientData)
//{
//    var stage1 = examinations.FirstOrDefault(x => x.Stage == 1);


//    var webStepsCounter = new WebStepsCounter(webPatientData.GetStepsCount());

//    if (stage1 != default)
//    {
//        if (stage1.BeginDate != webPatientData.Disp1BeginDate)
//        {
//            if (webPatientData.Disp1BeginDate != default)
//                TryDeleteLastSteps(webPatientData.Id, webStepsCounter.SubstractAllButSaveAmount(0));

//            TryAddStep(ExaminationStepKind.FirstBegin, stage1.BeginDate, 0, 0, webPatientData.Id);
//        }

//        if (stage1.EndDate != webPatientData.Disp1Date)
//        {
//            if (webPatientData.Disp1Date != default)
//                TryDeleteLastSteps(webPatientData.Id, webStepsCounter.SubstractAllButSaveAmount(1));

//            TryAddStep(ExaminationStepKind.FirstEnd, stage1.EndDate, 0, 0, webPatientData.Id);
//        }

//        if (stage1.EndDate != webPatientData.Disp1Date || (int)stage1.HealthGroup != webPatientData.Stage1ResultId || (int)stage1.Referral != webPatientData.Stage1DestId)
//        {
//            if (webPatientData.Disp1Date != default || webPatientData.Stage1ResultId != default || webPatientData.Stage1DestId != default)
//                TryDeleteLastSteps(webPatientData.Id, webStepsCounter.SubstractAllButSaveAmount(2));

//            TryAddStep(ExaminationStepKind.FirstResult, stage1.EndDate, stage1.HealthGroup, stage1.Referral, webPatientData.Id);
//        }
//    }

//    int webStage1ResultExist;

//    if (webPatientData.Stage1ResultId != default || stage1 != default)
//        webStage1ResultExist = 1;
//    else
//        webStage1ResultExist = 0;

//    var stage2 = examinations.FirstOrDefault(x => x.Stage == 2);

//    if (stage2 != default)
//    {
//        if (stage1 != default || webPatientData.Disp1Date != default)
//        {
//            var transferDate = stage1?.EndDate ?? webPatientData.Disp1Date.Value;

//            if (webPatientData.Disp2BeginDate != default)
//                TryDeleteLastSteps(webPatientData.Id, webStepsCounter.SubstractAllButSaveAmount(2 + webStage1ResultExist));

//            TryAddStep(ExaminationStepKind.TransferSecond, transferDate, 0, 0, webPatientData.Id);



//        }
//        else
//            return false;







//        if (stage2.BeginDate != webPatientData.Disp2BeginDate)
//        {
//            if (webPatientData.Disp2BeginDate != default)
//                TryDeleteLastSteps(webPatientData.Id, webStepsCounter.SubstractAllButSaveAmount(3 + webStage1ResultExist));

//            TryAddStep(ExaminationStepKind.FirstBegin, stage2.BeginDate, 0, 0, webPatientData.Id);
//        }

//        if (stage2.EndDate != webPatientData.Disp2Date)
//        {
//            if (webPatientData.Disp2Date != default)
//                TryDeleteLastSteps(webPatientData.Id, webStepsCounter.SubstractAllButSaveAmount(4 + webStage1ResultExist));

//            TryAddStep(ExaminationStepKind.FirstEnd, stage2.EndDate, 0, 0, webPatientData.Id);
//        }

//        if (stage2.EndDate != webPatientData.Disp2Date || (int)stage2.HealthGroup != webPatientData.Stage2ResultId || (int)stage2.Referral != webPatientData.Stage2DestId)
//        {
//            if (webPatientData.Disp2Date != default || webPatientData.Stage2ResultId != default || webPatientData.Stage2DestId != default)
//                TryDeleteLastSteps(webPatientData.Id, webStepsCounter.SubstractAllButSaveAmount(5 + webStage1ResultExist));

//            TryAddStep(ExaminationStepKind.FirstResult, stage2.EndDate, stage2.HealthGroup, stage2.Referral, webPatientData.Id);
//        }
//    }

//    return true;
//}
