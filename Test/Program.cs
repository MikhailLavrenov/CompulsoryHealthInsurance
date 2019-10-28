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
            TestWebServer();
        }
        static void TestWebServer()
        {
            var url = "http://11.0.0.205/";
            var proxyServer = "10.10.45.40";
            var proxyPort = 3128;
            var web = new WebServer(url, proxyServer, proxyPort);
            var cred = new Credential() { Login= "UshanovaTA", Password= "UshanovaTA1" };
            var r1=web.TryAuthorize(cred);           
            var r2=web.TryFindPatientInPlan("2751530822000157", ExaminationType.Dispanserizacia1, 2019,out var webPlanPatientData);
            var r3 = web.TryDeletePatientFromPlan(webPlanPatientData.Id);
            var r4 = web.TryFindPatientInSRZ("2751530822000157", 2019, out var PatientId);
            var r5 = web.TryAddPatientToPlan(PatientId, ExaminationType.Dispanserizacia1, 2019);

            var r6 = web.TryAddStep(ExaminationStage.FirstBegin, new DateTime(2019, 10, 25), 0, 0, webPlanPatientData.Id);
            var r7 = web.TryAddStep(ExaminationStage.FirstEnd, new DateTime(2019, 10, 28), 0, 0, webPlanPatientData.Id);
            var r8 = web.TryAddStep(ExaminationStage.FirstResult, new DateTime(2019, 10, 28),  HealthGroup.First, Referral.None, webPlanPatientData.Id);

            var r9 = web.TryDeleteLastStep(webPlanPatientData.Id);
            var r10 = web.TryDeleteLastStep(webPlanPatientData.Id);
            var r11 = web.TryDeleteLastStep(webPlanPatientData.Id);
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
    }
}
