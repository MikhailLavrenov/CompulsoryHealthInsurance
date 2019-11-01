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
        protected bool TryGetOrAddPatientToPlan(Models.Patient patient, Examination examination, out WebPatientData webPatientData)
        {
            //пациент не найден в нужном плане
            if (!TryGetPatientDataFromPlan(patient.InsuranceNumber, examination.Type, examination.Year, out webPatientData))
            {
                var otherExaminationTypes = Enum.GetValues(typeof(ExaminationKind)).Cast<ExaminationKind>().Where(x => x != ExaminationKind.None && x != examination.Type).ToList();

                //ищем в др. планах
                foreach (var examinationType in otherExaminationTypes)
                {
                    //пациент добавлен в другой план
                    if (TryGetPatientDataFromPlan(patient.InsuranceNumber, examinationType, examination.Year, out webPatientData))
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
                    TryGetPatientFromSRZ(patient.InsuranceNumber, examination.Year, out srzPatientId);

                TryAddPatientToPlan(srzPatientId, examination.Type, examination.Year);

                if (!TryGetPatientDataFromPlan(patient.InsuranceNumber, examination.Type, examination.Year, out webPatientData))
                    return false;
            }

            return true;
        }
        protected bool TryAddExamination(Models.Patient patient, Examination examination, WebPatientData webPatientData)
        {
            if (webPatientData.DispCancelDate != null)

                if (examination.Stage == 1)
                {
                    //if (webPatientData.Disp1BeginDate!=default && examination.BeginDate != webPatientData.Disp1BeginDate)


                    TryAddStep(ExaminationStepKind.FirstBegin, examination.BeginDate, 0, 0, webPatientData.Id);


                }
                else if (examination.Stage == 2)
                {

                }


            return true;
        }
        private static List<ExaminationStep> ConvertToExaminationSteps(IEnumerable<Examination> patientExaminations)
        {
            var examinationSteps = new List<ExaminationStep>();

            var examination = patientExaminations.FirstOrDefault(x => x.Stage == 1);

            if (examination != default)
            {
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstBegin,
                    Date = examination.BeginDate
                });
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstEnd,
                    Date = examination.EndDate
                });
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstResult,
                    Date = examination.EndDate,
                    HealthGroup = examination.HealthGroup,
                    Referral = examination.Referral
                });
            }

            examination = patientExaminations.FirstOrDefault(x => x.Stage == 2);

            if (examination != default)
            {
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.SecondBegin,
                    Date = examination.BeginDate
                });
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.SecondEnd,
                    Date = examination.EndDate
                });
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.SecondResult,
                    Date = examination.EndDate,
                    HealthGroup = examination.HealthGroup,
                    Referral = examination.Referral
                });
            }

            return examinationSteps;
        }
        private static List<ExaminationStep> ConvertToExaminationSteps(WebPatientData webPatientData)
        {
            var examinationSteps = new List<ExaminationStep>();

            if (webPatientData.Disp1BeginDate != default && webPatientData.Disp1Date != default && webPatientData.Stage1ResultId != default && webPatientData.Stage1DestId != default)
            {
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstBegin,
                    Date = webPatientData.Disp1BeginDate.Value
                });
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstEnd,
                    Date = webPatientData.Disp1Date.Value
                });
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.FirstResult,
                    Date = webPatientData.Disp1Date.Value,
                    HealthGroup = (HealthGroup)webPatientData.Stage1ResultId.Value,
                    Referral = (Referral)webPatientData.Stage1DestId.Value
                });
            }

            if (webPatientData.Disp2BeginDate != default && webPatientData.Disp2Date != default && webPatientData.Stage2ResultId != default && webPatientData.Stage2DestId != default)
            {
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.SecondBegin,
                    Date = webPatientData.Disp2BeginDate.Value
                });
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.SecondEnd,
                    Date = webPatientData.Disp2Date.Value
                });
                examinationSteps.Add(new ExaminationStep
                {
                    ExaminationStepKind = ExaminationStepKind.SecondResult,
                    Date = webPatientData.Disp1Date.Value,
                    HealthGroup = (HealthGroup)webPatientData.Stage1ResultId.Value,
                    Referral = (Referral)webPatientData.Stage1DestId.Value
                });
            }

            return examinationSteps;
        }

        #endregion
    }
}
