using CHI.Application.Models;
using CHI.Services.BillsRegister;
using CHI.Services.MedicalExaminations;
using System;
using System.Linq;

namespace CHI.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TestWebService();
        }

        static void TestBillsRegisters()
        {
            var fileName = @"C:\Users\ЛавреновМВ\Desktop\ExportFile_2019_10_10_15_59_11(2019_09_01 2019_09_30).zip";

            var register = new BillsRegisterService(fileName);
            var examinationNames = new string[] { @"DPM", @"DVM", @"DOM" };
            var patientsNames = new string[] { @"LPM", @"LVM", @"LOM" };

            var patientsExaminations = register.GetPatientsExaminations(examinationNames, patientsNames);
        }
        static void TestWebSiteApiMethod()
        {
            var url = "http://11.0.0.205/";
            var proxyServer = "10.10.45.40";
            var proxyPort = 3128;
            var web = new TestExaminationServiceApi(url,true, proxyServer, proxyPort);
        }
        static void TestWebService()
        {
            var url = "http://11.0.0.205/";
            var proxyServer = "10.10.45.40";
            var proxyPort = 3128;
            var credential = new Credential();
            credential.Login = "UshanovaTA";
            credential.Password = "UshanovaTA1";

            var web = new ExaminationService(url,true, proxyServer, proxyPort);

            var examination1Stage = new Examination
            {
                BeginDate = new DateTime(2019, 10, 10),
                EndDate = new DateTime(2019, 10, 15),
                HealthGroup = HealthGroup.ThirdA,
                Referral = Referral.LocalClinic
            };
            var examination2Stage = new Examination
            {
                BeginDate = new DateTime(2019, 10, 20),
                EndDate = new DateTime(2019, 10, 25),
                HealthGroup = HealthGroup.ThirdB,
                Referral = Referral.AnotherClinic
            };
            var patientExaminations = new PatientExaminations("2751530822000157", 2019, ExaminationKind.Dispanserizacia1)
            {
                Stage1 = examination1Stage,
                Stage2 = examination2Stage
            };

            web.Authorize(credential);
            web.AddPatientExaminations(patientExaminations);
        }

        class TestExaminationServiceApi : ExaminationServiceApi
        {
            public TestExaminationServiceApi(string URL,bool useProxy, string proxyAddress, int? proxyPort) : base(URL, useProxy, proxyAddress, proxyPort)
            {
                var credential = new Credential();
                credential.Login = "";
                credential.Password = "";
                Authorize(credential);
                var webPlanPatientData = GetPatientDataFromPlan(null,"2751530822000157", ExaminationKind.Dispanserizacia1, 2019);
                DeletePatientFromPlan(webPlanPatientData.Id);
                var srzPatientId = GetPatientIdFromSRZ("2751530822000157",null, 2019);
                AddPatientToPlan(srzPatientId.Value, ExaminationKind.Dispanserizacia1, 2019);

                AddStep(webPlanPatientData.Id, StepKind.FirstBegin, new DateTime(2019, 10, 25), 0, 0);
                AddStep(webPlanPatientData.Id, StepKind.FirstEnd, new DateTime(2019, 10, 28), 0, 0);
                AddStep(webPlanPatientData.Id, StepKind.FirstResult, new DateTime(2019, 10, 28), HealthGroup.First, Referral.None);

                DeleteLastStep(webPlanPatientData.Id);
                DeleteLastStep(webPlanPatientData.Id);
                DeleteLastStep(webPlanPatientData.Id);
            }


        }
    }
}
