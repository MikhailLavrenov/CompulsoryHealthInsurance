using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHI.Modules.MedicalExaminations.Models;
using CHI.Modules.MedicalExaminations.Services;
using PatientsFomsRepository.Models;

namespace CHI.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TestWebSite();
        }

        static void TestBillsRegisters()
        {
            var fileName = @"C:\Users\ЛавреновМВ\Desktop\ExportFile_2019_10_10_15_59_11(2019_09_01 2019_09_30).zip";

            var register = new BillsRegister(fileName);
            var examinationNames = new string[] { @"DPM", @"DVM", @"DOM" };
            var patientsNames = new string[] { @"LPM", @"LVM", @"LOM" };

            var patients = register.GetPatientsExaminations(examinationNames, patientsNames);
            var t = patients.Values.Where(x => x.Count > 1).ToList();
        }
        static void TestWebSite()
        {
            var url = "http://11.0.0.205/";
            var proxyServer = "10.10.45.40";
            var proxyPort = 3128;
            var web = new TestWebSiteApi(url, proxyServer, proxyPort);
        }


    }

    class TestWebSiteApi: WebSiteApi
    {
        public TestWebSiteApi(string URL)
    : this(URL, null, 0)
        { }
        public TestWebSiteApi(string URL, string proxyAddress, int proxyPort) : base(URL, proxyAddress, proxyPort)
        {
            var r1 = TryAuthorize("UshanovaTA", "UshanovaTA1");
            var r2 = TryGetPatientDataFromPlan("2751530822000157", ExaminationKind.Dispanserizacia1, 2019, out var webPlanPatientData);
            var r3 = TryDeletePatientFromPlan(webPlanPatientData.Id);
            var r4 = TryGetPatientFromSRZ("2751530822000157", 2019, out var srzPatientId);
            var r5 = TryAddPatientToPlan(srzPatientId, ExaminationKind.Dispanserizacia1, 2019);

            var r6 = TryAddStep(ExaminationStepKind.FirstBegin, new DateTime(2019, 10, 25), 0, 0, webPlanPatientData.Id);
            var r7 = TryAddStep(ExaminationStepKind.FirstEnd, new DateTime(2019, 10, 28), 0, 0, webPlanPatientData.Id);
            var r8 = TryAddStep(ExaminationStepKind.FirstResult, new DateTime(2019, 10, 28), HealthGroup.First, Referral.None, webPlanPatientData.Id);

            var r9 = TryDeleteLastStep(webPlanPatientData.Id);
            var r10 = TryDeleteLastStep(webPlanPatientData.Id);
            var r11 = TryDeleteLastStep(webPlanPatientData.Id);
        }
    }
}
