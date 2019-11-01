using CHI.Modules.MedicalExaminations.Models;
using System;
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
                var otherExaminationTypes = Enum.GetValues(typeof(ExaminationType)).Cast<ExaminationType>().Where(x => x != ExaminationType.None && x != examination.Type).ToList();

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
                        

                    TryAddStep(ExaminationStep.FirstBegin, examination.BeginDate, 0, 0, webPatientData.Id);


                }
                else if (examination.Stage == 2)
                {

                }


            return true;
        }
        #endregion
    }
}
