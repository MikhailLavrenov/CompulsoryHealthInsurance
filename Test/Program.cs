using CHI.Services;
using CHI.Services.MedicalExaminations;
using System;
using System.Collections.Generic;
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

            var patients = register.GetPatientsExaminations(examinationNames, patientsNames);
            var t = patients.Values.Where(x => x.Count > 1).ToList();
        }
        static void TestWebSiteApiMethod()
        {
            var url = "http://11.0.0.205/";
            var proxyServer = "10.10.45.40";
            var proxyPort = 3128;
            var web = new TestExaminationServiceApi(url, proxyServer, proxyPort);
        }
        static void TestWebService()
        {
            var url = "http://11.0.0.205/";
            var proxyServer = "10.10.45.40";
            var proxyPort = 3128;
            var login = "UshanovaTA";
            var password = "UshanovaTA1";

            var web = new ExaminationService(url, proxyServer, proxyPort);

            var patient = new Patient("2751530822000157");
            var examination1Stage = new Examination
            {
                Kind = ExaminationKind.Dispanserizacia1,
                Stage = 1,
                Year = 2019,
                BeginDate = new DateTime(2019, 10, 10),
                EndDate = new DateTime(2019, 10, 15),
                HealthGroup = ExaminationHealthGroup.ThirdA,
                Referral = ExaminationReferral.LocalClinic
            };
            var examination2Stage = new Examination
            {
                Kind = ExaminationKind.Dispanserizacia1,
                Stage = 2,
                Year = 2019,
                BeginDate = new DateTime(2019, 10, 20),
                EndDate = new DateTime(2019, 10, 25),
                HealthGroup = ExaminationHealthGroup.ThirdB,
                Referral = ExaminationReferral.AnotherClinic
            };
            var examinations = new List<Examination> { examination2Stage, examination1Stage };

            web.Authorize(login, password);
            web.AddPatientExaminations(patient, examinations);

        }

        class TestExaminationServiceApi : ExaminationServiceApi
        {
            public TestExaminationServiceApi(string URL)
        : this(URL, null, 0)
            { }
            public TestExaminationServiceApi(string URL, string proxyAddress, int proxyPort) : base(URL, proxyAddress, proxyPort)
            {
                Authorize("UshanovaTA", "UshanovaTA1");
                var webPlanPatientData = GetPatientDataFromPlan("2751530822000157", ExaminationKind.Dispanserizacia1, 2019);
                DeletePatientFromPlan(webPlanPatientData.Id);
                var srzPatientId = GetPatientFromSRZ("2751530822000157", 2019);
                AddPatientToPlan(srzPatientId, ExaminationKind.Dispanserizacia1, 2019);

                AddStep(webPlanPatientData.Id, ExaminationStepKind.FirstBegin, new DateTime(2019, 10, 25), 0, 0);
                AddStep(webPlanPatientData.Id, ExaminationStepKind.FirstEnd, new DateTime(2019, 10, 28), 0, 0);
                AddStep(webPlanPatientData.Id, ExaminationStepKind.FirstResult, new DateTime(2019, 10, 28), ExaminationHealthGroup.First, ExaminationReferral.None);

                DeleteLastStep(webPlanPatientData.Id);
                DeleteLastStep(webPlanPatientData.Id);
                DeleteLastStep(webPlanPatientData.Id);
            }


        }
    }
}
